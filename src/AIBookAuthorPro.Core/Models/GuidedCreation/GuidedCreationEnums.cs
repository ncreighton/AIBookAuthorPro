// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

namespace AIBookAuthorPro.Core.Models.GuidedCreation;

/// <summary>
/// Source of the initial prompt.
/// </summary>
public enum PromptSource
{
    /// <summary>Manually typed by user.</summary>
    Manual,
    /// <summary>Imported from Claude conversation.</summary>
    ImportedFromClaude,
    /// <summary>Imported from ChatGPT conversation.</summary>
    ImportedFromChatGPT,
    /// <summary>Imported from a text file.</summary>
    ImportedFromFile,
    /// <summary>Voice transcription input.</summary>
    VoiceTranscription,
    /// <summary>Imported from a template.</summary>
    Template,
    /// <summary>Pasted from clipboard.</summary>
    Clipboard
}

/// <summary>
/// Age range classification for target audience.
/// </summary>
public enum AudienceAgeRange
{
    /// <summary>Children (5-8).</summary>
    Children,
    /// <summary>Middle Grade (9-12).</summary>
    MiddleGrade,
    /// <summary>Young Adult (13-17).</summary>
    YoungAdult,
    /// <summary>New Adult (18-25).</summary>
    NewAdult,
    /// <summary>Adult (25+).</summary>
    Adult,
    /// <summary>All ages.</summary>
    AllAges
}

/// <summary>
/// Narrative pacing classification.
/// </summary>
public enum NarrativePacing
{
    /// <summary>Slow, contemplative pacing.</summary>
    Slow,
    /// <summary>Moderate, balanced pacing.</summary>
    Moderate,
    /// <summary>Fast, action-driven pacing.</summary>
    Fast,
    /// <summary>Variable pacing that changes throughout.</summary>
    Variable,
    /// <summary>Building, starts slow and accelerates.</summary>
    Building
}

/// <summary>
/// Book length classification.
/// </summary>
public enum BookLengthCategory
{
    /// <summary>Short story (under 7,500 words).</summary>
    ShortStory,
    /// <summary>Novelette (7,500-17,500 words).</summary>
    Novelette,
    /// <summary>Novella (17,500-40,000 words).</summary>
    Novella,
    /// <summary>Short novel (40,000-60,000 words).</summary>
    ShortNovel,
    /// <summary>Standard novel (60,000-100,000 words).</summary>
    StandardNovel,
    /// <summary>Long novel (100,000-150,000 words).</summary>
    LongNovel,
    /// <summary>Epic (150,000+ words).</summary>
    Epic
}

/// <summary>
/// Structure template types.
/// </summary>
public enum StructureTemplate
{
    /// <summary>Classic three-act structure.</summary>
    ThreeAct,
    /// <summary>Hero's Journey / Monomyth.</summary>
    HerosJourney,
    /// <summary>Seven-point story structure.</summary>
    SevenPoint,
    /// <summary>Save the Cat beat sheet.</summary>
    SaveTheCat,
    /// <summary>Freytag's Pyramid.</summary>
    FreytagPyramid,
    /// <summary>Dan Harmon's Story Circle.</summary>
    StoryCircle,
    /// <summary>Fichtean Curve.</summary>
    FichteanCurve,
    /// <summary>In Medias Res.</summary>
    InMediasRes,
    /// <summary>Non-linear / Experimental.</summary>
    NonLinear,
    /// <summary>Episodic structure.</summary>
    Episodic,
    /// <summary>Custom user-defined structure.</summary>
    Custom
}

/// <summary>
/// Blueprint status in the workflow.
/// </summary>
public enum BlueprintStatus
{
    /// <summary>Initial draft, not yet reviewed.</summary>
    Draft,
    /// <summary>Awaiting user review.</summary>
    PendingReview,
    /// <summary>Approved and ready for generation.</summary>
    Approved,
    /// <summary>Currently being used for generation.</summary>
    InGeneration,
    /// <summary>All generation complete.</summary>
    Completed,
    /// <summary>Archived, no longer active.</summary>
    Archived,
    /// <summary>Rejected, needs major revision.</summary>
    Rejected
}

