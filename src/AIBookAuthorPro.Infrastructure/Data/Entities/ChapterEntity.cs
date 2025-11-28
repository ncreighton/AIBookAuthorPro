// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

namespace AIBookAuthorPro.Infrastructure.Data.Entities;

/// <summary>
/// Chapter entity for database persistence.
/// </summary>
public class ChapterEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BookProjectId { get; set; }
    public int ChapterNumber { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? FormattedContent { get; set; } // HTML/RTF formatted
    public int WordCount { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Generating, Generated, Reviewing, Approved, Failed
    public double? QualityScore { get; set; }
    public int Version { get; set; } = 1;
    public int RevisionCount { get; set; }

    // Blueprint data stored as JSON
    public string? BlueprintJson { get; set; }
    public string? QualityReportJson { get; set; }
    public string? ContinuityReportJson { get; set; }
    public string? SummaryJson { get; set; }

    // Generation metadata
    public string? ModelUsed { get; set; }
    public int? TokensUsed { get; set; }
    public int? GenerationTimeMs { get; set; }
    public decimal? GenerationCost { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? GeneratedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }

    // Navigation properties
    public virtual BookProjectEntity? BookProject { get; set; }
    public virtual ICollection<GeneratedContentEntity> GeneratedContent { get; set; } = new List<GeneratedContentEntity>();
}
