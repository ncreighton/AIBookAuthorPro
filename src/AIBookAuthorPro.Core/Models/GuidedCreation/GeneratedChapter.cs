// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

namespace AIBookAuthorPro.Core.Models.GuidedCreation;

/// <summary>
/// Represents a fully generated chapter with content and metadata.
/// </summary>
public sealed class GeneratedChapter
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Reference to chapter blueprint.
    /// </summary>
    public Guid BlueprintChapterId { get; init; }

    /// <summary>
    /// Chapter number.
    /// </summary>
    public int ChapterNumber { get; init; }

    /// <summary>
    /// Chapter title.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Generation status.
    /// </summary>
    public ChapterGenerationStatus Status { get; set; } = ChapterGenerationStatus.Generated;

    /// <summary>
    /// When generated.
    /// </summary>
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// When last modified.
    /// </summary>
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When approved (if approved).
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    // ================== CONTENT ==================

    /// <summary>
    /// The generated content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Content formatted as HTML.
    /// </summary>
    public string? HtmlContent { get; set; }

    /// <summary>
    /// Generated scenes.
    /// </summary>
    public List<GeneratedScene> Scenes { get; init; } = new();

    /// <summary>
    /// Character states at end of chapter.
    /// </summary>
    public List<CharacterStateSnapshot> CharacterStatesAtEnd { get; set; } = new();

    /// <summary>
    /// Generation duration in milliseconds.
    /// </summary>
    public long GenerationDurationMs { get; set; }

    // ================== METRICS ==================

    /// <summary>
    /// Word count.
    /// </summary>
    public int WordCount { get; set; }

    /// <summary>
    /// Character count.
    /// </summary>
    public int CharacterCount { get; set; }

    /// <summary>
    /// Paragraph count.
    /// </summary>
    public int ParagraphCount { get; set; }

    /// <summary>
    /// Dialogue percentage.
    /// </summary>
    public double DialoguePercentage { get; set; }

    /// <summary>
    /// Average sentence length.
    /// </summary>
    public double AverageSentenceLength { get; set; }

    /// <summary>
    /// Reading time estimate (minutes).
    /// </summary>
    public double EstimatedReadingTimeMinutes => WordCount / 250.0;

    // ================== QUALITY ==================

    /// <summary>
    /// Quality report.
    /// </summary>
    public ComprehensiveQualityReport? QualityReport { get; set; }

    /// <summary>
    /// Overall quality score.
    /// </summary>
    public double? QualityScore => QualityReport?.OverallScore;

    /// <summary>
    /// Continuity report.
    /// </summary>
    public ContinuityReport? ContinuityReport { get; set; }

    /// <summary>
    /// Chapter summaries at different levels.
    /// </summary>
    public ChapterSummaries? Summaries { get; set; }

    // ================== GENERATION METADATA ==================

    /// <summary>
    /// Generation attempt number.
    /// </summary>
    public int GenerationAttempt { get; init; } = 1;

    /// <summary>
    /// Number of revisions applied.
    /// </summary>
    public int RevisionCount { get; set; }

    /// <summary>
    /// Model used.
    /// </summary>
    public string ModelUsed { get; init; } = string.Empty;

    /// <summary>
    /// Token usage.
    /// </summary>
    public TokenUsage TokenUsage { get; init; } = new();

    /// <summary>
    /// Generation cost.
    /// </summary>
    public decimal GenerationCost { get; init; }

    /// <summary>
    /// Generation duration.
    /// </summary>
    public TimeSpan GenerationDuration { get; init; }

    /// <summary>
    /// Context used for generation.
    /// </summary>
    public GenerationContextSummary? ContextUsed { get; init; }

    /// <summary>
    /// Failure reason if generation failed.
    /// </summary>
    public string? FailureReason { get; set; }

    // ================== REVISIONS ==================

    /// <summary>
    /// Revision history.
    /// </summary>
    public List<ChapterRevision> Revisions { get; init; } = new();

    /// <summary>
    /// Current version.
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// Has been manually edited.
    /// </summary>
    public bool ManuallyEdited { get; set; }

    // ================== SUMMARIES ==================

    /// <summary>
    /// AI-generated summary.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Compressed context for future chapters.
    /// </summary>
    public string? CompressedContext { get; set; }

    /// <summary>
    /// Key events that occurred.
    /// </summary>
    public List<string> KeyEvents { get; init; } = new();

    /// <summary>
    /// Character states at end of chapter.
    /// </summary>
    public List<CharacterStateSnapshot> CharacterStates { get; init; } = new();

    // ================== USER FEEDBACK ==================

    /// <summary>
    /// User notes.
    /// </summary>
    public List<string> UserNotes { get; init; } = new();

    /// <summary>
    /// User rating (1-5).
    /// </summary>
    public int? UserRating { get; set; }

    /// <summary>
    /// Is locked (cannot be regenerated).
    /// </summary>
    public bool IsLocked { get; set; }
}

