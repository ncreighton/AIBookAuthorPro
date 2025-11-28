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
    public required StructureTemplate Template { get; init; }

    /// <summary>
    /// Total word count target.
    /// </summary>
    public required int TotalWordCountTarget { get; init; }

    /// <summary>
    /// Total chapter count.
    /// </summary>
    public required int ChapterCount { get; init; }

    /// <summary>
    /// Point of view configuration.
    /// </summary>
    public required string PointOfView { get; init; }

    /// <summary>
    /// Multiple POV characters if applicable.
    /// </summary>
    public List<Guid> PovCharacterIds { get; init; } = new();

    /// <summary>
    /// Tense (past/present).
    /// </summary>
    public required string Tense { get; init; }

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
    public required PacingMap PacingStrategy { get; init; }

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
    public required int ActNumber { get; init; }

    /// <summary>
    /// Act name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Purpose of this act.
    /// </summary>
    public required string Purpose { get; init; }

    /// <summary>
    /// Percentage of total book.
    /// </summary>
    public required double PercentageOfBook { get; init; }

    /// <summary>
    /// Target word count for this act.
    /// </summary>
    public required int TargetWordCount { get; init; }

    /// <summary>
    /// First chapter number in this act.
    /// </summary>
    public required int StartChapter { get; init; }

    /// <summary>
    /// Last chapter number in this act.
    /// </summary>
    public required int EndChapter { get; init; }

    /// <summary>
    /// Description of emotional arc.
    /// </summary>
    public required string EmotionalArc { get; init; }

    /// <summary>
    /// Starting emotional state.
    /// </summary>
    public required string EmotionalStart { get; init; }

    /// <summary>
    /// Ending emotional state.
    /// </summary>
    public required string EmotionalEnd { get; init; }

    /// <summary>
    /// Key story beats in this act.
    /// </summary>
    public List<StoryBeat> KeyBeats { get; init; } = new();

    /// <summary>
    /// Major turning point in/ending this act.
    /// </summary>
    public required string TurningPoint { get; init; }

    /// <summary>
    /// Primary tension/conflict driver.
    /// </summary>
    public required string PrimaryTension { get; init; }
}

/// <summary>
/// Story beat definition.
/// </summary>
public sealed record StoryBeat
{
    /// <summary>
    /// Beat name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Beat description.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Chapter where this beat occurs.
    /// </summary>
    public required int ChapterNumber { get; init; }

    /// <summary>
    /// Approximate percentage point in the book.
    /// </summary>
    public required double PercentagePoint { get; init; }

    /// <summary>
    /// Emotional impact expected.
    /// </summary>
    public required string EmotionalImpact { get; init; }

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
    public required NarrativePacing OverallPacing { get; init; }

    /// <summary>
    /// Description of pacing strategy.
    /// </summary>
    public required string Strategy { get; init; }

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
    public required int ChapterNumber { get; init; }

    /// <summary>
    /// Intensity level (1-10).
    /// </summary>
    public required int Intensity { get; init; }

    /// <summary>
    /// Description of pacing at this point.
    /// </summary>
    public required string Description { get; init; }
}

/// <summary>
/// A peak tension moment.
/// </summary>
public sealed record TensionPeak
{
    /// <summary>
    /// Chapter number.
    /// </summary>
    public required int ChapterNumber { get; init; }

    /// <summary>
    /// Scene within chapter.
    /// </summary>
    public int? SceneNumber { get; init; }

    /// <summary>
    /// Type of peak (action, emotional, revelation).
    /// </summary>
    public required string PeakType { get; init; }

    /// <summary>
    /// Description.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Intensity (1-10).
    /// </summary>
    public required int Intensity { get; init; }
}

/// <summary>
/// A planned breather/recovery moment.
/// </summary>
public sealed record BreatherMoment
{
    /// <summary>
    /// Chapter number.
    /// </summary>
    public required int ChapterNumber { get; init; }

    /// <summary>
    /// Purpose of this breather.
    /// </summary>
    public required string Purpose { get; init; }

    /// <summary>
    /// What happens during this moment.
    /// </summary>
    public required string Content { get; init; }
}
