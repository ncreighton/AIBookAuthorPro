// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

namespace AIBookAuthorPro.Core.Models.GuidedCreation;

/// <summary>
/// Represents a step in the guided creation wizard.
/// </summary>
public enum GuidedCreationStep
{
    /// <summary>Welcome and introduction.</summary>
    Welcome,
    /// <summary>Seed prompt input.</summary>
    SeedPrompt,
    /// <summary>Prompt entry (alias for SeedPrompt).</summary>
    PromptEntry = SeedPrompt,
    /// <summary>Prompt analysis.</summary>
    Analysis,
    /// <summary>Prompt analysis (alias).</summary>
    PromptAnalysis = Analysis,
    /// <summary>Creative brief expansion.</summary>
    BriefExpansion,
    /// <summary>Clarification questions.</summary>
    Clarifications,
    /// <summary>Blueprint generation.</summary>
    BlueprintGeneration,
    /// <summary>Blueprint review.</summary>
    BlueprintReview,
    /// <summary>Character editor.</summary>
    CharacterEditor,
    /// <summary>World editor.</summary>
    WorldEditor,
    /// <summary>Plot editor.</summary>
    PlotEditor,
    /// <summary>Style editor.</summary>
    StyleEditor,
    /// <summary>Structure editor.</summary>
    StructureEditor,
    /// <summary>Configuration setup.</summary>
    Configuration,
    /// <summary>Settings confirmation (alias).</summary>
    SettingsConfirmation = Configuration,
    /// <summary>Generation in progress.</summary>
    Generation,
    /// <summary>Review and editing.</summary>
    Review,
    /// <summary>Review and refine (alias).</summary>
    ReviewAndRefine = Review,
    /// <summary>Export and completion.</summary>
    Export,
    /// <summary>Completed.</summary>
    Completed,
    /// <summary>Completion (alias).</summary>
    Completion = Completed
}

/// <summary>
/// Tracks the state of a guided creation wizard session.
/// </summary>
public sealed class GuidedCreationWizardSession
{
    /// <summary>Unique session identifier.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>When the session was started.</summary>
    public DateTime StartedAt { get; init; } = DateTime.UtcNow;

    /// <summary>When the session was last updated.</summary>
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Current wizard step.</summary>
    public GuidedCreationStep CurrentStep { get; set; } = GuidedCreationStep.Welcome;

    /// <summary>Step history for navigation.</summary>
    public List<GuidedCreationStep> StepHistory { get; init; } = new();

    /// <summary>The seed prompt entered by user.</summary>
    public BookSeedPrompt? SeedPrompt { get; set; }

    /// <summary>Analysis result from prompt.</summary>
    public PromptAnalysisResult? AnalysisResult { get; set; }

    /// <summary>Expanded creative brief.</summary>
    public ExpandedCreativeBrief? ExpandedBrief { get; set; }

    /// <summary>Clarification questions.</summary>
    public List<ClarificationQuestion>? Clarifications { get; set; }

    /// <summary>Required clarifications (alias for Clarifications).</summary>
    public List<ClarificationQuestion>? RequiredClarifications { get => Clarifications; set => Clarifications = value; }

    /// <summary>User responses to clarifications.</summary>
    public Dictionary<Guid, string> ClarificationResponses { get; init; } = new();

    /// <summary>Alias for StartedAt.</summary>
    public DateTime CreatedAt { get => StartedAt; }

    /// <summary>Generated book blueprint.</summary>
    public BookBlueprint? Blueprint { get; set; }

    /// <summary>Whether blueprint has been approved.</summary>
    public bool BlueprintApproved { get; set; }

    /// <summary>Generation configuration.</summary>
    public GenerationConfiguration? Configuration { get; set; }

    /// <summary>Active generation session.</summary>
    public GenerationSession? GenerationSession { get; set; }

    /// <summary>Session status.</summary>
    public WizardSessionStatus Status { get; set; } = WizardSessionStatus.InProgress;

    /// <summary>Any error message.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Progress percentage (0-100).</summary>
    public double ProgressPercentage { get; set; }

