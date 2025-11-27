// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Enums;
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
    private readonly Dictionary<string, AIProviderType> _modelToProviderMap;

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

        // Map model IDs to provider types
        _modelToProviderMap = new Dictionary<string, AIProviderType>(StringComparer.OrdinalIgnoreCase)
        {
            // Claude models
            ["claude-3-opus-20240229"] = AIProviderType.Anthropic,
            ["claude-3-5-sonnet-20241022"] = AIProviderType.Anthropic,
            ["claude-3-sonnet-20240229"] = AIProviderType.Anthropic,
            ["claude-3-haiku-20240307"] = AIProviderType.Anthropic,
            ["claude-sonnet-4-20250514"] = AIProviderType.Anthropic,
            ["claude-opus-4-20250514"] = AIProviderType.Anthropic,
            
            // OpenAI models
            ["gpt-4o"] = AIProviderType.OpenAI,
            ["gpt-4o-mini"] = AIProviderType.OpenAI,
            ["gpt-4-turbo"] = AIProviderType.OpenAI,
            ["gpt-4"] = AIProviderType.OpenAI,
            ["gpt-3.5-turbo"] = AIProviderType.OpenAI,
            
            // Gemini models
            ["gemini-1.5-pro"] = AIProviderType.Google,
            ["gemini-1.5-flash"] = AIProviderType.Google,
            ["gemini-pro"] = AIProviderType.Google
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
    public IAIProvider GetProvider(string modelId)
    {
        if (string.IsNullOrWhiteSpace(modelId))
        {
            return GetDefaultProvider();
        }

        if (_modelToProviderMap.TryGetValue(modelId, out var providerType))
        {
            return GetProvider(providerType);
        }

        // Try to infer from model name
        if (modelId.StartsWith("claude", StringComparison.OrdinalIgnoreCase))
        {
            return GetProvider(AIProviderType.Anthropic);
        }
        if (modelId.StartsWith("gpt", StringComparison.OrdinalIgnoreCase))
        {
            return GetProvider(AIProviderType.OpenAI);
        }
        if (modelId.StartsWith("gemini", StringComparison.OrdinalIgnoreCase))
        {
            return GetProvider(AIProviderType.Google);
        }

        // Default to Anthropic
        _logger.LogWarning("Unknown model ID '{ModelId}', defaulting to Anthropic", modelId);
        return GetProvider(AIProviderType.Anthropic);
    }

    /// <inheritdoc />
    public IAIProvider GetDefaultProvider()
    {
        // Try Anthropic first (Claude), then OpenAI, then Google
        var providers = GetConfiguredProviders();
        
        if (providers.Count == 0)
        {
            // Return Anthropic even if not configured, let it fail with a better error
            return GetProvider(AIProviderType.Anthropic);
        }

        // Prefer Anthropic if available
        var anthropic = providers.FirstOrDefault(p => p.ProviderName == "Anthropic");
        if (anthropic != null) return anthropic;

        // Then OpenAI
        var openai = providers.FirstOrDefault(p => p.ProviderName == "OpenAI");
        if (openai != null) return openai;

        // Any configured provider
        return providers[0];
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
    public IReadOnlyList<IAIProvider> GetConfiguredProviders()
    {
        return GetAllProviders()
            .Where(p => p.IsConfigured)
            .ToList();
    }

    /// <summary>
    /// Gets all available model information.
    /// </summary>
    public IReadOnlyList<AIModelInfo> GetAllAvailableModels()
    {
        var models = new List<AIModelInfo>();

        foreach (var provider in GetAllProviders())
        {
            models.AddRange(provider.AvailableModels);
        }

        return models;
    }

    /// <summary>
    /// Checks if a provider type is available.
    /// </summary>
    public bool IsProviderAvailable(AIProviderType providerType)
    {
        try
        {
            var provider = GetProvider(providerType);
            return provider.IsConfigured;
        }
        catch
        {
            return false;
        }
    }
}
