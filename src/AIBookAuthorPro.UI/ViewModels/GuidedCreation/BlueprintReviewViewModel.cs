// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Collections.ObjectModel;
using AIBookAuthorPro.Application.Services.GuidedCreation;
using AIBookAuthorPro.Core.Models.GuidedCreation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.UI.ViewModels.GuidedCreation;

/// <summary>
/// ViewModel for the blueprint review step of the guided creation wizard.
/// </summary>
public partial class BlueprintReviewViewModel : ObservableObject
{
    private readonly IBlueprintGeneratorService _blueprintGeneratorService;
    private readonly ILogger _logger;

    #region Observable Properties

    [ObservableProperty]
    private BookBlueprint? _blueprint;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isRegenerating;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private BlueprintSection _selectedSection = BlueprintSection.Identity;

    [ObservableProperty]
    private bool _isApproved;

    // Identity
    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _premise = string.Empty;

    [ObservableProperty]
    private string _logline = string.Empty;

    [ObservableProperty]
    private string _genre = string.Empty;

    [ObservableProperty]
    private int _targetWordCount;

    // Structure
    [ObservableProperty]
    private ObservableCollection<ActViewModel> _acts = new();

    [ObservableProperty]
    private int _totalChapters;

    [ObservableProperty]
    private string _pacingStrategy = string.Empty;

    // Characters
    [ObservableProperty]
    private ObservableCollection<CharacterSummaryViewModel> _characters = new();

    // World
    [ObservableProperty]
    private WorldSummaryViewModel? _worldSummary;

    // Plot
    [ObservableProperty]
    private PlotSummaryViewModel? _plotSummary;

    // Style
    [ObservableProperty]
    private StyleSummaryViewModel? _styleSummary;

    // Chapters
    [ObservableProperty]
    private ObservableCollection<ChapterBlueprintSummaryViewModel> _chapterSummaries = new();

    [ObservableProperty]
    private ChapterBlueprintSummaryViewModel? _selectedChapter;

    // Validation
    [ObservableProperty]
    private BlueprintValidationResult? _validationResult;

    [ObservableProperty]
    private bool _hasValidationIssues;

    [ObservableProperty]
    private ObservableCollection<string> _validationWarnings = new();

    #endregion

    #region Computed Properties

    public bool CanApprove => Blueprint != null && !HasValidationIssues;

    public IReadOnlyList<BlueprintSectionItem> SectionItems { get; } = new List<BlueprintSectionItem>
    {
        new(BlueprintSection.Identity, "Book Identity", "Book", "Title, premise, and core concept"),
        new(BlueprintSection.Structure, "Structure", "AccountTree", "Acts, chapters, and pacing"),
        new(BlueprintSection.Characters, "Characters", "People", "Main and supporting cast"),
        new(BlueprintSection.World, "World", "Public", "Settings and world-building"),
        new(BlueprintSection.Plot, "Plot", "Timeline", "Plot points and story beats"),
        new(BlueprintSection.Style, "Style", "Brush", "Voice, tone, and writing style")
    };

    #endregion

