// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Common;

namespace AIBookAuthorPro.Core.Models;

/// <summary>
/// Represents a research note for the book project.
/// </summary>
public sealed class ResearchNote : Entity
{
    /// <summary>
    /// Gets or sets the note title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the note content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source of the research.
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Gets or sets the source URL.
    /// </summary>
    public string? SourceUrl { get; set; }

    /// <summary>
    /// Gets or sets the research category.
    /// </summary>
    public ResearchCategory Category { get; set; } = ResearchCategory.General;

    /// <summary>
    /// Gets or sets tags for organization.
    /// </summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>
    /// Gets or sets associated chapter IDs.
    /// </summary>
    public List<Guid> ChapterIds { get; set; } = [];

    /// <summary>
    /// Gets or sets associated character IDs.
    /// </summary>
    public List<Guid> CharacterIds { get; set; } = [];

    /// <summary>
    /// Gets or sets associated location IDs.
    /// </summary>
    public List<Guid> LocationIds { get; set; } = [];

    /// <summary>
    /// Gets or sets whether this note is important/starred.
    /// </summary>
    public bool IsStarred { get; set; }

    /// <summary>
    /// Gets or sets the date the research was collected.
    /// </summary>
    public DateTime? DateCollected { get; set; }
}

/// <summary>
/// Categories for research notes.
/// </summary>
public enum ResearchCategory
{
    /// <summary>General research.</summary>
    General,
    /// <summary>Historical research.</summary>
    Historical,
    /// <summary>Scientific/technical research.</summary>
    Scientific,
    /// <summary>Cultural/social research.</summary>
    Cultural,
    /// <summary>Geographic research.</summary>
    Geographic,
    /// <summary>Character-related research.</summary>
    Character,
    /// <summary>Plot-related research.</summary>
    Plot,
    /// <summary>Language/dialogue research.</summary>
    Language,
    /// <summary>Image/visual reference.</summary>
    Visual,
    /// <summary>Other research.</summary>
    Other
}
