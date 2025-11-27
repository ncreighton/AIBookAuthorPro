// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Interfaces;
using AIBookAuthorPro.Core.Models.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AIBookAuthorPro.Infrastructure.AI;

/// <summary>
/// OpenAI GPT provider implementation.
/// </summary>
public sealed class OpenAIProvider : BaseAIProvider
{
    private const string ApiUrl = "https://api.openai.com/v1/chat/completions";
    
    private readonly OpenAISettings _settings;
    
    private static readonly List<AIModelInfo> _models = new()
    {
        new AIModelInfo
        {
            Id = "gpt-4o",
            Name = "GPT-4o",
            Provider = AIProviderType.OpenAI,
            MaxContextTokens = 128000,
            MaxOutputTokens = 16384,
            CostPer1KInputTokens = 0.005m,
            CostPer1KOutputTokens = 0.015m,
            SupportsVision = true,
            SupportsStreaming = true,
            Description = "Most capable GPT-4 model with vision capabilities"
        },
        new AIModelInfo
        {
            Id = "gpt-4o-mini",
            Name = "GPT-4o Mini",
            Provider = AIProviderType.OpenAI,
            MaxContextTokens = 128000,
            MaxOutputTokens = 16384,
            CostPer1KInputTokens = 0.00015m,
            CostPer1KOutputTokens = 0.0006m,
            SupportsVision = true,
            SupportsStreaming = true,
            Description = "Cost-effective smaller model with good performance"
        },
        new AIModelInfo
        {
            Id = "o1-preview",
            Name = "o1 Preview",
            Provider = AIProviderType.OpenAI,
            MaxContextTokens = 128000,
            MaxOutputTokens = 32768,
            CostPer1KInputTokens = 0.015m,
            CostPer1KOutputTokens = 0.06m,
            SupportsVision = false,
            SupportsStreaming = false,
            Description = "Advanced reasoning model for complex tasks"
        },
        new AIModelInfo
        {
            Id = "o1-mini",
            Name = "o1 Mini",
            Provider = AIProviderType.OpenAI,
            MaxContextTokens = 128000,
            MaxOutputTokens = 65536,
            CostPer1KInputTokens = 0.003m,
            CostPer1KOutputTokens = 0.012m,
            SupportsVision = false,
            SupportsStreaming = false,
            Description = "Faster reasoning model for simpler tasks"
        }
    };

    public override string Name => "OpenAI";
    public override AIProviderType ProviderType => AIProviderType.OpenAI;
    public override IReadOnlyList<AIModelInfo> AvailableModels => _models;

    public OpenAIProvider(
        HttpClient httpClient,
        IOptions<OpenAISettings> settings,
        ILogger<OpenAIProvider> logger)
        : base(httpClient, logger)
    {
        _settings = settings.Value;
        ConfigureHttpClient();
    }

    private void ConfigureHttpClient()
    {
        HttpClient.DefaultRequestHeaders.Clear();
        HttpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
        HttpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <inheritdoc />
    public override async Task<Result<AIGenerationResponse>> GenerateAsync(
        AIGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = ValidateRequest(request);
        if (!validation.IsSuccess)
        {
            return Result<AIGenerationResponse>.Failure(validation.Error!);
        }

        LogRequest(request);

        try
        {
            var openaiRequest = BuildRequest(request);
            
            var response = await HttpClient.PostAsJsonAsync(
                ApiUrl,
                openaiRequest,
                JsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                Logger.LogError("OpenAI API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return Result<AIGenerationResponse>.Failure($"API error: {response.StatusCode} - {errorContent}");
            }

            var result = await response.Content.ReadFromJsonAsync<OpenAIResponse>(JsonOptions, cancellationToken);
            if (result == null)
            {
                return Result<AIGenerationResponse>.Failure("Failed to deserialize API response");
            }

            var generationResponse = MapResponse(result);
            LogResponse(generationResponse);
            
            return Result<AIGenerationResponse>.Success(generationResponse);
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError(ex, "HTTP request failed for OpenAI API");
            return Result<AIGenerationResponse>.Failure($"Network error: {ex.Message}");
        }
        catch (TaskCanceledException ex) when (ex.CancellationToken == cancellationToken)
        {
            Logger.LogInformation("Request cancelled by user");
            return Result<AIGenerationResponse>.Failure("Request cancelled");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error in OpenAI generation");
            return Result<AIGenerationResponse>.Failure($"Unexpected error: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public override async IAsyncEnumerable<Result<AIStreamChunk>> GenerateStreamAsync(
        AIGenerationRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var validation = ValidateRequest(request);
        if (!validation.IsSuccess)
        {
            yield return Result<AIStreamChunk>.Failure(validation.Error!);
            yield break;
        }

        // o1 models don't support streaming
        if (request.Model.StartsWith("o1"))
        {
            var result = await GenerateAsync(request, cancellationToken);
            if (result.IsSuccess && result.Value != null)
            {
                yield return Result<AIStreamChunk>.Success(new AIStreamChunk
                {
                    Content = result.Value.Content,
                    IsComplete = true,
                    FinishReason = result.Value.FinishReason,
                    Usage = result.Value.Usage
                });
            }
            else
            {
                yield return Result<AIStreamChunk>.Failure(result.Error ?? "Unknown error");
            }
            yield break;
        }

        LogRequest(request);

        var openaiRequest = BuildRequest(request, stream: true);
        var json = JsonSerializer.Serialize(openaiRequest, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        try
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, ApiUrl) { Content = content };
            response = await HttpClient.SendAsync(
                httpRequest,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                yield return Result<AIStreamChunk>.Failure($"API error: {response.StatusCode} - {errorContent}");
                yield break;
            }
        }
        catch (Exception ex)
        {
            yield return Result<AIStreamChunk>.Failure($"Request failed: {ex.Message}");
            yield break;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrEmpty(line)) continue;
            
            if (!line.StartsWith("data: ")) continue;
            
            var data = line[6..];
            if (data == "[DONE]")
            {
                yield return Result<AIStreamChunk>.Success(new AIStreamChunk
                {
                    IsComplete = true,
                    FinishReason = "stop"
                });
                break;
            }

            OpenAIStreamResponse? chunk;
            try
            {
                chunk = JsonSerializer.Deserialize<OpenAIStreamResponse>(data, JsonOptions);
            }
            catch
            {
                continue;
            }

            if (chunk?.Choices?.FirstOrDefault()?.Delta?.Content is { } chunkContent)
            {
                yield return Result<AIStreamChunk>.Success(new AIStreamChunk
                {
                    Content = chunkContent,
                    IsComplete = false
                });
            }

            var finishReason = chunk?.Choices?.FirstOrDefault()?.FinishReason;
            if (!string.IsNullOrEmpty(finishReason))
            {
                yield return Result<AIStreamChunk>.Success(new AIStreamChunk
                {
                    IsComplete = true,
                    FinishReason = finishReason
                });
            }
        }
    }

    /// <inheritdoc />
    public override async Task<Result<bool>> ValidateApiKeyAsync(
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var testClient = new HttpClient();
            testClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            
            var response = await testClient.GetAsync(
                "https://api.openai.com/v1/models",
                cancellationToken);
            
            return Result<bool>.Success(response.IsSuccessStatusCode);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"API key validation failed: {ex.Message}");
        }
    }

