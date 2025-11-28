// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

namespace AIBookAuthorPro.Core.Models.GuidedCreation;

/// <summary>
/// Comprehensive quality report for a chapter.
/// </summary>
public sealed record ComprehensiveQualityReport
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Chapter ID.
    /// </summary>
    public required Guid ChapterId { get; init; }

    /// <summary>
    /// Chapter number.
    /// </summary>
    public required int ChapterNumber { get; init; }

    /// <summary>
    /// When evaluated.
    /// </summary>
    public DateTime EvaluatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Overall score (0-100).
    /// </summary>
    public required double OverallScore { get; init; }

    /// <summary>
    /// Verdict.
    /// </summary>
    public required QualityVerdict Verdict { get; init; }

    // ================== DIMENSION SCORES ==================

    /// <summary>
    /// Narrative quality score.
    /// </summary>
    public required DimensionScore NarrativeQuality { get; init; }

    /// <summary>
    /// Character consistency score.
    /// </summary>
    public required DimensionScore CharacterConsistency { get; init; }

    /// <summary>
    /// Plot adherence score.
    /// </summary>
    public required DimensionScore PlotAdherence { get; init; }

    /// <summary>
    /// Style consistency score.
    /// </summary>
    public required DimensionScore StyleConsistency { get; init; }

    /// <summary>
    /// Pacing quality score.
    /// </summary>
    public required DimensionScore PacingQuality { get; init; }

    /// <summary>
    /// Dialogue quality score.
    /// </summary>
    public required DimensionScore DialogueQuality { get; init; }

    /// <summary>
    /// Description quality score.
    /// </summary>
    public required DimensionScore DescriptionQuality { get; init; }

    /// <summary>
    /// Emotional impact score.
    /// </summary>
    public required DimensionScore EmotionalImpact { get; init; }

    /// <summary>
    /// Continuity accuracy score.
    /// </summary>
    public required DimensionScore ContinuityAccuracy { get; init; }

    /// <summary>
    /// Readability score.
    /// </summary>
    public required DimensionScore ReadabilityScore { get; init; }

    // ================== ISSUES ==================

    /// <summary>
    /// All issues found.
    /// </summary>
    public List<QualityIssue> Issues { get; init; } = new();

    /// <summary>
    /// Critical issue count.
    /// </summary>
    public int CriticalIssueCount => Issues.Count(i => i.Severity == QualityIssueSeverity.Critical);

    /// <summary>
    /// Major issue count.
    /// </summary>
    public int MajorIssueCount => Issues.Count(i => i.Severity == QualityIssueSeverity.Major);

    /// <summary>
    /// Minor issue count.
    /// </summary>
    public int MinorIssueCount => Issues.Count(i => i.Severity == QualityIssueSeverity.Minor);

    // ================== SUGGESTIONS ==================

    /// <summary>
    /// Improvement suggestions.
    /// </summary>
    public List<ImprovementSuggestion> Suggestions { get; init; } = new();

    // ================== AUTO-REVISION ==================

    /// <summary>
    /// Whether auto-revision is recommended.
    /// </summary>
    public required bool RecommendAutoRevision { get; init; }

    /// <summary>
    /// Revision instructions if auto-revision recommended.
    /// </summary>
    public List<RevisionInstruction> RevisionInstructions { get; init; } = new();

    /// <summary>
    /// Estimated quality improvement from revision.
    /// </summary>
    public double? EstimatedQualityImprovement { get; init; }
}

/// <summary>
/// Score for a quality dimension.
/// </summary>
public sealed record DimensionScore
{
    /// <summary>
    /// Dimension name.
    /// </summary>
    public required string Dimension { get; init; }

    /// <summary>
    /// Score (0-100).
    /// </summary>
    public required double Score { get; init; }

    /// <summary>
    /// Weight in overall score.
    /// </summary>
    public double Weight { get; init; } = 1.0;

    /// <summary>
    /// Explanation.
    /// </summary>
    public required string Explanation { get; init; }

    /// <summary>
    /// Strengths identified.
    /// </summary>
    public List<string> Strengths { get; init; } = new();

    /// <summary>
    /// Weaknesses identified.
    /// </summary>
    public List<string> Weaknesses { get; init; } = new();

    /// <summary>
    /// Specific examples.
    /// </summary>
    public List<string> Examples { get; init; } = new();
}

