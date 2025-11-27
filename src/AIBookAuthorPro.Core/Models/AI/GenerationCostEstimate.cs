// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Enums;

namespace AIBookAuthorPro.Core.Models.AI;

/// <summary>
/// Cost estimate for a generation operation.
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
    /// Gets the total estimated tokens.
    /// </summary>
    public int TotalEstimatedTokens => EstimatedInputTokens + EstimatedOutputTokens;

    /// <summary>
    /// Gets or sets the estimated cost in USD.
    /// </summary>
    public decimal EstimatedCostUsd { get; init; }

    /// <summary>
    /// Gets or sets the model ID used for estimation.
    /// </summary>
    public string ModelId { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the provider used for estimation.
    /// </summary>
    public AIProviderType Provider { get; init; }
}
