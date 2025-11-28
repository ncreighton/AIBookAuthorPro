// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Enums;
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
            ModelId = "gpt-4o",
            DisplayName = "GPT-4o",
            Provider = Enums.AIProviderType.OpenAI,
            MaxContextTokens = 128000,
            MaxOutputTokens = 16384,
            InputCostPer1K = 0.005m,
            OutputCostPer1K = 0.015m,
            SupportsStreaming = true,
            RecommendedFor = "Most capable GPT-4 model with vision capabilities"
        },
        new AIModelInfo
        {
            ModelId = "gpt-4o-mini",
            DisplayName = "GPT-4o Mini",
            Provider = Enums.AIProviderType.OpenAI,
            MaxContextTokens = 128000,
            MaxOutputTokens = 16384,
            InputCostPer1K = 0.00015m,
            OutputCostPer1K = 0.0006m,
            SupportsStreaming = true,
            RecommendedFor = "Cost-effective smaller model with good performance"
        },
        new AIModelInfo
        {
            ModelId = "o1-preview",
            DisplayName = "o1 Preview",
            Provider = Enums.AIProviderType.OpenAI,
            MaxContextTokens = 128000,
            MaxOutputTokens = 32768,
            InputCostPer1K = 0.015m,
            OutputCostPer1K = 0.06m,
            SupportsStreaming = false,
            RecommendedFor = "Advanced reasoning model for complex tasks"
        },
        new AIModelInfo
        {
            ModelId = "o1-mini",
            DisplayName = "o1 Mini",
            Provider = Enums.AIProviderType.OpenAI,
            MaxContextTokens = 128000,
            MaxOutputTokens = 65536,
            InputCostPer1K = 0.003m,
            OutputCostPer1K = 0.012m,
            SupportsStreaming = false,
            RecommendedFor = "Faster reasoning model for simpler tasks"
        }
    };

    /// <inheritdoc />
    public override string ProviderName => "OpenAI";
    
    /// <inheritdoc />
    public override bool SupportsStreaming => true;
    
    /// <inheritdoc />
    public override int MaxContextTokens => 128000;
    
    /// <inheritdoc />
    public override bool IsConfigured => !string.IsNullOrEmpty(_settings.ApiKey);
    
    /// <inheritdoc />
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
    public override async Task<Result<GenerationResult>> GenerateAsync(
        GenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = ValidateRequest(request);
        if (!validation.IsSuccess)
        {
            return Result<GenerationResult>.Failure(validation.Error!);
        }

        LogRequest(request);
        
        // Convert to compatibility type for internal processing
        var compatRequest = AIGenerationRequest.FromGenerationRequest(request);

        try
        {
            var openaiRequest = BuildRequest(compatRequest);
            
            var response = await HttpClient.PostAsJsonAsync(
                ApiUrl,
                openaiRequest,
                JsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                Logger.LogError("OpenAI API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return Result<GenerationResult>.Failure($"API error: {response.StatusCode} - {errorContent}");
            }

            var result = await response.Content.ReadFromJsonAsync<OpenAIResponse>(JsonOptions, cancellationToken);
            if (result == null)
            {
                return Result<GenerationResult>.Failure("Failed to deserialize API response");
            }

            var compatResponse = MapResponse(result);
            var generationResult = compatResponse.ToGenerationResult();
            LogResponse(generationResult);
            
            return Result<GenerationResult>.Success(generationResult);
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError(ex, "HTTP request failed for OpenAI API");
            return Result<GenerationResult>.Failure($"Network error: {ex.Message}");
        }
        catch (TaskCanceledException ex) when (ex.CancellationToken == cancellationToken)
        {
            Logger.LogInformation("Request cancelled by user");
            return Result<GenerationResult>.Failure("Request cancelled");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error in OpenAI generation");
            return Result<GenerationResult>.Failure($"Unexpected error: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public override async IAsyncEnumerable<string> StreamGenerateAsync(
        GenerationRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var validation = ValidateRequest(request);
        if (!validation.IsSuccess)
        {
            yield break;
        }

        LogRequest(request);
        
        // Convert to compatibility type for internal processing
        var compatRequest = AIGenerationRequest.FromGenerationRequest(request);
        
        // o1 models don't support streaming
        if (compatRequest.Model.StartsWith("o1"))
        {
            var result = await GenerateAsync(request, cancellationToken);
            if (result.IsSuccess && result.Value != null)
            {
                // Yield the entire content at once for non-streaming models
                yield return result.Value.Content;
            }
            yield break;
        }

        var openaiRequest = BuildRequest(compatRequest, stream: true);
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
                Logger.LogError("OpenAI streaming API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                yield break;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "OpenAI streaming request failed");
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
                yield return chunkContent;
            }
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
                InputTokens = response.Usage?.PromptTokens ?? 0,
                OutputTokens = response.Usage?.CompletionTokens ?? 0
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