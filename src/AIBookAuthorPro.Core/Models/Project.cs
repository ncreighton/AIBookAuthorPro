// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Enums;

namespace AIBookAuthorPro.Core.Models;

/// <summary>
/// Represents a book project containing all content and metadata.
/// This is the root aggregate for all book-related data.
/// </summary>
public sealed class Project : Entity
{
    private readonly List<Chapter> _chapters = [];
    private readonly List<Character> _characters = [];
    private readonly List<Location> _locations = [];
    private readonly List<ResearchNote> _researchNotes = [];

    /// <summary>
    /// Gets or sets the project name (typically the book title).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project description or logline.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the book metadata (title, author, ISBN, etc.).
    /// </summary>
    public BookMetadata Metadata { get; set; } = new();

    /// <summary>
    /// Gets or sets the book outline.
    /// </summary>
    public Outline? Outline { get; set; }

    /// <summary>
    /// Gets or sets the project status.
    /// </summary>
    public ProjectStatus Status { get; set; } = ProjectStatus.Planning;

    /// <summary>
    /// Gets or sets the template used for this project.
    /// </summary>
    public string? TemplateName { get; set; }

    /// <summary>
    /// Gets the collection of chapters in this project.
    /// </summary>
    public IReadOnlyList<Chapter> Chapters => _chapters.AsReadOnly();

    /// <summary>
    /// Gets the collection of characters in this project.
    /// </summary>
    public IReadOnlyList<Character> Characters => _characters.AsReadOnly();

    /// <summary>
    /// Gets the collection of locations/settings in this project.
    /// </summary>
    public IReadOnlyList<Location> Locations => _locations.AsReadOnly();

    /// <summary>
    /// Gets the collection of research notes for this project.
    /// </summary>
    public IReadOnlyList<ResearchNote> ResearchNotes => _researchNotes.AsReadOnly();

    /// <summary>
    /// Gets or sets the generation settings for this project.
    /// </summary>
    public GenerationSettings GenerationSettings { get; set; } = new();

    /// <summary>
    /// Gets or sets the path where the project file is saved.
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// Gets the total word count across all chapters.
    /// </summary>
    public int TotalWordCount => _chapters.Sum(c => c.WordCount);

    /// <summary>
    /// Gets the target word count for the entire book.
    /// </summary>
    public int TargetWordCount { get; set; } = 80000;

    /// <summary>
    /// Gets the completion percentage based on word count.
    /// </summary>
    public double CompletionPercentage => 
        TargetWordCount > 0 ? Math.Min(100, (double)TotalWordCount / TargetWordCount * 100) : 0;

    /// <summary>
    /// Adds a chapter to the project.
    /// </summary>
    /// <param name="chapter">The chapter to add.</param>
    public void AddChapter(Chapter chapter)
    {
        ArgumentNullException.ThrowIfNull(chapter);
        chapter.Order = _chapters.Count + 1;
        _chapters.Add(chapter);
        MarkAsModified();
    }

    /// <summary>
    /// Removes a chapter from the project.
    /// </summary>
    /// <param name="chapterId">The ID of the chapter to remove.</param>
    /// <returns>True if the chapter was removed; otherwise, false.</returns>
    public bool RemoveChapter(Guid chapterId)
    {
        var chapter = _chapters.FirstOrDefault(c => c.Id == chapterId);
        if (chapter is null) return false;
        
        _chapters.Remove(chapter);
        ReorderChapters();
        MarkAsModified();
        return true;
    }

    /// <summary>
    /// Moves a chapter to a new position.
    /// </summary>
    /// <param name="chapterId">The ID of the chapter to move.</param>
    /// <param name="newOrder">The new order position (1-based).</param>
    public void MoveChapter(Guid chapterId, int newOrder)
    {
        var chapter = _chapters.FirstOrDefault(c => c.Id == chapterId);
        if (chapter is null) return;

        newOrder = Math.Clamp(newOrder, 1, _chapters.Count);
        _chapters.Remove(chapter);
        _chapters.Insert(newOrder - 1, chapter);
        ReorderChapters();
        MarkAsModified();
    }

    /// <summary>
    /// Adds a character to the project.
    /// </summary>
    /// <param name="character">The character to add.</param>
    public void AddCharacter(Character character)
    {
        ArgumentNullException.ThrowIfNull(character);
        _characters.Add(character);
        MarkAsModified();
    }

    /// <summary>
    /// Removes a character from the project.
    /// </summary>
    /// <param name="characterId">The ID of the character to remove.</param>
    /// <returns>True if the character was removed; otherwise, false.</returns>
    public bool RemoveCharacter(Guid characterId)
    {
        var character = _characters.FirstOrDefault(c => c.Id == characterId);
        if (character is null) return false;
        
        _characters.Remove(character);
        MarkAsModified();
        return true;
    }

