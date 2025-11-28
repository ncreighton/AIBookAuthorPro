// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

namespace AIBookAuthorPro.Core.Models.GuidedCreation;

/// <summary>
/// Result of AI-powered prompt analysis revealing all implicit and explicit elements.
/// This comprehensive analysis forms the foundation for blueprint generation.
/// </summary>
public sealed record PromptAnalysisResult
{
    /// <summary>
    /// Unique identifier for this analysis.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Reference to the original prompt.
    /// </summary>
    public required Guid SeedPromptId { get; init; }

    /// <summary>
    /// When this analysis was generated.
    /// </summary>
    public DateTime AnalyzedAt { get; init; } = DateTime.UtcNow;

    // ================== CORE ELEMENTS ==================

    /// <summary>
    /// The distilled core concept of the book.
    /// </summary>
    public required string CoreConcept { get; init; }

    /// <summary>
    /// One-sentence logline capturing the essence.
    /// </summary>
    public required string Logline { get; init; }

    /// <summary>
    /// Primary genre classification.
    /// </summary>
    public required string PrimaryGenre { get; init; }

    /// <summary>
    /// Secondary/sub-genre classifications.
    /// </summary>
    public List<string> SubGenres { get; init; } = new();

    /// <summary>
    /// Detailed target audience description.
    /// </summary>
    public required string TargetAudience { get; init; }

    /// <summary>
    /// Age range classification.
    /// </summary>
    public required AudienceAgeRange AgeRange { get; init; }

    // ================== NARRATIVE ELEMENTS ==================

    /// <summary>
    /// The central theme of the book.
    /// </summary>
    public required string CentralTheme { get; init; }

    /// <summary>
    /// Supporting/secondary themes.
    /// </summary>
    public List<string> SecondaryThemes { get; init; } = new();

    /// <summary>
    /// Overall tone description.
    /// </summary>
    public required string ToneDescriptor { get; init; }

    /// <summary>
    /// Suggested pacing style.
    /// </summary>
    public required NarrativePacing SuggestedPacing { get; init; }

    /// <summary>
    /// Identified narrative hooks.
    /// </summary>
    public List<string> NarrativeHooks { get; init; } = new();

    /// <summary>
    /// Potential conflict types identified.
    /// </summary>
    public List<ConflictElement> IdentifiedConflicts { get; init; } = new();

    // ================== STRUCTURAL RECOMMENDATIONS ==================

    /// <summary>
    /// Recommended book length category.
    /// </summary>
    public required BookLengthCategory RecommendedLength { get; init; }

    /// <summary>
    /// Estimated total word count.
    /// </summary>
    public required int EstimatedWordCount { get; init; }

    /// <summary>
    /// Recommended number of chapters.
    /// </summary>
    public required int RecommendedChapterCount { get; init; }

    /// <summary>
    /// Suggested structure template.
    /// </summary>
    public required StructureTemplate SuggestedStructure { get; init; }

    /// <summary>
    /// Explanation for structure recommendation.
    /// </summary>
    public required string StructureRationale { get; init; }

    /// <summary>
    /// Alternative structures that could work.
    /// </summary>
    public List<StructureAlternative> AlternativeStructures { get; init; } = new();

    // ================== CHARACTER EXTRACTION ==================

    /// <summary>
    /// Characters implied or mentioned in the prompt.
    /// </summary>
    public List<ExtractedCharacterSeed> ImpliedCharacters { get; init; } = new();

    /// <summary>
    /// Suggested character archetypes for the story.
    /// </summary>
    public List<SuggestedArchetype> SuggestedArchetypes { get; init; } = new();

    // ================== WORLD-BUILDING ELEMENTS ==================

    /// <summary>
    /// World-building requirements analysis.
    /// </summary>
    public required WorldBuildingRequirements WorldRequirements { get; init; }

    /// <summary>
    /// Key locations implied by the prompt.
    /// </summary>
    public List<ExtractedLocationSeed> ImpliedLocations { get; init; } = new();

    // ================== MARKET ANALYSIS ==================

    /// <summary>
    /// Market positioning analysis.
    /// </summary>
    public required MarketPositioning MarketAnalysis { get; init; }

    // ================== CONFIDENCE SCORES ==================

    /// <summary>
    /// Confidence levels for the analysis.
    /// </summary>
    public required AnalysisConfidence Confidence { get; init; }

    // ================== CLARIFICATIONS NEEDED ==================

    /// <summary>
    /// Questions that need user clarification.
    /// </summary>
    public List<ClarificationRequest> ClarificationsNeeded { get; init; } = new();

