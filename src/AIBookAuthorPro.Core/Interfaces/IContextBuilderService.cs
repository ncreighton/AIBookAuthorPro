// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Enums;
using AIBookAuthorPro.Core.Models;
using AIBookAuthorPro.Core.Models.AI;

namespace AIBookAuthorPro.Core.Interfaces;

/// <summary>
/// Defines the contract for building AI generation context from project data.
/// </summary>
public interface IContextBuilderService
{
    /// <summary>
    /// Builds a complete generation context for a chapter.
    /// </summary>
    /// <param name="project">The project.</param>
    /// <param name="chapter">The chapter to generate for.</param>
    /// <param name="options">Context building options.</param>
    /// <returns>Result containing the built context.</returns>
    Result<GenerationContext> BuildChapterContext(
        Project project,
        Chapter chapter,
        ContextBuildOptions? options = null);

    /// <summary>
    /// Builds a system prompt for chapter generation.
    /// </summary>
    /// <param name="context">The generation context.</param>
    /// <returns>The system prompt string.</returns>
    string BuildSystemPrompt(GenerationContext context);

    /// <summary>
    /// Builds a user prompt for chapter generation.
    /// </summary>
    /// <param name="context">The generation context.</param>
    /// <param name="additionalInstructions">Optional additional instructions.</param>
    /// <returns>The user prompt string.</returns>
    string BuildUserPrompt(GenerationContext context, string? additionalInstructions = null);

    /// <summary>
    /// Builds a summary of previous chapters for context.
    /// </summary>
    /// <param name="project">The project.</param>
    /// <param name="upToChapterNumber">Include chapters up to this number.</param>
    /// <param name="maxTokens">Maximum tokens for the summary.</param>
    /// <returns>Result containing the summary.</returns>
    Result<string> BuildPreviousChaptersSummary(
        Project project,
        int upToChapterNumber,
        int maxTokens = 2000);

    /// <summary>
    /// Builds character context for relevant characters.
    /// </summary>
    /// <param name="project">The project.</param>
    /// <param name="characterIds">IDs of characters to include.</param>
    /// <param name="maxTokens">Maximum tokens for character context.</param>
    /// <returns>Result containing character context strings.</returns>
    Result<IReadOnlyList<string>> BuildCharacterContext(
        Project project,
        IEnumerable<Guid> characterIds,
        int maxTokens = 1000);

    /// <summary>
    /// Builds location context for relevant locations.
    /// </summary>
    /// <param name="project">The project.</param>
    /// <param name="locationIds">IDs of locations to include.</param>
    /// <param name="maxTokens">Maximum tokens for location context.</param>
    /// <returns>Result containing location context strings.</returns>
    Result<IReadOnlyList<string>> BuildLocationContext(
        Project project,
        IEnumerable<Guid> locationIds,
        int maxTokens = 500);

    /// <summary>
    /// Estimates the token count for a context.
    /// </summary>
    /// <param name="context">The generation context.</param>
    /// <returns>Estimated token count.</returns>
    int EstimateContextTokens(GenerationContext context);

    /// <summary>
    /// Optimizes context to fit within a token budget.
    /// </summary>
    /// <param name="context">The context to optimize.</param>
    /// <param name="maxTokens">Maximum allowed tokens.</param>
    /// <returns>Result containing the optimized context.</returns>
    Result<GenerationContext> OptimizeForTokenBudget(
        GenerationContext context,
        int maxTokens);
}

/// <summary>
/// Options for building generation context.
/// </summary>
public sealed class ContextBuildOptions
{
    /// <summary>
    /// Gets or sets whether to include previous chapter summary.
    /// </summary>
    public bool IncludePreviousSummary { get; init; } = true;

    /// <summary>
    /// Gets or sets whether to include character context.
    /// </summary>
    public bool IncludeCharacters { get; init; } = true;

    /// <summary>
    /// Gets or sets whether to include location context.
    /// </summary>
    public bool IncludeLocations { get; init; } = true;

    /// <summary>
    /// Gets or sets whether to include outline information.
    /// </summary>
    public bool IncludeOutline { get; init; } = true;

    /// <summary>
    /// Gets or sets whether to include style guide.
    /// </summary>
    public bool IncludeStyleGuide { get; init; } = true;

    /// <summary>
    /// Gets or sets the maximum tokens for previous summary.
    /// </summary>
    public int MaxPreviousSummaryTokens { get; init; } = 2000;

    /// <summary>
    /// Gets or sets the maximum tokens for character context.
    /// </summary>
    public int MaxCharacterTokens { get; init; } = 1000;

    /// <summary>
    /// Gets or sets the maximum tokens for location context.
    /// </summary>
    public int MaxLocationTokens { get; init; } = 500;

    /// <summary>
    /// Gets or sets the maximum total context tokens.
    /// </summary>
    public int MaxTotalContextTokens { get; init; } = 8000;

    /// <summary>
    /// Gets or sets custom character IDs to include (overrides auto-detection).
    /// </summary>
    public IReadOnlyList<Guid>? CustomCharacterIds { get; init; }

    /// <summary>
    /// Gets or sets custom location IDs to include (overrides auto-detection).
    /// </summary>
    public IReadOnlyList<Guid>? CustomLocationIds { get; init; }
}
