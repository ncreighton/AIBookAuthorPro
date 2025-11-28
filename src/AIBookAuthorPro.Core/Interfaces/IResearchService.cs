// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Enums;
using AIBookAuthorPro.Core.Models.Research;

namespace AIBookAuthorPro.Core.Interfaces;

/// <summary>
/// Service for performing research operations.
/// </summary>
public interface IResearchService
{
    /// <summary>
    /// Performs a research query and returns results.
    /// </summary>
    /// <param name="query">The research query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Research results.</returns>
    Task<Result<ResearchResultSet>> SearchAsync(ResearchQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates character name suggestions based on criteria.
    /// </summary>
    /// <param name="gender">Preferred gender (optional).</param>
    /// <param name="origin">Cultural/ethnic origin (optional).</param>
    /// <param name="timePeriod">Historical time period (optional).</param>
    /// <param name="count">Number of suggestions.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of name suggestions.</returns>
    Task<Result<List<NameSuggestion>>> GenerateCharacterNamesAsync(
        string? gender = null,
        string? origin = null,
        string? timePeriod = null,
        int count = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets genre tropes and conventions.
    /// </summary>
    /// <param name="genre">The genre to get tropes for.</param>
    /// <param name="includeSubversions">Include subverted/deconstructed tropes.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of genre tropes.</returns>
    Task<Result<List<GenreTrope>>> GetGenreTropesAsync(
        string genre,
        bool includeSubversions = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs historical accuracy check on text.
    /// </summary>
    /// <param name="text">The text to check.</param>
    /// <param name="timePeriod">The time period for accuracy.</param>
    /// <param name="location">The location for accuracy.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of potential anachronisms or inaccuracies.</returns>
    Task<Result<List<AccuracyIssue>>> CheckHistoricalAccuracyAsync(
        string text,
        string timePeriod,
        string? location = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets location research for a place.
    /// </summary>
    /// <param name="location">The location name.</param>
    /// <param name="aspectsToResearch">Specific aspects to focus on.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Location research result.</returns>
    Task<Result<LocationResearchResult>> ResearchLocationAsync(
        string location,
        List<string>? aspectsToResearch = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a research result to the project.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="result">The result to save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success or failure.</returns>
    Task<Result> SaveToProjectAsync(Guid projectId, ResearchResult result, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a historical accuracy issue found in text.
/// </summary>
public sealed class AccuracyIssue
{
    /// <summary>
    /// Gets or sets the problematic text.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the issue description.
    /// </summary>
    public string Issue { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the suggested correction.
    /// </summary>
    public string? Suggestion { get; set; }

    /// <summary>
    /// Gets or sets the severity (Low, Medium, High).
    /// </summary>
    public string Severity { get; set; } = "Medium";

    /// <summary>
    /// Gets or sets the category of issue.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the position in text.
    /// </summary>
    public int? StartIndex { get; set; }

    /// <summary>
    /// Gets or sets the length of problematic text.
    /// </summary>
    public int? Length { get; set; }
}

/// <summary>
/// Represents location research results.
/// </summary>
public sealed class LocationResearchResult
{
    /// <summary>
    /// Gets or sets the location name.
    /// </summary>
    public string LocationName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the general description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets geographical information.
    /// </summary>
    public GeographyInfo? Geography { get; set; }

    /// <summary>
    /// Gets or sets climate information.
    /// </summary>
    public ClimateInfo? Climate { get; set; }

    /// <summary>
    /// Gets or sets cultural information.
    /// </summary>
    public CultureInfo? Culture { get; set; }

    /// <summary>
    /// Gets or sets historical information.
    /// </summary>
    public List<HistoricalFact> History { get; set; } = [];

    /// <summary>
    /// Gets or sets notable landmarks.
    /// </summary>
    public List<string> Landmarks { get; set; } = [];

    /// <summary>
    /// Gets or sets sensory details for writing.
    /// </summary>
    public SensoryDetails? SensoryDetails { get; set; }
}

/// <summary>
/// Represents geography information.
/// </summary>
public sealed class GeographyInfo
{
    public string? Coordinates { get; set; }
    public string? Terrain { get; set; }
    public string? NearbyLocations { get; set; }
    public string? Population { get; set; }
}

/// <summary>
/// Represents climate information.
/// </summary>
public sealed class ClimateInfo
{
    public string? Type { get; set; }
    public string? Seasons { get; set; }
    public string? AverageTemperature { get; set; }
    public string? Weather { get; set; }
}

/// <summary>
/// Represents cultural information.
/// </summary>
public sealed class CultureInfo
{
    public string? Language { get; set; }
    public string? Religion { get; set; }
    public string? Customs { get; set; }
    public string? Food { get; set; }
    public string? Festivals { get; set; }
}

/// <summary>
/// Represents a historical fact.
/// </summary>
public sealed class HistoricalFact
{
    public string Period { get; set; } = string.Empty;
    public string Fact { get; set; } = string.Empty;
    public string? Source { get; set; }
}

/// <summary>
/// Represents sensory details for a location.
/// </summary>
public sealed class SensoryDetails
{
    public List<string> Sights { get; set; } = [];
    public List<string> Sounds { get; set; } = [];
    public List<string> Smells { get; set; } = [];
    public List<string> Textures { get; set; } = [];
    public List<string> Tastes { get; set; } = [];
    public string? Atmosphere { get; set; }
}
