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
    Task<Result<ChapterGenerationContext>> BuildChapterContextAsync(
        BookBlueprint blueprint,
        int chapterNumber,
        List<GeneratedChapter> previousChapters,
        ContextBuildingOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds scene generation context.
    /// </summary>
    Task<Result<SceneGenerationContext>> BuildSceneContextAsync(
        ChapterGenerationContext chapterContext,
        int sceneNumber,
        List<GeneratedScene> previousScenes,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Compresses context to fit within token limits.
    /// </summary>
    Task<Result<string>> CompressContextAsync(
        string context,
        int maxTokens,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds character context for appearing characters.
    /// </summary>
    Task<Result<string>> BuildCharacterContextAsync(
        List<Guid> characterIds,
        CharacterBible characterBible,
        List<CharacterStateSnapshot> currentStates,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds location context.
    /// </summary>
    Task<Result<string>> BuildLocationContextAsync(
        List<Guid> locationIds,
        WorldBible worldBible,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds plot context including active threads and upcoming beats.
    /// </summary>
    Task<Result<string>> BuildPlotContextAsync(
        ChapterBlueprint chapterBlueprint,
        PlotArchitecture plotArchitecture,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds style context from style guide.
    /// </summary>
    Task<Result<string>> BuildStyleContextAsync(
        StyleGuide styleGuide,
        ChapterTone chapterTone,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Estimates token count for content.
    /// </summary>
    int EstimateTokens(string content);
}

/// <summary>
/// Options for context building.
/// </summary>
public sealed record ContextBuildingOptions
{
    /// <summary>Maximum total context tokens.</summary>
    public int MaxContextTokens { get; init; } = 8000;

    /// <summary>Number of previous chapters to include.</summary>
    public int PreviousChaptersToInclude { get; init; } = 2;

    /// <summary>Include full previous chapter or summary.</summary>
    public bool IncludeFullPreviousChapter { get; init; } = false;

    /// <summary>Include character summaries.</summary>
    public bool IncludeCharacterSummaries { get; init; } = true;

    /// <summary>Include location descriptions.</summary>
    public bool IncludeLocationDescriptions { get; init; } = true;

    /// <summary>Include plot thread status.</summary>
    public bool IncludePlotThreadStatus { get; init; } = true;

    /// <summary>Include style guide excerpt.</summary>
    public bool IncludeStyleGuide { get; init; } = true;

    /// <summary>Compression strategy.</summary>
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
    /// <summary>Chapter blueprint.</summary>
    public required ChapterBlueprint Blueprint { get; init; }

    /// <summary>System prompt.</summary>
    public required string SystemPrompt { get; init; }

    /// <summary>Story context (previous chapters summary).</summary>
    public required string StoryContext { get; init; }

    /// <summary>Character context.</summary>
    public required string CharacterContext { get; init; }

    /// <summary>Location context.</summary>
    public required string LocationContext { get; init; }

    /// <summary>Plot context.</summary>
    public required string PlotContext { get; init; }

    /// <summary>Style context.</summary>
    public required string StyleContext { get; init; }

    /// <summary>Specific instructions for this chapter.</summary>
    public required string ChapterInstructions { get; init; }

    /// <summary>Must-include elements.</summary>
    public List<string> MustInclude { get; init; } = new();

    /// <summary>Must-avoid elements.</summary>
    public List<string> MustAvoid { get; init; } = new();

    /// <summary>Total token count.</summary>
    public int TotalTokens { get; init; }

    /// <summary>Available tokens for generation.</summary>
    public int AvailableTokensForGeneration { get; init; }

    /// <summary>Context summary for metadata.</summary>
    public required GenerationContextSummary Summary { get; init; }
}

/// <summary>
/// Context for scene generation.
/// </summary>
public sealed record SceneGenerationContext
{
    /// <summary>Scene blueprint.</summary>
    public required SceneBlueprint Blueprint { get; init; }

    /// <summary>Parent chapter context.</summary>
    public required ChapterGenerationContext ChapterContext { get; init; }

    /// <summary>Previous scenes in this chapter.</summary>
    public List<string> PreviousSceneContent { get; init; } = new();

    /// <summary>Scene-specific instructions.</summary>
    public required string SceneInstructions { get; init; }

    /// <summary>Transition from previous scene.</summary>
    public string? TransitionInstruction { get; init; }

    /// <summary>Target word count for scene.</summary>
    public required int TargetWordCount { get; init; }
}
