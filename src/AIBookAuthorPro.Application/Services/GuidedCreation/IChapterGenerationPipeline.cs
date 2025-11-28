// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Models.GuidedCreation;
using AIBookAuthorPro.Infrastructure.Services.GuidedCreation;

namespace AIBookAuthorPro.Application.Services.GuidedCreation;

/// <summary>
/// Pipeline for generating a single chapter through multiple steps.
/// </summary>
public interface IChapterGenerationPipeline
{
    /// <summary>
    /// Executes the full pipeline to generate a chapter.
    /// </summary>
    Task<Result<GeneratedChapter>> ExecuteAsync(
        ChapterGenerationContext context,
        IProgress<PipelineProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all pipeline steps.
    /// </summary>
    IReadOnlyList<IPipelineStep> GetSteps();

    /// <summary>
    /// Adds a custom step to the pipeline.
    /// </summary>
    void AddStep(IPipelineStep step);

    /// <summary>
    /// Removes a step from the pipeline.
    /// </summary>
    void RemoveStep(string stepName);
}

/// <summary>
/// Context for chapter generation containing all necessary data.
/// </summary>
public class ChapterGenerationContext
{
    public int ChapterNumber { get; set; }
    public ChapterBlueprint? ChapterBlueprint { get; set; }
    public BookBlueprint? Blueprint { get; set; }
    public string SystemPrompt { get; set; } = string.Empty;
    public string NarrativeContext { get; set; } = string.Empty;
    public string CharacterContext { get; set; } = string.Empty;
    public string WorldContext { get; set; } = string.Empty;
    public string PlotContext { get; set; } = string.Empty;
    public string StyleContext { get; set; } = string.Empty;
    public string ChapterInstructions { get; set; } = string.Empty;
    public List<ChapterSummary>? PreviousChapterSummaries { get; set; }
    public List<CharacterStateSnapshot>? CharacterStates { get; set; }
    public List<SetupPayoff>? ActiveSetups { get; set; }
    public List<SetupPayoff>? DuePayoffs { get; set; }
    public TokenBudget? TokenBudget { get; set; }
    public GenerationConfiguration? GenerationConfig { get; set; }
}

/// <summary>
/// Token budget allocation for context.
/// </summary>
public class TokenBudget
{
    public int TotalAvailable { get; set; }
    public int SystemPrompt { get; set; }
    public int NarrativeContext { get; set; }
    public int CharacterContext { get; set; }
    public int WorldContext { get; set; }
    public int PlotContext { get; set; }
    public int StyleContext { get; set; }
    public int ChapterInstructions { get; set; }
    public int Reserved { get; set; }
}
