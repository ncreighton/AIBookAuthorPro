// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

namespace AIBookAuthorPro.Infrastructure.Data.Entities;

/// <summary>
/// Character entity for database persistence.
/// </summary>
public class CharacterEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BookProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Role { get; set; }  // Protagonist, Antagonist, Supporting, Minor
    public string? Archetype { get; set; }
    public string? Description { get; set; }
    public string? Backstory { get; set; }
    public string? Motivation { get; set; }
    public string? Arc { get; set; }
    public string? VoiceDescription { get; set; }
    public string? SpeechPatterns { get; set; }
    public int? Age { get; set; }
    public string? PhysicalDescription { get; set; }
    public string? ImageUrl { get; set; }

    // Detailed data stored as JSON
    public string? TraitsJson { get; set; }
    public string? RelationshipsJson { get; set; }
    public string? ArcDetailsJson { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int SortOrder { get; set; }

    // Navigation
    public virtual BookProjectEntity? BookProject { get; set; }
}
