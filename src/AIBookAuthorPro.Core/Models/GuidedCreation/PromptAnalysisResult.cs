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
    public Guid SeedPromptId { get; init; }

    /// <summary>
    /// Alias for SeedPromptId.
    /// </summary>
    public Guid PromptId { get => SeedPromptId; init => SeedPromptId = value; }

    /// <summary>
    /// When this analysis was generated.
    /// </summary>
    public DateTime AnalyzedAt { get; init; } = DateTime.UtcNow;

    // ================== CORE ELEMENTS ==================

    /// <summary>
    /// The distilled core concept of the book.
    /// </summary>
    public string CoreConcept { get; init; } = string.Empty;

    /// <summary>
    /// One-sentence logline capturing the essence.
    /// </summary>
    public string Logline { get; init; } = string.Empty;

    /// <summary>
    /// Primary genre classification.
    /// </summary>
    public string PrimaryGenre { get; init; } = string.Empty;

    /// <summary>
    /// Alias for PrimaryGenre.
    /// </summary>
    public string DetectedGenre { get => PrimaryGenre; init => PrimaryGenre = value; }

    /// <summary>
    /// Secondary/sub-genre classifications.
    /// </summary>
    public List<string> SubGenres { get; init; } = new();

    /// <summary>
    /// Detailed target audience description.
    /// </summary>
    public string TargetAudience { get; init; } = string.Empty;

    /// <summary>
    /// Age range classification.
    /// </summary>
    public AudienceAgeRange AgeRange { get; init; } = AudienceAgeRange.Adult;

    /// <summary>
    /// Alias for AgeRange.
    /// </summary>
    public AudienceAgeRange AudienceAgeRange { get => AgeRange; init => AgeRange = value; }

    // ================== NARRATIVE ELEMENTS ==================

    /// <summary>
    /// The central theme of the book.
    /// </summary>
    public string CentralTheme { get; init; } = string.Empty;

    /// <summary>
    /// Supporting/secondary themes.
    /// </summary>
    public List<string> SecondaryThemes { get; init; } = new();

    /// <summary>
    /// Alias for SecondaryThemes.
    /// </summary>
    public List<string> ExtractedThemes { get => SecondaryThemes; init => SecondaryThemes = value; }

    /// <summary>
    /// Overall tone description.
    /// </summary>
    public string ToneDescriptor { get; init; } = string.Empty;

    /// <summary>
    /// Suggested pacing style.
    /// </summary>
    public NarrativePacing SuggestedPacing { get; init; } = NarrativePacing.Moderate;

    /// <summary>
    /// Alias for SuggestedPacing.
    /// </summary>
    public NarrativePacing Pacing { get => SuggestedPacing; init => SuggestedPacing = value; }

    /// <summary>
    /// Identified narrative hooks.
    /// </summary>
    public List<string> NarrativeHooks { get; init; } = new();

    /// <summary>
    /// Potential conflict types identified.
    /// </summary>
    public List<ConflictElement> IdentifiedConflicts { get; init; } = new();

    /// <summary>
    /// Core conflict description.
    /// </summary>
    public string CoreConflict { get; init; } = string.Empty;

    // ================== STRUCTURAL RECOMMENDATIONS ==================

    /// <summary>
    /// Recommended book length category.
    /// </summary>
    public BookLengthCategory RecommendedLength { get; init; } = BookLengthCategory.Novella;

    /// <summary>
    /// Estimated total word count.
    /// </summary>
    public int EstimatedWordCount { get; init; }

    /// <summary>
    /// Recommended number of chapters.
    /// </summary>
    public int RecommendedChapterCount { get; init; }

    /// <summary>
    /// Suggested structure template.
    /// </summary>
    public StructureTemplate SuggestedStructure { get; init; } = StructureTemplate.ThreeAct;

    /// <summary>
    /// Explanation for structure recommendation.
    /// </summary>
    public string StructureRationale { get; init; } = string.Empty;

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
    public WorldBuildingRequirements WorldRequirements { get; init; } = new();

    /// <summary>
    /// Key locations implied by the prompt.
    /// </summary>
    public List<ExtractedLocationSeed> ImpliedLocations { get; init; } = new();

    // ================== MARKET ANALYSIS ==================

    /// <summary>
    /// Market positioning analysis.
    /// </summary>
    public MarketPositioning MarketAnalysis { get; init; } = new();

    // ================== CONFIDENCE SCORES ==================

    /// <summary>
    /// Confidence levels for the analysis.
    /// </summary>
    public AnalysisConfidence Confidence { get; init; } = new();

    /// <summary>
    /// Alias for Confidence.
    /// </summary>
    public AnalysisConfidence AnalysisConfidence { get => Confidence; init => Confidence = value; }

    // ================== CLARIFICATIONS NEEDED ==================

    /// <summary>
    /// Questions that need user clarification.
    /// </summary>
    public List<ClarificationRequest> ClarificationsNeeded { get; init; } = new();

    /// <summary>
    /// Alias for ClarificationsNeeded.
    /// </summary>
    public List<ClarificationRequest> ClarificationRequests { get => ClarificationsNeeded; init => ClarificationsNeeded = value; }

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
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Description of the conflict.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Potential stakes involved.
    /// </summary>
    public string Stakes { get; init; } = string.Empty;

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
    public StructureTemplate Template { get; init; } = StructureTemplate.ThreeAct;

    /// <summary>
    /// Why this could work for the story.
    /// </summary>
    public string Rationale { get; init; } = string.Empty;

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
    public string SuggestedName { get; init; } = string.Empty;

    /// <summary>
    /// Identified role in the story.
    /// </summary>
    public CharacterRole Role { get; init; } = CharacterRole.Supporting;

    /// <summary>
    /// Brief description based on prompt.
    /// </summary>
    public string Description { get; init; } = string.Empty;

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
    public string ArchetypeName { get; init; } = string.Empty;

    /// <summary>
    /// Why this archetype would fit.
    /// </summary>
    public string Rationale { get; init; } = string.Empty;

    /// <summary>
    /// Suggested role.
    /// </summary>
    public CharacterRole SuggestedRole { get; init; } = CharacterRole.Supporting;

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
    public int ComplexityLevel { get; init; }

    /// <summary>
    /// Setting type (e.g., "Contemporary", "Historical", "Fantasy").
    /// </summary>
    public string SettingType { get; init; } = string.Empty;

    /// <summary>
    /// Time period or era.
    /// </summary>
    public string TimePeriod { get; init; } = string.Empty;

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
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Type of location.
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Description from prompt.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Significance to the story.
    /// </summary>
    public string Significance { get; init; } = string.Empty;

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
    public string PositioningStatement { get; init; } = string.Empty;
}

