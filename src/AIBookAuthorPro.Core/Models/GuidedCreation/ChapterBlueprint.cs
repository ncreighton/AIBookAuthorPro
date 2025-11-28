// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

namespace AIBookAuthorPro.Core.Models.GuidedCreation;

/// <summary>
/// Detailed blueprint for a single chapter including all generation parameters.
/// This is the definitive guide for generating chapter content.
/// </summary>
public sealed class ChapterBlueprint
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Chapter number in the book (0 for prologue, -1 for epilogue).
    /// </summary>
    public required int ChapterNumber { get; init; }

    /// <summary>
    /// Display number (may differ from internal number).
    /// </summary>
    public string DisplayNumber => ChapterNumber switch
    {
        0 => "Prologue",
        -1 => "Epilogue",
        < 0 => $"Interlude {Math.Abs(ChapterNumber)}",
        _ => $"Chapter {ChapterNumber}"
    };

    /// <summary>
    /// Chapter title.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Alternative titles considered.
    /// </summary>
    public List<string> AlternativeTitles { get; init; } = new();

    /// <summary>
    /// Act this chapter belongs to.
    /// </summary>
    public required int ActNumber { get; init; }

    // ================== CONTENT SPECIFICATION ==================

    /// <summary>
    /// Target word count.
    /// </summary>
    public required int TargetWordCount { get; init; }

    /// <summary>
    /// Minimum acceptable word count.
    /// </summary>
    public int MinWordCount => (int)(TargetWordCount * 0.85);

    /// <summary>
    /// Maximum acceptable word count.
    /// </summary>
    public int MaxWordCount => (int)(TargetWordCount * 1.15);

    /// <summary>
    /// Primary purpose of this chapter.
    /// </summary>
    public required string Purpose { get; init; }

    /// <summary>
    /// Detailed chapter summary (what happens).
    /// </summary>
    public required string Summary { get; init; }

    /// <summary>
    /// Opening hook - how the chapter should begin.
    /// </summary>
    public required string OpeningHook { get; init; }

    /// <summary>
    /// Closing hook - how the chapter should end.
    /// </summary>
    public required string ClosingHook { get; init; }

    /// <summary>
    /// Key events that must occur.
    /// </summary>
    public List<string> KeyEvents { get; init; } = new();

    // ================== SCENE BREAKDOWN ==================

    /// <summary>
    /// Scene-by-scene breakdown.
    /// </summary>
    public List<SceneBlueprint> Scenes { get; init; } = new();

    // ================== NARRATIVE ELEMENTS ==================

    /// <summary>
    /// Point of view for this chapter.
    /// </summary>
    public required string POV { get; init; }

    /// <summary>
    /// POV character ID if character-based POV.
    /// </summary>
    public Guid? POVCharacterId { get; init; }

    /// <summary>
    /// Chapter tone.
    /// </summary>
    public required ChapterTone Tone { get; init; }

    /// <summary>
    /// Pacing intensity.
    /// </summary>
    public required PacingIntensity Pacing { get; init; }

    /// <summary>
    /// Emotional journey of the chapter.
    /// </summary>
    public required EmotionalJourney EmotionalArc { get; init; }

    // ================== PLOT ELEMENTS ==================

    /// <summary>
    /// Plot threads active in this chapter.
    /// </summary>
    public List<PlotThreadReference> PlotThreads { get; init; } = new();

    /// <summary>
    /// Plot beats that occur.
    /// </summary>
    public List<string> PlotBeats { get; init; } = new();

    /// <summary>
    /// Revelations/discoveries in this chapter.
    /// </summary>
    public List<Revelation> Revelations { get; init; } = new();

    /// <summary>
    /// Cliffhanger details if chapter ends on one.
    /// </summary>
    public CliffhangerDetails? Cliffhanger { get; init; }

    // ================== CHARACTER INVOLVEMENT ==================

    /// <summary>
    /// Characters appearing in this chapter.
    /// </summary>
    public List<CharacterAppearance> CharacterAppearances { get; init; } = new();

    /// <summary>
    /// Character development beats.
    /// </summary>
    public List<CharacterDevelopmentBeat> CharacterBeats { get; init; } = new();

    /// <summary>
    /// Key character interactions.
    /// </summary>
    public List<CharacterInteraction> KeyInteractions { get; init; } = new();

    // ================== SETTING ==================

    /// <summary>
    /// Location IDs used in this chapter.
    /// </summary>
    public List<Guid> LocationIds { get; init; } = new();

    /// <summary>
    /// Timeline position.
    /// </summary>
    public required TimelinePosition Timeline { get; init; }

    /// <summary>
    /// Time of day/atmosphere.
    /// </summary>
    public required string TimeOfDay { get; init; }

    /// <summary>
    /// Weather/environmental conditions if relevant.
    /// </summary>
    public string? EnvironmentalConditions { get; init; }

    // ================== CONTINUITY ==================

    /// <summary>
    /// Continuity requirements from previous chapters.
    /// </summary>
    public List<ContinuityRequirement> ContinuityRequirements { get; init; } = new();

    /// <summary>
    /// Elements being set up for later payoff.
    /// </summary>
    public List<SetupElement> SetupElements { get; init; } = new();

    /// <summary>
    /// Elements being paid off from earlier setup.
    /// </summary>
    public List<PayoffElement> PayoffElements { get; init; } = new();

    // ================== THEMATIC ELEMENTS ==================

    /// <summary>
    /// Themes explored in this chapter.
    /// </summary>
    public List<ThemeReference> ThemeReferences { get; init; } = new();

    /// <summary>
    /// Motifs that should appear.
    /// </summary>
    public List<string> Motifs { get; init; } = new();

    /// <summary>
    /// Symbols used.
    /// </summary>
    public List<string> Symbols { get; init; } = new();

    // ================== DIALOGUE GUIDANCE ==================

    /// <summary>
    /// Key dialogue beats that must occur.
    /// </summary>
    public List<DialogueBeat> KeyDialogue { get; init; } = new();

    /// <summary>
    /// Information that must be conveyed through dialogue.
    /// </summary>
    public List<string> ExpositionThroughDialogue { get; init; } = new();

    // ================== SPECIAL INSTRUCTIONS ==================

    /// <summary>
    /// Elements that must be included.
    /// </summary>
    public List<string> MustInclude { get; init; } = new();

    /// <summary>
    /// Elements that must be avoided.
    /// </summary>
    public List<string> MustAvoid { get; init; } = new();

    /// <summary>
    /// Special writing instructions for this chapter.
    /// </summary>
    public List<string> SpecialInstructions { get; init; } = new();

    /// <summary>
    /// Style notes specific to this chapter.
    /// </summary>
    public string? StyleNotes { get; init; }

    // ================== GENERATION STATUS ==================

    /// <summary>
    /// Current generation status.
    /// </summary>
    public ChapterGenerationStatus GenerationStatus { get; set; } = ChapterGenerationStatus.Pending;

    /// <summary>
    /// History of generation attempts.
    /// </summary>
    public List<GenerationAttempt> GenerationHistory { get; init; } = new();

    /// <summary>
    /// User notes on this chapter.
    /// </summary>
    public List<string> UserNotes { get; init; } = new();

    /// <summary>
    /// Locked - cannot be regenerated without explicit unlock.
    /// </summary>
    public bool IsLocked { get; set; }
}

