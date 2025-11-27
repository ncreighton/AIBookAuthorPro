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
    /// <param name="request">The generation request.</param>
    /// <param name="progress">Progress reporter for UI updates.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing the generation result.</returns>
    Task<Result<GenerationResult>> GenerateChapterAsync(
        ChapterGenerationPipelineRequest request,
        IProgress<GenerationProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates chapter content with streaming output.
    /// </summary>
    /// <param name="request">The generation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async enumerable of streaming chunks.</returns>
    IAsyncEnumerable<StreamingChunk> GenerateChapterStreamingAsync(
        ChapterGenerationPipelineRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Refines existing content using AI.
    /// </summary>
    /// <param name="content">The content to refine.</param>
    /// <param name="instructions">Refinement instructions.</param>
    /// <param name="provider">The AI provider to use.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing the refined content.</returns>
    Task<Result<string>> RefineContentAsync(
        string content,
        string instructions,
        AIProviderType provider,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Estimates the cost of a generation request.
    /// </summary>
    /// <param name="request">The generation request.</param>
    /// <returns>Estimated cost information.</returns>
    GenerationCostEstimate EstimateCost(ChapterGenerationPipelineRequest request);

    /// <summary>
    /// Gets available generation modes.
    /// </summary>
    /// <returns>List of available generation modes.</returns>
    IReadOnlyList<GenerationModeInfo> GetAvailableModes();
}

/// <summary>
/// Request for chapter generation through the pipeline.
/// </summary>
public sealed class ChapterGenerationPipelineRequest
{
    /// <summary>
    /// Gets or sets the project context.
    /// </summary>
    public required Project Project { get; init; }

    /// <summary>
    /// Gets or sets the chapter to generate for.
    /// </summary>
    public required Chapter Chapter { get; init; }

    /// <summary>
    /// Gets or sets the generation mode.
    /// </summary>
    public GenerationMode Mode { get; init; } = GenerationMode.Standard;

    /// <summary>
    /// Gets or sets the AI provider to use.
    /// </summary>
    public AIProviderType Provider { get; init; } = AIProviderType.Claude;

    /// <summary>
    /// Gets or sets the specific model to use (optional).
    /// </summary>
    public string? ModelId { get; init; }

    /// <summary>
    /// Gets or sets the temperature setting.
    /// </summary>
    public double Temperature { get; init; } = 0.7;

    /// <summary>
    /// Gets or sets custom instructions for generation.
    /// </summary>
    public string? CustomInstructions { get; init; }

    /// <summary>
    /// Gets or sets whether to include character context.
    /// </summary>
    public bool IncludeCharacterContext { get; init; } = true;

    /// <summary>
    /// Gets or sets whether to include location context.
    /// </summary>
    public bool IncludeLocationContext { get; init; } = true;

    /// <summary>
    /// Gets or sets whether to include previous chapter summary.
    /// </summary>
    public bool IncludePreviousSummary { get; init; } = true;
}

/// <summary>
/// Progress information during generation.
/// </summary>
public sealed class GenerationProgress
{
    /// <summary>
    /// Gets or sets the current phase description.
    /// </summary>
    public string Phase { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the progress percentage (0-100).
    /// </summary>
    public double Percentage { get; init; }

    /// <summary>
    /// Gets or sets the current word count.
    /// </summary>
    public int WordCount { get; init; }

    /// <summary>
    /// Gets or sets the tokens used so far.
    /// </summary>
    public int TokensUsed { get; init; }

    /// <summary>
    /// Gets or sets the current content (for streaming).
    /// </summary>
    public string? CurrentContent { get; init; }

    /// <summary>
    /// Gets or sets whether the generation is complete.
    /// </summary>
    public bool IsComplete { get; init; }
}

/// <summary>
/// Cost estimate for a generation request.
/// </summary>
public sealed class GenerationCostEstimate
{
    /// <summary>
    /// Gets or sets the estimated input tokens.
    /// </summary>
    public int EstimatedInputTokens { get; init; }

    /// <summary>
    /// Gets or sets the estimated output tokens.
    /// </summary>
    public int EstimatedOutputTokens { get; init; }

    /// <summary>
    /// Gets or sets the estimated cost in USD.
    /// </summary>
    public decimal EstimatedCostUsd { get; init; }

    /// <summary>
    /// Gets or sets the model used for estimation.
    /// </summary>
    public string ModelId { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the provider used for estimation.
    /// </summary>
    public AIProviderType Provider { get; init; }
}

/// <summary>
/// Information about a generation mode.
/// </summary>
public sealed class GenerationModeInfo
{
    /// <summary>
    /// Gets or sets the mode.
    /// </summary>
    public GenerationMode Mode { get; init; }

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets recommended use cases.
    /// </summary>
    public string RecommendedFor { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets estimated generation time.
    /// </summary>
    public string EstimatedTime { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the relative cost indicator (1-5).
    /// </summary>
    public int CostIndicator { get; init; }
}
