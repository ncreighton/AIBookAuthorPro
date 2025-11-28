// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Enums;
using AIBookAuthorPro.Core.Interfaces;
using AIBookAuthorPro.Core.Models;
using AIBookAuthorPro.Core.Models.AI;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.Infrastructure.Services;

/// <summary>
/// Service for orchestrating AI content generation through various pipelines.
/// </summary>
public sealed class GenerationPipelineService : IGenerationPipelineService
{
    private readonly IAIProviderFactory _providerFactory;
    private readonly IContextBuilderService _contextBuilder;
    private readonly ILogger<GenerationPipelineService> _logger;

    // Model mappings for generation modes
    private static readonly Dictionary<AIProviderType, Dictionary<GenerationMode, string>> ModelMappings = new()
    {
        [AIProviderType.Claude] = new()
        {
            [GenerationMode.Fast] = "claude-3-5-haiku-20241022",
            [GenerationMode.Standard] = "claude-sonnet-4-20250514",
            [GenerationMode.HighQuality] = "claude-opus-4-20250514"
        },
        [AIProviderType.OpenAI] = new()
        {
            [GenerationMode.Fast] = "gpt-4o-mini",
            [GenerationMode.Standard] = "gpt-4o",
            [GenerationMode.HighQuality] = "o1-preview"
        }
    };

    // Pricing per 1K tokens (input, output)
    private static readonly Dictionary<string, (decimal Input, decimal Output)> ModelPricing = new()
    {
        ["claude-3-5-haiku-20241022"] = (0.00025m, 0.00125m),
        ["claude-sonnet-4-20250514"] = (0.003m, 0.015m),
        ["claude-opus-4-20250514"] = (0.015m, 0.075m),
        ["gpt-4o-mini"] = (0.00015m, 0.0006m),
        ["gpt-4o"] = (0.005m, 0.015m),
        ["o1-preview"] = (0.015m, 0.06m),
        ["o1-mini"] = (0.003m, 0.012m)
    };

