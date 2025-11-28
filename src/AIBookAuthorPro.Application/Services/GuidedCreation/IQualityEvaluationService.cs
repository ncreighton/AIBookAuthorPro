// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Models.GuidedCreation;

namespace AIBookAuthorPro.Application.Services.GuidedCreation;

/// <summary>
/// Service for evaluating generated content quality across multiple dimensions.
/// </summary>
public interface IQualityEvaluationService
{
    /// <summary>
    /// Performs comprehensive quality evaluation on generated chapter.
    /// </summary>
    /// <param name="chapter">The generated chapter.</param>
    /// <param name="blueprint">The chapter blueprint.</param>
    /// <param name="context">Context from previous chapters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Comprehensive quality report.</returns>
    Task<Result<ComprehensiveQualityReport>> EvaluateChapterAsync(
        GeneratedChapter chapter,
        ChapterBlueprint blueprint,
        QualityEvaluationContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates narrative quality (prose, flow, engagement).
    /// </summary>
    /// <param name="content">The content to evaluate.</param>
    /// <param name="styleGuide">The style guide.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dimension score.</returns>
    Task<Result<DimensionScore>> EvaluateNarrativeQualityAsync(
        string content,
        StyleGuide styleGuide,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates character consistency.
    /// </summary>
    /// <param name="content">The content to evaluate.</param>
    /// <param name="characters">Characters expected in the chapter.</param>
    /// <param name="characterBible">The character bible.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dimension score.</returns>
    Task<Result<DimensionScore>> EvaluateCharacterConsistencyAsync(
        string content,
        List<Guid> characters,
        CharacterBible characterBible,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates plot adherence.
    /// </summary>
    /// <param name="content">The content to evaluate.</param>
    /// <param name="blueprint">The chapter blueprint.</param>
    /// <param name="plotArchitecture">The plot architecture.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dimension score.</returns>
    Task<Result<DimensionScore>> EvaluatePlotAdherenceAsync(
        string content,
        ChapterBlueprint blueprint,
        PlotArchitecture plotArchitecture,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates style consistency.
    /// </summary>
    /// <param name="content">The content to evaluate.</param>
    /// <param name="styleGuide">The style guide.</param>
    /// <param name="previousChapters">Previous chapters for comparison.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dimension score.</returns>
    Task<Result<DimensionScore>> EvaluateStyleConsistencyAsync(
        string content,
        StyleGuide styleGuide,
        List<GeneratedChapter> previousChapters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates pacing quality.
    /// </summary>
    /// <param name="content">The content to evaluate.</param>
    /// <param name="expectedPacing">Expected pacing from blueprint.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dimension score.</returns>
    Task<Result<DimensionScore>> EvaluatePacingAsync(
        string content,
        PacingIntensity expectedPacing,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates dialogue quality.
    /// </summary>
    /// <param name="content">The content to evaluate.</param>
    /// <param name="characters">Characters in the chapter.</param>
    /// <param name="characterBible">The character bible.</param>
    /// <param name="dialogueGuidelines">Dialogue guidelines.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dimension score.</returns>
    Task<Result<DimensionScore>> EvaluateDialogueAsync(
        string content,
        List<Guid> characters,
        CharacterBible characterBible,
        DialogueGuidelines dialogueGuidelines,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates revision instructions based on quality report.
    /// </summary>
    /// <param name="report">The quality report.</param>
    /// <param name="maxInstructions">Maximum number of instructions.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of revision instructions.</returns>
    Task<Result<List<RevisionInstruction>>> GenerateRevisionInstructionsAsync(
        ComprehensiveQualityReport report,
        int maxInstructions = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Auto-fixes minor issues in content.
    /// </summary>
    /// <param name="content">The content to fix.</param>
    /// <param name="issues">Issues to fix.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Fixed content and list of fixes applied.</returns>
    Task<Result<AutoFixResult>> AutoFixIssuesAsync(
        string content,
        List<QualityIssue> issues,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Compares quality between two versions of content.
    /// </summary>
    /// <param name="original">Original content.</param>
    /// <param name="revised">Revised content.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Quality comparison result.</returns>
    Task<Result<QualityComparisonResult>> CompareQualityAsync(
        GeneratedChapter original,
        GeneratedChapter revised,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Context for quality evaluation.
/// </summary>
public sealed record QualityEvaluationContext
{
    /// <summary>
    /// Previous chapters for continuity checking.
    /// </summary>
    public List<GeneratedChapter> PreviousChapters { get; init; } = new();

    /// <summary>
    /// Character bible.
    /// </summary>
    public required CharacterBible CharacterBible { get; init; }

    /// <summary>
    /// World bible.
    /// </summary>
    public required WorldBible WorldBible { get; init; }

    /// <summary>
    /// Plot architecture.
    /// </summary>
    public required PlotArchitecture PlotArchitecture { get; init; }

    /// <summary>
    /// Style guide.
    /// </summary>
    public required StyleGuide StyleGuide { get; init; }

    /// <summary>
    /// Quality thresholds.
    /// </summary>
    public required QualityThresholds Thresholds { get; init; }

    /// <summary>
    /// Character states from previous chapters.
    /// </summary>
    public List<CharacterStateSnapshot> CharacterStates { get; init; } = new();

    /// <summary>
    /// Active plot threads.
    /// </summary>
    public List<PlotThread> ActivePlotThreads { get; init; } = new();
}

/// <summary>
/// Result of auto-fix operation.
/// </summary>
public sealed record AutoFixResult
{
    /// <summary>
    /// Fixed content.
    /// </summary>
    public required string FixedContent { get; init; }

    /// <summary>
    /// Fixes applied.
    /// </summary>
    public List<AppliedFix> FixesApplied { get; init; } = new();

    /// <summary>
    /// Issues that could not be fixed.
    /// </summary>
    public List<QualityIssue> UnfixedIssues { get; init; } = new();

    /// <summary>
    /// Estimated quality improvement.
    /// </summary>
    public double EstimatedImprovement { get; init; }
}

/// <summary>
/// Applied fix.
/// </summary>
public sealed record AppliedFix
{
    /// <summary>
    /// Issue ID.
    /// </summary>
    public required Guid IssueId { get; init; }

    /// <summary>
    /// Original text.
    /// </summary>
    public required string OriginalText { get; init; }

    /// <summary>
    /// Fixed text.
    /// </summary>
    public required string FixedText { get; init; }

    /// <summary>
    /// Reason for fix.
    /// </summary>
    public required string Reason { get; init; }
}

/// <summary>
/// Quality comparison result.
/// </summary>
public sealed record QualityComparisonResult
{
    /// <summary>
    /// Original quality score.
    /// </summary>
    public required double OriginalScore { get; init; }

    /// <summary>
    /// Revised quality score.
    /// </summary>
    public required double RevisedScore { get; init; }

    /// <summary>
    /// Score delta.
    /// </summary>
    public double ScoreDelta => RevisedScore - OriginalScore;

    /// <summary>
    /// Is improvement.
    /// </summary>
    public bool IsImprovement => ScoreDelta > 0;

    /// <summary>
    /// Dimensions improved.
    /// </summary>
    public List<string> DimensionsImproved { get; init; } = new();

    /// <summary>
    /// Dimensions degraded.
    /// </summary>
    public List<string> DimensionsDegraded { get; init; } = new();

    /// <summary>
    /// Issues resolved.
    /// </summary>
    public int IssuesResolved { get; init; }

    /// <summary>
    /// New issues introduced.
    /// </summary>
    public int NewIssuesIntroduced { get; init; }

    /// <summary>
    /// Recommendation.
    /// </summary>
    public required string Recommendation { get; init; }
}