    // ================== AI SUGGESTIONS ==================

    /// <summary>
    /// AI-generated suggestions to enhance the concept.
    /// </summary>
    public List<EnhancementSuggestion> EnhancementSuggestions { get; init; } = new();

    /// <summary>
    /// Potential pitfalls or challenges identified.
    /// </summary>
    public List<IdentifiedChallenge> IdentifiedChallenges { get; init; } = new();
}

/// <summary>
/// Conflict element identified in the prompt.
/// </summary>
public sealed record ConflictElement
{
    /// <summary>
    /// Type of conflict (e.g., "Person vs Person", "Person vs Self").
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Description of the conflict.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Potential stakes involved.
    /// </summary>
    public required string Stakes { get; init; }

    /// <summary>
    /// How central this conflict is to the story (0-1).
    /// </summary>
    public double Centrality { get; init; }
}

/// <summary>
/// Alternative structure suggestion.
/// </summary>
public sealed record StructureAlternative
{
    /// <summary>
    /// The alternative structure template.
    /// </summary>
    public required StructureTemplate Template { get; init; }

    /// <summary>
    /// Why this could work for the story.
    /// </summary>
    public required string Rationale { get; init; }

    /// <summary>
    /// Suitability score (0-100).
    /// </summary>
    public int SuitabilityScore { get; init; }
}

/// <summary>
/// Character seed extracted from prompt analysis.
/// </summary>
public sealed record ExtractedCharacterSeed
{
    /// <summary>
    /// Suggested name or placeholder.
    /// </summary>
    public required string SuggestedName { get; init; }

    /// <summary>
    /// Identified role in the story.
    /// </summary>
    public required CharacterRole Role { get; init; }

    /// <summary>
    /// Brief description based on prompt.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Suggested archetype.
    /// </summary>
    public string? Archetype { get; init; }

    /// <summary>
    /// Key traits identified.
    /// </summary>
    public List<string> KeyTraits { get; init; } = new();

    /// <summary>
    /// Potential character arc direction.
    /// </summary>
    public string? PotentialArc { get; init; }

    /// <summary>
    /// Confidence in this extraction (0-1).
    /// </summary>
    public double Confidence { get; init; }
}

/// <summary>
/// Suggested archetype for the story.
/// </summary>
public sealed record SuggestedArchetype
{
    /// <summary>
    /// The archetype name.
    /// </summary>
    public required string ArchetypeName { get; init; }

    /// <summary>
    /// Why this archetype would fit.
    /// </summary>
    public required string Rationale { get; init; }

    /// <summary>
    /// Suggested role.
    /// </summary>
    public required CharacterRole SuggestedRole { get; init; }

    /// <summary>
    /// Examples from popular fiction.
    /// </summary>
    public List<string> Examples { get; init; } = new();
}

/// <summary>
/// World-building complexity requirements.
/// </summary>
public sealed record WorldBuildingRequirements
{
    /// <summary>
    /// Complexity level (1-10).
    /// </summary>
    public required int ComplexityLevel { get; init; }

    /// <summary>
    /// Setting type (e.g., "Contemporary", "Historical", "Fantasy").
    /// </summary>
    public required string SettingType { get; init; }

    /// <summary>
    /// Time period or era.
    /// </summary>
    public required string TimePeriod { get; init; }

    /// <summary>
    /// Whether a magic/power system is needed.
    /// </summary>
    public bool RequiresMagicSystem { get; init; }

    /// <summary>
    /// Whether detailed political structures are needed.
    /// </summary>
    public bool RequiresPoliticalSystem { get; init; }

    /// <summary>
    /// Whether detailed technology is important.
    /// </summary>
    public bool RequiresTechnologyDetails { get; init; }

    /// <summary>
    /// Cultural elements that need development.
    /// </summary>
    public List<string> CulturalElements { get; init; } = new();

    /// <summary>
    /// Key world rules that need establishment.
    /// </summary>
    public List<string> KeyWorldRules { get; init; } = new();

    /// <summary>
    /// Research areas that may be needed.
    /// </summary>
    public List<string> ResearchAreas { get; init; } = new();
}

/// <summary>
/// Location seed extracted from prompt.
/// </summary>
public sealed record ExtractedLocationSeed
{
    /// <summary>
    /// Suggested name for the location.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Type of location.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Description from prompt.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Significance to the story.
    /// </summary>
    public required string Significance { get; init; }

