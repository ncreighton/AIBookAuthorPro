// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

namespace AIBookAuthorPro.Core.Models.AI;

/// <summary>
/// Progress information for generation operations.
/// </summary>
public sealed class GenerationProgress
{
    /// <summary>
    /// Gets or sets the current phase description.
    /// </summary>
    public string Phase { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the progress percentage (0-100).
    /// </summary>
    public double Percentage { get; init; }

    /// <summary>
    /// Gets or sets the current word count.
    /// </summary>
    public int WordCount { get; init; }

    /// <summary>
    /// Gets or sets the tokens used so far.
    /// </summary>
    public int TokensUsed { get; init; }

    /// <summary>
    /// Gets or sets whether generation is complete.
    /// </summary>
    public bool IsComplete { get; init; }

    /// <summary>
    /// Gets or sets any error message.
    /// </summary>
    public string? Error { get; init; }
}
