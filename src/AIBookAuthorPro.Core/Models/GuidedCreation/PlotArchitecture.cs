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
    public MainPlot MainPlot { get; init; } = new();

    /// <summary>
    /// Main conflict description (convenience property).
    /// </summary>
    public string MainConflict => MainPlot?.CentralConflict ?? string.Empty;

    /// <summary>
    /// All subplots.
    /// </summary>
    public List<Subplot> Subplots { get; init; } = new();

    /// <summary>
    /// Thematic structure.
    /// </summary>
    public ThematicStructure ThematicStructure { get; init; } = new();

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
    public SetupPayoffTracker SetupPayoffs { get; init; } = new();

    /// <summary>
    /// All plot threads.
    /// </summary>
    public List<PlotThread> PlotThreads { get; init; } = new();

    /// <summary>
    /// Tension map.
    /// </summary>
    public TensionMap TensionMap { get; init; } = new();
}

/// <summary>
/// Main plot definition.
/// </summary>
public sealed record MainPlot
{
    /// <summary>
    /// Plot type (e.g., "Quest", "Revenge", "Rags to Riches").
    /// </summary>
    public string PlotType { get; init; } = string.Empty;

    /// <summary>
    /// Central conflict.
    /// </summary>
    public string CentralConflict { get; init; } = string.Empty;

    /// <summary>
    /// Stakes description.
    /// </summary>
    public string Stakes { get; init; } = string.Empty;

    /// <summary>
    /// Stakes escalation through the story.
    /// </summary>
    public string StakesEscalation { get; init; } = string.Empty;

    /// <summary>
    /// The dramatic question.
    /// </summary>
    public string DramaticQuestion { get; init; } = string.Empty;

    /// <summary>
    /// Inciting incident.
    /// </summary>
    public PlotPoint IncitingIncident { get; init; } = new();

    /// <summary>
    /// First plot point / break into Act 2.
    /// </summary>
    public PlotPoint FirstPlotPoint { get; init; } = new();

    /// <summary>
    /// Midpoint.
    /// </summary>
    public PlotPoint Midpoint { get; init; } = new();

    /// <summary>
    /// Second plot point / break into Act 3.
    /// </summary>
    public PlotPoint SecondPlotPoint { get; init; } = new();

    /// <summary>
    /// All is lost / dark night.
    /// </summary>
    public PlotPoint DarkNight { get; init; } = new();

    /// <summary>
    /// Climax.
    /// </summary>
    public PlotPoint Climax { get; init; } = new();

    /// <summary>
    /// Resolution.
    /// </summary>
    public PlotPoint Resolution { get; init; } = new();

    /// <summary>
    /// All major plot beats.
    /// </summary>
    public List<PlotBeat> PlotBeats { get; init; } = new();

    /// <summary>
    /// Antagonistic force.
    /// </summary>
    public AntagonisticForce AntagonisticForce { get; init; } = new();
}

/// <summary>
/// Plot point definition.
/// </summary>
public sealed record PlotPoint
{
    /// <summary>
    /// Name of this plot point.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Chapter where this occurs.
    /// </summary>
    public int ChapterNumber { get; init; }

    /// <summary>
    /// Description of what happens.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Why this matters.
    /// </summary>
    public string Significance { get; init; } = string.Empty;

    /// <summary>
    /// Emotional impact.
    /// </summary>
    public string EmotionalImpact { get; init; } = string.Empty;

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
    public string Consequences { get; init; } = string.Empty;
}

/// <summary>
/// Plot beat.
/// </summary>
public sealed record PlotBeat
{
    /// <summary>
    /// Beat order.
    /// </summary>
    public int Order { get; init; }

    /// <summary>
    /// Beat name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Chapter number.
    /// </summary>
    public int ChapterNumber { get; init; }

    /// <summary>
    /// Percentage point in book.
    /// </summary>
    public double PercentagePoint { get; init; }

    /// <summary>
    /// Emotional impact.
    /// </summary>
    public string EmotionalImpact { get; init; } = string.Empty;

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
    public string BeatType { get; init; } = string.Empty;
}

