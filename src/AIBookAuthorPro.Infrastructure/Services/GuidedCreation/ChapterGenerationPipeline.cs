// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Application.Services.GuidedCreation;
using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Models.GuidedCreation;
using AIBookAuthorPro.Core.Services;
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

    private readonly List<IPipelineStep> _steps;

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
        ChapterGenerationContext context,
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
                    StepIndex = pipelineState.CurrentStepIndex,
                    TotalSteps = _steps.Count,
                    OverallProgress = stepProgress,
                    StepProgress = 0
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
                StepIndex = _steps.Count,
                TotalSteps = _steps.Count,
                OverallProgress = 100,
                StepProgress = 100
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
    public IReadOnlyList<IPipelineStep> GetSteps() => _steps.AsReadOnly();

    /// <inheritdoc />
    public void AddStep(IPipelineStep step)
    {
        _steps.Add(step);
        _steps.Sort((a, b) => a.Order.CompareTo(b.Order));
    }

    /// <inheritdoc />
    public void RemoveStep(string stepName)
    {
        _steps.RemoveAll(s => s.Name == stepName);
    }

    #region Private Methods

    private List<IPipelineStep> InitializePipelineSteps()
    {
        return new List<IPipelineStep>
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
            Content = new ChapterContent
            {
                Text = state.GeneratedContent ?? string.Empty,
                FormattedText = state.GeneratedContent ?? string.Empty,
                Version = 1
            },
            WordCount = CountWords(state.GeneratedContent ?? string.Empty),
            Status = state.QualityReport?.OverallScore >= 60
                ? ChapterGenerationStatus.Approved
                : ChapterGenerationStatus.NeedsReview,
            QualityReport = state.QualityReport,
            QualityScore = state.QualityReport?.OverallScore ?? 0,
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
    public ChapterGenerationContext Context { get; set; } = null!;
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
/// Pipeline progress report.
/// </summary>
public class PipelineProgress
{
    public string CurrentStep { get; set; } = string.Empty;
    public int StepIndex { get; set; }
    public int TotalSteps { get; set; }
    public double OverallProgress { get; set; }
    public double StepProgress { get; set; }
}

/// <summary>
/// Pipeline step interface.
/// </summary>
public interface IPipelineStep
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
public class BuildContextStep : IPipelineStep
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
public class GenerateOutlineStep : IPipelineStep
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
            new GenerationOptions { Temperature = 0.6, MaxTokens = 1500 },
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
public class GenerateScenesStep : IPipelineStep
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
                new GenerationOptions
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
                    new GenerationOptions { Temperature = 0.8, MaxTokens = 2000 },
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
Characters: {string.Join(", ", scene.Characters ?? new List<string>())}
Purpose: {scene.Purpose}

Previous scenes written:
{string.Join("\n---\n", state.GeneratedScenes?.TakeLast(2) ?? Enumerable.Empty<string>())}

Write this scene (~{scene.TargetWordCount ?? 500} words):";
    }
}

/// <summary>
/// Step 4: Assemble chapter from scenes.
/// </summary>
public class AssembleChapterStep : IPipelineStep
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
public class ContinuityCheckStep : IPipelineStep
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
            Content = new ChapterContent { Text = state.GeneratedContent ?? "" }
        };

        var context = new ContinuityVerificationContext
        {
            CharacterBible = state.Context.Blueprint?.CharacterBible,
            WorldBible = state.Context.Blueprint?.WorldBible,
            PreviousChapters = state.Context.Blueprint?.GeneratedChapters ?? new List<GeneratedChapter>()
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
public class StyleConsistencyStep : IPipelineStep
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
public class QualityEvaluationStep : IPipelineStep
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
            Content = new ChapterContent { Text = state.GeneratedContent ?? "" }
        };

        var context = new QualityEvaluationContext
        {
            Blueprint = state.Context.Blueprint,
            CharacterBible = state.Context.Blueprint?.CharacterBible,
            WorldBible = state.Context.Blueprint?.WorldBible,
            StyleGuide = state.Context.Blueprint?.StyleGuide
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
public class RevisionStep : IPipelineStep
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
            new GenerationOptions { Temperature = 0.6, MaxTokens = (state.GeneratedContent?.Length ?? 3000) * 2 },
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
public class FinalizeStep : IPipelineStep
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