/// <summary>
/// Comparable title information.
/// </summary>
public sealed record ComparableTitle
{
    /// <summary>
    /// Title of the comparable work.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Author of the work.
    /// </summary>
    public string Author { get; init; } = string.Empty;

    /// <summary>
    /// Why it's comparable.
    /// </summary>
    public string ComparisonReason { get; init; } = string.Empty;

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
    public string ConfidenceExplanation { get; init; } = string.Empty;

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
    public string Category { get; init; } = string.Empty;

    /// <summary>
    /// The question to ask.
    /// </summary>
    public string Question { get; init; } = string.Empty;

    /// <summary>
    /// Priority level.
    /// </summary>
    public ClarificationPriority Priority { get; init; } = ClarificationPriority.Important;

    /// <summary>
    /// Why this clarification is needed.
    /// </summary>
    public string Rationale { get; init; } = string.Empty;

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
    public string Category { get; init; } = string.Empty;

    /// <summary>
    /// The suggestion.
    /// </summary>
    public string Suggestion { get; init; } = string.Empty;

    /// <summary>
    /// Why this would improve the book.
    /// </summary>
    public string Rationale { get; init; } = string.Empty;

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
    public string Challenge { get; init; } = string.Empty;

    /// <summary>
    /// Category of challenge.
    /// </summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>
    /// Potential mitigation strategies.
    /// </summary>
    public List<string> Mitigations { get; init; } = new();

    /// <summary>
    /// Severity level (1-10).
    /// </summary>
    public int SeverityLevel { get; init; }
}
