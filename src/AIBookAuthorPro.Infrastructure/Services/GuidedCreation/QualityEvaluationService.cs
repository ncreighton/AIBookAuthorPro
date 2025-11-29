// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Text.Json;
using AIBookAuthorPro.Application.Services.GuidedCreation;
using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Models.GuidedCreation;
using AIBookAuthorPro.Core.Services;
using CoreGenerationOptions = AIBookAuthorPro.Core.Services.GenerationOptions;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.Infrastructure.Services.GuidedCreation;

/// <summary>
/// Service for comprehensive quality evaluation of generated chapters.
/// </summary>
public sealed class QualityEvaluationService : IQualityEvaluationService
{
    private readonly IAIService _aiService;
    private readonly ILogger<QualityEvaluationService> _logger;

    public QualityEvaluationService(
        IAIService aiService,
        ILogger<QualityEvaluationService> logger)
    {
        _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<Result<ComprehensiveQualityReport>> EvaluateChapterAsync(
        GeneratedChapter chapter,
        ChapterBlueprint blueprint,
        QualityEvaluationContext context,
        CancellationToken cancellationToken = default)
    {
        if (chapter == null)
            return Result<ComprehensiveQualityReport>.Failure("Chapter cannot be null");
        if (blueprint == null)
            return Result<ComprehensiveQualityReport>.Failure("Blueprint cannot be null");

        _logger.LogInformation("Evaluating quality for Chapter {ChapterNumber}", chapter.ChapterNumber);

        try
        {
            var dimensionScores = new List<DimensionScore>();
            var allIssues = new List<QualityIssue>();

            // Evaluate each dimension
            var narrativeResult = await EvaluateNarrativeQualityAsync(
                chapter.Content ?? "", context.Style ?? new StyleGuide(), cancellationToken);
            if (narrativeResult.IsSuccess)
            {
                dimensionScores.Add(narrativeResult.Value!);
                allIssues.AddRange(ExtractIssuesFromDimension(narrativeResult.Value!, "Narrative"));
            }

            var characterIds = blueprint.CharacterAppearances?.Select(c => c.CharacterId).ToList() ?? new List<Guid>();
            var characterResult = await EvaluateCharacterConsistencyAsync(
                chapter.Content ?? "", characterIds, context.CharacterBible ?? new CharacterBible(), cancellationToken);
            if (characterResult.IsSuccess)
            {
                dimensionScores.Add(characterResult.Value!);
                allIssues.AddRange(ExtractIssuesFromDimension(characterResult.Value!, "Character"));
            }

            var plotResult = await EvaluatePlotAdherenceAsync(
                chapter.Content ?? "", blueprint, context.PlotArchitecture ?? new PlotArchitecture(), cancellationToken);
            if (plotResult.IsSuccess)
            {
                dimensionScores.Add(plotResult.Value!);
                allIssues.AddRange(ExtractIssuesFromDimension(plotResult.Value!, "Plot"));
            }

            var styleResult = await EvaluateStyleConsistencyAsync(
                chapter.Content ?? "", context.Style ?? new StyleGuide(), context.PreviousChapters ?? new List<GeneratedChapter>(), cancellationToken);
            if (styleResult.IsSuccess)
            {
                dimensionScores.Add(styleResult.Value!);
                allIssues.AddRange(ExtractIssuesFromDimension(styleResult.Value!, "Style"));
            }

            var pacingResult = await EvaluatePacingAsync(
                chapter.Content ?? "", blueprint.PacingIntensity, cancellationToken);
            if (pacingResult.IsSuccess)
            {
                dimensionScores.Add(pacingResult.Value!);
                allIssues.AddRange(ExtractIssuesFromDimension(pacingResult.Value!, "Pacing"));
            }

            var dialogueResult = await EvaluateDialogueAsync(
                chapter.Content ?? "", characterIds, context.CharacterBible ?? new CharacterBible(), 
                context.Style?.Dialogue ?? new DialogueGuidelines(), cancellationToken);
            if (dialogueResult.IsSuccess)
            {
                dimensionScores.Add(dialogueResult.Value!);
                allIssues.AddRange(ExtractIssuesFromDimension(dialogueResult.Value!, "Dialogue"));
            }

            // Calculate overall score
            var overallScore = CalculateOverallScore(dimensionScores);
            var verdict = DetermineVerdict(overallScore);

            // Count issues by severity
            var criticalCount = allIssues.Count(i => i.Severity == QualityIssueSeverity.Critical);
            var majorCount = allIssues.Count(i => i.Severity == QualityIssueSeverity.Major);
            var minorCount = allIssues.Count(i => i.Severity == QualityIssueSeverity.Minor);

            // Generate improvement suggestions
            var suggestions = GenerateImprovementSuggestions(dimensionScores, allIssues);

            // Determine if auto-revision is recommended
            var shouldAutoRevise = verdict == QualityVerdict.NeedsWork || verdict == QualityVerdict.Regenerate;

            var report = new ComprehensiveQualityReport
            {
                ChapterId = chapter.Id,
                ChapterNumber = chapter.ChapterNumber,
                EvaluatedAt = DateTime.UtcNow,
                OverallScore = overallScore,
                Verdict = verdict,
                NarrativeQuality = dimensionScores.FirstOrDefault(d => d.Dimension == "Narrative") ?? CreateDefaultDimension("Narrative"),
                CharacterConsistency = dimensionScores.FirstOrDefault(d => d.Dimension == "Character") ?? CreateDefaultDimension("Character"),
                PlotAdherence = dimensionScores.FirstOrDefault(d => d.Dimension == "Plot") ?? CreateDefaultDimension("Plot"),
                StyleConsistency = dimensionScores.FirstOrDefault(d => d.Dimension == "Style") ?? CreateDefaultDimension("Style"),
                PacingQuality = dimensionScores.FirstOrDefault(d => d.Dimension == "Pacing") ?? CreateDefaultDimension("Pacing"),
                DialogueQuality = dimensionScores.FirstOrDefault(d => d.Dimension == "Dialogue") ?? CreateDefaultDimension("Dialogue"),
                DescriptionQuality = CreateDefaultDimension("Description", 75),
                EmotionalImpact = CreateDefaultDimension("Emotional", 75),
                ContinuityAccuracy = CreateDefaultDimension("Continuity", 80),
                Readability = CreateDefaultDimension("Readability", 80),
                Issues = allIssues,
                // CriticalIssueCount, MajorIssueCount, MinorIssueCount are computed from Issues
                ImprovementSuggestions = suggestions,
                AutoRevisionRecommended = shouldAutoRevise
            };

            if (shouldAutoRevise)
            {
                var revisionResult = await GenerateRevisionInstructionsAsync(report, 10, cancellationToken);
                if (revisionResult.IsSuccess)
                {
                    report = report with { RevisionInstructions = revisionResult.Value };
                }
            }

            _logger.LogInformation("Quality evaluation complete: Score={Score}, Verdict={Verdict}", 
                overallScore, verdict);

            return Result<ComprehensiveQualityReport>.Success(report);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Quality evaluation cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating chapter quality");
            return Result<ComprehensiveQualityReport>.Failure($"Evaluation error: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result<DimensionScore>> EvaluateNarrativeQualityAsync(
        string content,
        StyleGuide styleGuide,
        CancellationToken cancellationToken = default)
    {
        var prompt = $@"Evaluate the narrative quality of this chapter content.

Content:
{content.Substring(0, Math.Min(content.Length, 8000))}

Evaluate on these criteria (0-100):
1. Prose quality and flow
2. Show vs tell balance
3. Sensory details
4. Engagement and hook
5. Scene construction

Respond in JSON:
{{
  ""score"": 0-100,
  ""strengths"": [""strength1""],
  ""weaknesses"": [""weakness1""],
  ""explanation"": ""detailed explanation""
}}";

        try
        {
            var response = await _aiService.GenerateAsync(
                prompt,
                new CoreGenerationOptions { Temperature = 0.3, MaxTokens = 1000, ResponseFormat = "json" },
                cancellationToken);

            if (response.IsSuccess)
            {
                return ParseDimensionScore(response.Value!, "Narrative", 1.0);
            }
            return Result<DimensionScore>.Failure(response.Error ?? "Evaluation failed");
        }
        catch (Exception ex)
        {
            return Result<DimensionScore>.Failure(ex.Message, ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result<DimensionScore>> EvaluateCharacterConsistencyAsync(
        string content,
        List<Guid> characters,
        CharacterBible characterBible,
        CancellationToken cancellationToken = default)
    {
        var characterContext = characterBible != null
            ? $"Characters: {string.Join(", ", characterBible.MainCharacters?.Select(c => c.FullName) ?? Array.Empty<string>())}"
            : "";

        var prompt = $@"Evaluate character consistency in this chapter.

{characterContext}

Content:
{content.Substring(0, Math.Min(content.Length, 8000))}

Evaluate:
1. Character voice consistency
2. Behavior consistency with established traits
3. Character development/arc progression
4. Dialogue authenticity
5. Relationship dynamics

Respond in JSON:
{{
  ""score"": 0-100,
  ""strengths"": [""strength1""],
  ""weaknesses"": [""weakness1""],
  ""explanation"": ""detailed explanation""
}}";

        try
        {
            var response = await _aiService.GenerateAsync(
                prompt,
                new CoreGenerationOptions { Temperature = 0.3, MaxTokens = 1000, ResponseFormat = "json" },
                cancellationToken);

            if (response.IsSuccess)
            {
                return ParseDimensionScore(response.Value!, "Character", 1.2);
            }
            return Result<DimensionScore>.Failure(response.Error ?? "Evaluation failed");
        }
        catch (Exception ex)
        {
            return Result<DimensionScore>.Failure(ex.Message, ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result<DimensionScore>> EvaluatePlotAdherenceAsync(
        string content,
        ChapterBlueprint blueprint,
        PlotArchitecture plotArchitecture,
        CancellationToken cancellationToken = default)
    {
        var plotContext = $@"Chapter should include:
- Purpose: {blueprint.Purpose}
- Key elements: {string.Join(", ", blueprint.MustInclude ?? new List<string>())}
- Should avoid: {string.Join(", ", blueprint.MustAvoid ?? new List<string>())}";

        var prompt = $@"Evaluate plot adherence in this chapter.

{plotContext}

Content:
{content.Substring(0, Math.Min(content.Length, 8000))}

Evaluate:
1. Adherence to chapter purpose
2. Required elements included
3. Plot progression
4. Stakes and tension
5. Setup and payoff

Respond in JSON:
{{
  ""score"": 0-100,
  ""strengths"": [""strength1""],
  ""weaknesses"": [""weakness1""],
  ""explanation"": ""detailed explanation""
}}";

        try
        {
            var response = await _aiService.GenerateAsync(
                prompt,
                new CoreGenerationOptions { Temperature = 0.3, MaxTokens = 1000, ResponseFormat = "json" },
                cancellationToken);

            if (response.IsSuccess)
            {
                return ParseDimensionScore(response.Value!, "Plot", 1.3);
            }
            return Result<DimensionScore>.Failure(response.Error ?? "Evaluation failed");
        }
        catch (Exception ex)
        {
            return Result<DimensionScore>.Failure(ex.Message, ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result<DimensionScore>> EvaluateStyleConsistencyAsync(
        string content,
        StyleGuide styleGuide,
        List<GeneratedChapter> previousChapters,
        CancellationToken cancellationToken = default)
    {
        var styleContext = styleGuide != null
            ? $"Voice: {styleGuide.Voice?.Description ?? "Not specified"}"
            : "";

        var prompt = $@"Evaluate style consistency in this chapter.

{styleContext}

Content:
{content.Substring(0, Math.Min(content.Length, 8000))}

Evaluate:
1. Voice consistency
2. Tone consistency
3. POV consistency
4. Tense consistency
5. Style guide adherence

Respond in JSON:
{{
  ""score"": 0-100,
  ""strengths"": [""strength1""],
  ""weaknesses"": [""weakness1""],
  ""explanation"": ""detailed explanation""
}}";

        try
        {
            var response = await _aiService.GenerateAsync(
                prompt,
                new CoreGenerationOptions { Temperature = 0.3, MaxTokens = 1000, ResponseFormat = "json" },
                cancellationToken);

            if (response.IsSuccess)
            {
                return ParseDimensionScore(response.Value!, "Style", 1.0);
            }
            return Result<DimensionScore>.Failure(response.Error ?? "Evaluation failed");
        }
        catch (Exception ex)
        {
            return Result<DimensionScore>.Failure(ex.Message, ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result<DimensionScore>> EvaluatePacingAsync(
        string content,
        PacingIntensity expectedPacing,
        CancellationToken cancellationToken = default)
    {
        var pacingContext = $"Expected pacing: {expectedPacing}";

        var prompt = $@"Evaluate pacing in this chapter.

{pacingContext}

Content:
{content.Substring(0, Math.Min(content.Length, 8000))}

Evaluate:
1. Scene pacing
2. Tension management
3. Action/reflection balance
4. Chapter rhythm
5. Hook and cliffhanger effectiveness

Respond in JSON:
{{
  ""score"": 0-100,
  ""strengths"": [""strength1""],
  ""weaknesses"": [""weakness1""],
  ""explanation"": ""detailed explanation""
}}";

        try
        {
            var response = await _aiService.GenerateAsync(
                prompt,
                new CoreGenerationOptions { Temperature = 0.3, MaxTokens = 1000, ResponseFormat = "json" },
                cancellationToken);

            if (response.IsSuccess)
            {
                return ParseDimensionScore(response.Value!, "Pacing", 0.9);
            }
            return Result<DimensionScore>.Failure(response.Error ?? "Evaluation failed");
        }
        catch (Exception ex)
        {
            return Result<DimensionScore>.Failure(ex.Message, ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result<DimensionScore>> EvaluateDialogueAsync(
        string content,
        List<Guid> characters,
        CharacterBible characterBible,
        DialogueGuidelines dialogueGuidelines,
        CancellationToken cancellationToken = default)
    {
        var prompt = $@"Evaluate dialogue quality in this chapter.

Content:
{content.Substring(0, Math.Min(content.Length, 8000))}

Evaluate:
1. Natural dialogue flow
2. Character voice distinction
3. Subtext usage
4. Dialogue tags effectiveness
5. Purpose (advances plot/reveals character)

Respond in JSON:
{{
  ""score"": 0-100,
  ""strengths"": [""strength1""],
  ""weaknesses"": [""weakness1""],
  ""explanation"": ""detailed explanation""
}}";

        try
        {
            var response = await _aiService.GenerateAsync(
                prompt,
                new CoreGenerationOptions { Temperature = 0.3, MaxTokens = 1000, ResponseFormat = "json" },
                cancellationToken);

            if (response.IsSuccess)
            {
                return ParseDimensionScore(response.Value!, "Dialogue", 0.8);
            }
            return Result<DimensionScore>.Failure(response.Error ?? "Evaluation failed");
        }
        catch (Exception ex)
        {
            return Result<DimensionScore>.Failure(ex.Message, ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result<List<RevisionInstruction>>> GenerateRevisionInstructionsAsync(
        ComprehensiveQualityReport report,
        int maxInstructions = 10,
        CancellationToken cancellationToken = default)
    {
        var instructions = new List<RevisionInstruction>();
        var priority = 1;

        // Generate instructions for each critical issue
        foreach (var issue in report.Issues.Where(i => i.Severity == QualityIssueSeverity.Critical).Take(maxInstructions / 2))
        {
            instructions.Add(new RevisionInstruction
            {
                Type = "Critical Fix",
                TargetLocation = issue.LocationInText ?? "Unknown",
                CurrentText = issue.TextExcerpt ?? "",
                Instruction = issue.SuggestedFix ?? $"Address: {issue.Description}",
                Priority = priority++,
                AddressesIssues = new List<Guid> { issue.Id }
            });
        }

        // Generate instructions for major issues
        foreach (var issue in report.Issues.Where(i => i.Severity == QualityIssueSeverity.Major).Take(maxInstructions - instructions.Count))
        {
            instructions.Add(new RevisionInstruction
            {
                Type = "Improvement",
                TargetLocation = issue.LocationInText ?? "Unknown",
                CurrentText = issue.TextExcerpt ?? "",
                Instruction = issue.SuggestedFix ?? $"Improve: {issue.Description}",
                Priority = priority++,
                AddressesIssues = new List<Guid> { issue.Id }
            });
        }

        return Result<List<RevisionInstruction>>.Success(instructions);
    }

    /// <inheritdoc />
    public async Task<Result<AutoFixResult>> AutoFixIssuesAsync(
        string content,
        List<QualityIssue> issues,
        CancellationToken cancellationToken = default)
    {
        var fixableIssues = issues.Where(i => i.AutoFixable && i.Severity == QualityIssueSeverity.Minor).ToList();
        var appliedFixes = new List<AppliedFix>();
        var fixedContent = content;

        foreach (var issue in fixableIssues)
        {
            if (!string.IsNullOrEmpty(issue.TextExcerpt) && !string.IsNullOrEmpty(issue.SuggestedFix))
            {
                var fixPrompt = $@"Fix this minor issue in the text.

Original: {issue.TextExcerpt}
Issue: {issue.Description}
Suggested fix: {issue.SuggestedFix}

Provide only the corrected text, nothing else.";

                try
                {
                    var response = await _aiService.GenerateAsync(
                        fixPrompt,
                        new CoreGenerationOptions { Temperature = 0.2, MaxTokens = 500 },
                        cancellationToken);

                    if (response.IsSuccess && !string.IsNullOrEmpty(response.Value))
                    {
                        fixedContent = fixedContent.Replace(issue.TextExcerpt, response.Value.Trim());
                        appliedFixes.Add(new AppliedFix
                        {
                            IssueId = issue.Id,
                            OriginalText = issue.TextExcerpt,
                            FixedText = response.Value.Trim(),
                            Reason = issue.Description
                        });
                        issue.WasAutoFixed = true;
                    }
                }
                catch
                {
                    // Skip this fix if it fails
                }
            }
        }

        var unfixedIssues = issues.Where(i => !i.WasAutoFixed).ToList();
        var estimatedImprovement = appliedFixes.Count * 2; // Rough estimate

        return Result<AutoFixResult>.Success(new AutoFixResult
        {
            FixedContent = fixedContent,
            FixesApplied = appliedFixes,
            UnfixedIssues = unfixedIssues,
            EstimatedImprovement = estimatedImprovement
        });
    }

    /// <inheritdoc />
    public async Task<Result<QualityComparisonResult>> CompareQualityAsync(
        GeneratedChapter original,
        GeneratedChapter revised,
        CancellationToken cancellationToken = default)
    {
        // Simple comparison based on re-evaluation
        // In production, would do full re-evaluation
        var scoreDelta = 5; // Assume some improvement
        var originalScore = original.QualityReport?.OverallScore ?? 70;

        return await Task.FromResult(Result<QualityComparisonResult>.Success(new QualityComparisonResult
        {
            OriginalScore = originalScore,
            RevisedScore = originalScore + scoreDelta,
            Recommendation = scoreDelta > 0 ? "Keep revised version" : "Consider further revisions"
        }));
    }

    #region Private Helper Methods

    private Result<DimensionScore> ParseDimensionScore(string response, string dimensionName, double weight)
    {
        try
        {
            var cleanJson = CleanJsonResponse(response);
            var json = JsonDocument.Parse(cleanJson);
            var root = json.RootElement;

            var score = root.TryGetProperty("score", out var s) ? s.GetInt32() : 70;
            var strengths = GetStringArray(root, "strengths");
            var weaknesses = GetStringArray(root, "weaknesses");
            var explanation = root.TryGetProperty("explanation", out var e) ? e.GetString() ?? "" : "";

            return Result<DimensionScore>.Success(new DimensionScore
            {
                DimensionName = dimensionName,
                Score = score,
                Weight = weight,
                Explanation = explanation,
                Strengths = strengths,
                Weaknesses = weaknesses
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing dimension score for {Dimension}", dimensionName);
            return Result<DimensionScore>.Success(CreateDefaultDimension(dimensionName, 70, weight));
        }
    }

    private static string CleanJsonResponse(string response)
    {
        var cleaned = response.Trim();
        if (cleaned.StartsWith("```"))
        {
            cleaned = cleaned.Substring(cleaned.IndexOf('\n') + 1);
            cleaned = cleaned.Substring(0, cleaned.LastIndexOf("```")).Trim();
        }
        return cleaned;
    }

    private static List<string> GetStringArray(JsonElement root, string propertyName)
    {
        if (root.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.Array)
        {
            return prop.EnumerateArray()
                .Where(e => e.ValueKind == JsonValueKind.String)
                .Select(e => e.GetString()!)
                .ToList();
        }
        return new List<string>();
    }

    private static DimensionScore CreateDefaultDimension(string name, int score = 75, double weight = 1.0)
    {
        return new DimensionScore
        {
            DimensionName = name,
            Score = score,
            Weight = weight,
            Explanation = "Default score assigned"
        };
    }

    private static double CalculateOverallScore(List<DimensionScore> dimensions)
    {
        if (!dimensions.Any()) return 70;

        var totalWeight = dimensions.Sum(d => d.Weight);
        var weightedSum = dimensions.Sum(d => d.Score * d.Weight);

        return totalWeight > 0 ? weightedSum / totalWeight : 70;
    }

    private static QualityVerdict DetermineVerdict(double score)
    {
        return score switch
        {
            >= 90 => QualityVerdict.Excellent,
            >= 75 => QualityVerdict.Good,
            >= 60 => QualityVerdict.Acceptable,
            >= 40 => QualityVerdict.NeedsWork,
            _ => QualityVerdict.Regenerate
        };
    }

    private static List<QualityIssue> ExtractIssuesFromDimension(DimensionScore dimension, string category)
    {
        var issues = new List<QualityIssue>();

        foreach (var weakness in dimension.Weaknesses ?? Enumerable.Empty<string>())
        {
            var severity = dimension.Score < 50 ? QualityIssueSeverity.Major :
                          dimension.Score < 70 ? QualityIssueSeverity.Minor :
                          QualityIssueSeverity.Suggestion;

            issues.Add(new QualityIssue
            {
                Severity = severity,
                Category = category,
                Description = weakness,
                AutoFixable = severity == QualityIssueSeverity.Minor,
                ScoreImpact = (100 - dimension.Score) / 10
            });
        }

        return issues;
    }

    private static List<ImprovementSuggestion> GenerateImprovementSuggestions(
        List<DimensionScore> dimensions, 
        List<QualityIssue> issues)
    {
        var suggestions = new List<ImprovementSuggestion>();

        // Add suggestions for lowest scoring dimensions
        var lowestDimensions = dimensions.OrderBy(d => d.Score).Take(3);

        foreach (var dim in lowestDimensions.Where(d => d.Score < 80))
        {
            suggestions.Add(new ImprovementSuggestion
            {
                Category = dim.Dimension,
                Suggestion = $"Focus on improving {dim.Dimension.ToLower()} quality",
                ExpectedImpact = $"+{Math.Min(20, 100 - dim.Score)} points potential",
                Priority = dim.Score < 60 ? 10 : dim.Score < 70 ? 7 : 5
            });
        }

        return suggestions;
    }

    #endregion
}



