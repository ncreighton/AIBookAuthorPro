// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Interfaces;
using AIBookAuthorPro.Core.Models.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.Infrastructure.AI;

/// <summary>
/// Factory for creating and managing AI provider instances.
/// </summary>
public sealed class AIProviderFactory : IAIProviderFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AIProviderFactory> _logger;
    private readonly Dictionary<AIProviderType, Type> _providerTypes;

    public AIProviderFactory(
        IServiceProvider serviceProvider,
        ILogger<AIProviderFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _providerTypes = new Dictionary<AIProviderType, Type>
        {
            [AIProviderType.Anthropic] = typeof(AnthropicProvider),
            [AIProviderType.OpenAI] = typeof(OpenAIProvider),
            [AIProviderType.Google] = typeof(GeminiProvider)
        };
    }

    /// <inheritdoc />
    public IAIProvider GetProvider(AIProviderType providerType)
    {
        _logger.LogDebug("Getting AI provider: {ProviderType}", providerType);

        if (!_providerTypes.TryGetValue(providerType, out var type))
        {
            throw new NotSupportedException($"AI provider type '{providerType}' is not supported");
        }

        var provider = _serviceProvider.GetRequiredService(type) as IAIProvider
            ?? throw new InvalidOperationException($"Failed to resolve provider for type '{providerType}'");

        return provider;
    }

    /// <inheritdoc />
    public IReadOnlyList<IAIProvider> GetAllProviders()
    {
        var providers = new List<IAIProvider>();

        foreach (var kvp in _providerTypes)
        {
            try
            {
                var provider = _serviceProvider.GetService(kvp.Value) as IAIProvider;
                if (provider != null)
                {
                    providers.Add(provider);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to instantiate provider: {ProviderType}", kvp.Key);
            }
        }

        return providers;
    }

    /// <inheritdoc />
    public IReadOnlyList<AIModelInfo> GetAllAvailableModels()
    {
        var models = new List<AIModelInfo>();

        foreach (var provider in GetAllProviders())
        {
            models.AddRange(provider.AvailableModels);
        }

        return models;
    }

    /// <inheritdoc />
    public bool IsProviderAvailable(AIProviderType providerType)
    {
        try
        {
            var provider = GetProvider(providerType);
            return provider != null;
        }
        catch
        {
            return false;
        }
    }
}