// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Enums;

namespace AIBookAuthorPro.Core.Models.AI;

/// <summary>
/// Represents a request to generate content using an AI provider.
/// </summary>
public sealed class GenerationRequest
{
    /// <summary>
    /// Gets or sets the system prompt.
    /// </summary>
    public string SystemPrompt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user prompt/message.
    /// </summary>
    public string UserPrompt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the AI provider to use.
    /// </summary>
    public AIProviderType Provider { get; set; } = AIProviderType.Anthropic;

    /// <summary>
    /// Gets or sets the specific model to use.
    /// </summary>
    public string? ModelId { get; set; }

    /// <summary>
    /// Gets or sets the temperature (creativity) setting (0.0 - 1.5).
    /// </summary>
    public double Temperature { get; set; } = 0.7;

    /// <summary>
    /// Gets or sets the maximum tokens to generate.
    /// </summary>
    public int MaxTokens { get; set; } = 4000;

    /// <summary>
    /// Gets or sets the generation mode.
    /// </summary>
    public GenerationMode Mode { get; set; } = GenerationMode.Standard;

    /// <summary>
    /// Gets or sets additional context to include.
    /// </summary>
    public GenerationContext? Context { get; set; }

    /// <summary>
    /// Gets or sets metadata for tracking/caching.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = [];
}

/// <summary>
/// Represents the result of a content generation request.
/// </summary>
public sealed class GenerationResult
{
    /// <summary>
    /// Gets or sets whether the generation was successful.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Gets or sets the generated content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error message if generation failed.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Gets or sets token usage information.
    /// </summary>
    public TokenUsage? Usage { get; set; }

    /// <summary>
    /// Gets or sets the model that was used.
    /// </summary>
    public string? ModelUsed { get; set; }

    /// <summary>
    /// Gets or sets the generation duration.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets the finish reason.
    /// </summary>
    public string? FinishReason { get; set; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static GenerationResult Success(string content, TokenUsage? usage = null) => new()
    {
        IsSuccess = true,
        Content = content,
        Usage = usage
    };

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public static GenerationResult Failure(string error) => new()
    {
        IsSuccess = false,
        Error = error
    };
}

/// <summary>
/// Represents token usage information.
/// </summary>
public sealed class TokenUsage
{
    /// <summary>
    /// Gets or sets the number of input tokens.
    /// </summary>
    public int InputTokens { get; set; }

    /// <summary>
    /// Gets or sets the number of output tokens.
    /// </summary>
    public int OutputTokens { get; set; }

    /// <summary>
    /// Gets the total tokens used.
    /// </summary>
    public int TotalTokens => InputTokens + OutputTokens;

    /// <summary>
    /// Gets or sets the estimated cost in USD.
    /// </summary>
    public decimal EstimatedCost { get; set; }
}

/// <summary>
/// Information about an AI model.
/// </summary>
public sealed class AIModelInfo
{
    /// <summary>
    /// Gets or sets the model ID.
    /// </summary>
    public string ModelId { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the provider type.
    /// </summary>
    public AIProviderType Provider { get; init; }

    /// <summary>
    /// Gets or sets the maximum context window in tokens.
    /// </summary>
    public int MaxContextTokens { get; init; }

    /// <summary>
    /// Gets or sets the maximum output tokens.
    /// </summary>
    public int MaxOutputTokens { get; init; }

    /// <summary>
    /// Gets or sets the cost per 1K input tokens in USD.
    /// </summary>
    public decimal InputCostPer1K { get; init; }

    /// <summary>
    /// Gets or sets the cost per 1K output tokens in USD.
    /// </summary>
    public decimal OutputCostPer1K { get; init; }

    /// <summary>
    /// Gets or sets whether this model supports streaming.
    /// </summary>
    public bool SupportsStreaming { get; init; } = true;

    /// <summary>
    /// Gets or sets the recommended use case.
    /// </summary>
    public string? RecommendedFor { get; init; }

    /// <summary>
    /// Gets or sets the model tier (fast, balanced, premium).
    /// </summary>
    public ModelTier Tier { get; init; } = ModelTier.Standard;
}

/// <summary>
/// Model tier classification.
/// </summary>
public enum ModelTier
{
    /// <summary>Fast, economical models.</summary>
    Fast = 0,
    /// <summary>Balanced quality and speed.</summary>
    Standard = 1,
    /// <summary>Highest quality models.</summary>
    Premium = 2
}
