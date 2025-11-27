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
/// Google Gemini AI provider implementation.
/// </summary>
public sealed class GeminiProvider : BaseAIProvider
{
    private const string ApiBaseUrl = "https://generativelanguage.googleapis.com/v1beta";
    
    private readonly GeminiSettings _settings;
    
    private static readonly List<AIModelInfo> _models = new()
    {
        new AIModelInfo
        {
            Id = "gemini-2.0-flash-exp",
            Name = "Gemini 2.0 Flash",
            Provider = AIProviderType.Google,
            MaxContextTokens = 1000000,
            MaxOutputTokens = 8192,
            CostPer1KInputTokens = 0.00015m, // Free tier available
            CostPer1KOutputTokens = 0.0006m,
            SupportsVision = true,
            SupportsStreaming = true,
            Description = "Latest experimental model with 1M context window"
        },
        new AIModelInfo
        {
            Id = "gemini-1.5-pro",
            Name = "Gemini 1.5 Pro",
            Provider = AIProviderType.Google,
            MaxContextTokens = 2000000,
            MaxOutputTokens = 8192,
            CostPer1KInputTokens = 0.00125m,
            CostPer1KOutputTokens = 0.005m,
            SupportsVision = true,
            SupportsStreaming = true,
            Description = "Most capable Gemini model with 2M context window"
        },
        new AIModelInfo
        {
            Id = "gemini-1.5-flash",
            Name = "Gemini 1.5 Flash",
            Provider = AIProviderType.Google,
            MaxContextTokens = 1000000,
            MaxOutputTokens = 8192,
            CostPer1KInputTokens = 0.000075m,
            CostPer1KOutputTokens = 0.0003m,
            SupportsVision = true,
            SupportsStreaming = true,
            Description = "Fast and efficient model for high-volume tasks"
        },
        new AIModelInfo
        {
            Id = "gemini-1.5-flash-8b",
            Name = "Gemini 1.5 Flash 8B",
            Provider = AIProviderType.Google,
            MaxContextTokens = 1000000,
            MaxOutputTokens = 8192,
            CostPer1KInputTokens = 0.0000375m,
            CostPer1KOutputTokens = 0.00015m,
            SupportsVision = true,
            SupportsStreaming = true,
            Description = "Smallest and fastest Flash model for simple tasks"
        }
    };

    public override string Name => "Google Gemini";
    public override AIProviderType ProviderType => AIProviderType.Google;
    public override IReadOnlyList<AIModelInfo> AvailableModels => _models;

    public GeminiProvider(
        HttpClient httpClient,
        IOptions<GeminiSettings> settings,
        ILogger<GeminiProvider> logger)
        : base(httpClient, logger)
    {
        _settings = settings.Value;
        ConfigureHttpClient();
    }

