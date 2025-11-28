// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Enums;

namespace AIBookAuthorPro.Core.Models.Research;

/// <summary>
/// Represents a research query for gathering information.
/// </summary>
public sealed class ResearchQuery : Entity
{
    /// <summary>
    /// Gets or sets the query text.
    /// </summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of research being performed.
    /// </summary>
    public ResearchType Type { get; set; } = ResearchType.General;

    /// <summary>
    /// Gets or sets the time period context for historical research.
    /// </summary>
    public string? TimePeriod { get; set; }

    /// <summary>
    /// Gets or sets the geographic context.
    /// </summary>
    public string? GeographicContext { get; set; }

    /// <summary>
    /// Gets or sets the genre context for genre-specific research.
    /// </summary>
    public string? GenreContext { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of results to return.
    /// </summary>
    public int MaxResults { get; set; } = 10;

    /// <summary>
    /// Gets or sets whether to include AI-generated summaries.
    /// </summary>
    public bool IncludeAISummary { get; set; } = true;

    /// <summary>
    /// Gets or sets the project ID this research is associated with.
    /// </summary>
    public Guid? ProjectId { get; set; }
}

/// <summary>
/// Represents a research result from various sources.
/// </summary>
public sealed class ResearchResult : Entity
{
    /// <summary>
    /// Gets or sets the title of the result.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the content/snippet of the result.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source URL if applicable.
    /// </summary>
    public string? SourceUrl { get; set; }

    /// <summary>
    /// Gets or sets the source name (e.g., "Wikipedia", "Historical Archive").
    /// </summary>
    public string? SourceName { get; set; }

    /// <summary>
    /// Gets or sets the relevance score (0-1).
    /// </summary>
    public double RelevanceScore { get; set; }

    /// <summary>
    /// Gets or sets the AI-generated summary of this result.
    /// </summary>
    public string? AISummary { get; set; }

    /// <summary>
    /// Gets or sets the type of research this result belongs to.
    /// </summary>
    public ResearchType Type { get; set; }

    /// <summary>
    /// Gets or sets extracted key facts from this result.
    /// </summary>
    public List<string> KeyFacts { get; set; } = [];

    /// <summary>
    /// Gets or sets whether this result has been saved to the project.
    /// </summary>
    public bool IsSaved { get; set; }

    /// <summary>
    /// Gets or sets the date the information was retrieved.
    /// </summary>
    public DateTime RetrievedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Represents a collection of research results with metadata.
/// </summary>
public sealed class ResearchResultSet
{
    /// <summary>
    /// Gets or sets the original query.
    /// </summary>
    public ResearchQuery Query { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of results.
    /// </summary>
    public List<ResearchResult> Results { get; set; } = [];

    /// <summary>
    /// Gets or sets the AI-generated overall summary.
    /// </summary>
    public string? OverallSummary { get; set; }

    /// <summary>
    /// Gets or sets suggested follow-up queries.
    /// </summary>
    public List<string> SuggestedFollowUps { get; set; } = [];

    /// <summary>
    /// Gets or sets the total time taken for the research.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets any warnings or notes about the results.
    /// </summary>
    public List<string> Notes { get; set; } = [];
}

/// <summary>
/// Represents a generated character name suggestion.
/// </summary>
public sealed class NameSuggestion
{
    /// <summary>
    /// Gets or sets the suggested name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the origin/ethnicity of the name.
    /// </summary>
    public string? Origin { get; set; }

    /// <summary>
    /// Gets or sets the meaning of the name.
    /// </summary>
    public string? Meaning { get; set; }

    /// <summary>
    /// Gets or sets the gender association.
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// Gets or sets the time period the name is appropriate for.
    /// </summary>
    public string? TimePeriod { get; set; }

    /// <summary>
    /// Gets or sets popularity information.
    /// </summary>
    public string? Popularity { get; set; }

    /// <summary>
    /// Gets or sets notable characters/people with this name.
    /// </summary>
    public List<string> NotableExamples { get; set; } = [];
}

/// <summary>
/// Represents a genre trope or convention.
/// </summary>
public sealed class GenreTrope
{
    /// <summary>
    /// Gets or sets the name of the trope.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the trope.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets examples of this trope in popular works.
    /// </summary>
    public List<string> Examples { get; set; } = [];

    /// <summary>
    /// Gets or sets the genre this trope belongs to.
    /// </summary>
    public string Genre { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this is a subverted/deconstructed trope.
    /// </summary>
    public bool IsSubversion { get; set; }

    /// <summary>
    /// Gets or sets related tropes.
    /// </summary>
    public List<string> RelatedTropes { get; set; } = [];

    /// <summary>
    /// Gets or sets tips for using this trope effectively.
    /// </summary>
    public List<string> WritingTips { get; set; } = [];
}
