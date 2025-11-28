// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

namespace AIBookAuthorPro.Core.Models.GuidedCreation;

/// <summary>
/// Configuration for the book generation process.
/// </summary>
public sealed record GenerationConfiguration
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// AI provider settings.
    /// </summary>
    public required AIProviderSettings AISettings { get; init; }

    /// <summary>
    /// Generation behavior settings.
    /// </summary>
    public required GenerationBehavior Behavior { get; init; }

    /// <summary>
    /// Automation settings.
    /// </summary>
    public required AutomationSettings Automation { get; init; }

    /// <summary>
    /// Quality thresholds.
    /// </summary>
    public required QualityThresholds QualityThresholds { get; init; }

    /// <summary>
    /// Retry settings.
    /// </summary>
    public required RetrySettings Retry { get; init; }

    /// <summary>
    /// Cost limits.
    /// </summary>
    public required CostLimits CostLimits { get; init; }

    /// <summary>
    /// Notification settings.
    /// </summary>
    public required NotificationSettings Notifications { get; init; }

    /// <summary>
    /// Output settings.
    /// </summary>
    public required OutputSettings Output { get; init; }
}

/// <summary>
/// AI provider settings.
/// </summary>
public sealed record AIProviderSettings
{
    /// <summary>
    /// Primary provider to use.
    /// </summary>
    public required string PrimaryProvider { get; init; }

    /// <summary>
    /// Primary model.
    /// </summary>
    public required string PrimaryModel { get; init; }

    /// <summary>
    /// Fallback provider.
    /// </summary>
    public string? FallbackProvider { get; init; }

    /// <summary>
    /// Fallback model.
    /// </summary>
    public string? FallbackModel { get; init; }

    /// <summary>
    /// Temperature for generation.
    /// </summary>
    public double Temperature { get; init; } = 0.7;

    /// <summary>
    /// Top P sampling.
    /// </summary>
    public double TopP { get; init; } = 0.95;

    /// <summary>
    /// Max tokens per request.
    /// </summary>
    public int MaxTokensPerRequest { get; init; } = 4096;

    /// <summary>
    /// Request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; init; } = 120;

    /// <summary>
    /// Use streaming.
    /// </summary>
    public bool UseStreaming { get; init; } = true;
}

/// <summary>
/// Generation behavior settings.
/// </summary>
public sealed record GenerationBehavior
{
    /// <summary>
    /// Generate chapters sequentially or in parallel.
    /// </summary>
    public required GenerationMode Mode { get; init; }

    /// <summary>
    /// Max parallel generations (if parallel mode).
    /// </summary>
    public int MaxParallelGenerations { get; init; } = 1;

    /// <summary>
    /// Pause between chapters (seconds).
    /// </summary>
    public int DelayBetweenChapters { get; init; } = 2;

    /// <summary>
    /// Auto-save interval during generation.
    /// </summary>
    public int AutoSaveIntervalMinutes { get; init; } = 5;

    /// <summary>
    /// Generate in scenes vs full chapters.
    /// </summary>
    public bool GenerateByScene { get; init; } = true;

    /// <summary>
    /// Include previous chapter context.
    /// </summary>
    public bool IncludePreviousContext { get; init; } = true;

    /// <summary>
    /// How many previous chapters to include.
    /// </summary>
    public int PreviousContextChapters { get; init; } = 2;

    /// <summary>
    /// Context compression strategy.
    /// </summary>
    public required string ContextCompression { get; init; }
}

/// <summary>
/// Generation mode.
/// </summary>
public enum GenerationMode
{
    /// <summary>
    /// Generate chapters one at a time.
    /// </summary>
    Sequential,

    /// <summary>
    /// Generate multiple chapters in parallel.
    /// </summary>
    Parallel,

    /// <summary>
    /// Manual - require approval for each.
    /// </summary>
    Manual
}

/// <summary>
/// Automation settings.
/// </summary>
public sealed record AutomationSettings
{
    /// <summary>
    /// Run fully autonomously.
    /// </summary>
    public bool FullyAutonomous { get; init; } = true;

    /// <summary>
    /// Auto-approve chapters above quality threshold.
    /// </summary>
    public bool AutoApproveAboveThreshold { get; init; } = true;

    /// <summary>
    /// Auto-retry failed generations.
    /// </summary>
    public bool AutoRetryFailed { get; init; } = true;

