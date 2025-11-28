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
/// Anthropic Claude AI provider implementation.
/// </summary>
public sealed class AnthropicProvider : BaseAIProvider
{
    private const string ApiUrl = "https://api.anthropic.com/v1/messages";
    private const string ApiVersion = "2023-06-01";
    
    private readonly AnthropicSettings _settings;
    
    private static readonly List<AIModelInfo> _models = new()
    {
        new AIModelInfo
        {
            ModelId = "claude-sonnet-4-20250514",
            DisplayName = "Claude Sonnet 4",
            Provider = AIProviderType.Claude,
            MaxContextTokens = 200000,
            MaxOutputTokens = 64000,
            InputCostPer1K = 0.003m,
            OutputCostPer1K = 0.015m,
            SupportsStreaming = true,
            RecommendedFor = "Most intelligent model with excellent creative writing capabilities"
        },
        new AIModelInfo
        {
            ModelId = "claude-sonnet-4-20250514",
            DisplayName = "Claude Sonnet 4",
            Provider = AIProviderType.Claude,
            MaxContextTokens = 200000,
            MaxOutputTokens = 64000,
            InputCostPer1K = 0.003m,
            OutputCostPer1K = 0.015m,
            SupportsStreaming = true,
            RecommendedFor = "Balanced performance and speed for everyday tasks"
        },
        new AIModelInfo
        {
            ModelId = "claude-haiku-3-5-20241022",
            DisplayName = "Claude 3.5 Haiku",
            Provider = AIProviderType.Claude,
            MaxContextTokens = 200000,
            MaxOutputTokens = 8192,
            InputCostPer1K = 0.00025m,
            OutputCostPer1K = 0.00125m,
            SupportsStreaming = true,
            RecommendedFor = "Fastest model for quick iterations"
        }
    };

    /// <inheritdoc />
    public override string ProviderName => "Anthropic Claude";
    
    /// <inheritdoc />
    public override bool SupportsStreaming => true;
    
    /// <inheritdoc />
    public override int MaxContextTokens => 200000;
    
    /// <inheritdoc />
    public override bool IsConfigured => !string.IsNullOrEmpty(_settings.ApiKey);
    
    /// <inheritdoc />
    public override IReadOnlyList<AIModelInfo> AvailableModels => _models;

    public AnthropicProvider(
        HttpClient httpClient,
        IOptions<AnthropicSettings> settings,
        ILogger<AnthropicProvider> logger)
        : base(httpClient, logger)
    {
        _settings = settings.Value;
        ConfigureHttpClient();
    }

    private void ConfigureHttpClient()
    {
        HttpClient.DefaultRequestHeaders.Clear();
        HttpClient.DefaultRequestHeaders.Add("x-api-key", _settings.ApiKey);
        HttpClient.DefaultRequestHeaders.Add("anthropic-version", ApiVersion);
        HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
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
            var anthropicRequest = BuildRequest(compatRequest);
            
            var response = await HttpClient.PostAsJsonAsync(
                ApiUrl,
                anthropicRequest,
                JsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                Logger.LogError("Anthropic API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return Result<GenerationResult>.Failure($"API error: {response.StatusCode} - {errorContent}");
            }

            var result = await response.Content.ReadFromJsonAsync<AnthropicResponse>(JsonOptions, cancellationToken);
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
            Logger.LogError(ex, "HTTP request failed for Anthropic API");
            return Result<GenerationResult>.Failure($"Network error: {ex.Message}");
        }
        catch (TaskCanceledException ex) when (ex.CancellationToken == cancellationToken)
        {
            Logger.LogInformation("Request cancelled by user");
            return Result<GenerationResult>.Failure("Request cancelled");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error in Anthropic generation");
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
        var anthropicRequest = BuildRequest(compatRequest, stream: true);
        var json = JsonSerializer.Serialize(anthropicRequest, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        try
        {
            response = await HttpClient.PostAsync(ApiUrl, content, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                Logger.LogError("Anthropic streaming API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                yield break;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Anthropic streaming request failed");
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
            if (data == "[DONE]") break;

            AnthropicStreamEvent? evt;
            try
            {
                evt = JsonSerializer.Deserialize<AnthropicStreamEvent>(data, JsonOptions);
            }
            catch
            {
                continue;
            }

            if (evt == null) continue;

            if (evt.Type == "content_block_delta" && evt.Delta?.Type == "text_delta" && !string.IsNullOrEmpty(evt.Delta.Text))
            {
                yield return evt.Delta.Text;
            }
        }
    }

    private object BuildRequest(AIGenerationRequest request, bool stream = false)
    {
        var messages = new List<object>();
        
        // Note: Context is handled in BuildSystemPrompt, so we don't need to add it separately here

        messages.Add(new { role = "user", content = request.Prompt });

        return new
        {
            model = request.Model,
            max_tokens = request.MaxTokens,
            temperature = request.Temperature,
            system = request.Context != null ? BuildSystemPrompt(request.Context) : null,
            messages,
            stream
        };
    }

    private static AIGenerationResponse MapResponse(AnthropicResponse response, string model)
    {
        var content = response.Content?.FirstOrDefault()?.Text ?? string.Empty;
        
        return new AIGenerationResponse
        {
            Content = content,
            Model = model,
            FinishReason = response.StopReason ?? "unknown",
            Usage = new TokenUsage
            {
                InputTokens = response.Usage?.InputTokens ?? 0,
                OutputTokens = response.Usage?.OutputTokens ?? 0
            },
            GeneratedAt = DateTime.UtcNow
        };
    }
}

#region Anthropic API Models

internal sealed class AnthropicResponse
{
    public string? Id { get; set; }
    public string? Type { get; set; }
    public string? Role { get; set; }
    public List<AnthropicContentBlock>? Content { get; set; }
    public string? Model { get; set; }
    public string? StopReason { get; set; }
    public AnthropicUsage? Usage { get; set; }
}

internal sealed class AnthropicContentBlock
{
    public string? Type { get; set; }
    public string? Text { get; set; }
}

internal sealed class AnthropicUsage
{
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
}

internal sealed class AnthropicStreamEvent
{
    public string? Type { get; set; }
    public AnthropicStreamMessage? Message { get; set; }
    public AnthropicStreamDelta? Delta { get; set; }
    public AnthropicStreamUsage? Usage { get; set; }
    public int? Index { get; set; }
}

internal sealed class AnthropicStreamMessage
{
    public AnthropicStreamUsage? Usage { get; set; }
}

internal sealed class AnthropicStreamDelta
{
    public string? Type { get; set; }
    public string? Text { get; set; }
}

internal sealed class AnthropicStreamUsage
{
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
}

#endregion

public sealed class AnthropicSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string DefaultModel { get; set; } = "claude-sonnet-4-20250514";
}