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
            Id = "claude-sonnet-4-20250514",
            Name = "Claude Sonnet 4",
            Provider = AIProviderType.Anthropic,
            MaxContextTokens = 200000,
            MaxOutputTokens = 64000,
            CostPer1KInputTokens = 0.003m,
            CostPer1KOutputTokens = 0.015m,
            SupportsVision = true,
            SupportsStreaming = true,
            Description = "Most intelligent model with excellent creative writing capabilities"
        },
        new AIModelInfo
        {
            Id = "claude-sonnet-4-20250514",
            Name = "Claude Sonnet 4",
            Provider = AIProviderType.Anthropic,
            MaxContextTokens = 200000,
            MaxOutputTokens = 64000,
            CostPer1KInputTokens = 0.003m,
            CostPer1KOutputTokens = 0.015m,
            SupportsVision = true,
            SupportsStreaming = true,
            Description = "Balanced performance and speed for everyday tasks"
        },
        new AIModelInfo
        {
            Id = "claude-haiku-3-5-20241022",
            Name = "Claude 3.5 Haiku",
            Provider = AIProviderType.Anthropic,
            MaxContextTokens = 200000,
            MaxOutputTokens = 8192,
            CostPer1KInputTokens = 0.00025m,
            CostPer1KOutputTokens = 0.00125m,
            SupportsVision = true,
            SupportsStreaming = true,
            Description = "Fastest model for quick iterations"
        }
    };

    public override string Name => "Anthropic Claude";
    public override AIProviderType ProviderType => AIProviderType.Anthropic;
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
            var anthropicRequest = BuildRequest(request);
            
            var response = await HttpClient.PostAsJsonAsync(
                ApiUrl,
                anthropicRequest,
                JsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                Logger.LogError("Anthropic API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return Result<AIGenerationResponse>.Failure($"API error: {response.StatusCode} - {errorContent}");
            }

            var result = await response.Content.ReadFromJsonAsync<AnthropicResponse>(JsonOptions, cancellationToken);
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
            Logger.LogError(ex, "HTTP request failed for Anthropic API");
            return Result<AIGenerationResponse>.Failure($"Network error: {ex.Message}");
        }
        catch (TaskCanceledException ex) when (ex.CancellationToken == cancellationToken)
        {
            Logger.LogInformation("Request cancelled by user");
            return Result<AIGenerationResponse>.Failure("Request cancelled");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error in Anthropic generation");
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

        var anthropicRequest = BuildRequest(request, stream: true);
        var json = JsonSerializer.Serialize(anthropicRequest, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        try
        {
            response = await HttpClient.PostAsync(ApiUrl, content, cancellationToken);
            
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

        var totalInputTokens = 0;
        var totalOutputTokens = 0;

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

            switch (evt.Type)
            {
                case "message_start":
                    if (evt.Message?.Usage != null)
                    {
                        totalInputTokens = evt.Message.Usage.InputTokens;
                    }
                    break;

                case "content_block_delta":
                    if (evt.Delta?.Type == "text_delta" && !string.IsNullOrEmpty(evt.Delta.Text))
                    {
                        yield return Result<AIStreamChunk>.Success(new AIStreamChunk
                        {
                            Content = evt.Delta.Text,
                            IsComplete = false
                        });
                    }
                    break;

                case "message_delta":
                    if (evt.Usage != null)
                    {
                        totalOutputTokens = evt.Usage.OutputTokens;
                    }
                    break;

                case "message_stop":
                    yield return Result<AIStreamChunk>.Success(new AIStreamChunk
                    {
                        IsComplete = true,
                        FinishReason = "stop",
                        Usage = new TokenUsage
                        {
                            PromptTokens = totalInputTokens,
                            CompletionTokens = totalOutputTokens,
                            TotalTokens = totalInputTokens + totalOutputTokens
                        }
                    });
                    break;
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
            testClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
            testClient.DefaultRequestHeaders.Add("anthropic-version", ApiVersion);
            
            var testRequest = new
            {
                model = "claude-haiku-3-5-20241022",
                max_tokens = 10,
                messages = new[] { new { role = "user", content = "Hi" } }
            };

            var response = await testClient.PostAsJsonAsync(ApiUrl, testRequest, cancellationToken);
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
        
        // Add context as a user message if present
        if (!string.IsNullOrEmpty(request.Context?.OutlineContext) ||
            !string.IsNullOrEmpty(request.Context?.PreviousChapterSummary))
        {
            var contextParts = new List<string>();
            if (!string.IsNullOrEmpty(request.Context.PreviousChapterSummary))
            {
                contextParts.Add($"Previous chapter summary: {request.Context.PreviousChapterSummary}");
            }
            if (!string.IsNullOrEmpty(request.Context.OutlineContext))
            {
                contextParts.Add($"Chapter outline: {request.Context.OutlineContext}");
            }
        }

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
                PromptTokens = response.Usage?.InputTokens ?? 0,
                CompletionTokens = response.Usage?.OutputTokens ?? 0,
                TotalTokens = (response.Usage?.InputTokens ?? 0) + (response.Usage?.OutputTokens ?? 0)
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