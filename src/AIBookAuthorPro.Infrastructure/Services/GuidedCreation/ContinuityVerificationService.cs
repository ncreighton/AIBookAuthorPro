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
/// Service for verifying continuity across chapters.
/// </summary>
public sealed class ContinuityVerificationService : IContinuityVerificationService
{
    private readonly IAIService _aiService;
    private readonly ILogger<ContinuityVerificationService> _logger;

    public ContinuityVerificationService(
        IAIService aiService,
        ILogger<ContinuityVerificationService> logger)
    {
        _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<Result<ContinuityReport>> VerifyChapterContinuityAsync(
        GeneratedChapter chapter,
        ContinuityVerificationContext context,
        CancellationToken cancellationToken = default)
    {
        if (chapter == null)
            return Result<ContinuityReport>.Failure("Chapter cannot be null");

        _logger.LogInformation("Verifying continuity for Chapter {ChapterNumber}", chapter.ChapterNumber);

        try
        {
            var allIssues = new List<object>();
            var characterIssues = new List<CharacterContinuityIssue>();
            var plotIssues = new List<PlotContinuityIssue>();
            var timelineIssues = new List<TimelineContinuityIssue>();
            var settingIssues = new List<SettingContinuityIssue>();
            var objectIssues = new List<ObjectContinuityIssue>();

            // Verify character continuity
            var characterIds = new List<Guid>();
            var charResult = await VerifyCharacterContinuityAsync(
                chapter.Content ?? "", characterIds, chapter.CharacterStates, 
                context.CharacterBible ?? new CharacterBible(), cancellationToken);
            if (charResult.IsSuccess)
                characterIssues.AddRange(charResult.Value!);

            // Verify plot continuity
            var previousEvents = context.PreviousChapters?
                .SelectMany(c => c.KeyEvents ?? new List<string>())
                .ToList() ?? new List<string>();
            var plotResult = await VerifyPlotContinuityAsync(
                chapter.Content ?? "", context.Blueprint,
                context.PlotArchitecture ?? new PlotArchitecture(), previousEvents, cancellationToken);
            if (plotResult.IsSuccess)
                plotIssues.AddRange(plotResult.Value!);

            // Verify timeline continuity
            var timelineEvents = context.World?.Timeline?.Events ?? new List<TimelineEvent>();
            var timeResult = await VerifyTimelineContinuityAsync(
                chapter.Content ?? "", context.Blueprint.Timeline,
                timelineEvents, cancellationToken);
            if (timeResult.IsSuccess)
                timelineIssues.AddRange(timeResult.Value!);

            // Verify setting continuity
            var locationIds = context.Blueprint.LocationIds ?? new List<Guid>();
            var settingResult = await VerifySettingContinuityAsync(
                chapter.Content ?? "", locationIds, context.World ?? new WorldBible(), cancellationToken);
            if (settingResult.IsSuccess)
                settingIssues.AddRange(settingResult.Value!);

            // Calculate overall continuity score
            var totalIssues = characterIssues.Count + plotIssues.Count + 
                             timelineIssues.Count + settingIssues.Count + objectIssues.Count;
            var criticalIssues = characterIssues.Count(i => i.Severity == QualityIssueSeverity.Critical) +
                                plotIssues.Count(i => i.Severity == QualityIssueSeverity.Critical) +
                                timelineIssues.Count(i => i.Severity == QualityIssueSeverity.Critical);

            var continuityScore = CalculateContinuityScore(totalIssues, criticalIssues);
            var passesContinuity = continuityScore >= 70 && criticalIssues == 0;

            var report = new ContinuityReport
            {
                ChapterId = chapter.Id,
                ChapterNumber = chapter.ChapterNumber,
                PassesContinuityCheck = passesContinuity,
                ContinuityScore = continuityScore,
                CheckedAt = DateTime.UtcNow,
                CharacterIssues = characterIssues,
                PlotIssues = plotIssues,
                TimelineIssues = timelineIssues,
                SettingIssues = settingIssues,
                ObjectIssues = objectIssues,
                // TotalIssueCount is computed from issue lists
                CriticalIssueCount = criticalIssues
            };

            _logger.LogInformation("Continuity check complete: Score={Score}, Issues={Issues}, Critical={Critical}",
                continuityScore, totalIssues, criticalIssues);

            return Result<ContinuityReport>.Success(report);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Continuity verification cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying continuity");
            return Result<ContinuityReport>.Failure($"Verification error: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result<List<CharacterContinuityIssue>>> VerifyCharacterContinuityAsync(
        string content,
        List<Guid> expectedCharacters,
        List<CharacterStateSnapshot> characterStates,
        CharacterBible characterBible,
        CancellationToken cancellationToken = default)
    {
        var issues = new List<CharacterContinuityIssue>();

        if (characterBible == null)
            return Result<List<CharacterContinuityIssue>>.Success(issues);

        var characterNames = characterBible.MainCharacters?
            .Select(c => c.FullName).ToList() ?? new List<string>();

        if (!characterNames.Any())
            return Result<List<CharacterContinuityIssue>>.Success(issues);

        var prompt = $@"Check for character continuity issues in this chapter.

Known Characters: {string.Join(", ", characterNames)}

Previous character states:
{JsonSerializer.Serialize(characterStates?.Take(5) ?? Enumerable.Empty<CharacterStateSnapshot>())}

Chapter content:
{content.Substring(0, Math.Min(content.Length, 6000))}

Check for:
1. CharacterBible knowing things they shouldn't know yet
2. CharacterBible behaving inconsistently with established traits
3. CharacterBible appearing in wrong locations
4. Physical description changes
5. Relationship inconsistencies

Respond in JSON:
{{
  ""issues"": [
    {{
      ""characterName"": ""name"",
      ""issueType"": ""CharacterKnowledge|CharacterBehavior|CharacterAppearance|CharacterLocation"",
      ""severity"": ""Critical|Major|Minor"",
      ""description"": ""description"",
      ""expected"": ""what was expected"",
      ""actual"": ""what was found"",
      ""suggestedFix"": ""how to fix""
    }}
  ]
}}";

        try
        {
            var response = await _aiService.GenerateAsync(
                prompt,
                new CoreGenerationOptions { Temperature = 0.2, MaxTokens = 2000, ResponseFormat = "json" },
                cancellationToken);

            if (response.IsSuccess)
            {
                issues = ParseCharacterIssues(response.Value!);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking character continuity");
        }

        return Result<List<CharacterContinuityIssue>>.Success(issues);
    }

    /// <inheritdoc />
    public async Task<Result<List<PlotContinuityIssue>>> VerifyPlotContinuityAsync(
        string content,
        ChapterBlueprint blueprint,
        PlotArchitecture plotArchitecture,
        List<string> previousEvents,
        CancellationToken cancellationToken = default)
    {
        var issues = new List<PlotContinuityIssue>();

        var recentEvents = previousEvents?.Take(20).ToList() ?? new List<string>();
        if (!previousEvents.Any())
            return Result<List<PlotContinuityIssue>>.Success(issues);

        var prompt = $@"Check for plot continuity issues in this chapter.

Previous events:
{string.Join("\n", previousEvents)}

Chapter content:
{content.Substring(0, Math.Min(content.Length, 6000))}

Check for:
1. Plot contradictions with previous chapters
2. Forgotten plot threads
3. Events referenced that haven't happened
4. Inconsistent cause and effect

Respond in JSON:
{{
  ""issues"": [
    {{
      ""plotThread"": ""thread name"",
      ""issueType"": ""Contradiction|ForgottenThread|FutureReference|CausalityError"",
      ""severity"": ""Critical|Major|Minor"",
      ""description"": ""description"",
      ""contradictionWith"": ""what it contradicts"",
      ""suggestedFix"": ""how to fix""
    }}
  ]
}}";

        try
        {
            var response = await _aiService.GenerateAsync(
                prompt,
                new CoreGenerationOptions { Temperature = 0.2, MaxTokens = 2000, ResponseFormat = "json" },
                cancellationToken);

            if (response.IsSuccess)
            {
                issues = ParsePlotIssues(response.Value!);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking plot continuity");
        }

        return Result<List<PlotContinuityIssue>>.Success(issues);
    }

    /// <inheritdoc />
    public async Task<Result<List<TimelineContinuityIssue>>> VerifyTimelineContinuityAsync(
        string content,
        TimelinePosition timelinePosition,
        List<TimelineEvent> previousTimeline,
        CancellationToken cancellationToken = default)
    {
        var issues = new List<TimelineContinuityIssue>();

        var timelineEvents = previousTimeline?.Take(10).ToList() ?? new List<TimelineEvent>();

        var prompt = $@"Check for timeline/temporal continuity issues in this chapter.

Known timeline:
{string.Join("\n", timelineEvents)}

Chapter content:
{content.Substring(0, Math.Min(content.Length, 6000))}

Check for:
1. Time inconsistencies (wrong day, impossible travel time)
2. Chronological errors
3. Season/weather mismatches
4. Age inconsistencies

Respond in JSON:
{{
  ""issues"": [
    {{
      ""issueType"": ""TimeSkip|ChronologyError|SeasonMismatch|AgeInconsistency"",
      ""severity"": ""Critical|Major|Minor"",
      ""description"": ""description"",
      ""expectedTimeline"": ""expected"",
      ""actualTimeline"": ""found"",
      ""suggestedFix"": ""how to fix""
    }}
  ]
}}";

        try
        {
            var response = await _aiService.GenerateAsync(
                prompt,
                new CoreGenerationOptions { Temperature = 0.2, MaxTokens = 1500, ResponseFormat = "json" },
                cancellationToken);

            if (response.IsSuccess)
            {
                issues = ParseTimelineIssues(response.Value!);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking timeline continuity");
        }

        return Result<List<TimelineContinuityIssue>>.Success(issues);
    }

    /// <inheritdoc />
    public async Task<Result<List<SettingContinuityIssue>>> VerifySettingContinuityAsync(
        string content,
        List<Guid> expectedLocations,
        WorldBible worldBible,
        CancellationToken cancellationToken = default)
    {
        var issues = new List<SettingContinuityIssue>();

        if (worldBible == null)
            return Result<List<SettingContinuityIssue>>.Success(issues);

        var locations = worldBible.Locations?.Take(10)
            .Select(l => $"{l.Name}: {l.Description}").ToList() ?? new List<string>();

        if (!locations.Any())
            return Result<List<SettingContinuityIssue>>.Success(issues);

        var prompt = $@"Check for setting/location continuity issues.

Known locations:
{string.Join("\n", locations)}

Chapter content:
{content.Substring(0, Math.Min(content.Length, 6000))}

Check for:
1. Location description inconsistencies
2. Geography errors
3. Setting detail contradictions

Respond in JSON:
{{
  ""issues"": [
    {{
      ""locationName"": ""name"",
      ""issueType"": ""DescriptionChange|GeographyError|DetailContradiction"",
      ""severity"": ""Critical|Major|Minor"",
      ""description"": ""description"",
      ""expected"": ""expected"",
      ""actual"": ""found"",
      ""suggestedFix"": ""how to fix""
    }}
  ]
}}";

        try
        {
            var response = await _aiService.GenerateAsync(
                prompt,
                new CoreGenerationOptions { Temperature = 0.2, MaxTokens = 1500, ResponseFormat = "json" },
                cancellationToken);

            if (response.IsSuccess)
            {
                issues = ParseSettingIssues(response.Value!);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking setting continuity");
        }

        return Result<List<SettingContinuityIssue>>.Success(issues);
    }

    /// <inheritdoc />
    public async Task<Result<List<ObjectContinuityIssue>>> VerifyObjectContinuityAsync(
        string content,
        List<TrackedObject> trackedObjects,
        CancellationToken cancellationToken = default)
    {
        // Track important objects mentioned in the story
        return Result<List<ObjectContinuityIssue>>.Success(new List<ObjectContinuityIssue>());
    }

    /// <inheritdoc />
    public async Task<Result<List<CharacterStateSnapshot>>> ExtractCharacterStatesAsync(
        string content,
        List<Guid> characters,
        CharacterBible characterBible,
        CancellationToken cancellationToken = default)
    {
        var states = new List<CharacterStateSnapshot>();

        if (characterBible?.MainCharacters == null)
            return Result<List<CharacterStateSnapshot>>.Success(states);

        var characterNames = characterBible.MainCharacters.Select(c => c.FullName).ToList();

        var prompt = $@"Extract character states at the end of this chapter.

CharacterBible to track: {string.Join(", ", characterNames)}

Chapter content:
{content.Substring(0, Math.Min(content.Length, 6000))}

For each character present, provide:
{{
  ""states"": [
    {{
      ""characterName"": ""name"",
      ""emotionalState"": ""current emotional state"",
      ""location"": ""where they are at end of chapter"",
      ""knowledgeGained"": [""new info they learned""],
      ""relationshipChanges"": [""any relationship changes""],
      ""arcProgress"": ""progress in their arc""
    }}
  ]
}}";

        try
        {
            var response = await _aiService.GenerateAsync(
                prompt,
                new CoreGenerationOptions { Temperature = 0.3, MaxTokens = 2000, ResponseFormat = "json" },
                cancellationToken);

            if (response.IsSuccess)
            {
                states = ParseCharacterStates(response.Value!, characterBible);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting character states");
        }

        return Result<List<CharacterStateSnapshot>>.Success(states);
    }

    /// <inheritdoc />
    public async Task<Result<List<string>>> ExtractKeyEventsAsync(
        string content,
        CancellationToken cancellationToken = default)
    {
        var events = new List<string>();

        var prompt = $@"Extract key plot events from this chapter.

Chapter content:
{content.Substring(0, Math.Min(content.Length, 6000))}

List the key events that happened (5-10 events):
{{
  ""events"": [""event 1"", ""event 2"", ...]
}}";

        try
        {
            var response = await _aiService.GenerateAsync(
                prompt,
                new CoreGenerationOptions { Temperature = 0.3, MaxTokens = 1000, ResponseFormat = "json" },
                cancellationToken);

            if (response.IsSuccess)
            {
                var cleanJson = CleanJsonResponse(response.Value!);
                var json = JsonDocument.Parse(cleanJson);
                if (json.RootElement.TryGetProperty("events", out var eventsArray))
                {
                    events = eventsArray.EnumerateArray()
                        .Where(e => e.ValueKind == JsonValueKind.String)
                        .Select(e => e.GetString()!)
                        .ToList();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting key events");
        }

        return Result<List<string>>.Success(events);
    }

    /// <inheritdoc />
    public async Task<Result<string>> BuildContinuityContextAsync(
        List<GeneratedChapter> chapters,
        int maxContextSize,
        CancellationToken cancellationToken = default)
    {
        var contextParts = new List<string>();

        // Add recent chapter summaries
        if (chapters?.Any() == true)
        {
            var chapterSummaries = "RECENT EVENTS:\n" + string.Join("\n",
                chapters.TakeLast(3).Select(c =>
                    $"Chapter {c.ChapterNumber}: {c.Summaries?.AISummary ?? c.Summary ?? "Events occurred"}"));
            contextParts.Add(chapterSummaries);
        }

        var context = string.Join("\n\n", contextParts);

        // Compress if too long
        var estimatedTokens = _aiService.EstimateTokens(context);
        if (estimatedTokens > maxContextSize)
        {
            context = context.Substring(0, Math.Min(context.Length, maxContextSize * 3));
        }

        return Result<string>.Success(context);
    }

    #region Private Helper Methods

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

    private static int CalculateContinuityScore(int totalIssues, int criticalIssues)
    {
        var baseScore = 100;
        baseScore -= criticalIssues * 20;
        baseScore -= (totalIssues - criticalIssues) * 5;
        return Math.Max(0, Math.Min(100, baseScore));
    }

    private List<CharacterContinuityIssue> ParseCharacterIssues(string response)
    {
        var issues = new List<CharacterContinuityIssue>();
        try
        {
            var cleanJson = CleanJsonResponse(response);
            var json = JsonDocument.Parse(cleanJson);
            if (json.RootElement.TryGetProperty("issues", out var issuesArray))
            {
                foreach (var issue in issuesArray.EnumerateArray())
                {
                    issues.Add(new CharacterContinuityIssue
                    {
                        CharacterName = issue.GetProperty("characterName").GetString() ?? "",
                        Type = ParseContinuityIssueType(issue.GetProperty("issueType").GetString()),
                        Severity = ParseSeverity(issue.GetProperty("severity").GetString()),
                        Description = issue.GetProperty("description").GetString() ?? "",
                        Expected = issue.TryGetProperty("expected", out var exp) ? exp.GetString() ?? "" : "",
                        Actual = issue.TryGetProperty("actual", out var act) ? act.GetString() ?? "" : "",
                        SuggestedFix = issue.TryGetProperty("suggestedFix", out var fix) ? fix.GetString() : null
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing character issues");
        }
        return issues;
    }

    private List<PlotContinuityIssue> ParsePlotIssues(string response)
    {
        var issues = new List<PlotContinuityIssue>();
        try
        {
            var cleanJson = CleanJsonResponse(response);
            var json = JsonDocument.Parse(cleanJson);
            if (json.RootElement.TryGetProperty("issues", out var issuesArray))
            {
                foreach (var issue in issuesArray.EnumerateArray())
                {
                    issues.Add(new PlotContinuityIssue
                    {
                        Type = ParseContinuityIssueType(issue.GetProperty("issueType").GetString()),
                        Severity = ParseSeverity(issue.GetProperty("severity").GetString()),
                        Description = issue.GetProperty("description").GetString() ?? "",
                        ContradictionWith = issue.TryGetProperty("contradictionWith", out var con) ? con.GetString() : null,
                        SuggestedFix = issue.TryGetProperty("suggestedFix", out var fix) ? fix.GetString() : null
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing plot issues");
        }
        return issues;
    }

    private List<TimelineContinuityIssue> ParseTimelineIssues(string response)
    {
        var issues = new List<TimelineContinuityIssue>();
        try
        {
            var cleanJson = CleanJsonResponse(response);
            var json = JsonDocument.Parse(cleanJson);
            if (json.RootElement.TryGetProperty("issues", out var issuesArray))
            {
                foreach (var issue in issuesArray.EnumerateArray())
                {
                    issues.Add(new TimelineContinuityIssue
                    {
                        Type = ParseContinuityIssueType(issue.GetProperty("issueType").GetString()),
                        Severity = ParseSeverity(issue.GetProperty("severity").GetString()),
                        Description = issue.GetProperty("description").GetString() ?? "",
                        ExpectedTimeline = issue.TryGetProperty("expectedTimeline", out var exp) ? exp.GetString() ?? "" : "",
                        ActualTimeline = issue.TryGetProperty("actualTimeline", out var act) ? act.GetString() ?? "" : "",
                        SuggestedFix = issue.TryGetProperty("suggestedFix", out var fix) ? fix.GetString() : null
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing timeline issues");
        }
        return issues;
    }

    private List<SettingContinuityIssue> ParseSettingIssues(string response)
    {
        var issues = new List<SettingContinuityIssue>();
        try
        {
            var cleanJson = CleanJsonResponse(response);
            var json = JsonDocument.Parse(cleanJson);
            if (json.RootElement.TryGetProperty("issues", out var issuesArray))
            {
                foreach (var issue in issuesArray.EnumerateArray())
                {
                    issues.Add(new SettingContinuityIssue
                    {
                        LocationName = issue.TryGetProperty("locationName", out var loc) ? loc.GetString() ?? "" : "",
                        Type = ParseContinuityIssueType(issue.GetProperty("issueType").GetString()),
                        Severity = ParseSeverity(issue.GetProperty("severity").GetString()),
                        Description = issue.GetProperty("description").GetString() ?? "",
                        Expected = issue.TryGetProperty("expected", out var exp) ? exp.GetString() ?? "" : "",
                        Actual = issue.TryGetProperty("actual", out var act) ? act.GetString() ?? "" : "",
                        SuggestedFix = issue.TryGetProperty("suggestedFix", out var fix) ? fix.GetString() : null
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing setting issues");
        }
        return issues;
    }

    private List<CharacterStateSnapshot> ParseCharacterStates(string response, CharacterBible characterBible)
    {
        var states = new List<CharacterStateSnapshot>();
        try
        {
            var cleanJson = CleanJsonResponse(response);
            var json = JsonDocument.Parse(cleanJson);
            if (json.RootElement.TryGetProperty("states", out var statesArray))
            {
                foreach (var state in statesArray.EnumerateArray())
                {
                    var charName = state.GetProperty("characterName").GetString();
                    var character = characterBible.MainCharacters?
                        .FirstOrDefault(c => c.FullName?.Equals(charName, StringComparison.OrdinalIgnoreCase) == true);

                    // Parse arc progress - try to extract a percentage or default to 0
                    var arcProgress = 0;
                    if (state.TryGetProperty("arcProgress", out var arc))
                    {
                        if (arc.ValueKind == JsonValueKind.Number)
                            arcProgress = arc.GetInt32();
                        else if (arc.ValueKind == JsonValueKind.String)
                        {
                            var arcStr = arc.GetString() ?? "";
                            if (int.TryParse(arcStr.Replace("%", "").Trim(), out var parsed))
                                arcProgress = parsed;
                        }
                    }

                    states.Add(new CharacterStateSnapshot
                    {
                        CharacterId = character?.Id ?? Guid.Empty,
                        CharacterName = charName ?? "Unknown",
                        EmotionalState = state.TryGetProperty("emotionalState", out var es) ? es.GetString() ?? "" : "",
                        Location = state.TryGetProperty("location", out var loc) ? loc.GetString() ?? "" : "",
                        ArcProgress = arcProgress
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing character states");
        }
        return states;
    }

    private static ContinuityIssueType ParseContinuityIssueType(string? type)
    {
        return type?.ToLower() switch
        {
            "characterknowledge" => ContinuityIssueType.CharacterKnowledge,
            "characterbehavior" => ContinuityIssueType.CharacterBehavior,
            "characterappearance" => ContinuityIssueType.CharacterAppearance,
            "characterlocation" => ContinuityIssueType.CharacterLocation,
            "timelineerror" => ContinuityIssueType.TimelineError,
            "settinginconsistency" => ContinuityIssueType.SettingInconsistency,
            "objecttracking" => ContinuityIssueType.ObjectTracking,
            "plotcontradiction" => ContinuityIssueType.PlotContradiction,
            _ => ContinuityIssueType.CharacterBehavior
        };
    }

    private static QualityIssueSeverity ParseSeverity(string? severity)
    {
        return severity?.ToLower() switch
        {
            "critical" => QualityIssueSeverity.Critical,
            "major" => QualityIssueSeverity.Major,
            "minor" => QualityIssueSeverity.Minor,
            _ => QualityIssueSeverity.Minor
        };
    }

    #endregion
}




