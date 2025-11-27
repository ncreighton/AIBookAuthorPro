// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

namespace AIBookAuthorPro.Core.Models.AI;

/// <summary>
/// Options for building generation context.
/// </summary>
public sealed class ContextBuildOptions
{
    /// <summary>
    /// Gets or sets whether to include previous chapters summary.
    /// </summary>
    public bool IncludePreviousSummary { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum tokens for previous summary.
    /// </summary>
    public int MaxPreviousSummaryTokens { get; set; } = 2000;

    /// <summary>
    /// Gets or sets whether to include character context.
    /// </summary>
    public bool IncludeCharacters { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum tokens for character context.
    /// </summary>
    public int MaxCharacterTokens { get; set; } = 1000;

    /// <summary>
    /// Gets or sets custom character IDs to include (null = auto-select).
    /// </summary>
    public IEnumerable<Guid>? CustomCharacterIds { get; set; }

    /// <summary>
    /// Gets or sets whether to include location context.
    /// </summary>
    public bool IncludeLocations { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum tokens for location context.
    /// </summary>
    public int MaxLocationTokens { get; set; } = 500;

    /// <summary>
    /// Gets or sets custom location IDs to include (null = auto-select).
    /// </summary>
    public IEnumerable<Guid>? CustomLocationIds { get; set; }

    /// <summary>
    /// Gets or sets the maximum total context tokens (0 = no limit).
    /// </summary>
    public int MaxTotalContextTokens { get; set; } = 8000;

    /// <summary>
    /// Gets or sets whether to include research notes.
    /// </summary>
    public bool IncludeResearchNotes { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum tokens for research notes.
    /// </summary>
    public int MaxResearchNotesTokens { get; set; } = 500;

    /// <summary>
    /// Creates default options.
    /// </summary>
    public static ContextBuildOptions Default => new();

    /// <summary>
    /// Creates minimal options for faster generation.
    /// </summary>
    public static ContextBuildOptions Minimal => new()
    {
        IncludePreviousSummary = true,
        MaxPreviousSummaryTokens = 500,
        IncludeCharacters = true,
        MaxCharacterTokens = 300,
        IncludeLocations = false,
        MaxTotalContextTokens = 2000
    };

    /// <summary>
    /// Creates comprehensive options for best quality.
    /// </summary>
    public static ContextBuildOptions Comprehensive => new()
    {
        IncludePreviousSummary = true,
        MaxPreviousSummaryTokens = 4000,
        IncludeCharacters = true,
        MaxCharacterTokens = 2000,
        IncludeLocations = true,
        MaxLocationTokens = 1000,
        IncludeResearchNotes = true,
        MaxResearchNotesTokens = 1000,
        MaxTotalContextTokens = 16000
    };
}
