// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

namespace AIBookAuthorPro.Core.Models.AI;

/// <summary>
/// Represents a chunk of streaming content from an AI provider.
/// </summary>
public sealed class StreamingChunk
{
    /// <summary>
    /// Gets or sets the text content of this chunk.
    /// </summary>
    public string? Text { get; init; }

    /// <summary>
    /// Gets or sets whether this is the final chunk.
    /// </summary>
    public bool IsFinal { get; init; }

    /// <summary>
    /// Gets or sets the reason generation finished (if final).
    /// </summary>
    public string? FinishReason { get; init; }

    /// <summary>
    /// Gets or sets the token usage (only available in final chunk).
    /// </summary>
    public TokenUsage? Usage { get; init; }

    /// <summary>
    /// Gets or sets the model used for generation.
    /// </summary>
    public string? Model { get; init; }

    /// <summary>
    /// Gets or sets error information if an error occurred.
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// Gets whether this chunk contains an error.
    /// </summary>
    public bool IsError => !string.IsNullOrEmpty(Error);
}
