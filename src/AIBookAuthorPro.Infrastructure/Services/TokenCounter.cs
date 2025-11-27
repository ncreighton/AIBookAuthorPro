// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Interfaces;

namespace AIBookAuthorPro.Infrastructure.Services;

/// <summary>
/// Token counting service for estimating AI model token usage.
/// </summary>
public sealed class TokenCounter : ITokenCounter
{
    // Model context window sizes
    private static readonly Dictionary<string, int> ModelContextSizes = new(StringComparer.OrdinalIgnoreCase)
    {
        // Claude models
        ["claude-3-opus-20240229"] = 200000,
        ["claude-3-5-sonnet-20241022"] = 200000,
        ["claude-3-sonnet-20240229"] = 200000,
        ["claude-3-haiku-20240307"] = 200000,
        ["claude-sonnet-4-20250514"] = 200000,
        ["claude-opus-4-20250514"] = 200000,
        
        // OpenAI models
        ["gpt-4o"] = 128000,
        ["gpt-4o-mini"] = 128000,
        ["gpt-4-turbo"] = 128000,
        ["gpt-4"] = 8192,
        ["gpt-3.5-turbo"] = 16385,
        
        // Gemini models
        ["gemini-1.5-pro"] = 1000000,
        ["gemini-1.5-flash"] = 1000000,
        ["gemini-pro"] = 32000
    };

    // Model max output tokens
    private static readonly Dictionary<string, int> ModelMaxOutputTokens = new(StringComparer.OrdinalIgnoreCase)
    {
        // Claude models
        ["claude-3-opus-20240229"] = 4096,
        ["claude-3-5-sonnet-20241022"] = 8192,
        ["claude-3-sonnet-20240229"] = 4096,
        ["claude-3-haiku-20240307"] = 4096,
        ["claude-sonnet-4-20250514"] = 8192,
        ["claude-opus-4-20250514"] = 8192,
        
        // OpenAI models
        ["gpt-4o"] = 16384,
        ["gpt-4o-mini"] = 16384,
        ["gpt-4-turbo"] = 4096,
        ["gpt-4"] = 8192,
        ["gpt-3.5-turbo"] = 4096,
        
        // Gemini models
        ["gemini-1.5-pro"] = 8192,
        ["gemini-1.5-flash"] = 8192,
        ["gemini-pro"] = 8192
    };

    // Pricing per 1K tokens (input, output)
    private static readonly Dictionary<string, (decimal Input, decimal Output)> ModelPricing = new(StringComparer.OrdinalIgnoreCase)
    {
        // Claude models
        ["claude-3-opus-20240229"] = (0.015m, 0.075m),
        ["claude-3-5-sonnet-20241022"] = (0.003m, 0.015m),
        ["claude-3-sonnet-20240229"] = (0.003m, 0.015m),
        ["claude-3-haiku-20240307"] = (0.00025m, 0.00125m),
        ["claude-sonnet-4-20250514"] = (0.003m, 0.015m),
        ["claude-opus-4-20250514"] = (0.015m, 0.075m),
        
        // OpenAI models
        ["gpt-4o"] = (0.005m, 0.015m),
        ["gpt-4o-mini"] = (0.00015m, 0.0006m),
        ["gpt-4-turbo"] = (0.01m, 0.03m),
        ["gpt-4"] = (0.03m, 0.06m),
        ["gpt-3.5-turbo"] = (0.0005m, 0.0015m),
        
        // Gemini models
        ["gemini-1.5-pro"] = (0.0035m, 0.0105m),
        ["gemini-1.5-flash"] = (0.00035m, 0.00105m),
        ["gemini-pro"] = (0.00025m, 0.0005m)
    };

    /// <inheritdoc />
    public int EstimateTokens(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        // Rough estimation: ~4 characters per token for English text
        // This is a simplification - actual tokenization varies by model
        return (int)Math.Ceiling(text.Length / 4.0);
    }

    /// <inheritdoc />
    public int EstimateTokens(IEnumerable<string> texts)
    {
        return texts.Sum(EstimateTokens);
    }

    /// <inheritdoc />
    public int GetMaxContextTokens(string modelId)
    {
        return ModelContextSizes.GetValueOrDefault(modelId, 8000);
    }

    /// <inheritdoc />
    public int GetMaxOutputTokens(string modelId)
    {
        return ModelMaxOutputTokens.GetValueOrDefault(modelId, 4096);
    }

    /// <inheritdoc />
    public int GetRemainingOutputTokens(string modelId, int inputTokens)
    {
        var maxContext = GetMaxContextTokens(modelId);
        var maxOutput = GetMaxOutputTokens(modelId);
        var available = maxContext - inputTokens;
        return Math.Min(available, maxOutput);
    }

    /// <inheritdoc />
    public bool FitsInContext(string text, string modelId, int reserveOutputTokens = 4000)
    {
        var tokens = EstimateTokens(text);
        var maxContext = GetMaxContextTokens(modelId);
        return tokens + reserveOutputTokens <= maxContext;
    }

    /// <inheritdoc />
    public string TruncateToTokenLimit(string text, int maxTokens)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        var estimatedChars = maxTokens * 4;
        if (text.Length <= estimatedChars)
            return text;

        // Truncate at word boundary
        var truncated = text[..estimatedChars];
        var lastSpace = truncated.LastIndexOf(' ');
        if (lastSpace > estimatedChars * 0.8)
        {
            truncated = truncated[..lastSpace];
        }

        return truncated + "...";
    }

    /// <inheritdoc />
    public decimal EstimateCost(string modelId, int inputTokens, int outputTokens)
    {
        if (!ModelPricing.TryGetValue(modelId, out var pricing))
        {
            // Default pricing if model not found
            pricing = (0.003m, 0.015m);
        }

        var inputCost = (inputTokens / 1000.0m) * pricing.Input;
        var outputCost = (outputTokens / 1000.0m) * pricing.Output;

        return inputCost + outputCost;
    }
}
