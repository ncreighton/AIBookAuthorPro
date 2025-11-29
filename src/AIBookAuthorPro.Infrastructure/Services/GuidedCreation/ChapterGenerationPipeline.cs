// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Application.Services.GuidedCreation;
using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Models.GuidedCreation;
using AIBookAuthorPro.Core.Services;
using CoreGenerationOptions = AIBookAuthorPro.Core.Services.GenerationOptions;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.Infrastructure.Services.GuidedCreation;

/// <summary>
/// Pipeline for generating a single chapter through multiple steps.
/// </summary>
public sealed class ChapterGenerationPipeline : IChapterGenerationPipeline
{
    private readonly IAIService _aiService;
    private readonly IQualityEvaluationService _qualityService;
    private readonly IContinuityVerificationService _continuityService;
    private readonly ILogger<ChapterGenerationPipeline> _logger;

    private readonly List<IInternalPipelineStep> _steps;

    public ChapterGenerationPipeline(
        IAIService aiService,
        IQualityEvaluationService qualityService,
        IContinuityVerificationService continuityService,
        ILogger<ChapterGenerationPipeline> logger)
    {
        _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
        _qualityService = qualityService ?? throw new ArgumentNullException(nameof(qualityService));
        _continuityService = continuityService ?? throw new ArgumentNullException(nameof(continuityService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _steps = InitializePipelineSteps();
    }

    /// <inheritdoc />
    public async Task<Result<GeneratedChapter>> ExecuteAsync(
        PipelineContext context,
        IProgress<PipelineProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (context == null)
            return Result<GeneratedChapter>.Failure("Context cannot be null");

        _logger.LogInformation("Starting chapter generation pipeline for Chapter {ChapterNumber}",
            context.ChapterNumber);

        var pipelineState = new PipelineState
        {
            Context = context,
            CurrentStepIndex = 0,
            StartedAt = DateTime.UtcNow
        };

        try
        {
            foreach (var step in _steps)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var stepProgress = (double)pipelineState.CurrentStepIndex / _steps.Count * 100;

                progress?.Report(new PipelineProgress
                {
                    CurrentStep = step.Name,
                    StepNumber = pipelineState.CurrentStepIndex + 1,
                    TotalSteps = _steps.Count,
                    Percentage = stepProgress,
                    Message = $"Executing step: {step.Name}"
                });

                _logger.LogDebug("Executing pipeline step: {StepName}", step.Name);

                var stepResult = await step.ExecuteAsync(pipelineState, cancellationToken);

                if (stepResult.IsFailure)
                {
                    _logger.LogWarning("Pipeline step {StepName} failed: {Error}", step.Name, stepResult.Error);

                    // Some steps are optional
                    if (step.IsRequired)
                    {
                        return Result<GeneratedChapter>.Failure(
                            $"Pipeline failed at step '{step.Name}': {stepResult.Error}");
                    }
                }

                pipelineState.CurrentStepIndex++;
            }

            // Build final chapter from pipeline state
            var chapter = BuildChapterFromState(pipelineState);

            progress?.Report(new PipelineProgress
            {
                CurrentStep = "Complete",
                StepNumber = _steps.Count,
                TotalSteps = _steps.Count,
                Percentage = 100,
                Message = "Chapter generation complete"
            });

            _logger.LogInformation("Chapter generation pipeline completed for Chapter {ChapterNumber}",
                context.ChapterNumber);

            return Result<GeneratedChapter>.Success(chapter);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Pipeline cancelled at step {Step}", pipelineState.CurrentStepIndex);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Pipeline error at step {Step}", pipelineState.CurrentStepIndex);
            return Result<GeneratedChapter>.Failure($"Pipeline error: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<AIBookAuthorPro.Core.Models.GuidedCreation.IPipelineStep> GetSteps()
    {
        // Convert internal steps to Core interface
        return _steps.Select(s => new CorePipelineStepAdapter(s)).ToList().AsReadOnly();
    }

    /// <inheritdoc />
    public void AddStep(AIBookAuthorPro.Core.Models.GuidedCreation.IPipelineStep step)
    {
        // Wrap Core step for internal use
        _steps.Add(new InternalPipelineStepAdapter(step));
        _steps.Sort((a, b) => a.Order.CompareTo(b.Order));
    }

    /// <inheritdoc />
    public void RemoveStep(string stepName)
    {
        _steps.RemoveAll(s => s.Name == stepName);
    }

    // Adapter to convert internal step to Core interface
    private sealed class CorePipelineStepAdapter : AIBookAuthorPro.Core.Models.GuidedCreation.IPipelineStep
    {
        private readonly IInternalPipelineStep _internalStep;
        public CorePipelineStepAdapter(IInternalPipelineStep step) => _internalStep = step;
        public string Name => _internalStep.Name;
        public int Order => _internalStep.Order;
        public bool IsEnabled => true;
        public async Task<PipelineStepResult> ExecuteAsync(PipelineContext context, CancellationToken cancellationToken)
        {
            // Note: This adapter can't fully execute since it uses different state types
            return new PipelineStepResult { Success = true, StepName = _internalStep.Name };
        }
    }

    // Adapter to use Core step internally
    private sealed class InternalPipelineStepAdapter : IInternalPipelineStep
    {
        private readonly AIBookAuthorPro.Core.Models.GuidedCreation.IPipelineStep _coreStep;
        public InternalPipelineStepAdapter(AIBookAuthorPro.Core.Models.GuidedCreation.IPipelineStep step) => _coreStep = step;
        public string Name => _coreStep.Name;
        public int Order => _coreStep.Order;
        public bool IsRequired => _coreStep.IsEnabled;
        public Task<Result> ExecuteAsync(PipelineState state, CancellationToken cancellationToken)
        {
            // This is a simplified adapter
            return Task.FromResult(Result.Success());
        }
    }

    #region Private Methods

    private List<IInternalPipelineStep> InitializePipelineSteps()
    {
        return new List<IInternalPipelineStep>
        {
            new BuildContextStep(_aiService, _logger, 1),
            new GenerateOutlineStep(_aiService, _logger, 2),
            new GenerateScenesStep(_aiService, _logger, 3),
            new AssembleChapterStep(_logger, 4),
            new ContinuityCheckStep(_continuityService, _logger, 5),
            new StyleConsistencyStep(_aiService, _logger, 6),
            new QualityEvaluationStep(_qualityService, _logger, 7),
            new RevisionStep(_aiService, _logger, 8),
            new FinalizeStep(_logger, 9)
        };
    }

    private static GeneratedChapter BuildChapterFromState(PipelineState state)
    {
        return new GeneratedChapter
        {
            Id = Guid.NewGuid(),
            ChapterNumber = state.Context.ChapterNumber,
            Title = state.Context.ChapterBlueprint?.Title ?? $"Chapter {state.Context.ChapterNumber}",
            Content = state.GeneratedContent ?? string.Empty,
            WordCount = CountWords(state.GeneratedContent ?? string.Empty),
            Status = state.QualityReport?.OverallScore >= 60
                ? ChapterGenerationStatus.Approved
                : ChapterGenerationStatus.NeedsReview,
            QualityReport = state.QualityReport,
            // QualityScore is computed from QualityReport, no need to set
            ContinuityReport = state.ContinuityReport,
            GeneratedAt = DateTime.UtcNow,
            GenerationDurationMs = (int)(DateTime.UtcNow - state.StartedAt).TotalMilliseconds,
            Summaries = state.Summaries,
            CharacterStatesAtEnd = state.CharacterStates ?? new List<CharacterStateSnapshot>()
        };
    }

    private static int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        return text.Split(new[] { ' ', '\n', '\r', '\t' },
            StringSplitOptions.RemoveEmptyEntries).Length;
    }

    #endregion
}

/// <summary>
/// Pipeline state passed between steps.
/// </summary>
public class PipelineState
{
    public PipelineContext Context { get; set; } = null!;
    public int CurrentStepIndex { get; set; }
    public DateTime StartedAt { get; set; }
    public string? ChapterOutline { get; set; }
    public List<string>? GeneratedScenes { get; set; }
    public string? GeneratedContent { get; set; }
    public ComprehensiveQualityReport? QualityReport { get; set; }
    public ContinuityReport? ContinuityReport { get; set; }
    public ChapterSummaries? Summaries { get; set; }
    public List<CharacterStateSnapshot>? CharacterStates { get; set; }
    public int RevisionCount { get; set; }
}

/// <summary>
/// Internal pipeline progress report.
/// </summary>
internal class InternalPipelineProgress
{
    public string CurrentStep { get; set; } = string.Empty;
    public int StepIndex { get; set; }
    public int TotalSteps { get; set; }
    public double OverallProgress { get; set; }
    public double StepProgress { get; set; }
    
    public PipelineProgress ToCoreProgress() => new PipelineProgress
    {
        CurrentStep = CurrentStep,
        StepNumber = StepIndex + 1,
        TotalSteps = TotalSteps,
        Percentage = OverallProgress,
        Message = $"Processing step {StepIndex + 1} of {TotalSteps}: {CurrentStep}"
    };
}

/// <summary>
/// Local pipeline step interface for internal implementation.
/// </summary>
internal interface IInternalPipelineStep
{
    string Name { get; }
    int Order { get; }
    bool IsRequired { get; }
    Task<Result> ExecuteAsync(PipelineState state, CancellationToken cancellationToken);
}

#region Pipeline Step Implementations

/// <summary>
/// Step 1: Build comprehensive context.
/// </summary>
public class BuildContextStep : IInternalPipelineStep
{
    private readonly IAIService _aiService;
    private readonly ILogger _logger;

    public string Name => "Build Context";
    public int Order { get; }
    public bool IsRequired => true;

    public BuildContextStep(IAIService aiService, ILogger logger, int order)
    {
        _aiService = aiService;
        _logger = logger;
        Order = order;
    }

    public async Task<Result> ExecuteAsync(PipelineState state, CancellationToken cancellationToken)
    {
        // Context is already built by the time we get here
        _logger.LogDebug("Context validated for Chapter {Chapter}", state.Context.ChapterNumber);
        return Result.Success();
    }
}

/// <summary>
/// Step 2: Generate chapter outline.
/// </summary>
public class GenerateOutlineStep : IInternalPipelineStep
{
    private readonly IAIService _aiService;
    private readonly ILogger _logger;

    public string Name => "Generate Outline";
    public int Order { get; }
    public bool IsRequired => true;

    public GenerateOutlineStep(IAIService aiService, ILogger logger, int order)
    {
        _aiService = aiService;
        _logger = logger;
        Order = order;
    }

    public async Task<Result> ExecuteAsync(PipelineState state, CancellationToken cancellationToken)
    {
        var prompt = $@"Create a detailed outline for this chapter.

Chapter {state.Context.ChapterNumber}: {state.Context.ChapterBlueprint?.Title}
Purpose: {state.Context.ChapterBlueprint?.Purpose}

Provide a scene-by-scene outline with key beats, character actions, and emotional moments.";

        var result = await _aiService.GenerateAsync(
            prompt,
            new CoreGenerationOptions { Temperature = 0.6, MaxTokens = 1500 },
            cancellationToken);

        if (result.IsSuccess)
        {
            state.ChapterOutline = result.Value;
            return Result.Success();
        }

        return Result.Failure(result.Error ?? "Failed to generate outline");
    }
}

/// <summary>
/// Step 3: Generate scenes.
/// </summary>
public class GenerateScenesStep : IInternalPipelineStep
{
    private readonly IAIService _aiService;
    private readonly ILogger _logger;

    public string Name => "Generate Scenes";
    public int Order { get; }
    public bool IsRequired => true;

    public GenerateScenesStep(IAIService aiService, ILogger logger, int order)
    {
        _aiService = aiService;
        _logger = logger;
        Order = order;
    }

    public async Task<Result> ExecuteAsync(PipelineState state, CancellationToken cancellationToken)
    {
        var scenes = state.Context.ChapterBlueprint?.Scenes ?? new List<SceneBlueprint>();
        state.GeneratedScenes = new List<string>();

        if (!scenes.Any())
        {
            // Generate as single piece
            var fullPrompt = BuildFullChapterPrompt(state);
            var result = await _aiService.GenerateAsync(
                fullPrompt,
                new CoreGenerationOptions
                {
                    Temperature = 0.8,
                    MaxTokens = (int)(state.Context.ChapterBlueprint?.TargetWordCount ?? 3000) * 2
                },
                cancellationToken);

            if (result.IsSuccess)
            {
                state.GeneratedScenes.Add(result.Value!);
            }
            else
            {
                return Result.Failure(result.Error ?? "Failed to generate chapter content");
            }
        }
        else
        {
            // Generate scene by scene
            foreach (var scene in scenes.OrderBy(s => s.Order))
            {
                var scenePrompt = BuildScenePrompt(state, scene);
                var result = await _aiService.GenerateAsync(
                    scenePrompt,
                    new CoreGenerationOptions { Temperature = 0.8, MaxTokens = 2000 },
                    cancellationToken);

                if (result.IsSuccess)
                {
                    state.GeneratedScenes.Add(result.Value!);
                }
            }
        }

        return Result.Success();
    }

    private static string BuildFullChapterPrompt(PipelineState state)
    {
        return $@"{state.Context.SystemPrompt}

{state.Context.NarrativeContext}

{state.Context.CharacterContext}

{state.Context.ChapterInstructions}

Write the complete chapter (~{state.Context.ChapterBlueprint?.TargetWordCount ?? 3000} words):";
    }

    private static string BuildScenePrompt(PipelineState state, SceneBlueprint scene)
    {
        return $@"Write Scene {scene.Order}: {scene.Title}

Location: {scene.Location}
CharacterBible: {string.Join(", ", scene.Characters ?? new List<string>())}
Purpose: {scene.Purpose}

Previous scenes written:
{string.Join("\n---\n", state.GeneratedScenes?.TakeLast(2) ?? Enumerable.Empty<string>())}

Write this scene (~{(scene.TargetWordCount > 0 ? scene.TargetWordCount : 500)} words):";
    }
}

/// <summary>
/// Step 4: Assemble chapter from scenes.
/// </summary>
public class AssembleChapterStep : IInternalPipelineStep
{
    private readonly ILogger _logger;

    public string Name => "Assemble Chapter";
    public int Order { get; }
    public bool IsRequired => true;

    public AssembleChapterStep(ILogger logger, int order)
    {
        _logger = logger;
        Order = order;
    }

    public Task<Result> ExecuteAsync(PipelineState state, CancellationToken cancellationToken)
    {
        if (state.GeneratedScenes == null || !state.GeneratedScenes.Any())
        {
            return Task.FromResult(Result.Failure("No scenes to assemble"));
        }

        state.GeneratedContent = string.Join("\n\n", state.GeneratedScenes);
        return Task.FromResult(Result.Success());
    }
}

/// <summary>
/// Step 5: Continuity check.
/// </summary>
public class ContinuityCheckStep : IInternalPipelineStep
{
    private readonly IContinuityVerificationService _continuityService;
    private readonly ILogger _logger;

    public string Name => "Continuity Check";
    public int Order { get; }
    public bool IsRequired => false;

    public ContinuityCheckStep(IContinuityVerificationService continuityService, ILogger logger, int order)
    {
        _continuityService = continuityService;
        _logger = logger;
        Order = order;
    }

    public async Task<Result> ExecuteAsync(PipelineState state, CancellationToken cancellationToken)
    {
        var tempChapter = new GeneratedChapter
        {
            Id = Guid.NewGuid(),
            ChapterNumber = state.Context.ChapterNumber,
            Content = state.GeneratedContent ?? ""
        };

        var context = new ContinuityVerificationContext
        {
            CharacterBible = state.Context.Blueprint?.Characters,
            World = state.Context.Blueprint?.World,
            PreviousChapters = state.Context.PreviousChapters ?? new List<GeneratedChapter>()
        };

        var result = await _continuityService.VerifyChapterContinuityAsync(
            tempChapter, context, cancellationToken);

        if (result.IsSuccess)
        {
            state.ContinuityReport = result.Value;
        }

        return Result.Success();
    }
}

/// <summary>
/// Step 6: Style consistency check.
/// </summary>
public class StyleConsistencyStep : IInternalPipelineStep
{
    private readonly IAIService _aiService;
    private readonly ILogger _logger;

    public string Name => "Style Consistency";
    public int Order { get; }
    public bool IsRequired => false;

    public StyleConsistencyStep(IAIService aiService, ILogger logger, int order)
    {
        _aiService = aiService;
        _logger = logger;
        Order = order;
    }

    public Task<Result> ExecuteAsync(PipelineState state, CancellationToken cancellationToken)
    {
        // Style check is handled as part of quality evaluation
        return Task.FromResult(Result.Success());
    }
}

/// <summary>
/// Step 7: Quality evaluation.
/// </summary>
public class QualityEvaluationStep : IInternalPipelineStep
{
    private readonly IQualityEvaluationService _qualityService;
    private readonly ILogger _logger;

    public string Name => "Quality Evaluation";
    public int Order { get; }
    public bool IsRequired => false;

    public QualityEvaluationStep(IQualityEvaluationService qualityService, ILogger logger, int order)
    {
        _qualityService = qualityService;
        _logger = logger;
        Order = order;
    }

    public async Task<Result> ExecuteAsync(PipelineState state, CancellationToken cancellationToken)
    {
        var tempChapter = new GeneratedChapter
        {
            Id = Guid.NewGuid(),
            ChapterNumber = state.Context.ChapterNumber,
            Content = state.GeneratedContent ?? ""
        };

        var context = new QualityEvaluationContext
        {
            Blueprint = state.Context.ChapterBlueprint,
            CharacterBible = state.Context.Blueprint?.Characters,
            World = state.Context.Blueprint?.World,
            Style = state.Context.Blueprint?.Style
        };

        var result = await _qualityService.EvaluateChapterAsync(
            tempChapter,
            state.Context.ChapterBlueprint!,
            context,
            cancellationToken);

        if (result.IsSuccess)
        {
            state.QualityReport = result.Value;
        }

        return Result.Success();
    }
}

/// <summary>
/// Step 8: Revision if needed.
/// </summary>
public class RevisionStep : IInternalPipelineStep
{
    private readonly IAIService _aiService;
    private readonly ILogger _logger;
    private const int MaxRevisions = 2;

    public string Name => "Revision";
    public int Order { get; }
    public bool IsRequired => false;

    public RevisionStep(IAIService aiService, ILogger logger, int order)
    {
        _aiService = aiService;
        _logger = logger;
        Order = order;
    }

    public async Task<Result> ExecuteAsync(PipelineState state, CancellationToken cancellationToken)
    {
        if (state.QualityReport == null || state.QualityReport.OverallScore >= 70)
        {
            return Result.Success();
        }

        if (state.RevisionCount >= MaxRevisions)
        {
            _logger.LogWarning("Max revisions reached for Chapter {Chapter}", state.Context.ChapterNumber);
            return Result.Success();
        }

        var revisionInstructions = state.QualityReport.RevisionInstructions?.Take(5).ToList()
            ?? new List<RevisionInstruction>();

        if (!revisionInstructions.Any())
        {
            return Result.Success();
        }

        var prompt = $@"Revise this chapter based on the following feedback:

{string.Join("\n", revisionInstructions.Select(i => $"- {i.Instruction}"))}

Current chapter:
{state.GeneratedContent}

Provide the fully revised chapter:";

        var result = await _aiService.GenerateAsync(
            prompt,
            new CoreGenerationOptions { Temperature = 0.6, MaxTokens = (state.GeneratedContent?.Length ?? 3000) * 2 },
            cancellationToken);

        if (result.IsSuccess)
        {
            state.GeneratedContent = result.Value;
            state.RevisionCount++;
        }

        return Result.Success();
    }
}

/// <summary>
/// Step 9: Finalize chapter.
/// </summary>
public class FinalizeStep : IInternalPipelineStep
{
    private readonly ILogger _logger;

    public string Name => "Finalize";
    public int Order { get; }
    public bool IsRequired => true;

    public FinalizeStep(ILogger logger, int order)
    {
        _logger = logger;
        Order = order;
    }

    public Task<Result> ExecuteAsync(PipelineState state, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(state.GeneratedContent))
        {
            return Task.FromResult(Result.Failure("No content generated"));
        }

        _logger.LogInformation("Chapter {Chapter} finalized with {Words} words",
            state.Context.ChapterNumber,
            state.GeneratedContent.Split(' ').Length);

        return Task.FromResult(Result.Success());
    }
}

#endregion



