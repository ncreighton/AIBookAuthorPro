// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Common;

namespace AIBookAuthorPro.Core.Models;

/// <summary>
/// Represents a character in the book.
/// </summary>
public sealed class Character : Entity
{
    private readonly List<CharacterRelationship> _relationships = [];

    /// <summary>
    /// Gets or sets the character's full name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the character's nickname or alias.
    /// </summary>
    public string? Nickname { get; set; }

    /// <summary>
    /// Gets or sets the character's role (protagonist, antagonist, supporting, etc.).
    /// </summary>
    public string Role { get; set; } = "Supporting";

    /// <summary>
    /// Gets or sets whether this is a main character.
    /// </summary>
    public bool IsMainCharacter { get; set; }

    /// <summary>
    /// Gets or sets the character's age.
    /// </summary>
    public int? Age { get; set; }

    /// <summary>
    /// Gets or sets the character's gender.
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// Gets or sets the character's physical description.
    /// </summary>
    public string? PhysicalDescription { get; set; }

    /// <summary>
    /// Gets or sets the character's personality traits.
    /// </summary>
    public List<string> PersonalityTraits { get; set; } = [];

    /// <summary>
    /// Gets or sets the character's backstory.
    /// </summary>
    public string? Backstory { get; set; }

    /// <summary>
    /// Gets or sets the character's goals/motivations.
    /// </summary>
    public string? Goals { get; set; }

    /// <summary>
    /// Gets or sets the character's fears/weaknesses.
    /// </summary>
    public string? Fears { get; set; }

    /// <summary>
    /// Gets or sets the character's arc description.
    /// </summary>
    public string? CharacterArc { get; set; }

    /// <summary>
    /// Gets or sets the character's speaking style/voice notes.
    /// </summary>
    public string? VoiceNotes { get; set; }

    /// <summary>
    /// Gets or sets the character's occupation.
    /// </summary>
    public string? Occupation { get; set; }

    /// <summary>
    /// Gets or sets additional notes about this character.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets a brief summary for quick reference.
    /// </summary>
    public string? QuickSummary { get; set; }

    /// <summary>
    /// Gets the character's relationships with other characters.
    /// </summary>
    public IReadOnlyList<CharacterRelationship> Relationships => _relationships.AsReadOnly();

    /// <summary>
    /// Adds a relationship to another character.
    /// </summary>
    /// <param name="relationship">The relationship to add.</param>
    public void AddRelationship(CharacterRelationship relationship)
    {
        ArgumentNullException.ThrowIfNull(relationship);
        _relationships.Add(relationship);
        MarkAsModified();
    }

    /// <summary>
    /// Removes a relationship.
    /// </summary>
    /// <param name="otherCharacterId">The ID of the other character in the relationship.</param>
    /// <returns>True if the relationship was removed; otherwise, false.</returns>
    public bool RemoveRelationship(Guid otherCharacterId)
    {
        var relationship = _relationships.FirstOrDefault(r => r.OtherCharacterId == otherCharacterId);
        if (relationship is null) return false;
        
        _relationships.Remove(relationship);
        MarkAsModified();
        return true;
    }

    /// <summary>
    /// Generates a context string for AI prompts.
    /// </summary>
    /// <returns>A formatted string describing the character for AI context.</returns>
    public string ToContextString()
    {
        var parts = new List<string>
        {
            $"Name: {Name}"
        };

        if (!string.IsNullOrWhiteSpace(Role))
            parts.Add($"Role: {Role}");
        
        if (Age.HasValue)
            parts.Add($"Age: {Age}");
        
        if (!string.IsNullOrWhiteSpace(Gender))
            parts.Add($"Gender: {Gender}");
        
        if (!string.IsNullOrWhiteSpace(PhysicalDescription))
            parts.Add($"Appearance: {PhysicalDescription}");
        
        if (PersonalityTraits.Count > 0)
            parts.Add($"Personality: {string.Join(", ", PersonalityTraits)}");
        
        if (!string.IsNullOrWhiteSpace(Goals))
            parts.Add($"Goals: {Goals}");
        
        if (!string.IsNullOrWhiteSpace(VoiceNotes))
            parts.Add($"Voice/Speech: {VoiceNotes}");
        
        if (!string.IsNullOrWhiteSpace(QuickSummary))
            parts.Add($"Summary: {QuickSummary}");

        return string.Join("\n", parts);
    }
}

/// <summary>
/// Represents a relationship between two characters.
/// </summary>
public sealed class CharacterRelationship
{
    /// <summary>
    /// Gets or sets the ID of the other character in this relationship.
    /// </summary>
    public Guid OtherCharacterId { get; set; }

    /// <summary>
    /// Gets or sets the type of relationship (friend, enemy, lover, family, etc.).
    /// </summary>
    public string RelationshipType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a description of the relationship.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the relationship strength (-100 to 100, negative = hostile).
    /// </summary>
    public int Strength { get; set; } = 0;
}

/// <summary>
/// Represents a location or setting in the book.
/// </summary>
public sealed class Location : Entity
{
    /// <summary>
    /// Gets or sets the location name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the location type (city, room, forest, etc.).
    /// </summary>
    public string? LocationType { get; set; }

    /// <summary>
    /// Gets or sets the detailed description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets sensory details (sights, sounds, smells).
    /// </summary>
    public string? SensoryDetails { get; set; }

    /// <summary>
    /// Gets or sets the atmosphere/mood of this location.
    /// </summary>
    public string? Atmosphere { get; set; }

    /// <summary>
    /// Gets or sets historical or background information.
    /// </summary>
    public string? History { get; set; }

    /// <summary>
    /// Gets or sets significant features of this location.
    /// </summary>
    public List<string> Features { get; set; } = [];

    /// <summary>
    /// Gets or sets the parent location ID (for hierarchical locations).
    /// </summary>
    public Guid? ParentLocationId { get; set; }

    /// <summary>
    /// Gets or sets additional notes.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Generates a context string for AI prompts.
    /// </summary>
    /// <returns>A formatted string describing the location for AI context.</returns>
    public string ToContextString()
    {
        var parts = new List<string>
        {
            $"Location: {Name}"
        };

        if (!string.IsNullOrWhiteSpace(LocationType))
            parts.Add($"Type: {LocationType}");
        
        if (!string.IsNullOrWhiteSpace(Description))
            parts.Add($"Description: {Description}");
        
        if (!string.IsNullOrWhiteSpace(SensoryDetails))
            parts.Add($"Sensory Details: {SensoryDetails}");
        
        if (!string.IsNullOrWhiteSpace(Atmosphere))
            parts.Add($"Atmosphere: {Atmosphere}");
        
        if (Features.Count > 0)
            parts.Add($"Features: {string.Join(", ", Features)}");

        return string.Join("\n", parts);
    }
}

/// <summary>
/// Represents a research note for the project.
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
    /// Gets or sets the source/citation.
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Gets or sets the source URL.
    /// </summary>
    public string? SourceUrl { get; set; }

    /// <summary>
    /// Gets or sets tags for categorization.
    /// </summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>
    /// Gets or sets related chapter IDs.
    /// </summary>
    public List<Guid> RelatedChapterIds { get; set; } = [];
}
