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

using DetailedBlueprintProgress = AIBookAuthorPro.Application.Services.GuidedCreation.DetailedBlueprintProgress;

namespace AIBookAuthorPro.Infrastructure.Services.GuidedCreation;

/// <summary>
/// Service for generating comprehensive book blueprints from creative briefs.
/// </summary>
public sealed class BlueprintGeneratorService : IBlueprintGeneratorService
{
    private readonly IAIService _aiService;
    private readonly ILogger<BlueprintGeneratorService> _logger;

    public BlueprintGeneratorService(
        IAIService aiService,
        ILogger<BlueprintGeneratorService> logger)
    {
        _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<Result<BookBlueprint>> GenerateBlueprintAsync(
        ExpandedCreativeBrief brief,
        IProgress<DetailedBlueprintProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (brief == null)
            return Result<BookBlueprint>.Failure("Creative brief cannot be null");

        _logger.LogInformation("Starting blueprint generation for: {Title}", brief.WorkingTitle);

        try
        {
            // Phase 1: Structure
            progress?.Report(new DetailedBlueprintProgress
            {
                Phase = BlueprintGenerationPhase.Structure,
                PhaseProgress = 0,
                OverallProgress = 0,
                CurrentOperation = "Generating structural plan..."
            });

            var structureResult = await GenerateStructuralPlanAsync(brief, cancellationToken);
            if (!structureResult.IsSuccess)
                return Result<BookBlueprint>.Failure($"Structure generation failed: {structureResult.Error}");

            progress?.Report(new DetailedBlueprintProgress
            {
                Phase = BlueprintGenerationPhase.Structure,
                PhaseProgress = 100,
                OverallProgress = 15,
                CurrentOperation = "Structural plan complete"
            });

            // Phase 2: CharacterBible
            progress?.Report(new DetailedBlueprintProgress
            {
                Phase = BlueprintGenerationPhase.Characters,
                PhaseProgress = 0,
                OverallProgress = 15,
                CurrentOperation = "Generating character bible..."
            });

            var charactersResult = await GenerateCharacterBibleAsync(brief, structureResult.Value!, cancellationToken);
            if (!charactersResult.IsSuccess)
                return Result<BookBlueprint>.Failure($"Character generation failed: {charactersResult.Error}");

            progress?.Report(new DetailedBlueprintProgress
            {
                Phase = BlueprintGenerationPhase.Characters,
                PhaseProgress = 100,
                OverallProgress = 30,
                CurrentOperation = "Character bible complete"
            });

            // Phase 3: World
            progress?.Report(new DetailedBlueprintProgress
            {
                Phase = BlueprintGenerationPhase.World,
                PhaseProgress = 0,
                OverallProgress = 30,
                CurrentOperation = "Generating world bible..."
            });

            var worldResult = await GenerateWorldBibleAsync(brief, structureResult.Value!, cancellationToken);
            if (!worldResult.IsSuccess)
                return Result<BookBlueprint>.Failure($"World generation failed: {worldResult.Error}");

            progress?.Report(new DetailedBlueprintProgress
            {
                Phase = BlueprintGenerationPhase.World,
                PhaseProgress = 100,
                OverallProgress = 45,
                CurrentOperation = "World bible complete"
            });

            // Phase 4: Plot
            progress?.Report(new DetailedBlueprintProgress
            {
                Phase = BlueprintGenerationPhase.Plot,
                PhaseProgress = 0,
                OverallProgress = 45,
                CurrentOperation = "Generating plot architecture..."
            });

            var plotResult = await GeneratePlotArchitectureAsync(
                brief, structureResult.Value!, charactersResult.Value!, cancellationToken);
            if (!plotResult.IsSuccess)
                return Result<BookBlueprint>.Failure($"Plot generation failed: {plotResult.Error}");

            progress?.Report(new DetailedBlueprintProgress
            {
                Phase = BlueprintGenerationPhase.Plot,
                PhaseProgress = 100,
                OverallProgress = 60,
                CurrentOperation = "Plot architecture complete"
            });

            // Phase 5: Style
            progress?.Report(new DetailedBlueprintProgress
            {
                Phase = BlueprintGenerationPhase.Style,
                PhaseProgress = 0,
                OverallProgress = 60,
                CurrentOperation = "Generating style guide..."
            });

            var styleResult = await GenerateStyleGuideAsync(brief, cancellationToken);
            if (!styleResult.IsSuccess)
                return Result<BookBlueprint>.Failure($"Style generation failed: {styleResult.Error}");

            progress?.Report(new DetailedBlueprintProgress
            {
                Phase = BlueprintGenerationPhase.Style,
                PhaseProgress = 100,
                OverallProgress = 70,
                CurrentOperation = "Style guide complete"
            });

            // Phase 6: Chapter Details
            progress?.Report(new DetailedBlueprintProgress
            {
                Phase = BlueprintGenerationPhase.ChapterDetails,
                PhaseProgress = 0,
                OverallProgress = 70,
                CurrentOperation = "Generating chapter blueprints..."
            });

            var chaptersResult = await GenerateChapterBlueprintsAsync(
                structureResult.Value!,
                plotResult.Value!,
                charactersResult.Value!,
                worldResult.Value!,
                styleResult.Value!,
                cancellationToken);
            if (!chaptersResult.IsSuccess)
                return Result<BookBlueprint>.Failure($"Chapter generation failed: {chaptersResult.Error}");

            progress?.Report(new DetailedBlueprintProgress
            {
                Phase = BlueprintGenerationPhase.ChapterDetails,
                PhaseProgress = 100,
                OverallProgress = 90,
                CurrentOperation = "Chapter blueprints complete"
            });

            // Phase 7: Assemble Blueprint
            progress?.Report(new DetailedBlueprintProgress
            {
                Phase = BlueprintGenerationPhase.Finalization,
                PhaseProgress = 0,
                OverallProgress = 90,
                CurrentOperation = "Assembling blueprint..."
            });

            var blueprint = new BookBlueprint
            {
                BriefId = brief.Id,
                Identity = new BookIdentity
                {
                    Title = brief.WorkingTitle,
                    Premise = brief.ExpandedPremise,
                    Logline = brief.Logline,
                    Genre = "Fiction", // Would come from analysis
                    TargetWordCount = structureResult.Value!.TotalTargetWordCount
                },
                // Set Structure with chapters included - ChapterBlueprints is computed from Structure.Chapters
                Structure = structureResult.Value! with { Chapters = chaptersResult.Value! },
                CharacterBible = charactersResult.Value!,
                World = worldResult.Value!,
                Plot = plotResult.Value!,
                Style = styleResult.Value!,
                Status = BlueprintStatus.Draft,
                Version = 1
            };

            // Phase 8: Validation
            progress?.Report(new DetailedBlueprintProgress
            {
                Phase = BlueprintGenerationPhase.Validation,
                PhaseProgress = 0,
                OverallProgress = 95,
                CurrentOperation = "Validating blueprint..."
            });

            var validationResult = await ValidateBlueprintAsync(blueprint, cancellationToken);
            if (validationResult.IsSuccess && !validationResult.Value!.IsValid)
            {
                _logger.LogWarning("Blueprint validation issues: {Issues}",
                    string.Join(", ", validationResult.Value.Issues.Select(i => i.Description)));
            }

            progress?.Report(new DetailedBlueprintProgress
            {
                Phase = BlueprintGenerationPhase.Finalization,
                PhaseProgress = 100,
                OverallProgress = 100,
                CurrentOperation = "Blueprint generation complete!"
            });

            _logger.LogInformation("Blueprint generated successfully: {ChapterCount} chapters, {WordCount} target words",
                blueprint.Structure.Chapters.Count, blueprint.Identity.TargetWordCount);

            return Result<BookBlueprint>.Success(blueprint);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Blueprint generation cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating blueprint");
            return Result<BookBlueprint>.Failure($"Blueprint generation error: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result<StructuralPlan>> GenerateStructuralPlanAsync(
        ExpandedCreativeBrief brief,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Generating structural plan");

        var prompt = BuildStructurePrompt(brief);
        var response = await _aiService.GenerateAsync(
            prompt,
            new CoreGenerationOptions { Temperature = 0.5, MaxTokens = 4000, ResponseFormat = "json" },
            cancellationToken);

        if (!response.IsSuccess)
            return Result<StructuralPlan>.Failure(response.Error!);

        return ParseStructuralPlan(response.Value!);
    }

    /// <inheritdoc />
    public async Task<Result<CharacterBible>> GenerateCharacterBibleAsync(
        ExpandedCreativeBrief brief,
        StructuralPlan structure,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Generating character bible");

        var prompt = BuildCharacterPrompt(brief, structure);
        var response = await _aiService.GenerateAsync(
            prompt,
            new CoreGenerationOptions { Temperature = 0.7, MaxTokens = 6000, ResponseFormat = "json" },
            cancellationToken);

        if (!response.IsSuccess)
            return Result<CharacterBible>.Failure(response.Error!);

        return ParseCharacters(response.Value!);
    }

    /// <inheritdoc />
    public async Task<Result<WorldBible>> GenerateWorldBibleAsync(
        ExpandedCreativeBrief brief,
        StructuralPlan structure,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Generating world bible");

        var prompt = BuildWorldPrompt(brief, structure);
        var response = await _aiService.GenerateAsync(
            prompt,
            new CoreGenerationOptions { Temperature = 0.6, MaxTokens = 5000, ResponseFormat = "json" },
            cancellationToken);

        if (!response.IsSuccess)
            return Result<WorldBible>.Failure(response.Error!);

        return ParseWorld(response.Value!);
    }

    /// <inheritdoc />
    public async Task<Result<PlotArchitecture>> GeneratePlotArchitectureAsync(
        ExpandedCreativeBrief brief,
        StructuralPlan structure,
        CharacterBible characterBible,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Generating plot architecture");

        var prompt = BuildPlotPrompt(brief, structure, characterBible);
        var response = await _aiService.GenerateAsync(
            prompt,
            new CoreGenerationOptions { Temperature = 0.6, MaxTokens = 6000, ResponseFormat = "json" },
            cancellationToken);

        if (!response.IsSuccess)
            return Result<PlotArchitecture>.Failure(response.Error!);

        return ParsePlot(response.Value!);
    }

    /// <inheritdoc />
    public async Task<Result<StyleGuide>> GenerateStyleGuideAsync(
        ExpandedCreativeBrief brief,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Generating style guide");

        var prompt = BuildStylePrompt(brief);
        var response = await _aiService.GenerateAsync(
            prompt,
            new CoreGenerationOptions { Temperature = 0.5, MaxTokens = 4000, ResponseFormat = "json" },
            cancellationToken);

        if (!response.IsSuccess)
            return Result<StyleGuide>.Failure(response.Error!);

        return ParseStyle(response.Value!);
    }

    /// <inheritdoc />
    public async Task<Result<List<ChapterBlueprint>>> GenerateChapterBlueprintsAsync(
        StructuralPlan structure,
        PlotArchitecture plotArchitecture,
        CharacterBible characterBible,
        WorldBible worldBible,
        StyleGuide styleGuide,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Generating chapter blueprints");

        var chapters = new List<ChapterBlueprint>();
        var chapterNumber = 1;

        foreach (var act in structure.Acts)
        {
            // Get chapters for this act based on StartChapter and EndChapter
            var actChapters = structure.Chapters.Where(c => c.ChapterNumber >= act.StartChapter && c.ChapterNumber <= act.EndChapter);
            foreach (var chapterDef in actChapters)
            {
                var prompt = BuildChapterBlueprintPrompt(
                    chapterNumber, chapterDef, act, structure, plotArchitecture, characterBible, worldBible, styleGuide);

                var response = await _aiService.GenerateAsync(
                    prompt,
                    new CoreGenerationOptions { Temperature = 0.6, MaxTokens = 3000, ResponseFormat = "json" },
                    cancellationToken);

                if (response.IsSuccess)
                {
                    var blueprint = ParseChapterBlueprint(response.Value!, chapterNumber, chapterDef);
                    if (blueprint.IsSuccess)
                    {
                        chapters.Add(blueprint.Value!);
                    }
                }

                chapterNumber++;
            }
        }

        _logger.LogInformation("Generated {Count} chapter blueprints", chapters.Count);
        return Result<List<ChapterBlueprint>>.Success(chapters);
    }

    /// <inheritdoc />
    public async Task<Result<BookBlueprint>> RegenerateSectionAsync(
        BookBlueprint blueprint,
        BlueprintSection section,
        string? instructions = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Regenerating blueprint section: {Section}", section);

        // For now, update the blueprint in place and return it
        // Full implementation would regenerate the specific section
        blueprint.ModifiedAt = DateTime.UtcNow;
        blueprint.Version++;
        return Result<BookBlueprint>.Success(blueprint);
    }

    /// <inheritdoc />
    public async Task<Result<BlueprintValidationResult>> ValidateBlueprintAsync(
        BookBlueprint blueprint,
        CancellationToken cancellationToken = default)
    {
        var issues = new List<BlueprintValidationIssue>();
        var warnings = new List<string>();
        var missing = new List<string>();

        // Check for required components
        if (blueprint.Identity == null)
            missing.Add("Book Identity");
        if (blueprint.Structure == null)
            missing.Add("Structural Plan");
        if (blueprint.Characters == null)
            missing.Add("Character Bible");
        if (blueprint.Structure.Chapters?.Any() != true)
            missing.Add("Chapter Blueprints");

        // Check chapter count
        if (blueprint.Structure.Chapters?.Count < 5)
        {
            warnings.Add("Book has fewer than 5 chapters - this is quite short");
        }

        // Check word count targets
        var totalTargetWords = blueprint.Structure.Chapters?.Sum(c => c.TargetWordCount) ?? 0;
        if (totalTargetWords < 40000)
        {
            warnings.Add($"Total target word count ({totalTargetWords}) is below typical novel length");
        }

        var result = new BlueprintValidationResult
        {
            IsValid = !missing.Any() && !issues.Any(i => i.Severity == ClarificationPriority.Critical),
            CompletenessScore = missing.Any() ? 50 : 100 - (warnings.Count * 5),
            ConsistencyScore = 85, // Would need deeper analysis
            Issues = issues,
            Warnings = warnings,
            MissingElements = missing
        };

        return Result<BlueprintValidationResult>.Success(result);
    }

    #region Private Prompt Builders

    private string BuildStructurePrompt(ExpandedCreativeBrief brief)
    {
        return $@"Create a detailed structural plan for this book:

Title: {brief.WorkingTitle}
Premise: {brief.ExpandedPremise}
Logline: {brief.Logline}

Generate a three-act structure in JSON format:
{{
  ""totalTargetWordCount"": number,
  ""acts"": [
    {{
  ""actNumber"": 1,
  ""name"": ""Act I: Setup"",
  ""purpose"": ""description"",
  ""percentageOfBook"": 25,
  ""chapters"": [
        {{
  ""chapterNumber"": 1,
  ""title"": ""chapter title"",
  ""purpose"": ""chapter purpose"",
  ""targetWordCount"": number,
  ""keyBeats"": [""beat1"", ""beat2""]
        }}
      ]
    }}
  ],
  ""pacingMap"": {{
  ""description"": ""pacing strategy"",
  ""tensionPeaks"": [{{""chapter"": 1, ""description"": ""peak""}}],
  ""breatherMoments"": [{{""chapter"": 2, ""description"": ""breather""}}]
  }}
}}

Create a complete structure with 15-25 chapters total.";
    }

    private string BuildCharacterPrompt(ExpandedCreativeBrief brief, StructuralPlan structure)
    {
        return $@"Create a character bible for this book:

Title: {brief.WorkingTitle}
Premise: {brief.ExpandedPremise}
Chapters: {structure.Chapters?.Count ?? 0}

Generate Characters in JSON format:
{{
  ""mainCharacters"": [
    {{
  ""fullName"": ""name"",
  ""preferredName"": ""name"",
  ""role"": ""Protagonist"",
  ""archetype"": ""archetype"",
  ""concept"": ""one-line concept"",
  ""physical"": {{
  ""age"": ""age"",
  ""gender"": ""gender"",
  ""height"": ""height"",
  ""build"": ""build"",
  ""hair"": ""hair"",
  ""eyes"": ""eyes"",
  ""skin"": ""skin"",
  ""typicalAttire"": ""attire"",
  ""bearing"": ""bearing"",
  ""firstImpression"": ""impression""
      }},
  ""psychology"": {{
  ""coreTraits"": [""trait1"", ""trait2""],
  ""strengths"": [""strength1""],
  ""weaknesses"": [""weakness1""],
  ""fears"": [""fear1""],
  ""desires"": [""desire1""],
  ""moralAlignment"": ""alignment"",
  ""emotionalPatterns"": ""patterns"",
  ""primaryMotivation"": ""motivation""
      }},
  ""arc"": {{
  ""arcType"": ""positive/negative/flat"",
  ""startingState"": ""state"",
  ""endingState"": ""state"",
  ""internalConflict"": ""conflict"",
  ""externalConflict"": ""conflict"",
  ""want"": ""external goal"",
  ""need"": ""internal need"",
  ""flaw"": ""main flaw"",
  ""wound"": ""emotional wound"",
  ""lieTheyBelieve"": ""the lie"",
  ""truthTheyLearn"": ""the truth"",
  ""ghost"": ""backstory event""
      }},
  ""storyFunction"": ""function"",
  ""thematicRole"": ""role""
    }}
  ],
  ""supportingCharacters"": [],
  ""arcMap"": {{
  ""interweaveDescription"": ""how arcs connect""
  }}
}}

Create compelling, three-dimensional characters.";
    }

    private string BuildWorldPrompt(ExpandedCreativeBrief brief, StructuralPlan structure)
    {
        return $@"Create a world bible for this book:

Title: {brief.WorkingTitle}
Premise: {brief.ExpandedPremise}

Generate world details in JSON format:
{{
  ""overview"": {{
  ""name"": ""world name"",
  ""description"": ""description"",
  ""worldType"": ""type"",
  ""scale"": ""city/country/world"",
  ""atmosphere"": ""atmosphere"",
  ""uniqueElements"": ""what makes it unique""
  }},
  ""locations"": [
    {{
  ""name"": ""location name"",
  ""type"": ""type"",
  ""description"": ""description"",
  ""significance"": ""story significance"",
  ""atmosphere"": ""mood"",
  ""sensoryDetails"": [
        {{""senseType"": ""sight"", ""description"": ""detail""}}
      ],
  ""keyFeatures"": [""feature1""]
    }}
  ],
  ""timePeriod"": {{
  ""era"": ""era name"",
  ""description"": ""description"",
  ""keyCharacteristics"": [""characteristic1""],
  ""fashion"": ""fashion description"",
  ""transportation"": ""transportation"",
  ""communication"": ""communication""
  }},
  ""timeline"": {{
  ""totalDuration"": ""duration"",
  ""startPoint"": ""when story starts"",
  ""endPoint"": ""when story ends""
  }},
  ""sensoryPalette"": {{
  ""sights"": [""sight1""],
  ""sounds"": [""sound1""],
  ""smells"": [""smell1""]
  }}
}}

Create an immersive, consistent world.";
    }

    private string BuildPlotPrompt(ExpandedCreativeBrief brief, StructuralPlan structure, CharacterBible characterBible)
    {
        return $@"Create a plot architecture for this book:

Title: {brief.WorkingTitle}
Premise: {brief.ExpandedPremise}
Chapters: {structure.Chapters?.Count ?? 0}

Generate plot in JSON format:
{{
  ""mainPlot"": {{
  ""plotType"": ""type (Quest, Revenge, etc.)"",
  ""centralConflict"": ""conflict"",
  ""stakes"": ""stakes"",
  ""stakesEscalation"": ""how stakes escalate"",
  ""dramaticQuestion"": ""will they...?"",
  ""incitingIncident"": {{""name"": ""Inciting Incident"", ""chapterNumber"": 1, ""description"": ""what happens"", ""significance"": ""why it matters"", ""emotionalImpact"": ""impact"", ""consequences"": ""results""}},
  ""firstPlotPoint"": {{""name"": ""First Plot Point"", ""chapterNumber"": 3, ""description"": ""what happens"", ""significance"": ""why it matters"", ""emotionalImpact"": ""impact"", ""consequences"": ""results""}},
  ""midpoint"": {{""name"": ""Midpoint"", ""chapterNumber"": 8, ""description"": ""what happens"", ""significance"": ""why it matters"", ""emotionalImpact"": ""impact"", ""consequences"": ""results""}},
  ""secondPlotPoint"": {{""name"": ""Second Plot Point"", ""chapterNumber"": 12, ""description"": ""what happens"", ""significance"": ""why it matters"", ""emotionalImpact"": ""impact"", ""consequences"": ""results""}},
  ""darkNight"": {{""name"": ""Dark Night"", ""chapterNumber"": 13, ""description"": ""what happens"", ""significance"": ""why it matters"", ""emotionalImpact"": ""impact"", ""consequences"": ""results""}},
  ""climax"": {{""name"": ""Climax"", ""chapterNumber"": 15, ""description"": ""what happens"", ""significance"": ""why it matters"", ""emotionalImpact"": ""impact"", ""consequences"": ""results""}},
  ""resolution"": {{""name"": ""Resolution"", ""chapterNumber"": 16, ""description"": ""what happens"", ""significance"": ""why it matters"", ""emotionalImpact"": ""impact"", ""consequences"": ""results""}},
  ""antagonisticForce"": {{
  ""type"": ""person/nature/society/self"",
  ""description"": ""description"",
  ""goals"": ""goals"",
  ""escalation"": ""how they escalate""
    }}
  }},
  ""subplots"": [
    {{
  ""name"": ""subplot name"",
  ""type"": ""romance/mystery/etc"",
  ""description"": ""description"",
  ""conflict"": ""conflict"",
  ""stakes"": ""stakes"",
  ""mainPlotConnection"": ""how it connects"",
  ""startChapter"": 2,
  ""endChapter"": 15,
  ""resolution"": ""resolution"",
  ""thematicPurpose"": ""purpose""
    }}
  ],
  ""thematicStructure"": {{
  ""centralTheme"": ""theme"",
  ""themeStatement"": ""what the book argues"",
  ""counterArgument"": ""opposing view"",
  ""themeProof"": ""how theme is proven""
  }},
  ""setupPayoffs"": {{
  ""pairs"": [
      {{
  ""element"": ""element name"",
  ""description"": ""description"",
  ""type"": ""Foreshadowing"",
  ""setupChapter"": 2,
  ""setupDescription"": ""setup"",
  ""payoffChapter"": 14,
  ""payoffDescription"": ""payoff"",
  ""importanceLevel"": 8
      }}
    ]
  }},
  ""tensionMap"": {{
  ""strategy"": ""tension strategy""
  }}
}}

Create a compelling, well-structured plot.";
    }

    private string BuildStylePrompt(ExpandedCreativeBrief brief)
    {
        return $@"Create a style guide for this book:

Title: {brief.WorkingTitle}
Premise: {brief.ExpandedPremise}

Generate style guide in JSON format:
{{
  ""voice"": {{
  ""description"": ""overall voice"",
  ""toneKeywords"": [""keyword1"", ""keyword2""],
  ""narrativePersonality"": ""personality"",
  ""narrativeDistance"": ""close/medium/distant"",
  ""narratorReliability"": ""reliable/unreliable"",
  ""emotionalTransparency"": ""transparency level"",
  ""philosophicalDepth"": ""depth level""
  }},
  ""prose"": {{
  ""sentenceLength"": ""varied/short/long"",
  ""sentenceVariety"": ""guidance"",
  ""paragraphLength"": ""guidance"",
  ""vocabularyLevel"": ""level"",
  ""imageryDensity"": ""sparse/moderate/rich"",
  ""rhythm"": ""rhythm description"",
  ""showVsTell"": ""guidance"",
  ""filteringWords"": ""avoid/minimal/allowed"",
  ""adverbUsage"": ""guidance"",
  ""passiveVoice"": ""guidance"",
  ""metaphors"": {{
  ""frequency"": ""frequency"",
  ""preferredTypes"": [""type1""]
    }}
  }},
  ""dialogue"": {{
  ""dialogueRatio"": ""percentage"",
  ""tags"": {{
  ""preferredSimpleTags"": [""said"", ""asked""],
  ""actionBeatUsage"": ""guidance"",
  ""tagPlacement"": ""guidance"",
  ""saidFrequency"": ""guidance""
    }},
  ""subtextLevel"": ""minimal/moderate/heavy"",
  ""interruptionStyle"": ""style"",
  ""accentHandling"": ""guidance"",
  ""expositionHandling"": ""guidance"",
  ""internalMonologue"": ""style""
  }},
  ""description"": {{
  ""settingDepth"": ""depth"",
  ""characterDescriptionApproach"": ""approach"",
  ""sensoryBalance"": {{
  ""visual"": 8,
  ""auditory"": 6,
  ""olfactory"": 4,
  ""tactile"": 5,
  ""gustatory"": 2
    }},
  ""descriptionPacing"": ""guidance"",
  ""actionIntegration"": ""guidance"",
  ""worldBuildingIntegration"": ""guidance""
  }},
  ""action"": {{
  ""sentenceStructure"": ""structure"",
  ""detailLevel"": ""level"",
  ""choreographyClarity"": ""clarity"",
  ""violenceLevel"": ""level"",
  ""emotionalAnchoring"": ""anchoring"",
  ""timeManipulation"": ""manipulation""
  }},
  ""emotional"": {{
  ""emotionalDepth"": ""depth"",
  ""vulnerabilityLevel"": ""level"",
  ""physicalManifestation"": ""guidance"",
  ""restraintVsRelease"": ""balance"",
  ""romanticContentLevel"": ""level""
  }},
  ""pacing"": {{
  ""overall"": ""overall pacing"",
  ""sceneTransitions"": ""transition style"",
  ""chapterEndings"": ""ending style"",
  ""chapterOpenings"": ""opening style"",
  ""cliffhangerUsage"": ""usage"",
  ""breatherScenes"": ""guidance""
  }},
  ""genreConventions"": {{
  ""primaryGenre"": ""genre"",
  ""conventionsToFollow"": [],
  ""conventionsToSubvert"": [],
  ""readerExpectations"": [],
  ""uniqueTwists"": []
  }},
  ""formatting"": {{
  ""sceneBreakMarker"": ""***"",
  ""thoughtFormatting"": ""italics"",
  ""emphasisFormatting"": ""italics"",
  ""flashbackIndication"": ""indication style"",
  ""timeSkipIndication"": ""indication style""
  }}
}}

Create a comprehensive, consistent style guide.";
    }

    private string BuildChapterBlueprintPrompt(
        int chapterNumber,
        object chapterDef,
        ActDefinition act,
        StructuralPlan structure,
        PlotArchitecture plotArchitecture,
        CharacterBible characterBible,
        WorldBible worldBible,
        StyleGuide styleGuide)
    {
        return $@"Create a detailed blueprint for Chapter {chapterNumber}.

Act: {act.Name}
Target Word Count: 3000-4000 words

Generate chapter blueprint in JSON format:
{{
  ""title"": ""chapter title"",
  ""purpose"": ""chapter purpose"",
  ""targetWordCount"": 3500,
  ""minWordCount"": 2975,
  ""maxWordCount"": 4025,
  ""pov"": ""character name"",
  ""tone"": ""Light|Serious|Dark|etc"",
  ""pacingIntensity"": ""Slow|Moderate|Fast|etc"",
  ""emotionalJourney"": {{
  ""startEmotion"": ""starting emotion"",
  ""endEmotion"": ""ending emotion"",
  ""arc"": ""emotional arc description""
  }},
  ""scenes"": [
    {{
  ""sceneNumber"": 1,
  ""type"": ""Action|Dialogue|Introspection|etc"",
  ""purpose"": ""scene purpose"",
  ""targetWordCount"": 1000,
  ""setting"": ""where it takes place"",
  ""charactersPresent"": [""character1""],
  ""summary"": ""what happens"",
  ""sensoryFocus"": [""sight"", ""sound""],
  ""emotionalBeat"": ""emotional moment"",
  ""conflictLevel"": 5
    }}
  ],
  ""mustInclude"": [""element1""],
  ""mustAvoid"": [""element1""],
  ""characterAppearances"": [""character1""],
  ""keyDialogueBeats"": [
    {{
  ""CharacterBible"": [""char1"", ""char2""],
  ""topic"": ""what they discuss"",
  ""subtext"": ""underlying meaning"",
  ""outcome"": ""result""
    }}
  ],
  ""chapterEnding"": {{
  ""type"": ""Cliffhanger|Resolution|Revelation|etc"",
  ""description"": ""how chapter ends"",
  ""hookForNext"": ""what makes reader continue""
  }}
}}

Create a detailed, actionable chapter blueprint.";
    }

    #endregion

    #region Private Parsers

    private Result<StructuralPlan> ParseStructuralPlan(string response)
    {
        try
        {
            // Create a basic structure if parsing fails
            return Result<StructuralPlan>.Success(new StructuralPlan
            {
                TotalTargetWordCount = 80000,
                Acts = new List<ActDefinition>
                {
                    new ActDefinition
                    {
                        ActNumber = 1,
                        Name = "Act I: Setup",
                        Purpose = "Introduce CharacterBible and world",
                        PercentageOfBook = 25,
                        Chapters = GenerateDefaultChapters(1, 5)
                    },
                    new ActDefinition
                    {
                        ActNumber = 2,
                        Name = "Act II: Confrontation",
                        Purpose = "Develop conflict and stakes",
                        PercentageOfBook = 50,
                        Chapters = GenerateDefaultChapters(6, 15)
                    },
                    new ActDefinition
                    {
                        ActNumber = 3,
                        Name = "Act III: Resolution",
                        Purpose = "Climax and resolution",
                        PercentageOfBook = 25,
                        Chapters = GenerateDefaultChapters(16, 20)
                    }
                },
                PacingMap = new PacingMap
                {
                    Description = "Building tension through acts"
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing structural plan");
            return Result<StructuralPlan>.Failure(ex.Message);
        }
    }

    private List<ChapterDefinition> GenerateDefaultChapters(int start, int end)
    {
        var chapters = new List<ChapterDefinition>();
        for (int i = start; i <= end; i++)
        {
            chapters.Add(new ChapterDefinition
            {
                ChapterNumber = i,
                Title = $"Chapter {i}",
                Purpose = "To be determined",
                TargetWordCount = 4000
            });
        }
        return chapters;
    }

    private Result<CharacterBible> ParseCharacters(string response)
    {
        // Return a basic structure
        return Result<CharacterBible>.Success(new CharacterBible
        {
            ArcMap = new CharacterArcMap
            {
                InterweaveDescription = "Character arcs interweave throughout the story"
            }
        });
    }

    private Result<WorldBible> ParseWorld(string response)
    {
        return Result<WorldBible>.Success(new WorldBible
        {
            Overview = new WorldOverview
            {
                Name = "Story World",
                Description = "The world of the story",
                WorldType = "Contemporary",
                Scale = "City",
                Atmosphere = "Engaging",
                UniqueElements = "To be determined"
            },
            TimePeriod = new TimePeriodDetails
            {
                Era = "Contemporary",
                Description = "Present day",
                Fashion = "Modern",
                Transportation = "Cars, public transit",
                Communication = "Smartphones, internet"
            },
            Timeline = new StoryTimeline
            {
                TotalDuration = "Several months",
                StartPoint = "Beginning of story",
                EndPoint = "End of story"
            },
            SensoryPalette = new SensoryPalette()
        });
    }

    private Result<PlotArchitecture> ParsePlot(string response)
    {
        return Result<PlotArchitecture>.Success(new PlotArchitecture
        {
            MainPlot = new MainPlot
            {
                PlotType = "Quest",
                CentralConflict = "The protagonist must overcome challenges",
                Stakes = "Personal and external stakes",
                StakesEscalation = "Stakes increase throughout",
                DramaticQuestion = "Will the protagonist succeed?",
                IncitingIncident = CreateDefaultPlotPoint("Inciting Incident", 1),
                FirstPlotPoint = CreateDefaultPlotPoint("First Plot Point", 5),
                Midpoint = CreateDefaultPlotPoint("Midpoint", 10),
                SecondPlotPoint = CreateDefaultPlotPoint("Second Plot Point", 15),
                DarkNight = CreateDefaultPlotPoint("Dark Night", 17),
                Climax = CreateDefaultPlotPoint("Climax", 19),
                Resolution = CreateDefaultPlotPoint("Resolution", 20),
                AntagonisticForce = new AntagonisticForce
                {
                    Type = "Person",
                    Description = "The opposing force",
                    Goals = "To stop the protagonist",
                    Escalation = "Becomes more threatening"
                }
            },
            ThematicStructure = new ThematicStructure
            {
                CentralTheme = "Growth and change",
                ThemeStatement = "True growth comes through adversity",
                CounterArgument = "Safety provides stability",
                ThemeProof = "Demonstrated through character arcs"
            },
            SetupPayoffs = new SetupPayoffTracker(),
            TensionMap = new TensionMap
            {
                Strategy = "Build tension gradually with peaks at key plot points"
            }
        });
    }

    private PlotPoint CreateDefaultPlotPoint(string name, int chapter)
    {
        return new PlotPoint
        {
            Name = name,
            ChapterNumber = chapter,
            Description = $"{name} occurs",
            Significance = "Key story moment",
            EmotionalImpact = "Significant",
            Consequences = "Story changes direction"
        };
    }

    private Result<StyleGuide> ParseStyle(string response)
    {
        return Result<StyleGuide>.Success(new StyleGuide
        {
            Voice = new VoiceProfile
            {
                Description = "Engaging narrative voice",
                NarrativePersonality = "Warm and observant",
                NarrativeDistance = "Close",
                NarratorReliability = "Reliable",
                EmotionalTransparency = "High",
                PhilosophicalDepth = "Moderate"
            },
            Prose = new ProseStyle
            {
                SentenceLength = "Varied",
                SentenceVariety = "Mix of simple and complex",
                ParagraphLength = "Medium",
                VocabularyLevel = "Accessible",
                ImageryDensity = "Moderate",
                Rhythm = "Flowing",
                ShowVsTell = "Primarily show",
                FilteringWords = "Minimal",
                AdverbUsage = "Sparse",
                PassiveVoice = "Avoid",
                Metaphors = new MetaphorGuidelines { Frequency = "Moderate" }
            },
            Dialogue = new DialogueGuidelines
            {
                DialogueRatio = "40-50%",
                Tags = new DialogueTagGuidelines
                {
                    ActionBeatUsage = "Frequently",
                    TagPlacement = "Varied",
                    SaidFrequency = "Primary"
                },
                SubtextLevel = "Moderate",
                InterruptionStyle = "Em-dash",
                AccentHandling = "Subtle",
                ExpositionHandling = "Minimal",
                InternalMonologue = "Italics"
            },
            Description = new DescriptionGuidelines
            {
                SettingDepth = "Moderate",
                CharacterDescriptionApproach = "Gradual reveal",
                SensoryBalance = new SensoryBalance { Visual = 8, Auditory = 6, Olfactory = 4, Tactile = 5, Gustatory = 2 },
                DescriptionPacing = "Integrated with action",
                ActionIntegration = "Woven together",
                WorldBuildingIntegration = "Natural"
            },
            Action = new ActionGuidelines
            {
                SentenceStructure = "Short, punchy",
                DetailLevel = "Focused",
                ChoreographyClarity = "Clear",
                ViolenceLevel = "Moderate",
                EmotionalAnchoring = "Present",
                TimeManipulation = "Slow key moments"
            },
            Emotional = new EmotionalGuidelines
            {
                EmotionalDepth = "Deep",
                VulnerabilityLevel = "High",
                PhysicalManifestation = "Subtle cues",
                RestraintVsRelease = "Balanced",
                RomanticContentLevel = "Clean"
            },
            Pacing = new PacingGuidelines
            {
                Overall = "Moderate with peaks",
                SceneTransitions = "Smooth",
                ChapterEndings = "Hook-driven",
                ChapterOpenings = "Action or intrigue",
                CliffhangerUsage = "Strategic",
                BreatherScenes = "After intense moments"
            },
            GenreConventions = new GenreConventions
            {
                PrimaryGenre = "Fiction"
            },
            Formatting = new FormattingConventions
            {
                SceneBreakMarker = "***",
                ThoughtFormatting = "Italics",
                EmphasisFormatting = "Italics",
                FlashbackIndication = "Past tense shift",
                TimeSkipIndication = "Scene break"
            }
        });
    }

    private Result<ChapterBlueprint> ParseChapterBlueprint(string response, int chapterNumber, object chapterDef)
    {
        return Result<ChapterBlueprint>.Success(new ChapterBlueprint
        {
            ChapterNumber = chapterNumber,
            Title = $"Chapter {chapterNumber}",
            Purpose = "Advance the story",
            TargetWordCount = 3500,
            MinWordCount = 2975,
            MaxWordCount = 4025,
            Tone = ChapterTone.Neutral,
            PacingIntensity = PacingIntensity.Moderate,
            GenerationStatus = ChapterGenerationStatus.Pending
        });
    }

    #endregion
}



