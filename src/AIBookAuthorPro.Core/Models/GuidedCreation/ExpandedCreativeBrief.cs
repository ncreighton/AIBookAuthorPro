// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

namespace AIBookAuthorPro.Core.Models.GuidedCreation;

/// <summary>
/// Expanded creative brief generated from prompt analysis and user clarifications.
/// This serves as the comprehensive foundation for blueprint generation.
/// </summary>
public sealed record ExpandedCreativeBrief
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Reference to the seed prompt.
    /// </summary>
    public Guid SeedPromptId { get; init; }

    /// <summary>
    /// Alias for SeedPromptId.
    /// </summary>
    public Guid PromptId { get => SeedPromptId; init => SeedPromptId = value; }

    /// <summary>
    /// Reference to the analysis result.
    /// </summary>
    public Guid AnalysisResultId { get; init; }

    /// <summary>
    /// Alias for AnalysisResultId.
    /// </summary>
    public Guid AnalysisId { get => AnalysisResultId; init => AnalysisResultId = value; }

    /// <summary>
    /// When this brief was created.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    // ================== BOOK IDENTITY ==================

    /// <summary>
    /// Working title for the book.
    /// </summary>
    public string WorkingTitle { get; init; } = string.Empty;

    /// <summary>
    /// Alternative title options.
    /// </summary>
    public List<string> AlternativeTitles { get; init; } = new();

    /// <summary>
    /// Optional subtitle.
    /// </summary>
    public string? Subtitle { get; init; }

    /// <summary>
    /// Book series name if part of a series.
    /// </summary>
    public string? SeriesName { get; init; }

    /// <summary>
    /// Book number in series.
    /// </summary>
    public int? SeriesNumber { get; init; }

    // ================== PREMISE & PITCH ==================

    /// <summary>
    /// Expanded premise (1-2 paragraphs).
    /// </summary>
    public string ExpandedPremise { get; init; } = string.Empty;

    /// <summary>
    /// Alias for ExpandedPremise.
    /// </summary>
    public string Premise { get => ExpandedPremise; init => ExpandedPremise = value; }

    /// <summary>
    /// One-sentence logline.
    /// </summary>
    public string Logline { get; init; } = string.Empty;

    /// <summary>
    /// Elevator pitch (30 seconds).
    /// </summary>
    public string ElevatorPitch { get; init; } = string.Empty;

    /// <summary>
    /// Back cover blurb.
    /// </summary>
    public string BackCoverBlurb { get; init; } = string.Empty;

    /// <summary>
    /// Extended synopsis (1-2 pages).
    /// </summary>
    public string ExtendedSynopsis { get; init; } = string.Empty;

    // ================== GENRE & MARKET ==================

    /// <summary>
    /// Primary genre.
    /// </summary>
    public string PrimaryGenre { get; init; } = string.Empty;

    /// <summary>
    /// Sub-genres.
    /// </summary>
    public List<string> SubGenres { get; init; } = new();

    /// <summary>
    /// Genre-specific tropes to use.
    /// </summary>
    public List<GenreTrope> TropesToUse { get; init; } = new();

    /// <summary>
    /// Tropes to avoid or subvert.
    /// </summary>
    public List<GenreTrope> TropesToAvoid { get; init; } = new();

    /// <summary>
    /// Comparable titles.
    /// </summary>
    public List<string> ComparableTitles { get; init; } = new();

    /// <summary>
    /// Unique selling points.
    /// </summary>
    public List<string> UniqueSellingPoints { get; init; } = new();

    // ================== TARGET AUDIENCE ==================

    /// <summary>
    /// Detailed audience description.
    /// </summary>
    public string TargetAudienceDescription { get; init; } = string.Empty;

    /// <summary>
    /// Age range.
    /// </summary>
    public AudienceAgeRange AgeRange { get; init; } = AudienceAgeRange.Adult;

    /// <summary>
    /// Reader expectations for this genre/audience.
    /// </summary>
    public List<string> ReaderExpectations { get; init; } = new();

    /// <summary>
    /// Content warnings/triggers to note.
    /// </summary>
    public List<string> ContentWarnings { get; init; } = new();

    // ================== THEMES ==================

    /// <summary>
    /// Central theme with exploration notes.
    /// </summary>
    public ThemeDefinition CentralTheme { get; init; } = new();

    /// <summary>
    /// Secondary themes.
    /// </summary>
    public List<ThemeDefinition> SecondaryThemes { get; init; } = new();

    /// <summary>
    /// Motifs to weave throughout.
    /// </summary>
    public List<MotifDefinition> Motifs { get; init; } = new();

    /// <summary>
    /// Symbols to use.
    /// </summary>
    public List<SymbolDefinition> Symbols { get; init; } = new();

    // ================== TONE & STYLE ==================

    /// <summary>
    /// Overall tone description.
    /// </summary>
    public string ToneDescription { get; init; } = string.Empty;

    /// <summary>
    /// Mood keywords.
    /// </summary>
    public List<string> MoodKeywords { get; init; } = new();

    /// <summary>
    /// Prose style description.
    /// </summary>
    public string ProseStyleDescription { get; init; } = string.Empty;

    /// <summary>
    /// Narrative voice characteristics.
    /// </summary>
    public List<string> VoiceCharacteristics { get; init; } = new();

    /// <summary>
    /// Dialogue style notes.
    /// </summary>
    public string DialogueStyle { get; init; } = string.Empty;

    /// <summary>
    /// Pacing strategy.
    /// </summary>
    public NarrativePacing PacingStrategy { get; init; } = NarrativePacing.Moderate;

    // ================== STRUCTURAL PLAN ==================

    /// <summary>
    /// Chosen structure template.
    /// </summary>
    public StructureTemplate StructureTemplate { get; init; } = StructureTemplate.ThreeAct;

    /// <summary>
    /// Target word count.
    /// </summary>
    public int TargetWordCount { get; init; }

    /// <summary>
    /// Planned chapter count.
    /// </summary>
    public int PlannedChapterCount { get; init; }

    /// <summary>
    /// Average chapter length target.
    /// </summary>
    public int AverageChapterLength { get; init; }

    /// <summary>
    /// Point of view.
    /// </summary>
    public string PointOfView { get; init; } = string.Empty;

    /// <summary>
    /// Tense (past/present).
    /// </summary>
    public string Tense { get; init; } = string.Empty;

    // ================== CONFLICT FRAMEWORK ==================

    /// <summary>
    /// Central conflict definition.
    /// </summary>
    public ConflictFramework CentralConflict { get; init; } = new();

    /// <summary>
    /// Secondary conflicts.
    /// </summary>
    public List<ConflictFramework> SecondaryConflicts { get; init; } = new();

    // ================== KEY STORY ELEMENTS ==================

    /// <summary>
    /// Opening hook concept.
    /// </summary>
    public string OpeningHook { get; init; } = string.Empty;

    /// <summary>
    /// Inciting incident description.
    /// </summary>
    public string IncitingIncident { get; init; } = string.Empty;

    /// <summary>
    /// Midpoint turn description.
    /// </summary>
    public string MidpointTurn { get; init; } = string.Empty;

    /// <summary>
    /// Dark night/low point description.
    /// </summary>
    public string DarkNightOfSoul { get; init; } = string.Empty;

    /// <summary>
    /// Climax description.
    /// </summary>
    public string Climax { get; init; } = string.Empty;

    /// <summary>
    /// Resolution description.
    /// </summary>
    public string Resolution { get; init; } = string.Empty;

    /// <summary>
    /// Ending type (happy, bittersweet, tragic, open).
    /// </summary>
    public string EndingType { get; init; } = string.Empty;

    // ================== RESEARCH REQUIREMENTS ==================

    /// <summary>
    /// Research topics needed.
    /// </summary>
    public List<ResearchRequirement> ResearchRequirements { get; init; } = new();

    // ================== GENERATION PREFERENCES ==================

    /// <summary>
    /// Elements that must be included.
    /// </summary>
    public List<string> MustInclude { get; init; } = new();

    /// <summary>
    /// Elements that must be avoided.
    /// </summary>
    public List<string> MustAvoid { get; init; } = new();

    /// <summary>
    /// Special instructions for AI generation.
    /// </summary>
    public List<string> SpecialInstructions { get; init; } = new();
}