    /// <summary>When the session was completed.</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>Calculates progress percentage based on current step.</summary>
    public void UpdateProgress()
    {
        ProgressPercentage = CurrentStep switch
        {
            GuidedCreationStep.Welcome => 0,
            GuidedCreationStep.SeedPrompt => 8,
            GuidedCreationStep.Analysis => 16,
            GuidedCreationStep.BriefExpansion => 24,
            GuidedCreationStep.Clarifications => 32,
            GuidedCreationStep.BlueprintGeneration => 48,
            GuidedCreationStep.BlueprintReview => 56,
            GuidedCreationStep.Configuration => 64,
            GuidedCreationStep.Generation => 80,
            GuidedCreationStep.Review => 90,
            GuidedCreationStep.Export => 95,
            GuidedCreationStep.Completed => 100,
            _ => 0
        };
    }
}

/// <summary>
/// Status of wizard session.
/// </summary>
public enum WizardSessionStatus
{
    /// <summary>Session is in progress.</summary>
    InProgress,
    /// <summary>Session is paused.</summary>
    Paused,
    /// <summary>Session completed successfully.</summary>
    Completed,
    /// <summary>Session was cancelled.</summary>
    Cancelled,
    /// <summary>Session encountered an error.</summary>
    Error
}

/// <summary>
/// A clarification question to ask the user during guided creation.
/// </summary>
public sealed record ClarificationQuestion
{
    /// <summary>Unique identifier.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>The question text.</summary>
    public string Question { get; init; } = string.Empty;

    /// <summary>Why this question matters.</summary>
    public string Rationale { get; init; } = string.Empty;

    /// <summary>Category of question.</summary>
    public ClarificationCategory Category { get; init; } = ClarificationCategory.Plot;

    /// <summary>Priority level.</summary>
    public ClarificationPriority Priority { get; init; } = ClarificationPriority.Important;

    /// <summary>Suggested answer options (if applicable).</summary>
    public List<string>? SuggestedOptions { get; init; }

    /// <summary>Default answer if user skips.</summary>
    public string? DefaultAnswer { get; init; }

    /// <summary>User's response.</summary>
    public string? Response { get; set; }

    /// <summary>Whether this has been answered.</summary>
    public bool IsAnswered => !string.IsNullOrWhiteSpace(Response);
}

/// <summary>
/// Category of clarification question.
/// </summary>
public enum ClarificationCategory
{
    /// <summary>Plot-related questions.</summary>
    Plot,
    /// <summary>Character-related questions.</summary>
    Character,
    /// <summary>World-building questions.</summary>
    WorldBuilding,
    /// <summary>Style and tone questions.</summary>
    Style,
    /// <summary>Audience and genre questions.</summary>
    Audience,
    /// <summary>Structural questions.</summary>
    Structure,
    /// <summary>Theme-related questions.</summary>
    Theme,
    /// <summary>General/other questions.</summary>
    General
}

/// <summary>
/// Summary of wizard progress.
/// </summary>
public sealed record WizardProgressSummary
{
    /// <summary>Current step.</summary>
    public GuidedCreationStep CurrentStep { get; init; } = GuidedCreationStep.Welcome;

    /// <summary>Total steps.</summary>
    public int TotalSteps { get; init; } = 12;

    /// <summary>Current step number (1-based).</summary>
    public int CurrentStepNumber { get; init; }

    /// <summary>Current step index (0-based).</summary>
    public int CurrentStepIndex { get => CurrentStepNumber - 1; }

    /// <summary>Progress percentage.</summary>
    public double ProgressPercentage { get; init; }

    /// <summary>Alias for ProgressPercentage.</summary>
    public double OverallProgressPercentage { get => ProgressPercentage; init => ProgressPercentage = value; }

    /// <summary>Steps completed.</summary>
    public List<GuidedCreationStep> CompletedSteps { get; init; } = new();

    /// <summary>Steps remaining.</summary>
    public List<GuidedCreationStep> RemainingSteps { get; init; } = new();

    /// <summary>Estimated time remaining.</summary>
    public TimeSpan? EstimatedTimeRemaining { get; init; }

    /// <summary>Time elapsed.</summary>
    public TimeSpan TimeElapsed { get; init; } = TimeSpan.Zero;

    /// <summary>Alias for TimeElapsed.</summary>
    public TimeSpan TimeSpentSoFar { get => TimeElapsed; init => TimeElapsed = value; }

    /// <summary>Whether analysis result is available.</summary>
    public bool HasAnalysisResult { get; init; }

