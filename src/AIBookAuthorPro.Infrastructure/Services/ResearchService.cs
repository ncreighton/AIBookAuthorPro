// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Text.Json;
using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Enums;
using AIBookAuthorPro.Core.Interfaces;
using AIBookAuthorPro.Core.Models.Research;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.Infrastructure.Services;

/// <summary>
/// Implementation of research service using AI providers.
/// </summary>
public sealed class ResearchService : IResearchService
{
    private readonly IAIProvider _aiProvider;
    private readonly IProjectService _projectService;
    private readonly ILogger<ResearchService> _logger;

    public ResearchService(
        IAIProvider aiProvider,
        IProjectService projectService,
        ILogger<ResearchService> logger)
    {
        _aiProvider = aiProvider ?? throw new ArgumentNullException(nameof(aiProvider));
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<Result<ResearchResultSet>> SearchAsync(
        ResearchQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Performing research search for query: {Query}, Type: {Type}", 
                query.Query, query.Type);

            var prompt = BuildResearchPrompt(query);
            var request = new GenerationRequest
            {
                Prompt = prompt,
                MaxTokens = 4000,
                Temperature = 0.3 // Lower temperature for factual research
            };

            var result = await _aiProvider.GenerateAsync(request, cancellationToken);
            if (result.IsFailure)
            {
                return Result<ResearchResultSet>.Failure($"Research failed: {result.Error}");
            }

            var resultSet = ParseResearchResults(result.Value!.Content, query);
            resultSet.Duration = result.Value.Duration;

            _logger.LogInformation("Research completed with {Count} results for query: {Query}",
                resultSet.Results.Count, query.Query);

            return Result<ResearchResultSet>.Success(resultSet);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Research search cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing research search for query: {Query}", query.Query);
            return Result<ResearchResultSet>.Failure($"Research error: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result<List<NameSuggestion>>> GenerateCharacterNamesAsync(
        string? gender = null,
        string? origin = null,
        string? timePeriod = null,
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Generating {Count} character names - Gender: {Gender}, Origin: {Origin}, Period: {Period}",
                count, gender, origin, timePeriod);

            var prompt = $@"Generate {count} character name suggestions for a novel with the following criteria:
{(gender != null ? $"- Gender: {gender}" : "- Any gender")}
{(origin != null ? $"- Cultural/Ethnic Origin: {origin}" : "- Any cultural background")}
{(timePeriod != null ? $"- Time Period: {timePeriod}" : "- Contemporary or any period")}

For each name, provide:
1. The full name (first and last)
2. Cultural origin
3. Meaning of the name (if known)
4. Gender association
5. Time period appropriateness
6. Popularity/usage notes
7. 1-2 notable examples of characters or people with this name

Format as JSON array:
[
  {{
    ""name"": ""Full Name"",
    ""origin"": ""Cultural origin"",
    ""meaning"": ""Name meaning"",
    ""gender"": ""Male/Female/Unisex"",
    ""timePeriod"": ""Appropriate era"",
    ""popularity"": ""Usage notes"",
    ""notableExamples"": [""Example 1"", ""Example 2""]
  }}
]

Provide diverse, interesting names that would work well for fiction characters.
Return ONLY the JSON array, no other text.";

            var request = new GenerationRequest
            {
                Prompt = prompt,
                MaxTokens = 2000,
                Temperature = 0.8 // Higher temperature for creative variety
            };

            var result = await _aiProvider.GenerateAsync(request, cancellationToken);
            if (result.IsFailure)
            {
                return Result<List<NameSuggestion>>.Failure($"Name generation failed: {result.Error}");
            }

            var names = ParseNameSuggestions(result.Value!.Content);
            
            _logger.LogInformation("Generated {Count} character name suggestions", names.Count);
            return Result<List<NameSuggestion>>.Success(names);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Name generation cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating character names");
            return Result<List<NameSuggestion>>.Failure($"Name generation error: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result<List<GenreTrope>>> GetGenreTropesAsync(
        string genre,
        bool includeSubversions = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting genre tropes for: {Genre}, Include subversions: {IncludeSubversions}",
                genre, includeSubversions);

            var prompt = $@"Provide a comprehensive list of common tropes and conventions in {genre} fiction.

For each trope, include:
1. Name of the trope
2. Description of what it is
3. 2-3 examples from popular works
4. Related tropes
5. Tips for using it effectively
{(includeSubversions ? "6. How it can be subverted or deconstructed" : "")}

Format as JSON array:
[
  {{
    ""name"": ""Trope Name"",
    ""description"": ""What this trope is"",
    ""examples"": [""Book/Movie 1"", ""Book/Movie 2""],
    ""genre"": ""{genre}"",
    ""isSubversion"": false,
    ""relatedTropes"": [""Related Trope 1""],
    ""writingTips"": [""Tip 1"", ""Tip 2""]
  }}
]

Include 15-20 tropes, mixing classic and modern variations.
Return ONLY the JSON array, no other text.";

            var request = new GenerationRequest
            {
                Prompt = prompt,
                MaxTokens = 4000,
                Temperature = 0.4
            };

            var result = await _aiProvider.GenerateAsync(request, cancellationToken);
            if (result.IsFailure)
            {
                return Result<List<GenreTrope>>.Failure($"Trope retrieval failed: {result.Error}");
            }

            var tropes = ParseGenreTropes(result.Value!.Content);
            
            _logger.LogInformation("Retrieved {Count} genre tropes for {Genre}", tropes.Count, genre);
            return Result<List<GenreTrope>>.Success(tropes);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Genre trope retrieval cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting genre tropes for {Genre}", genre);
            return Result<List<GenreTrope>>.Failure($"Trope retrieval error: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result<List<AccuracyIssue>>> CheckHistoricalAccuracyAsync(
        string text,
        string timePeriod,
        string? location = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Checking historical accuracy for period: {Period}, Location: {Location}",
                timePeriod, location);

            var prompt = $@"Analyze the following text for historical accuracy issues related to the time period: {timePeriod}
{(location != null ? $"Location context: {location}" : "")}

TEXT TO ANALYZE:
{text}

Identify any:
1. Anachronisms (things that didn't exist in that time period)
2. Incorrect terminology or language for the era
3. Inaccurate cultural practices or customs
4. Technology or objects that weren't available
5. Historical events or dates that are wrong
6. Social norms that don't match the period

For each issue found, provide:
- The problematic text excerpt
- What the issue is
- A suggested correction
- Severity (Low/Medium/High)
- Category of issue

Format as JSON array:
[
  {{
    ""text"": ""problematic excerpt"",
    ""issue"": ""description of the problem"",
    ""suggestion"": ""how to fix it"",
    ""severity"": ""Medium"",
    ""category"": ""Anachronism/Language/Culture/Technology/Events/Social""
  }}
]

If no issues are found, return an empty array: []
Return ONLY the JSON array, no other text.";

            var request = new GenerationRequest
            {
                Prompt = prompt,
                MaxTokens = 3000,
                Temperature = 0.2 // Low temperature for accuracy
            };

            var result = await _aiProvider.GenerateAsync(request, cancellationToken);
            if (result.IsFailure)
            {
                return Result<List<AccuracyIssue>>.Failure($"Accuracy check failed: {result.Error}");
            }

            var issues = ParseAccuracyIssues(result.Value!.Content);
            
            _logger.LogInformation("Found {Count} historical accuracy issues", issues.Count);
            return Result<List<AccuracyIssue>>.Success(issues);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Historical accuracy check cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking historical accuracy");
            return Result<List<AccuracyIssue>>.Failure($"Accuracy check error: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result<LocationResearchResult>> ResearchLocationAsync(
        string location,
        List<string>? aspectsToResearch = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Researching location: {Location}", location);

            var aspects = aspectsToResearch ?? new List<string> 
            { 
                "geography", "climate", "culture", "history", "landmarks", "sensory details" 
            };

            var prompt = $@"Provide comprehensive research about the location: {location}

Focus on these aspects: {string.Join(", ", aspects)}

Include:
1. General description
2. Geographical information (terrain, coordinates, nearby locations, population)
3. Climate information (type, seasons, weather patterns)
4. Cultural information (language, religion, customs, food, festivals)
5. Historical facts (key events and periods)
6. Notable landmarks
7. Sensory details for fiction writing (sights, sounds, smells, textures, tastes, atmosphere)

Format as JSON:
{{
  ""locationName"": ""{location}"",
  ""description"": ""General description"",
  ""geography"": {{
    ""coordinates"": ""lat, long or general area"",
    ""terrain"": ""description"",
    ""nearbyLocations"": ""nearby places"",
    ""population"": ""if applicable""
  }},
  ""climate"": {{
    ""type"": ""climate classification"",
    ""seasons"": ""seasonal patterns"",
    ""averageTemperature"": ""range"",
    ""weather"": ""typical weather""
  }},
  ""culture"": {{
    ""language"": ""languages spoken"",
    ""religion"": ""predominant religions"",
    ""customs"": ""notable customs"",
    ""food"": ""local cuisine"",
    ""festivals"": ""celebrations""
  }},
  ""history"": [
    {{""period"": ""time period"", ""fact"": ""historical fact"", ""source"": ""optional source""}}
  ],
  ""landmarks"": [""landmark 1"", ""landmark 2""],
  ""sensoryDetails"": {{
    ""sights"": [""visual detail 1""],
    ""sounds"": [""sound 1""],
    ""smells"": [""smell 1""],
    ""textures"": [""texture 1""],
    ""tastes"": [""taste 1""],
    ""atmosphere"": ""overall feel""
  }}
}}

Return ONLY the JSON object, no other text.";

            var request = new GenerationRequest
            {
                Prompt = prompt,
                MaxTokens = 4000,
                Temperature = 0.3
            };

            var result = await _aiProvider.GenerateAsync(request, cancellationToken);
            if (result.IsFailure)
            {
                return Result<LocationResearchResult>.Failure($"Location research failed: {result.Error}");
            }

            var locationResult = ParseLocationResult(result.Value!.Content);
            
            _logger.LogInformation("Completed location research for: {Location}", location);
            return Result<LocationResearchResult>.Success(locationResult);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Location research cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error researching location: {Location}", location);
            return Result<LocationResearchResult>.Failure($"Location research error: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result> SaveToProjectAsync(
        Guid projectId,
        ResearchResult result,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Saving research result to project {ProjectId}", projectId);

            var projectResult = await _projectService.GetProjectAsync(projectId, cancellationToken);
            if (projectResult.IsFailure)
            {
                return Result.Failure($"Project not found: {projectResult.Error}");
            }

            var project = projectResult.Value!;
            var note = new Core.Models.ResearchNote
            {
                Title = result.Title,
                Content = result.Content,
                Category = result.Type.ToString(),
                Source = result.SourceUrl,
                Tags = result.KeyFacts
            };

            project.AddResearchNote(note);
            result.IsSaved = true;

            var saveResult = await _projectService.SaveProjectAsync(project, cancellationToken);
            if (saveResult.IsFailure)
            {
                return Result.Failure($"Failed to save: {saveResult.Error}");
            }

            _logger.LogInformation("Saved research result '{Title}' to project {ProjectId}", 
                result.Title, projectId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving research to project {ProjectId}", projectId);
            return Result.Failure($"Save error: {ex.Message}");
        }
    }

    #region Private Methods

    private static string BuildResearchPrompt(ResearchQuery query)
    {
        var contextParts = new List<string>();
        
        if (!string.IsNullOrEmpty(query.TimePeriod))
            contextParts.Add($"Time Period: {query.TimePeriod}");
        if (!string.IsNullOrEmpty(query.GeographicContext))
            contextParts.Add($"Geographic Context: {query.GeographicContext}");
        if (!string.IsNullOrEmpty(query.GenreContext))
            contextParts.Add($"Genre Context: {query.GenreContext}");

        var context = contextParts.Count > 0 
            ? $"\n\nContext:\n{string.Join("\n", contextParts)}" 
            : "";

        return $@"Research the following topic for a fiction writer: {query.Query}
{context}

Research Type: {query.Type}

Provide {query.MaxResults} relevant results with:
1. Title/Topic
2. Key information content
3. Source type
4. Key facts extracted
5. Relevance to fiction writing

{(query.IncludeAISummary ? "Also provide an overall summary of the findings." : "")}

Format as JSON:
{{
  ""results"": [
    {{
      ""title"": ""Result title"",
      ""content"": ""Detailed information"",
      ""sourceName"": ""Type of source"",
      ""relevanceScore"": 0.95,
      ""keyFacts"": [""fact 1"", ""fact 2""]
    }}
  ],
  ""overallSummary"": ""Summary of findings"",
  ""suggestedFollowUps"": [""follow-up query 1""]
}}

Return ONLY the JSON object, no other text.";
    }

    private static ResearchResultSet ParseResearchResults(string content, ResearchQuery query)
    {
        var resultSet = new ResearchResultSet { Query = query };

        try
        {
            var json = ExtractJson(content);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("results", out var results))
            {
                foreach (var item in results.EnumerateArray())
                {
                    var result = new ResearchResult
                    {
                        Title = item.GetProperty("title").GetString() ?? "",
                        Content = item.GetProperty("content").GetString() ?? "",
                        SourceName = item.TryGetProperty("sourceName", out var sn) ? sn.GetString() : null,
                        RelevanceScore = item.TryGetProperty("relevanceScore", out var rs) ? rs.GetDouble() : 0.5,
                        Type = query.Type,
                        RetrievedAt = DateTime.UtcNow
                    };

                    if (item.TryGetProperty("keyFacts", out var facts))
                    {
                        foreach (var fact in facts.EnumerateArray())
                        {
                            result.KeyFacts.Add(fact.GetString() ?? "");
                        }
                    }

                    resultSet.Results.Add(result);
                }
            }

            if (root.TryGetProperty("overallSummary", out var summary))
            {
                resultSet.OverallSummary = summary.GetString();
            }

            if (root.TryGetProperty("suggestedFollowUps", out var followUps))
            {
                foreach (var fu in followUps.EnumerateArray())
                {
                    resultSet.SuggestedFollowUps.Add(fu.GetString() ?? "");
                }
            }
        }
        catch (JsonException ex)
        {
            resultSet.Notes.Add($"Warning: Could not fully parse results - {ex.Message}");
            resultSet.Results.Add(new ResearchResult
            {
                Title = "Research Results",
                Content = content,
                Type = query.Type,
                RetrievedAt = DateTime.UtcNow
            });
        }

        return resultSet;
    }

    private static List<NameSuggestion> ParseNameSuggestions(string content)
    {
        var names = new List<NameSuggestion>();

        try
        {
            var json = ExtractJson(content);
            using var doc = JsonDocument.Parse(json);

            foreach (var item in doc.RootElement.EnumerateArray())
            {
                var name = new NameSuggestion
                {
                    Name = item.GetProperty("name").GetString() ?? "",
                    Origin = item.TryGetProperty("origin", out var o) ? o.GetString() : null,
                    Meaning = item.TryGetProperty("meaning", out var m) ? m.GetString() : null,
                    Gender = item.TryGetProperty("gender", out var g) ? g.GetString() : null,
                    TimePeriod = item.TryGetProperty("timePeriod", out var t) ? t.GetString() : null,
                    Popularity = item.TryGetProperty("popularity", out var p) ? p.GetString() : null
                };

                if (item.TryGetProperty("notableExamples", out var examples))
                {
                    foreach (var ex in examples.EnumerateArray())
                    {
                        name.NotableExamples.Add(ex.GetString() ?? "");
                    }
                }

                names.Add(name);
            }
        }
        catch (JsonException)
        {
            // Return empty list if parsing fails
        }

        return names;
    }

    private static List<GenreTrope> ParseGenreTropes(string content)
    {
        var tropes = new List<GenreTrope>();

        try
        {
            var json = ExtractJson(content);
            using var doc = JsonDocument.Parse(json);

            foreach (var item in doc.RootElement.EnumerateArray())
            {
                var trope = new GenreTrope
                {
                    Name = item.GetProperty("name").GetString() ?? "",
                    Description = item.GetProperty("description").GetString() ?? "",
                    Genre = item.TryGetProperty("genre", out var g) ? g.GetString() ?? "" : "",
                    IsSubversion = item.TryGetProperty("isSubversion", out var s) && s.GetBoolean()
                };

                if (item.TryGetProperty("examples", out var examples))
                {
                    foreach (var ex in examples.EnumerateArray())
                    {
                        trope.Examples.Add(ex.GetString() ?? "");
                    }
                }

                if (item.TryGetProperty("relatedTropes", out var related))
                {
                    foreach (var r in related.EnumerateArray())
                    {
                        trope.RelatedTropes.Add(r.GetString() ?? "");
                    }
                }

                if (item.TryGetProperty("writingTips", out var tips))
                {
                    foreach (var tip in tips.EnumerateArray())
                    {
                        trope.WritingTips.Add(tip.GetString() ?? "");
                    }
                }

                tropes.Add(trope);
            }
        }
        catch (JsonException)
        {
            // Return empty list if parsing fails
        }

        return tropes;
    }

    private static List<AccuracyIssue> ParseAccuracyIssues(string content)
    {
        var issues = new List<AccuracyIssue>();

        try
        {
            var json = ExtractJson(content);
            using var doc = JsonDocument.Parse(json);

            foreach (var item in doc.RootElement.EnumerateArray())
            {
                var issue = new AccuracyIssue
                {
                    Text = item.GetProperty("text").GetString() ?? "",
                    Issue = item.GetProperty("issue").GetString() ?? "",
                    Suggestion = item.TryGetProperty("suggestion", out var s) ? s.GetString() : null,
                    Severity = item.TryGetProperty("severity", out var sev) ? sev.GetString() ?? "Medium" : "Medium",
                    Category = item.TryGetProperty("category", out var c) ? c.GetString() ?? "" : ""
                };

                issues.Add(issue);
            }
        }
        catch (JsonException)
        {
            // Return empty list if parsing fails
        }

        return issues;
    }

    private static LocationResearchResult ParseLocationResult(string content)
    {
        var result = new LocationResearchResult();

        try
        {
            var json = ExtractJson(content);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            result.LocationName = root.GetProperty("locationName").GetString() ?? "";
            result.Description = root.GetProperty("description").GetString() ?? "";

            if (root.TryGetProperty("geography", out var geo))
            {
                result.Geography = new GeographyInfo
                {
                    Coordinates = geo.TryGetProperty("coordinates", out var c) ? c.GetString() : null,
                    Terrain = geo.TryGetProperty("terrain", out var t) ? t.GetString() : null,
                    NearbyLocations = geo.TryGetProperty("nearbyLocations", out var n) ? n.GetString() : null,
                    Population = geo.TryGetProperty("population", out var p) ? p.GetString() : null
                };
            }

            if (root.TryGetProperty("climate", out var climate))
            {
                result.Climate = new ClimateInfo
                {
                    Type = climate.TryGetProperty("type", out var t) ? t.GetString() : null,
                    Seasons = climate.TryGetProperty("seasons", out var s) ? s.GetString() : null,
                    AverageTemperature = climate.TryGetProperty("averageTemperature", out var a) ? a.GetString() : null,
                    Weather = climate.TryGetProperty("weather", out var w) ? w.GetString() : null
                };
            }

            if (root.TryGetProperty("culture", out var culture))
            {
                result.Culture = new CultureInfo
                {
                    Language = culture.TryGetProperty("language", out var l) ? l.GetString() : null,
                    Religion = culture.TryGetProperty("religion", out var r) ? r.GetString() : null,
                    Customs = culture.TryGetProperty("customs", out var c) ? c.GetString() : null,
                    Food = culture.TryGetProperty("food", out var f) ? f.GetString() : null,
                    Festivals = culture.TryGetProperty("festivals", out var fe) ? fe.GetString() : null
                };
            }

            if (root.TryGetProperty("history", out var history))
            {
                foreach (var h in history.EnumerateArray())
                {
                    result.History.Add(new HistoricalFact
                    {
                        Period = h.GetProperty("period").GetString() ?? "",
                        Fact = h.GetProperty("fact").GetString() ?? "",
                        Source = h.TryGetProperty("source", out var s) ? s.GetString() : null
                    });
                }
            }

            if (root.TryGetProperty("landmarks", out var landmarks))
            {
                foreach (var l in landmarks.EnumerateArray())
                {
                    result.Landmarks.Add(l.GetString() ?? "");
                }
            }

            if (root.TryGetProperty("sensoryDetails", out var sensory))
            {
                result.SensoryDetails = new SensoryDetails
                {
                    Atmosphere = sensory.TryGetProperty("atmosphere", out var a) ? a.GetString() : null
                };

                if (sensory.TryGetProperty("sights", out var sights))
                    foreach (var s in sights.EnumerateArray())
                        result.SensoryDetails.Sights.Add(s.GetString() ?? "");

                if (sensory.TryGetProperty("sounds", out var sounds))
                    foreach (var s in sounds.EnumerateArray())
                        result.SensoryDetails.Sounds.Add(s.GetString() ?? "");

                if (sensory.TryGetProperty("smells", out var smells))
                    foreach (var s in smells.EnumerateArray())
                        result.SensoryDetails.Smells.Add(s.GetString() ?? "");

                if (sensory.TryGetProperty("textures", out var textures))
                    foreach (var t in textures.EnumerateArray())
                        result.SensoryDetails.Textures.Add(t.GetString() ?? "");

                if (sensory.TryGetProperty("tastes", out var tastes))
                    foreach (var t in tastes.EnumerateArray())
                        result.SensoryDetails.Tastes.Add(t.GetString() ?? "");
            }
        }
        catch (JsonException)
        {
            result.LocationName = "Unknown";
            result.Description = content;
        }

        return result;
    }

    private static string ExtractJson(string content)
    {
        // Find JSON in the content (handle markdown code blocks)
        var trimmed = content.Trim();
        
        if (trimmed.StartsWith("```json"))
        {
            trimmed = trimmed[7..];
            var endIndex = trimmed.LastIndexOf("```");
            if (endIndex > 0)
                trimmed = trimmed[..endIndex];
        }
        else if (trimmed.StartsWith("```"))
        {
            trimmed = trimmed[3..];
            var endIndex = trimmed.LastIndexOf("```");
            if (endIndex > 0)
                trimmed = trimmed[..endIndex];
        }

        return trimmed.Trim();
    }

    #endregion
}
