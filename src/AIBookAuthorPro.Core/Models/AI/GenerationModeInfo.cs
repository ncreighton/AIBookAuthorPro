// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Enums;

namespace AIBookAuthorPro.Core.Models.AI;

/// <summary>
/// Information about a generation mode.
/// </summary>
public sealed class GenerationModeInfo
{
    /// <summary>
    /// Gets or sets the generation mode.
    /// </summary>
    public GenerationMode Mode { get; init; }

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets what this mode is recommended for.
    /// </summary>
    public string RecommendedFor { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the estimated time.
    /// </summary>
    public string EstimatedTime { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the cost indicator (1-5).
    /// </summary>
    public int CostIndicator { get; init; }
}
