// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

namespace AIBookAuthorPro.Core.Interfaces;

/// <summary>
/// Options for building generation context.
/// </summary>
public sealed class ContextBuildOptions
{
    /// <summary>
    /// Gets or sets whether to include character context.
    /// </summary>
    public bool IncludeCharacters { get; init; } = true;

    /// <summary>
    /// Gets or sets whether to include location context.
    /// </summary>
    public bool IncludeLocations { get; init; } = true;

    /// <summary>
    /// Gets or sets whether to include previous chapter summary.
    /// </summary>
    public bool IncludePreviousSummary { get; init; } = true;

    /// <summary>
    /// Gets or sets the maximum characters for previous summary.
    /// </summary>
    public int MaxPreviousSummaryChars { get; init; } = 2000;

    /// <summary>
    /// Gets or sets the maximum number of characters to include.
    /// </summary>
    public int MaxCharacters { get; init; } = 10;

    /// <summary>
    /// Gets or sets the maximum number of locations to include.
    /// </summary>
    public int MaxLocations { get; init; } = 5;

    /// <summary>
    /// Gets or sets the maximum characters per character description.
    /// </summary>
    public int MaxCharsPerCharacter { get; init; } = 500;

    /// <summary>
    /// Gets or sets the maximum characters per location description.
    /// </summary>
    public int MaxCharsPerLocation { get; init; } = 300;

    /// <summary>
    /// Gets or sets the total token budget for context.
    /// </summary>
    public int? TokenBudget { get; init; }
}
