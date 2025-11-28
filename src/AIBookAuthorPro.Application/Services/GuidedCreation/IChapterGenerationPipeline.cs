// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Models.GuidedCreation;

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
        PipelineContext context,
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
/// Token budget allocation for context.
/// </summary>
public sealed class TokenBudget
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
