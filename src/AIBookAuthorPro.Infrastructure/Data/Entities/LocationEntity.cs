// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

namespace AIBookAuthorPro.Infrastructure.Data.Entities;

/// <summary>
/// Location/setting entity for database persistence.
/// </summary>
public class LocationEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BookProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Type { get; set; }  // City, Building, Room, Landscape, etc.
    public string? Description { get; set; }
    public string? Atmosphere { get; set; }
    public string? Significance { get; set; }
    public string? ImageUrl { get; set; }
    public Guid? ParentLocationId { get; set; }  // For hierarchical locations

    // Detailed data stored as JSON
    public string? SensoryDetailsJson { get; set; }
    public string? HistoryJson { get; set; }
    public string? MapDataJson { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int SortOrder { get; set; }

    // Navigation
    public virtual BookProjectEntity? BookProject { get; set; }
}