    /// <summary>Whether blueprint is available.</summary>
    public bool HasBlueprint { get; init; }

    /// <summary>Whether blueprint is approved.</summary>
    public bool IsBlueprintApproved { get; init; }

    /// <summary>Whether generation session exists.</summary>
    public bool HasGenerationSession { get; init; }

    /// <summary>Generation progress details.</summary>
    public GenerationProgress? GenerationProgress { get; init; }

    /// <summary>Status message.</summary>
    public string StatusMessage { get; init; } = string.Empty;
}

/// <summary>
/// Progress report for blueprint generation.
/// </summary>
public sealed record BlueprintGenerationProgress
{
    /// <summary>Current phase.</summary>
    public string Phase { get; init; } = string.Empty;

    /// <summary>Progress percentage (0-100).</summary>
    public double Percentage { get; init; }

    /// <summary>Status message.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Components completed.</summary>
    public List<string> CompletedComponents { get; init; } = new();

    /// <summary>Current component being generated.</summary>
    public string? CurrentComponent { get; init; }
}

/// <summary>
/// Progress report for generation.
/// </summary>
public sealed record GenerationProgress
{
    /// <summary>Current chapter number.</summary>
    public int CurrentChapter { get; init; }

    /// <summary>Total chapters.</summary>
    public int TotalChapters { get; init; }

    /// <summary>Current scene within chapter.</summary>
    public int? CurrentScene { get; init; }

    /// <summary>Total scenes in current chapter.</summary>
    public int? TotalScenes { get; init; }

    /// <summary>Overall progress percentage.</summary>
    public double OverallPercentage { get; init; }

    /// <summary>Chapter progress percentage.</summary>
    public double ChapterPercentage { get; init; }

    /// <summary>Status message.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Words generated so far.</summary>
    public int WordsGenerated { get; init; }

    /// <summary>Estimated words remaining.</summary>
    public int EstimatedWordsRemaining { get; init; }

    /// <summary>Estimated time remaining.</summary>
    public TimeSpan? EstimatedTimeRemaining { get; init; }

    /// <summary>Current generation phase.</summary>
    public GenerationPhase Phase { get; init; } = GenerationPhase.Initialization;
}

/// <summary>
/// Chapter summary for context.
/// </summary>
public sealed record ChapterSummary
{
    /// <summary>Chapter number.</summary>
    public int ChapterNumber { get; init; }

    /// <summary>Chapter title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Brief summary.</summary>
    public string Summary { get; init; } = string.Empty;

    /// <summary>Key events.</summary>
    public List<string> KeyEvents { get; init; } = new();

    /// <summary>Characters appearing.</summary>
    public List<Guid> CharactersAppearing { get; init; } = new();

    /// <summary>Locations used.</summary>
    public List<Guid> Locations { get; init; } = new();

    /// <summary>Plot threads advanced.</summary>
    public List<Guid> PlotThreadsAdvanced { get; init; } = new();

    /// <summary>Word count.</summary>
    public int WordCount { get; init; }
}

/// <summary>
/// Setup/payoff tracking.
/// </summary>
public sealed record SetupPayoff
{
    /// <summary>Unique identifier.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Type of setup/payoff.</summary>
    public SetupPayoffType Type { get; init; } = SetupPayoffType.PlotDevice;

    /// <summary>Description.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Chapter where setup occurs.</summary>
    public int SetupChapter { get; init; }

    /// <summary>Expected payoff chapter.</summary>
    public int? ExpectedPayoffChapter { get; init; }

    /// <summary>Payoff chapter alias (returns Expected or Actual).</summary>
    public int? PayoffChapter { get => ActualPayoffChapter ?? ExpectedPayoffChapter; }

    /// <summary>Actual payoff chapter (if resolved).</summary>
    public int? ActualPayoffChapter { get; init; }

    /// <summary>Whether this has been paid off.</summary>
    public bool IsResolved { get; init; }

    /// <summary>Related plot thread.</summary>
    public Guid? RelatedPlotThreadId { get; init; }
}

/// <summary>
/// Pipeline step interface.
/// </summary>
public interface IPipelineStep
{
    /// <summary>Step name.</summary>
    string Name { get; }

    /// <summary>Step order.</summary>
    int Order { get; }