/// <summary>
/// Detailed blueprint for a single scene within a chapter.
/// </summary>
public sealed record SceneBlueprint
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Scene number within chapter.
    /// </summary>
    public required int SceneNumber { get; init; }

    /// <summary>
    /// Scene title/label.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Purpose of this scene.
    /// </summary>
    public required string Purpose { get; init; }

    /// <summary>
    /// Scene summary.
    /// </summary>
    public required string Summary { get; init; }

    /// <summary>
    /// Target word count.
    /// </summary>
    public required int TargetWordCount { get; init; }

    /// <summary>
    /// Scene type.
    /// </summary>
    public required SceneType Type { get; init; }

    /// <summary>
    /// Location ID.
    /// </summary>
    public required Guid LocationId { get; init; }

    /// <summary>
    /// Character IDs present in scene.
    /// </summary>
    public List<Guid> CharacterIds { get; init; } = new();

    /// <summary>
    /// POV character for this scene (if different from chapter).
    /// </summary>
    public Guid? ScenePOVCharacterId { get; init; }

    /// <summary>
    /// Conflict in this scene.
    /// </summary>
    public required string Conflict { get; init; }

    /// <summary>
    /// Resolution of scene conflict.
    /// </summary>
    public required string Resolution { get; init; }

    /// <summary>
    /// Sensory details to include.
    /// </summary>
    public List<SensoryDetail> SensoryDetails { get; init; } = new();

    /// <summary>
    /// Key dialogue beats.
    /// </summary>
    public List<DialogueBeat> KeyDialogue { get; init; } = new();

    /// <summary>
    /// Scene transition type.
    /// </summary>
    public required string TransitionIn { get; init; }

    /// <summary>
    /// Transition out of scene.
    /// </summary>
    public required string TransitionOut { get; init; }

    /// <summary>
    /// Pacing for this scene.
    /// </summary>
    public required PacingIntensity Pacing { get; init; }

    /// <summary>
    /// Emotional tone.
    /// </summary>
    public required ChapterTone Tone { get; init; }

    /// <summary>
    /// Special instructions for this scene.
    /// </summary>
    public List<string> SpecialInstructions { get; init; } = new();
}

