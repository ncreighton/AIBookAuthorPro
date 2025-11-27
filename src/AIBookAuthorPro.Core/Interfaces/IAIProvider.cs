// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Models.AI;

namespace AIBookAuthorPro.Core.Interfaces;

/// <summary>
/// Defines the contract for AI content generation providers.
/// </summary>
public interface IAIProvider
{
    /// <summary>
    /// Gets the name of this provider.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Gets whether this provider supports streaming responses.
    /// </summary>
    bool SupportsStreaming { get; }

    /// <summary>
    /// Gets the maximum context window size in tokens.
    /// </summary>
    int MaxContextTokens { get; }

    /// <summary>
    /// Gets whether this provider is currently configured and available.
    /// </summary>
    bool IsConfigured { get; }

    /// <summary>
    /// Gets the available models for this provider.
    /// </summary>
    IReadOnlyList<AIModelInfo> AvailableModels { get; }

    /// <summary>
    /// Generates content based on the provided request.
    /// </summary>
    /// <param name="request">The generation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The generation result.</returns>
    Task<Result<GenerationResult>> GenerateAsync(
        GenerationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates content with streaming response.
    /// </summary>
    /// <param name="request">The generation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of content chunks.</returns>
    IAsyncEnumerable<string> StreamGenerateAsync(
        GenerationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Estimates the token count for the given text.
    /// </summary>
    /// <param name="text">The text to count.</param>
    /// <returns>Estimated token count.</returns>
    int EstimateTokenCount(string text);

    /// <summary>
    /// Gets the available models for this provider.
    /// </summary>
    /// <returns>List of available model identifiers.</returns>
    IReadOnlyList<string> GetAvailableModels();
}

/// <summary>
/// Factory for creating AI providers.
/// </summary>
public interface IAIProviderFactory
{
    /// <summary>
    /// Gets an AI provider by type.
    /// </summary>
    /// <param name="providerType">The provider type.</param>
    /// <returns>The AI provider instance.</returns>
    IAIProvider GetProvider(Enums.AIProviderType providerType);

    /// <summary>
    /// Gets an AI provider by model ID string.
    /// </summary>
    /// <param name="modelId">The model identifier.</param>
    /// <returns>The AI provider instance.</returns>
    IAIProvider GetProvider(string modelId);

    /// <summary>
    /// Gets the default AI provider.
    /// </summary>
    /// <returns>The default provider.</returns>
    IAIProvider GetDefaultProvider();

    /// <summary>
    /// Gets all available providers.
    /// </summary>
    /// <returns>List of all providers.</returns>
    IReadOnlyList<IAIProvider> GetAllProviders();

    /// <summary>
    /// Gets all configured (ready to use) providers.
    /// </summary>
    /// <returns>List of configured providers.</returns>
    IReadOnlyList<IAIProvider> GetConfiguredProviders();
}
