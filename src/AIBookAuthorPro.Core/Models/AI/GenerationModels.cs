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
    public AIProviderType Provider { get; set; } = AIProviderType.Claude;

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
/// Represents context information for content generation.
/// </summary>
public sealed class GenerationContext
{
    /// <summary>
    /// Gets or sets the project ID.
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the chapter ID being generated.
    /// </summary>
    public Guid? ChapterId { get; set; }

    /// <summary>
    /// Gets or sets the chapter number.
    /// </summary>
    public int ChapterNumber { get; set; }

    /// <summary>
    /// Gets or sets the book title.
    /// </summary>
    public string BookTitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the genre.
    /// </summary>
    public string Genre { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target audience.
    /// </summary>
    public string? TargetAudience { get; set; }

    /// <summary>
    /// Gets or sets the writing style description.
    /// </summary>
    public string? Style { get; set; }

    /// <summary>
    /// Gets or sets the point of view.
    /// </summary>
    public PointOfView PointOfView { get; set; }

    /// <summary>
    /// Gets or sets the tense.
    /// </summary>
    public Tense Tense { get; set; }

    /// <summary>
    /// Gets or sets the summary of previous chapters.
    /// </summary>
    public string? PreviousSummary { get; set; }

    /// <summary>
    /// Gets or sets the current chapter outline.
    /// </summary>
    public string? ChapterOutline { get; set; }

    /// <summary>
    /// Gets or sets relevant character information.
    /// </summary>
    public List<string> CharacterContexts { get; set; } = [];

    /// <summary>
    /// Gets or sets relevant location information.
    /// </summary>
    public List<string> LocationContexts { get; set; } = [];

    /// <summary>
    /// Gets or sets custom notes/instructions.
    /// </summary>
    public string? CustomNotes { get; set; }

    /// <summary>
    /// Gets or sets the target word count.
    /// </summary>
    public int TargetWordCount { get; set; } = 3000;
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
/// Represents a streaming chunk of generated content.
/// </summary>
public sealed class StreamingChunk
{
    /// <summary>
    /// Gets or sets the text content of this chunk.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this is the final chunk.
    /// </summary>
    public bool IsFinal { get; set; }

    /// <summary>
    /// Gets or sets the finish reason (if final).
    /// </summary>
    public string? FinishReason { get; set; }

    /// <summary>
    /// Gets or sets usage information (if final).
    /// </summary>
    public TokenUsage? Usage { get; set; }
}
