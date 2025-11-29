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
/// Service for analyzing and expanding user prompts into comprehensive book concepts.
/// </summary>
public sealed class PromptAnalysisService : IPromptAnalysisService
{
    private readonly IAIService _aiService;
    private readonly ILogger<PromptAnalysisService> _logger;

    private const int MinimumPromptLength = 50;
    private const int IdealPromptLength = 200;

    public PromptAnalysisService(
        IAIService aiService,
        ILogger<PromptAnalysisService> logger)
    {
        _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<Result<PromptAnalysisResult>> AnalyzePromptAsync(
        BookSeedPrompt prompt,
        CancellationToken cancellationToken = default)
    {
        if (prompt == null)
            return Result<PromptAnalysisResult>.Failure("Prompt cannot be null");

        _logger.LogInformation("Analyzing prompt: {PromptLength} CharacterBible", prompt.RawPrompt.Length);

        try
        {
            var validationResult = ValidatePrompt(prompt);
            if (!validationResult.IsSuccess || !validationResult.Value!.IsValid)
            {
                _logger.LogWarning("Prompt validation failed: {Issues}", 
                    string.Join(", ", validationResult.Value?.Issues.Select(i => i.Message) ?? Array.Empty<string>()));
            }

            var analysisPrompt = BuildAnalysisPrompt(prompt);
            
            var response = await _aiService.GenerateAsync(
                analysisPrompt,
                new CoreGenerationOptions
                {
                    Temperature = 0.3,
                    MaxTokens = 4000,
                    ResponseFormat = "json"
                },
                cancellationToken);

            if (!response.IsSuccess)
            {
                _logger.LogError("AI analysis failed: {Error}", response.Error);
                return Result<PromptAnalysisResult>.Failure($"Analysis failed: {response.Error}");
            }

            var result = ParseAnalysisResponse(response.Value!, prompt.Id);
            
            _logger.LogInformation("Analysis complete. Genre: {Genre}, Confidence: {Confidence}",
                result.DetectedGenre, result.Confidence.GenreConfidence);

            return Result<PromptAnalysisResult>.Success(result);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Prompt analysis cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing prompt");
            return Result<PromptAnalysisResult>.Failure($"Analysis error: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result<ExpandedCreativeBrief>> ExpandPromptAsync(
        BookSeedPrompt prompt,
        PromptAnalysisResult analysis,
        Dictionary<Guid, string>? clarifications = null,
        CancellationToken cancellationToken = default)
    {
        if (prompt == null)
            return Result<ExpandedCreativeBrief>.Failure("Prompt cannot be null");
        if (analysis == null)
            return Result<ExpandedCreativeBrief>.Failure("Analysis cannot be null");

        _logger.LogInformation("Expanding prompt into creative brief");

        try
        {
            var expansionPrompt = BuildExpansionPrompt(prompt, analysis, clarifications);
            
            var response = await _aiService.GenerateAsync(
                expansionPrompt,
                new CoreGenerationOptions
                {
                    Temperature = 0.7,
                    MaxTokens = 6000,
                    ResponseFormat = "json"
                },
                cancellationToken);

            if (!response.IsSuccess)
            {
                _logger.LogError("Brief expansion failed: {Error}", response.Error);
                return Result<ExpandedCreativeBrief>.Failure($"Expansion failed: {response.Error}");
            }

            var brief = ParseExpansionResponse(response.Value!, prompt.Id, analysis.Id);
            
            _logger.LogInformation("Creative brief generated: {Title}", brief.WorkingTitle);

            return Result<ExpandedCreativeBrief>.Success(brief);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Prompt expansion cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error expanding prompt");
            return Result<ExpandedCreativeBrief>.Failure($"Expansion error: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result<List<ClarificationRequest>>> GenerateClarificationsAsync(
        PromptAnalysisResult analysis,
        CancellationToken cancellationToken = default)
    {
        if (analysis == null)
            return Result<List<ClarificationRequest>>.Failure("Analysis cannot be null");

        _logger.LogInformation("Generating clarification questions");

        try
        {
            var clarifications = new List<ClarificationRequest>();

            // Add existing clarifications from analysis
            if (analysis.ClarificationRequests?.Any() == true)
            {
                clarifications.AddRange(analysis.ClarificationRequests);
            }

            // Generate additional clarifications based on low confidence areas
            if (analysis.Confidence.GenreConfidence < 0.7)
            {
                clarifications.Add(new ClarificationRequest
                {
                    Question = "What genre best describes your book? (e.g., Fantasy, Romance, Thriller, Literary Fiction)",
                    Category = "Genre",
                    Priority = ClarificationPriority.Important,
                    SuggestedOptions = new List<string> 
                    { 
                        "Fantasy", "Science Fiction", "Romance", "Thriller", 
                        "Mystery", "Literary Fiction", "Horror", "Historical Fiction" 
                    }
                });
            }

            if (analysis.Confidence.CharacterConfidence < 0.6)
            {
                clarifications.Add(new ClarificationRequest
                {
                    Question = "Can you tell me more about your main character? What drives them?",
                    Category = "CharacterBible",
                    Priority = ClarificationPriority.Important
                });
            }

            if (analysis.Confidence.StructureConfidence < 0.5)
            {
                clarifications.Add(new ClarificationRequest
                {
                    Question = "How long do you envision your book being? (Short novel ~50k words, Standard ~80k, Epic ~120k+)",
                    Category = "Structure",
                    Priority = ClarificationPriority.Optional,
                    SuggestedOptions = new List<string>
                    {
                        "Short Novel (40-60k words)",
                        "Standard Novel (70-90k words)",
                        "Long Novel (100-120k words)",
                        "Epic (120k+ words)"
                    }
                });
            }

            _logger.LogInformation("Generated {Count} clarification questions", clarifications.Count);

            return Result<List<ClarificationRequest>>.Success(clarifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating clarifications");
            return Result<List<ClarificationRequest>>.Failure($"Error: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public Result<PromptValidationResult> ValidatePrompt(BookSeedPrompt prompt)
    {
        if (prompt == null)
            return Result<PromptValidationResult>.Failure("Prompt cannot be null");

        var issues = new List<PromptValidationIssue>();
        var suggestions = new List<string>();

        // Check minimum length
        var meetsMinimum = prompt.RawPrompt.Length >= MinimumPromptLength;
        if (!meetsMinimum)
        {
            issues.Add(new PromptValidationIssue
            {
                Type = "Length",
                Message = $"Prompt is too short. Minimum {MinimumPromptLength} CharacterBible required.",
                Severity = ClarificationPriority.Critical
            });
        }

        // Check for basic story elements
        var hasCharacterHint = ContainsCharacterReference(prompt.RawPrompt);
        if (!hasCharacterHint)
        {
            suggestions.Add("Consider mentioning your main character or protagonist.");
        }

        var hasConflictHint = ContainsConflictReference(prompt.RawPrompt);
        if (!hasConflictHint)
        {
            suggestions.Add("Adding a hint about the central conflict would help.");
        }

        // Calculate clarity score
        var clarityScore = CalculateClarityScore(prompt.RawPrompt);

        var result = new PromptValidationResult
        {
            IsValid = !issues.Any(i => i.Severity == ClarificationPriority.Critical),
            Issues = issues,
            Suggestions = suggestions,
            MeetsMinimumLength = meetsMinimum,
            ClarityScore = clarityScore
        };

        return Result<PromptValidationResult>.Success(result);
    }

    /// <inheritdoc />
    public async Task<Result<List<EnhancementSuggestion>>> SuggestEnhancementsAsync(
        PromptAnalysisResult analysis,
        CancellationToken cancellationToken = default)
    {
        if (analysis == null)
            return Result<List<EnhancementSuggestion>>.Failure("Analysis cannot be null");

        // Return existing suggestions from analysis
        var suggestions = analysis.EnhancementSuggestions?.ToList() ?? new List<EnhancementSuggestion>();

        // Add confidence-based suggestions
        if (analysis.Confidence.WorldBuildingConfidence < 0.5)
        {
            suggestions.Add(new EnhancementSuggestion
            {
                Category = "World Building",
                Suggestion = "Consider adding details about the setting - time period, location, or unique world elements.",
                Rationale = "Helps create a more immersive and consistent world.",
                ImpactLevel = 3
            });
        }

        return Result<List<EnhancementSuggestion>>.Success(suggestions);
    }

    /// <inheritdoc />
    public async Task<Result<BookSeedPrompt>> ImportFromConversationAsync(
        string conversationText,
        PromptSource source,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(conversationText))
            return Result<BookSeedPrompt>.Failure("Conversation text cannot be empty");

        _logger.LogInformation("Importing prompt from {Source}", source);

        try
        {
            var extractionPrompt = BuildExtractionPrompt(conversationText, source);
            
            var response = await _aiService.GenerateAsync(
                extractionPrompt,
                new CoreGenerationOptions
                {
                    Temperature = 0.2,
                    MaxTokens = 2000
                },
                cancellationToken);

            if (!response.IsSuccess)
            {
                return Result<BookSeedPrompt>.Failure($"Extraction failed: {response.Error}");
            }

            var seedPrompt = new BookSeedPrompt
            {
                RawPrompt = response.Value!.Trim(),
                Source = source,
                SubmittedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Extracted prompt: {Length} CharacterBible", seedPrompt.RawPrompt.Length);

            return Result<BookSeedPrompt>.Success(seedPrompt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing from conversation");
            return Result<BookSeedPrompt>.Failure($"Import error: {ex.Message}", ex);
        }
    }

    #region Private Methods

    private string BuildAnalysisPrompt(BookSeedPrompt prompt)
    {
        return $@"You are an expert book development editor analyzing a creative prompt for a new book.

Analyze the following book concept and extract all implicit and explicit elements:

<prompt>
{prompt.RawPrompt}
</prompt>

{(prompt.AdditionalContext != null ? $"<additional_context>\n{prompt.AdditionalContext}\n</additional_context>" : "")}

{(prompt.ComparableTitles?.Any() == true ? $"<comparable_titles>\n{string.Join(", ", prompt.ComparableTitles)}\n</comparable_titles>" : "")}

Provide a comprehensive analysis in JSON format with the following structure:
{{
  ""detectedGenre"": ""primary genre"",
  ""subGenres"": [""list of sub-genres""],
  ""targetAudience"": ""description of target readers"",
  ""audienceAgeRange"": ""Children|MiddleGrade|YoungAdult|NewAdult|Adult|AllAges"",
  ""estimatedWordCount"": number,
  ""suggestedStructure"": ""ThreeAct|HerosJourney|SevenPoint|SaveTheCat|etc"",
  ""extractedThemes"": [""theme1"", ""theme2""],
  ""coreConflict"": ""description of central conflict"",
  ""tone"": ""description of overall tone"",
  ""pacing"": ""Slow|Moderate|Fast|Variable"",
  ""extractedCharacters"": [
    {{
      ""name"": ""character name or placeholder"",
      ""role"": ""Protagonist|Antagonist|Supporting|etc"",
      ""description"": ""brief description"",
      ""arcType"": ""arc description""
    }}
  ],
  ""extractedLocations"": [
    {{
      ""name"": ""location name"",
      ""type"": ""location type"",
      ""significance"": ""why it matters""
    }}
  ],
  ""worldBuildingRequirements"": [""requirement1"", ""requirement2""],
  ""researchRequirements"": [""research topic 1"", ""research topic 2""],
  ""marketAnalysis"": {{
    ""comparableAuthors"": [""author1"", ""author2""],
    ""marketTrends"": ""analysis of current market"",
    ""uniqueSellingPoints"": [""usp1"", ""usp2""]
  }},
  ""analysisConfidence"": {{
    ""genreConfidence"": 0.0-1.0,
    ""themeConfidence"": 0.0-1.0,
    ""structureConfidence"": 0.0-1.0,
    ""characterConfidence"": 0.0-1.0,
    ""worldBuildingConfidence"": 0.0-1.0
  }},
  ""clarificationNeeded"": [
    {{
      ""question"": ""clarifying question"",
      ""category"": ""category"",
      ""priority"": ""Critical|Important|Optional"",
      ""reason"": ""why this is needed""
    }}
  ],
  ""enhancementSuggestions"": [
    {{
      ""category"": ""category"",
      ""suggestion"": ""suggestion text"",
      ""impact"": ""expected impact"",
      ""priority"": 1-5
    }}
  ],
  ""identifiedChallenges"": [""challenge1"", ""challenge2""]
}}

Provide thorough analysis. Be specific and actionable.";
    }

    private string BuildExpansionPrompt(
        BookSeedPrompt prompt, 
        PromptAnalysisResult analysis,
        Dictionary<Guid, string>? clarifications)
    {
        var clarificationContext = "";
        if (clarifications?.Any() == true)
        {
            clarificationContext = "\n\nUser clarifications:\n" + 
                string.Join("\n", clarifications.Select(c => $"- {c.Value}"));
        }

        return $@"You are an expert book development editor creating a comprehensive creative brief.

Original prompt:
{prompt.RawPrompt}

Analysis results:
- Genre: {analysis.DetectedGenre}
- Themes: {string.Join(", ", analysis.ExtractedThemes ?? new List<string>())}
- Target Audience: {analysis.TargetAudience}
- Core Conflict: {analysis.CoreConflict}
{clarificationContext}

Create a comprehensive creative brief in JSON format:
{{
  ""workingTitle"": ""suggested title"",
  ""premise"": ""2-3 sentence premise"",
  ""logline"": ""single sentence logline"",
  ""elevatorPitch"": ""30-second pitch"",
  ""backCoverBlurb"": ""compelling back cover text"",
  ""extendedSynopsis"": ""detailed 500-word synopsis"",
  ""themeDefinitions"": [
    {{
      ""theme"": ""theme name"",
      ""definition"": ""what this theme means in the story"",
      ""exploration"": ""how it will be explored""
    }}
  ],
  ""motifs"": [""recurring motif 1"", ""motif 2""],
  ""symbols"": [
    {{
      ""symbol"": ""symbol name"",
      ""meaning"": ""what it represents""
    }}
  ],
  ""genreTropes"": {{
    ""embraced"": [""trope to use well""],
    ""subverted"": [""trope to subvert""],
    ""avoided"": [""cliche to avoid""]
  }},
  ""conflictFramework"": {{
    ""internal"": ""protagonist internal conflict"",
    ""external"": ""external conflict"",
    ""philosophical"": ""thematic/philosophical conflict"",
    ""interpersonal"": ""relationship conflicts""
  }},
  ""emotionalJourney"": ""description of reader's emotional arc"",
  ""researchRequirements"": [
    {{
      ""topic"": ""research topic"",
      ""importance"": ""why needed"",
      ""suggestedSources"": [""source1""]
    }}
  ]
}}

Be creative, specific, and compelling. This brief will guide the entire book creation.";
    }

    private string BuildExtractionPrompt(string conversationText, PromptSource source)
    {
        return $@"Extract the core book concept from this {source} conversation.

Conversation:
{conversationText}

Extract and consolidate the key book concept into a clear, comprehensive prompt that captures:
- The story premise
- Main CharacterBible mentioned
- Setting/world details
- Genre/tone
- Any specific requirements or preferences

Provide ONLY the consolidated prompt, no additional commentary.";
    }

    private PromptAnalysisResult ParseAnalysisResponse(string response, Guid promptId)
    {
        try
        {
            // Clean response if wrapped in markdown
            var cleanJson = response.Trim();
            if (cleanJson.StartsWith("```"))
            {
                cleanJson = cleanJson.Substring(cleanJson.IndexOf('\n') + 1);
                cleanJson = cleanJson.Substring(0, cleanJson.LastIndexOf("```")).Trim();
            }

            var json = JsonDocument.Parse(cleanJson);
            var root = json.RootElement;

            return new PromptAnalysisResult
            {
                PromptId = promptId,
                DetectedGenre = root.GetProperty("detectedGenre").GetString() ?? "Unknown",
                SubGenres = GetStringArray(root, "subGenres"),
                TargetAudience = root.GetProperty("targetAudience").GetString() ?? "",
                AudienceAgeRange = ParseEnum<AudienceAgeRange>(root, "audienceAgeRange", AudienceAgeRange.Adult),
                EstimatedWordCount = root.TryGetProperty("estimatedWordCount", out var wc) ? wc.GetInt32() : 80000,
                SuggestedStructure = ParseEnum<StructureTemplate>(root, "suggestedStructure", StructureTemplate.ThreeAct),
                ExtractedThemes = GetStringArray(root, "extractedThemes"),
                CoreConflict = root.GetProperty("coreConflict").GetString() ?? "",
                ToneDescriptor = root.TryGetProperty("tone", out var t) ? t.GetString() ?? "" : "",
                Pacing = ParseEnum<NarrativePacing>(root, "pacing", NarrativePacing.Moderate),
                AnalysisConfidence = ParseConfidence(root),
                AnalyzedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing analysis response, using defaults");
            return new PromptAnalysisResult
            {
                PromptId = promptId,
                DetectedGenre = "Fiction",
                TargetAudience = "General Adult",
                AudienceAgeRange = AudienceAgeRange.Adult,
                EstimatedWordCount = 80000,
                SuggestedStructure = StructureTemplate.ThreeAct,
                CoreConflict = "To be determined",
                Pacing = NarrativePacing.Moderate,
                AnalysisConfidence = new AnalysisConfidence(),
                AnalyzedAt = DateTime.UtcNow
            };
        }
    }

    private ExpandedCreativeBrief ParseExpansionResponse(string response, Guid promptId, Guid analysisId)
    {
        try
        {
            var cleanJson = response.Trim();
            if (cleanJson.StartsWith("```"))
            {
                cleanJson = cleanJson.Substring(cleanJson.IndexOf('\n') + 1);
                cleanJson = cleanJson.Substring(0, cleanJson.LastIndexOf("```")).Trim();
            }

            var json = JsonDocument.Parse(cleanJson);
            var root = json.RootElement;

            return new ExpandedCreativeBrief
            {
                PromptId = promptId,
                AnalysisId = analysisId,
                WorkingTitle = root.GetProperty("workingTitle").GetString() ?? "Untitled",
                Premise = root.GetProperty("premise").GetString() ?? "",
                Logline = root.GetProperty("logline").GetString() ?? "",
                ElevatorPitch = root.TryGetProperty("elevatorPitch", out var ep) ? ep.GetString() ?? "" : "",
                BackCoverBlurb = root.TryGetProperty("backCoverBlurb", out var bcb) ? bcb.GetString() ?? "" : "",
                ExtendedSynopsis = root.TryGetProperty("extendedSynopsis", out var es) ? es.GetString() ?? "" : "",
                Motifs = GetStringArray(root, "motifs").Select(m => new MotifDefinition { Motif = m }).ToList(),
                MoodKeywords = root.TryGetProperty("emotionalJourney", out var ej) 
                    ? (ej.ValueKind == JsonValueKind.Array ? GetStringArray(root, "emotionalJourney") : new List<string> { ej.GetString() ?? "" })
                    : new List<string>(),
                CreatedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing expansion response, using defaults");
            return new ExpandedCreativeBrief
            {
                PromptId = promptId,
                AnalysisId = analysisId,
                WorkingTitle = "Untitled Project",
                Premise = "To be developed",
                Logline = "To be developed",
                CreatedAt = DateTime.UtcNow
            };
        }
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

    private static T ParseEnum<T>(JsonElement root, string propertyName, T defaultValue) where T : struct, Enum
    {
        if (root.TryGetProperty(propertyName, out var prop))
        {
            var value = prop.GetString();
            if (Enum.TryParse<T>(value, true, out var result))
                return result;
        }
        return defaultValue;
    }

    private static AnalysisConfidence ParseConfidence(JsonElement root)
    {
        if (root.TryGetProperty("analysisConfidence", out var conf))
        {
            return new AnalysisConfidence
            {
                GenreConfidence = conf.TryGetProperty("genreConfidence", out var gc) ? gc.GetDouble() : 0.5,
                ThemeConfidence = conf.TryGetProperty("themeConfidence", out var tc) ? tc.GetDouble() : 0.5,
                StructureConfidence = conf.TryGetProperty("structureConfidence", out var sc) ? sc.GetDouble() : 0.5,
                CharacterConfidence = conf.TryGetProperty("characterConfidence", out var cc) ? cc.GetDouble() : 0.5,
                WorldBuildingConfidence = conf.TryGetProperty("worldBuildingConfidence", out var wbc) ? wbc.GetDouble() : 0.5
            };
        }
        return new AnalysisConfidence();
    }

    private static bool ContainsCharacterReference(string text)
    {
        var characterKeywords = new[] { "character", "protagonist", "hero", "heroine", "main", "she", "he", "they", "woman", "man", "girl", "boy" };
        return characterKeywords.Any(k => text.Contains(k, StringComparison.OrdinalIgnoreCase));
    }

    private static bool ContainsConflictReference(string text)
    {
        var conflictKeywords = new[] { "conflict", "struggle", "fight", "battle", "against", "must", "challenge", "problem", "threat", "danger" };
        return conflictKeywords.Any(k => text.Contains(k, StringComparison.OrdinalIgnoreCase));
    }

    private static int CalculateClarityScore(string text)
    {
        var score = 50; // Base score

        if (text.Length >= IdealPromptLength) score += 20;
        else if (text.Length >= MinimumPromptLength) score += 10;

        if (ContainsCharacterReference(text)) score += 10;
        if (ContainsConflictReference(text)) score += 10;

        // Check for setting indicators
        var settingKeywords = new[] { "world", "city", "kingdom", "future", "past", "century", "planet", "country" };
        if (settingKeywords.Any(k => text.Contains(k, StringComparison.OrdinalIgnoreCase))) score += 10;

        return Math.Min(100, score);
    }

    #endregion
}



