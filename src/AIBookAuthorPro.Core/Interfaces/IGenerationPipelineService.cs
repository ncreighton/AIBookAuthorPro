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
/// Defines the contract for AI content generation pipeline services.
/// </summary>
public interface IGenerationPipelineService
{
    /// <summary>
    /// Generates chapter content using the specified mode and settings.
    /// </summary>
    System.Threading.Tasks.Task<Result<GenerationResult>> GenerateChapterAsync(
        ChapterGenerationPipelineRequest request,
        IProgress<GenerationProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates chapter content with streaming output.
    /// </summary>
    IAsyncEnumerable<StreamingChunk> GenerateChapterStreamingAsync(
        ChapterGenerationPipelineRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Refines existing content using AI.
    /// </summary>
    System.Threading.Tasks.Task<Result<string>> RefineContentAsync(
        string content,
        string instructions,
        Enums.AIProviderType provider,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Estimates the cost of a generation request.
    /// </summary>
    GenerationCostEstimate EstimateCost(ChapterGenerationPipelineRequest request);

    /// <summary>
    /// Gets available generation modes.
    /// </summary>
    IReadOnlyList<GenerationModeInfo> GetAvailableModes();
}