/// <summary>
/// Quality issue.
/// </summary>
public sealed record QualityIssue
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Severity.
    /// </summary>
    public required QualityIssueSeverity Severity { get; init; }

    /// <summary>
    /// Category.
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// Subcategory.
    /// </summary>
    public string? Subcategory { get; init; }

    /// <summary>
    /// Description.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Location in text.
    /// </summary>
    public required string Location { get; init; }

    /// <summary>
    /// Problematic text excerpt.
    /// </summary>
    public string? TextExcerpt { get; init; }

    /// <summary>
    /// Suggested fix.
    /// </summary>
    public string? SuggestedFix { get; init; }

    /// <summary>
    /// Whether this can be auto-fixed.
    /// </summary>
    public bool AutoFixable { get; init; }

    /// <summary>
    /// Whether this was auto-fixed.
    /// </summary>
    public bool WasAutoFixed { get; set; }

    /// <summary>
    /// Impact on score.
    /// </summary>
    public double ScoreImpact { get; init; }
}

/// <summary>
/// Improvement suggestion.
/// </summary>
public sealed record ImprovementSuggestion
{
    /// <summary>
    /// Category.
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// Suggestion.
    /// </summary>
    public required string Suggestion { get; init; }

    /// <summary>
    /// Expected impact.
    /// </summary>
    public required string ExpectedImpact { get; init; }

    /// <summary>
    /// Priority (1-10).
    /// </summary>
    public int Priority { get; init; }

    /// <summary>
    /// Example if applicable.
    /// </summary>
    public string? Example { get; init; }
}

/// <summary>
/// Revision instruction.
/// </summary>
public sealed record RevisionInstruction
{
    /// <summary>
    /// Instruction type.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Target location.
    /// </summary>
    public required string TargetLocation { get; init; }

    /// <summary>
    /// Current text.
    /// </summary>
    public string? CurrentText { get; init; }

    /// <summary>
    /// Instruction.
    /// </summary>
    public required string Instruction { get; init; }

    /// <summary>
    /// Suggested replacement.
    /// </summary>
    public string? SuggestedReplacement { get; init; }

    /// <summary>
    /// Priority.
    /// </summary>
    public required int Priority { get; init; }

    /// <summary>
    /// Issues this addresses.
    /// </summary>
    public List<Guid> AddressesIssues { get; init; } = new();
}

/// <summary>
/// Continuity report.
/// </summary>
public sealed record ContinuityReport
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Chapter ID.
    /// </summary>
    public required Guid ChapterId { get; init; }

    /// <summary>
    /// Chapter number.
    /// </summary>
    public required int ChapterNumber { get; init; }

    /// <summary>
    /// Whether passes check.
    /// </summary>
    public required bool PassesContinuityCheck { get; init; }

    /// <summary>
    /// Continuity score (0-100).
    /// </summary>
    public required double ContinuityScore { get; init; }

    /// <summary>
    /// Checked at.
    /// </summary>
    public DateTime CheckedAt { get; init; } = DateTime.UtcNow;

    // ================== ISSUE CATEGORIES ==================

    /// <summary>
    /// Character continuity issues.
    /// </summary>
    public List<CharacterContinuityIssue> CharacterIssues { get; init; } = new();

    /// <summary>
    /// Plot continuity issues.
    /// </summary>
    public List<PlotContinuityIssue> PlotIssues { get; init; } = new();

    /// <summary>
    /// Timeline continuity issues.
    /// </summary>
    public List<TimelineContinuityIssue> TimelineIssues { get; init; } = new();

    /// <summary>
    /// Setting continuity issues.
    /// </summary>
    public List<SettingContinuityIssue> SettingIssues { get; init; } = new();

    /// <summary>
    /// Object tracking issues.
    /// </summary>
    public List<ObjectContinuityIssue> ObjectIssues { get; init; } = new();

    /// <summary>
    /// Total issue count.
    /// </summary>
    public int TotalIssueCount => 
        CharacterIssues.Count + PlotIssues.Count + TimelineIssues.Count + 
        SettingIssues.Count + ObjectIssues.Count;

    /// <summary>
    /// Critical issue count.
    /// </summary>
    public int CriticalIssueCount { get; init; }
}

/// <summary>
/// Character continuity issue.
/// </summary>
public sealed record CharacterContinuityIssue
{
    /// <summary>
    /// Character ID.
    /// </summary>
    public required Guid CharacterId { get; init; }

    /// <summary>
    /// Character name.
    /// </summary>
    public required string CharacterName { get; init; }

    /// <summary>
    /// Issue type.
    /// </summary>
    public required ContinuityIssueType Type { get; init; }

