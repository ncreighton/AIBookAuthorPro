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
    public int ChapterNumber { get; init; }

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
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Alternative titles considered.
    /// </summary>
    public List<string> AlternativeTitles { get; init; } = new();

    /// <summary>
    /// Act this chapter belongs to.
    /// </summary>
    public int ActNumber { get; init; }

    // ================== CONTENT SPECIFICATION ==================

    /// <summary>
    /// Target word count.
    /// </summary>
    public int TargetWordCount { get; init; }

    /// <summary>
    /// Minimum acceptable word count (defaults to 85% of target).
    /// </summary>
    public int MinWordCount { get; init; }

    /// <summary>
    /// Computed minimum if not set.
    /// </summary>
    public int EffectiveMinWordCount => MinWordCount > 0 ? MinWordCount : (int)(TargetWordCount * 0.85);

    /// <summary>
    /// Maximum acceptable word count (defaults to 115% of target).
    /// </summary>
    public int MaxWordCount { get; init; }

    /// <summary>
    /// Computed maximum if not set.
    /// </summary>
    public int EffectiveMaxWordCount => MaxWordCount > 0 ? MaxWordCount : (int)(TargetWordCount * 1.15);

    /// <summary>
    /// Primary purpose of this chapter.
    /// </summary>
    public string Purpose { get; init; } = string.Empty;

    /// <summary>
    /// Detailed chapter summary (what happens).
    /// </summary>
    public string Summary { get; init; } = string.Empty;

    /// <summary>
    /// Opening hook - how the chapter should begin.
    /// </summary>
    public string OpeningHook { get; init; } = string.Empty;

    /// <summary>
    /// Closing hook - how the chapter should end.
    /// </summary>
    public string ClosingHook { get; init; } = string.Empty;

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
    public string POV { get; init; } = string.Empty;

    /// <summary>
    /// POV character ID if character-based POV.
    /// </summary>
    public Guid? POVCharacterId { get; init; }

    /// <summary>
    /// Chapter tone.
    /// </summary>
    public ChapterTone Tone { get; init; } = ChapterTone.Serious;

    /// <summary>
    /// Pacing intensity.
    /// </summary>
    public PacingIntensity Pacing { get; init; } = PacingIntensity.Moderate;

    /// <summary>
    /// Alias for Pacing (same type name).
    /// </summary>
    public PacingIntensity PacingIntensity { get => Pacing; init => Pacing = value; }

    /// <summary>
    /// Emotional journey of the chapter.
    /// </summary>
    public EmotionalJourney EmotionalArc { get; init; } = new();

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
    /// Alias for PlotBeats.
    /// </summary>
    public List<string> PlotPoints { get => PlotBeats; init => PlotBeats = value; }

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
    /// Alias for CharacterAppearances.
    /// </summary>
    public List<CharacterAppearance> CharactersInChapter { get => CharacterAppearances; init => CharacterAppearances = value; }

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
    /// Alias for LocationIds.
    /// </summary>
    public List<Guid> LocationsUsed { get => LocationIds; init => LocationIds = value; }

    /// <summary>
    /// Timeline position.
    /// </summary>
    public TimelinePosition Timeline { get; init; } = new();

    /// <summary>
    /// Time of day/atmosphere.
    /// </summary>
    public string TimeOfDay { get; init; } = string.Empty;

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
    public int SceneNumber { get; init; }

    /// <summary>
    /// Alias for SceneNumber.
    /// </summary>
    public int Order { get => SceneNumber; init => SceneNumber = value; }

    /// <summary>
    /// Scene title/label.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Purpose of this scene.
    /// </summary>
    public string Purpose { get; init; } = string.Empty;

    /// <summary>
    /// Scene summary.
    /// </summary>
    public string Summary { get; init; } = string.Empty;

    /// <summary>
    /// Target word count.
    /// </summary>
    public int TargetWordCount { get; init; }

    /// <summary>
    /// Scene type.
    /// </summary>
    public SceneType Type { get; init; } = SceneType.Action;

    /// <summary>
    /// Location ID.
    /// </summary>
    public Guid LocationId { get; init; }

    /// <summary>
    /// Location name (convenience property).
    /// </summary>
    public string? Location { get; init; }

    /// <summary>
    /// Character IDs present in scene.
    /// </summary>
    public List<Guid> CharacterIds { get; init; } = new();

    /// <summary>
    /// Character names (convenience property).
    /// </summary>
    public List<string> Characters { get; init; } = new();

    /// <summary>
    /// POV character for this scene (if different from chapter).
    /// </summary>
    public Guid? ScenePOVCharacterId { get; init; }

    /// <summary>
    /// Conflict in this scene.
    /// </summary>
    public string Conflict { get; init; } = string.Empty;

    /// <summary>
    /// Resolution of scene conflict.
    /// </summary>
    public string Resolution { get; init; } = string.Empty;

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
    public string TransitionIn { get; init; } = string.Empty;

    /// <summary>
    /// Transition out of scene.
    /// </summary>
    public string TransitionOut { get; init; } = string.Empty;

    /// <summary>
    /// Pacing for this scene.
    /// </summary>
    public PacingIntensity Pacing { get; init; } = PacingIntensity.Moderate;

    /// <summary>
    /// Emotional tone.
    /// </summary>
    public ChapterTone Tone { get; init; } = ChapterTone.Serious;

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
    public string Sense { get; init; } = string.Empty;

    /// <summary>
    /// The detail to include.
    /// </summary>
    public string Detail { get; init; } = string.Empty;

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
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// Subtext/underlying meaning.
    /// </summary>
    public string? Subtext { get; init; }

    /// <summary>
    /// Emotional tone.
    /// </summary>
    public string Tone { get; init; } = string.Empty;

    /// <summary>
    /// Purpose of this dialogue.
    /// </summary>
    public string Purpose { get; init; } = string.Empty;
}