/// <summary>
/// Theme definition with exploration guidance.
/// </summary>
public sealed record ThemeDefinition
{
    /// <summary>
    /// Theme statement.
    /// </summary>
    public string Theme { get; init; } = string.Empty;

    /// <summary>
    /// How the theme should be explored.
    /// </summary>
    public string Exploration { get; init; } = string.Empty;

    /// <summary>
    /// Key scenes where theme appears.
    /// </summary>
    public List<string> KeyMoments { get; init; } = new();

    /// <summary>
    /// Character most connected to this theme.
    /// </summary>
    public string? PrimaryCharacter { get; init; }

    /// <summary>
    /// The question this theme asks.
    /// </summary>
    public string? ThematicQuestion { get; init; }

    /// <summary>
    /// The answer the book proposes.
    /// </summary>
    public string? ThematicAnswer { get; init; }
}

/// <summary>
/// Motif definition for recurring elements.
/// </summary>
public sealed record MotifDefinition
{
    /// <summary>
    /// The motif element.
    /// </summary>
    public string Motif { get; init; } = string.Empty;

    /// <summary>
    /// What it represents.
    /// </summary>
    public string Meaning { get; init; } = string.Empty;

    /// <summary>
    /// Where it should appear.
    /// </summary>
    public List<string> Occurrences { get; init; } = new();