/// <summary>
/// Antagonistic force.
/// </summary>
public sealed record AntagonisticForce
{
    /// <summary>
    /// Type (person, nature, society, self, supernatural).
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Character ID if applicable.
    /// </summary>
    public Guid? CharacterId { get; init; }

    /// <summary>
    /// Goals of the antagonistic force.
    /// </summary>
    public string Goals { get; init; } = string.Empty;

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
    public string Escalation { get; init; } = string.Empty;
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
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Type (romance, mystery, character development, etc.).
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Central conflict.
    /// </summary>
    public string Conflict { get; init; } = string.Empty;

    /// <summary>
    /// Stakes.
    /// </summary>
    public string Stakes { get; init; } = string.Empty;

    /// <summary>
    /// How it connects to main plot.
    /// </summary>
    public string MainPlotConnection { get; init; } = string.Empty;

    /// <summary>
    /// Characters involved.
    /// </summary>
    public List<Guid> InvolvedCharacters { get; init; } = new();

    /// <summary>
    /// Starting chapter.
    /// </summary>
    public int StartChapter { get; init; }

    /// <summary>
    /// Ending chapter.
    /// </summary>
    public int EndChapter { get; init; }

    /// <summary>
    /// Current status of the subplot.
    /// </summary>
    public SubplotStatus Status { get; init; } = SubplotStatus.Active;

    /// <summary>
    /// Key beats.
    /// </summary>
    public List<PlotBeat> KeyBeats { get; init; } = new();

    /// <summary>
    /// Resolution.
    /// </summary>
    public string Resolution { get; init; } = string.Empty;

    /// <summary>
    /// Thematic purpose.
    /// </summary>
    public string ThematicPurpose { get; init; } = string.Empty;
}

/// <summary>
/// Thematic structure.
/// </summary>
public sealed record ThematicStructure
{
    /// <summary>
    /// Central theme.
    /// </summary>
    public string CentralTheme { get; init; } = string.Empty;

    /// <summary>
    /// Theme statement (the argument the book makes).
    /// </summary>
    public string ThemeStatement { get; init; } = string.Empty;

    /// <summary>
    /// Counter-argument explored.
    /// </summary>
    public string CounterArgument { get; init; } = string.Empty;

    /// <summary>
    /// How the theme is proven.
    /// </summary>
    public string ThemeProof { get; init; } = string.Empty;

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
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Element.
    /// </summary>
    public string Element { get; init; } = string.Empty;

    /// <summary>
    /// Meaning.
    /// </summary>
    public string Meaning { get; init; } = string.Empty;

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
    public string ElementA { get; init; } = string.Empty;

    /// <summary>
    /// Second element.
    /// </summary>
    public string ElementB { get; init; } = string.Empty;

    /// <summary>
    /// How they contrast.
    /// </summary>
    public string Contrast { get; init; } = string.Empty;

    /// <summary>
    /// Thematic purpose.
    /// </summary>
    public string Purpose { get; init; } = string.Empty;
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
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Type (revelation, reversal, etc.).
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Chapter where revealed.
    /// </summary>
    public int RevealChapter { get; init; }

    /// <summary>
    /// The twist itself.
    /// </summary>
    public string Twist { get; init; } = string.Empty;

    /// <summary>
    /// Impact on story.
    /// </summary>
    public string Impact { get; init; } = string.Empty;

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
    public string EmotionalResponse { get; init; } = string.Empty;
}

/// <summary>
/// Twist setup.
/// </summary>
public sealed record TwistSetup
{
    /// <summary>
    /// Chapter.
    /// </summary>
    public int ChapterNumber { get; init; }

    /// <summary>
    /// The setup.
    /// </summary>
    public string Setup { get; init; } = string.Empty;

    /// <summary>
    /// How subtle it should be.
    /// </summary>
    public string Subtlety { get; init; } = string.Empty;
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
    public string Question { get; init; } = string.Empty;

