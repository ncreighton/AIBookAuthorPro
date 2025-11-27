// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

namespace AIBookAuthorPro.Core.Interfaces;

/// <summary>
/// Service for counting and estimating tokens for AI model context windows.
/// </summary>
public interface ITokenCounter
{
    /// <summary>
    /// Estimates the number of tokens in a string.
    /// </summary>
    /// <param name="text">The text to count tokens for.</param>
    /// <returns>Estimated token count.</returns>
    int EstimateTokens(string? text);

    /// <summary>
    /// Estimates the number of tokens in multiple strings.
    /// </summary>
    /// <param name="texts">The texts to count tokens for.</param>
    /// <returns>Total estimated token count.</returns>
    int EstimateTokens(IEnumerable<string> texts);

    /// <summary>
    /// Gets the maximum context window size for a model.
    /// </summary>
    /// <param name="modelId">The model identifier.</param>
    /// <returns>Maximum tokens allowed in context.</returns>
    int GetMaxContextTokens(string modelId);

    /// <summary>
    /// Gets the maximum output tokens for a model.
    /// </summary>
    /// <param name="modelId">The model identifier.</param>
    /// <returns>Maximum tokens allowed in output.</returns>
    int GetMaxOutputTokens(string modelId);

    /// <summary>
    /// Calculates how many tokens are remaining for output given input tokens.
    /// </summary>
    /// <param name="modelId">The model identifier.</param>
    /// <param name="inputTokens">Number of input tokens used.</param>
    /// <returns>Remaining tokens available for output.</returns>
    int GetRemainingOutputTokens(string modelId, int inputTokens);

    /// <summary>
    /// Checks if the text fits within the model's context window.
    /// </summary>
    /// <param name="text">The text to check.</param>
    /// <param name="modelId">The model identifier.</param>
    /// <param name="reserveOutputTokens">Tokens to reserve for output.</param>
    /// <returns>True if text fits within context.</returns>
    bool FitsInContext(string text, string modelId, int reserveOutputTokens = 4000);

    /// <summary>
    /// Truncates text to fit within a token limit.
    /// </summary>
    /// <param name="text">The text to truncate.</param>
    /// <param name="maxTokens">Maximum tokens allowed.</param>
    /// <returns>Truncated text.</returns>
    string TruncateToTokenLimit(string text, int maxTokens);

    /// <summary>
    /// Estimates the cost for a request based on token counts.
    /// </summary>
    /// <param name="modelId">The model identifier.</param>
    /// <param name="inputTokens">Number of input tokens.</param>
    /// <param name="outputTokens">Number of output tokens.</param>
    /// <returns>Estimated cost in USD.</returns>
    decimal EstimateCost(string modelId, int inputTokens, int outputTokens);
}
