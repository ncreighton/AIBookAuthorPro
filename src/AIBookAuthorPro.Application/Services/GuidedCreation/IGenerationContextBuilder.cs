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
    /// <summary>Chapter number.</summary>
    public int ChapterNumber { get; init; }

    /// <summary>Chapter blueprint.</summary>
    public ChapterBlueprint ChapterBlueprint { get; init; } = new();

    /// <summary>Chapter blueprint (alias for ChapterBlueprint).</summary>
    public ChapterBlueprint Blueprint { get => ChapterBlueprint; init => ChapterBlueprint = value; }

    /// <summary>System prompt.</summary>
    public string SystemPrompt { get; init; } = string.Empty;

    /// <summary>Story context (previous chapters summary).</summary>
    public string StoryContext { get; init; } = string.Empty;

    /// <summary>Narrative context.</summary>
    public string NarrativeContext { get; init; } = string.Empty;

    /// <summary>Character context.</summary>
    public string CharacterContext { get; init; } = string.Empty;

    /// <summary>Location context.</summary>
    public string LocationContext { get; init; } = string.Empty;

    /// <summary>World context.</summary>
    public string WorldContext { get; init; } = string.Empty;

    /// <summary>Plot context.</summary>
    public string PlotContext { get; init; } = string.Empty;

    /// <summary>Style context.</summary>
    public string StyleContext { get; init; } = string.Empty;

    /// <summary>Specific instructions for this chapter.</summary>
    public string ChapterInstructions { get; init; } = string.Empty;

    /// <summary>Must-include elements.</summary>
    public List<string> MustInclude { get; init; } = new();

    /// <summary>Must-avoid elements.</summary>
    public List<string> MustAvoid { get; init; } = new();

    /// <summary>Previous chapter summaries.</summary>
    public List<string> PreviousChapterSummaries { get; init; } = new();

    /// <summary>Character states.</summary>
    public List<CharacterStateSnapshot> CharacterStates { get; init; } = new();

    /// <summary>Active setups.</summary>
    public List<SetupPayoff> ActiveSetups { get; init; } = new();

    /// <summary>Due payoffs.</summary>
    public List<SetupPayoff> DuePayoffs { get; init; } = new();

    /// <summary>Token budget.</summary>
    public int TokenBudget { get; init; }

    /// <summary>Generation config.</summary>
    public GenerationConfiguration? GenerationConfig { get; init; }

    /// <summary>Total token count.</summary>
    public int TotalTokens { get; init; }

    /// <summary>Available tokens for generation.</summary>
    public int AvailableTokensForGeneration { get; init; }

    /// <summary>Context summary for metadata.</summary>
    public GenerationContextSummary Summary { get; init; } = new();
}

/// <summary>
/// Context for scene generation.
/// </summary>
public sealed record SceneGenerationContext
{
    /// <summary>Scene blueprint.</summary>
    public SceneBlueprint Blueprint { get; init; } = new();

    /// <summary>Parent chapter context.</summary>
    public ChapterGenerationContext ChapterContext { get; init; } = new();

    /// <summary>Previous scenes in this chapter.</summary>
    public List<string> PreviousSceneContent { get; init; } = new();

    /// <summary>Alias for PreviousSceneContent.</summary>
    public List<string> PreviousScenes { get => PreviousSceneContent; init => PreviousSceneContent = value; }

    /// <summary>Scene number.</summary>
    public int SceneNumber { get; init; }

    /// <summary>Scene-specific instructions.</summary>
    public string SceneInstructions { get; init; } = string.Empty;

    /// <summary>Transition from previous scene.</summary>
    public string? TransitionInstruction { get; init; }

    /// <summary>Target word count for scene.</summary>
    public int TargetWordCount { get; init; }
}