    /// <summary>
    /// The answer.
    /// </summary>
    public string Answer { get; init; } = string.Empty;

    /// <summary>
    /// When introduced.
    /// </summary>
    public int IntroductionChapter { get; init; }

    /// <summary>
    /// When resolved.
    /// </summary>
    public int ResolutionChapter { get; init; }

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
    public string Clue { get; init; } = string.Empty;

    /// <summary>
    /// Chapter.
    /// </summary>
    public int ChapterNumber { get; init; }

    /// <summary>
    /// How obvious.
    /// </summary>
    public string Obviousness { get; init; } = string.Empty;

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
    /// Items in the tracker (for compatibility).
    /// </summary>
    public List<SetupPayoff> Items { get; init; } = new();

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
    public string Element { get; init; } = string.Empty;

    /// <summary>
    /// Description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Type of setup.
    /// </summary>
    public SetupPayoffType Type { get; init; } = SetupPayoffType.PlotDevice;

    /// <summary>
    /// Setup chapter.
    /// </summary>
    public int SetupChapter { get; init; }

    /// <summary>
    /// Setup description.
    /// </summary>
    public string SetupDescription { get; init; } = string.Empty;

    /// <summary>
    /// Payoff chapter.
    /// </summary>
    public int PayoffChapter { get; init; }

    /// <summary>
    /// Payoff description.
    /// </summary>
    public string PayoffDescription { get; init; } = string.Empty;

    /// <summary>
    /// Importance level.
    /// </summary>
    public int ImportanceLevel { get; init; }
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
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Thread type (main, subplot, character, mystery).
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Starting chapter.
    /// </summary>
    public int StartChapter { get; init; }

    /// <summary>
    /// Ending chapter.
    /// </summary>
    public int EndChapter { get; init; }

    /// <summary>
    /// Chapter appearances.
    /// </summary>
    public List<ThreadAppearance> Appearances { get; init; } = new();

    /// <summary>
    /// Resolution.
    /// </summary>
    public string Resolution { get; init; } = string.Empty;

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
    public int ChapterNumber { get; init; }

    /// <summary>
    /// Action (advance, pause, resolve).
    /// </summary>
    public string Action { get; init; } = string.Empty;

    /// <summary>
    /// Details.
    /// </summary>
    public string Details { get; init; } = string.Empty;
}

/// <summary>
/// Tension map.
/// </summary>
public sealed record TensionMap
{
    /// <summary>
    /// Description of tension strategy.
    /// </summary>
    public string Strategy { get; init; } = string.Empty;

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
    public int ChapterNumber { get; init; }

    /// <summary>
    /// Start level (1-10).
    /// </summary>
    public int StartLevel { get; init; }

    /// <summary>
    /// End level.
    /// </summary>
    public int EndLevel { get; init; }

    /// <summary>
    /// Peak level.
    /// </summary>
    public int PeakLevel { get; init; }

    /// <summary>
    /// Tension type.
    /// </summary>
    public string TensionType { get; init; } = string.Empty;
}

/// <summary>
/// Tension peak event.
/// </summary>
public sealed record TensionPeakEvent
{
    /// <summary>
    /// Chapter.
    /// </summary>
    public int ChapterNumber { get; init; }

    /// <summary>
    /// Event.
    /// </summary>
    public string Event { get; init; } = string.Empty;

    /// <summary>
    /// Tension level (1-10).
    /// </summary>
    public int Level { get; init; }

    /// <summary>
    /// Type of tension.
    /// </summary>
    public string Type { get; init; } = string.Empty;
}

/// <summary>
/// Recovery moment.
/// </summary>
public sealed record RecoveryMoment
{
    /// <summary>
    /// Chapter.
    /// </summary>
    public int ChapterNumber { get; init; }

    /// <summary>
    /// Purpose.
    /// </summary>
    public string Purpose { get; init; } = string.Empty;

    /// <summary>
    /// Content.
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// Duration (approximate word count).
    /// </summary>
    public int ApproximateWordCount { get; init; }
}
