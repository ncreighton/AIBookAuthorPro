// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

namespace AIBookAuthorPro.Core.Models.GuidedCreation;

/// <summary>
/// Complete plot architecture for the book.
/// </summary>
public sealed record PlotArchitecture
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Main plot definition.
    /// </summary>
    public required MainPlot MainPlot { get; init; }

    /// <summary>
    /// All subplots.
    /// </summary>
    public List<Subplot> Subplots { get; init; } = new();

    /// <summary>
    /// Thematic structure.
    /// </summary>
    public required ThematicStructure ThematicStructure { get; init; }

    /// <summary>
    /// Plot twists.
    /// </summary>
    public List<PlotTwist> PlotTwists { get; init; } = new();

    /// <summary>
    /// Mystery elements if applicable.
    /// </summary>
    public List<MysteryElement> Mysteries { get; init; } = new();

    /// <summary>
    /// Setup and payoff tracker.
    /// </summary>
    public required SetupPayoffTracker SetupPayoffs { get; init; }

    /// <summary>
    /// All plot threads.
    /// </summary>
    public List<PlotThread> PlotThreads { get; init; } = new();

    /// <summary>
    /// Tension map.
    /// </summary>
    public required TensionMap TensionMap { get; init; }
}

/// <summary>
/// Main plot definition.
/// </summary>
public sealed record MainPlot
{
    /// <summary>
    /// Plot type (e.g., "Quest", "Revenge", "Rags to Riches").
    /// </summary>
    public required string PlotType { get; init; }

    /// <summary>
    /// Central conflict.
    /// </summary>
    public required string CentralConflict { get; init; }

    /// <summary>
    /// Stakes description.
    /// </summary>
    public required string Stakes { get; init; }

    /// <summary>
    /// Stakes escalation through the story.
    /// </summary>
    public required string StakesEscalation { get; init; }

    /// <summary>
    /// The dramatic question.
    /// </summary>
    public required string DramaticQuestion { get; init; }

    /// <summary>
    /// Inciting incident.
    /// </summary>
    public required PlotPoint IncitingIncident { get; init; }

    /// <summary>
    /// First plot point / break into Act 2.
    /// </summary>
    public required PlotPoint FirstPlotPoint { get; init; }

    /// <summary>
    /// Midpoint.
    /// </summary>
    public required PlotPoint Midpoint { get; init; }

    /// <summary>
    /// Second plot point / break into Act 3.
    /// </summary>
    public required PlotPoint SecondPlotPoint { get; init; }

    /// <summary>
    /// All is lost / dark night.
    /// </summary>
    public required PlotPoint DarkNight { get; init; }

    /// <summary>
    /// Climax.
    /// </summary>
    public required PlotPoint Climax { get; init; }

    /// <summary>
    /// Resolution.
    /// </summary>
    public required PlotPoint Resolution { get; init; }

    /// <summary>
    /// All major plot beats.
    /// </summary>
    public List<PlotBeat> PlotBeats { get; init; } = new();

    /// <summary>
    /// Antagonistic force.
    /// </summary>
    public required AntagonisticForce AntagonisticForce { get; init; }
}

/// <summary>
/// Plot point definition.
/// </summary>
public sealed record PlotPoint
{
    /// <summary>
    /// Name of this plot point.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Chapter where this occurs.
    /// </summary>
    public required int ChapterNumber { get; init; }

    /// <summary>
    /// Description of what happens.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Why this matters.
    /// </summary>
    public required string Significance { get; init; }

    /// <summary>
    /// Emotional impact.
    /// </summary>
    public required string EmotionalImpact { get; init; }

    /// <summary>
    /// Characters involved.
    /// </summary>
    public List<Guid> InvolvedCharacters { get; init; } = new();

    /// <summary>
    /// Location.
    /// </summary>
    public Guid? LocationId { get; init; }

    /// <summary>
    /// What changes as a result.
    /// </summary>
    public required string Consequences { get; init; }
}

/// <summary>
/// Plot beat.
/// </summary>
public sealed record PlotBeat
{
    /// <summary>
    /// Beat order.
    /// </summary>
    public required int Order { get; init; }

