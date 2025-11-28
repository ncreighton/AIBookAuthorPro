// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Common;

namespace AIBookAuthorPro.Core.Services;

/// <summary>
/// Core AI service interface for text generation.
/// </summary>
public interface IAIService
{
    /// <summary>
    /// Generates text using the AI model.
    /// </summary>
    /// <param name="prompt">The prompt to send.</param>
    /// <param name="options">Generation options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Generated text.</returns>
    Task<Result<string>> GenerateAsync(
        string prompt,
        GenerationOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates text with streaming output.
    /// </summary>
    /// <param name="prompt">The prompt to send.</param>
    /// <param name="options">Generation options.</param>
    /// <param name="onChunk">Callback for each chunk.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Complete generated text.</returns>
    Task<Result<string>> GenerateStreamingAsync(
        string prompt,
        GenerationOptions options,
        Action<string> onChunk,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates text with a system prompt.
    /// </summary>
    /// <param name="systemPrompt">System prompt.</param>
    /// <param name="userPrompt">User prompt.</param>
    /// <param name="options">Generation options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Generated text.</returns>
    Task<Result<string>> GenerateWithSystemAsync(
        string systemPrompt,
        string userPrompt,
        GenerationOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current AI provider name.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Gets the current model name.
    /// </summary>
    string ModelName { get; }

    /// <summary>
    /// Estimates token count for text.
    /// </summary>
    /// <param name="text">Text to estimate.</param>
    /// <returns>Estimated token count.</returns>
    int EstimateTokens(string text);
}

/// <summary>
/// Options for AI generation.
/// </summary>
public sealed record GenerationOptions
{
    /// <summary>
    /// Temperature for generation (0-1).
    /// </summary>
    public double Temperature { get; init; } = 0.7;

    /// <summary>
    /// Maximum tokens to generate.
    /// </summary>
    public int MaxTokens { get; init; } = 4096;

    /// <summary>
    /// Top P sampling.
    /// </summary>
    public double TopP { get; init; } = 0.95;

    /// <summary>
    /// Response format ("text" or "json").
    /// </summary>
    public string ResponseFormat { get; init; } = "text";

    /// <summary>
    /// Stop sequences.
    /// </summary>
    public List<string>? StopSequences { get; init; }

    /// <summary>
    /// Model override (if different from default).
    /// </summary>
    public string? ModelOverride { get; init; }

    /// <summary>
    /// Request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; init; } = 120;
}