    /// <summary>
    /// Severity.
    /// </summary>
    public required QualityIssueSeverity Severity { get; init; }

    /// <summary>
    /// Description.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Expected behavior/state.
    /// </summary>
    public required string Expected { get; init; }

    /// <summary>
    /// Actual behavior/state.
    /// </summary>
    public required string Actual { get; init; }

    /// <summary>
    /// Location in text.
    /// </summary>
    public required string LocationInText { get; init; }

    /// <summary>
    /// Source chapter (where expectation was set).
    /// </summary>
    public int? SourceChapter { get; init; }

    /// <summary>
    /// Suggested fix.
    /// </summary>
    public string? SuggestedFix { get; init; }
}

/// <summary>
/// Plot continuity issue.
/// </summary>
public sealed record PlotContinuityIssue
{
    /// <summary>
    /// Plot thread ID if applicable.
    /// </summary>
    public Guid? PlotThreadId { get; init; }

    /// <summary>
    /// Issue type.
    /// </summary>
    public required ContinuityIssueType Type { get; init; }

    /// <summary>
    /// Severity.
    /// </summary>
    public required QualityIssueSeverity Severity { get; init; }

    /// <summary>
    /// Description.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Location in text.
    /// </summary>
    public required string LocationInText { get; init; }

    /// <summary>
    /// Contradiction with.
    /// </summary>
    public string? ContradictionWith { get; init; }

    /// <summary>
    /// Source chapter.
    /// </summary>
    public int? SourceChapter { get; init; }

    /// <summary>
    /// Suggested fix.
    /// </summary>
    public string? SuggestedFix { get; init; }
}

/// <summary>
/// Timeline continuity issue.
/// </summary>
public sealed record TimelineContinuityIssue
{
    /// <summary>
    /// Issue type.
    /// </summary>
    public required ContinuityIssueType Type { get; init; }

    /// <summary>
    /// Severity.
    /// </summary>
    public required QualityIssueSeverity Severity { get; init; }

    /// <summary>
    /// Description.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Expected timeline.
    /// </summary>
    public required string ExpectedTimeline { get; init; }

    /// <summary>
    /// Actual timeline in text.
    /// </summary>
    public required string ActualTimeline { get; init; }

    /// <summary>
    /// Location in text.
    /// </summary>
    public required string LocationInText { get; init; }

    /// <summary>
    /// Suggested fix.
    /// </summary>
    public string? SuggestedFix { get; init; }
}

/// <summary>
/// Setting continuity issue.
/// </summary>
public sealed record SettingContinuityIssue
{
    /// <summary>
    /// Location ID if applicable.
    /// </summary>
    public Guid? LocationId { get; init; }

    /// <summary>
    /// Location name.
    /// </summary>
    public required string LocationName { get; init; }

    /// <summary>
    /// Issue type.
    /// </summary>
    public required ContinuityIssueType Type { get; init; }

    /// <summary>
    /// Severity.
    /// </summary>
    public required QualityIssueSeverity Severity { get; init; }

    /// <summary>
    /// Description.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Expected detail.
    /// </summary>
    public required string Expected { get; init; }

    /// <summary>
    /// Actual detail.
    /// </summary>
    public required string Actual { get; init; }

    /// <summary>
    /// Location in text.
    /// </summary>
    public required string LocationInText { get; init; }

    /// <summary>
    /// Suggested fix.
    /// </summary>
    public string? SuggestedFix { get; init; }
}

/// <summary>
/// Object/item continuity issue.
/// </summary>
public sealed record ObjectContinuityIssue
{
    /// <summary>
    /// Object name.
    /// </summary>
    public required string ObjectName { get; init; }

    /// <summary>
    /// Issue type.
    /// </summary>
    public required ContinuityIssueType Type { get; init; }

    /// <summary>
    /// Severity.
    /// </summary>
    public required QualityIssueSeverity Severity { get; init; }

    /// <summary>
    /// Description.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Expected state.
    /// </summary>
    public required string ExpectedState { get; init; }

    /// <summary>
    /// Actual state.
    /// </summary>
    public required string ActualState { get; init; }

    /// <summary>
    /// Location in text.
    /// </summary>
    public required string LocationInText { get; init; }

    /// <summary>
    /// Source chapter.
    /// </summary>
    public int? SourceChapter { get; init; }

    /// <summary>
    /// Suggested fix.
    /// </summary>
    public string? SuggestedFix { get; init; }
}