    /// <summary>
    /// Beat name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Description.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Chapter number.
    /// </summary>
    public required int ChapterNumber { get; init; }

    /// <summary>
    /// Percentage point in book.
    /// </summary>
    public required double PercentagePoint { get; init; }

    /// <summary>
    /// Emotional impact.
    /// </summary>
    public required string EmotionalImpact { get; init; }

    /// <summary>
    /// Characters involved.
    /// </summary>
    public List<Guid> InvolvedCharacters { get; init; } = new();

    /// <summary>
    /// Whether this is a mandatory structural beat.
    /// </summary>
    public bool IsMandatory { get; init; }

    /// <summary>
    /// Beat type.
    /// </summary>
    public required string BeatType { get; init; }
}

/// <summary>
/// Antagonistic force.
/// </summary>
public sealed record AntagonisticForce
{
    /// <summary>
    /// Type (person, nature, society, self, supernatural).
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Description.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Character ID if applicable.
    /// </summary>
    public Guid? CharacterId { get; init; }

    /// <summary>
    /// Goals of the antagonistic force.
    /// </summary>
    public required string Goals { get; init; }

    /// <summary>
    /// Methods used.
    /// </summary>
    public List<string> Methods { get; init; } = new();

    /// <summary>
    /// Strengths.
    /// </summary>
    public List<string> Strengths { get; init; } = new();

    /// <summary>
    /// Weaknesses.
    /// </summary>
    public List<string> Weaknesses { get; init; } = new();

    /// <summary>
    /// How they escalate.
    /// </summary>
    public required string Escalation { get; init; }
}

/// <summary>
/// Subplot definition.
/// </summary>
public sealed record Subplot
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Subplot name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Type (romance, mystery, character development, etc.).
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Description.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Central conflict.
    /// </summary>
    public required string Conflict { get; init; }

    /// <summary>
    /// Stakes.
    /// </summary>
    public required string Stakes { get; init; }

    /// <summary>
    /// How it connects to main plot.
    /// </summary>
    public required string MainPlotConnection { get; init; }

    /// <summary>
    /// Characters involved.
    /// </summary>
    public List<Guid> InvolvedCharacters { get; init; } = new();

    /// <summary>
    /// Starting chapter.
    /// </summary>
    public required int StartChapter { get; init; }

    /// <summary>
    /// Ending chapter.
    /// </summary>
    public required int EndChapter { get; init; }

    /// <summary>
    /// Key beats.
    /// </summary>
    public List<PlotBeat> KeyBeats { get; init; } = new();

    /// <summary>
    /// Resolution.
    /// </summary>
    public required string Resolution { get; init; }

    /// <summary>
    /// Thematic purpose.
    /// </summary>
    public required string ThematicPurpose { get; init; }
}

/// <summary>
/// Thematic structure.
/// </summary>
public sealed record ThematicStructure
{
    /// <summary>
    /// Central theme.
    /// </summary>
    public required string CentralTheme { get; init; }

    /// <summary>
    /// Theme statement (the argument the book makes).
    /// </summary>
    public required string ThemeStatement { get; init; }

    /// <summary>
    /// Counter-argument explored.
    /// </summary>
    public required string CounterArgument { get; init; }

    /// <summary>
    /// How the theme is proven.
    /// </summary>
    public required string ThemeProof { get; init; }

    /// <summary>
    /// Thematic elements.
    /// </summary>
    public List<ThematicElement> Elements { get; init; } = new();

    /// <summary>
    /// Thematic mirrors (contrasting elements).
    /// </summary>
    public List<ThematicMirror> Mirrors { get; init; } = new();
}

/// <summary>
/// Thematic element.
/// </summary>
public sealed record ThematicElement
{
    /// <summary>
    /// Element type (motif, symbol, recurring image).
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Element.
    /// </summary>
    public required string Element { get; init; }

    /// <summary>
    /// Meaning.
    /// </summary>
    public required string Meaning { get; init; }

