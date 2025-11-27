// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Text;
using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Enums;
using AIBookAuthorPro.Core.Interfaces;
using AIBookAuthorPro.Core.Models;
using AIBookAuthorPro.Core.Models.AI;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.Infrastructure.Services;

/// <summary>
/// Service for building AI generation context from project data.
/// </summary>
public sealed class ContextBuilderService : IContextBuilderService
{
    private readonly ILogger<ContextBuilderService> _logger;

    // Token estimation: ~4 chars per token for English text
    private const double CharsPerToken = 4.0;

    /// <summary>
    /// Initializes a new instance of the ContextBuilderService.
    /// </summary>
    public ContextBuilderService(ILogger<ContextBuilderService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Result<GenerationContext> BuildChapterContext(
        Project project,
        Chapter chapter,
        ContextBuildOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(chapter);

        options ??= new ContextBuildOptions();

        try
        {
            _logger.LogDebug("Building context for chapter {ChapterNumber}: {Title}",
                chapter.Order, chapter.Title);

            var context = new GenerationContext
            {
                ProjectId = project.Id,
                ChapterId = chapter.Id,
                ChapterNumber = chapter.Order,
                BookTitle = project.Metadata.Title,
                Genre = project.Metadata.Genre,
                TargetAudience = project.Metadata.TargetAudience,
                Style = project.Metadata.WritingStyle,
                PointOfView = project.Metadata.PointOfView,
                Tense = project.Metadata.Tense,
                ChapterOutline = chapter.Outline,
                TargetWordCount = chapter.TargetWordCount > 0 ? chapter.TargetWordCount : 3000,
                CustomNotes = chapter.Notes
            };

            // Build previous chapters summary
            if (options.IncludePreviousSummary && chapter.Order > 1)
            {
                var summaryResult = BuildPreviousChaptersSummary(
                    project,
                    chapter.Order - 1,
                    options.MaxPreviousSummaryTokens);

                if (summaryResult.IsSuccess)
                {
                    context.PreviousSummary = summaryResult.Value;
                }
                else
                {
                    _logger.LogWarning("Failed to build previous summary: {Error}", summaryResult.Error);
                }
            }

            // Build character context
            if (options.IncludeCharacters)
            {
                var characterIds = options.CustomCharacterIds??
                    GetRelevantCharacterIds(project, chapter);

                var charResult = BuildCharacterContext(
                    project,
                    characterIds,
                    options.MaxCharacterTokens);

                if (charResult.IsSuccess && charResult.Value != null)
                {
                    context.CharacterContexts = [.. charResult.Value];
                }
            }

            // Build location context
            if (options.IncludeLocations)
            {
                var locationIds = options.CustomLocationIds??
                    GetRelevantLocationIds(project, chapter);

                var locResult = BuildLocationContext(
                    project,
                    locationIds,
                    options.MaxLocationTokens);

                if (locResult.IsSuccess && locResult.Value != null)
                {
                    context.LocationContexts = [.. locResult.Value];
                }
            }

            // Optimize if needed
            if (options.MaxTotalContextTokens > 0)
            {
                var currentTokens = EstimateContextTokens(context);
                if (currentTokens > options.MaxTotalContextTokens)
                {
                    _logger.LogDebug("Context exceeds budget ({Current} > {Max}), optimizing",
                        currentTokens, options.MaxTotalContextTokens);

                    var optimizedResult = OptimizeForTokenBudget(context, options.MaxTotalContextTokens);
                    if (optimizedResult.IsSuccess && optimizedResult.Value != null)
                    {
                        context = optimizedResult.Value;
                    }
                }
            }

            _logger.LogInformation("Built context for chapter {ChapterNumber} ({TokenEstimate} estimated tokens)",
                chapter.Order, EstimateContextTokens(context));

            return Result<GenerationContext>.Success(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build chapter context");
            return Result<GenerationContext>.Failure($"Failed to build context: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public string BuildSystemPrompt(GenerationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var sb = new StringBuilder();

        sb.AppendLine("You are an expert fiction writer with decades of experience crafting compelling narratives.");
        sb.AppendLine();
        sb.AppendLine("Your task is to write a chapter for a novel with the following specifications:");
        sb.AppendLine();

        // Book metadata
        sb.AppendLine("## BOOK INFORMATION");
        sb.AppendLine($"- Title: {context.BookTitle}");
        sb.AppendLine($"- Genre: {context.Genre}");

        if (!string.IsNullOrEmpty(context.TargetAudience))
            sb.AppendLine($"- Target Audience: {context.TargetAudience}");

        sb.AppendLine($"- Point of View: {FormatPov(context.PointOfView)}");
        sb.AppendLine($"- Tense: {FormatTense(context.Tense)}");

        if (!string.IsNullOrEmpty(context.Style))
            sb.AppendLine($"- Writing Style: {context.Style}");

        sb.AppendLine();

        // Chapter info
        sb.AppendLine($"## CHAPTER {context.ChapterNumber}");
        sb.AppendLine($"Target word count: approximately {context.TargetWordCount} words");
        sb.AppendLine();

        // Character context
        if (context.CharacterContexts.Count > 0)
        {
            sb.AppendLine("## CHARACTERS");
            foreach (var charContext in context.CharacterContexts)
            {
                sb.AppendLine(charContext);
                sb.AppendLine();
            }
        }

        // Location context
        if (context.LocationContexts.Count > 0)
        {
            sb.AppendLine("## LOCATIONS");
            foreach (var locContext in context.LocationContexts)
            {
                sb.AppendLine(locContext);
                sb.AppendLine();
            }
        }

        // Previous summary
        if (!string.IsNullOrEmpty(context.PreviousSummary))
        {
            sb.AppendLine("## STORY SO FAR");
            sb.AppendLine(context.PreviousSummary);
            sb.AppendLine();
        }

        // Writing guidelines
        sb.AppendLine("## WRITING GUIDELINES");
        sb.AppendLine("1. Write vivid, sensory prose that immerses the reader");
        sb.AppendLine("2. Balance dialogue, action, and description appropriately");
        sb.AppendLine("3. Maintain consistent character voices and personalities");
        sb.AppendLine("4. Show don't tell - use actions and details to convey emotion");
        sb.AppendLine("5. End the chapter with appropriate tension or closure based on story position");
        sb.AppendLine("6. Use scene breaks (marked with ***) when shifting time or location significantly");
        sb.AppendLine();

        return sb.ToString();
    }

    /// <inheritdoc />
    public string BuildUserPrompt(GenerationContext context, string? additionalInstructions = null)
    {
        ArgumentNullException.ThrowIfNull(context);

        var sb = new StringBuilder();

        sb.AppendLine($"Write Chapter {context.ChapterNumber} of \"{context.BookTitle}\".");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(context.ChapterOutline))
        {
            sb.AppendLine("## CHAPTER OUTLINE/BEATS");
            sb.AppendLine(context.ChapterOutline);
            sb.AppendLine();
        }

        if (!string.IsNullOrEmpty(context.CustomNotes))
        {
            sb.AppendLine("## AUTHOR NOTES");
            sb.AppendLine(context.CustomNotes);
            sb.AppendLine();
        }

        if (!string.IsNullOrEmpty(additionalInstructions))
        {
            sb.AppendLine("## ADDITIONAL INSTRUCTIONS");
            sb.AppendLine(additionalInstructions);
            sb.AppendLine();
        }

        sb.AppendLine($"Write approximately {context.TargetWordCount} words. Begin the chapter now:");

        return sb.ToString();
    }

    /// <inheritdoc />
    public Result<string> BuildPreviousChaptersSummary(
        Project project,
        int upToChapterNumber,
        int maxTokens = 2000)
    {
        ArgumentNullException.ThrowIfNull(project);

        try
        {
            var previousChapters = project.Chapters
                .Where(c => c.Order <= upToChapterNumber)
                .OrderBy(c => c.Order)
                .ToList();

            if (previousChapters.Count == 0)
                return Result<string>.Success(string.Empty);

            var sb = new StringBuilder();
            var maxChars = (int)(maxTokens * CharsPerToken);

            foreach (var chapter in previousChapters)
            {
                var summary = chapter.Summary;

                // If no summary, generate a brief one from content
                if (string.IsNullOrEmpty(summary) && !string.IsNullOrEmpty(chapter.Content))
                {
                    // Take first ~200 chars as a basic summary
                    var content = chapter.Content.Trim();
                    summary = content.Length > 200
                        ? content[..200] + "..."
                        : content;
                }

                if (!string.IsNullOrEmpty(summary))
                {
                    var chapterSummary = $"Chapter {chapter.Order} ({chapter.Title}): {summary}";

                    if (sb.Length + chapterSummary.Length > maxChars)
                    {
                        _logger.LogDebug("Truncating previous summary at chapter {ChapterNumber}", chapter.Order);
                        break;
                    }

                    sb.AppendLine(chapterSummary);
                }
            }

            return Result<string>.Success(sb.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build previous chapters summary");
            return Result<string>.Failure($"Failed to build summary: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public Result<IReadOnlyList<string>> BuildCharacterContext(
        Project project,
        IEnumerable<Guid> characterIds,
        int maxTokens = 1000)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(characterIds);

        try
        {
            var contexts = new List<string>();
            var maxChars = (int)(maxTokens * CharsPerToken);
            var totalChars = 0;

            foreach (var charId in characterIds)
            {
                var character = project.Characters.FirstOrDefault(c => c.Id == charId);
                if (character == null) continue;

                var charContext = BuildSingleCharacterContext(character);

                if (totalChars + charContext.Length > maxChars)
                {
                    _logger.LogDebug("Truncating character context at {CharacterName}", character.Name);
                    break;
                }

                contexts.Add(charContext);
                totalChars += charContext.Length;
            }

            return Result<IReadOnlyList<string>>.Success(contexts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build character context");
            return Result<IReadOnlyList<string>>.Failure($"Failed to build character context: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public Result<IReadOnlyList<string>> BuildLocationContext(
        Project project,
        IEnumerable<Guid> locationIds,
        int maxTokens = 500)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(locationIds);

        try
        {
            var contexts = new List<string>();
            var maxChars = (int)(maxTokens * CharsPerToken);
            var totalChars = 0;

            foreach (var locId in locationIds)
            {
                var location = project.Locations.FirstOrDefault(l => l.Id == locId);
                if (location == null) continue;

                var locContext = BuildSingleLocationContext(location);

                if (totalChars + locContext.Length > maxChars)
                {
                    _logger.LogDebug("Truncating location context at {LocationName}", location.Name);
                    break;
                }

                contexts.Add(locContext);
                totalChars += locContext.Length;
            }

            return Result<IReadOnlyList<string>>.Success(contexts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build location context");
            return Result<IReadOnlyList<string>>.Failure($"Failed to build location context: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public int EstimateContextTokens(GenerationContext context)
    {
        if (context == null) return 0;

        var totalChars = 0;

        totalChars += context.BookTitle?.Length ?? 0;
        totalChars += context.Genre?.Length ?? 0;
        totalChars += context.TargetAudience?.Length ?? 0;
        totalChars += context.Style?.Length ?? 0;
        totalChars += context.PreviousSummary?.Length ?? 0;
        totalChars += context.ChapterOutline?.Length ?? 0;
        totalChars += context.CustomNotes?.Length ?? 0;

        foreach (var charContext in context.CharacterContexts)
        {
            totalChars += charContext.Length;
        }

        foreach (var locContext in context.LocationContexts)
        {
            totalChars += locContext.Length;
        }

        // Add overhead for system prompt template
        totalChars += 1500;

        return (int)Math.Ceiling(totalChars / CharsPerToken);
    }

    /// <inheritdoc />
    public Result<GenerationContext> OptimizeForTokenBudget(
        GenerationContext context,
        int maxTokens)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            var currentTokens = EstimateContextTokens(context);

            if (currentTokens <= maxTokens)
                return Result<GenerationContext>.Success(context);

            _logger.LogDebug("Optimizing context from {Current} to {Max} tokens",
                currentTokens, maxTokens);

            // Create a copy with potentially reduced context
            var optimized = new GenerationContext
            {
                ProjectId = context.ProjectId,
                ChapterId = context.ChapterId,
                ChapterNumber = context.ChapterNumber,
                BookTitle = context.BookTitle,
                Genre = context.Genre,
                TargetAudience = context.TargetAudience,
                Style = context.Style,
                PointOfView = context.PointOfView,
                Tense = context.Tense,
                ChapterOutline = context.ChapterOutline,
                CustomNotes = context.CustomNotes,
                TargetWordCount = context.TargetWordCount
            };

            currentTokens = EstimateContextTokens(optimized);

            // Add back what we can
            if (currentTokens < maxTokens && !string.IsNullOrEmpty(context.PreviousSummary))
            {
                var summaryTokens = (int)Math.Ceiling(context.PreviousSummary.Length / CharsPerToken);
                if (currentTokens + summaryTokens <= maxTokens)
                {
                    optimized.PreviousSummary = context.PreviousSummary;
                    currentTokens += summaryTokens;
                }
                else
                {
                    // Truncate summary
                    var allowedChars = (int)((maxTokens - currentTokens) * CharsPerToken * 0.8);
                    if (allowedChars > 200)
                    {
                        optimized.PreviousSummary = context.PreviousSummary[..allowedChars] + "...";
                        currentTokens = EstimateContextTokens(optimized);
                    }
                }
            }

            // Add characters one by one
            foreach (var charContext in context.CharacterContexts)
            {
                var charTokens = (int)Math.Ceiling(charContext.Length / CharsPerToken);
                if (currentTokens + charTokens <= maxTokens)
                {
                    optimized.CharacterContexts.Add(charContext);
                    currentTokens += charTokens;
                }
                else
                {
                    break;
                }
            }

            // Add locations one by one
            foreach (var locContext in context.LocationContexts)
            {
                var locTokens = (int)Math.Ceiling(locContext.Length / CharsPerToken);
                if (currentTokens + locTokens <= maxTokens)
                {
                    optimized.LocationContexts.Add(locContext);
                    currentTokens += locTokens;
                }
                else
                {
                    break;
                }
            }

            _logger.LogDebug("Optimized context to {Tokens} tokens", EstimateContextTokens(optimized));

            return Result<GenerationContext>.Success(optimized);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to optimize context");
            return Result<GenerationContext>.Failure($"Failed to optimize: {ex.Message}", ex);
        }
    }

    #region Private Methods

    private static IEnumerable<Guid> GetRelevantCharacterIds(Project project, Chapter chapter)
    {
        // Prioritize: POV character, then characters explicitly linked to chapter,
        // then main characters
        var ids = new HashSet<Guid>();

        if (chapter.PovCharacterId.HasValue)
            ids.Add(chapter.PovCharacterId.Value);

        foreach (var charId in chapter.CharacterIds)
            ids.Add(charId);

        // Add main characters if we have room
        var mainCharacters = project.Characters
            .Where(c => c.Role == CharacterRole.Protagonist || c.Role == CharacterRole.Antagonist)
            .Take(5);

        foreach (var character in mainCharacters)
            ids.Add(character.Id);

        return ids.Take(10);
    }

    private static IEnumerable<Guid> GetRelevantLocationIds(Project project, Chapter chapter)
    {
        var ids = new HashSet<Guid>();

        if (chapter.PrimaryLocationId.HasValue)
            ids.Add(chapter.PrimaryLocationId.Value);

        foreach (var locId in chapter.LocationIds)
            ids.Add(locId);

        return ids.Take(5);
    }

    private static string BuildSingleCharacterContext(Character character)
    {
        var sb = new StringBuilder();

        sb.Append($"**{character.Name}**");

        if (!string.IsNullOrEmpty(character.Role.ToString()))
            sb.Append($" ({character.Role})");

        sb.AppendLine();

        if (!string.IsNullOrEmpty(character.Description))
            sb.AppendLine($"Description: {character.Description}");

        if (character.Traits.Count > 0)
            sb.AppendLine($"Traits: {string.Join(", ", character.Traits.Take(5))}");

        if (!string.IsNullOrEmpty(character.Goals))
            sb.AppendLine($"Goals: {character.Goals}");

        if (!string.IsNullOrEmpty(character.Voice))
            sb.AppendLine($"Voice/Speech patterns: {character.Voice}");

        return sb.ToString();
    }

    private static string BuildSingleLocationContext(Location location)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"**{location.Name}**");

        if (!string.IsNullOrEmpty(location.Description))
            sb.AppendLine($"Description: {location.Description}");

        if (!string.IsNullOrEmpty(location.Atmosphere))
            sb.AppendLine($"Atmosphere: {location.Atmosphere}");

        return sb.ToString();
    }

    private static string FormatPov(PointOfView pov) => pov switch
    {
        PointOfView.FirstPerson => "First Person (I/We)",
        PointOfView.SecondPerson => "Second Person (You)",
        PointOfView.ThirdLimited => "Third Person Limited",
        PointOfView.ThirdOmniscient => "Third Person Omniscient",
        _ => pov.ToString()
    };

    private static string FormatTense(Tense tense) => tense switch
    {
        Tense.Past => "Past Tense",
        Tense.Present => "Present Tense",
        Tense.Future => "Future Tense",
        _ => tense.ToString()
    };

    #endregion
}
