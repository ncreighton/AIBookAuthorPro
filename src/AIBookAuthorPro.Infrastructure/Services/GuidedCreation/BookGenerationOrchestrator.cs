// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Diagnostics;
using AIBookAuthorPro.Application.Services.GuidedCreation;
using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Models.GuidedCreation;
using AIBookAuthorPro.Core.Services;
using CoreGenerationOptions = AIBookAuthorPro.Core.Services.GenerationOptions;
using Microsoft.Extensions.Logging;

using GenerationOptions = AIBookAuthorPro.Application.Services.GuidedCreation.GenerationOptions;
using DetailedGenerationProgress = AIBookAuthorPro.Application.Services.GuidedCreation.DetailedGenerationProgress;

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

    private readonly Dictionary<Guid, GenerationSession> _sessions = new();
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
    public async Task<Result<GenerationSession>> StartFullGenerationAsync(
        BookBlueprint blueprint,
        GenerationOptions options,
        IProgress<DetailedGenerationProgress>? progress = null,
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

            var config = options.ConfigurationOverride ?? blueprint.Configuration ?? new GenerationConfiguration();

            var session = new GenerationSession
            {
                Id = Guid.NewGuid(),
                BlueprintId = blueprint.Id,
                Blueprint = blueprint,
                Status = GenerationSessionStatus.InProgress,
                StartedAt = DateTime.UtcNow,
                Configuration = config,
                TotalChapters = blueprint.Structure.Chapters?.Count ?? 0,
                Chapters = new List<GeneratedChapter>()
            };

            _sessions[session.Id] = session;

            var chapterBlueprints = blueprint.Structure.Chapters?
                .OrderBy(c => c.ChapterNumber)
                .ToList() ?? new List<ChapterBlueprint>();

            var startChapter = options.StartFromChapter ?? 1;
            var endChapter = options.EndAtChapter ?? chapterBlueprints.Count;

            var stopwatch = Stopwatch.StartNew();

            for (var i = 0; i < chapterBlueprints.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var chapterBlueprint = chapterBlueprints[i];
                var chapterNumber = chapterBlueprint.ChapterNumber;

                if (chapterNumber < startChapter || chapterNumber > endChapter)
                    continue;

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

                var progressPercent = (double)session.CompletedChapters / session.TotalChapters * 100;
                progress?.Report(new DetailedGenerationProgress
                {
                    OverallPercentage = progressPercent,
                    CurrentChapter = chapterNumber,
                    TotalChapters = session.TotalChapters,
                    CurrentOperation = $"Generating Chapter {chapterNumber}: {chapterBlueprint.Title}",
                    Status = GenerationProgressStatus.Generating,
                    ElapsedTime = stopwatch.Elapsed,
                    WordsGenerated = session.GeneratedWordCount
                });

                var chapterResult = await GenerateChapterInternalAsync(
                    chapterNumber, blueprint, session.Chapters, config, cancellationToken);

                if (chapterResult.IsFailure)
                {
                    _logger.LogWarning("Failed to generate Chapter {Chapter}: {Error}",
                        chapterNumber, chapterResult.Error);

                    session.Chapters.Add(new GeneratedChapter
                    {
                        ChapterNumber = chapterNumber,
                        Title = chapterBlueprint.Title,
                        Status = ChapterGenerationStatus.Failed,
                        FailureReason = chapterResult.Error
                    });
                    session.FailedChapters.Add(chapterNumber);
                }
                else
                {
                    session.Chapters.Add(chapterResult.Value!);
                    session.CompletedChapters++;
                    session.GeneratedWordCount += chapterResult.Value!.WordCount;
                }

                session.CurrentChapter = chapterNumber;
                session.LastActivityAt = DateTime.UtcNow;
            }

            stopwatch.Stop();
            session.CompletedAt = DateTime.UtcNow;
            session.Status = _isCancelled ? GenerationSessionStatus.Cancelled :
                            session.FailedChapters.Count > 0 ? GenerationSessionStatus.CompletedWithErrors :
                            GenerationSessionStatus.Completed;

            if (session.Chapters.Any(c => c.QualityReport != null))
            {
                session.AverageQualityScore = session.Chapters
                    .Where(c => c.QualityReport != null)
                    .Average(c => c.QualityReport!.OverallScore);
            }

            progress?.Report(new DetailedGenerationProgress
            {
                OverallPercentage = 100,
                CurrentChapter = session.TotalChapters,
                TotalChapters = session.TotalChapters,
                CurrentOperation = "Generation complete",
                Status = GenerationProgressStatus.Complete,
                ElapsedTime = stopwatch.Elapsed,
                WordsGenerated = session.GeneratedWordCount
            });

            _logger.LogInformation("Book generation completed: {Completed}/{Total} chapters, {Words:N0} words",
                session.CompletedChapters, session.TotalChapters, session.GeneratedWordCount);

            return Result<GenerationSession>.Success(session);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Book generation cancelled");
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
        GenerationSession session,
        int chapterNumber,
        IProgress<ChapterGenerationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (session?.Blueprint == null)
            return Result<GeneratedChapter>.Failure("Invalid session or blueprint");

        return await GenerateChapterInternalAsync(
            chapterNumber,
            session.Blueprint,
            session.Chapters,
            session.Configuration ?? new GenerationConfiguration(),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result> PauseGenerationAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        lock (_stateLock)
        {
            _isPaused = true;
        }
        _logger.LogInformation("Generation paused for session {SessionId}", sessionId);
        return Task.FromResult(Result.Success());
    }

    /// <inheritdoc />
    public Task<Result<GenerationSession>> ResumeGenerationAsync(
        Guid sessionId,
        IProgress<DetailedGenerationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        lock (_stateLock)
        {
            _isPaused = false;
        }
        _logger.LogInformation("Generation resumed for session {SessionId}", sessionId);

        if (_sessions.TryGetValue(sessionId, out var session))
        {
            return Task.FromResult(Result<GenerationSession>.Success(session));
        }
        return Task.FromResult(Result<GenerationSession>.Failure("Session not found"));
    }

    /// <inheritdoc />
    public Task<Result> CancelGenerationAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        lock (_stateLock)
        {
            _isCancelled = true;
            _isPaused = false;
        }
        _logger.LogInformation("Generation cancelled for session {SessionId}", sessionId);
        return Task.FromResult(Result.Success());
    }

    /// <inheritdoc />
    public async Task<Result<GeneratedChapter>> RegenerateChapterAsync(
        Guid sessionId,
        int chapterNumber,
        ChapterRegenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
            return Result<GeneratedChapter>.Failure("Session not found");

        var existingIndex = session.Chapters.FindIndex(c => c.ChapterNumber == chapterNumber);
        var previousChapters = session.Chapters
            .Where(c => c.ChapterNumber < chapterNumber)
            .ToList();

        var result = await GenerateChapterInternalAsync(
            chapterNumber,
            session.Blueprint!,
            previousChapters,
            session.Configuration ?? new GenerationConfiguration(),
            cancellationToken);

        if (result.IsSuccess && existingIndex >= 0)
        {
            session.Chapters[existingIndex] = result.Value!;
        }

        return result;
    }

    /// <inheritdoc />
    public Task<Result> ApproveChapterAsync(
        Guid sessionId,
        int chapterNumber,
        CancellationToken cancellationToken = default)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
            return Task.FromResult(Result.Failure("Session not found"));

        var chapter = session.Chapters.FirstOrDefault(c => c.ChapterNumber == chapterNumber);
        if (chapter == null)
            return Task.FromResult(Result.Failure("Chapter not found"));

        chapter.Status = ChapterGenerationStatus.Approved;
        return Task.FromResult(Result.Success());
    }

    /// <inheritdoc />
    public async Task<Result<GeneratedChapter>> RequestRevisionAsync(
        Guid sessionId,
        int chapterNumber,
        string instructions,
        CancellationToken cancellationToken = default)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
            return Result<GeneratedChapter>.Failure("Session not found");

        var chapter = session.Chapters.FirstOrDefault(c => c.ChapterNumber == chapterNumber);
        if (chapter == null)
            return Result<GeneratedChapter>.Failure("Chapter not found");

        var prompt = $@"Revise this chapter:

Instructions: {instructions}

Current content:
{chapter.Content}

Provide the revised chapter:";

        var result = await _aiService.GenerateAsync(
            prompt,
            new CoreGenerationOptions { Temperature = 0.6, MaxTokens = (chapter.WordCount * 2) },
            cancellationToken);

        if (result.IsFailure)
            return Result<GeneratedChapter>.Failure(result.Error!);

        chapter.Content = result.Value!;
        chapter.WordCount = CountWords(result.Value!);
        chapter.RevisionCount++;
        chapter.Version++;
        chapter.Status = ChapterGenerationStatus.Generated;

        return Result<GeneratedChapter>.Success(chapter);
    }

    /// <inheritdoc />
    public Task<Result<GenerationSession>> GetSessionStateAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
            return Task.FromResult(Result<GenerationSession>.Success(session));

        return Task.FromResult(Result<GenerationSession>.Failure("Session not found"));
    }

    /// <inheritdoc />
    public Task<Result<GenerationStatistics>> GetStatisticsAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
            return Task.FromResult(Result<GenerationStatistics>.Failure("Session not found"));

        var stats = new GenerationStatistics
        {
            SessionId = sessionId,
            TotalChapters = session.TotalChapters,
            CompletedChapters = session.CompletedChapters,
            TotalWordsGenerated = session.GeneratedWordCount,
            AverageQualityScore = session.AverageQualityScore,
            TotalTimeElapsed = (session.CompletedAt ?? DateTime.UtcNow) - session.StartedAt,
            ChapterStats = session.Chapters.Select(c => new ChapterStatistics
            {
                ChapterNumber = c.ChapterNumber,
                WordCount = c.WordCount,
                QualityScore = c.QualityScore ?? 0.0,
                Status = c.Status,
                IssuesFound = c.QualityReport?.TotalIssueCount ?? 0
            }).ToList()
        };

        return Task.FromResult(Result<GenerationStatistics>.Success(stats));
    }

    #region Private Methods

    private async Task<Result<GeneratedChapter>> GenerateChapterInternalAsync(
        int chapterNumber,
        BookBlueprint blueprint,
        List<GeneratedChapter> previousChapters,
        GenerationConfiguration config,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Generating Chapter {ChapterNumber}", chapterNumber);

        try
        {
            var chapterBlueprint = blueprint.Structure.Chapters?
                .FirstOrDefault(c => c.ChapterNumber == chapterNumber);

            if (chapterBlueprint == null)
                return Result<GeneratedChapter>.Failure($"No blueprint for chapter {chapterNumber}");

            // Build context
            var contextResult = await _contextBuilder.BuildChapterContextAsync(
                blueprint, chapterNumber, previousChapters, new ContextBuildingOptions(), cancellationToken);

            if (contextResult.IsFailure)
                return Result<GeneratedChapter>.Failure($"Context build failed: {contextResult.Error}");

            var context = contextResult.Value!;

            // Generate content
            var generationPrompt = BuildGenerationPrompt(context);
            var targetTokens = (chapterBlueprint.TargetWordCount * 3) / 2;

            var generationResponse = await _aiService.GenerateAsync(
                generationPrompt,
                new CoreGenerationOptions
                {
                    Temperature = config.AISettings?.Temperature ?? 0.8,
                    MaxTokens = targetTokens,
                    TopP = config.AISettings?.TopP ?? 0.95
                },
                cancellationToken);

            if (generationResponse.IsFailure)
                return Result<GeneratedChapter>.Failure($"Generation failed: {generationResponse.Error}");

            var content = generationResponse.Value!;
            var wordCount = CountWords(content);

            var chapter = new GeneratedChapter
            {
                Id = Guid.NewGuid(),
                ChapterNumber = chapterNumber,
                Title = chapterBlueprint.Title,
                Content = content,
                WordCount = wordCount,
                Status = ChapterGenerationStatus.Generated,
                GeneratedAt = DateTime.UtcNow,
                GenerationDurationMs = (int)stopwatch.ElapsedMilliseconds
            };

            // Quality evaluation
            var qualityContext = new QualityEvaluationContext
            {
                Blueprint = chapterBlueprint,
                CharacterBible = blueprint.Characters,
                World = blueprint.World,
                Style = blueprint.Style,
                PreviousChapters = previousChapters
            };

            var qualityResult = await _qualityService.EvaluateChapterAsync(
                chapter, chapterBlueprint, qualityContext, cancellationToken);

            if (qualityResult.IsSuccess)
            {
                chapter.QualityReport = qualityResult.Value;
                // QualityScore is computed from QualityReport, no need to set it
            }

            // Continuity check
            var continuityContext = new ContinuityVerificationContext
            {
                CharacterBible = blueprint.Characters,
                World = blueprint.World,
                PreviousChapters = previousChapters
            };

            var continuityResult = await _continuityService.VerifyChapterContinuityAsync(
                chapter, continuityContext, cancellationToken);

            if (continuityResult.IsSuccess)
            {
                chapter.ContinuityReport = continuityResult.Value;
            }

            // Extract character states
            if (blueprint.Characters != null)
            {
                var characterIds = chapterBlueprint.CharacterAppearances?
                    .Select(ca => ca.CharacterId)
                    .ToList() ?? new List<Guid>();
                    
                var statesResult = await _continuityService.ExtractCharacterStatesAsync(
                    content, characterIds, blueprint.Characters, cancellationToken);

                if (statesResult.IsSuccess && statesResult.Value != null)
                {
                    chapter.CharacterStatesAtEnd = statesResult.Value;
                }
            }

            chapter.Status = chapter.QualityScore >= 60
                ? ChapterGenerationStatus.Approved
                : ChapterGenerationStatus.NeedsReview;

            stopwatch.Stop();
            chapter.GenerationDurationMs = (int)stopwatch.ElapsedMilliseconds;

            _logger.LogInformation(
                "Chapter {Chapter} generated: {Words} words, quality {Score:F0}, {Duration}ms",
                chapterNumber, wordCount, chapter.QualityScore, stopwatch.ElapsedMilliseconds);

            return Result<GeneratedChapter>.Success(chapter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Chapter {ChapterNumber}", chapterNumber);
            return Result<GeneratedChapter>.Failure($"Generation error: {ex.Message}", ex);
        }
    }

    private static string BuildGenerationPrompt(ChapterGenerationContext context)
    {
        return $@"{context.SystemPrompt}

---

{context.NarrativeContext}

{context.CharacterContext}

{context.WorldContext}

{context.PlotContext}

{context.StyleContext}

---

{context.ChapterInstructions}

Now write the complete chapter. Begin:";
    }

    private static int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        return text.Split(new[] { ' ', '\n', '\r', '\t' },
            StringSplitOptions.RemoveEmptyEntries).Length;
    }

    #endregion
}