    /// <summary>
    /// Chapters where it appears.
    /// </summary>
    public List<int> Appearances { get; init; } = new();

    /// <summary>
    /// Evolution through story.
    /// </summary>
    public string? Evolution { get; init; }
}

/// <summary>
/// Thematic mirror.
/// </summary>
public sealed record ThematicMirror
{
    /// <summary>
    /// First element.
    /// </summary>
    public required string ElementA { get; init; }

    /// <summary>
    /// Second element.
    /// </summary>
    public required string ElementB { get; init; }

    /// <summary>
    /// How they contrast.
    /// </summary>
    public required string Contrast { get; init; }

    /// <summary>
    /// Thematic purpose.
    /// </summary>
    public required string Purpose { get; init; }
}

/// <summary>
/// Plot twist.
/// </summary>
public sealed record PlotTwist
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Twist name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Type (revelation, reversal, etc.).
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Chapter where revealed.
    /// </summary>
    public required int RevealChapter { get; init; }

    /// <summary>
    /// The twist itself.
    /// </summary>
    public required string Twist { get; init; }

    /// <summary>
    /// Impact on story.
    /// </summary>
    public required string Impact { get; init; }

    /// <summary>
    /// Setup chapters (foreshadowing).
    /// </summary>
    public List<TwistSetup> Setups { get; init; } = new();

    /// <summary>
    /// Red herrings.
    /// </summary>
    public List<string> RedHerrings { get; init; } = new();

    /// <summary>
    /// Emotional response expected.
    /// </summary>
    public required string EmotionalResponse { get; init; }
}

/// <summary>
/// Twist setup.
/// </summary>
public sealed record TwistSetup
{
    /// <summary>
    /// Chapter.
    /// </summary>
    public required int ChapterNumber { get; init; }

    /// <summary>
    /// The setup.
    /// </summary>
    public required string Setup { get; init; }

    /// <summary>
    /// How subtle it should be.
    /// </summary>
    public required string Subtlety { get; init; }
}

/// <summary>
/// Mystery element.
/// </summary>
public sealed record MysteryElement
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// The mystery question.
    /// </summary>
    public required string Question { get; init; }

    /// <summary>
    /// The answer.
    /// </summary>
    public required string Answer { get; init; }

    /// <summary>
    /// When introduced.
    /// </summary>
    public required int IntroductionChapter { get; init; }

    /// <summary>
    /// When resolved.
    /// </summary>
    public required int ResolutionChapter { get; init; }

    /// <summary>
    /// Clues planted.
    /// </summary>
    public List<MysteryClue> Clues { get; init; } = new();

    /// <summary>
    /// Red herrings.
    /// </summary>
    public List<MysteryClue> RedHerrings { get; init; } = new();
}

/// <summary>
/// Mystery clue.
/// </summary>
public sealed record MysteryClue
{
    /// <summary>
    /// The clue.
    /// </summary>
    public required string Clue { get; init; }

    /// <summary>
    /// Chapter.
    /// </summary>
    public required int ChapterNumber { get; init; }

    /// <summary>
    /// How obvious.
    /// </summary>
    public required string Obviousness { get; init; }

    /// <summary>
    /// Who discovers it.
    /// </summary>
    public List<Guid> DiscovererIds { get; init; } = new();
}

/// <summary>
/// Setup and payoff tracker.
/// </summary>
public sealed record SetupPayoffTracker
{
    /// <summary>
    /// All setup/payoff pairs.
    /// </summary>
    public List<SetupPayoffPair> Pairs { get; init; } = new();

    /// <summary>
    /// Orphaned setups (need payoff).
    /// </summary>
    public List<Guid> OrphanedSetups { get; init; } = new();

    /// <summary>
    /// Unearned payoffs (need setup).
    /// </summary>
    public List<Guid> UnearnedPayoffs { get; init; } = new();
}