    /// <summary>
    /// Atmosphere/mood of the location.
    /// </summary>
    public string? Atmosphere { get; init; }
}

/// <summary>
/// Market positioning analysis.
/// </summary>
public sealed record MarketPositioning
{
    /// <summary>
    /// Comparable titles in the market.
    /// </summary>
    public List<ComparableTitle> ComparableTitles { get; init; } = new();

    /// <summary>
    /// Identified market trends this aligns with.
    /// </summary>
    public List<string> MarketTrends { get; init; } = new();

    /// <summary>
    /// Unique selling points.
    /// </summary>
    public List<string> UniqueSellingPoints { get; init; } = new();

    /// <summary>
    /// Potential challenges in the market.
    /// </summary>
    public List<string> MarketChallenges { get; init; } = new();

    /// <summary>
    /// Suggested positioning statement.
    /// </summary>
    public required string PositioningStatement { get; init; }
}

/// <summary>
/// Comparable title information.
/// </summary>
public sealed record ComparableTitle
{
    /// <summary>
    /// Title of the comparable work.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Author of the work.
    /// </summary>
    public required string Author { get; init; }

    /// <summary>
    /// Why it's comparable.
    /// </summary>
    public required string ComparisonReason { get; init; }

    /// <summary>
    /// What to learn from it.
    /// </summary>
    public string? LearningPoint { get; init; }
}

/// <summary>
/// Analysis confidence scores.
/// </summary>
public sealed record AnalysisConfidence
{
    /// <summary>
    /// Confidence in genre classification (0-1).
    /// </summary>
    public double GenreConfidence { get; init; }

    /// <summary>
    /// Confidence in theme identification (0-1).
    /// </summary>
    public double ThemeConfidence { get; init; }

    /// <summary>
    /// Confidence in structure recommendation (0-1).
    /// </summary>
    public double StructureConfidence { get; init; }

    /// <summary>
    /// Confidence in character extraction (0-1).
    /// </summary>
    public double CharacterConfidence { get; init; }

    /// <summary>
    /// Confidence in world-building assessment (0-1).
    /// </summary>
    public double WorldBuildingConfidence { get; init; }

    /// <summary>
    /// Overall confidence score (0-1).
    /// </summary>
    public double OverallConfidence { get; init; }

    /// <summary>
    /// Explanation of confidence levels.
    /// </summary>
    public required string ConfidenceExplanation { get; init; }

    /// <summary>
    /// Factors that reduced confidence.
    /// </summary>
    public List<string> ConfidenceReducers { get; init; } = new();
}

/// <summary>
/// Clarification request for ambiguous elements.
/// </summary>
public sealed record ClarificationRequest
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Category of the clarification.
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// The question to ask.
    /// </summary>
    public required string Question { get; init; }

    /// <summary>
    /// Priority level.
    /// </summary>
    public required ClarificationPriority Priority { get; init; }

    /// <summary>
    /// Why this clarification is needed.
    /// </summary>
    public required string Rationale { get; init; }

    /// <summary>
    /// Suggested options if applicable.
    /// </summary>
    public List<string> SuggestedOptions { get; init; } = new();

    /// <summary>
    /// Default answer if user skips.
    /// </summary>
    public string? DefaultAnswer { get; init; }

    /// <summary>
    /// User's response once answered.
    /// </summary>
    public string? UserResponse { get; set; }

    /// <summary>
    /// Whether this has been answered.
    /// </summary>
    public bool IsAnswered { get; set; }
}

/// <summary>
/// Suggestion to enhance the concept.
/// </summary>
public sealed record EnhancementSuggestion
{
    /// <summary>
    /// Category of enhancement.
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// The suggestion.
    /// </summary>
    public required string Suggestion { get; init; }

    /// <summary>
    /// Why this would improve the book.
    /// </summary>
    public required string Rationale { get; init; }

    /// <summary>
    /// Impact level (1-10).
    /// </summary>
    public int ImpactLevel { get; init; }

    /// <summary>
    /// Whether user accepted this suggestion.
    /// </summary>
    public bool? Accepted { get; set; }
}

/// <summary>
/// Challenge or pitfall identified.
/// </summary>
public sealed record IdentifiedChallenge
{
    /// <summary>
    /// The challenge description.
    /// </summary>
    public required string Challenge { get; init; }

    /// <summary>
    /// Category of challenge.
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// Potential mitigation strategies.
    /// </summary>
    public List<string> Mitigations { get; init; } = new();

    /// <summary>
    /// Severity level (1-10).
    /// </summary>
    public int SeverityLevel { get; init; }
}