/// <summary>
/// Sensory detail to include.
/// </summary>
public sealed record SensoryDetail
{
    /// <summary>
    /// Sense (sight, sound, smell, taste, touch).
    /// </summary>
    public required string Sense { get; init; }

    /// <summary>
    /// The detail to include.
    /// </summary>
    public required string Detail { get; init; }

    /// <summary>
    /// Significance/purpose.
    /// </summary>
    public string? Significance { get; init; }
}

/// <summary>
/// Key dialogue beat.
/// </summary>
public sealed record DialogueBeat
{
    /// <summary>
    /// Characters involved.
    /// </summary>
    public List<Guid> CharacterIds { get; init; } = new();

    /// <summary>
    /// What needs to be communicated.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Subtext/underlying meaning.
    /// </summary>
    public string? Subtext { get; init; }

    /// <summary>
    /// Emotional tone.
    /// </summary>
    public required string Tone { get; init; }

    /// <summary>
    /// Purpose of this dialogue.
    /// </summary>
    public required string Purpose { get; init; }
}

/// <summary>
/// Emotional journey within a chapter.
/// </summary>
public sealed record EmotionalJourney
{
    /// <summary>
    /// Starting emotional state.
    /// </summary>
    public required string StartingState { get; init; }

    /// <summary>
    /// Ending emotional state.
    /// </summary>
    public required string EndingState { get; init; }

    /// <summary>
    /// Peak emotional moment.
    /// </summary>
    public required string PeakMoment { get; init; }

    /// <summary>
    /// Low point if any.
    /// </summary>
    public string? LowPoint { get; init; }

    /// <summary>
    /// Key emotional beats.
    /// </summary>
    public List<string> EmotionalBeats { get; init; } = new();
}

/// <summary>
/// Reference to a plot thread.
/// </summary>
public sealed record PlotThreadReference
{
    /// <summary>
    /// Plot thread ID.
    /// </summary>
    public required Guid ThreadId { get; init; }

    /// <summary>
    /// Thread name.
    /// </summary>
    public required string ThreadName { get; init; }

    /// <summary>
    /// Action taken on this thread (advance, pause, resolve).
    /// </summary>
    public required string Action { get; init; }

    /// <summary>
    /// Details of what happens.
    /// </summary>
    public required string Details { get; init; }
}

/// <summary>
/// A revelation or discovery.
/// </summary>
public sealed record Revelation
{
    /// <summary>
    /// What is revealed.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Who learns it.
    /// </summary>
    public List<Guid> DiscovererIds { get; init; } = new();

    /// <summary>
    /// Impact on the story.
    /// </summary>
    public required string Impact { get; init; }

    /// <summary>
    /// How it's revealed.
    /// </summary>
    public required string Method { get; init; }
}

/// <summary>
/// Cliffhanger details.
/// </summary>
public sealed record CliffhangerDetails
{
    /// <summary>
    /// Type of cliffhanger.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// What the cliffhanger is.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// When it's resolved.
    /// </summary>
    public required int ResolutionChapter { get; init; }

    /// <summary>
    /// Tension level (1-10).
    /// </summary>
    public required int TensionLevel { get; init; }
}

/// <summary>
/// Character appearance in a chapter.
/// </summary>
public sealed record CharacterAppearance
{
    /// <summary>
    /// Character ID.
    /// </summary>
    public required Guid CharacterId { get; init; }

    /// <summary>
    /// Role in this chapter.
    /// </summary>
    public required string RoleInChapter { get; init; }

    /// <summary>
    /// First scene they appear.
    /// </summary>
    public required int FirstSceneNumber { get; init; }

    /// <summary>
    /// Scenes they appear in.
    /// </summary>
    public List<int> SceneNumbers { get; init; } = new();

    /// <summary>
    /// Character's goal in this chapter.
    /// </summary>
    public required string ChapterGoal { get; init; }

    /// <summary>
    /// Emotional state.
    /// </summary>
    public required string EmotionalState { get; init; }
}

/// <summary>
/// Character development beat.
/// </summary>
public sealed record CharacterDevelopmentBeat
{
    /// <summary>
    /// Character ID.
    /// </summary>
    public required Guid CharacterId { get; init; }

    /// <summary>
    /// Type of development.
    /// </summary>
    public required string DevelopmentType { get; init; }

    /// <summary>
    /// Description of the beat.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Scene where this occurs.
    /// </summary>
    public int? SceneNumber { get; init; }
}

