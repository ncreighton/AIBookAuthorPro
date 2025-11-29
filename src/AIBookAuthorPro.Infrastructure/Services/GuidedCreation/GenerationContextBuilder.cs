// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Text;
using AIBookAuthorPro.Application.Services.GuidedCreation;
using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Models.GuidedCreation;
using AIBookAuthorPro.Core.Services;
using CoreGenerationOptions = AIBookAuthorPro.Core.Services.GenerationOptions;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.Infrastructure.Services.GuidedCreation;

/// <summary>
/// Service for building comprehensive context for chapter generation.
/// </summary>
public sealed partial class GenerationContextBuilder : IGenerationContextBuilder
{
    private readonly IAIService _aiService;
    private readonly ILogger<GenerationContextBuilder> _logger;

    private const int DefaultMaxTokens = 8000;
    private const int SummaryMaxTokens = 500;

    public GenerationContextBuilder(
        IAIService aiService,
        ILogger<GenerationContextBuilder> logger)
    {
        _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<Result<ChapterGenerationContext>> BuildContextAsync(
        int chapterNumber,
        BookBlueprint blueprint,
        List<GeneratedChapter> previousChapters,
        GenerationConfiguration config,
        CancellationToken cancellationToken = default)
    {
        if (blueprint == null)
            return Result<ChapterGenerationContext>.Failure("Blueprint cannot be null");

        _logger.LogInformation("Building context for Chapter {ChapterNumber}", chapterNumber);

        try
        {
            var chapterBlueprint = blueprint.Structure.Chapters?
                .FirstOrDefault(c => c.ChapterNumber == chapterNumber);

            if (chapterBlueprint == null)
            {
                return Result<ChapterGenerationContext>.Failure(
                    $"No blueprint found for chapter {chapterNumber}");
            }

            var maxTokens = config?.ContextWindowSize ?? DefaultMaxTokens;

            // Build all context components
            var systemPrompt = await BuildSystemPromptAsync(blueprint, config, cancellationToken);
            var narrativeContext = await BuildNarrativeContextAsync(
                previousChapters, maxTokens / 3, cancellationToken);
            var characterContext = BuildCharacterContext(blueprint.Characters, chapterBlueprint);
            var worldContext = BuildWorldContext(blueprint.World, chapterBlueprint);
            var plotContext = BuildPlotContext(blueprint.Plot, chapterBlueprint);
            var styleContext = BuildStyleContext(blueprint.Style);
            var chapterInstructions = BuildChapterInstructions(chapterBlueprint, blueprint);

            // Extract character states from previous chapters
            var characterStates = ExtractLatestCharacterStates(previousChapters);

            // Get relevant setup/payoff tracking
            var activeSetups = GetActiveSetups(blueprint.Plot, chapterNumber);
            var duePayoffs = GetDuePayoffs(blueprint.Plot, chapterNumber);

            // Calculate token budget allocation
            var tokenBudget = CalculateTokenBudget(maxTokens);

            // Extract values from Results
            var systemPromptValue = systemPrompt.IsSuccess ? systemPrompt.Value! : string.Empty;
            var narrativeContextValue = narrativeContext.IsSuccess ? narrativeContext.Value! : string.Empty;
            var characterContextValue = characterContext.IsSuccess ? characterContext.Value! : string.Empty;
            var worldContextValue = worldContext.IsSuccess ? worldContext.Value! : string.Empty;
            var plotContextValue = plotContext.IsSuccess ? plotContext.Value! : string.Empty;
            var styleContextValue = styleContext.IsSuccess ? styleContext.Value! : string.Empty;
            var chapterInstructionsValue = chapterInstructions.IsSuccess ? chapterInstructions.Value! : string.Empty;
            var tokenBudgetValue = tokenBudget.IsSuccess ? tokenBudget.Value!.TotalAvailable : maxTokens;

            var context = new ChapterGenerationContext
            {
                ChapterNumber = chapterNumber,
                ChapterBlueprint = chapterBlueprint,
                SystemPrompt = systemPromptValue,
                NarrativeContext = narrativeContextValue,
                CharacterContext = characterContextValue,
                WorldContext = worldContextValue,
                PlotContext = plotContextValue,
                StyleContext = styleContextValue,
                ChapterInstructions = chapterInstructionsValue,
                PreviousChapterSummaries = GetChapterSummaryStrings(previousChapters),
                CharacterStates = characterStates,
                ActiveSetups = activeSetups,
                DuePayoffs = duePayoffs,
                TokenBudget = tokenBudgetValue,
                GenerationConfig = config ?? new GenerationConfiguration()
            };

            _logger.LogInformation("Context built successfully for Chapter {ChapterNumber}", chapterNumber);
            return Result<ChapterGenerationContext>.Success(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building context for Chapter {ChapterNumber}", chapterNumber);
            return Result<ChapterGenerationContext>.Failure($"Context build error: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result<string>> BuildSystemPromptAsync(
        BookBlueprint blueprint,
        GenerationConfiguration config,
        CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder();

        sb.AppendLine("You are an expert fiction author with decades of experience writing compelling narratives.");
        sb.AppendLine();

        // Book identity
        if (blueprint.Identity != null)
        {
            sb.AppendLine($"You are writing \"{blueprint.Identity.Title}\" - {blueprint.Identity.Genre}.");
            sb.AppendLine($"Premise: {blueprint.Identity.ExpandedPremise}");
            sb.AppendLine();
        }

        // Style guidelines
        if (blueprint.Style != null)
        {
            sb.AppendLine("WRITING STYLE:");
            if (blueprint.Style.Voice != null)
            {
                sb.AppendLine($"- Voice: {blueprint.Style.Voice.Description}");
                sb.AppendLine($"- POV: {blueprint.Style.Voice.PointOfView}");
                sb.AppendLine($"- Tense: {blueprint.Style.Voice.Tense}");
            }
            if (blueprint.Style.Prose != null)
            {
                sb.AppendLine($"- Sentence style: {blueprint.Style.Prose.SentenceStyle}");
                sb.AppendLine($"- Vocabulary level: {blueprint.Style.Prose.VocabularyLevel}");
            }
            sb.AppendLine();
        }

        // Quality requirements
        sb.AppendLine("QUALITY REQUIREMENTS:");
        sb.AppendLine("- Show, don't tell - use vivid sensory details");
        sb.AppendLine("- Write natural, character-appropriate dialogue");
        sb.AppendLine("- Maintain consistent pacing and tension");
        sb.AppendLine("- End chapters with hooks that compel reading");
        sb.AppendLine("- Stay true to established character voices and traits");
        sb.AppendLine();

        // Config-specific instructions
        if (config != null)
        {
            if (config.ContentSettings != null)
            {
                sb.AppendLine($"Content rating: {config.ContentSettings.ContentRating}");
                if (config.ContentSettings.AvoidTopics?.Any() == true)
                {
                    sb.AppendLine($"Avoid: {string.Join(", ", config.ContentSettings.AvoidTopics)}");
                }
            }
        }

        return Result<string>.Success(sb.ToString());
    }

    /// <inheritdoc />
    public async Task<Result<string>> BuildNarrativeContextAsync(
        List<GeneratedChapter> previousChapters,
        int maxTokens,
        CancellationToken cancellationToken = default)
    {
        if (previousChapters == null || !previousChapters.Any())
        {
            return Result<string>.Success("This is the first chapter of the book.");
        }

        var sb = new StringBuilder();
        sb.AppendLine("STORY SO FAR:");
        sb.AppendLine();

        // Get recent chapters (prioritize most recent)
        var recentChapters = previousChapters
            .OrderByDescending(c => c.ChapterNumber)
            .Take(5)
            .OrderBy(c => c.ChapterNumber)
            .ToList();

        foreach (var chapter in recentChapters)
        {
            var summary = chapter.Summaries?.AISummary ?? 
                         chapter.Summaries?.BriefSummary ?? 
                         chapter.Summary ??
                         "Events occurred in this chapter.";
            
            sb.AppendLine($"Chapter {chapter.ChapterNumber} ({chapter.Title}):");
            sb.AppendLine(summary);
            sb.AppendLine();
        }

        // Add the most recent chapter's ending for continuity
        var lastChapter = previousChapters.OrderByDescending(c => c.ChapterNumber).FirstOrDefault();
        if (!string.IsNullOrEmpty(lastChapter?.Content))
        {
            var lastParagraphs = GetLastParagraphs(lastChapter.Content, 2);
            sb.AppendLine("LAST CHAPTER ENDED WITH:");
            sb.AppendLine(lastParagraphs);
            sb.AppendLine();
        }

        var result = sb.ToString();

        // Compress if too long
        var estimatedTokens = _aiService.EstimateTokens(result);
        if (estimatedTokens > maxTokens)
        {
            var compressResult = await CompressContextAsync(result, maxTokens, cancellationToken);
            if (compressResult.IsSuccess)
            {
                result = compressResult.Value!;
            }
        }

        return Result<string>.Success(result);
    }

    /// <inheritdoc />
    public Result<string> BuildCharacterContext(
        CharacterBible? characterBible,
        ChapterBlueprint chapterBlueprint)
    {
        if (characterBible == null)
            return Result<string>.Success("");

        var sb = new StringBuilder();
        sb.AppendLine("CharacterBible IN THIS CHAPTER:");
        sb.AppendLine();

        // Get CharacterBible appearing in this chapter (extract character IDs)
        var chapterCharacterIds = chapterBlueprint.CharactersInChapter?
            .Select(ca => ca.CharacterId)
            .ToList() ?? new List<Guid>();
        
        // Add main CharacterBible relevant to this chapter
        var relevantMains = characterBible.MainCharacters?
            .Where(c => chapterCharacterIds.Count == 0 || 
                       chapterCharacterIds.Contains(c.Id))
            .ToList() ?? new List<CharacterProfile>();

        foreach (var character in relevantMains.Take(5))
        {
            sb.AppendLine($"**{character.FullName}** ({character.Role})");
            sb.AppendLine($"  Concept: {character.Concept}");
            if (character.CoreTraits?.Any() == true)
                sb.AppendLine($"  Traits: {string.Join(", ", character.CoreTraits.Take(5))}");
            if (!string.IsNullOrEmpty(character.SpeechPattern))
                sb.AppendLine($"  Speech: {character.SpeechPattern}");
            if (character.Arc != null)
                sb.AppendLine($"  Arc: {character.Arc.Type} - Current: {character.Arc.CurrentPhase}");
            sb.AppendLine();
        }

        // Add supporting CharacterBible briefly
        var relevantSupporting = characterBible.SupportingCharacters?
            .Where(c => chapterCharacterIds.Count == 0 || 
                       chapterCharacterIds.Contains(c.Id))
            .Take(3)
            .ToList() ?? new List<CharacterProfile>();

        if (relevantSupporting.Any())
        {
            sb.AppendLine("Supporting CharacterBible:");
            foreach (var character in relevantSupporting)
            {
                sb.AppendLine($"- {character.FullName}: {character.Concept}");
            }
            sb.AppendLine();
        }

        return Result<string>.Success(sb.ToString());
    }

    /// <inheritdoc />
    public Result<string> BuildWorldContext(
        WorldBible? worldBible,
        ChapterBlueprint chapterBlueprint)
    {
        if (worldBible == null)
            return Result<string>.Success("");

        var sb = new StringBuilder();
        sb.AppendLine("SETTING FOR THIS CHAPTER:");
        sb.AppendLine();

        // Add locations for this chapter
        var chapterLocationIds = chapterBlueprint.LocationsUsed ?? new List<Guid>();
        var relevantLocations = worldBible.Locations?
            .Where(l => chapterLocationIds.Count == 0 ||
                       chapterLocationIds.Contains(l.Id))
            .Take(3)
            .ToList() ?? new List<LocationProfile>();

        foreach (var location in relevantLocations)
        {
            sb.AppendLine($"**{location.Name}**");
            sb.AppendLine($"  {location.Description}");
            if (location.Atmosphere != null)
                sb.AppendLine($"  Atmosphere: {location.Atmosphere}");
            if (location.SensoryDetails?.Any() == true)
                sb.AppendLine($"  Sensory: {string.Join("; ", location.SensoryDetails.Take(3))}");
            sb.AppendLine();
        }

        // Add relevant world rules
        if (worldBible.Rules?.Any() == true)
        {
            sb.AppendLine("World rules to maintain:");
            foreach (var rule in worldBible.Rules.Take(5))
            {
                sb.AppendLine($"- {rule}");
            }
            sb.AppendLine();
        }

        // Add magic system if applicable
        if (worldBible.MagicSystem != null && !string.IsNullOrEmpty(worldBible.MagicSystem.Name))
        {
            sb.AppendLine("Magic system:");
            sb.AppendLine($"  {worldBible.MagicSystem.Description}");
            if (worldBible.MagicSystem.Limitations?.Any() == true)
                sb.AppendLine($"  Limitations: {string.Join(", ", worldBible.MagicSystem.Limitations.Take(3))}");
            sb.AppendLine();
        }

        return Result<string>.Success(sb.ToString());
    }

    /// <inheritdoc />
    public Result<string> BuildPlotContext(
        PlotArchitecture? plotArchitecture,
        ChapterBlueprint chapterBlueprint)
    {
        if (plotArchitecture == null)
            return Result<string>.Success("");

        var sb = new StringBuilder();
        sb.AppendLine("PLOT CONTEXT:");
        sb.AppendLine();

        // Main conflict
        if (plotArchitecture.MainPlot != null)
        {
            sb.AppendLine($"Central conflict: {plotArchitecture.MainPlot.CentralConflict}");
            sb.AppendLine($"Stakes: {plotArchitecture.MainPlot.Stakes}");
            sb.AppendLine();
        }

        // Active subplots
        var activeSubplots = plotArchitecture.Subplots?
            .Where(s => s.Status == SubplotStatus.Active)
            .Take(3)
            .ToList() ?? new List<Subplot>();

        if (activeSubplots.Any())
        {
            sb.AppendLine("Active subplots:");
            foreach (var subplot in activeSubplots)
            {
                sb.AppendLine($"- {subplot.Name}: {subplot.Description}");
            }
            sb.AppendLine();
        }

        // Chapter-specific plot points
        if (chapterBlueprint.PlotPoints?.Any() == true)
        {
            sb.AppendLine("Plot points for this chapter:");
            foreach (var point in chapterBlueprint.PlotPoints)
            {
                sb.AppendLine($"- {point}");
            }
            sb.AppendLine();
        }

        return Result<string>.Success(sb.ToString());
    }

    /// <inheritdoc />
    public Result<string> BuildStyleContext(StyleGuide? styleGuide)
    {
        if (styleGuide == null)
            return Result<string>.Success("");

        var sb = new StringBuilder();
        sb.AppendLine("STYLE GUIDELINES:");
        sb.AppendLine();

        if (styleGuide.Voice != null)
        {
            sb.AppendLine($"Voice: {styleGuide.Voice.Description}");
            sb.AppendLine($"POV: {styleGuide.Voice.PointOfView}");
            sb.AppendLine($"Tense: {styleGuide.Voice.Tense}");
            sb.AppendLine();
        }

        if (styleGuide.Prose != null)
        {
            sb.AppendLine($"Prose style: {styleGuide.Prose.SentenceStyle}");
            sb.AppendLine($"Paragraph length: {styleGuide.Prose.ParagraphLength}");
            if (styleGuide.Prose.PreferredWords?.Any() == true)
                sb.AppendLine($"Preferred vocabulary: {string.Join(", ", styleGuide.Prose.PreferredWords.Take(10))}");
            if (styleGuide.Prose.AvoidWords?.Any() == true)
                sb.AppendLine($"Avoid: {string.Join(", ", styleGuide.Prose.AvoidWords.Take(10))}");
            sb.AppendLine();
        }

        if (styleGuide.Dialogue != null)
        {
            sb.AppendLine($"Dialogue style: {styleGuide.Dialogue.TagStyle}");
            sb.AppendLine($"Dialogue percentage: ~{styleGuide.Dialogue.DialogueToNarrativeRatio}%");
            sb.AppendLine();
        }

        return Result<string>.Success(sb.ToString());
    }

    /// <inheritdoc />
    public Result<string> BuildChapterInstructions(
        ChapterBlueprint chapterBlueprint,
        BookBlueprint bookBlueprint)
    {
        var sb = new StringBuilder();
        sb.AppendLine("CHAPTER REQUIREMENTS:");
        sb.AppendLine();

        sb.AppendLine($"Chapter {chapterBlueprint.ChapterNumber}: {chapterBlueprint.Title}");
        sb.AppendLine($"Purpose: {chapterBlueprint.Purpose}");
        sb.AppendLine($"Target length: ~{chapterBlueprint.TargetWordCount:N0} words");
        sb.AppendLine($"Pacing: {chapterBlueprint.PacingIntensity}");
        sb.AppendLine();

        // Opening
        if (!string.IsNullOrEmpty(chapterBlueprint.OpeningHook))
        {
            sb.AppendLine($"Opening: {chapterBlueprint.OpeningHook}");
        }

        // Scenes
        if (chapterBlueprint.Scenes?.Any() == true)
        {
            sb.AppendLine();
            sb.AppendLine("Scenes to include:");
            foreach (var scene in chapterBlueprint.Scenes)
            {
                sb.AppendLine($"  {scene.Order}. {scene.Title}: {scene.Purpose}");
                sb.AppendLine($"     Location: {scene.Location}, CharacterBible: {string.Join(", ", scene.Characters ?? new List<string>())}");
            }
        }

        // Must include
        if (chapterBlueprint.MustInclude?.Any() == true)
        {
            sb.AppendLine();
            sb.AppendLine("MUST include:");
            foreach (var item in chapterBlueprint.MustInclude)
            {
                sb.AppendLine($"  ✓ {item}");
            }
        }

        // Must avoid
        if (chapterBlueprint.MustAvoid?.Any() == true)
        {
            sb.AppendLine();
            sb.AppendLine("MUST avoid:");
            foreach (var item in chapterBlueprint.MustAvoid)
            {
                sb.AppendLine($"  ✗ {item}");
            }
        }

        // Ending
        if (!string.IsNullOrEmpty(chapterBlueprint.ClosingHook))
        {
            sb.AppendLine();
            sb.AppendLine($"End with: {chapterBlueprint.ClosingHook}");
        }

        // Emotional arc
        if (chapterBlueprint.EmotionalArc != null)
        {
            sb.AppendLine();
            sb.AppendLine($"Emotional journey: {chapterBlueprint.EmotionalArc.StartingEmotion} → {chapterBlueprint.EmotionalArc.EndingEmotion}");
        }

        return Result<string>.Success(sb.ToString());
    }

    /// <inheritdoc />
    public async Task<Result<string>> CompressContextAsync(
        string context,
        int maxTokens,
        CancellationToken cancellationToken = default)
    {
        var currentTokens = _aiService.EstimateTokens(context);
        if (currentTokens <= maxTokens)
        {
            return Result<string>.Success(context);
        }

        _logger.LogInformation("Compressing context from {Current} to {Max} tokens", 
            currentTokens, maxTokens);

        var prompt = $@"Compress the following context while preserving all critical information for story continuity.
Keep character names, key events, emotional states, and plot-relevant details.
Remove redundancy and verbose descriptions.

Context to compress:
{context}

Provide only the compressed context, nothing else.";

        try
        {
            var response = await _aiService.GenerateAsync(
                prompt,
                new CoreGenerationOptions { Temperature = 0.2, MaxTokens = maxTokens },
                cancellationToken);

            if (response.IsSuccess && !string.IsNullOrEmpty(response.Value))
            {
                return Result<string>.Success(response.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error compressing context, truncating instead");
        }

        // Fallback: simple truncation from middle
        var targetLength = maxTokens * 3; // Rough char estimate
        if (context.Length > targetLength)
        {
            var halfLength = targetLength / 2;
            context = context.Substring(0, halfLength) + 
                     "\n...\n" + 
                     context.Substring(context.Length - halfLength);
        }

        return Result<string>.Success(context);
    }

    /// <inheritdoc />
    public Result<TokenBudget> CalculateTokenBudget(int totalAvailable)
    {
        // Allocate tokens across different context components
        var budget = new TokenBudget
        {
            TotalAvailable = totalAvailable,
            SystemPrompt = (int)(totalAvailable * 0.10),      // 10%
            NarrativeContext = (int)(totalAvailable * 0.25), // 25%
            CharacterContext = (int)(totalAvailable * 0.15), // 15%
            WorldContext = (int)(totalAvailable * 0.10),     // 10%
            PlotContext = (int)(totalAvailable * 0.10),      // 10%
            StyleContext = (int)(totalAvailable * 0.05),     // 5%
            ChapterInstructions = (int)(totalAvailable * 0.15), // 15%
            Reserved = (int)(totalAvailable * 0.10)          // 10% buffer
        };

        return Result<TokenBudget>.Success(budget);
    }

    #region Private Helper Methods

    private static string GetLastParagraphs(string text, int count)
    {
        var paragraphs = text.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        var lastParagraphs = paragraphs.TakeLast(count);
        return string.Join("\n\n", lastParagraphs);
    }

    private static List<ChapterSummary> GetChapterSummaries(List<GeneratedChapter>? chapters)
    {
        if (chapters == null) return new List<ChapterSummary>();

        return chapters
            .OrderBy(c => c.ChapterNumber)
            .Select(c => new ChapterSummary
            {
                ChapterNumber = c.ChapterNumber,
                Title = c.Title ?? $"Chapter {c.ChapterNumber}",
                Summary = c.Summaries?.AISummary ?? c.Summaries?.BriefSummary ?? c.Summary ?? "",
                KeyEvents = c.Summaries?.KeyEvents ?? new List<string>(),
                WordCount = c.WordCount
            })
            .ToList();
    }

    private static List<string> GetChapterSummaryStrings(List<GeneratedChapter>? chapters)
    {
        if (chapters == null) return new List<string>();

        return chapters
            .OrderBy(c => c.ChapterNumber)
            .Select(c => c.Summaries?.AISummary ?? c.Summaries?.BriefSummary ?? c.Summary ?? $"Chapter {c.ChapterNumber}")
            .ToList();
    }

    private static List<CharacterStateSnapshot> ExtractLatestCharacterStates(
        List<GeneratedChapter>? chapters)
    {
        if (chapters == null || !chapters.Any())
            return new List<CharacterStateSnapshot>();

        var lastChapter = chapters.OrderByDescending(c => c.ChapterNumber).FirstOrDefault();
        return lastChapter?.CharacterStatesAtEnd ?? new List<CharacterStateSnapshot>();
    }

    private static List<SetupPayoff> GetActiveSetups(
        PlotArchitecture? plotArchitecture, 
        int currentChapter)
    {
        if (plotArchitecture?.SetupPayoffs == null)
            return new List<SetupPayoff>();

        return plotArchitecture.SetupPayoffs.Items
            .Where(sp => sp.SetupChapter < currentChapter && 
                        (sp.PayoffChapter == null || sp.PayoffChapter >= currentChapter))
            .ToList();
    }

    private static List<SetupPayoff> GetDuePayoffs(
        PlotArchitecture? plotArchitecture, 
        int currentChapter)
    {
        if (plotArchitecture?.SetupPayoffs == null)
            return new List<SetupPayoff>();

        return plotArchitecture.SetupPayoffs.Items
            .Where(sp => sp.PayoffChapter == currentChapter)
            .ToList();
    }

    #endregion
}

/// <summary>
/// Summary of a generated chapter for context building.
/// </summary>
public class ChapterSummary
{
    public int ChapterNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public List<string> KeyEvents { get; set; } = new();
    public int WordCount { get; set; }
}

// Additional interface implementations added for compatibility
public partial class GenerationContextBuilder
{
    /// <inheritdoc />
    public Task<Result<ChapterGenerationContext>> BuildChapterContextAsync(
        BookBlueprint blueprint,
        int chapterNumber,
        List<GeneratedChapter> previousChapters,
        ContextBuildingOptions options,
        CancellationToken cancellationToken = default)
    {
        // Delegate to existing BuildContextAsync method
        return BuildContextAsync(chapterNumber, blueprint, previousChapters, 
            new GenerationConfiguration(), cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<SceneGenerationContext>> BuildSceneContextAsync(
        ChapterGenerationContext chapterContext,
        int sceneNumber,
        List<GeneratedScene> previousScenes,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<SceneGenerationContext>.Success(new SceneGenerationContext
        {
            ChapterContext = chapterContext,
            SceneNumber = sceneNumber,
            PreviousSceneContent = previousScenes?.Select(s => s.Content ?? string.Empty).ToList() ?? new()
        }));
    }

    /// <inheritdoc />
    public Task<Result<string>> BuildCharacterContextAsync(
        List<Guid> characterIds,
        CharacterBible characterBible,
        List<CharacterStateSnapshot> currentStates,
        CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder();
        if (characterBible?.MainCharacters != null)
        {
            foreach (var character in characterBible.MainCharacters.Where(c => characterIds.Contains(c.Id)))
            {
                sb.AppendLine($"Character: {character.FullName}");
                sb.AppendLine($"Role: {character.Role}");
            }
        }
        return Task.FromResult(Result<string>.Success(sb.ToString()));
    }

    /// <inheritdoc />
    public Task<Result<string>> BuildLocationContextAsync(
        List<Guid> locationIds,
        WorldBible worldBible,
        CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder();
        if (worldBible?.Locations != null)
        {
            foreach (var location in worldBible.Locations.Where(l => locationIds.Contains(l.Id)))
            {
                sb.AppendLine($"Location: {location.Name}");
                sb.AppendLine($"Description: {location.Description}");
            }
        }
        return Task.FromResult(Result<string>.Success(sb.ToString()));
    }

    /// <inheritdoc />
    public Task<Result<string>> BuildPlotContextAsync(
        ChapterBlueprint chapterBlueprint,
        PlotArchitecture plotArchitecture,
        CancellationToken cancellationToken = default)
    {
        var context = $"Chapter Purpose: {chapterBlueprint.Purpose}";
        return Task.FromResult(Result<string>.Success(context));
    }

    /// <inheritdoc />
    public Task<Result<string>> BuildStyleContextAsync(
        StyleGuide styleGuide,
        ChapterTone chapterTone,
        CancellationToken cancellationToken = default)
    {
        var style = styleGuide?.Voice?.Description ?? "Standard narrative voice";
        return Task.FromResult(Result<string>.Success(style));
    }

    /// <inheritdoc />
    public int EstimateTokens(string content)
    {
        if (string.IsNullOrEmpty(content)) return 0;
        // Rough estimate: ~4 CharacterBible per token
        return content.Length / 4;
    }
}




