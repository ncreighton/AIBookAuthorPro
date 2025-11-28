// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

namespace AIBookAuthorPro.Core.Models.GuidedCreation;

/// <summary>
/// Represents an active book generation session.
/// Tracks all progress, metrics, and generated content.
/// </summary>
public sealed class GenerationSession
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Reference to the blueprint.
    /// </summary>
    public required Guid BlueprintId { get; init; }

    /// <summary>
    /// Current status.
    /// </summary>
    public GenerationSessionStatus Status { get; set; } = GenerationSessionStatus.Initializing;

    /// <summary>
    /// When session started.
    /// </summary>
    public DateTime StartedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// When session completed.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// When session was paused.
    /// </summary>
    public DateTime? PausedAt { get; set; }

    /// <summary>
    /// When session was last active.
    /// </summary>
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;

    // ================== PROGRESS TRACKING ==================

    /// <summary>
    /// Total chapters to generate.
    /// </summary>
    public required int TotalChapters { get; init; }

    /// <summary>
    /// Chapters completed.
    /// </summary>
    public int CompletedChapters { get; set; }

    /// <summary>
    /// Current chapter being generated.
    /// </summary>
    public int CurrentChapter { get; set; }

    /// <summary>
    /// Current scene within chapter.
    /// </summary>
    public int? CurrentScene { get; set; }

    /// <summary>
    /// Current phase.
    /// </summary>
    public GenerationPhase CurrentPhase { get; set; } = GenerationPhase.Initialization;

    /// <summary>
    /// Current operation description.
    /// </summary>
    public string CurrentOperation { get; set; } = "Initializing...";

    /// <summary>
    /// Target word count.
    /// </summary>
    public required int TotalWordCountTarget { get; init; }

    /// <summary>
    /// Words generated so far.
    /// </summary>
    public int GeneratedWordCount { get; set; }

    /// <summary>
    /// Overall progress (0-100).
    /// </summary>
    public double OverallProgress => TotalChapters > 0 
        ? (double)CompletedChapters / TotalChapters * 100 
        : 0;

    // ================== TIME TRACKING ==================

    /// <summary>
    /// Total elapsed time.
    /// </summary>
    public TimeSpan ElapsedTime { get; set; }

    /// <summary>
    /// Estimated time remaining.
    /// </summary>
    public TimeSpan? EstimatedTimeRemaining { get; set; }

    /// <summary>
    /// Average time per chapter.
    /// </summary>
    public TimeSpan? AverageTimePerChapter { get; set; }

    /// <summary>
    /// Time spent paused.
    /// </summary>
    public TimeSpan TotalPausedTime { get; set; }

    // ================== QUALITY METRICS ==================

    /// <summary>
    /// Quality results for each chapter.
    /// </summary>
    public List<ChapterQualityResult> QualityResults { get; init; } = new();

    /// <summary>
    /// Overall quality score.
    /// </summary>
    public double OverallQualityScore { get; set; }

    /// <summary>
    /// Average quality score.
    /// </summary>
    public double AverageQualityScore => QualityResults.Count > 0
        ? QualityResults.Average(q => q.OverallScore)
        : 0;

    /// <summary>
    /// Chapters requiring review.
    /// </summary>
    public List<int> ChaptersRequiringReview { get; init; } = new();

    /// <summary>
    /// Chapters that failed quality.
    /// </summary>
    public List<int> FailedChapters { get; init; } = new();

    // ================== COST TRACKING ==================

    /// <summary>
    /// Token usage tracker.
    /// </summary>
    public required TokenUsageTracker TokenUsage { get; init; }

    /// <summary>
    /// Estimated cost so far.
    /// </summary>
    public decimal EstimatedCost { get; set; }

    /// <summary>
    /// Cost limit.
    /// </summary>
    public decimal CostLimit { get; set; }

    /// <summary>
    /// Cost per chapter.
    /// </summary>
    public List<ChapterCost> ChapterCosts { get; init; } = new();

    // ================== GENERATED CONTENT ==================

    /// <summary>
    /// Generated chapters.
    /// </summary>
    public List<GeneratedChapter> Chapters { get; init; } = new();

    // ================== ERROR TRACKING ==================

    /// <summary>
    /// Errors encountered.
    /// </summary>
    public List<GenerationError> Errors { get; init; } = new();

    /// <summary>
    /// Warnings.
    /// </summary>
    public List<GenerationWarning> Warnings { get; init; } = new();

    /// <summary>
    /// Total retry count.
    /// </summary>
    public int TotalRetries { get; set; }

    // ================== ACTIVITY LOG ==================

    /// <summary>
    /// Activity log.
    /// </summary>
    public List<ActivityLogEntry> ActivityLog { get; init; } = new();

    // ================== CHECKPOINTS ==================

    /// <summary>
    /// Checkpoints for resume capability.
    /// </summary>
    public List<SessionCheckpoint> Checkpoints { get; init; } = new();

    /// <summary>
    /// Last checkpoint.
    /// </summary>
    public SessionCheckpoint? LastCheckpoint { get; set; }
}

