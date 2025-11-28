// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Runtime.CompilerServices;
using System.Text;
using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Interfaces;
using AIBookAuthorPro.Core.Models;
using AIBookAuthorPro.Core.Models.AI;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.Infrastructure.Services;

/// <summary>
/// Service for generating chapter content using AI providers.
/// </summary>
public sealed class ChapterGeneratorService : IChapterGeneratorService
{
    private readonly IAIProviderFactory _providerFactory;
    private readonly IContextBuilderService _contextBuilder;
    private readonly ITokenCounter _tokenCounter;
    private readonly ILogger<ChapterGeneratorService> _logger;

    public ChapterGeneratorService(
        IAIProviderFactory providerFactory,
        IContextBuilderService contextBuilder,
        ITokenCounter tokenCounter,
        ILogger<ChapterGeneratorService> logger)
    {
        _providerFactory = providerFactory ?? throw new ArgumentNullException(nameof(providerFactory));
        _contextBuilder = contextBuilder ?? throw new ArgumentNullException(nameof(contextBuilder));
        _tokenCounter = tokenCounter ?? throw new ArgumentNullException(nameof(tokenCounter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<Result<GenerationResult>> GenerateChapterAsync(
        ChapterGenerationRequest request,
        IProgress<GenerationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting chapter generation for chapter {ChapterId}", request.Chapter.Id);
            
            progress?.Report(new GenerationProgress 
            { 
                Phase = "Preparing - Building context...", 
                Percentage = 10 
            });

            // Build context for the chapter
            var contextResult = _contextBuilder.BuildChapterContext(
                request.Project, 
                request.Chapter, 
                null);
            
            if (!contextResult.IsSuccess || contextResult.Value == null)
            {
                return Result<GenerationResult>.Failure(contextResult.Error ?? "Failed to build context");
            }
            
            var context = contextResult.Value;

            progress?.Report(new GenerationProgress 
            { 
                Phase = "Generating chapter content...", 
                Percentage = 30 
            });

            // Get the appropriate AI provider
            var provider = _providerFactory.GetProvider(request.ModelId ?? "claude-3-5-sonnet-20241022");

            // Build the generation prompt
            var prompt = BuildChapterPrompt(request, context);

            // Generate content
            var generationRequest = new GenerationRequest
            {
                UserPrompt = prompt,
                MaxTokens = request.MaxTokens,
                Temperature = request.Temperature,
                SystemPrompt = BuildSystemPrompt(request.Project)
            };

            var result = await provider.GenerateAsync(generationRequest, cancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Chapter generation failed: {Error}", result.Error);
                return Result<GenerationResult>.Failure(result.Error ?? "Generation failed");
            }

            progress?.Report(new GenerationProgress 
            { 
                Phase = "Complete - Chapter generated successfully", 
                Percentage = 100 
            });

            _logger.LogInformation("Chapter generation completed for chapter {ChapterId}", request.Chapter.Id);

            return Result<GenerationResult>.Success(result.Value!);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Chapter generation cancelled for chapter {ChapterId}", request.Chapter.Id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating chapter {ChapterId}", request.Chapter.Id);
            return Result<GenerationResult>.Failure($"Error generating chapter: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result<string>> ContinueWritingAsync(
        Chapter chapter,
        Project project,
        string? additionalInstructions = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var provider = _providerFactory.GetDefaultProvider();
            
            var prompt = new StringBuilder();
            prompt.AppendLine("Continue writing from where the chapter left off. Maintain the same voice, style, and pacing.");
            
            if (!string.IsNullOrWhiteSpace(additionalInstructions))
            {
                prompt.AppendLine($"\nAdditional instructions: {additionalInstructions}");
            }

            prompt.AppendLine($"\nCurrent chapter content:\n{chapter.Content}");
            prompt.AppendLine("\nContinue the story:");

            var request = new GenerationRequest
            {
                UserPrompt = prompt.ToString(),
                MaxTokens = 2000,
                Temperature = 0.7,
                SystemPrompt = BuildSystemPrompt(project)
            };

            var result = await provider.GenerateAsync(request, cancellationToken);

            return result.IsSuccess
                ? Result<string>.Success(result.Value!.Content)
                : Result<string>.Failure(result.Error ?? "Failed to continue writing");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error continuing chapter {ChapterId}", chapter.Id);
            return Result<string>.Failure($"Error continuing chapter: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result<string>> RewriteSectionAsync(
        Chapter chapter,
        string selectedText,
        string instructions,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var provider = _providerFactory.GetDefaultProvider();

            var prompt = $@"Rewrite the following section according to these instructions:

Instructions: {instructions}

Section to rewrite:
{selectedText}

Rewritten section:";

            var request = new GenerationRequest
            {
                UserPrompt = prompt,
                MaxTokens = Math.Max(1000, _tokenCounter.EstimateTokens(selectedText) * 2),
                Temperature = 0.7
            };

            var result = await provider.GenerateAsync(request, cancellationToken);

            return result.IsSuccess
                ? Result<string>.Success(result.Value!.Content)
                : Result<string>.Failure(result.Error ?? "Failed to rewrite section");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rewriting section in chapter {ChapterId}", chapter.Id);
            return Result<string>.Failure($"Error rewriting section: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result<string>> GenerateChapterOutlineAsync(
        Chapter chapter,
        Project project,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var provider = _providerFactory.GetDefaultProvider();

            var prompt = $@"Create a detailed outline for Chapter {chapter.Order}: {chapter.Title}

Book: {project.Metadata.Title}
Genre: {project.Metadata.Genre}

Chapter Summary: {chapter.Summary}

Provide a structured outline including:
1. Opening scene and hook
2. Key plot points
3. Character development moments
4. Conflicts and tension
5. Chapter ending/cliffhanger

Outline:";

            var request = new GenerationRequest
            {
                UserPrompt = prompt,
                MaxTokens = 1500,
                Temperature = 0.6,
                SystemPrompt = BuildSystemPrompt(project)
            };

            var result = await provider.GenerateAsync(request, cancellationToken);

            return result.IsSuccess
                ? Result<string>.Success(result.Value!.Content)
                : Result<string>.Failure(result.Error ?? "Failed to generate outline");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating outline for chapter {ChapterId}", chapter.Id);
            return Result<string>.Failure($"Error generating outline: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<string> StreamGenerateChapterAsync(
        ChapterGenerationRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Pass null for options - BuildChapterContext will use defaults
        var contextResult = _contextBuilder.BuildChapterContext(
            request.Project, 
            request.Chapter, 
            null);
        
        if (!contextResult.IsSuccess || contextResult.Value == null)
        {
            yield break;
        }
        
        var context = contextResult.Value;
        var provider = _providerFactory.GetProvider(request.ModelId ?? "claude-3-5-sonnet-20241022");
        var prompt = BuildChapterPrompt(request, context);

        var generationRequest = new GenerationRequest
        {
            UserPrompt = prompt,
            MaxTokens = request.MaxTokens,
            Temperature = request.Temperature,
            SystemPrompt = BuildSystemPrompt(request.Project)
        };

        // Streaming not supported in current API, generate synchronously
        var result = await provider.GenerateAsync(generationRequest, cancellationToken);
        if (result.IsSuccess && result.Value != null)
        {
            // Return content as single chunk
            yield return result.Value.Content;
        }
    }

    /// <inheritdoc />
    public Task<GenerationCostEstimate> EstimateGenerationCostAsync(ChapterGenerationRequest request)
    {
        var modelId = request.ModelId ?? "claude-3-5-sonnet-20241022";
        
        // Estimate input tokens from context
        var contextTokens = 0;
        if (request.IncludeCharacters) contextTokens += 500;
        if (request.IncludeLocations) contextTokens += 300;
        if (request.IncludeOutline) contextTokens += 400;
        if (request.IncludePreviousChapter) contextTokens += 2000;
        
        // contextTokens += _tokenCounter.EstimateTokens(request.Chapter.Synopsis); // Chapter.Synopsis not available
        contextTokens += _tokenCounter.EstimateTokens(request.AdditionalInstructions);

        var outputTokens = request.MaxTokens;
        var estimatedCost = _tokenCounter.EstimateCost(modelId, contextTokens, outputTokens);

        return Task.FromResult(new GenerationCostEstimate
        {
            EstimatedInputTokens = contextTokens,
            EstimatedOutputTokens = outputTokens,
            EstimatedCostUsd = estimatedCost,
            ModelId = modelId
        });
    }

    private static string BuildSystemPrompt(Project project)
    {
        return $@"You are a professional fiction writer with expertise in {project.Metadata.Genre} novels. 
Your writing style should be engaging, immersive, and appropriate for the target audience.

Book: {project.Metadata.Title}
Genre: {project.Metadata.Genre}
Writing Style: {project.Metadata.WritingStyle}
POV: {project.Metadata.PointOfView}
Tense: {project.Metadata.Tense}

Write in a compelling narrative voice, with vivid descriptions, natural dialogue, and well-paced action.
Show emotions through actions and dialogue rather than telling.
Maintain consistency with established characters, settings, and plot elements.";
    }

    private static string BuildChapterPrompt(ChapterGenerationRequest request, GenerationContext context)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine($"Write Chapter {request.Chapter.Order}: {request.Chapter.Title}");
        sb.AppendLine();
        
        // Note: Chapter.Synopsis and context properties may not be available in current API
        // if (!string.IsNullOrWhiteSpace(request.Chapter.Synopsis))
        // {
        //     sb.AppendLine($"Chapter Synopsis: {request.Chapter.Synopsis}");
        //     sb.AppendLine();
        // }

        // if (!string.IsNullOrWhiteSpace(context.PreviousChapterSummary))
        // {
        //     sb.AppendLine("Previous Chapter Summary:");
        //     sb.AppendLine(context.PreviousChapterSummary);
        //     sb.AppendLine();
        // }

        // if (context.RelevantCharacters != null && context.RelevantCharacters.Count > 0)
        // {
        //     sb.AppendLine("Characters in this chapter:");
        //     foreach (var character in context.RelevantCharacters)
        //     {
        //         sb.AppendLine($"- {character.Name}: {character.Description}");
        //     }
        //     sb.AppendLine();
        // }

        // if (context.RelevantLocations != null && context.RelevantLocations.Count > 0)
        // {
        //     sb.AppendLine("Settings in this chapter:");
        //     foreach (var location in context.RelevantLocations)
        //     {
        //         sb.AppendLine($"- {location.Name}: {location.Description}");
        //     }
        //     sb.AppendLine();
        // }

        if (!string.IsNullOrWhiteSpace(request.AdditionalInstructions))
        {
            sb.AppendLine($"Additional instructions: {request.AdditionalInstructions}");
            sb.AppendLine();
        }

        sb.AppendLine("Begin writing the chapter:");

        return sb.ToString();
    }
}