    private object BuildRequest(AIGenerationRequest request, bool stream = false)
    {
        var messages = new List<object>();
        
        // Add system message
        if (request.Context != null)
        {
            messages.Add(new { role = "system", content = BuildSystemPrompt(request.Context) });
        }
        else
        {
            messages.Add(new 
            { 
                role = "system", 
                content = "You are an expert author and creative writing assistant. Your writing is engaging, vivid, and maintains consistent quality." 
            });
        }

        // Add user message
        messages.Add(new { role = "user", content = request.Prompt });

        // o1 models have different parameters
        if (request.Model.StartsWith("o1"))
        {
            return new
            {
                model = request.Model,
                messages,
                max_completion_tokens = request.MaxTokens
            };
        }

        return new
        {
            model = request.Model,
            messages,
            max_tokens = request.MaxTokens,
            temperature = request.Temperature,
            stream
        };
    }

    private static AIGenerationResponse MapResponse(OpenAIResponse response)
    {
        var choice = response.Choices?.FirstOrDefault();
        var content = choice?.Message?.Content ?? string.Empty;
        
        return new AIGenerationResponse
        {
            Content = content,
            Model = response.Model ?? "unknown",
            FinishReason = choice?.FinishReason ?? "unknown",
            Usage = new TokenUsage
            {
                PromptTokens = response.Usage?.PromptTokens ?? 0,
                CompletionTokens = response.Usage?.CompletionTokens ?? 0,
                TotalTokens = response.Usage?.TotalTokens ?? 0
            },
            GeneratedAt = DateTime.UtcNow
        };
    }
}

#region OpenAI API Models

internal sealed class OpenAIResponse
{
    public string? Id { get; set; }
    public string? Object { get; set; }
    public long Created { get; set; }
    public string? Model { get; set; }
    public List<OpenAIChoice>? Choices { get; set; }
    public OpenAIUsage? Usage { get; set; }
}

internal sealed class OpenAIChoice
{
    public int Index { get; set; }
    public OpenAIMessage? Message { get; set; }
    public string? FinishReason { get; set; }
}

internal sealed class OpenAIMessage
{
    public string? Role { get; set; }
    public string? Content { get; set; }
}

internal sealed class OpenAIUsage
{
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public int TotalTokens { get; set; }
}

internal sealed class OpenAIStreamResponse
{
    public string? Id { get; set; }
    public string? Object { get; set; }
    public long Created { get; set; }
    public string? Model { get; set; }
    public List<OpenAIStreamChoice>? Choices { get; set; }
}

internal sealed class OpenAIStreamChoice
{
    public int Index { get; set; }
    public OpenAIStreamDelta? Delta { get; set; }
    public string? FinishReason { get; set; }
}

internal sealed class OpenAIStreamDelta
{
    public string? Role { get; set; }
    public string? Content { get; set; }
}

#endregion

public sealed class OpenAISettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string DefaultModel { get; set; } = "gpt-4o";
    public string? OrganizationId { get; set; }
}