    /// <summary>
    /// Auto-fix minor issues.
    /// </summary>
    public bool AutoFixMinorIssues { get; init; } = true;

    /// <summary>
    /// Pause for review at act breaks.
    /// </summary>
    public bool PauseAtActBreaks { get; init; } = false;

    /// <summary>
    /// Pause for review at midpoint.
    /// </summary>
    public bool PauseAtMidpoint { get; init; } = false;

    /// <summary>
    /// Always pause for key plot points.
    /// </summary>
    public bool PauseForKeyPlotPoints { get; init; } = false;

    /// <summary>
    /// Chapters to require manual review.
    /// </summary>
    public List<int> ManualReviewChapters { get; init; } = new();
}

/// <summary>
/// Quality thresholds.
/// </summary>
public sealed record QualityThresholds
{
    /// <summary>
    /// Minimum overall quality score (0-100).
    /// </summary>
    public int MinOverallScore { get; init; } = 70;

    /// <summary>
    /// Minimum narrative quality.
    /// </summary>
    public int MinNarrativeScore { get; init; } = 70;

    /// <summary>
    /// Minimum character consistency.
    /// </summary>
    public int MinCharacterScore { get; init; } = 75;

    /// <summary>
    /// Minimum plot adherence.
    /// </summary>
    public int MinPlotScore { get; init; } = 80;

    /// <summary>
    /// Minimum style consistency.
    /// </summary>
    public int MinStyleScore { get; init; } = 70;

    /// <summary>
    /// Minimum continuity accuracy.
    /// </summary>
    public int MinContinuityScore { get; init; } = 85;

    /// <summary>
    /// Score for auto-approval.
    /// </summary>
    public int AutoApprovalThreshold { get; init; } = 80;

    /// <summary>
    /// Score requiring regeneration.
    /// </summary>
    public int RegenerationThreshold { get; init; } = 50;
}

/// <summary>
/// Retry settings.
/// </summary>
public sealed record RetrySettings
{
    /// <summary>
    /// Max retry attempts.
    /// </summary>
    public int MaxRetries { get; init; } = 3;

    /// <summary>
    /// Delay between retries (seconds).
    /// </summary>
    public int RetryDelaySeconds { get; init; } = 5;

    /// <summary>
    /// Use exponential backoff.
    /// </summary>
    public bool ExponentialBackoff { get; init; } = true;

    /// <summary>
    /// Switch to fallback after failures.
    /// </summary>
    public int SwitchToFallbackAfter { get; init; } = 2;

    /// <summary>
    /// Retry on rate limit.
    /// </summary>
    public bool RetryOnRateLimit { get; init; } = true;

    /// <summary>
    /// Rate limit wait seconds.
    /// </summary>
    public int RateLimitWaitSeconds { get; init; } = 60;
}

/// <summary>
/// Cost limits.
/// </summary>
public sealed record CostLimits
{
    /// <summary>
    /// Maximum cost for entire book.
    /// </summary>
    public decimal MaxTotalCost { get; init; } = 50.00m;

    /// <summary>
    /// Maximum cost per chapter.
    /// </summary>
    public decimal MaxCostPerChapter { get; init; } = 5.00m;

    /// <summary>
    /// Warn at percentage of limit.
    /// </summary>
    public int WarnAtPercentage { get; init; } = 80;

    /// <summary>
    /// Pause at percentage of limit.
    /// </summary>
    public int PauseAtPercentage { get; init; } = 95;

    /// <summary>
    /// Track token usage.
    /// </summary>
    public bool TrackTokenUsage { get; init; } = true;
}

/// <summary>
/// Notification settings.
/// </summary>
public sealed record NotificationSettings
{
    /// <summary>
    /// Notify on chapter completion.
    /// </summary>
    public bool NotifyOnChapterComplete { get; init; } = false;

    /// <summary>
    /// Notify on errors.
    /// </summary>
    public bool NotifyOnError { get; init; } = true;

    /// <summary>
    /// Notify when review needed.
    /// </summary>
    public bool NotifyOnReviewNeeded { get; init; } = true;

    /// <summary>
    /// Notify on completion.
    /// </summary>
    public bool NotifyOnCompletion { get; init; } = true;

    /// <summary>
    /// Sound alerts.
    /// </summary>
    public bool PlaySounds { get; init; } = true;

