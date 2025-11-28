// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Models.GuidedCreation;

namespace AIBookAuthorPro.Application.Services.GuidedCreation;

/// <summary>
/// Pipeline for generating a single chapter through multiple processing steps.
/// </summary>
public interface IChapterGenerationPipeline
{
    /// <summary>
    /// Executes the complete chapter generation pipeline.
    /// </summary>
    /// <param name="context">Chapter generation context.</param>
    /// <param name="progress">Progress reporter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Generated chapter.</returns>
    Task<Result<GeneratedChapter>> ExecuteAsync(
        ChapterGenerationContext context,
        IProgress<PipelineProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the pipeline steps.
    /// </summary>
    /// <returns>Ordered list of pipeline steps.</returns>
    IReadOnlyList<IPipelineStep> GetSteps();

    /// <summary>
    /// Adds a step to the pipeline.
    /// </summary>
    /// <param name="step">The step to add.</param>
    /// <returns>The pipeline for chaining.</returns>
    IChapterGenerationPipeline AddStep(IPipelineStep step);

    /// <summary>
    /// Removes a step from the pipeline.
    /// </summary>
    /// <param name="stepName">Name of the step to remove.</param>
    /// <returns>The pipeline for chaining.</returns>
    IChapterGenerationPipeline RemoveStep(string stepName);
}

/// <summary>
/// A single step in the generation pipeline.
/// </summary>
public interface IPipelineStep
{
    /// <summary>
    /// Step name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Step order.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Whether this step is required.
    /// </summary>
    bool IsRequired { get; }

    /// <summary>
    /// Whether this step can be retried on failure.
    /// </summary>
    bool CanRetry { get; }

    /// <summary>
    /// Maximum retry attempts.
    /// </summary>
    int MaxRetries { get; }

    /// <summary>
    /// Executes the pipeline step.
    /// </summary>
    /// <param name="state">Current pipeline state.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated pipeline state.</returns>
    Task<Result<PipelineState>> ExecuteAsync(
        PipelineState state,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Pipeline execution state passed between steps.
/// </summary>
public sealed class PipelineState
{
    /// <summary>
    /// Chapter generation context.
    /// </summary>
    public required ChapterGenerationContext Context { get; init; }

    /// <summary>
    /// Blueprint for the chapter.
    /// </summary>
    public required ChapterBlueprint Blueprint { get; init; }

    /// <summary>
    /// Generated outline if available.
    /// </summary>
    public string? GeneratedOutline { get; set; }

    /// <summary>
    /// Generated scenes.
    /// </summary>
    public List<GeneratedScene> GeneratedScenes { get; init; } = new();

    /// <summary>
    /// Assembled chapter content.
    /// </summary>
    public string? AssembledContent { get; set; }

    /// <summary>
    /// Quality report.
    /// </summary>
    public ComprehensiveQualityReport? QualityReport { get; set; }

    /// <summary>
    /// Continuity report.
    /// </summary>
    public ContinuityReport? ContinuityReport { get; set; }

    /// <summary>
    /// Revision instructions if needed.
    /// </summary>
    public List<RevisionInstruction> RevisionInstructions { get; init; } = new();

    /// <summary>
    /// Final generated chapter.
    /// </summary>
    public GeneratedChapter? FinalChapter { get; set; }

    /// <summary>
    /// Step results.
    /// </summary>
    public Dictionary<string, StepResult> StepResults { get; init; } = new();

    /// <summary>
    /// Token usage so far.
    /// </summary>
    public TokenUsage TokenUsage { get; set; } = new(0, 0);

    /// <summary>
    /// Elapsed time.
    /// </summary>
    public TimeSpan ElapsedTime { get; set; }

    /// <summary>
    /// Errors encountered.
    /// </summary>
    public List<string> Errors { get; init; } = new();

    /// <summary>
    /// Warnings.
    /// </summary>
    public List<string> Warnings { get; init; } = new();
}

/// <summary>
/// Result of a pipeline step execution.
/// </summary>
public sealed record StepResult
{
    /// <summary>
    /// Step name.
    /// </summary>
    public required string StepName { get; init; }

    /// <summary>
    /// Whether step succeeded.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Execution time.
    /// </summary>
    public required TimeSpan ExecutionTime { get; init; }

    /// <summary>
    /// Retry count.
    /// </summary>
    public int RetryCount { get; init; }

    /// <summary>
    /// Token usage.
    /// </summary>
    public TokenUsage? TokenUsage { get; init; }

    /// <summary>
    /// Error message if failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Output data.
    /// </summary>
    public Dictionary<string, object>? OutputData { get; init; }
}

/// <summary>
/// Pipeline progress report.
/// </summary>
public sealed record PipelineProgress
{
    /// <summary>
    /// Current step name.
    /// </summary>
    public required string CurrentStep { get; init; }

    /// <summary>
    /// Current step number.
    /// </summary>
    public required int CurrentStepNumber { get; init; }

    /// <summary>
    /// Total steps.
    /// </summary>
    public required int TotalSteps { get; init; }

    /// <summary>
    /// Overall progress (0-100).
    /// </summary>
    public required double OverallProgress { get; init; }

    /// <summary>
    /// Step progress (0-100).
    /// </summary>
    public required double StepProgress { get; init; }

    /// <summary>
    /// Status message.
    /// </summary>
    public required string StatusMessage { get; init; }

    /// <summary>
    /// Is streaming content.
    /// </summary>
    public bool IsStreaming { get; init; }

    /// <summary>
    /// Latest content chunk if streaming.
    /// </summary>
    public string? LatestChunk { get; init; }

    /// <summary>
    /// Words generated so far.
    /// </summary>
    public int WordsGenerated { get; init; }
}

// ================== PIPELINE STEP IMPLEMENTATIONS ==================

/// <summary>
/// Step 1: Build context for generation.
/// </summary>
public interface IBuildContextStep : IPipelineStep { }

/// <summary>
/// Step 2: Generate chapter outline.
/// </summary>
public interface IGenerateOutlineStep : IPipelineStep { }

/// <summary>
/// Step 3: Generate scenes.
/// </summary>
public interface IGenerateScenesStep : IPipelineStep { }

/// <summary>
/// Step 4: Assemble chapter from scenes.
/// </summary>
public interface IAssembleChapterStep : IPipelineStep { }

/// <summary>
/// Step 5: Check continuity.
/// </summary>
public interface IContinuityCheckStep : IPipelineStep { }

/// <summary>
/// Step 6: Check style consistency.
/// </summary>
public interface IStyleConsistencyStep : IPipelineStep { }

/// <summary>
/// Step 7: Evaluate quality.
/// </summary>
public interface IQualityEvaluationStep : IPipelineStep { }

/// <summary>
/// Step 8: Perform revisions if needed.
/// </summary>
public interface IRevisionStep : IPipelineStep { }

/// <summary>
/// Step 9: Finalize chapter.
/// </summary>
public interface IFinalizeStep : IPipelineStep { }