/// <summary>
/// Generated scene.
/// </summary>
public sealed class GeneratedScene
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Reference to scene blueprint.
    /// </summary>
    public Guid BlueprintSceneId { get; init; }

    /// <summary>
    /// Scene number.
    /// </summary>
    public int SceneNumber { get; init; }

    /// <summary>
    /// Content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Word count.
    /// </summary>
    public int WordCount { get; set; }

    /// <summary>
    /// Start position in chapter.
    /// </summary>
    public int StartPosition { get; init; }

    /// <summary>
    /// End position in chapter.
    /// </summary>
    public int EndPosition { get; init; }
}

/// <summary>
/// Chapter revision.
/// </summary>
public sealed record ChapterRevision
{
    /// <summary>
    /// Revision number.
    /// </summary>
    public int RevisionNumber { get; init; }

    /// <summary>
    /// Timestamp.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Revision type (auto, manual, AI).
    /// </summary>
    public string RevisionType { get; init; } = "auto";

    /// <summary>
    /// What was changed.
    /// </summary>
    public string ChangeDescription { get; init; } = string.Empty;

    /// <summary>
    /// Previous content (for undo).
    /// </summary>
    public string? PreviousContent { get; init; }

    /// <summary>
    /// Issues addressed.
    /// </summary>
    public List<string> IssuesAddressed { get; init; } = new();

    /// <summary>
    /// Quality delta.
    /// </summary>
    public double? QualityDelta { get; init; }
}

/// <summary>
/// Generation context summary.
/// </summary>
public sealed record GenerationContextSummary
{
    /// <summary>
    /// Previous chapters included.
    /// </summary>
    public List<int> PreviousChaptersIncluded { get; init; } = new();

    /// <summary>
    /// Characters in context.
    /// </summary>
    public List<string> CharactersInContext { get; init; } = new();

    /// <summary>
    /// Locations in context.
    /// </summary>
    public List<string> LocationsInContext { get; init; } = new();

    /// <summary>
    /// Plot threads in context.
    /// </summary>
    public List<string> PlotThreadsInContext { get; init; } = new();

    /// <summary>
    /// Total context tokens.
    /// </summary>
    public int TotalContextTokens { get; init; }
}

/// <summary>
/// Character state snapshot.
/// </summary>
public sealed record CharacterStateSnapshot
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
    /// Emotional state.
    /// </summary>
    public string EmotionalState { get; init; } = string.Empty;

    /// <summary>
    /// Physical location.
    /// </summary>
    public string Location { get; init; } = string.Empty;

    /// <summary>
    /// Key knowledge gained.
    /// </summary>
    public List<string> KnowledgeGained { get; init; } = new();

    /// <summary>
    /// Relationships changed.
    /// </summary>
    public List<string> RelationshipChanges { get; init; } = new();

    /// <summary>
    /// Arc progress.
    /// </summary>
    public int ArcProgress { get; init; }
}
