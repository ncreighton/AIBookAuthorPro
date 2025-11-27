// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Runtime.CompilerServices;
using System.Text.Json;
using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Interfaces;
using AIBookAuthorPro.Core.Models.AI;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.Infrastructure.AI;

/// <summary>
/// Base class for AI provider implementations with common functionality.
/// </summary>
public abstract class BaseAIProvider : IAIProvider
{
    protected readonly HttpClient HttpClient;
    protected readonly ILogger Logger;
    protected readonly JsonSerializerOptions JsonOptions;
    
    public abstract string Name { get; }
    public abstract AIProviderType ProviderType { get; }
    public abstract IReadOnlyList<AIModelInfo> AvailableModels { get; }

    protected BaseAIProvider(HttpClient httpClient, ILogger logger)
    {
        HttpClient = httpClient;
        Logger = logger;
        JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = false
        };
    }

    /// <inheritdoc />
    public abstract Task<Result<AIGenerationResponse>> GenerateAsync(
        AIGenerationRequest request,
        CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public abstract IAsyncEnumerable<Result<AIStreamChunk>> GenerateStreamAsync(
        AIGenerationRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public virtual Task<Result<bool>> ValidateApiKeyAsync(
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        // Default implementation - providers should override for actual validation
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return Task.FromResult(Result<bool>.Failure("API key is required"));
        }
        return Task.FromResult(Result<bool>.Success(true));
    }

    /// <inheritdoc />
    public virtual Task<Result<TokenUsage>> CountTokensAsync(
        string text,
        string model,
        CancellationToken cancellationToken = default)
    {
        // Rough estimation: ~4 characters per token on average
        var estimatedTokens = (int)Math.Ceiling(text.Length / 4.0);
        return Task.FromResult(Result<TokenUsage>.Success(new TokenUsage
        {
            PromptTokens = estimatedTokens,
            CompletionTokens = 0,
            TotalTokens = estimatedTokens
        }));
    }

    /// <summary>
    /// Builds the system prompt for book generation based on context.
    /// </summary>
    protected string BuildSystemPrompt(GenerationContext context)
    {
        var parts = new List<string>
        {
            "You are an expert author and creative writing assistant specializing in book creation.",
            "Your writing is engaging, vivid, and maintains consistent quality throughout."
        };

        if (!string.IsNullOrEmpty(context.Genre))
        {
            parts.Add($"You are writing in the {context.Genre} genre.");
        }

        if (!string.IsNullOrEmpty(context.ToneStyle))
        {
            parts.Add($"The tone and style should be: {context.ToneStyle}.");
        }

        if (!string.IsNullOrEmpty(context.TargetAudience))
        {
            parts.Add($"The target audience is: {context.TargetAudience}.");
        }

        if (!string.IsNullOrEmpty(context.WritingStyle))
        {
            parts.Add($"Follow this writing style: {context.WritingStyle}");
        }

        if (context.CharacterProfiles?.Any() == true)
        {
            parts.Add("\n## Character Profiles\n" + string.Join("\n\n", 
                context.CharacterProfiles.Select(c => $"**{c.Name}**: {c.Description}")));
        }

        if (context.WorldBuildingNotes?.Any() == true)
        {
            parts.Add("\n## World Building Notes\n" + string.Join("\n", context.WorldBuildingNotes));
        }

        if (!string.IsNullOrEmpty(context.PreviousChapterSummary))
        {
            parts.Add($"\n## Previous Chapter Summary\n{context.PreviousChapterSummary}");
        }

        if (!string.IsNullOrEmpty(context.OutlineContext))
        {
            parts.Add($"\n## Chapter Outline\n{context.OutlineContext}");
        }

        return string.Join("\n\n", parts);
    }

    /// <summary>
    /// Validates the request before processing.
    /// </summary>
    protected Result ValidateRequest(AIGenerationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
        {
            return Result.Failure("Prompt is required");
        }

        if (string.IsNullOrWhiteSpace(request.Model))
        {
            return Result.Failure("Model is required");
        }

        if (request.MaxTokens <= 0)
        {
            return Result.Failure("MaxTokens must be greater than 0");
        }

        return Result.Success();
    }

    /// <summary>
    /// Logs the generation request for debugging.
    /// </summary>
    protected void LogRequest(AIGenerationRequest request)
    {
        Logger.LogDebug(
            "AI Generation Request - Model: {Model}, MaxTokens: {MaxTokens}, Temperature: {Temperature}, Streaming: {Streaming}",
            request.Model,
            request.MaxTokens,
            request.Temperature,
            request.Stream);
    }

    /// <summary>
    /// Logs the generation response for debugging.
    /// </summary>
    protected void LogResponse(AIGenerationResponse response)
    {
        Logger.LogDebug(
            "AI Generation Response - Tokens: {TotalTokens}, FinishReason: {FinishReason}",
            response.Usage?.TotalTokens,
            response.FinishReason);
    }
}

/// <summary>
/// Stream chunk for incremental AI responses.
/// </summary>
public sealed class AIStreamChunk
{
    public string? Content { get; init; }
    public bool IsComplete { get; init; }
    public string? FinishReason { get; init; }
    public TokenUsage? Usage { get; init; }
}