    private void ConfigureHttpClient()
    {
        HttpClient.DefaultRequestHeaders.Clear();
        HttpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    private string GetApiUrl(string model, bool stream = false)
    {
        var action = stream ? "streamGenerateContent" : "generateContent";
        return $"{ApiBaseUrl}/models/{model}:{action}?key={_settings.ApiKey}";
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
            var geminiRequest = BuildRequest(request);
            var url = GetApiUrl(request.Model);
            
            var response = await HttpClient.PostAsJsonAsync(
                url,
                geminiRequest,
                JsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                Logger.LogError("Gemini API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return Result<AIGenerationResponse>.Failure($"API error: {response.StatusCode} - {errorContent}");
            }

            var result = await response.Content.ReadFromJsonAsync<GeminiResponse>(JsonOptions, cancellationToken);
            if (result == null)
            {
                return Result<AIGenerationResponse>.Failure("Failed to deserialize API response");
            }

            var generationResponse = MapResponse(result, request.Model);
            LogResponse(generationResponse);
            
            return Result<AIGenerationResponse>.Success(generationResponse);
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError(ex, "HTTP request failed for Gemini API");
            return Result<AIGenerationResponse>.Failure($"Network error: {ex.Message}");
        }
        catch (TaskCanceledException ex) when (ex.CancellationToken == cancellationToken)
        {
            Logger.LogInformation("Request cancelled by user");
            return Result<AIGenerationResponse>.Failure("Request cancelled");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error in Gemini generation");
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

        LogRequest(request);

        var geminiRequest = BuildRequest(request);
        var json = JsonSerializer.Serialize(geminiRequest, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var url = GetApiUrl(request.Model, stream: true) + "&alt=sse";

        HttpResponseMessage response;
        try
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
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

        var totalPromptTokens = 0;
        var totalCompletionTokens = 0;

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrEmpty(line)) continue;
            
            if (!line.StartsWith("data: ")) continue;
            
            var data = line[6..];

            GeminiResponse? chunk;
            try
            {
                chunk = JsonSerializer.Deserialize<GeminiResponse>(data, JsonOptions);
            }
            catch
            {
                continue;
            }

            if (chunk == null) continue;

            var text = chunk.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
            if (!string.IsNullOrEmpty(text))
            {
                yield return Result<AIStreamChunk>.Success(new AIStreamChunk
                {
                    Content = text,
                    IsComplete = false
                });
            }

            // Track token usage from metadata
            if (chunk.UsageMetadata != null)
            {
                totalPromptTokens = chunk.UsageMetadata.PromptTokenCount;
                totalCompletionTokens = chunk.UsageMetadata.CandidatesTokenCount;
            }

            var finishReason = chunk.Candidates?.FirstOrDefault()?.FinishReason;
            if (!string.IsNullOrEmpty(finishReason) && finishReason != "UNSPECIFIED")
            {
                yield return Result<AIStreamChunk>.Success(new AIStreamChunk
                {
                    IsComplete = true,
                    FinishReason = finishReason,
                    Usage = new TokenUsage
                    {
                        PromptTokens = totalPromptTokens,
                        CompletionTokens = totalCompletionTokens,
                        TotalTokens = totalPromptTokens + totalCompletionTokens
                    }
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
            var url = $"{ApiBaseUrl}/models?key={apiKey}";
            
            var response = await testClient.GetAsync(url, cancellationToken);
            return Result<bool>.Success(response.IsSuccessStatusCode);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"API key validation failed: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public override async Task<Result<TokenUsage>> CountTokensAsync(
        string text,
        string model,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{ApiBaseUrl}/models/{model}:countTokens?key={_settings.ApiKey}";
            var request = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text } } }
                }
            };

            var response = await HttpClient.PostAsJsonAsync(url, request, JsonOptions, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                // Fall back to estimation
                return await base.CountTokensAsync(text, model, cancellationToken);
            }

            var result = await response.Content.ReadFromJsonAsync<GeminiCountTokensResponse>(
                JsonOptions, cancellationToken);
            
            return Result<TokenUsage>.Success(new TokenUsage
            {
                PromptTokens = result?.TotalTokens ?? 0,
                CompletionTokens = 0,
                TotalTokens = result?.TotalTokens ?? 0
            });
        }
        catch
        {
            return await base.CountTokensAsync(text, model, cancellationToken);
        }
    }

    private object BuildRequest(AIGenerationRequest request)
    {
        var contents = new List<object>();
        
        // Gemini uses a different structure - system instruction separate from contents
        string? systemInstruction = null;
        if (request.Context != null)
        {
            systemInstruction = BuildSystemPrompt(request.Context);
        }

        contents.Add(new
        {
            role = "user",
            parts = new[] { new { text = request.Prompt } }
        });

        var result = new Dictionary<string, object>
        {
            ["contents"] = contents,
            ["generationConfig"] = new
            {
                temperature = request.Temperature,
                maxOutputTokens = request.MaxTokens,
                topP = 0.95,
                topK = 40
            }
        };

        if (!string.IsNullOrEmpty(systemInstruction))
        {
            result["systemInstruction"] = new
            {
                parts = new[] { new { text = systemInstruction } }
            };
        }

        return result;
    }

    private static AIGenerationResponse MapResponse(GeminiResponse response, string model)
    {
        var candidate = response.Candidates?.FirstOrDefault();
        var content = candidate?.Content?.Parts?.FirstOrDefault()?.Text ?? string.Empty;
        
        return new AIGenerationResponse
        {
            Content = content,
            Model = model,
            FinishReason = candidate?.FinishReason ?? "unknown",
            Usage = new TokenUsage
            {
                PromptTokens = response.UsageMetadata?.PromptTokenCount ?? 0,
                CompletionTokens = response.UsageMetadata?.CandidatesTokenCount ?? 0,
                TotalTokens = response.UsageMetadata?.TotalTokenCount ?? 0
            },
            GeneratedAt = DateTime.UtcNow
        };
    }
}

#region Gemini API Models

internal sealed class GeminiResponse
{
    public List<GeminiCandidate>? Candidates { get; set; }
    public GeminiUsageMetadata? UsageMetadata { get; set; }
}

internal sealed class GeminiCandidate
{
    public GeminiContent? Content { get; set; }
    public string? FinishReason { get; set; }
    public int Index { get; set; }
}

internal sealed class GeminiContent
{
    public List<GeminiPart>? Parts { get; set; }
    public string? Role { get; set; }
}

internal sealed class GeminiPart
{
    public string? Text { get; set; }
}

internal sealed class GeminiUsageMetadata
{
    public int PromptTokenCount { get; set; }
    public int CandidatesTokenCount { get; set; }
    public int TotalTokenCount { get; set; }
}

internal sealed class GeminiCountTokensResponse
{
    public int TotalTokens { get; set; }
}

#endregion

public sealed class GeminiSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string DefaultModel { get; set; } = "gemini-1.5-flash";
}