    public BlueprintReviewViewModel(
        IBlueprintGeneratorService blueprintGeneratorService,
        ILogger logger)
    {
        _blueprintGeneratorService = blueprintGeneratorService ?? throw new ArgumentNullException(nameof(blueprintGeneratorService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Commands

    [RelayCommand]
    private void SelectSection(BlueprintSection section)
    {
        SelectedSection = section;
    }

    [RelayCommand]
    private async Task RegenerateSectionAsync(BlueprintSection section, CancellationToken cancellationToken)
    {
        if (Blueprint == null) return;

        IsRegenerating = true;
        StatusMessage = $"Regenerating {section}...";

        try
        {
            var result = await _blueprintGeneratorService.RegenerateSectionAsync(
                Blueprint, section, null, cancellationToken);

            if (result.IsSuccess)
            {
                LoadBlueprint(result.Value!);
                StatusMessage = $"{section} regenerated successfully";
            }
            else
            {
                StatusMessage = $"Failed to regenerate: {result.Error}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error regenerating section {Section}", section);
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsRegenerating = false;
        }
    }

    [RelayCommand]
    private async Task ValidateBlueprintAsync(CancellationToken cancellationToken)
    {
        if (Blueprint == null) return;

        StatusMessage = "Validating blueprint...";

        try
        {
            var result = await _blueprintGeneratorService.ValidateBlueprintAsync(
                Blueprint, cancellationToken);

            if (result.IsSuccess)
            {
                ValidationResult = result.Value;
                HasValidationIssues = !result.Value!.IsValid;

                ValidationWarnings.Clear();
                foreach (var warning in result.Value.Warnings)
                    ValidationWarnings.Add(warning);

                StatusMessage = result.Value.IsValid 
                    ? "Blueprint is valid and ready for approval" 
                    : "Blueprint has issues that need attention";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating blueprint");
            StatusMessage = $"Validation error: {ex.Message}";
        }
    }

    [RelayCommand(CanExecute = nameof(CanApprove))]
    private void ApproveBlueprint()
    {
        IsApproved = true;
        StatusMessage = "Blueprint approved! Ready for generation.";
    }

    [RelayCommand]
    private void SelectChapter(ChapterBlueprintSummaryViewModel chapter)
    {
        SelectedChapter = chapter;
    }

    [RelayCommand]
    private void EditTitle()
    {
        // Open title edit dialog
    }

    [RelayCommand]
    private void EditPremise()
    {
        // Open premise edit dialog
    }

    #endregion

    #region Public Methods

    public void LoadBlueprint(BookBlueprint blueprint)
    {
        Blueprint = blueprint;

        // Load identity
        Title = blueprint.Identity?.Title ?? "Untitled";
        Premise = blueprint.Identity?.Premise ?? "";
        Logline = blueprint.Identity?.Logline ?? "";
        Genre = blueprint.Identity?.Genre ?? "Fiction";
        TargetWordCount = blueprint.Identity?.TargetWordCount ?? 80000;

        // Load structure
        LoadStructure(blueprint.Structure);

        // Load characters
        LoadCharacters(blueprint.Characters);

        // Load world
        LoadWorld(blueprint.World);

        // Load plot
        LoadPlot(blueprint.Plot);

        // Load style
        LoadStyle(blueprint.Style);

        // Load chapter summaries
        LoadChapterSummaries(blueprint.ChapterBlueprints);

        IsApproved = false;
        OnPropertyChanged(nameof(CanApprove));
    }

    #endregion

    #region Private Methods

    private void LoadStructure(StructuralPlan? structure)
    {
        Acts.Clear();
        
        if (structure?.Acts == null)
        {
            TotalChapters = 0;
            PacingStrategy = "Not defined";
            return;
        }

        foreach (var act in structure.Acts)
        {
            Acts.Add(new ActViewModel
            {
                ActNumber = act.ActNumber,
                Name = act.Name,
                Purpose = act.Purpose ?? "",
                PercentageOfBook = act.PercentageOfBook,
                ChapterCount = act.Chapters?.Count ?? 0
            });
        }

        TotalChapters = structure.Acts.Sum(a => a.Chapters?.Count ?? 0);
        PacingStrategy = structure.PacingMap?.Description ?? "Standard pacing";
    }

    private void LoadCharacters(CharacterBible? characters)
    {
        Characters.Clear();

        if (characters?.MainCharacters == null) return;

        foreach (var character in characters.MainCharacters)
        {
            Characters.Add(new CharacterSummaryViewModel
            {
                Id = character.Id,
                Name = character.FullName ?? "Unknown",
                Role = character.Role.ToString(),
                Archetype = character.Archetype ?? "",
                Concept = character.Concept ?? "",
                ArcType = character.Arc?.ArcType ?? ""
            });
        }

        if (characters.SupportingCharacters != null)
        {
            foreach (var character in characters.SupportingCharacters)
            {
                Characters.Add(new CharacterSummaryViewModel
                {
                    Id = character.Id,
                    Name = character.FullName ?? "Unknown",
                    Role = character.Role.ToString(),
                    Archetype = character.Archetype ?? "",
                    Concept = character.Concept ?? "",
                    ArcType = character.Arc?.ArcType ?? "",
                    IsSupporting = true
                });
            }
        }
    }

    private void LoadWorld(WorldBible? world)
    {
        if (world == null)
        {
            WorldSummary = null;
            return;
        }

        WorldSummary = new WorldSummaryViewModel
        {
            WorldName = world.Overview?.Name ?? "Unknown",
            WorldType = world.Overview?.WorldType ?? "Unknown",
            Description = world.Overview?.Description ?? "",
            Era = world.TimePeriod?.Era ?? "Unknown",
            LocationCount = world.Locations?.Count ?? 0,
            HasMagicSystem = world.MagicSystem != null,
            Atmosphere = world.Overview?.Atmosphere ?? ""
        };
    }

    private void LoadPlot(PlotArchitecture? plot)
    {
        if (plot == null)
        {
            PlotSummary = null;
            return;
        }

        PlotSummary = new PlotSummaryViewModel
        {
            PlotType = plot.MainPlot?.PlotType ?? "Unknown",
            CentralConflict = plot.MainPlot?.CentralConflict ?? "",
            Stakes = plot.MainPlot?.Stakes ?? "",
            DramaticQuestion = plot.MainPlot?.DramaticQuestion ?? "",
            SubplotCount = plot.Subplots?.Count ?? 0,
            CentralTheme = plot.ThematicStructure?.CentralTheme ?? "",
            ThemeStatement = plot.ThematicStructure?.ThemeStatement ?? ""
        };
    }

    private void LoadStyle(StyleGuide? style)
    {
        if (style == null)
        {
            StyleSummary = null;
            return;
        }

        StyleSummary = new StyleSummaryViewModel
        {
            VoiceDescription = style.Voice?.Description ?? "",
            NarrativeDistance = style.Voice?.NarrativeDistance ?? "Medium",
            SentenceLength = style.Prose?.SentenceLength ?? "Varied",
            DialogueRatio = style.Dialogue?.DialogueRatio ?? "40-50%",
            ToneKeywords = style.Voice?.ToneKeywords != null 
                ? string.Join(", ", style.Voice.ToneKeywords) 
                : "",
            PrimaryGenre = style.GenreConventions?.PrimaryGenre ?? "Fiction"
        };
    }

    private void LoadChapterSummaries(List<ChapterBlueprint>? chapters)
    {
        ChapterSummaries.Clear();

        if (chapters == null) return;

        foreach (var chapter in chapters)
        {
            ChapterSummaries.Add(new ChapterBlueprintSummaryViewModel
            {
                ChapterNumber = chapter.ChapterNumber,
                Title = chapter.Title ?? $"Chapter {chapter.ChapterNumber}",
                Purpose = chapter.Purpose ?? "",
                TargetWordCount = chapter.TargetWordCount,
                SceneCount = chapter.Scenes?.Count ?? 0,
                Pov = chapter.Pov ?? "Third Person",
                Tone = chapter.Tone.ToString()
            });
        }
    }

    #endregion
}

#region Supporting ViewModels

public sealed record BlueprintSectionItem(
    BlueprintSection Section,
    string DisplayName,
    string Icon,
    string Description);

public partial class ActViewModel : ObservableObject
{
    [ObservableProperty]
    private int _actNumber;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _purpose = string.Empty;

    [ObservableProperty]
    private int _percentageOfBook;

    [ObservableProperty]
    private int _chapterCount;
}

public partial class CharacterSummaryViewModel : ObservableObject
{
    public Guid Id { get; init; }

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _role = string.Empty;

    [ObservableProperty]
    private string _archetype = string.Empty;

    [ObservableProperty]
    private string _concept = string.Empty;

    [ObservableProperty]
    private string _arcType = string.Empty;

    [ObservableProperty]
    private bool _isSupporting;
}

public partial class WorldSummaryViewModel : ObservableObject
{
    [ObservableProperty]
    private string _worldName = string.Empty;

    [ObservableProperty]
    private string _worldType = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _era = string.Empty;

    [ObservableProperty]
    private int _locationCount;

    [ObservableProperty]
    private bool _hasMagicSystem;

    [ObservableProperty]
    private string _atmosphere = string.Empty;
}

public partial class PlotSummaryViewModel : ObservableObject
{
    [ObservableProperty]
    private string _plotType = string.Empty;

    [ObservableProperty]
    private string _centralConflict = string.Empty;

    [ObservableProperty]
    private string _stakes = string.Empty;

    [ObservableProperty]
    private string _dramaticQuestion = string.Empty;

    [ObservableProperty]
    private int _subplotCount;

    [ObservableProperty]
    private string _centralTheme = string.Empty;

    [ObservableProperty]
    private string _themeStatement = string.Empty;
}

public partial class StyleSummaryViewModel : ObservableObject
{
    [ObservableProperty]
    private string _voiceDescription = string.Empty;

    [ObservableProperty]
    private string _narrativeDistance = string.Empty;

    [ObservableProperty]
    private string _sentenceLength = string.Empty;

    [ObservableProperty]
    private string _dialogueRatio = string.Empty;

    [ObservableProperty]
    private string _toneKeywords = string.Empty;

    [ObservableProperty]
    private string _primaryGenre = string.Empty;
}

public partial class ChapterBlueprintSummaryViewModel : ObservableObject
{
    [ObservableProperty]
    private int _chapterNumber;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _purpose = string.Empty;

    [ObservableProperty]
    private int _targetWordCount;

    [ObservableProperty]
    private int _sceneCount;

    [ObservableProperty]
    private string _pov = string.Empty;

    [ObservableProperty]
    private string _tone = string.Empty;
}

#endregion