/// <summary>
/// Emotional journey within a chapter.
/// </summary>
public sealed record EmotionalJourney
{
    /// <summary>
    /// Starting emotional state.
    /// </summary>
    public string StartingState { get; init; } = string.Empty;

    /// <summary>
    /// Alias for StartingState.
    /// </summary>
    public string StartingEmotion { get => StartingState; init => StartingState = value; }

    /// <summary>
    /// Ending emotional state.
    /// </summary>
    public string EndingState { get; init; } = string.Empty;

    /// <summary>
    /// Alias for EndingState.
    /// </summary>
    public string EndingEmotion { get => EndingState; init => EndingState = value; }

    /// <summary>
    /// Peak emotional moment.
    /// </summary>
    public string PeakMoment { get; init; } = string.Empty;

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
    public Guid ThreadId { get; init; }

    /// <summary>
    /// Thread name.
    /// </summary>
    public string ThreadName { get; init; } = string.Empty;

    /// <summary>
    /// Action taken on this thread (advance, pause, resolve).
    /// </summary>
    public string Action { get; init; } = string.Empty;

    /// <summary>
    /// Details of what happens.
    /// </summary>
    public string Details { get; init; } = string.Empty;
}

/// <summary>
/// A revelation or discovery.
/// </summary>
public sealed record Revelation
{
    /// <summary>
    /// What is revealed.
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// Who learns it.
    /// </summary>
    public List<Guid> DiscovererIds { get; init; } = new();

    /// <summary>
    /// Impact on the story.
    /// </summary>
    public string Impact { get; init; } = string.Empty;

    /// <summary>
    /// How it's revealed.
    /// </summary>
    public string Method { get; init; } = string.Empty;
}

/// <summary>
/// Cliffhanger details.
/// </summary>
public sealed record CliffhangerDetails
{
    /// <summary>
    /// Type of cliffhanger.
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// What the cliffhanger is.
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// When it's resolved.
    /// </summary>
    public int ResolutionChapter { get; init; }

    /// <summary>
    /// Tension level (1-10).
    /// </summary>
    public int TensionLevel { get; init; }
}

/// <summary>
/// Character appearance in a chapter.
/// </summary>
public sealed record CharacterAppearance
{
    /// <summary>
    /// Character ID.
    /// </summary>
    public Guid CharacterId { get; init; }

    /// <summary>
    /// Role in this chapter.
    /// </summary>
    public string RoleInChapter { get; init; } = string.Empty;

    /// <summary>
    /// First scene they appear.
    /// </summary>
    public int FirstSceneNumber { get; init; }

    /// <summary>
    /// Scenes they appear in.
    /// </summary>
    public List<int> SceneNumbers { get; init; } = new();

