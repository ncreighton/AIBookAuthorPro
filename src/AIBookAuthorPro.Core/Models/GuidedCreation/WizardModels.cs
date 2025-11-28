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
    /// <summary>Prompt analysis.</summary>
    Analysis,
    /// <summary>Creative brief expansion.</summary>
    BriefExpansion,
    /// <summary>Clarification questions.</summary>
    Clarifications,
    /// <summary>Blueprint generation.</summary>
    BlueprintGeneration,
    /// <summary>Blueprint review.</summary>
    BlueprintReview,
    /// <summary>Configuration setup.</summary>
    Configuration,
    /// <summary>Generation in progress.</summary>
    Generation,
    /// <summary>Review and editing.</summary>
    Review,
    /// <summary>Export and completion.</summary>
    Export,
    /// <summary>Completed.</summary>
    Completed
}

/// <summary>
/// Tracks the state of a guided creation wizard session.
/// </summary>
public sealed class GuidedCreationWizardSession
{
    /// <summary>
    /// Unique session identifier.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// When the session was started.
    /// </summary>
    public DateTime StartedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// When the session was last updated.
    /// </summary>
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Current wizard step.
    /// </summary>
    public GuidedCreationStep CurrentStep { get; set; } = GuidedCreationStep.Welcome;

    /// <summary>
    /// Step history for navigation.
    /// </summary>
    public List<GuidedCreationStep> StepHistory { get; init; } = new();

    /// <summary>
    /// The seed prompt entered by user.
    /// </summary>
    public BookSeedPrompt? SeedPrompt { get; set; }

    /// <summary>
    /// Analysis result from prompt.
    /// </summary>
    public PromptAnalysisResult? AnalysisResult { get; set; }

    /// <summary>
    /// Expanded creative brief.
    /// </summary>
    public ExpandedCreativeBrief? ExpandedBrief { get; set; }

    /// <summary>
    /// Clarification questions.
    /// </summary>
    public List<ClarificationQuestion>? Clarifications { get; set; }

    /// <summary>
    /// User responses to clarifications.
    /// </summary>
    public Dictionary<Guid, string> ClarificationResponses { get; init; } = new();

    /// <summary>
    /// Generated book blueprint.
    /// </summary>
    public BookBlueprint? Blueprint { get; set; }

    /// <summary>
    /// Whether blueprint has been approved.
    /// </summary>
    public bool BlueprintApproved { get; set; }

    /// <summary>
    /// Generation configuration.
    /// </summary>
    public GenerationConfiguration? Configuration { get; set; }

    /// <summary>
    /// Active generation session.
    /// </summary>
    public GenerationSession? GenerationSession { get; set; }

    /// <summary>
    /// Session status.
    /// </summary>
    public WizardSessionStatus Status { get; set; } = WizardSessionStatus.InProgress;

    /// <summary>
    /// Any error message.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Progress percentage (0-100).
    /// </summary>
    public double ProgressPercentage { get; set; }

    /// <summary>
    /// Calculates progress percentage based on current step.
    /// </summary>
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
/// Summary of wizard progress.
/// </summary>
public sealed record WizardProgressSummary
{
    /// <summary>
    /// Current step.
    /// </summary>
    public required GuidedCreationStep CurrentStep { get; init; }

    /// <summary>
    /// Total steps.
    /// </summary>
    public int TotalSteps { get; init; } = 12;

    /// <summary>
    /// Current step number (1-based).
    /// </summary>
    public required int CurrentStepNumber { get; init; }

    /// <summary>
    /// Progress percentage.
    /// </summary>
    public required double ProgressPercentage { get; init; }

    /// <summary>
    /// Steps completed.
    /// </summary>
    public List<GuidedCreationStep> CompletedSteps { get; init; } = new();

    /// <summary>
    /// Steps remaining.
    /// </summary>
    public List<GuidedCreationStep> RemainingSteps { get; init; } = new();

    /// <summary>
    /// Estimated time remaining.
    /// </summary>
    public TimeSpan? EstimatedTimeRemaining { get; init; }

    /// <summary>
    /// Time elapsed.
    /// </summary>
    public required TimeSpan TimeElapsed { get; init; }

    /// <summary>
    /// Status message.
    /// </summary>
    public required string StatusMessage { get; init; }
}

/// <summary>
/// Progress report for blueprint generation.
/// </summary>
public sealed record BlueprintGenerationProgress
{
    /// <summary>
    /// Current phase.
    /// </summary>
    public required string Phase { get; init; }

    /// <summary>
    /// Progress percentage (0-100).
    /// </summary>
    public required double Percentage { get; init; }

    /// <summary>
    /// Status message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Components completed.
    /// </summary>
    public List<string> CompletedComponents { get; init; } = new();

    /// <summary>
    /// Current component being generated.
    /// </summary>
    public string? CurrentComponent { get; init; }
}

/// <summary>
/// Progress report for generation.
/// </summary>
public sealed record GenerationProgress
{
    /// <summary>
    /// Current chapter number.
    /// </summary>
    public required int CurrentChapter { get; init; }

    /// <summary>
    /// Total chapters.
    /// </summary>
    public required int TotalChapters { get; init; }

    /// <summary>
    /// Current scene within chapter.
    /// </summary>
    public int? CurrentScene { get; init; }

    /// <summary>
    /// Total scenes in current chapter.
    /// </summary>
    public int? TotalScenes { get; init; }

    /// <summary>
    /// Overall progress percentage.
    /// </summary>
    public required double OverallPercentage { get; init; }

    /// <summary>
    /// Chapter progress percentage.
    /// </summary>
    public required double ChapterPercentage { get; init; }

    /// <summary>
    /// Status message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Words generated so far.
    /// </summary>
    public int WordsGenerated { get; init; }

