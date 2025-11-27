// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Models;
using AIBookAuthorPro.Core.Models.AI;

namespace AIBookAuthorPro.Core.Interfaces;

/// <summary>
/// Service for generating chapter content using AI providers.
/// </summary>
public interface IChapterGeneratorService
{
    /// <summary>
    /// Generates content for a chapter based on the provided request.
    /// </summary>
    /// <param name="request">The chapter generation request.</param>
    /// <param name="progress">Optional progress reporter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The generated chapter content.</returns>
    Task<Result<GenerationResult>> GenerateChapterAsync(
        ChapterGenerationRequest request,
        IProgress<GenerationProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Continues writing from where the chapter left off.
    /// </summary>
    /// <param name="chapter">The chapter to continue.</param>
    /// <param name="project">The project containing the chapter.</param>
    /// <param name="additionalInstructions">Optional additional instructions.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The continued content.</returns>
    Task<Result<string>> ContinueWritingAsync(
        Chapter chapter,
        Project project,
        string? additionalInstructions = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rewrites a section of chapter content.
    /// </summary>
    /// <param name="chapter">The chapter containing the content.</param>
    /// <param name="selectedText">The text to rewrite.</param>
    /// <param name="instructions">Instructions for the rewrite.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The rewritten content.</returns>
    Task<Result<string>> RewriteSectionAsync(
        Chapter chapter,
        string selectedText,
        string instructions,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates an outline for a chapter.
    /// </summary>
    /// <param name="chapter">The chapter to outline.</param>
    /// <param name="project">The project containing the chapter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The generated outline.</returns>
    Task<Result<string>> GenerateChapterOutlineAsync(
        Chapter chapter,
        Project project,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams content generation for real-time display.
    /// </summary>
    /// <param name="request">The chapter generation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async enumerable of content chunks.</returns>
    IAsyncEnumerable<string> StreamGenerateChapterAsync(
        ChapterGenerationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Estimates the cost of generating a chapter.
    /// </summary>
    /// <param name="request">The chapter generation request.</param>
    /// <returns>The estimated cost.</returns>
    Task<GenerationCostEstimate> EstimateGenerationCostAsync(ChapterGenerationRequest request);
}

/// <summary>
/// Request for chapter generation.
/// </summary>
public record ChapterGenerationRequest
{
    /// <summary>
    /// The project containing the chapter.
    /// </summary>
    public required Project Project { get; init; }

    /// <summary>
    /// The chapter to generate content for.
    /// </summary>
    public required Chapter Chapter { get; init; }

    /// <summary>
    /// The AI model to use.
    /// </summary>
    public string? ModelId { get; init; }

    /// <summary>
    /// Temperature for generation (0-1).
    /// </summary>
    public double Temperature { get; init; } = 0.7;

    /// <summary>
    /// Maximum tokens to generate.
    /// </summary>
    public int MaxTokens { get; init; } = 4000;

    /// <summary>
    /// Additional instructions for generation.
    /// </summary>
    public string? AdditionalInstructions { get; init; }

    /// <summary>
    /// Include previous chapter context.
    /// </summary>
    public bool IncludePreviousChapter { get; init; } = true;

    /// <summary>
    /// Include character information.
    /// </summary>
    public bool IncludeCharacters { get; init; } = true;

    /// <summary>
    /// Include location information.
    /// </summary>
    public bool IncludeLocations { get; init; } = true;

    /// <summary>
    /// Include outline information.
    /// </summary>
    public bool IncludeOutline { get; init; } = true;
}
