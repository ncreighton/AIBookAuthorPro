// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Collections.ObjectModel;
using AIBookAuthorPro.Core.Enums;
using AIBookAuthorPro.Core.Interfaces;
using AIBookAuthorPro.Core.Models.Covers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace AIBookAuthorPro.UI.ViewModels;

/// <summary>
/// ViewModel for Book Cover generation and management.
/// </summary>
public partial class CoverViewModel : ObservableObject
{
    private readonly ICoverService _coverService;
    private readonly IProjectService _projectService;
    private readonly ILogger<CoverViewModel> _logger;

    #region Observable Properties

    [ObservableProperty]
    private Guid? _currentProjectId;

    [ObservableProperty]
    private BookCover? _activeCover;

    [ObservableProperty]
    private ObservableCollection<BookCover> _allCovers = new();

    [ObservableProperty]
    private BookCover? _selectedCover;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotGenerating))]
    [NotifyCanExecuteChangedFor(nameof(GenerateCoverCommand))]
    [NotifyCanExecuteChangedFor(nameof(GenerateVariationsCommand))]
    [NotifyCanExecuteChangedFor(nameof(GenerateFullCoverCommand))]
    [NotifyCanExecuteChangedFor(nameof(ExportForKDPCommand))]
    private bool _isGenerating;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _successMessage;

    [ObservableProperty]
    private double _generationProgress;

    [ObservableProperty]
    private string? _generationStatus;

    // Generation Request Properties
    [ObservableProperty]
    private string _coverTitle = string.Empty;

    [ObservableProperty]
    private string _coverAuthor = string.Empty;

    [ObservableProperty]
    private string _coverGenre = string.Empty;

    [ObservableProperty]
    private string? _coverDescription;

    [ObservableProperty]
    private CoverStyle _selectedStyle = CoverStyle.Photographic;

    [ObservableProperty]
    private string? _mood;

    [ObservableProperty]
    private string _includeElements = string.Empty;

    [ObservableProperty]
    private string _excludeElements = string.Empty;

    [ObservableProperty]
    private string? _additionalInstructions;

    [ObservableProperty]
    private ImageGenerationProvider _selectedProvider = ImageGenerationProvider.DallE;

    [ObservableProperty]
    private int _variationCount = 4;

    // Color Scheme
    [ObservableProperty]
    private string _primaryColor = "#1976D2";

    [ObservableProperty]
    private string _secondaryColor = "#FF6F00";

    [ObservableProperty]
    private string _backgroundColor = "#FFFFFF";

    [ObservableProperty]
    private string _textColor = "#000000";

    // Generated Variations
    [ObservableProperty]
    private ObservableCollection<CoverVariation> _generatedVariations = new();

    [ObservableProperty]
    private CoverVariation? _selectedVariation;

    // Templates
    [ObservableProperty]
    private ObservableCollection<CoverTemplate> _availableTemplates = new();

    [ObservableProperty]
    private CoverTemplate? _selectedTemplate;

    [ObservableProperty]
    private string? _templateGenreFilter;

    // Prompt Suggestions
    [ObservableProperty]
    private ObservableCollection<string> _promptSuggestions = new();

    [ObservableProperty]
    private string? _selectedPromptSuggestion;

    // Print Specifications
    [ObservableProperty]
    private int _pageCount = 200;

    [ObservableProperty]
    private TrimSize _selectedTrimSize = TrimSize.Size6x9;

    [ObservableProperty]
    private PaperType _selectedPaperType = PaperType.Cream;

    [ObservableProperty]
    private CoverSpecifications? _specifications;

    // Back Cover Content
    [ObservableProperty]
    private string _backCoverBlurb = string.Empty;

    [ObservableProperty]
    private string _authorBio = string.Empty;

    [ObservableProperty]
    private string? _authorPhotoPath;

    [ObservableProperty]
    private ObservableCollection<ReviewQuote> _reviewQuotes = new();

    [ObservableProperty]
    private bool _includeBarcode = true;

    [ObservableProperty]
    private string? _price;

    // Validation
    [ObservableProperty]
    private ObservableCollection<CoverValidationIssue> _validationIssues = new();

    [ObservableProperty]
    private BookFormatType _validationFormatType = BookFormatType.Ebook;

    // Available Providers
    [ObservableProperty]
    private ObservableCollection<ImageGenerationProvider> _availableProviders = new();

    #endregion

    #region Computed Properties

    public bool IsNotGenerating => !IsGenerating;

    public IReadOnlyList<CoverStyle> AvailableStyles { get; } = Enum.GetValues<CoverStyle>().ToList();
    public IReadOnlyList<TrimSize> AvailableTrimSizes { get; } = Enum.GetValues<TrimSize>().ToList();
    public IReadOnlyList<PaperType> AvailablePaperTypes { get; } = Enum.GetValues<PaperType>().ToList();
    public IReadOnlyList<BookFormatType> AvailableFormatTypes { get; } = Enum.GetValues<BookFormatType>().ToList();

    public bool HasActiveCover => ActiveCover != null;
    public bool HasSelectedVariation => SelectedVariation != null;

    #endregion

    public CoverViewModel(
        ICoverService coverService,
        IProjectService projectService,
        ILogger<CoverViewModel> logger)
    {
        _coverService = coverService ?? throw new ArgumentNullException(nameof(coverService));
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Commands

    [RelayCommand]
    private async Task LoadCoversAsync(CancellationToken cancellationToken)
    {
        if (CurrentProjectId == null) return;

        try
        {
            var result = await _coverService.GetAllCoversAsync(CurrentProjectId.Value, cancellationToken);
            if (result.IsSuccess)
            {
                AllCovers.Clear();
                foreach (var cover in result.Value!)
                {
                    AllCovers.Add(cover);
                }
                
                ActiveCover = result.Value.FirstOrDefault(c => c.IsActive);
                OnPropertyChanged(nameof(HasActiveCover));
            }

            // Load available providers
            var providers = _coverService.GetAvailableProviders();
            AvailableProviders.Clear();
            foreach (var provider in providers)
            {
                AvailableProviders.Add(provider);
            }

            // Load templates
            await LoadTemplatesAsync(cancellationToken);

            _logger.LogInformation("Loaded {Count} covers for project {ProjectId}", 
                AllCovers.Count, CurrentProjectId);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load covers: {ex.Message}";
            _logger.LogError(ex, "Error loading covers");
        }
    }

    [RelayCommand]
    private async Task LoadTemplatesAsync(CancellationToken cancellationToken)
    {
        try
        {
            CoverStyle? styleFilter = null;
            var result = await _coverService.GetTemplatesAsync(TemplateGenreFilter, styleFilter, cancellationToken);
            if (result.IsSuccess)
            {
                AvailableTemplates.Clear();
                foreach (var template in result.Value!)
                {
                    AvailableTemplates.Add(template);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading templates");
        }
    }

    [RelayCommand(CanExecute = nameof(CanGenerateCover))]
    private async Task GenerateCoverAsync(CancellationToken cancellationToken)
    {
        IsGenerating = true;
        GenerationProgress = 0;
        GenerationStatus = "Preparing cover generation...";
        ErrorMessage = null;

        try
        {
            _logger.LogDebug("Generating cover for '{Title}'", CoverTitle);

            var request = BuildGenerationRequest();
            GenerationStatus = $"Generating {VariationCount} cover variations...";

            var result = await _coverService.GenerateCoverAsync(request, cancellationToken);

            if (result.IsSuccess)
            {
                GeneratedVariations.Clear();
                foreach (var variation in result.Value!.Variations)
                {
                    GeneratedVariations.Add(variation);
                }

                if (GeneratedVariations.Count > 0)
                {
                    SelectedVariation = GeneratedVariations[0];
                }

                GenerationStatus = $"Generated {result.Value.Variations.Count} variations";
                SuccessMessage = $"Successfully generated {result.Value.Variations.Count} cover variations!";
                
                _logger.LogInformation("Generated {Count} cover variations in {Duration}ms",
                    result.Value.Variations.Count, result.Value.Duration.TotalMilliseconds);
            }
            else
            {
                ErrorMessage = result.Error;
                GenerationStatus = "Generation failed";
            }
        }
        catch (OperationCanceledException)
        {
            GenerationStatus = "Generation cancelled";
            _logger.LogInformation("Cover generation cancelled");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to generate cover: {ex.Message}";
            GenerationStatus = "Generation failed";
            _logger.LogError(ex, "Error generating cover");
        }
        finally
        {
            IsGenerating = false;
            GenerationProgress = 100;
        }
    }

    private bool CanGenerateCover() => !IsGenerating && !string.IsNullOrWhiteSpace(CoverTitle);

    [RelayCommand(CanExecute = nameof(IsNotGenerating))]
    private async Task GenerateVariationsAsync(CancellationToken cancellationToken)
    {
        if (SelectedCover == null) return;

        IsGenerating = true;
        GenerationStatus = "Generating additional variations...";

        try
        {
            var result = await _coverService.GenerateVariationsAsync(
                SelectedCover.Id, 
                VariationCount, 
                cancellationToken);

            if (result.IsSuccess)
            {
                foreach (var variation in result.Value!)
                {
                    GeneratedVariations.Add(variation);
                }
                SuccessMessage = $"Generated {result.Value.Count} additional variations!";
            }
            else
            {
                ErrorMessage = result.Error;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to generate variations: {ex.Message}";
            _logger.LogError(ex, "Error generating variations");
        }
        finally
        {
            IsGenerating = false;
        }
    }

    [RelayCommand]
    private async Task GeneratePromptSuggestionsAsync(CancellationToken cancellationToken)
    {
        if (CurrentProjectId == null) return;

        try
        {
            var result = await _coverService.GeneratePromptSuggestionsAsync(
                CurrentProjectId.Value,
                SelectedStyle,
                cancellationToken);

            if (result.IsSuccess)
            {
                PromptSuggestions.Clear();
                foreach (var prompt in result.Value!)
                {
                    PromptSuggestions.Add(prompt);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating prompt suggestions");
        }
    }

    [RelayCommand]
    private void UsePromptSuggestion(string? prompt)
    {
        if (string.IsNullOrEmpty(prompt)) return;
        AdditionalInstructions = prompt;
    }

    [RelayCommand]
    private async Task SaveVariationAsCoverAsync(CoverVariation? variation, CancellationToken cancellationToken)
    {
        if (variation == null || CurrentProjectId == null) return;

        try
        {
            var cover = new BookCover
            {
                Title = CoverTitle,
                Style = SelectedStyle,
                FrontCoverPath = variation.ImagePath,
                GenerationPrompt = variation.Prompt,
                GenerationProvider = SelectedProvider,
                IsActive = AllCovers.Count == 0 // Make active if first cover
            };

            var result = await _coverService.SaveCoverAsync(CurrentProjectId.Value, cover, cancellationToken);
            if (result.IsSuccess)
            {
                AllCovers.Add(cover);
                if (cover.IsActive)
                {
                    ActiveCover = cover;
                    OnPropertyChanged(nameof(HasActiveCover));
                }
                SuccessMessage = "Cover saved successfully!";
                _logger.LogInformation("Saved cover {CoverId} for project {ProjectId}", 
                    cover.Id, CurrentProjectId);
            }
            else
            {
                ErrorMessage = result.Error;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to save cover: {ex.Message}";
            _logger.LogError(ex, "Error saving cover");
        }
    }

    [RelayCommand]
    private async Task SetActiveAsync(BookCover? cover, CancellationToken cancellationToken)
    {
        if (cover == null || CurrentProjectId == null) return;

        try
        {
            var result = await _coverService.SetActiveCoverAsync(
                CurrentProjectId.Value, 
                cover.Id, 
                cancellationToken);

            if (result.IsSuccess)
            {
                foreach (var c in AllCovers)
                {
                    c.IsActive = c.Id == cover.Id;
                }
                ActiveCover = cover;
                OnPropertyChanged(nameof(HasActiveCover));
                SuccessMessage = "Active cover updated!";
            }
            else
            {
                ErrorMessage = result.Error;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to set active cover: {ex.Message}";
            _logger.LogError(ex, "Error setting active cover");
        }
    }

    [RelayCommand]
    private async Task DeleteCoverAsync(BookCover? cover, CancellationToken cancellationToken)
    {
        if (cover == null || CurrentProjectId == null) return;

        try
        {
            var result = await _coverService.DeleteCoverAsync(
                CurrentProjectId.Value, 
                cover.Id, 
                cancellationToken);

            if (result.IsSuccess)
            {
                AllCovers.Remove(cover);
                if (cover.IsActive)
                {
                    ActiveCover = AllCovers.FirstOrDefault();
                    if (ActiveCover != null)
                    {
                        await SetActiveAsync(ActiveCover, cancellationToken);
                    }
                }
                OnPropertyChanged(nameof(HasActiveCover));
                SuccessMessage = "Cover deleted!";
            }
            else
            {
                ErrorMessage = result.Error;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to delete cover: {ex.Message}";
            _logger.LogError(ex, "Error deleting cover");
        }
    }

    [RelayCommand]
    private async Task CreateFromTemplateAsync(CancellationToken cancellationToken)
    {
        if (SelectedTemplate == null || CurrentProjectId == null) return;

        try
        {
            // Add any customizations from UI
            CoverTextElements? customizations = null;
            // TODO: Build customizations from UI fields if needed

            var result = await _coverService.CreateFromTemplateAsync(
                CurrentProjectId.Value,
                SelectedTemplate.Id,
                customizations,
                cancellationToken);

            if (result.IsSuccess)
            {
                AllCovers.Add(result.Value!);
                SelectedCover = result.Value;
                SuccessMessage = "Cover created from template!";
            }
            else
            {
                ErrorMessage = result.Error;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to create from template: {ex.Message}";
            _logger.LogError(ex, "Error creating from template");
        }
    }

    [RelayCommand]
    private async Task CalculateSpecificationsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _coverService.CalculateSpecificationsAsync(
                PageCount,
                SelectedTrimSize,
                SelectedPaperType,
                cancellationToken);

            if (result.IsSuccess)
            {
                Specifications = result.Value;
                _logger.LogInformation("Calculated cover specifications: {Width}x{Height}",
                    Specifications?.FullCoverWidthPixels, Specifications?.FullCoverHeightPixels);
            }
            else
            {
                ErrorMessage = result.Error;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to calculate specifications: {ex.Message}";
            _logger.LogError(ex, "Error calculating specifications");
        }
    }

    [RelayCommand(CanExecute = nameof(CanGenerateFullCover))]
    private async Task GenerateFullCoverAsync(CancellationToken cancellationToken)
    {
        if (SelectedCover == null || Specifications == null) return;

        IsGenerating = true;
        GenerationStatus = "Generating full wrap cover...";

        try
        {
            var backContent = new BackCoverContent
            {
                Blurb = BackCoverBlurb,
                AuthorBio = AuthorBio,
                AuthorPhotoPath = AuthorPhotoPath,
                Reviews = ReviewQuotes.ToList(),
                IncludeBarcode = IncludeBarcode,
                Price = Price
            };

            var result = await _coverService.GenerateFullCoverAsync(
                SelectedCover.Id,
                Specifications,
                backContent,
                cancellationToken);

            if (result.IsSuccess)
            {
                SelectedCover.FullCoverPath = result.Value;
                SuccessMessage = "Full cover generated successfully!";
                _logger.LogInformation("Generated full cover at {Path}", result.Value);
            }
            else
            {
                ErrorMessage = result.Error;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to generate full cover: {ex.Message}";
            _logger.LogError(ex, "Error generating full cover");
        }
        finally
        {
            IsGenerating = false;
        }
    }

    private bool CanGenerateFullCover() => !IsGenerating && SelectedCover != null && Specifications != null;

    [RelayCommand(CanExecute = nameof(IsNotGenerating))]
    private async Task ExportForKDPAsync(CancellationToken cancellationToken)
    {
        if (SelectedCover == null) return;

        try
        {
            var dialog = new SaveFileDialog
            {
                Filter = ValidationFormatType == BookFormatType.Ebook 
                    ? "JPEG Image|*.jpg|PNG Image|*.png" 
                    : "PDF File|*.pdf",
                DefaultExt = ValidationFormatType == BookFormatType.Ebook ? ".jpg" : ".pdf",
                FileName = $"{CoverTitle}_cover"
            };

            if (dialog.ShowDialog() == true)
            {
                var result = await _coverService.ExportForKDPAsync(
                    SelectedCover.Id,
                    dialog.FileName,
                    ValidationFormatType,
                    cancellationToken);

                if (result.IsSuccess)
                {
                    SuccessMessage = $"Cover exported to {dialog.FileName}";
                }
                else
                {
                    ErrorMessage = result.Error;
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to export cover: {ex.Message}";
            _logger.LogError(ex, "Error exporting cover");
        }
    }

    [RelayCommand]
    private async Task ValidateCoverAsync(CancellationToken cancellationToken)
    {
        if (SelectedCover == null) return;

        try
        {
            var result = await _coverService.ValidateCoverAsync(
                SelectedCover.Id,
                ValidationFormatType,
                cancellationToken);

            if (result.IsSuccess)
            {
                ValidationIssues.Clear();
                foreach (var issue in result.Value!)
                {
                    ValidationIssues.Add(issue);
                }

                if (result.Value.Count == 0)
                {
                    SuccessMessage = "Cover passes all validation checks!";
                }
            }
            else
            {
                ErrorMessage = result.Error;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to validate cover: {ex.Message}";
            _logger.LogError(ex, "Error validating cover");
        }
    }

    [RelayCommand]
    private async Task ImportCoverAsync(CancellationToken cancellationToken)
    {
        if (CurrentProjectId == null) return;

        try
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.tiff;*.tif|All Files|*.*",
                Title = "Select Cover Image"
            };

            if (dialog.ShowDialog() == true)
            {
                var result = await _coverService.ImportCoverAsync(
                    CurrentProjectId.Value,
                    dialog.FileName,
                    CoverType.Front,
                    cancellationToken);

                if (result.IsSuccess)
                {
                    AllCovers.Add(result.Value!);
                    SelectedCover = result.Value;
                    SuccessMessage = "Cover imported successfully!";
                }
                else
                {
                    ErrorMessage = result.Error;
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to import cover: {ex.Message}";
            _logger.LogError(ex, "Error importing cover");
        }
    }

    [RelayCommand]
    private void AddReviewQuote()
    {
        ReviewQuotes.Add(new ReviewQuote());
    }

    [RelayCommand]
    private void RemoveReviewQuote(ReviewQuote? quote)
    {
        if (quote != null)
        {
            ReviewQuotes.Remove(quote);
        }
    }

    [RelayCommand]
    private void RateVariation(CoverVariation? variation)
    {
        // Rating would be set via binding
        if (variation != null)
        {
            _logger.LogDebug("Rated variation {Id}: {Rating}", variation.Id, variation.Rating);
        }
    }

    [RelayCommand]
    private void ClearMessages()
    {
        ErrorMessage = null;
        SuccessMessage = null;
    }

    #endregion

    #region Private Methods

    private CoverGenerationRequest BuildGenerationRequest()
    {
        var includeList = string.IsNullOrWhiteSpace(IncludeElements) 
            ? null 
            : IncludeElements.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        var excludeList = string.IsNullOrWhiteSpace(ExcludeElements)
            ? null
            : ExcludeElements.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        return new CoverGenerationRequest
        {
            Title = CoverTitle,
            Author = CoverAuthor,
            Genre = CoverGenre,
            Description = CoverDescription,
            Style = SelectedStyle,
            Mood = Mood,
            IncludeElements = includeList,
            ExcludeElements = excludeList,
            ColorScheme = new ColorScheme
            {
                PrimaryColor = PrimaryColor,
                SecondaryColor = SecondaryColor,
                BackgroundColor = BackgroundColor,
                TextColor = TextColor
            },
            Provider = SelectedProvider,
            VariationCount = VariationCount,
            Specifications = Specifications,
            AdditionalInstructions = AdditionalInstructions
        };
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Sets the current project context and loads data.
    /// </summary>
    public async Task SetProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        CurrentProjectId = projectId;

        // Load project data to populate defaults
        var projectResult = await _projectService.GetProjectAsync(projectId, cancellationToken);
        if (projectResult.IsSuccess && projectResult.Value != null)
        {
            var project = projectResult.Value;
            CoverTitle = project.Metadata.Title;
            CoverAuthor = project.Metadata.Author;
            CoverGenre = project.Metadata.Genre;
            CoverDescription = project.Description;
        }

        await LoadCoversAsync(cancellationToken);
    }

    #endregion
}