/// <summary>
/// Chapter generation status.
/// </summary>
public enum ChapterGenerationStatus
{
    /// <summary>Not yet started.</summary>
    Pending,
    /// <summary>In the generation queue.</summary>
    Queued,
    /// <summary>Currently being generated.</summary>
    Generating,
    /// <summary>Generation complete, awaiting review.</summary>
    Generated,
    /// <summary>Being reviewed by user or AI.</summary>
    InReview,
    /// <summary>Approved and finalized.</summary>
    Approved,
    /// <summary>Needs revision based on feedback.</summary>
    NeedsRevision,
    /// <summary>Generation failed.</summary>
    Failed,
    /// <summary>Skipped by user.</summary>
    Skipped
}

/// <summary>
/// Scene type classification.
/// </summary>
public enum SceneType
{
    /// <summary>Action sequence.</summary>
    Action,
    /// <summary>Dialogue-focused scene.</summary>
    Dialogue,
    /// <summary>Internal thoughts/reflection.</summary>
    Introspection,
    /// <summary>Description/world-building.</summary>
    Description,
    /// <summary>Flashback sequence.</summary>
    Flashback,
    /// <summary>Dream sequence.</summary>
    Dream,
    /// <summary>Montage/time passage.</summary>
    Montage,
    /// <summary>Transition scene.</summary>
    Transition,
    /// <summary>Revelation/discovery.</summary>
    Revelation,
    /// <summary>Confrontation.</summary>
    Confrontation,
    /// <summary>Romance/relationship.</summary>
    Romance,
    /// <summary>Comic relief.</summary>
    ComicRelief,
    /// <summary>Suspense/tension building.</summary>
    Suspense,
    /// <summary>Resolution/aftermath.</summary>
    Resolution
}

/// <summary>
/// Character role in the story.
/// </summary>
public enum CharacterRole
{
    /// <summary>Main protagonist.</summary>
    Protagonist,
    /// <summary>Secondary protagonist.</summary>
    Deuteragonist,
    /// <summary>Main antagonist.</summary>
    Antagonist,
    /// <summary>Love interest.</summary>
    LoveInterest,
    /// <summary>Mentor figure.</summary>
    Mentor,
    /// <summary>Sidekick/companion.</summary>
    Sidekick,
    /// <summary>Confidant.</summary>
    Confidant,
    /// <summary>Foil character.</summary>
    Foil,
    /// <summary>Comic relief.</summary>
    ComicRelief,
    /// <summary>Supporting character.</summary>
    Supporting,
    /// <summary>Minor/background character.</summary>
    Minor,
    /// <summary>Narrator (if separate from protagonist).</summary>
    Narrator
}

/// <summary>
/// Setup/Payoff tracking types.
/// </summary>
public enum SetupPayoffType
{
    /// <summary>Subtle hint at future events.</summary>
    Foreshadowing,
    /// <summary>Object that must be used later (Chekhov's Gun).</summary>
    ChekhovsGun,
    /// <summary>Recurring symbolic element.</summary>
    SymbolicMotif,
    /// <summary>Character trait that becomes important.</summary>
    CharacterTrait,
    /// <summary>Plot device planted early.</summary>
    PlotDevice,
    /// <summary>Thematic element that recurs.</summary>
    ThematicElement,
    /// <summary>Red herring (intentional misdirection).</summary>
    RedHerring,
    /// <summary>Running gag or callback.</summary>
    RunningGag
}

/// <summary>
/// Generation session status.
/// </summary>
public enum GenerationSessionStatus
{
    /// <summary>Setting up the generation session.</summary>
    Initializing,
    /// <summary>Actively generating content.</summary>
    Running,
    /// <summary>Temporarily paused by user.</summary>
    Paused,
    /// <summary>Waiting for user review/input.</summary>
    WaitingForReview,
    /// <summary>All content successfully generated.</summary>
    Completed,
    /// <summary>Generation encountered an unrecoverable error.</summary>
    Failed,
    /// <summary>Cancelled by user.</summary>
    Cancelled
}

/// <summary>
/// Generation pipeline phase.
/// </summary>
public enum GenerationPhase
{
    /// <summary>Initial setup phase.</summary>
    Initialization,
    /// <summary>Building context for generation.</summary>
    ContextBuilding,
    /// <summary>Generating chapter content.</summary>
    ChapterGeneration,
    /// <summary>Running quality checks.</summary>
    QualityCheck,
    /// <summary>Applying revisions.</summary>
    Revision,
    /// <summary>Final polish and assembly.</summary>
    Finalization
}

