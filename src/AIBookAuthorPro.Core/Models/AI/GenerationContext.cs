// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Enums;

namespace AIBookAuthorPro.Core.Models.AI;

/// <summary>
/// Contains all context needed for AI generation.
/// </summary>
public sealed record GenerationContext
{
    /// <summary>
    /// Gets or sets the project ID.
    /// </summary>
    public Guid ProjectId { get; init; }

    /// <summary>
    /// Gets or sets the chapter ID.
    /// </summary>
    public Guid? ChapterId { get; init; }

    /// <summary>
    /// Gets or sets the book title.
    /// </summary>
    public string BookTitle { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the book genre.
    /// </summary>
    public string Genre { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the target audience.
    /// </summary>
    public string TargetAudience { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the writing style.
    /// </summary>
    public string Style { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the point of view.
    /// </summary>
    public PointOfView PointOfView { get; init; }

    /// <summary>
    /// Gets or sets the tense.
    /// </summary>
    public Tense Tense { get; init; }

    /// <summary>
    /// Gets or sets the chapter number.
    /// </summary>
    public int ChapterNumber { get; init; }

    /// <summary>
    /// Gets or sets the chapter title.
    /// </summary>
    public string ChapterTitle { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the chapter outline.
    /// </summary>
    public string? ChapterOutline { get; init; }

    /// <summary>
    /// Gets or sets the target word count.
    /// </summary>
    public int TargetWordCount { get; init; }

    /// <summary>
    /// Gets or sets the summary of previous chapters.
    /// </summary>
    public string? PreviousSummary { get; init; }

    /// <summary>
    /// Gets or sets character context strings.
    /// </summary>
    public IReadOnlyList<string> CharacterContexts { get; init; } = [];

    /// <summary>
    /// Gets or sets location context strings.
    /// </summary>
    public IReadOnlyList<string> LocationContexts { get; init; } = [];

    /// <summary>
    /// Gets or sets author notes for this chapter.
    /// </summary>
    public string? AuthorNotes { get; init; }

    /// <summary>
    /// Gets or sets any additional context.
    /// </summary>
    public string? AdditionalContext { get; init; }

    /// <summary>
    /// Gets or sets custom notes/instructions.
    /// </summary>
    public string? CustomNotes { get; init; }
}
