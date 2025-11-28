// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Models.GuidedCreation;

namespace AIBookAuthorPro.Application.Services.GuidedCreation;

/// <summary>
/// Orchestrates the complete book generation process from blueprint to finished manuscript.
/// </summary>
public interface IBookGenerationOrchestrator
{
    /// <summary>
    /// Initiates autonomous generation of entire book from approved blueprint.
    /// </summary>
    /// <param name="blueprint">The approved blueprint.</param>
    /// <param name="options">Generation options.</param>
    /// <param name="progress">Progress reporter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Generation session.</returns>
    Task<Result<GenerationSession>> StartFullGenerationAsync(
        BookBlueprint blueprint,
        GenerationOptions options,
        IProgress<GenerationProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a single chapter.
    /// </summary>
    /// <param name="session">The generation session.</param>
    /// <param name="chapterNumber">Chapter number to generate.</param>
    /// <param name="progress">Progress reporter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Generated chapter.</returns>
    Task<Result<GeneratedChapter>> GenerateChapterAsync(
        GenerationSession session,
        int chapterNumber,
        IProgress<ChapterGenerationProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Pauses generation at the next safe checkpoint.
    /// </summary>
    /// <param name="sessionId">Session ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result of pause operation.</returns>
    Task<Result> PauseGenerationAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resumes a paused generation session.
    /// </summary>
    /// <param name="sessionId">Session ID.</param>
    /// <param name="progress">Progress reporter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Resumed session.</returns>
    Task<Result<GenerationSession>> ResumeGenerationAsync(
        Guid sessionId,
        IProgress<GenerationProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a generation session.
    /// </summary>
    /// <param name="sessionId">Session ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result of cancellation.</returns>
    Task<Result> CancelGenerationAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Regenerates a specific chapter with optional modifications.
    /// </summary>
    /// <param name="sessionId">Session ID.</param>
    /// <param name="chapterNumber">Chapter to regenerate.</param>
    /// <param name="options">Regeneration options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Regenerated chapter.</returns>
    Task<Result<GeneratedChapter>> RegenerateChapterAsync(
        Guid sessionId,
        int chapterNumber,
        ChapterRegenerationOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Approves a generated chapter.
    /// </summary>
    /// <param name="sessionId">Session ID.</param>
    /// <param name="chapterNumber">Chapter number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result of approval.</returns>
    Task<Result> ApproveChapterAsync(
        Guid sessionId,
        int chapterNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Requests revision of a chapter.
    /// </summary>
    /// <param name="sessionId">Session ID.</param>
    /// <param name="chapterNumber">Chapter number.</param>
    /// <param name="instructions">Revision instructions.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Revised chapter.</returns>
    Task<Result<GeneratedChapter>> RequestRevisionAsync(
        Guid sessionId,
        int chapterNumber,
        string instructions,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current state of a generation session.
    /// </summary>
    /// <param name="sessionId">Session ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Session state.</returns>
    Task<Result<GenerationSession>> GetSessionStateAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets generation statistics.
    /// </summary>
    /// <param name="sessionId">Session ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Generation statistics.</returns>
    Task<Result<GenerationStatistics>> GetStatisticsAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Options for book generation.
/// </summary>
public sealed record GenerationOptions
{
    /// <summary>Use the configuration from the blueprint.</summary>
    public bool UseConfigurationFromBlueprint { get; init; } = true;

    /// <summary>Override configuration if provided.</summary>
    public GenerationConfiguration? ConfigurationOverride { get; init; }

    /// <summary>Start from specific chapter.</summary>
    public int? StartFromChapter { get; init; }

    /// <summary>End at specific chapter.</summary>
    public int? EndAtChapter { get; init; }

    /// <summary>Skip already generated chapters.</summary>
    public bool SkipExistingChapters { get; init; } = true;

    /// <summary>Dry run - validate without generating.</summary>
    public bool DryRun { get; init; }
}

/// <summary>
/// Chapter regeneration options.
/// </summary>
public sealed record ChapterRegenerationOptions
{
    /// <summary>Specific instructions for regeneration.</summary>
    public string? Instructions { get; init; }

    /// <summary>Keep specific elements from original.</summary>
    public List<string> KeepElements { get; init; } = new();

    /// <summary>Change specific elements.</summary>
    public Dictionary<string, string> ChangeElements { get; init; } = new();

    /// <summary>Use different model.</summary>
    public string? UseModel { get; init; }

    /// <summary>Different temperature.</summary>
    public double? Temperature { get; init; }

    /// <summary>Preserve dialogue.</summary>
    public bool PreserveDialogue { get; init; }

    /// <summary>Preserve key scenes.</summary>
    public bool PreserveKeyScenes { get; init; }
}

/// <summary>
/// Generation progress report.
/// </summary>
public sealed record GenerationProgress
{
    /// <summary>Current phase.</summary>
    public GenerationPhase Phase { get; init; }

    /// <summary>Current operation description.</summary>
    public string CurrentOperation { get; init; } = string.Empty;

    /// <summary>Overall progress (0-100).</summary>
    public double OverallPercentage { get; init; }

    /// <summary>Phase progress (0-100).</summary>
    public double PhaseProgress { get; init; }

    /// <summary>Current chapter number if applicable.</summary>
    public int? CurrentChapter { get; init; }

    /// <summary>Total chapters.</summary>
    public int TotalChapters { get; init; }

    /// <summary>Current scene if applicable.</summary>
    public int? CurrentScene { get; init; }

    /// <summary>Words generated.</summary>
    public int WordsGenerated { get; init; }

    /// <summary>Target words.</summary>
    public int TargetWords { get; init; }

    /// <summary>Elapsed time.</summary>
    public TimeSpan ElapsedTime { get; init; }

    /// <summary>Estimated remaining time.</summary>
    public TimeSpan? EstimatedRemaining { get; init; }

    /// <summary>Cost so far.</summary>
    public decimal CostSoFar { get; init; }

    /// <summary>Recent activities.</summary>
    public List<string> RecentActivities { get; init; } = new();

    /// <summary>Average quality score so far.</summary>
    public double? AverageQualityScore { get; init; }

    /// <summary>Issues found.</summary>
    public int IssuesFound { get; init; }

    /// <summary>Issues auto-fixed.</summary>
    public int IssuesAutoFixed { get; init; }

    /// <summary>Progress status.</summary>
    public GenerationProgressStatus Status { get; init; }
}

/// <summary>
/// Generation progress status.
/// </summary>
public enum GenerationProgressStatus
{
    /// <summary>Not started.</summary>
    NotStarted,
    /// <summary>Currently generating.</summary>
    Generating,
    /// <summary>Evaluating quality.</summary>
    Evaluating,
    /// <summary>Revising content.</summary>
    Revising,
    /// <summary>Paused.</summary>
    Paused,
    /// <summary>Complete.</summary>
    Complete,
    /// <summary>Error occurred.</summary>
    Error,
    /// <summary>Cancelled.</summary>
    Cancelled
}

/// <summary>
/// Generation phase.
/// </summary>
public enum GenerationPhase
{
    /// <summary>Initializing.</summary>
    Initializing,
    /// <summary>Building context.</summary>
    BuildingContext,
    /// <summary>Generating content.</summary>
    Generating,
    /// <summary>Quality checking.</summary>
    QualityCheck,
    /// <summary>Continuity verification.</summary>
    ContinuityVerification,
    /// <summary>Revising.</summary>
    Revising,
    /// <summary>Finalizing.</summary>
    Finalizing,
    /// <summary>Completed.</summary>
    Completed
}

/// <summary>
/// Chapter generation progress.
/// </summary>
public sealed record ChapterGenerationProgress
{
    /// <summary>Chapter number.</summary>
    public int ChapterNumber { get; init; }

    /// <summary>Current step.</summary>
    public string CurrentStep { get; init; } = string.Empty;

    /// <summary>Progress (0-100).</summary>
    public double Progress { get; init; }

    /// <summary>Current scene if generating by scene.</summary>
    public int? CurrentScene { get; init; }

    /// <summary>Words generated.</summary>
    public int WordsGenerated { get; init; }

    /// <summary>Is streaming.</summary>
    public bool IsStreaming { get; init; }

    /// <summary>Latest content chunk if streaming.</summary>
    public string? LatestChunk { get; init; }
}

/// <summary>
/// Generation statistics.
/// </summary>
public sealed record GenerationStatistics
{
    /// <summary>Session ID.</summary>
    public Guid SessionId { get; init; }

    /// <summary>Total chapters.</summary>
    public int TotalChapters { get; init; }

    /// <summary>Completed chapters.</summary>
    public int CompletedChapters { get; init; }

    /// <summary>Total words generated.</summary>
    public int TotalWordsGenerated { get; init; }

    /// <summary>Target word count.</summary>
    public int TargetWordCount { get; init; }

    /// <summary>Total tokens used.</summary>
    public long TotalTokensUsed { get; init; }

    /// <summary>Total cost.</summary>
    public decimal TotalCost { get; init; }

    /// <summary>Total time elapsed.</summary>
    public TimeSpan TotalTimeElapsed { get; init; }

    /// <summary>Average time per chapter.</summary>
    public TimeSpan AverageTimePerChapter { get; init; }

    /// <summary>Average quality score.</summary>
    public double AverageQualityScore { get; init; }

    /// <summary>Total issues found.</summary>
    public int TotalIssuesFound { get; init; }

    /// <summary>Total issues auto-fixed.</summary>
    public int TotalIssuesAutoFixed { get; init; }

    /// <summary>Chapters requiring review.</summary>
    public int ChaptersRequiringReview { get; init; }

    /// <summary>Total retry count.</summary>
    public int TotalRetries { get; init; }

    /// <summary>Words per minute rate.</summary>
    public double WordsPerMinute => TotalTimeElapsed.TotalMinutes > 0 
        ? TotalWordsGenerated / TotalTimeElapsed.TotalMinutes 
        : 0;

    /// <summary>Cost per word.</summary>
    public decimal CostPerWord => TotalWordsGenerated > 0 
        ? TotalCost / TotalWordsGenerated 
        : 0;

    /// <summary>Estimated cost for full book.</summary>
    public decimal EstimatedTotalCost => CompletedChapters > 0 
        ? TotalCost / CompletedChapters * TotalChapters 
        : 0;

    /// <summary>Chapter statistics.</summary>
    public List<ChapterStatistics> ChapterStats { get; init; } = new();
}

/// <summary>
/// Statistics for a single chapter.
/// </summary>
public sealed record ChapterStatistics
{
    /// <summary>Chapter number.</summary>
    public int ChapterNumber { get; init; }

    /// <summary>Word count.</summary>
    public int WordCount { get; init; }

    /// <summary>Quality score.</summary>
    public double QualityScore { get; init; }

    /// <summary>Generation time.</summary>
    public TimeSpan GenerationTime { get; init; }

    /// <summary>Cost.</summary>
    public decimal Cost { get; init; }

    /// <summary>Attempt count.</summary>
    public int AttemptCount { get; init; }

    /// <summary>Issues found.</summary>
    public int IssuesFound { get; init; }

    /// <summary>Status.</summary>
    public ChapterGenerationStatus Status { get; init; }
}
