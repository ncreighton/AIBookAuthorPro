// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Models.GuidedCreation;

namespace AIBookAuthorPro.Application.Services.GuidedCreation;

/// <summary>
/// Service for building comprehensive context for AI generation.
/// </summary>
public interface IGenerationContextBuilder
{
    /// <summary>
    /// Builds complete generation context for a chapter.
    /// </summary>
    /// <param name="blueprint">The book blueprint.</param>
    /// <param name="chapterNumber">Chapter number to generate.</param>
    /// <param name="previousChapters">Previously generated chapters.</param>
    /// <param name="options">Context building options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Generation context.</returns>
    Task<Result<ChapterGenerationContext>> BuildChapterContextAsync(
        BookBlueprint blueprint,
        int chapterNumber,
        List<GeneratedChapter> previousChapters,
        ContextBuildingOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds scene generation context.
    /// </summary>
    /// <param name="chapterContext">The chapter context.</param>
    /// <param name="sceneNumber">Scene number within chapter.</param>
    /// <param name="previousScenes">Previously generated scenes in this chapter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Scene generation context.</returns>
    Task<Result<SceneGenerationContext>> BuildSceneContextAsync(
        ChapterGenerationContext chapterContext,
        int sceneNumber,
        List<GeneratedScene> previousScenes,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Compresses context to fit within token limits.
    /// </summary>
    /// <param name="context">The context to compress.</param>
    /// <param name="maxTokens">Maximum token count.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Compressed context.</returns>
    Task<Result<string>> CompressContextAsync(
        string context,
        int maxTokens,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds character context for appearing characters.
    /// </summary>
    /// <param name="characterIds">Character IDs.</param>
    /// <param name="characterBible">The character bible.</param>
    /// <param name="currentStates">Current character states.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Character context string.</returns>
    Task<Result<string>> BuildCharacterContextAsync(
        List<Guid> characterIds,
        CharacterBible characterBible,
        List<CharacterStateSnapshot> currentStates,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds location context.
    /// </summary>
    /// <param name="locationIds">Location IDs.</param>
    /// <param name="worldBible">The world bible.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Location context string.</returns>
    Task<Result<string>> BuildLocationContextAsync(
        List<Guid> locationIds,
        WorldBible worldBible,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds plot context including active threads and upcoming beats.
    /// </summary>
    /// <param name="chapterBlueprint">The chapter blueprint.</param>
    /// <param name="plotArchitecture">The plot architecture.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Plot context string.</returns>
    Task<Result<string>> BuildPlotContextAsync(
        ChapterBlueprint chapterBlueprint,
        PlotArchitecture plotArchitecture,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds style context from style guide.
    /// </summary>
    /// <param name="styleGuide">The style guide.</param>
    /// <param name="chapterTone">Specific tone for this chapter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Style context string.</returns>
    Task<Result<string>> BuildStyleContextAsync(
        StyleGuide styleGuide,
        ChapterTone chapterTone,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Estimates token count for content.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <returns>Estimated token count.</returns>
    int EstimateTokens(string content);
}

/// <summary>
/// Options for context building.
/// </summary>
public sealed record ContextBuildingOptions
{
    /// <summary>
    /// Maximum total context tokens.
    /// </summary>
    public int MaxContextTokens { get; init; } = 8000;

    /// <summary>
    /// Number of previous chapters to include.
    /// </summary>
    public int PreviousChaptersToInclude { get; init; } = 2;

    /// <summary>
    /// Include full previous chapter or summary.
    /// </summary>
    public bool IncludeFullPreviousChapter { get; init; } = false;

    /// <summary>
    /// Include character summaries.
    /// </summary>
    public bool IncludeCharacterSummaries { get; init; } = true;

    /// <summary>
    /// Include location descriptions.
    /// </summary>
    public bool IncludeLocationDescriptions { get; init; } = true;

    /// <summary>
    /// Include plot thread status.
    /// </summary>
    public bool IncludePlotThreadStatus { get; init; } = true;

    /// <summary>
    /// Include style guide excerpt.
    /// </summary>
    public bool IncludeStyleGuide { get; init; } = true;

    /// <summary>
    /// Compression strategy.
    /// </summary>
    public CompressionStrategy CompressionStrategy { get; init; } = CompressionStrategy.Summarize;
}

/// <summary>
/// Compression strategy.
/// </summary>
public enum CompressionStrategy
{
    /// <summary>Truncate content.</summary>
    Truncate,
    /// <summary>Summarize using AI.</summary>
    Summarize,
    /// <summary>Extract key points.</summary>
    ExtractKeyPoints,
    /// <summary>Use sliding window.</summary>
    SlidingWindow
}

/// <summary>
/// Complete context for chapter generation.
/// </summary>
public sealed record ChapterGenerationContext
{
    /// <summary>
    /// Chapter blueprint.
    /// </summary>
    public required ChapterBlueprint Blueprint { get; init; }

    /// <summary>
    /// System prompt.
    /// </summary>
    public required string SystemPrompt { get; init; }

    /// <summary>
    /// Story context (previous chapters summary).
    /// </summary>
    public required string StoryContext { get; init; }

    /// <summary>
    /// Character context.
    /// </summary>
    public required string CharacterContext { get; init; }

    /// <summary>
    /// Location context.
    /// </summary>
    public required string LocationContext { get; init; }

    /// <summary>
    /// Plot context.
    /// </summary>
    public required string PlotContext { get; init; }

    /// <summary>
    /// Style context.
    /// </summary>
    public required string StyleContext { get; init; }

    /// <summary>
    /// Specific instructions for this chapter.
    /// </summary>
    public required string ChapterInstructions { get; init; }

    /// <summary>
    /// Must-include elements.
    /// </summary>
    public List<string> MustInclude { get; init; } = new();

    /// <summary>
    /// Must-avoid elements.
    /// </summary>
    public List<string> MustAvoid { get; init; } = new();

    /// <summary>
    /// Total token count.
    /// </summary>
    public int TotalTokens { get; init; }

    /// <summary>
    /// Available tokens for generation.
    /// </summary>
    public int AvailableTokensForGeneration { get; init; }

    /// <summary>
    /// Context summary for metadata.
    /// </summary>
    public required GenerationContextSummary Summary { get; init; }
}

/// <summary>
/// Context for scene generation.
/// </summary>
public sealed record SceneGenerationContext
{
    /// <summary>
    /// Scene blueprint.
    /// </summary>
    public required SceneBlueprint Blueprint { get; init; }

    /// <summary>
    /// Parent chapter context.
    /// </summary>
    public required ChapterGenerationContext ChapterContext { get; init; }

    /// <summary>
    /// Previous scenes in this chapter.
    /// </summary>
    public List<string> PreviousSceneContent { get; init; } = new();

    /// <summary>
    /// Scene-specific instructions.
    /// </summary>
    public required string SceneInstructions { get; init; }

    /// <summary>
    /// Transition from previous scene.
    /// </summary>
    public string? TransitionInstruction { get; init; }

    /// <summary>
    /// Target word count for scene.
    /// </summary>
    public required int TargetWordCount { get; init; }
}
