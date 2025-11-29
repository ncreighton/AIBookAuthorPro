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
    public Guid ChapterId { get; init; }

    /// <summary>
    /// Chapter number.
    /// </summary>
    public int ChapterNumber { get; init; }

    /// <summary>
    /// When evaluated.
    /// </summary>
    public DateTime EvaluatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Overall score (0-100).
    /// </summary>
    public double OverallScore { get; init; }

    /// <summary>
    /// Verdict.
    /// </summary>
    public QualityVerdict Verdict { get; init; } = QualityVerdict.NeedsWork;

    // ================== DIMENSION SCORES ==================

    /// <summary>
    /// Narrative quality score.
    /// </summary>
    public DimensionScore NarrativeQuality { get; init; } = new();

    /// <summary>
    /// Character consistency score.
    /// </summary>
    public DimensionScore CharacterConsistency { get; init; } = new();

    /// <summary>
    /// Plot adherence score.
    /// </summary>
    public DimensionScore PlotAdherence { get; init; } = new();

    /// <summary>
    /// Style consistency score.
    /// </summary>
    public DimensionScore StyleConsistency { get; init; } = new();

    /// <summary>
    /// Pacing quality score.
    /// </summary>
    public DimensionScore PacingQuality { get; init; } = new();

    /// <summary>
    /// Dialogue quality score.
    /// </summary>
    public DimensionScore DialogueQuality { get; init; } = new();

    /// <summary>
    /// Description quality score.
    /// </summary>
    public DimensionScore DescriptionQuality { get; init; } = new();

    /// <summary>
    /// Emotional impact score.
    /// </summary>
    public DimensionScore EmotionalImpact { get; init; } = new();

    /// <summary>
    /// Continuity accuracy score.
    /// </summary>
    public DimensionScore ContinuityAccuracy { get; init; } = new();

    /// <summary>
    /// Readability score.
    /// </summary>
    public DimensionScore ReadabilityScore { get; init; } = new();

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

    /// <summary>
    /// Total issue count.
    /// </summary>
    public int TotalIssueCount => Issues.Count;

    /// <summary>
    /// Readability alias for ReadabilityScore.
    /// </summary>
    public DimensionScore Readability { get => ReadabilityScore; init => ReadabilityScore = value; }

    // ================== SUGGESTIONS ==================

    /// <summary>
    /// Improvement suggestions.
    /// </summary>
    public List<ImprovementSuggestion> Suggestions { get; init; } = new();

    /// <summary>
    /// Alias for Suggestions.
    /// </summary>
    public List<ImprovementSuggestion> ImprovementSuggestions { get => Suggestions; init => Suggestions = value; }

    // ================== AUTO-REVISION ==================

    /// <summary>
    /// Whether auto-revision is recommended.
    /// </summary>
    public bool RecommendAutoRevision { get; init; }

    /// <summary>
    /// Alias for RecommendAutoRevision.
    /// </summary>
    public bool AutoRevisionRecommended { get => RecommendAutoRevision; init => RecommendAutoRevision = value; }

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
    public string Dimension { get; init; } = string.Empty;

    /// <summary>
    /// Alias for Dimension.
    /// </summary>
    public string DimensionName { get => Dimension; init => Dimension = value; }

    /// <summary>
    /// Score (0-100).
    /// </summary>
    public double Score { get; init; }

    /// <summary>
    /// Weight in overall score.
    /// </summary>
    public double Weight { get; init; } = 1.0;

    /// <summary>
    /// Explanation.
    /// </summary>
    public string Explanation { get; init; } = string.Empty;

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
    public QualityIssueSeverity Severity { get; init; } = QualityIssueSeverity.Minor;

    /// <summary>
    /// Category.
    /// </summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>
    /// Subcategory.
    /// </summary>
    public string? Subcategory { get; init; }

    /// <summary>
    /// Description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Location in text.
    /// </summary>
    public string Location { get; init; } = string.Empty;

    /// <summary>
    /// Alias for Location.
    /// </summary>
    public string LocationInText { get => Location; init => Location = value; }

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
    public string Category { get; init; } = string.Empty;

    /// <summary>
    /// Suggestion.
    /// </summary>
    public string Suggestion { get; init; } = string.Empty;

    /// <summary>
    /// Expected impact.
    /// </summary>
    public string ExpectedImpact { get; init; } = string.Empty;

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
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Target location.
    /// </summary>
    public string TargetLocation { get; init; } = string.Empty;

    /// <summary>
    /// Current text.
    /// </summary>
    public string? CurrentText { get; init; }

    /// <summary>
    /// Instruction.
    /// </summary>
    public string Instruction { get; init; } = string.Empty;

    /// <summary>
    /// Suggested replacement.
    /// </summary>
    public string? SuggestedReplacement { get; init; }

    /// <summary>
    /// Priority.
    /// </summary>
    public int Priority { get; init; }

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
    public Guid ChapterId { get; init; }

    /// <summary>
    /// Chapter number.
    /// </summary>
    public int ChapterNumber { get; init; }

    /// <summary>
    /// Whether passes check.
    /// </summary>
    public bool PassesContinuityCheck { get; init; }

    /// <summary>
    /// Continuity score (0-100).
    /// </summary>
    public double ContinuityScore { get; init; }

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
    public Guid CharacterId { get; init; }

    /// <summary>
    /// Character name.
    /// </summary>
    public string CharacterName { get; init; } = string.Empty;

    /// <summary>
    /// Issue type.
    /// </summary>
    public ContinuityIssueType Type { get; init; } = ContinuityIssueType.CharacterBehavior;

    /// <summary>
    /// Severity.
    /// </summary>
    public QualityIssueSeverity Severity { get; init; } = QualityIssueSeverity.Minor;

    /// <summary>
    /// Description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Expected behavior/state.
    /// </summary>
    public string Expected { get; init; } = string.Empty;

    /// <summary>
    /// Actual behavior/state.
    /// </summary>
    public string Actual { get; init; } = string.Empty;

    /// <summary>
    /// Location in text.
    /// </summary>
    public string LocationInText { get; init; } = string.Empty;

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
    public ContinuityIssueType Type { get; init; } = ContinuityIssueType.CharacterBehavior;

    /// <summary>
    /// Severity.
    /// </summary>
    public QualityIssueSeverity Severity { get; init; } = QualityIssueSeverity.Minor;

    /// <summary>
    /// Description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Location in text.
    /// </summary>
    public string LocationInText { get; init; } = string.Empty;

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
    public ContinuityIssueType Type { get; init; } = ContinuityIssueType.CharacterBehavior;

    /// <summary>
    /// Severity.
    /// </summary>
    public QualityIssueSeverity Severity { get; init; } = QualityIssueSeverity.Minor;

    /// <summary>
    /// Description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Expected timeline.
    /// </summary>
    public string ExpectedTimeline { get; init; } = string.Empty;

    /// <summary>
    /// Actual timeline in text.
    /// </summary>
    public string ActualTimeline { get; init; } = string.Empty;

    /// <summary>
    /// Location in text.
    /// </summary>
    public string LocationInText { get; init; } = string.Empty;

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
    public string LocationName { get; init; } = string.Empty;

    /// <summary>
    /// Issue type.
    /// </summary>
    public ContinuityIssueType Type { get; init; } = ContinuityIssueType.CharacterBehavior;

    /// <summary>
    /// Severity.
    /// </summary>
    public QualityIssueSeverity Severity { get; init; } = QualityIssueSeverity.Minor;

    /// <summary>
    /// Description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Expected detail.
    /// </summary>
    public string Expected { get; init; } = string.Empty;

    /// <summary>
    /// Actual detail.
    /// </summary>
    public string Actual { get; init; } = string.Empty;

    /// <summary>
    /// Location in text.
    /// </summary>
    public string LocationInText { get; init; } = string.Empty;

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
    public string ObjectName { get; init; } = string.Empty;

    /// <summary>
    /// Issue type.
    /// </summary>
    public ContinuityIssueType Type { get; init; } = ContinuityIssueType.CharacterBehavior;

    /// <summary>
    /// Severity.
    /// </summary>
    public QualityIssueSeverity Severity { get; init; } = QualityIssueSeverity.Minor;

    /// <summary>
    /// Description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Expected state.
    /// </summary>
    public string ExpectedState { get; init; } = string.Empty;

    /// <summary>
    /// Actual state.
    /// </summary>
    public string ActualState { get; init; } = string.Empty;

    /// <summary>
    /// Location in text.
    /// </summary>
    public string LocationInText { get; init; } = string.Empty;

    /// <summary>
    /// Source chapter.
    /// </summary>
    public int? SourceChapter { get; init; }

    /// <summary>
    /// Suggested fix.
    /// </summary>
    public string? SuggestedFix { get; init; }
}
