// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

namespace AIBookAuthorPro.Infrastructure.Data.Entities;

/// <summary>
/// Generated content entity for tracking AI generations.
/// </summary>
public class GeneratedContentEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ChapterId { get; set; }
    public string ContentType { get; set; } = "Chapter"; // Chapter, Outline, Scene, Dialogue, Description
    public string? Content { get; set; }
    public int Version { get; set; } = 1;
    public bool IsSelected { get; set; } // Is this the currently selected version?

    // Generation details
    public string? ModelUsed { get; set; }
    public double? Temperature { get; set; }
    public int? InputTokens { get; set; }
    public int? OutputTokens { get; set; }
    public decimal? Cost { get; set; }
    public int? GenerationTimeMs { get; set; }

    // Quality metrics
    public double? QualityScore { get; set; }
    public string? QualityReportJson { get; set; }

    // Prompt used (for regeneration/debugging)
    public string? PromptUsed { get; set; }
    public string? SystemPromptUsed { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? UserFeedback { get; set; }
    public bool WasRevised { get; set; }

    // Navigation
    public virtual ChapterEntity? Chapter { get; set; }
}