    /// <summary>
    /// How it should evolve.
    /// </summary>
    public string? Evolution { get; init; }
}

/// <summary>
/// Symbol definition.
/// </summary>
public sealed record SymbolDefinition
{
    /// <summary>
    /// The symbol.
    /// </summary>
    public string Symbol { get; init; } = string.Empty;

    /// <summary>
    /// What it symbolizes.
    /// </summary>
    public string Symbolism { get; init; } = string.Empty;

    /// <summary>
    /// Key appearances.
    /// </summary>
    public List<string> Appearances { get; init; } = new();
}

/// <summary>
/// Genre trope with usage notes.
/// </summary>
public sealed record GenreTrope
{
    /// <summary>
    /// Trope name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Description of the trope.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// How to use or subvert it.
    /// </summary>
    public string? UsageNotes { get; init; }

    /// <summary>
    /// Whether to play straight, subvert, or lampshade.
    /// </summary>
    public string? Treatment { get; init; }
}

/// <summary>
/// Conflict framework definition.
/// </summary>
public sealed record ConflictFramework
{
    /// <summary>
    /// Type of conflict.
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Description of the conflict.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// What's at stake.
    /// </summary>
    public string Stakes { get; init; } = string.Empty;

    /// <summary>
    /// Source of the conflict.
    /// </summary>
    public string Source { get; init; } = string.Empty;

    /// <summary>
    /// How it escalates.
    /// </summary>
    public string Escalation { get; init; } = string.Empty;

    /// <summary>
    /// How it resolves.
    /// </summary>
    public string Resolution { get; init; } = string.Empty;

    /// <summary>
    /// Characters involved.
    /// </summary>
    public List<string> InvolvedCharacters { get; init; } = new();
}

/// <summary>
/// Research requirement.
/// </summary>
public sealed record ResearchRequirement
{
    /// <summary>
    /// Topic to research.
    /// </summary>
    public string Topic { get; init; } = string.Empty;

    /// <summary>
    /// Why it's needed.
    /// </summary>
    public string Reason { get; init; } = string.Empty;

    /// <summary>
    /// Priority level.
    /// </summary>
    public ClarificationPriority Priority { get; init; } = ClarificationPriority.Important;

    /// <summary>
    /// Suggested resources.
    /// </summary>
    public List<string> SuggestedResources { get; init; } = new();

    /// <summary>
    /// Chapters where this applies.
    /// </summary>
    public List<int> RelevantChapters { get; init; } = new();
}