/// <summary>
/// Setup and payoff pair.
/// </summary>
public sealed record SetupPayoffPair
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Element being set up.
    /// </summary>
    public required string Element { get; init; }

    /// <summary>
    /// Description.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Type of setup.
    /// </summary>
    public required SetupPayoffType Type { get; init; }

    /// <summary>
    /// Setup chapter.
    /// </summary>
    public required int SetupChapter { get; init; }

    /// <summary>
    /// Setup description.
    /// </summary>
    public required string SetupDescription { get; init; }

    /// <summary>
    /// Payoff chapter.
    /// </summary>
    public required int PayoffChapter { get; init; }

    /// <summary>
    /// Payoff description.
    /// </summary>
    public required string PayoffDescription { get; init; }

    /// <summary>
    /// Importance level.
    /// </summary>
    public required int ImportanceLevel { get; init; }
}

/// <summary>
/// Plot thread.
/// </summary>
public sealed record PlotThread
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Thread name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Description.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Thread type (main, subplot, character, mystery).
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Starting chapter.
    /// </summary>
    public required int StartChapter { get; init; }

    /// <summary>
    /// Ending chapter.
    /// </summary>
    public required int EndChapter { get; init; }

    /// <summary>
    /// Chapter appearances.
    /// </summary>
    public List<ThreadAppearance> Appearances { get; init; } = new();

    /// <summary>
    /// Resolution.
    /// </summary>
    public required string Resolution { get; init; }

    /// <summary>
    /// Related characters.
    /// </summary>
    public List<Guid> RelatedCharacters { get; init; } = new();
}

/// <summary>
/// Thread appearance in a chapter.
/// </summary>
public sealed record ThreadAppearance
{
    /// <summary>
    /// Chapter.
    /// </summary>
    public required int ChapterNumber { get; init; }

    /// <summary>
    /// Action (advance, pause, resolve).
    /// </summary>
    public required string Action { get; init; }

    /// <summary>
    /// Details.
    /// </summary>
    public required string Details { get; init; }
}

/// <summary>
/// Tension map.
/// </summary>
public sealed record TensionMap
{
    /// <summary>
    /// Description of tension strategy.
    /// </summary>
    public required string Strategy { get; init; }

    /// <summary>
    /// Tension levels by chapter.
    /// </summary>
    public List<ChapterTensionLevel> ChapterLevels { get; init; } = new();

    /// <summary>
    /// Tension peaks.
    /// </summary>
    public List<TensionPeakEvent> Peaks { get; init; } = new();

    /// <summary>
    /// Recovery moments.
    /// </summary>
    public List<RecoveryMoment> RecoveryMoments { get; init; } = new();
}

/// <summary>
/// Chapter tension level.
/// </summary>
public sealed record ChapterTensionLevel
{
    /// <summary>
    /// Chapter.
    /// </summary>
    public required int ChapterNumber { get; init; }

    /// <summary>
    /// Start level (1-10).
    /// </summary>
    public required int StartLevel { get; init; }

    /// <summary>
    /// End level.
    /// </summary>
    public required int EndLevel { get; init; }

    /// <summary>
    /// Peak level.
    /// </summary>
    public required int PeakLevel { get; init; }

    /// <summary>
    /// Tension type.
    /// </summary>
    public required string TensionType { get; init; }
}

/// <summary>
/// Tension peak event.
/// </summary>
public sealed record TensionPeakEvent
{
    /// <summary>
    /// Chapter.
    /// </summary>
    public required int ChapterNumber { get; init; }

    /// <summary>
    /// Event.
    /// </summary>
    public required string Event { get; init; }

    /// <summary>
    /// Tension level (1-10).
    /// </summary>
    public required int Level { get; init; }

    /// <summary>
    /// Type of tension.
    /// </summary>
    public required string Type { get; init; }
}

/// <summary>
/// Recovery moment.
/// </summary>
public sealed record RecoveryMoment
{
    /// <summary>
    /// Chapter.
    /// </summary>
    public required int ChapterNumber { get; init; }

    /// <summary>
    /// Purpose.
    /// </summary>
    public required string Purpose { get; init; }

    /// <summary>
    /// Content.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Duration (approximate word count).
    /// </summary>
    public required int ApproximateWordCount { get; init; }
}
