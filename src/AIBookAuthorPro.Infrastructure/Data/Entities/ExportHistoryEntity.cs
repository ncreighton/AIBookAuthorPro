// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

namespace AIBookAuthorPro.Infrastructure.Data.Entities;

/// <summary>
/// Export history entity for tracking book exports.
/// </summary>
public class ExportHistoryEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BookProjectId { get; set; }
    public string Format { get; set; } = "DOCX"; // DOCX, PDF, EPUB, MOBI, HTML, Markdown, KDP
    public string? FileUrl { get; set; }  // Cloud storage URL if uploaded
    public string? FilePath { get; set; }  // Local file path
    public long? FileSizeBytes { get; set; }
    public string? FileName { get; set; }

    // Export settings used (stored as JSON)
    public string? ExportSettingsJson { get; set; }

    // Chapters included
    public int ChaptersIncluded { get; set; }
    public int TotalWordCount { get; set; }

    // KDP-specific fields
    public bool IsKdpReady { get; set; }
    public string? KdpCategoryPath { get; set; }
    public string? KdpKeywords { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool WasSuccessful { get; set; } = true;
    public string? ErrorMessage { get; set; }

    // Navigation
    public virtual BookProjectEntity? BookProject { get; set; }
}