    /// <summary>
    /// Show desktop notifications.
    /// </summary>
    public bool DesktopNotifications { get; init; } = true;
}

/// <summary>
/// Output settings.
/// </summary>
public sealed record OutputSettings
{
    /// <summary>
    /// Save intermediate drafts.
    /// </summary>
    public bool SaveIntermediateDrafts { get; init; } = true;

    /// <summary>
    /// Draft save location.
    /// </summary>
    public string DraftSaveLocation { get; init; } = "drafts";

    /// <summary>
    /// Export format on completion.
    /// </summary>
    public string ExportFormat { get; init; } = "docx";

    /// <summary>
    /// Include generation metadata.
    /// </summary>
    public bool IncludeMetadata { get; init; } = true;

    /// <summary>
    /// Generate chapter summaries.
    /// </summary>
    public bool GenerateSummaries { get; init; } = true;

    /// <summary>
    /// Generate continuity report.
    /// </summary>
    public bool GenerateContinuityReport { get; init; } = true;
}

/// <summary>
/// Quality gate configuration.
/// </summary>
public sealed record QualityGateConfiguration
{
    /// <summary>
    /// Enable quality gates.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Quality dimensions to check.
    /// </summary>
    public List<QualityDimensionConfig> Dimensions { get; init; } = new();

    /// <summary>
    /// Continuity checks.
    /// </summary>
    public required ContinuityCheckConfig ContinuityChecks { get; init; }

    /// <summary>
    /// Style consistency checks.
    /// </summary>
    public required StyleCheckConfig StyleChecks { get; init; }

    /// <summary>
    /// Auto-revision settings.
    /// </summary>
    public required AutoRevisionConfig AutoRevision { get; init; }
}

/// <summary>
/// Quality dimension configuration.
/// </summary>
public sealed record QualityDimensionConfig
{
    /// <summary>
    /// Dimension name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Weight in overall score.
    /// </summary>
    public double Weight { get; init; } = 1.0;

    /// <summary>
    /// Minimum acceptable score.
    /// </summary>
    public int MinScore { get; init; } = 70;
}

/// <summary>
/// Continuity check configuration.
/// </summary>
public sealed record ContinuityCheckConfig
{
    /// <summary>
    /// Check character consistency.
    /// </summary>
    public bool CheckCharacters { get; init; } = true;

    /// <summary>
    /// Check timeline.
    /// </summary>
    public bool CheckTimeline { get; init; } = true;

    /// <summary>
    /// Check locations.
    /// </summary>
    public bool CheckLocations { get; init; } = true;

    /// <summary>
    /// Check objects/items.
    /// </summary>
    public bool CheckObjects { get; init; } = true;

    /// <summary>
    /// Check plot points.
    /// </summary>
    public bool CheckPlotPoints { get; init; } = true;

    /// <summary>
    /// Fail on critical issues.
    /// </summary>
    public bool FailOnCritical { get; init; } = true;
}

/// <summary>
/// Style check configuration.
/// </summary>
public sealed record StyleCheckConfig
{
    /// <summary>
    /// Check voice consistency.
    /// </summary>
    public bool CheckVoice { get; init; } = true;

    /// <summary>
    /// Check tone.
    /// </summary>
    public bool CheckTone { get; init; } = true;

    /// <summary>
    /// Check pacing.
    /// </summary>
    public bool CheckPacing { get; init; } = true;

    /// <summary>
    /// Check dialogue.
    /// </summary>
    public bool CheckDialogue { get; init; } = true;

    /// <summary>
    /// Check prose style.
    /// </summary>
    public bool CheckProse { get; init; } = true;
}

/// <summary>
/// Auto-revision configuration.
/// </summary>
public sealed record AutoRevisionConfig
{
    /// <summary>
    /// Enable auto-revision.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Max revision attempts.
    /// </summary>
    public int MaxAttempts { get; init; } = 2;

    /// <summary>
    /// Fix minor issues automatically.
    /// </summary>
    public bool FixMinorIssues { get; init; } = true;

    /// <summary>
    /// Fix major issues automatically.
    /// </summary>
    public bool FixMajorIssues { get; init; } = false;

    /// <summary>
    /// Always require approval after revision.
    /// </summary>
    public bool RequireApprovalAfterRevision { get; init; } = false;
}