/// <summary>
/// Quality evaluation verdict.
/// </summary>
public enum QualityVerdict
{
    /// <summary>90-100: Ready to publish, exceptional quality.</summary>
    Excellent,
    /// <summary>75-89: Good quality, minor polish needed.</summary>
    Good,
    /// <summary>60-74: Acceptable, some revision recommended.</summary>
    Acceptable,
    /// <summary>40-59: Needs work, significant revision needed.</summary>
    NeedsWork,
    /// <summary>0-39: Should be regenerated entirely.</summary>
    Regenerate
}

/// <summary>
/// Quality issue severity.
/// </summary>
public enum QualityIssueSeverity
{
    /// <summary>Must fix before accepting.</summary>
    Critical,
    /// <summary>Should fix for quality.</summary>
    Major,
    /// <summary>Nice to fix but not essential.</summary>
    Minor,
    /// <summary>Optional improvement suggestion.</summary>
    Suggestion
}

/// <summary>
/// Continuity issue type.
/// </summary>
public enum ContinuityIssueType
{
    /// <summary>Character knows something they shouldn't.</summary>
    CharacterKnowledge,
    /// <summary>Character acts inconsistently.</summary>
    CharacterBehavior,
    /// <summary>Physical description doesn't match.</summary>
    CharacterAppearance,
    /// <summary>Character in wrong location.</summary>
    CharacterLocation,
    /// <summary>Timeline impossibility.</summary>
    TimelineError,
    /// <summary>Setting details inconsistent.</summary>
    SettingInconsistency,
    /// <summary>Object appears/disappears incorrectly.</summary>
    ObjectTracking,
    /// <summary>Contradicts established plot.</summary>
    PlotContradiction,
    /// <summary>Dialogue inconsistency.</summary>
    DialogueInconsistency,
    /// <summary>Tone/voice shift.</summary>
    ToneShift
}

/// <summary>
/// Clarification request priority.
/// </summary>
public enum ClarificationPriority
{
    /// <summary>Must answer before proceeding.</summary>
    Critical,
    /// <summary>Important for quality but can use defaults.</summary>
    Important,
    /// <summary>Optional, can infer from context.</summary>
    Optional
}

/// <summary>
/// Wizard step in the guided creation flow.
/// </summary>
public enum WizardStep
{
    /// <summary>User enters or imports their creative prompt.</summary>
    PromptEntry,
    /// <summary>AI analyzes the prompt.</summary>
    PromptAnalysis,
    /// <summary>User answers clarifying questions.</summary>
    Clarifications,
    /// <summary>Review the generated blueprint overview.</summary>
    BlueprintReview,
    /// <summary>Edit chapter structure and outline.</summary>
    StructureEditor,
    /// <summary>Edit character profiles.</summary>
    CharacterEditor,
    /// <summary>Edit plot architecture.</summary>
    PlotEditor,
    /// <summary>Review world/setting details.</summary>
    WorldEditor,
    /// <summary>Review style guide.</summary>
    StyleEditor,
    /// <summary>Confirm generation settings.</summary>
    SettingsConfirmation,
    /// <summary>Active generation phase.</summary>
    Generation,
    /// <summary>Review and refine generated content.</summary>
    ReviewAndRefine,
    /// <summary>Final export and completion.</summary>
    Completion
}

/// <summary>
/// Chapter tone classification.
/// </summary>
public enum ChapterTone
{
    /// <summary>Lighthearted and fun.</summary>
    Light,
    /// <summary>Serious and dramatic.</summary>
    Serious,
    /// <summary>Dark and tense.</summary>
    Dark,
    /// <summary>Romantic and emotional.</summary>
    Romantic,
    /// <summary>Mysterious and suspenseful.</summary>
    Mysterious,
    /// <summary>Action-packed and exciting.</summary>
    Action,
    /// <summary>Contemplative and philosophical.</summary>
    Contemplative,
    /// <summary>Humorous and comedic.</summary>
    Humorous,
    /// <summary>Hopeful and uplifting.</summary>
    Hopeful,
    /// <summary>Melancholic and sad.</summary>
    Melancholic,
    /// <summary>Neutral, balanced.</summary>
    Neutral
}

/// <summary>
/// Pacing intensity level.
/// </summary>
public enum PacingIntensity
{
    /// <summary>Very slow, meditative.</summary>
    VerySlow,
    /// <summary>Slow, deliberate.</summary>
    Slow,
    /// <summary>Moderate pace.</summary>
    Moderate,
    /// <summary>Fast-paced.</summary>
    Fast,
    /// <summary>Breakneck, intense.</summary>
    Breakneck
}
