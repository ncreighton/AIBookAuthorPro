// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

namespace AIBookAuthorPro.Infrastructure.Data.Entities;

/// <summary>
/// Book project entity for database persistence.
/// </summary>
public class BookProjectEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? Genre { get; set; }
    public string? SubGenre { get; set; }
    public string? TargetAudience { get; set; }
    public string? Premise { get; set; }
    public string? Logline { get; set; }
    public int TargetWordCount { get; set; }
    public int CurrentWordCount { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, InProgress, Generating, Review, Complete
    public double CompletionPercentage { get; set; }

    // Stored as JSON
    public string? OutlineJson { get; set; }
    public string? CharacterBibleJson { get; set; }
    public string? WorldBibleJson { get; set; }
    public string? StyleGuideJson { get; set; }
    public string? BlueprintJson { get; set; }
    public string? SettingsJson { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastGeneratedAt { get; set; }
    public string? CoverImageUrl { get; set; }

    // Navigation properties
    public virtual UserEntity? User { get; set; }
    public virtual ICollection<ChapterEntity> Chapters { get; set; } = new List<ChapterEntity>();
    public virtual ICollection<CharacterEntity> Characters { get; set; } = new List<CharacterEntity>();
    public virtual ICollection<LocationEntity> Locations { get; set; } = new List<LocationEntity>();
    public virtual ICollection<ExportHistoryEntity> ExportHistory { get; set; } = new List<ExportHistoryEntity>();
}