    /// <summary>
    /// Estimated words remaining.
    /// </summary>
    public int EstimatedWordsRemaining { get; init; }

    /// <summary>
    /// Estimated time remaining.
    /// </summary>
    public TimeSpan? EstimatedTimeRemaining { get; init; }

    /// <summary>
    /// Current generation phase.
    /// </summary>
    public required GenerationPhase Phase { get; init; }
}

/// <summary>
/// Chapter summary for context.
/// </summary>
public sealed record ChapterSummary
{
    /// <summary>
    /// Chapter number.
    /// </summary>
    public required int ChapterNumber { get; init; }

    /// <summary>
    /// Chapter title.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Brief summary.
    /// </summary>
    public required string Summary { get; init; }

    /// <summary>
    /// Key events.
    /// </summary>
    public List<string> KeyEvents { get; init; } = new();

    /// <summary>
    /// Characters appearing.
    /// </summary>
    public List<Guid> CharactersAppearing { get; init; } = new();

    /// <summary>
    /// Locations used.
    /// </summary>
    public List<Guid> Locations { get; init; } = new();

    /// <summary>
    /// Plot threads advanced.
    /// </summary>
    public List<Guid> PlotThreadsAdvanced { get; init; } = new();

    /// <summary>
    /// Word count.
    /// </summary>
    public int WordCount { get; init; }
}

/// <summary>
/// Setup/payoff tracking.
/// </summary>
public sealed record SetupPayoff
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Type of setup/payoff.
    /// </summary>
    public required SetupPayoffType Type { get; init; }

    /// <summary>
    /// Description.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Chapter where setup occurs.
    /// </summary>
    public required int SetupChapter { get; init; }

    /// <summary>
    /// Expected payoff chapter.
    /// </summary>
    public int? ExpectedPayoffChapter { get; init; }

    /// <summary>
    /// Actual payoff chapter (if resolved).
    /// </summary>
    public int? ActualPayoffChapter { get; init; }

    /// <summary>
    /// Whether this has been paid off.
    /// </summary>
    public bool IsResolved { get; init; }

    /// <summary>
    /// Related plot thread.
    /// </summary>
    public Guid? RelatedPlotThreadId { get; init; }
}

/// <summary>
/// Pipeline step interface.
/// </summary>
public interface IPipelineStep
{
    /// <summary>
    /// Step name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Step order.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Whether step is enabled.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Executes the step.
    /// </summary>
    Task<PipelineStepResult> ExecuteAsync(
        PipelineContext context,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Pipeline context passed between steps.
/// </summary>
public sealed class PipelineContext
{
    /// <summary>
    /// Chapter being generated.
    /// </summary>
    public required int ChapterNumber { get; init; }

    /// <summary>
    /// Book blueprint.
    /// </summary>
    public required BookBlueprint Blueprint { get; init; }

    /// <summary>
    /// Chapter blueprint.
    /// </summary>
    public required ChapterBlueprint ChapterBlueprint { get; init; }

    /// <summary>
    /// Generated content (built up through pipeline).
    /// </summary>
    public string GeneratedContent { get; set; } = string.Empty;

    /// <summary>
    /// Step results.
    /// </summary>
    public Dictionary<string, object> StepResults { get; init; } = new();

    /// <summary>
    /// Metadata.
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Result from a pipeline step.
/// </summary>
public sealed record PipelineStepResult
{
    /// <summary>
    /// Whether step succeeded.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Error message if failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Content produced by step.
    /// </summary>
    public string? Content { get; init; }

    /// <summary>
    /// Additional data.
    /// </summary>
    public Dictionary<string, object>? Data { get; init; }

    /// <summary>
    /// Whether to continue pipeline.
    /// </summary>
    public bool ShouldContinue { get; init; } = true;

    /// <summary>
    /// Creates success result.
    /// </summary>
    public static PipelineStepResult Succeeded(string? content = null, Dictionary<string, object>? data = null)
        => new() { Success = true, Content = content, Data = data };

    /// <summary>
    /// Creates failure result.
    /// </summary>
    public static PipelineStepResult Failed(string error, bool shouldContinue = false)
        => new() { Success = false, ErrorMessage = error, ShouldContinue = shouldContinue };
}

/// <summary>
/// Pipeline progress report.
/// </summary>
public sealed record PipelineProgress
{
    /// <summary>
    /// Current step name.
    /// </summary>
    public required string CurrentStep { get; init; }

    /// <summary>
    /// Step number (1-based).
    /// </summary>
    public required int StepNumber { get; init; }

    /// <summary>
    /// Total steps.
    /// </summary>
    public required int TotalSteps { get; init; }

    /// <summary>
    /// Progress percentage.
    /// </summary>
    public required double Percentage { get; init; }

    /// <summary>
    /// Status message.
    /// </summary>
    public required string Message { get; init; }
}

/// <summary>
/// Generation context summary for metadata.
/// </summary>
public sealed record GenerationContextSummary
{
    /// <summary>
    /// Chapter number.
    /// </summary>
    public required int ChapterNumber { get; init; }

    /// <summary>
    /// Characters included.
    /// </summary>
    public required int CharactersIncluded { get; init; }

    /// <summary>
    /// Locations included.
    /// </summary>
    public required int LocationsIncluded { get; init; }

    /// <summary>
    /// Plot threads referenced.
    /// </summary>
    public required int PlotThreadsReferenced { get; init; }

    /// <summary>
    /// Previous chapters summarized.
    /// </summary>
    public required int PreviousChaptersSummarized { get; init; }

    /// <summary>
    /// Context token count.
    /// </summary>
    public required int ContextTokenCount { get; init; }

    /// <summary>
    /// When context was built.
    /// </summary>
    public DateTime BuiltAt { get; init; } = DateTime.UtcNow;
}
