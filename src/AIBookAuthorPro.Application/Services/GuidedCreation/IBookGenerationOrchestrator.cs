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
    Task<Result<GenerationSession>> StartFullGenerationAsync(
        BookBlueprint blueprint,
        GenerationOptions options,
        IProgress<DetailedGenerationProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a single chapter.
    /// </summary>
    Task<Result<GeneratedChapter>> GenerateChapterAsync(
        GenerationSession session,
        int chapterNumber,
        IProgress<ChapterGenerationProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Pauses generation at the next safe checkpoint.
    /// </summary>
    Task<Result> PauseGenerationAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resumes a paused generation session.
    /// </summary>
    Task<Result<GenerationSession>> ResumeGenerationAsync(
        Guid sessionId,
        IProgress<DetailedGenerationProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a generation session.
    /// </summary>
    Task<Result> CancelGenerationAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Regenerates a specific chapter with optional modifications.
    /// </summary>
    Task<Result<GeneratedChapter>> RegenerateChapterAsync(
        Guid sessionId,
        int chapterNumber,
        ChapterRegenerationOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Approves a generated chapter.
    /// </summary>
    Task<Result> ApproveChapterAsync(
        Guid sessionId,
        int chapterNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Requests revision of a chapter.
    /// </summary>
    Task<Result<GeneratedChapter>> RequestRevisionAsync(
        Guid sessionId,
        int chapterNumber,
        string instructions,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current state of a generation session.
    /// </summary>
    Task<Result<GenerationSession>> GetSessionStateAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets generation statistics.
    /// </summary>
    Task<Result<GenerationStatistics>> GetStatisticsAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Options for book generation.
/// </summary>
public sealed record GenerationOptions
{
    public bool UseConfigurationFromBlueprint { get; init; } = true;
    public GenerationConfiguration? ConfigurationOverride { get; init; }
    public int? StartFromChapter { get; init; }
    public int? EndAtChapter { get; init; }
    public bool SkipExistingChapters { get; init; } = true;
    public bool DryRun { get; init; }
}

/// <summary>
/// Chapter regeneration options.
/// </summary>
public sealed record ChapterRegenerationOptions
{
    public string? Instructions { get; init; }
    public List<string> KeepElements { get; init; } = new();
    public Dictionary<string, string> ChangeElements { get; init; } = new();
    public string? UseModel { get; init; }
    public double? Temperature { get; init; }
    public bool PreserveDialogue { get; init; }
    public bool PreserveKeyScenes { get; init; }
}

/// <summary>
/// Detailed generation progress report for internal orchestration.
/// </summary>
/// <remarks>
/// This is distinct from Core.Models.GuidedCreation.GenerationProgress
/// which is used for external API progress reporting.
/// </remarks>
public sealed record DetailedGenerationProgress
{
    public GenerationPhase Phase { get; init; }
    public string CurrentOperation { get; init; } = string.Empty;
    public double OverallPercentage { get; init; }
    public double PhaseProgress { get; init; }
    public int? CurrentChapter { get; init; }
    public int TotalChapters { get; init; }
    public int? CurrentScene { get; init; }
    public int WordsGenerated { get; init; }
    public int TargetWords { get; init; }
    public TimeSpan ElapsedTime { get; init; }
    public TimeSpan? EstimatedRemaining { get; init; }
    public decimal CostSoFar { get; init; }
    public List<string> RecentActivities { get; init; } = new();
    public double? AverageQualityScore { get; init; }
    public int IssuesFound { get; init; }
    public int IssuesAutoFixed { get; init; }
    public GenerationProgressStatus Status { get; init; }
}

public enum GenerationProgressStatus
{
    NotStarted,
    Generating,
    Evaluating,
    Revising,
    Paused,
    Complete,
    Error,
    Cancelled
}

public enum GenerationPhase
{
    Initializing,
    BuildingContext,
    Generating,
    QualityCheck,
    ContinuityVerification,
    Revising,
    Finalizing,
    Completed
}

public sealed record ChapterGenerationProgress
{
    public int ChapterNumber { get; init; }
    public string CurrentStep { get; init; } = string.Empty;
    public double Progress { get; init; }
    public int? CurrentScene { get; init; }
    public int WordsGenerated { get; init; }
    public bool IsStreaming { get; init; }
    public string? LatestChunk { get; init; }
}

public sealed record GenerationStatistics
{
    public Guid SessionId { get; init; }
    public int TotalChapters { get; init; }
    public int CompletedChapters { get; init; }
    public int TotalWordsGenerated { get; init; }
    public int TargetWordCount { get; init; }
    public long TotalTokensUsed { get; init; }
    public decimal TotalCost { get; init; }
    public TimeSpan TotalTimeElapsed { get; init; }
    public TimeSpan AverageTimePerChapter { get; init; }
    public double AverageQualityScore { get; init; }
    public int TotalIssuesFound { get; init; }
    public int TotalIssuesAutoFixed { get; init; }
    public int ChaptersRequiringReview { get; init; }
    public int TotalRetries { get; init; }
    public double WordsPerMinute => TotalTimeElapsed.TotalMinutes > 0 
        ? TotalWordsGenerated / TotalTimeElapsed.TotalMinutes : 0;
    public decimal CostPerWord => TotalWordsGenerated > 0 
        ? TotalCost / TotalWordsGenerated : 0;
    public decimal EstimatedTotalCost => CompletedChapters > 0 
        ? TotalCost / CompletedChapters * TotalChapters : 0;
    public List<ChapterStatistics> ChapterStats { get; init; } = new();
}

public sealed record ChapterStatistics
{
    public int ChapterNumber { get; init; }
    public int WordCount { get; init; }
    public double QualityScore { get; init; }
    public TimeSpan GenerationTime { get; init; }
    public decimal Cost { get; init; }
    public int AttemptCount { get; init; }
    public int IssuesFound { get; init; }
    public ChapterGenerationStatus Status { get; init; }
}
