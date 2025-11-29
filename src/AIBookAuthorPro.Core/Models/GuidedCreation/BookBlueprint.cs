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
    public Guid SeedPromptId { get; init; }

    /// <summary>
    /// Reference to the analysis result.
    /// </summary>
    public Guid AnalysisResultId { get; init; }

    /// <summary>
    /// Reference to the creative brief.
    /// </summary>
    public Guid CreativeBriefId { get; init; }

    /// <summary>
    /// Alias for CreativeBriefId.
    /// </summary>
    public Guid BriefId { get => CreativeBriefId; init => CreativeBriefId = value; }

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
    public BookIdentity Identity { get; init; } = new();

    // ================== STRUCTURAL PLAN ==================

    /// <summary>
    /// Complete structural plan for the book.
    /// </summary>
    public StructuralPlan Structure { get; init; } = new();

    /// <summary>
    /// Alias for Structure.
    /// </summary>
    public StructuralPlan StructuralPlan { get => Structure; init => Structure = value; }

    // ================== CHARACTER BIBLE ==================

    /// <summary>
    /// Complete character bible.
    /// </summary>
    public CharacterBible Characters { get; init; } = new();

    /// <summary>
    /// Alias for Characters.
    /// </summary>
    public CharacterBible CharacterBible { get => Characters; init => Characters = value; }

    /// <summary>
    /// Convenience property for chapter blueprints (from Structure).
    /// </summary>
    public List<ChapterBlueprint>? ChapterBlueprints => Structure?.Chapters;

    /// <summary>
    /// Alias for ChapterBlueprints.
    /// </summary>
    public List<ChapterBlueprint>? Chapters => ChapterBlueprints;

    // ================== WORLD BIBLE ==================

    /// <summary>
    /// Complete world bible.
    /// </summary>
    public WorldBible World { get; init; } = new();

    // ================== PLOT ARCHITECTURE ==================

    /// <summary>
    /// Complete plot architecture.
    /// </summary>
    public PlotArchitecture Plot { get; init; } = new();

    // ================== STYLE GUIDE ==================

    /// <summary>
    /// Complete style guide.
    /// </summary>
    public StyleGuide Style { get; init; } = new();

    // ================== GENERATION CONFIGURATION ==================

    /// <summary>
    /// Configuration for the generation process.
    /// </summary>
    public GenerationConfiguration GenerationConfig { get; init; } = new();

    /// <summary>
    /// Alias for GenerationConfig.
    /// </summary>
    public GenerationConfiguration Configuration { get => GenerationConfig; init => GenerationConfig = value; }

    // ================== QUALITY GATES ==================

    /// <summary>
    /// Quality gate configuration.
    /// </summary>
    public QualityGateConfiguration QualityGates { get; init; } = new();

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
    public string WorkingTitle { get; init; } = string.Empty;

    /// <summary>
    /// Title alias for WorkingTitle.
    /// </summary>
    public string Title { get => WorkingTitle; init => WorkingTitle = value; }

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
    public string Premise { get; init; } = string.Empty;

    /// <summary>
    /// Expanded premise alias.
    /// </summary>
    public string ExpandedPremise { get => Premise; init => Premise = value; }

    /// <summary>
    /// Target word count.
    /// </summary>
    public int TargetWordCount { get; init; }

    /// <summary>
    /// One-sentence logline.
    /// </summary>
    public string Logline { get; init; } = string.Empty;

    /// <summary>
    /// Back cover blurb.
    /// </summary>
    public string BackCoverBlurb { get; init; } = string.Empty;

    /// <summary>
    /// Extended synopsis.
    /// </summary>
    public string Synopsis { get; init; } = string.Empty;

    /// <summary>
    /// Primary genre.
    /// </summary>
    public string Genre { get; init; } = string.Empty;

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
    public string TargetAudience { get; init; } = string.Empty;

    /// <summary>
    /// Age range classification.
    /// </summary>
    public AudienceAgeRange AgeRange { get; init; } = AudienceAgeRange.Adult;

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
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Book number in series.
    /// </summary>
    public int BookNumber { get; init; }

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
    public int RevisionNumber { get; init; }

    /// <summary>
    /// When revision was made.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// What was changed.
    /// </summary>
    public string ChangeDescription { get; init; } = string.Empty;

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
    public string Section { get; init; } = string.Empty;

    /// <summary>
    /// Specific element ID if applicable.
    /// </summary>
    public Guid? ElementId { get; init; }

    /// <summary>
    /// The note content.
    /// </summary>
    public string Note { get; init; } = string.Empty;

    /// <summary>
    /// When this was created.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Priority/importance.
    /// </summary>
    public ClarificationPriority Priority { get; init; } = ClarificationPriority.Optional;
}