    /// <summary>Whether step is enabled.</summary>
    bool IsEnabled { get; }

    /// <summary>Executes the step.</summary>
    Task<PipelineStepResult> ExecuteAsync(
        PipelineContext context,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Pipeline context passed between steps.
/// </summary>
public sealed class PipelineContext
{
    /// <summary>Chapter being generated.</summary>
    public int ChapterNumber { get; init; }

    /// <summary>Book blueprint.</summary>
    public BookBlueprint Blueprint { get; init; } = new();

    /// <summary>Chapter blueprint.</summary>
    public ChapterBlueprint ChapterBlueprint { get; init; } = new();

    /// <summary>Generated content (built up through pipeline).</summary>
    public string GeneratedContent { get; set; } = string.Empty;

    /// <summary>Step results.</summary>
    public Dictionary<string, object> StepResults { get; init; } = new();

    /// <summary>Metadata.</summary>
    public Dictionary<string, object> Metadata { get; init; } = new();

    /// <summary>Character context string.</summary>
    public string CharacterContext { get; set; } = string.Empty;

    /// <summary>Narrative context string.</summary>
    public string NarrativeContext { get; set; } = string.Empty;

    /// <summary>Chapter-specific instructions.</summary>
    public string ChapterInstructions { get; set; } = string.Empty;

    /// <summary>System prompt for generation.</summary>
    public string SystemPrompt { get; set; } = string.Empty;

    /// <summary>Previously generated chapters in this session.</summary>
    public List<GeneratedChapter> PreviousChapters { get; set; } = new();
}

/// <summary>
/// Result from a pipeline step.
/// </summary>
public sealed record PipelineStepResult
{
    /// <summary>Whether step succeeded.</summary>
    public bool Success { get; init; }

    /// <summary>Name of the step that produced this result.</summary>
    public string StepName { get; init; } = string.Empty;

    /// <summary>Error message if failed.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Content produced by step.</summary>
    public string? Content { get; init; }

    /// <summary>Additional data.</summary>
    public Dictionary<string, object>? Data { get; init; }

    /// <summary>Whether to continue pipeline.</summary>
    public bool ShouldContinue { get; init; } = true;

    /// <summary>Creates success result.</summary>
    public static PipelineStepResult Succeeded(string? content = null, Dictionary<string, object>? data = null)
        => new() { Success = true, Content = content, Data = data };

    /// <summary>Creates failure result.</summary>
    public static PipelineStepResult Failed(string error, bool shouldContinue = false)
        => new() { Success = false, ErrorMessage = error, ShouldContinue = shouldContinue };
}

/// <summary>
/// Pipeline progress report.
/// </summary>
public sealed record PipelineProgress
{
    /// <summary>Current step name.</summary>
    public string CurrentStep { get; init; } = string.Empty;

    /// <summary>Step number (1-based).</summary>
    public int StepNumber { get; init; }

    /// <summary>Total steps.</summary>
    public int TotalSteps { get; init; }

    /// <summary>Progress percentage.</summary>
    public double Percentage { get; init; }

    /// <summary>Status message.</summary>
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// Container for chapter summaries.
/// </summary>
public sealed record ChapterSummaries
{
    /// <summary>List of summaries for each chapter.</summary>
    public List<ChapterSummary> Summaries { get; init; } = new();

    /// <summary>Overall plot summary so far.</summary>
    public string PlotSummary { get; init; } = string.Empty;

    /// <summary>AI-generated summary.</summary>
    public string AISummary { get; init; } = string.Empty;

    /// <summary>Brief one-line summary.</summary>
    public string BriefSummary { get; init; } = string.Empty;

    /// <summary>Key events in the chapter.</summary>
    public List<string> KeyEvents { get; init; } = new();
}

/// <summary>
/// Simple chapter definition for structural planning.
/// </summary>
public sealed record ChapterDefinition
{
    /// <summary>Chapter number.</summary>
    public int ChapterNumber { get; init; }

    /// <summary>Chapter title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Chapter purpose.</summary>
    public string Purpose { get; init; } = string.Empty;

    /// <summary>Target word count.</summary>
    public int TargetWordCount { get; init; }

    /// <summary>Key beats in this chapter.</summary>
    public List<string> KeyBeats { get; init; } = new();
}