    /// <summary>
    /// Initializes a new instance of the GenerationPipelineService.
    /// </summary>
    public GenerationPipelineService(
        IAIProviderFactory providerFactory,
        IContextBuilderService contextBuilder,
        ILogger<GenerationPipelineService> logger)
    {
        _providerFactory = providerFactory;
        _contextBuilder = contextBuilder;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<GenerationResult>> GenerateChapterAsync(
        ChapterGenerationPipelineRequest request,
        IProgress<GenerationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Starting chapter generation: Chapter {ChapterNumber}, Mode: {Mode}, Provider: {Provider}",
                request.Chapter.Order, request.Mode, request.Provider);

            // Build context
            progress?.Report(new GenerationProgress
            {
                Phase = "Building context...",
                Percentage = 5
            });

            var contextResult = _contextBuilder.BuildChapterContext(
                request.Project,
                request.Chapter,
                new ContextBuildOptions
                {
                    IncludeCharacters = request.IncludeCharacterContext,
                    IncludeLocations = request.IncludeLocationContext,
                    IncludePreviousSummary = request.IncludePreviousSummary
                });

            if (contextResult.IsFailure)
            {
                return Result<GenerationResult>.Failure("Failed to build context: " + contextResult.Error);
            }

            var context = contextResult.Value!;

            // Build prompts
            progress?.Report(new GenerationProgress
            {
                Phase = "Preparing prompts...",
                Percentage = 10
            });

            var systemPrompt = _contextBuilder.BuildSystemPrompt(context);
            var userPrompt = _contextBuilder.BuildUserPrompt(context, request.CustomInstructions);

            // Get the appropriate model
            var modelId = request.ModelId ?? GetModelForMode(request.Provider, request.Mode);

            // Create generation request
            var generationRequest = new GenerationRequest
            {
                SystemPrompt = systemPrompt,
                UserPrompt = userPrompt,
                Provider = request.Provider,
                ModelId = modelId,
                Temperature = request.Temperature,
                MaxTokens = EstimateMaxTokensForWordCount(request.Chapter.TargetWordCount),
                Mode = request.Mode,
                Context = context
            };

            // Get provider
            var provider = _providerFactory.GetProvider(request.Provider);

            if (!provider.IsConfigured)
            {
                return Result<GenerationResult>.Failure(provider.ProviderName + " is not configured. Please add API key in settings.");
            }

            progress?.Report(new GenerationProgress
            {
                Phase = "Generating with " + provider.ProviderName + "...",
                Percentage = 15
            });

            // Execute based on mode
            GenerationResult result;

            if (request.Mode == GenerationMode.HighQuality)
            {
                // Premium mode: multi-pass generation
                result = await ExecutePremiumPipelineAsync(
                    provider,
                    generationRequest,
                    progress,
                    cancellationToken);
            }
            else
            {
                // Quick/Standard mode: single pass
                result = await ExecuteSinglePassAsync(
                    provider,
                    generationRequest,
                    progress,
                    cancellationToken);
            }

            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;
            result.ModelUsed = modelId;

            if (result.IsSuccess)
            {
                progress?.Report(new GenerationProgress
                {
                    Phase = "Generation complete!",
                    Percentage = 100,
                    IsComplete = true,
                    WordCount = CountWords(result.Content),
                    TokensUsed = result.Usage?.TotalTokens ?? 0
                });

                _logger.LogInformation(
                    "Chapter generation complete: {WordCount} words, {Tokens} tokens, {Duration}ms",
                    CountWords(result.Content),
                    result.Usage?.TotalTokens ?? 0,
                    stopwatch.ElapsedMilliseconds);
            }

            return Result<GenerationResult>.Success(result);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Chapter generation cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chapter generation failed");
            return Result<GenerationResult>.Failure("Generation failed: " + ex.Message, ex);
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<StreamingChunk> GenerateChapterStreamingAsync(
        ChapterGenerationPipelineRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation("Starting streaming chapter generation: Chapter {ChapterNumber}",
            request.Chapter.Order);

        // Build context
        var contextResult = _contextBuilder.BuildChapterContext(
            request.Project,
            request.Chapter,
            new ContextBuildOptions
            {
                IncludeCharacters = request.IncludeCharacterContext,
                IncludeLocations = request.IncludeLocationContext,
                IncludePreviousSummary = request.IncludePreviousSummary
            });

        if (contextResult.IsFailure)
        {
            yield return new StreamingChunk
            {
                Text = "Error: " + contextResult.Error,
                IsFinal = true,
                FinishReason = "error"
            };
            yield break;
        }

        var context = contextResult.Value!;
        var systemPrompt = _contextBuilder.BuildSystemPrompt(context);
        var userPrompt = _contextBuilder.BuildUserPrompt(context, request.CustomInstructions);

        var modelId = request.ModelId ?? GetModelForMode(request.Provider, request.Mode);

        var generationRequest = new GenerationRequest
        {
            SystemPrompt = systemPrompt,
            UserPrompt = userPrompt,
            Provider = request.Provider,
            ModelId = modelId,
            Temperature = request.Temperature,
            MaxTokens = EstimateMaxTokensForWordCount(request.Chapter.TargetWordCount),
            Mode = request.Mode,
            Context = context
        };

        var provider = _providerFactory.GetProvider(request.Provider);

        if (!provider.IsConfigured)
        {
            yield return new StreamingChunk
            {
                Text = "Error: " + provider.ProviderName + " is not configured.",
                IsFinal = true,
                FinishReason = "error"
            };
            yield break;
        }

        if (!provider.SupportsStreaming)
        {
            // Fall back to non-streaming
            var result = await provider.GenerateAsync(generationRequest, cancellationToken);

            if (result.IsSuccess && result.Value != null)
            {
                yield return new StreamingChunk
                {
                    Text = result.Value.Content,
                    IsFinal = true,
                    FinishReason = result.Value.FinishReason,
                    Usage = result.Value.Usage
                };
            }
            else
            {
                yield return new StreamingChunk
                {
                    Text = "Error: " + result.Error,
                    IsFinal = true,
                    FinishReason = "error"
                };
            }
            yield break;
        }

        await foreach (var chunk in provider.GenerateStreamingAsync(generationRequest, cancellationToken))
        {
            yield return chunk;
        }
    }

    /// <inheritdoc />
    public async Task<Result<string>> RefineContentAsync(
        string content,
        string instructions,
        AIProviderType provider,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(instructions);

        try
        {
            _logger.LogDebug("Refining content ({Length} chars) with provider {Provider}",
                content.Length, provider);

            var aiProvider = _providerFactory.GetProvider(provider);

            if (!aiProvider.IsConfigured)
            {
                return Result<string>.Failure(aiProvider.ProviderName + " is not configured.");
            }

            var systemPromptText = "You are an expert editor and writing assistant. Your task is to refine and improve the given text according to the instructions provided. Maintain the author's voice and style while making improvements.\n\nGuidelines:\n1. Preserve the core meaning and plot points\n2. Maintain consistent character voices\n3. Keep approximately the same length unless instructed otherwise\n4. Return only the refined text, no explanations";

            var userPromptText = "Original text:\n" + content + "\n\nRefinement instructions:\n" + instructions + "\n\nRefined version:";

            var request = new GenerationRequest
            {
                SystemPrompt = systemPromptText,
                UserPrompt = userPromptText,
                Provider = provider,
                ModelId = GetModelForMode(provider, GenerationMode.Standard),
                Temperature = 0.5,
                MaxTokens = EstimateMaxTokensForWordCount(CountWords(content) + 500)
            };

            var result = await aiProvider.GenerateAsync(request, cancellationToken);

            if (result.IsSuccess && result.Value != null)
            {
                _logger.LogDebug("Content refined successfully");
                return Result<string>.Success(result.Value.Content);
            }

            return Result<string>.Failure(result.Error ?? "Refinement failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refine content");
            return Result<string>.Failure("Refinement failed: " + ex.Message, ex);
        }
    }

    /// <inheritdoc />
    public GenerationCostEstimate EstimateCost(ChapterGenerationPipelineRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var modelId = request.ModelId ?? GetModelForMode(request.Provider, request.Mode);

        // Estimate input tokens based on context
        var contextResult = _contextBuilder.BuildChapterContext(
            request.Project,
            request.Chapter,
            new ContextBuildOptions
            {
                IncludeCharacters = request.IncludeCharacterContext,
                IncludeLocations = request.IncludeLocationContext,
                IncludePreviousSummary = request.IncludePreviousSummary
            });

        var inputTokens = contextResult.IsSuccess
            ? _contextBuilder.EstimateContextTokens(contextResult.Value!)
            : 2000;

        // Estimate output tokens based on target word count
        // Roughly 1.3 tokens per word for English prose
        var outputTokens = (int)(request.Chapter.TargetWordCount * 1.3);

        // For premium mode, multiply by 2 for the refinement pass
        if (request.Mode == GenerationMode.Premium)
        {
            inputTokens = (int)(inputTokens * 1.5);
            outputTokens = (int)(outputTokens * 1.3);
        }

        // Calculate cost
        var pricing = ModelPricing.GetValueOrDefault(modelId, (0.003m, 0.015m));
        var inputCost = (inputTokens / 1000m) * pricing.Input;
        var outputCost = (outputTokens / 1000m) * pricing.Output;

        return new GenerationCostEstimate
        {
            EstimatedInputTokens = inputTokens,
            EstimatedOutputTokens = outputTokens,
            EstimatedCostUsd = inputCost + outputCost,
            ModelId = modelId,
            Provider = request.Provider
        };
    }

    /// <inheritdoc />
    public IReadOnlyList<GenerationModeInfo> GetAvailableModes()
    {
        return
        [
            new GenerationModeInfo
            {
                Mode = GenerationMode.Fast,
                Name = "Quick Draft",
                Description = "Fast generation using economical models. Best for drafts and exploration.",
                RecommendedFor = "First drafts, brainstorming, experimentation",
                EstimatedTime = "30-60 seconds",
                CostIndicator = 1
            },
            new GenerationModeInfo
            {
                Mode = GenerationMode.Standard,
                Name = "Standard",
                Description = "Balanced quality and speed using mid-tier models. Good for most chapters.",
                RecommendedFor = "Regular chapter writing, dialogue, descriptions",
                EstimatedTime = "1-2 minutes",
                CostIndicator = 3
            },
            new GenerationModeInfo
            {
                Mode = GenerationMode.HighQuality,
                Name = "Premium",
                Description = "Highest quality using top-tier models with refinement pass. Best for important chapters.",
                RecommendedFor = "Opening chapters, climactic scenes, final drafts",
                EstimatedTime = "3-5 minutes",
                CostIndicator = 5
            }
        ];
    }

    #region Private Methods

    private async Task<GenerationResult> ExecuteSinglePassAsync(
        IAIProvider provider,
        GenerationRequest request,
        IProgress<GenerationProgress>? progress,
        CancellationToken cancellationToken)
    {
        var result = await provider.GenerateAsync(request, cancellationToken);

        if (result.IsSuccess && result.Value != null)
        {
            return result.Value;
        }

        return GenerationResult.Failure(result.Error ?? "Generation failed");
    }

    private async Task<GenerationResult> ExecutePremiumPipelineAsync(
        IAIProvider provider,
        GenerationRequest request,
        IProgress<GenerationProgress>? progress,
        CancellationToken cancellationToken)
    {
        // Pass 1: Initial generation
        progress?.Report(new GenerationProgress
        {
            Phase = "Pass 1: Initial generation...",
            Percentage = 20
        });

        var initialResult = await provider.GenerateAsync(request, cancellationToken);

        if (initialResult.IsFailure || initialResult.Value == null)
        {
            return GenerationResult.Failure(initialResult.Error ?? "Initial generation failed");
        }

        var initialContent = initialResult.Value.Content;
        var initialUsage = initialResult.Value.Usage;

        _logger.LogDebug("Pass 1 complete: {WordCount} words", CountWords(initialContent));

        // Pass 2: Self-review and refinement
        progress?.Report(new GenerationProgress
        {
            Phase = "Pass 2: Reviewing and refining...",
            Percentage = 60,
            WordCount = CountWords(initialContent)
        });

        var refinementPrompt = "Review and improve the following chapter draft. Focus on:\n1. Strengthening prose and eliminating weak phrases\n2. Enhancing sensory details and immersion\n3. Improving dialogue naturalness\n4. Ensuring consistent pacing\n5. Tightening any loose scenes\n\nOriginal draft:\n" + initialContent + "\n\nProvide the improved version:";

        var refinementRequest = new GenerationRequest
        {
            SystemPrompt = request.SystemPrompt + "\n\nYou are now in editing mode. Refine and improve the draft while maintaining its core structure and voice.",
            UserPrompt = refinementPrompt,
            Provider = request.Provider,
            ModelId = request.ModelId,
            Temperature = 0.4, // Lower temperature for refinement
            MaxTokens = request.MaxTokens
        };

        var refinedResult = await provider.GenerateAsync(refinementRequest, cancellationToken);

        if (refinedResult.IsSuccess && refinedResult.Value != null)
        {
            // Combine token usage
            var totalUsage = new TokenUsage
            {
                InputTokens = (initialUsage?.InputTokens ?? 0) + (refinedResult.Value.Usage?.InputTokens ?? 0),
                OutputTokens = (initialUsage?.OutputTokens ?? 0) + (refinedResult.Value.Usage?.OutputTokens ?? 0)
            };

            // Calculate cost
            var pricing = ModelPricing.GetValueOrDefault(request.ModelId ?? "", (0.003m, 0.015m));
            totalUsage.EstimatedCost = (totalUsage.InputTokens / 1000m * pricing.Input) +
                                       (totalUsage.OutputTokens / 1000m * pricing.Output);

            _logger.LogDebug("Pass 2 complete: {WordCount} words", CountWords(refinedResult.Value.Content));

            return new GenerationResult
            {
                IsSuccess = true,
                Content = refinedResult.Value.Content,
                Usage = totalUsage,
                FinishReason = refinedResult.Value.FinishReason
            };
        }

        // If refinement fails, return initial result
        _logger.LogWarning("Refinement pass failed, returning initial result");
        return initialResult.Value;
    }

    private static string GetModelForMode(AIProviderType provider, GenerationMode mode)
    {
        if (ModelMappings.TryGetValue(provider, out var providerModels))
        {
            if (providerModels.TryGetValue(mode, out var modelId))
            {
                return modelId;
            }
        }

        // Default fallback
        return provider switch
        {
            AIProviderType.Claude => "claude-sonnet-4-20250514",
            AIProviderType.OpenAI => "gpt-4o",
            _ => "claude-sonnet-4-20250514"
        };
    }

    private static int EstimateMaxTokensForWordCount(int wordCount)
    {
        // Roughly 1.3 tokens per word, plus buffer
        return (int)(wordCount * 1.3 * 1.2);
    }

    private static int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        return text.Split([' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries).Length;
    }

    #endregion
}
