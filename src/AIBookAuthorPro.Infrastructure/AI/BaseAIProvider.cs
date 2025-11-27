// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Enums;
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
    
    /// <inheritdoc />
    public abstract string ProviderName { get; }
    
    /// <inheritdoc />
    public abstract bool SupportsStreaming { get; }
    
    /// <inheritdoc />
    public abstract int MaxContextTokens { get; }
    
    /// <inheritdoc />
    public abstract bool IsConfigured { get; }
    
    /// <inheritdoc />
    public abstract IReadOnlyList<AIModelInfo> AvailableModels { get; }

    protected BaseAIProvider(HttpClient httpClient, ILogger logger)
    {
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = false
        };
    }

    /// <inheritdoc />
    public abstract Task<Result<GenerationResult>> GenerateAsync(
        GenerationRequest request,
        CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public abstract IAsyncEnumerable<string> StreamGenerateAsync(
        GenerationRequest request,
        CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public virtual int EstimateTokenCount(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;
            
        // Rough estimation: ~4 characters per token on average for English
        return (int)Math.Ceiling(text.Length / 4.0);
    }

    /// <inheritdoc />
    public virtual IReadOnlyList<string> GetAvailableModels()
    {
        return AvailableModels.Select(m => m.ModelId).ToList();
    }

    /// <summary>
    /// Builds the system prompt for book generation based on context.
    /// </summary>
    protected string BuildSystemPrompt(GenerationContext? context)
    {
        if (context == null)
        {
            return "You are an expert author and creative writing assistant specializing in book creation. " +
                   "Your writing is engaging, vivid, and maintains consistent quality throughout.";
        }

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
    protected Result ValidateRequest(GenerationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserPrompt))
        {
            return Result.Failure("User prompt is required");
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
    protected void LogRequest(GenerationRequest request)
    {
        Logger.LogDebug(
            "AI Generation Request - Provider: {Provider}, Mode: {Mode}, MaxTokens: {MaxTokens}, Temperature: {Temperature}",
            ProviderName,
            request.Mode,
            request.MaxTokens,
            request.Temperature);
    }

    /// <summary>
    /// Logs the generation response for debugging.
    /// </summary>
    protected void LogResponse(GenerationResult response)
    {
        Logger.LogDebug(
            "AI Generation Response - Tokens: {TotalTokens}, FinishReason: {FinishReason}",
            response.Usage?.TotalTokens,
            response.FinishReason);
    }
}
