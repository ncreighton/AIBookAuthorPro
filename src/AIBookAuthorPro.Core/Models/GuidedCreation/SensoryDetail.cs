// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

namespace AIBookAuthorPro.Core.Models.GuidedCreation;

/// <summary>
/// Sensory detail for locations and scenes.
/// </summary>
public sealed record SensoryDetail
{
    /// <summary>
    /// Sense type (sight, sound, smell, taste, touch).
    /// </summary>
    public required string SenseType { get; init; }

    /// <summary>
    /// Description.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Intensity (subtle, moderate, strong).
    /// </summary>
    public string Intensity { get; init; } = "moderate";

    /// <summary>
    /// Emotional association.
    /// </summary>
    public string? EmotionalAssociation { get; init; }

    /// <summary>
    /// When this is prominent (time of day, season, etc.).
    /// </summary>
    public string? WhenProminent { get; init; }
}

/// <summary>
/// Token usage record.
/// </summary>
public sealed record TokenUsage(
    int InputTokens,
    int OutputTokens)
{
    /// <summary>
    /// Total tokens.
    /// </summary>
    public int TotalTokens => InputTokens + OutputTokens;
}
