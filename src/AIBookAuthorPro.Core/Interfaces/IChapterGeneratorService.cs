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
    System.Threading.Tasks.Task<Result<GenerationResult>> GenerateChapterAsync(
        ChapterGenerationRequest request,
        IProgress<GenerationProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Continues writing from where the chapter left off.
    /// </summary>
    System.Threading.Tasks.Task<Result<string>> ContinueWritingAsync(
        Chapter chapter,
        Project project,
        string? additionalInstructions = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rewrites a section of chapter content.
    /// </summary>
    System.Threading.Tasks.Task<Result<string>> RewriteSectionAsync(
        Chapter chapter,
        string selectedText,
        string instructions,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates an outline for a chapter.
    /// </summary>
    System.Threading.Tasks.Task<Result<string>> GenerateChapterOutlineAsync(
        Chapter chapter,
        Project project,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams content generation for real-time display.
    /// </summary>
    IAsyncEnumerable<string> StreamGenerateChapterAsync(
        ChapterGenerationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Estimates the cost of generating a chapter.
    /// </summary>
    System.Threading.Tasks.Task<GenerationCostEstimate> EstimateGenerationCostAsync(ChapterGenerationRequest request);
}

/// <summary>
/// Request for chapter generation.
/// </summary>
public record ChapterGenerationRequest
{
    public required Project Project { get; init; }
    public required Chapter Chapter { get; init; }
    public string? ModelId { get; init; }
    public double Temperature { get; init; } = 0.7;
    public int MaxTokens { get; init; } = 4000;
    public string? AdditionalInstructions { get; init; }
    public bool IncludePreviousChapter { get; init; } = true;
    public bool IncludeCharacters { get; init; } = true;
    public bool IncludeLocations { get; init; } = true;
    public bool IncludeOutline { get; init; } = true;
}
