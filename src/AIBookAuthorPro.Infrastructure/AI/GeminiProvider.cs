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
            ModelId = "gemini-2.0-flash-exp",
            DisplayName = "Gemini 2.0 Flash",
            Provider = Enums.AIProviderType.Ollama, // Note: Gemini uses Ollama enum value temporarily
            MaxContextTokens = 1000000,
            MaxOutputTokens = 8192,
            InputCostPer1K = 0.00015m, // Free tier available
            OutputCostPer1K = 0.0006m,
            SupportsStreaming = true,
            RecommendedFor = "Latest experimental model with 1M context window"
        },
        new AIModelInfo
        {
            ModelId = "gemini-1.5-pro",
            DisplayName = "Gemini 1.5 Pro",
            Provider = Enums.AIProviderType.Ollama, // Note: Gemini uses Ollama enum value temporarily
            MaxContextTokens = 2000000,
            MaxOutputTokens = 8192,
            InputCostPer1K = 0.00125m,
            OutputCostPer1K = 0.005m,
            SupportsStreaming = true,
            RecommendedFor = "Most capable Gemini model with 2M context window"
        },
        new AIModelInfo
        {
            ModelId = "gemini-1.5-flash",
            DisplayName = "Gemini 1.5 Flash",
            Provider = Enums.AIProviderType.Ollama, // Note: Gemini uses Ollama enum value temporarily
            MaxContextTokens = 1000000,
            MaxOutputTokens = 8192,
            InputCostPer1K = 0.000075m,
            OutputCostPer1K = 0.0003m,
            SupportsStreaming = true,
            RecommendedFor = "Fast and efficient model for high-volume tasks"
        },
        new AIModelInfo
        {
            ModelId = "gemini-1.5-flash-8b",
            DisplayName = "Gemini 1.5 Flash 8B",
            Provider = Enums.AIProviderType.Ollama, // Note: Gemini uses Ollama enum value temporarily
            MaxContextTokens = 1000000,
            MaxOutputTokens = 8192,
            InputCostPer1K = 0.0000375m,
            OutputCostPer1K = 0.00015m,
            SupportsStreaming = true,
            RecommendedFor = "Smallest and fastest Flash model for simple tasks"
        }
    };

    /// <inheritdoc />
    public override string ProviderName => "Google Gemini";
    
    /// <inheritdoc />
    public override bool SupportsStreaming => true;
    
    /// <inheritdoc />
    public override int MaxContextTokens => 2000000;
    
    /// <inheritdoc />
    public override bool IsConfigured => !string.IsNullOrEmpty(_settings.ApiKey);
    
    /// <inheritdoc />
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
            var geminiRequest = BuildRequest(compatRequest);
            var url = GetApiUrl(compatRequest.Model);
            
            var response = await HttpClient.PostAsJsonAsync(
                url,
                geminiRequest,
                JsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                Logger.LogError("Gemini API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return Result<GenerationResult>.Failure($"API error: {response.StatusCode} - {errorContent}");
            }

            var result = await response.Content.ReadFromJsonAsync<GeminiResponse>(JsonOptions, cancellationToken);
            if (result == null)
            {
                return Result<GenerationResult>.Failure("Failed to deserialize API response");
            }

            var compatResponse = MapResponse(result, compatRequest.Model);
            var generationResult = compatResponse.ToGenerationResult();
            LogResponse(generationResult);
            
            return Result<GenerationResult>.Success(generationResult);
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError(ex, "HTTP request failed for Gemini API");
            return Result<GenerationResult>.Failure($"Network error: {ex.Message}");
        }
        catch (TaskCanceledException ex) when (ex.CancellationToken == cancellationToken)
        {
            Logger.LogInformation("Request cancelled by user");
            return Result<GenerationResult>.Failure("Request cancelled");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error in Gemini generation");
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
        var geminiRequest = BuildRequest(compatRequest);
        var json = JsonSerializer.Serialize(geminiRequest, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var url = GetApiUrl(compatRequest.Model, stream: true) + "&alt=sse";

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
                Logger.LogError("Gemini streaming API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                yield break;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Gemini streaming request failed");
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
                yield return text;
            }
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
                InputTokens = response.UsageMetadata?.PromptTokenCount ?? 0,
                OutputTokens = response.UsageMetadata?.CandidatesTokenCount ?? 0
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