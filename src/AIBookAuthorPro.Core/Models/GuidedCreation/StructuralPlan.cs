// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

namespace AIBookAuthorPro.Core.Models.GuidedCreation;

/// <summary>
/// Complete structural plan for the book including acts, chapters, and scenes.
/// </summary>
public sealed record StructuralPlan
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Structure template being used.
    /// </summary>
    public StructureTemplate Template { get; init; } = StructureTemplate.ThreeAct;

    /// <summary>
    /// Total word count target.
    /// </summary>
    public int TotalWordCountTarget { get; init; }

    /// <summary>
    /// Alias for TotalWordCountTarget.
    /// </summary>
    public int TotalTargetWordCount { get => TotalWordCountTarget; init => TotalWordCountTarget = value; }

    /// <summary>
    /// Total chapter count.
    /// </summary>
    public int ChapterCount { get; init; }

    /// <summary>
    /// Point of view configuration.
    /// </summary>
    public string PointOfView { get; init; } = "Third Person Limited";

    /// <summary>
    /// Multiple POV characters if applicable.
    /// </summary>
    public List<Guid> PovCharacterIds { get; init; } = new();

    /// <summary>
    /// Tense (past/present).
    /// </summary>
    public string Tense { get; init; } = "Past";

    /// <summary>
    /// Act definitions.
    /// </summary>
    public List<ActDefinition> Acts { get; init; } = new();

    /// <summary>
    /// All chapter blueprints.
    /// </summary>
    public List<ChapterBlueprint> Chapters { get; init; } = new();

    /// <summary>
    /// Pacing strategy map.
    /// </summary>
    public PacingMap PacingStrategy { get; init; } = new();

    /// <summary>
    /// Alias for PacingStrategy.
    /// </summary>
    public PacingMap Pacing { get => PacingStrategy; init => PacingStrategy = value; }

    /// <summary>
    /// Another alias for PacingStrategy.
    /// </summary>
    public PacingMap PacingMap { get => PacingStrategy; init => PacingStrategy = value; }

    /// <summary>
    /// Prologue if planned.
    /// </summary>
    public ChapterBlueprint? Prologue { get; init; }

    /// <summary>
    /// Epilogue if planned.
    /// </summary>
    public ChapterBlueprint? Epilogue { get; init; }

    /// <summary>
    /// Interlude chapters if any.
    /// </summary>
    public List<ChapterBlueprint> Interludes { get; init; } = new();
}

/// <summary>
/// Definition of a story act.
/// </summary>
public sealed record ActDefinition
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Act number.
    /// </summary>
    public int ActNumber { get; init; }

    /// <summary>
    /// Act name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Purpose of this act.
    /// </summary>
    public string Purpose { get; init; } = string.Empty;

    /// <summary>
    /// Percentage of total book.
    /// </summary>
    public double PercentageOfBook { get; init; }

    /// <summary>
    /// Target word count for this act.
    /// </summary>
    public int TargetWordCount { get; init; }

    /// <summary>
    /// First chapter number in this act.
    /// </summary>
    public int StartChapter { get; init; }

    /// <summary>
    /// Last chapter number in this act.
    /// </summary>
    public int EndChapter { get; init; }

    /// <summary>
    /// Chapter definitions in this act.
    /// </summary>
    public List<ChapterDefinition> Chapters { get; init; } = new();

    /// <summary>
    /// Description of emotional arc.
    /// </summary>
    public string EmotionalArc { get; init; } = string.Empty;

    /// <summary>
    /// Starting emotional state.
    /// </summary>
    public string EmotionalStart { get; init; } = string.Empty;

    /// <summary>
    /// Ending emotional state.
    /// </summary>
    public string EmotionalEnd { get; init; } = string.Empty;

    /// <summary>
    /// Key story beats in this act.
    /// </summary>
    public List<StoryBeat> KeyBeats { get; init; } = new();

    /// <summary>
    /// Major turning point in/ending this act.
    /// </summary>
    public string TurningPoint { get; init; } = string.Empty;

    /// <summary>
    /// Primary tension/conflict driver.
    /// </summary>
    public string PrimaryTension { get; init; } = string.Empty;
}

/// <summary>
/// Story beat definition.
/// </summary>
public sealed record StoryBeat
{
    /// <summary>
    /// Beat name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Beat description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Chapter where this beat occurs.
    /// </summary>
    public int ChapterNumber { get; init; }

    /// <summary>
    /// Approximate percentage point in the book.
    /// </summary>
    public double PercentagePoint { get; init; }

    /// <summary>
    /// Emotional impact expected.
    /// </summary>
    public string EmotionalImpact { get; init; } = string.Empty;

    /// <summary>
    /// Is this a mandatory structural beat?
    /// </summary>
    public bool IsMandatory { get; init; }
}

/// <summary>
/// Pacing map showing intensity throughout the book.
/// </summary>
public sealed record PacingMap
{
    /// <summary>
    /// Overall pacing style.
    /// </summary>
    public NarrativePacing OverallPacing { get; init; } = NarrativePacing.Moderate;

    /// <summary>
    /// Description of pacing strategy.
    /// </summary>
    public string Strategy { get; init; } = string.Empty;

    /// <summary>
    /// Alias for Strategy.
    /// </summary>
    public string Description { get => Strategy; init => Strategy = value; }

    /// <summary>
    /// Pacing points throughout the book.
    /// </summary>
    public List<PacingPoint> PacingPoints { get; init; } = new();

    /// <summary>
    /// Key tension peaks.
    /// </summary>
    public List<TensionPeak> TensionPeaks { get; init; } = new();

    /// <summary>
    /// Planned breather moments.
    /// </summary>
    public List<BreatherMoment> BreatherMoments { get; init; } = new();
}

/// <summary>
/// A point on the pacing curve.
/// </summary>
public sealed record PacingPoint
{
    /// <summary>
    /// Chapter number.
    /// </summary>
    public int ChapterNumber { get; init; }

    /// <summary>
    /// Intensity level (1-10).
    /// </summary>
    public int Intensity { get; init; }

    /// <summary>
    /// Description of pacing at this point.
    /// </summary>
    public string Description { get; init; } = string.Empty;
}

/// <summary>
/// A peak tension moment.
/// </summary>
public sealed record TensionPeak
{
    /// <summary>
    /// Chapter number.
    /// </summary>
    public int ChapterNumber { get; init; }

    /// <summary>
    /// Scene within chapter.
    /// </summary>
    public int? SceneNumber { get; init; }

    /// <summary>
    /// Type of peak (action, emotional, revelation).
    /// </summary>
    public string PeakType { get; init; } = string.Empty;

    /// <summary>
    /// Description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Intensity (1-10).
    /// </summary>
    public int Intensity { get; init; }
}

/// <summary>
/// A planned breather/recovery moment.
/// </summary>
public sealed record BreatherMoment
{
    /// <summary>
    /// Chapter number.
    /// </summary>
    public int ChapterNumber { get; init; }

    /// <summary>
    /// Purpose of this breather.
    /// </summary>
    public string Purpose { get; init; } = string.Empty;

    /// <summary>
    /// What happens during this moment.
    /// </summary>
    public string Content { get; init; } = string.Empty;
}
