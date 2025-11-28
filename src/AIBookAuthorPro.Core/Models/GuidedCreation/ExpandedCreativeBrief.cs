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
    public required Guid SeedPromptId { get; init; }

    /// <summary>
    /// Reference to the analysis result.
    /// </summary>
    public required Guid AnalysisResultId { get; init; }

    /// <summary>
    /// When this brief was created.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    // ================== BOOK IDENTITY ==================

    /// <summary>
    /// Working title for the book.
    /// </summary>
    public required string WorkingTitle { get; init; }

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
    public required string ExpandedPremise { get; init; }

    /// <summary>
    /// One-sentence logline.
    /// </summary>
    public required string Logline { get; init; }

    /// <summary>
    /// Elevator pitch (30 seconds).
    /// </summary>
    public required string ElevatorPitch { get; init; }

    /// <summary>
    /// Back cover blurb.
    /// </summary>
    public required string BackCoverBlurb { get; init; }

    /// <summary>
    /// Extended synopsis (1-2 pages).
    /// </summary>
    public required string ExtendedSynopsis { get; init; }

    // ================== GENRE & MARKET ==================

    /// <summary>
    /// Primary genre.
    /// </summary>
    public required string PrimaryGenre { get; init; }

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
    public required string TargetAudienceDescription { get; init; }

    /// <summary>
    /// Age range.
    /// </summary>
    public required AudienceAgeRange AgeRange { get; init; }

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
    public required ThemeDefinition CentralTheme { get; init; }

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
    public required string ToneDescription { get; init; }

    /// <summary>
    /// Mood keywords.
    /// </summary>
    public List<string> MoodKeywords { get; init; } = new();

    /// <summary>
    /// Prose style description.
    /// </summary>
    public required string ProseStyleDescription { get; init; }

    /// <summary>
    /// Narrative voice characteristics.
    /// </summary>
    public List<string> VoiceCharacteristics { get; init; } = new();

    /// <summary>
    /// Dialogue style notes.
    /// </summary>
    public required string DialogueStyle { get; init; }

    /// <summary>
    /// Pacing strategy.
    /// </summary>
    public required NarrativePacing PacingStrategy { get; init; }

    // ================== STRUCTURAL PLAN ==================

    /// <summary>
    /// Chosen structure template.
    /// </summary>
    public required StructureTemplate StructureTemplate { get; init; }

    /// <summary>
    /// Target word count.
    /// </summary>
    public required int TargetWordCount { get; init; }

    /// <summary>
    /// Planned chapter count.
    /// </summary>
    public required int PlannedChapterCount { get; init; }

    /// <summary>
    /// Average chapter length target.
    /// </summary>
    public required int AverageChapterLength { get; init; }

    /// <summary>
    /// Point of view.
    /// </summary>
    public required string PointOfView { get; init; }

    /// <summary>
    /// Tense (past/present).
    /// </summary>
    public required string Tense { get; init; }

    // ================== CONFLICT FRAMEWORK ==================

    /// <summary>
    /// Central conflict definition.
    /// </summary>
    public required ConflictFramework CentralConflict { get; init; }

    /// <summary>
    /// Secondary conflicts.
    /// </summary>
    public List<ConflictFramework> SecondaryConflicts { get; init; } = new();

    // ================== KEY STORY ELEMENTS ==================

    /// <summary>
    /// Opening hook concept.
    /// </summary>
    public required string OpeningHook { get; init; }

    /// <summary>
    /// Inciting incident description.
    /// </summary>
    public required string IncitingIncident { get; init; }

    /// <summary>
    /// Midpoint turn description.
    /// </summary>
    public required string MidpointTurn { get; init; }

    /// <summary>
    /// Dark night/low point description.
    /// </summary>
    public required string DarkNightOfSoul { get; init; }

    /// <summary>
    /// Climax description.
    /// </summary>
    public required string Climax { get; init; }

    /// <summary>
    /// Resolution description.
    /// </summary>
    public required string Resolution { get; init; }

    /// <summary>
    /// Ending type (happy, bittersweet, tragic, open).
    /// </summary>
    public required string EndingType { get; init; }

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
    public required string Theme { get; init; }

    /// <summary>
    /// How the theme should be explored.
    /// </summary>
    public required string Exploration { get; init; }

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
    public required string Motif { get; init; }

    /// <summary>
    /// What it represents.
    /// </summary>
    public required string Meaning { get; init; }

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
    public required string Symbol { get; init; }

    /// <summary>
    /// What it symbolizes.
    /// </summary>
    public required string Symbolism { get; init; }

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
    public required string Name { get; init; }

    /// <summary>
    /// Description of the trope.
    /// </summary>
    public required string Description { get; init; }

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
    public required string Type { get; init; }

    /// <summary>
    /// Description of the conflict.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// What's at stake.
    /// </summary>
    public required string Stakes { get; init; }

    /// <summary>
    /// Source of the conflict.
    /// </summary>
    public required string Source { get; init; }

    /// <summary>
    /// How it escalates.
    /// </summary>
    public required string Escalation { get; init; }

    /// <summary>
    /// How it resolves.
    /// </summary>
    public required string Resolution { get; init; }

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
    public required string Topic { get; init; }

    /// <summary>
    /// Why it's needed.
    /// </summary>
    public required string Reason { get; init; }

    /// <summary>
    /// Priority level.
    /// </summary>
    public required ClarificationPriority Priority { get; init; }

    /// <summary>
    /// Suggested resources.
    /// </summary>
    public List<string> SuggestedResources { get; init; } = new();

    /// <summary>
    /// Chapters where this applies.
    /// </summary>
    public List<int> RelevantChapters { get; init; } = new();
}
