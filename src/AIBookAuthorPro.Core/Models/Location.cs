// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Common;

namespace AIBookAuthorPro.Core.Models;

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
    /// Gets or sets the location description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the atmosphere/mood of this location.
    /// </summary>
    public string? Atmosphere { get; set; }

    /// <summary>
    /// Gets or sets sensory details (sights, sounds, smells, etc.).
    /// </summary>
    public string? SensoryDetails { get; set; }

    /// <summary>
    /// Gets or sets the location type (city, building, natural, etc.).
    /// </summary>
    public LocationType Type { get; set; } = LocationType.Other;

    /// <summary>
    /// Gets or sets the parent location ID (for hierarchical locations).
    /// </summary>
    public Guid? ParentLocationId { get; set; }

    /// <summary>
    /// Gets or sets historical notes about this location.
    /// </summary>
    public string? History { get; set; }

    /// <summary>
    /// Gets or sets notable features of this location.
    /// </summary>
    public List<string> Features { get; set; } = [];

    /// <summary>
    /// Gets or sets IDs of characters associated with this location.
    /// </summary>
    public List<Guid> CharacterIds { get; set; } = [];

    /// <summary>
    /// Gets or sets additional notes about this location.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets an image path/URL for the location.
    /// </summary>
    public string? ImagePath { get; set; }

    /// <summary>
    /// Gets or sets tags for categorization.
    /// </summary>
    public List<string> Tags { get; set; } = [];
}

/// <summary>
/// Types of locations in a story.
/// </summary>
public enum LocationType
{
    /// <summary>A city or town.</summary>
    City,
    /// <summary>A building or structure.</summary>
    Building,
    /// <summary>A natural landscape.</summary>
    Natural,
    /// <summary>A room or interior space.</summary>
    Room,
    /// <summary>A neighborhood or district.</summary>
    Neighborhood,
    /// <summary>A country or region.</summary>
    Region,
    /// <summary>A fictional/fantasy world.</summary>
    World,
    /// <summary>A vehicle or transportation.</summary>
    Vehicle,
    /// <summary>Other type of location.</summary>
    Other
}
