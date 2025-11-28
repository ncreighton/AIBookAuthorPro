// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

namespace AIBookAuthorPro.Core.Models.GuidedCreation;

/// <summary>
/// Complete architectural blueprint for book generation.
/// This is the master document that orchestrates all content creation.
/// </summary>
public sealed class BookBlueprint
{
    /// <summary>
    /// Unique identifier for this blueprint.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Reference to the seed prompt.
    /// </summary>
    public required Guid SeedPromptId { get; init; }

    /// <summary>
    /// Reference to the analysis result.
    /// </summary>
    public required Guid AnalysisResultId { get; init; }

    /// <summary>
    /// Reference to the creative brief.
    /// </summary>
    public required Guid CreativeBriefId { get; init; }

    /// <summary>
    /// Current status of the blueprint.
    /// </summary>
    public BlueprintStatus Status { get; set; } = BlueprintStatus.Draft;

    /// <summary>
    /// When this blueprint was created.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// When this blueprint was last modified.
    /// </summary>
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this blueprint was approved for generation.
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// Version number for tracking changes.
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// Notes about this version.
    /// </summary>
    public string? VersionNotes { get; set; }

    // ================== BOOK IDENTITY ==================

    /// <summary>
    /// Book identity information.
    /// </summary>
    public required BookIdentity Identity { get; init; }

    // ================== STRUCTURAL PLAN ==================

    /// <summary>
    /// Complete structural plan for the book.
    /// </summary>
    public required StructuralPlan Structure { get; init; }

    // ================== CHARACTER BIBLE ==================

    /// <summary>
    /// Complete character bible.
    /// </summary>
    public required CharacterBible Characters { get; init; }

    // ================== WORLD BIBLE ==================

    /// <summary>
    /// Complete world bible.
    /// </summary>
    public required WorldBible World { get; init; }

    // ================== PLOT ARCHITECTURE ==================

    /// <summary>
    /// Complete plot architecture.
    /// </summary>
    public required PlotArchitecture Plot { get; init; }

    // ================== STYLE GUIDE ==================

    /// <summary>
    /// Complete style guide.
    /// </summary>
    public required StyleGuide Style { get; init; }

    // ================== GENERATION CONFIGURATION ==================

    /// <summary>
    /// Configuration for the generation process.
    /// </summary>
    public required GenerationConfiguration GenerationConfig { get; init; }

    // ================== QUALITY GATES ==================

    /// <summary>
    /// Quality gate configuration.
    /// </summary>
    public required QualityGateConfiguration QualityGates { get; init; }

    // ================== TRACKING ==================

    /// <summary>
    /// History of changes to this blueprint.
    /// </summary>
    public List<BlueprintRevision> RevisionHistory { get; init; } = new();

    /// <summary>
    /// User notes and annotations.
    /// </summary>
    public List<BlueprintAnnotation> Annotations { get; init; } = new();
}

/// <summary>
/// Book identity information.
/// </summary>
public sealed record BookIdentity
{
    /// <summary>
    /// Working title.
    /// </summary>
    public required string WorkingTitle { get; init; }

    /// <summary>
    /// Alternative titles considered.
    /// </summary>
    public List<string> AlternativeTitles { get; init; } = new();

    /// <summary>
    /// Subtitle if any.
    /// </summary>
    public string? Subtitle { get; init; }

    /// <summary>
    /// One-paragraph premise.
    /// </summary>
    public required string Premise { get; init; }

    /// <summary>
    /// One-sentence logline.
    /// </summary>
    public required string Logline { get; init; }

    /// <summary>
    /// Back cover blurb.
    /// </summary>
    public required string BackCoverBlurb { get; init; }

    /// <summary>
    /// Extended synopsis.
    /// </summary>
    public required string Synopsis { get; init; }

    /// <summary>
    /// Primary genre.
    /// </summary>
    public required string Genre { get; init; }

    /// <summary>
    /// Sub-genres.
    /// </summary>
    public List<string> SubGenres { get; init; } = new();

    /// <summary>
    /// Comparable titles.
    /// </summary>
    public List<string> ComparableTitles { get; init; } = new();

    /// <summary>
    /// Marketing keywords.
    /// </summary>
    public List<string> Keywords { get; init; } = new();

    /// <summary>
    /// Target audience description.
    /// </summary>
    public required string TargetAudience { get; init; }

    /// <summary>
    /// Age range classification.
    /// </summary>
    public required AudienceAgeRange AgeRange { get; init; }

    /// <summary>
    /// Content warnings.
    /// </summary>
    public List<string> ContentWarnings { get; init; } = new();

    /// <summary>
    /// Series information if applicable.
    /// </summary>
    public SeriesInfo? Series { get; init; }
}

/// <summary>
/// Series information.
/// </summary>
public sealed record SeriesInfo
{
    /// <summary>
    /// Series name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Book number in series.
    /// </summary>
    public required int BookNumber { get; init; }

    /// <summary>
    /// Planned total books.
    /// </summary>
    public int? PlannedTotalBooks { get; init; }

    /// <summary>
    /// Series arc description.
    /// </summary>
    public string? SeriesArc { get; init; }

    /// <summary>
    /// Previous book summary if not first.
    /// </summary>
    public string? PreviousBookSummary { get; init; }
}

/// <summary>
/// Blueprint revision record.
/// </summary>
public sealed record BlueprintRevision
{
    /// <summary>
    /// Revision number.
    /// </summary>
    public required int RevisionNumber { get; init; }

    /// <summary>
    /// When revision was made.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// What was changed.
    /// </summary>
    public required string ChangeDescription { get; init; }

    /// <summary>
    /// Specific sections modified.
    /// </summary>
    public List<string> SectionsModified { get; init; } = new();

    /// <summary>
    /// Reason for the change.
    /// </summary>
    public string? Reason { get; init; }
}

/// <summary>
/// User annotation on the blueprint.
/// </summary>
public sealed record BlueprintAnnotation
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Section this annotation applies to.
    /// </summary>
    public required string Section { get; init; }

    /// <summary>
    /// Specific element ID if applicable.
    /// </summary>
    public Guid? ElementId { get; init; }

    /// <summary>
    /// The note content.
    /// </summary>
    public required string Note { get; init; }

    /// <summary>
    /// When this was created.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Priority/importance.
    /// </summary>
    public ClarificationPriority Priority { get; init; } = ClarificationPriority.Optional;
}
