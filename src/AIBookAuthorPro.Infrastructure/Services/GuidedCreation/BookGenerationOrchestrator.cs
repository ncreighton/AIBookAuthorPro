// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Diagnostics;
using AIBookAuthorPro.Application.Services.GuidedCreation;
using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Models.GuidedCreation;
using AIBookAuthorPro.Core.Services;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.Infrastructure.Services.GuidedCreation;

/// <summary>
/// Orchestrates the complete book generation process.
/// </summary>
public sealed class BookGenerationOrchestrator : IBookGenerationOrchestrator
{
    private readonly IAIService _aiService;
    private readonly IGenerationContextBuilder _contextBuilder;
    private readonly IQualityEvaluationService _qualityService;
    private readonly IContinuityVerificationService _continuityService;
    private readonly ILogger<BookGenerationOrchestrator> _logger;

    private GenerationSession? _currentSession;
    private bool _isPaused;
    private bool _isCancelled;
    private readonly object _stateLock = new();

    public BookGenerationOrchestrator(
        IAIService aiService,
        IGenerationContextBuilder contextBuilder,
        IQualityEvaluationService qualityService,
        IContinuityVerificationService continuityService,
        ILogger<BookGenerationOrchestrator> logger)
    {
        _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
        _contextBuilder = contextBuilder ?? throw new ArgumentNullException(nameof(contextBuilder));
        _qualityService = qualityService ?? throw new ArgumentNullException(nameof(qualityService));
        _continuityService = continuityService ?? throw new ArgumentNullException(nameof(continuityService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<Result<GenerationSession>> StartGenerationAsync(
        BookBlueprint blueprint,
        GenerationConfiguration config,
        IProgress<GenerationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (blueprint == null)
            return Result<GenerationSession>.Failure("Blueprint cannot be null");

        _logger.LogInformation("Starting book generation for '{Title}'", blueprint.Identity?.Title);

        try
        {
            lock (_stateLock)
            {
                _isPaused = false;
                _isCancelled = false;
            }

            // Initialize session
            var session = new GenerationSession
            {
                Id = Guid.NewGuid(),
                BlueprintId = blueprint.Id,
                Status = GenerationSessionStatus.InProgress,
                StartedAt = DateTime.UtcNow,
                Configuration = config ?? new GenerationConfiguration(),
                TotalChapters = blueprint.ChapterBlueprints?.Count ?? 0,
                GeneratedChapters = new List<GeneratedChapter>()
            };

            _currentSession = session;

            // Generate each chapter
            var chapterBlueprints = blueprint.ChapterBlueprints?
                .OrderBy(c => c.ChapterNumber)
                .ToList() ?? new List<ChapterBlueprint>();

            for (var i = 0; i < chapterBlueprints.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Check for pause
                while (_isPaused && !_isCancelled)
                {
                    await Task.Delay(500, cancellationToken);
                }

                if (_isCancelled)
                {
                    session.Status = GenerationSessionStatus.Cancelled;
                    break;
                }

                var chapterBlueprint = chapterBlueprints[i];
                var chapterNumber = chapterBlueprint.ChapterNumber;

                // Report progress
                var progressPercent = (double)i / chapterBlueprints.Count * 100;
                progress?.Report(new GenerationProgress
                {
                    OverallPercentage = progressPercent,
                    CurrentChapter = chapterNumber,
                    TotalChapters = chapterBlueprints.Count,
                    CurrentOperation = $"Generating Chapter {chapterNumber}: {chapterBlueprint.Title}",
                    Status = GenerationProgressStatus.Generating
                });

                // Generate the chapter
                var chapterResult = await GenerateChapterAsync(
                    chapterNumber,
                    blueprint,
                    session.GeneratedChapters,
                    config,
                    progress,
                    cancellationToken);

                if (chapterResult.IsFailure)
                {
                    _logger.LogWarning("Failed to generate Chapter {ChapterNumber}: {Error}", 
                        chapterNumber, chapterResult.Error);
                    
                    // Create failed chapter entry
                    session.GeneratedChapters.Add(new GeneratedChapter
                    {
                        ChapterNumber = chapterNumber,
                        Title = chapterBlueprint.Title,
                        Status = ChapterGenerationStatus.Failed,
                        FailureReason = chapterResult.Error
                    });
                    
                    session.FailedChapters++;
                }
                else
                {
                    session.GeneratedChapters.Add(chapterResult.Value!);
                    session.CompletedChapters++;
                    session.TotalWordsGenerated += chapterResult.Value!.WordCount;
                }

                // Update session metrics
                session.CurrentChapter = chapterNumber;
                session.LastActivityAt = DateTime.UtcNow;
            }

            // Finalize session
            session.CompletedAt = DateTime.UtcNow;
            session.Status = _isCancelled ? GenerationSessionStatus.Cancelled :
                            session.FailedChapters > 0 ? GenerationSessionStatus.CompletedWithErrors :
                            GenerationSessionStatus.Completed;

            // Calculate final metrics
            session.AverageQualityScore = session.GeneratedChapters
                .Where(c => c.QualityReport != null)
                .Average(c => c.QualityReport!.OverallScore);

            progress?.Report(new GenerationProgress
            {
                OverallPercentage = 100,
                CurrentChapter = session.TotalChapters,
                TotalChapters = session.TotalChapters,
                CurrentOperation = "Generation complete",
                Status = GenerationProgressStatus.Complete
            });

            _logger.LogInformation("Book generation completed: {Completed}/{Total} chapters, {Words:N0} words",
                session.CompletedChapters, session.TotalChapters, session.TotalWordsGenerated);

            return Result<GenerationSession>.Success(session);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Book generation cancelled");
            if (_currentSession != null)
            {
                _currentSession.Status = GenerationSessionStatus.Cancelled;
                return Result<GenerationSession>.Success(_currentSession);
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during book generation");
            return Result<GenerationSession>.Failure($"Generation error: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result<GeneratedChapter>> GenerateChapterAsync(
        int chapterNumber,
        BookBlueprint blueprint,
        List<GeneratedChapter> previousChapters,
        GenerationConfiguration config,
        IProgress<GenerationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Generating Chapter {ChapterNumber}", chapterNumber);

        try
        {
            var chapterBlueprint = blueprint.ChapterBlueprints?
                .FirstOrDefault(c => c.ChapterNumber == chapterNumber);

            if (chapterBlueprint == null)
            {
                return Result<GeneratedChapter>.Failure($"No blueprint for chapter {chapterNumber}");
            }

            // Build generation context
            progress?.Report(new GenerationProgress
            {
                CurrentOperation = "Building context...",
                Status = GenerationProgressStatus.Generating
            });

            var contextResult = await _contextBuilder.BuildContextAsync(
                chapterNumber, blueprint, previousChapters, config, cancellationToken);

            if (contextResult.IsFailure)
            {
                return Result<GeneratedChapter>.Failure($"Context build failed: {contextResult.Error}");
            }

            var context = contextResult.Value!;

            // Generate chapter content
            progress?.Report(new GenerationProgress
            {
                CurrentOperation = "Generating content...",
                Status = GenerationProgressStatus.Generating
            });

            var generationPrompt = BuildGenerationPrompt(context);
            var targetTokens = EstimateTargetTokens(chapterBlueprint.TargetWordCount);

            var generationResponse = await _aiService.GenerateAsync(
                generationPrompt,
                new GenerationOptions
                {
                    Temperature = config?.AISettings?.Temperature ?? 0.8,
                    MaxTokens = targetTokens,
                    TopP = config?.AISettings?.TopP ?? 0.95
                },
                cancellationToken);

            if (generationResponse.IsFailure)
            {
                return Result<GeneratedChapter>.Failure($"Generation failed: {generationResponse.Error}");
            }

            var content = generationResponse.Value!;
            var wordCount = CountWords(content);

            // Create chapter object
            var chapter = new GeneratedChapter
            {
                Id = Guid.NewGuid(),
                ChapterNumber = chapterNumber,
                Title = chapterBlueprint.Title,
                Content = new ChapterContent
                {
                    Text = content,
                    FormattedText = content,
                    Version = 1
                },
                WordCount = wordCount,
                Status = ChapterGenerationStatus.Generated,
                GeneratedAt = DateTime.UtcNow,
                GenerationDurationMs = (int)stopwatch.ElapsedMilliseconds
            };

            // Quality evaluation
            progress?.Report(new GenerationProgress
            {
                CurrentOperation = "Evaluating quality...",
                Status = GenerationProgressStatus.Evaluating
            });

            var qualityContext = new QualityEvaluationContext
            {
                Blueprint = blueprint,
                CharacterBible = blueprint.CharacterBible,
                WorldBible = blueprint.WorldBible,
                StyleGuide = blueprint.StyleGuide,
                PreviousChapters = previousChapters
            };

            var qualityResult = await _qualityService.EvaluateChapterAsync(
                chapter, chapterBlueprint, qualityContext, cancellationToken);

            if (qualityResult.IsSuccess)
            {
                chapter.QualityReport = qualityResult.Value;
                chapter.QualityScore = qualityResult.Value!.OverallScore;
            }

            // Continuity check
            progress?.Report(new GenerationProgress
            {
                CurrentOperation = "Checking continuity...",
                Status = GenerationProgressStatus.Evaluating
            });

            var continuityContext = new ContinuityVerificationContext
            {
                CharacterBible = blueprint.CharacterBible,
                WorldBible = blueprint.WorldBible,
                PreviousChapters = previousChapters,
                CharacterStates = ExtractCharacterStates(previousChapters),
                PreviousEvents = ExtractPreviousEvents(previousChapters)
            };

            var continuityResult = await _continuityService.VerifyChapterContinuityAsync(
                chapter, continuityContext, cancellationToken);

            if (continuityResult.IsSuccess)
            {
                chapter.ContinuityReport = continuityResult.Value;
            }

            // Generate summaries
            var summaryResult = await GenerateChapterSummaryAsync(
                content, chapterBlueprint, cancellationToken);

            if (summaryResult.IsSuccess)
            {
                chapter.Summaries = summaryResult.Value;
            }

            // Extract character states for next chapter
            var statesResult = await _continuityService.ExtractCharacterStatesAsync(
                content, blueprint.CharacterBible!, cancellationToken);

            if (statesResult.IsSuccess)
            {
                chapter.CharacterStatesAtEnd = statesResult.Value;
            }

            // Auto-revision if needed
            if (config?.QualitySettings?.AutoRevisionEnabled == true &&
                chapter.QualityScore < (config.QualitySettings.MinimumAcceptableScore ?? 70))
            {
                progress?.Report(new GenerationProgress
                {
                    CurrentOperation = "Auto-revising...",
                    Status = GenerationProgressStatus.Revising
                });

                var revisionResult = await ReviseChapterAsync(
                    chapter, chapter.QualityReport!, config, cancellationToken);

                if (revisionResult.IsSuccess)
                {
                    chapter = revisionResult.Value!;
                }
            }

            // Finalize status
            chapter.Status = chapter.QualityScore >= 60
                ? ChapterGenerationStatus.Approved
                : ChapterGenerationStatus.NeedsReview;

            stopwatch.Stop();
            chapter.GenerationDurationMs = (int)stopwatch.ElapsedMilliseconds;

            _logger.LogInformation(
                "Chapter {ChapterNumber} generated: {Words} words, quality {Score:F0}, {Duration}ms",
                chapterNumber, wordCount, chapter.QualityScore, stopwatch.ElapsedMilliseconds);

            return Result<GeneratedChapter>.Success(chapter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Chapter {ChapterNumber}", chapterNumber);
            return Result<GeneratedChapter>.Failure($"Chapter generation error: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result<GeneratedChapter>> RegenerateChapterAsync(
        int chapterNumber,
        GenerationSession session,
        string? feedback = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Regenerating Chapter {ChapterNumber}", chapterNumber);

        // Find and remove existing chapter
        var existingIndex = session.GeneratedChapters
            .FindIndex(c => c.ChapterNumber == chapterNumber);

        var previousChapters = session.GeneratedChapters
            .Where(c => c.ChapterNumber < chapterNumber)
            .ToList();

        // Regenerate
        var result = await GenerateChapterAsync(
            chapterNumber,
            session.Blueprint!,
            previousChapters,
            session.Configuration ?? new GenerationConfiguration(),
            null,
            cancellationToken);

        if (result.IsSuccess && existingIndex >= 0)
        {
            session.GeneratedChapters[existingIndex] = result.Value!;
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<Result<GeneratedChapter>> ReviseChapterAsync(
        GeneratedChapter chapter,
        ComprehensiveQualityReport qualityReport,
        GenerationConfiguration config,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Revising Chapter {ChapterNumber}", chapter.ChapterNumber);

        var instructions = qualityReport.RevisionInstructions ?? new List<RevisionInstruction>();
        if (!instructions.Any())
        {
            _logger.LogInformation("No revision instructions, skipping revision");
            return Result<GeneratedChapter>.Success(chapter);
        }

        var revisionPrompt = BuildRevisionPrompt(chapter.Content?.Text ?? "", instructions);

        var response = await _aiService.GenerateAsync(
            revisionPrompt,
            new GenerationOptions { Temperature = 0.6, MaxTokens = EstimateTargetTokens(chapter.WordCount) },
            cancellationToken);

        if (response.IsFailure)
        {
            return Result<GeneratedChapter>.Failure($"Revision failed: {response.Error}");
        }

        var revisedChapter = chapter with
        {
            Content = new ChapterContent
            {
                Text = response.Value!,
                FormattedText = response.Value!,
                Version = (chapter.Content?.Version ?? 0) + 1
            },
            WordCount = CountWords(response.Value!),
            RevisionCount = chapter.RevisionCount + 1,
            Status = ChapterGenerationStatus.Generated
        };

        return Result<GeneratedChapter>.Success(revisedChapter);
    }

    /// <inheritdoc />
    public async Task<Result> PauseGenerationAsync(CancellationToken cancellationToken = default)
    {
        lock (_stateLock)
        {
            _isPaused = true;
        }
        _logger.LogInformation("Generation paused");
        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result> ResumeGenerationAsync(CancellationToken cancellationToken = default)
    {
        lock (_stateLock)
        {
            _isPaused = false;
        }
        _logger.LogInformation("Generation resumed");
        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result> CancelGenerationAsync(CancellationToken cancellationToken = default)
    {
        lock (_stateLock)
        {
            _isCancelled = true;
            _isPaused = false;
        }
        _logger.LogInformation("Generation cancelled");
        return Result.Success();
    }

    /// <inheritdoc />
    public Result<GenerationSession?> GetCurrentSession()
    {
        return Result<GenerationSession?>.Success(_currentSession);
    }

    /// <inheritdoc />
    public async Task<Result<GenerationMetrics>> CalculateMetricsAsync(
        GenerationSession session,
        CancellationToken cancellationToken = default)
    {
        var completedChapters = session.GeneratedChapters
            .Where(c => c.Status == ChapterGenerationStatus.Approved ||
                       c.Status == ChapterGenerationStatus.Generated)
            .ToList();

        var metrics = new GenerationMetrics
        {
            TotalChapters = session.TotalChapters,
            CompletedChapters = completedChapters.Count,
            TotalWords = completedChapters.Sum(c => c.WordCount),
            AverageWordsPerChapter = completedChapters.Any() 
                ? (int)completedChapters.Average(c => c.WordCount) : 0,
            AverageQualityScore = completedChapters.Any()
                ? completedChapters.Average(c => c.QualityScore) : 0,
            TotalGenerationTimeMs = completedChapters.Sum(c => c.GenerationDurationMs),
            AverageGenerationTimeMs = completedChapters.Any()
                ? (int)completedChapters.Average(c => c.GenerationDurationMs) : 0,
            TotalRevisions = completedChapters.Sum(c => c.RevisionCount),
            EstimatedCost = CalculateEstimatedCost(session)
        };

        return Result<GenerationMetrics>.Success(metrics);
    }

    #region Private Helper Methods

    private string BuildGenerationPrompt(ChapterGenerationContext context)
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine(context.SystemPrompt);
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine(context.NarrativeContext);
        sb.AppendLine();
        sb.AppendLine(context.CharacterContext);
        sb.AppendLine();
        sb.AppendLine(context.WorldContext);
        sb.AppendLine();
        sb.AppendLine(context.PlotContext);
        sb.AppendLine();
        sb.AppendLine(context.StyleContext);
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine(context.ChapterInstructions);
        sb.AppendLine();
        sb.AppendLine("Now write the complete chapter. Begin:");

        return sb.ToString();
    }

    private string BuildRevisionPrompt(string content, List<RevisionInstruction> instructions)
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine("Revise the following chapter according to these instructions:");
        sb.AppendLine();
        sb.AppendLine("REVISION INSTRUCTIONS:");
        foreach (var instruction in instructions.OrderBy(i => i.Priority).Take(5))
        {
            sb.AppendLine($"- {instruction.Instruction}");
        }
        sb.AppendLine();
        sb.AppendLine("CURRENT CHAPTER:");
        sb.AppendLine(content);
        sb.AppendLine();
        sb.AppendLine("Provide the fully revised chapter. Maintain the same length and all working elements:");

        return sb.ToString();
    }

    private async Task<Result<ChapterSummaries>> GenerateChapterSummaryAsync(
        string content,
        ChapterBlueprint blueprint,
        CancellationToken cancellationToken)
    {
        var prompt = $@"Summarize this chapter:

{content.Substring(0, Math.Min(content.Length, 6000))}

Provide:
1. A brief 1-2 sentence summary
2. A detailed paragraph summary
3. List of 3-5 key events

Format:
BRIEF: [summary]
DETAILED: [summary]
EVENTS:
- [event 1]
- [event 2]
...";

        try
        {
            var response = await _aiService.GenerateAsync(
                prompt,
                new GenerationOptions { Temperature = 0.3, MaxTokens = 800 },
                cancellationToken);

            if (response.IsSuccess)
            {
                return ParseSummaryResponse(response.Value!);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error generating chapter summary");
        }

        return Result<ChapterSummaries>.Success(new ChapterSummaries
        {
            BriefSummary = "Chapter events occurred.",
            AISummary = "Events unfolded in this chapter."
        });
    }

    private Result<ChapterSummaries> ParseSummaryResponse(string response)
    {
        var summaries = new ChapterSummaries();
        var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            if (line.StartsWith("BRIEF:", StringComparison.OrdinalIgnoreCase))
            {
                summaries.BriefSummary = line.Substring(6).Trim();
            }
            else if (line.StartsWith("DETAILED:", StringComparison.OrdinalIgnoreCase))
            {
                summaries.AISummary = line.Substring(9).Trim();
            }
            else if (line.StartsWith("- "))
            {
                summaries.KeyEvents ??= new List<string>();
                summaries.KeyEvents.Add(line.Substring(2).Trim());
            }
        }

        if (string.IsNullOrEmpty(summaries.BriefSummary))
            summaries.BriefSummary = "Chapter events occurred.";
        if (string.IsNullOrEmpty(summaries.AISummary))
            summaries.AISummary = summaries.BriefSummary;

        return Result<ChapterSummaries>.Success(summaries);
    }

    private static int EstimateTargetTokens(int wordCount)
    {
        // Roughly 1.3 tokens per word for generation
        return (int)(wordCount * 1.5);
    }

    private static int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        return text.Split(new[] { ' ', '\n', '\r', '\t' }, 
            StringSplitOptions.RemoveEmptyEntries).Length;
    }

    private static List<CharacterStateSnapshot> ExtractCharacterStates(
        List<GeneratedChapter> chapters)
    {
        var lastChapter = chapters
            .OrderByDescending(c => c.ChapterNumber)
            .FirstOrDefault();

        return lastChapter?.CharacterStatesAtEnd ?? new List<CharacterStateSnapshot>();
    }

    private static List<string> ExtractPreviousEvents(List<GeneratedChapter> chapters)
    {
        return chapters
            .SelectMany(c => c.Summaries?.KeyEvents ?? Enumerable.Empty<string>())
            .ToList();
    }

    private static decimal CalculateEstimatedCost(GenerationSession session)
    {
        // Rough estimate: $0.01 per 1000 words generated
        return session.TotalWordsGenerated / 1000m * 0.01m;
    }

    #endregion
}