/// <summary>
/// Token usage tracker.
/// </summary>
public sealed class TokenUsageTracker
{
    /// <summary>
    /// Total input tokens.
    /// </summary>
    public long TotalInputTokens { get; set; }

    /// <summary>
    /// Total output tokens.
    /// </summary>
    public long TotalOutputTokens { get; set; }

    /// <summary>
    /// Total tokens.
    /// </summary>
    public long TotalTokens => TotalInputTokens + TotalOutputTokens;

    /// <summary>
    /// Tokens by chapter.
    /// </summary>
    public Dictionary<int, TokenUsage> TokensByChapter { get; init; } = new();

    /// <summary>
    /// Tokens by phase.
    /// </summary>
    public Dictionary<GenerationPhase, TokenUsage> TokensByPhase { get; init; } = new();
}

/// <summary>
/// Cost per chapter.
/// </summary>
public sealed record ChapterCost
{
    /// <summary>
    /// Chapter number.
    /// </summary>
    public required int ChapterNumber { get; init; }

    /// <summary>
    /// Cost.
    /// </summary>
    public required decimal Cost { get; init; }

    /// <summary>
    /// Token usage.
    /// </summary>
    public required TokenUsage TokenUsage { get; init; }

    /// <summary>
    /// Attempts.
    /// </summary>
    public int Attempts { get; init; }
}

/// <summary>
/// Chapter quality result.
/// </summary>
public sealed record ChapterQualityResult
{
    /// <summary>
    /// Chapter number.
    /// </summary>
    public required int ChapterNumber { get; init; }

    /// <summary>
    /// Overall score.
    /// </summary>
    public required double OverallScore { get; init; }

    /// <summary>
    /// Verdict.
    /// </summary>
    public required QualityVerdict Verdict { get; init; }

    /// <summary>
    /// Issue count.
    /// </summary>
    public int IssueCount { get; init; }

    /// <summary>
    /// Critical issues.
    /// </summary>
    public int CriticalIssues { get; init; }

    /// <summary>
    /// Auto-fixed count.
    /// </summary>
    public int AutoFixedCount { get; init; }

    /// <summary>
    /// Checked at.
    /// </summary>
    public DateTime CheckedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Generation error.
/// </summary>
public sealed record GenerationError
{
    /// <summary>
    /// Error ID.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Timestamp.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Phase when error occurred.
    /// </summary>
    public required GenerationPhase Phase { get; init; }

    /// <summary>
    /// Chapter number if applicable.
    /// </summary>
    public int? ChapterNumber { get; init; }

    /// <summary>
    /// Error type.
    /// </summary>
    public required string ErrorType { get; init; }

    /// <summary>
    /// Error message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Stack trace.
    /// </summary>
    public string? StackTrace { get; init; }

    /// <summary>
    /// Whether recovered.
    /// </summary>
    public bool Recovered { get; init; }

    /// <summary>
    /// Recovery action taken.
    /// </summary>
    public string? RecoveryAction { get; init; }
}

/// <summary>
/// Generation warning.
/// </summary>
public sealed record GenerationWarning
{
    /// <summary>
    /// Timestamp.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Category.
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// Message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Chapter number if applicable.
    /// </summary>
    public int? ChapterNumber { get; init; }
}

/// <summary>
/// Activity log entry.
/// </summary>
public sealed record ActivityLogEntry
{
    /// <summary>
    /// Timestamp.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Activity type.
    /// </summary>
    public required string ActivityType { get; init; }

    /// <summary>
    /// Description.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Details.
    /// </summary>
    public Dictionary<string, object>? Details { get; init; }
}

/// <summary>
/// Session checkpoint.
/// </summary>
public sealed record SessionCheckpoint
{
    /// <summary>
    /// Checkpoint ID.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Created at.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Last completed chapter.
    /// </summary>
    public required int LastCompletedChapter { get; init; }

    /// <summary>
    /// Session state snapshot.
    /// </summary>
    public required string StateSnapshot { get; init; }

    /// <summary>
    /// Metrics at checkpoint.
    /// </summary>
    public required CheckpointMetrics Metrics { get; init; }
}

/// <summary>
/// Checkpoint metrics.
/// </summary>
public sealed record CheckpointMetrics
{
    /// <summary>
    /// Words generated.
    /// </summary>
    public int WordsGenerated { get; init; }

    /// <summary>
    /// Elapsed time.
    /// </summary>
    public TimeSpan ElapsedTime { get; init; }

    /// <summary>
    /// Cost so far.
    /// </summary>
    public decimal Cost { get; init; }

    /// <summary>
    /// Average quality.
    /// </summary>
    public double AverageQuality { get; init; }
}