/// <summary>
/// Character interaction.
/// </summary>
public sealed record CharacterInteraction
{
    /// <summary>
    /// Character IDs involved.
    /// </summary>
    public required List<Guid> CharacterIds { get; init; }

    /// <summary>
    /// Nature of interaction.
    /// </summary>
    public required string Nature { get; init; }

    /// <summary>
    /// Outcome/result.
    /// </summary>
    public required string Outcome { get; init; }

    /// <summary>
    /// Impact on their relationship.
    /// </summary>
    public required string RelationshipImpact { get; init; }
}

/// <summary>
/// Timeline position.
/// </summary>
public sealed record TimelinePosition
{
    /// <summary>
    /// Story day number (1-based).
    /// </summary>
    public required int StoryDay { get; init; }

    /// <summary>
    /// Specific date if applicable.
    /// </summary>
    public DateTime? SpecificDate { get; init; }

    /// <summary>
    /// Relative timing.
    /// </summary>
    public required string RelativeTiming { get; init; }

    /// <summary>
    /// Duration this chapter covers.
    /// </summary>
    public required string Duration { get; init; }

    /// <summary>
    /// Notes on timeline.
    /// </summary>
    public string? TimelineNotes { get; init; }
}

/// <summary>
/// Continuity requirement.
/// </summary>
public sealed record ContinuityRequirement
{
    /// <summary>
    /// What must be maintained.
    /// </summary>
    public required string Element { get; init; }

    /// <summary>
    /// Source chapter.
    /// </summary>
    public required int SourceChapter { get; init; }

    /// <summary>
    /// Details.
    /// </summary>
    public required string Details { get; init; }

    /// <summary>
    /// Priority level.
    /// </summary>
    public required ClarificationPriority Priority { get; init; }
}

/// <summary>
/// Setup element for future payoff.
/// </summary>
public sealed record SetupElement
{
    /// <summary>
    /// Setup ID for tracking.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// What is being set up.
    /// </summary>
    public required string Element { get; init; }

    /// <summary>
    /// How it's introduced.
    /// </summary>
    public required string Introduction { get; init; }

    /// <summary>
    /// When payoff occurs.
    /// </summary>
    public required int PayoffChapter { get; init; }

    /// <summary>
    /// Type of setup.
    /// </summary>
    public required SetupPayoffType Type { get; init; }
}

/// <summary>
/// Payoff element.
/// </summary>
public sealed record PayoffElement
{
    /// <summary>
    /// Reference to setup.
    /// </summary>
    public required Guid SetupId { get; init; }

    /// <summary>
    /// What is being paid off.
    /// </summary>
    public required string Element { get; init; }

    /// <summary>
    /// How it's paid off.
    /// </summary>
    public required string Payoff { get; init; }

    /// <summary>
    /// Impact level.
    /// </summary>
    public required int ImpactLevel { get; init; }
}

/// <summary>
/// Theme reference.
/// </summary>
public sealed record ThemeReference
{
    /// <summary>
    /// Theme being explored.
    /// </summary>
    public required string Theme { get; init; }

    /// <summary>
    /// How it's explored.
    /// </summary>
    public required string Exploration { get; init; }

    /// <summary>
    /// Through which element.
    /// </summary>
    public required string Vehicle { get; init; }
}

/// <summary>
/// Generation attempt record.
/// </summary>
public sealed record GenerationAttempt
{
    /// <summary>
    /// Attempt number.
    /// </summary>
    public required int AttemptNumber { get; init; }

    /// <summary>
    /// When attempted.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Whether successful.
    /// </summary>
    public required bool Successful { get; init; }

    /// <summary>
    /// Word count generated.
    /// </summary>
    public int? WordCount { get; init; }

    /// <summary>
    /// Quality score achieved.
    /// </summary>
    public double? QualityScore { get; init; }

    /// <summary>
    /// Failure reason if failed.
    /// </summary>
    public string? FailureReason { get; init; }

    /// <summary>
    /// Token usage.
    /// </summary>
    public TokenUsage? TokenUsage { get; init; }

    /// <summary>
    /// Model used.
    /// </summary>
    public string? ModelUsed { get; init; }
}

/// <summary>
/// Token usage tracking.
/// </summary>
public sealed record TokenUsage
{
    /// <summary>
    /// Input tokens.
    /// </summary>
    public required int InputTokens { get; init; }

    /// <summary>
    /// Output tokens.
    /// </summary>
    public required int OutputTokens { get; init; }

    /// <summary>
    /// Total tokens.
    /// </summary>
    public int TotalTokens => InputTokens + OutputTokens;

    /// <summary>
    /// Estimated cost.
    /// </summary>
    public decimal? EstimatedCost { get; init; }
}
