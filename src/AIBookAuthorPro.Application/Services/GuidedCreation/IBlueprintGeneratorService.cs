// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Models.GuidedCreation;

namespace AIBookAuthorPro.Application.Services.GuidedCreation;

/// <summary>
/// Service for generating comprehensive book blueprints from creative briefs.
/// </summary>
public interface IBlueprintGeneratorService
{
    /// <summary>
    /// Generates a complete book blueprint from a creative brief.
    /// </summary>
    /// <param name="brief">The expanded creative brief.</param>
    /// <param name="progress">Progress reporter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Complete book blueprint.</returns>
    Task<Result<BookBlueprint>> GenerateBlueprintAsync(
        ExpandedCreativeBrief brief,
        IProgress<DetailedBlueprintProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates the structural plan (acts, chapters, scenes).
    /// </summary>
    /// <param name="brief">The creative brief.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Structural plan.</returns>
    Task<Result<StructuralPlan>> GenerateStructuralPlanAsync(
        ExpandedCreativeBrief brief,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates the character bible.
    /// </summary>
    /// <param name="brief">The creative brief.</param>
    /// <param name="structure">The structural plan for character placement.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Character bible.</returns>
    Task<Result<CharacterBible>> GenerateCharacterBibleAsync(
        ExpandedCreativeBrief brief,
        StructuralPlan structure,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates the world bible.
    /// </summary>
    /// <param name="brief">The creative brief.</param>
    /// <param name="structure">The structural plan.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>World bible.</returns>
    Task<Result<WorldBible>> GenerateWorldBibleAsync(
        ExpandedCreativeBrief brief,
        StructuralPlan structure,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates the plot architecture.
    /// </summary>
    /// <param name="brief">The creative brief.</param>
    /// <param name="structure">The structural plan.</param>
    /// <param name="characters">The character bible.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Plot architecture.</returns>
    Task<Result<PlotArchitecture>> GeneratePlotArchitectureAsync(
        ExpandedCreativeBrief brief,
        StructuralPlan structure,
        CharacterBible characters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates the style guide.
    /// </summary>
    /// <param name="brief">The creative brief.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Style guide.</returns>
    Task<Result<StyleGuide>> GenerateStyleGuideAsync(
        ExpandedCreativeBrief brief,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates detailed chapter blueprints.
    /// </summary>
    /// <param name="structure">The structural plan.</param>
    /// <param name="plot">The plot architecture.</param>
    /// <param name="characters">The character bible.</param>
    /// <param name="world">The world bible.</param>
    /// <param name="style">The style guide.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of chapter blueprints.</returns>
    Task<Result<List<ChapterBlueprint>>> GenerateChapterBlueprintsAsync(
        StructuralPlan structure,
        PlotArchitecture plot,
        CharacterBible characters,
        WorldBible world,
        StyleGuide style,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Regenerates a specific section of the blueprint.
    /// </summary>
    /// <param name="blueprint">The current blueprint.</param>
    /// <param name="section">The section to regenerate.</param>
    /// <param name="instructions">Optional instructions for regeneration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated blueprint.</returns>
    Task<Result<BookBlueprint>> RegenerateSectionAsync(
        BookBlueprint blueprint,
        BlueprintSection section,
        string? instructions = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a blueprint for completeness and consistency.
    /// </summary>
    /// <param name="blueprint">The blueprint to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result.</returns>
    Task<Result<BlueprintValidationResult>> ValidateBlueprintAsync(
        BookBlueprint blueprint,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Detailed blueprint generation progress for internal orchestration.
/// </summary>
/// <remarks>
/// This is distinct from Core.Models.GuidedCreation.BlueprintGenerationProgress
/// which is used for external API progress reporting.
/// </remarks>
public sealed record DetailedBlueprintProgress
{
    /// <summary>
    /// Current phase.
    /// </summary>
    public required BlueprintGenerationPhase Phase { get; init; }

    /// <summary>
    /// Phase progress (0-100).
    /// </summary>
    public required double PhaseProgress { get; init; }

    /// <summary>
    /// Overall progress (0-100).
    /// </summary>
    public required double OverallProgress { get; init; }

    /// <summary>
    /// Current operation.
    /// </summary>
    public required string CurrentOperation { get; init; }

    /// <summary>
    /// Estimated time remaining.
    /// </summary>
    public TimeSpan? EstimatedTimeRemaining { get; init; }
}

/// <summary>
/// Blueprint generation phases.
/// </summary>
public enum BlueprintGenerationPhase
{
    /// <summary>Generating structural plan.</summary>
    Structure,
    /// <summary>Generating characters.</summary>
    Characters,
    /// <summary>Generating world.</summary>
    World,
    /// <summary>Generating plot.</summary>
    Plot,
    /// <summary>Generating style guide.</summary>
    Style,
    /// <summary>Generating chapter details.</summary>
    ChapterDetails,
    /// <summary>Validating blueprint.</summary>
    Validation,
    /// <summary>Finalizing.</summary>
    Finalization
}

/// <summary>
/// Blueprint section for regeneration.
/// </summary>
public enum BlueprintSection
{
    /// <summary>Book identity.</summary>
    Identity,
    /// <summary>Structural plan.</summary>
    Structure,
    /// <summary>Character bible.</summary>
    Characters,
    /// <summary>World bible.</summary>
    World,
    /// <summary>Plot architecture.</summary>
    Plot,
    /// <summary>Style guide.</summary>
    Style,
    /// <summary>Specific chapter.</summary>
    Chapter,
    /// <summary>Specific character.</summary>
    SpecificCharacter,
    /// <summary>Specific location.</summary>
    SpecificLocation
}

/// <summary>
/// Blueprint validation result.
/// </summary>
public sealed record BlueprintValidationResult
{
    /// <summary>
    /// Whether the blueprint is valid.
    /// </summary>
    public required bool IsValid { get; init; }

    /// <summary>
    /// Overall completeness score (0-100).
    /// </summary>
    public required int CompletenessScore { get; init; }

    /// <summary>
    /// Consistency score (0-100).
    /// </summary>
    public required int ConsistencyScore { get; init; }

    /// <summary>
    /// Validation issues.
    /// </summary>
    public List<BlueprintValidationIssue> Issues { get; init; } = new();

    /// <summary>
    /// Warnings.
    /// </summary>
    public List<string> Warnings { get; init; } = new();

    /// <summary>
    /// Missing elements.
    /// </summary>
    public List<string> MissingElements { get; init; } = new();

    /// <summary>
    /// Inconsistencies found.
    /// </summary>
    public List<string> Inconsistencies { get; init; } = new();
}

/// <summary>
/// Blueprint validation issue.
/// </summary>
public sealed record BlueprintValidationIssue
{
    /// <summary>
    /// Section with issue.
    /// </summary>
    public required string Section { get; init; }

    /// <summary>
    /// Issue description.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Severity.
    /// </summary>
    public required ClarificationPriority Severity { get; init; }

    /// <summary>
    /// Suggested fix.
    /// </summary>
    public string? SuggestedFix { get; init; }
}