    /// <summary>
    /// Adds a location to the project.
    /// </summary>
    /// <param name="location">The location to add.</param>
    public void AddLocation(Location location)
    {
        ArgumentNullException.ThrowIfNull(location);
        _locations.Add(location);
        MarkAsModified();
    }

    /// <summary>
    /// Removes a location from the project.
    /// </summary>
    /// <param name="locationId">The ID of the location to remove.</param>
    /// <returns>True if the location was removed; otherwise, false.</returns>
    public bool RemoveLocation(Guid locationId)
    {
        var location = _locations.FirstOrDefault(l => l.Id == locationId);
        if (location is null) return false;
        
        _locations.Remove(location);
        MarkAsModified();
        return true;
    }

    /// <summary>
    /// Adds a research note to the project.
    /// </summary>
    /// <param name="note">The research note to add.</param>
    public void AddResearchNote(ResearchNote note)
    {
        ArgumentNullException.ThrowIfNull(note);
        _researchNotes.Add(note);
        MarkAsModified();
    }

    /// <summary>
    /// Removes a research note from the project.
    /// </summary>
    /// <param name="noteId">The ID of the note to remove.</param>
    /// <returns>True if the note was removed; otherwise, false.</returns>
    public bool RemoveResearchNote(Guid noteId)
    {
        var note = _researchNotes.FirstOrDefault(n => n.Id == noteId);
        if (note is null) return false;
        
        _researchNotes.Remove(note);
        MarkAsModified();
        return true;
    }

    private void ReorderChapters()
    {
        for (int i = 0; i < _chapters.Count; i++)
        {
            _chapters[i].Order = i + 1;
        }
    }
}

/// <summary>
/// Represents metadata for a book.
/// </summary>
public sealed class BookMetadata
{
    /// <summary>
    /// Gets or sets the book title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the book subtitle.
    /// </summary>
    public string? Subtitle { get; set; }

    /// <summary>
    /// Gets or sets the author name.
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the author bio.
    /// </summary>
    public string? AuthorBio { get; set; }

    /// <summary>
    /// Gets or sets the book genre.
    /// </summary>
    public string Genre { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional genre tags.
    /// </summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>
    /// Gets or sets the ISBN-10.
    /// </summary>
    public string? Isbn10 { get; set; }

    /// <summary>
    /// Gets or sets the ISBN-13.
    /// </summary>
    public string? Isbn13 { get; set; }

    /// <summary>
    /// Gets or sets the publisher name.
    /// </summary>
    public string? Publisher { get; set; }

    /// <summary>
    /// Gets or sets the publication date.
    /// </summary>
    public DateTime? PublicationDate { get; set; }

    /// <summary>
    /// Gets or sets the language code (e.g., "en-US").
    /// </summary>
    public string Language { get; set; } = "en-US";

    /// <summary>
    /// Gets or sets the book description/blurb.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets keywords for SEO/discovery.
    /// </summary>
    public List<string> Keywords { get; set; } = [];

    /// <summary>
    /// Gets or sets the copyright notice.
    /// </summary>
    public string? Copyright { get; set; }

    /// <summary>
    /// Gets or sets the target audience description.
    /// </summary>
    public string? TargetAudience { get; set; }

    /// <summary>
    /// Gets or sets the book category.
    /// </summary>
    public BookCategory Category { get; set; } = BookCategory.Fiction;
}

/// <summary>
/// Represents generation settings for AI content creation.
/// </summary>
public sealed class GenerationSettings
{
    /// <summary>
    /// Gets or sets the preferred AI provider.
    /// </summary>
    public AIProviderType Provider { get; set; } = AIProviderType.Claude;

    /// <summary>
    /// Gets or sets the generation mode/quality level.
    /// </summary>
    public GenerationMode Mode { get; set; } = GenerationMode.Standard;

    /// <summary>
    /// Gets or sets the creativity level (temperature, 0.0 - 1.5).
    /// </summary>
    public double Temperature { get; set; } = 0.7;

    /// <summary>
    /// Gets or sets the default point of view.
    /// </summary>
    public PointOfView PointOfView { get; set; } = PointOfView.ThirdPersonLimited;

    /// <summary>
    /// Gets or sets the default tense.
    /// </summary>
    public Tense Tense { get; set; } = Tense.Past;

    /// <summary>
    /// Gets or sets the target chapter length in words.
    /// </summary>
    public int TargetChapterLength { get; set; } = 3000;

    /// <summary>
    /// Gets or sets the writing style description.
    /// </summary>
    public string? StyleDescription { get; set; }

    /// <summary>
    /// Gets or sets the tone description (e.g., "dark", "humorous").
    /// </summary>
    public string? ToneDescription { get; set; }

    /// <summary>
    /// Gets or sets custom instructions to include in every generation.
    /// </summary>
    public string? CustomInstructions { get; set; }
}