    /// <summary>
    /// Character's goal in this chapter.
    /// </summary>
    public string ChapterGoal { get; init; } = string.Empty;

    /// <summary>
    /// Emotional state.
    /// </summary>
    public string EmotionalState { get; init; } = string.Empty;
}

/// <summary>
/// Character development beat.
/// </summary>
public sealed record CharacterDevelopmentBeat
{
    /// <summary>
    /// Character ID.
    /// </summary>
    public Guid CharacterId { get; init; }

    /// <summary>
    /// Type of development.
    /// </summary>
    public string DevelopmentType { get; init; } = string.Empty;

    /// <summary>
    /// Description of the beat.
    /// </summary>
    public string Description { get; init; } = string.Empty;

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
    public List<Guid> CharacterIds { get; init; } = new();

    /// <summary>
    /// Nature of interaction.
    /// </summary>
    public string Nature { get; init; } = string.Empty;

    /// <summary>
    /// Outcome/result.
    /// </summary>
    public string Outcome { get; init; } = string.Empty;

    /// <summary>
    /// Impact on their relationship.
    /// </summary>
    public string RelationshipImpact { get; init; } = string.Empty;
}

/// <summary>
/// Timeline position.
/// </summary>
public sealed record TimelinePosition
{
    /// <summary>
    /// Story day number (1-based).
    /// </summary>
    public int StoryDay { get; init; }

    /// <summary>
    /// Specific date if applicable.
    /// </summary>
    public DateTime? SpecificDate { get; init; }

    /// <summary>
    /// Relative timing.
    /// </summary>
    public string RelativeTiming { get; init; } = string.Empty;

    /// <summary>
    /// Duration this chapter covers.
    /// </summary>
    public string Duration { get; init; } = string.Empty;

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
    public string Element { get; init; } = string.Empty;

    /// <summary>
    /// Source chapter.
    /// </summary>
    public int SourceChapter { get; init; }

    /// <summary>
    /// Details.
    /// </summary>
    public string Details { get; init; } = string.Empty;

    /// <summary>
    /// Priority level.
    /// </summary>
    public ClarificationPriority Priority { get; init; } = ClarificationPriority.Important;
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
    public string Element { get; init; } = string.Empty;

    /// <summary>
    /// How it's introduced.
    /// </summary>
    public string Introduction { get; init; } = string.Empty;

    /// <summary>
    /// When payoff occurs.
    /// </summary>
    public int PayoffChapter { get; init; }

    /// <summary>
    /// Type of setup.
    /// </summary>
    public SetupPayoffType Type { get; init; } = SetupPayoffType.PlotDevice;
}

/// <summary>
/// Payoff element.
/// </summary>
public sealed record PayoffElement
{
    /// <summary>
    /// Reference to setup.
    /// </summary>
    public Guid SetupId { get; init; }

    /// <summary>
    /// What is being paid off.
    /// </summary>
    public string Element { get; init; } = string.Empty;

    /// <summary>
    /// How it's paid off.
    /// </summary>
    public string Payoff { get; init; } = string.Empty;

    /// <summary>
    /// Impact level.
    /// </summary>
    public int ImpactLevel { get; init; }
}

/// <summary>
/// Theme reference.
/// </summary>
public sealed record ThemeReference
{
    /// <summary>
    /// Theme being explored.
    /// </summary>
    public string Theme { get; init; } = string.Empty;

    /// <summary>
    /// How it's explored.
    /// </summary>
    public string Exploration { get; init; } = string.Empty;

    /// <summary>
    /// Through which element.
    /// </summary>
    public string Vehicle { get; init; } = string.Empty;
}

/// <summary>
/// Generation attempt record.
/// </summary>
public sealed record GenerationAttempt
{
    /// <summary>
    /// Attempt number.
    /// </summary>
    public int AttemptNumber { get; init; }

    /// <summary>
    /// When attempted.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Whether successful.
    /// </summary>
    public bool Successful { get; init; }

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
    public int InputTokens { get; init; }

    /// <summary>
    /// Output tokens.
    /// </summary>
    public int OutputTokens { get; init; }

    /// <summary>
    /// Total tokens.
    /// </summary>
    public int TotalTokens => InputTokens + OutputTokens;

    /// <summary>
    /// Estimated cost.
    /// </summary>
    public decimal? EstimatedCost { get; init; }
}
