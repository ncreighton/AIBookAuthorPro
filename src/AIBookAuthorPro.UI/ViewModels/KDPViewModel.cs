// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Collections.ObjectModel;
using AIBookAuthorPro.Core.Enums;
using AIBookAuthorPro.Core.Interfaces;
using AIBookAuthorPro.Core.Models.KDP;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.UI.ViewModels;

/// <summary>
/// ViewModel for KDP publishing features.
/// </summary>
public partial class KDPViewModel : ObservableObject
{
    private readonly IKDPService _kdpService;
    private readonly IProjectService _projectService;
    private readonly ILogger<KDPViewModel> _logger;

    #region Observable Properties

    [ObservableProperty]
    private Guid? _currentProjectId;

    [ObservableProperty]
    private KDPMetadata? _metadata;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    [NotifyCanExecuteChangedFor(nameof(SaveMetadataCommand))]
    [NotifyCanExecuteChangedFor(nameof(GenerateKeywordsCommand))]
    [NotifyCanExecuteChangedFor(nameof(GenerateDescriptionCommand))]
    [NotifyCanExecuteChangedFor(nameof(SuggestCategoriesCommand))]
    [NotifyCanExecuteChangedFor(nameof(CalculatePrintSpecsCommand))]
    [NotifyCanExecuteChangedFor(nameof(CalculateRoyaltiesCommand))]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _successMessage;

    // Category Search
    [ObservableProperty]
    private string _categorySearchQuery = string.Empty;

    [ObservableProperty]
    private ObservableCollection<KDPCategory> _categorySearchResults = new();

    [ObservableProperty]
    private KDPCategory? _selectedPrimaryCategory;

    [ObservableProperty]
    private KDPCategory? _selectedSecondaryCategory;

    [ObservableProperty]
    private ObservableCollection<KDPCategory> _suggestedCategories = new();

    // Keywords
    [ObservableProperty]
    private ObservableCollection<KeywordSuggestion> _keywordSuggestions = new();

    [ObservableProperty]
    private ObservableCollection<string> _selectedKeywords = new();

    [ObservableProperty]
    private string _newKeyword = string.Empty;

    // Description
    [ObservableProperty]
    private ObservableCollection<BookDescriptionSuggestion> _descriptionSuggestions = new();

    [ObservableProperty]
    private BookDescriptionSuggestion? _selectedDescription;

    [ObservableProperty]
    private string _currentDescription = string.Empty;

    [ObservableProperty]
    private int _descriptionCharacterCount;

    [ObservableProperty]
    private string? _descriptionStyle;

    // Print Specifications
    [ObservableProperty]
    private int _pageCount = 200;

    [ObservableProperty]
    private TrimSize _selectedTrimSize = TrimSize.Size6x9;

    [ObservableProperty]
    private PaperType _selectedPaperType = PaperType.Cream;

    [ObservableProperty]
    private bool _hasBleed;

    [ObservableProperty]
    private PrintSpecifications? _printSpecifications;

    // Pricing & Royalties
    [ObservableProperty]
    private decimal _listPrice = 14.99m;

    [ObservableProperty]
    private KDPMarketplace _selectedMarketplace = KDPMarketplace.US;

    [ObservableProperty]
    private BookFormatType _selectedFormatType = BookFormatType.Ebook;

    [ObservableProperty]
    private PricingInfo? _pricingInfo;

    // Series
    [ObservableProperty]
    private string _newSeriesName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<SeriesInfo> _allSeries = new();

    [ObservableProperty]
    private SeriesInfo? _selectedSeries;

    [ObservableProperty]
    private int _seriesPosition = 1;

    // A+ Content
    [ObservableProperty]
    private APlusContent? _aplusContent;

    [ObservableProperty]
    private ObservableCollection<APlusModuleType> _selectedModuleTypes = new()
    {
        APlusModuleType.StandardText,
        APlusModuleType.StandardImageText
    };

    // Validation
    [ObservableProperty]
    private ObservableCollection<ValidationIssue> _validationIssues = new();

    [ObservableProperty]
    private bool _hasValidationErrors;

    #endregion

    #region Computed Properties

    public bool IsNotBusy => !IsBusy;

    public IReadOnlyList<TrimSize> AvailableTrimSizes { get; } = Enum.GetValues<TrimSize>().ToList();
    public IReadOnlyList<PaperType> AvailablePaperTypes { get; } = Enum.GetValues<PaperType>().ToList();
    public IReadOnlyList<KDPMarketplace> AvailableMarketplaces { get; } = Enum.GetValues<KDPMarketplace>().ToList();
    public IReadOnlyList<BookFormatType> AvailableFormatTypes { get; } = Enum.GetValues<BookFormatType>().ToList();
    public IReadOnlyList<APlusModuleType> AvailableModuleTypes { get; } = Enum.GetValues<APlusModuleType>().ToList();

    public int RemainingKeywords => 7 - SelectedKeywords.Count;
    public bool CanAddMoreKeywords => SelectedKeywords.Count < 7;

    #endregion

    public KDPViewModel(
        IKDPService kdpService,
        IProjectService projectService,
        ILogger<KDPViewModel> logger)
    {
        _kdpService = kdpService ?? throw new ArgumentNullException(nameof(kdpService));
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Commands

    [RelayCommand]
    private async Task LoadMetadataAsync(CancellationToken cancellationToken)
    {
        if (CurrentProjectId == null) return;

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var result = await _kdpService.GetMetadataAsync(CurrentProjectId.Value, cancellationToken);
            if (result.IsSuccess)
            {
                Metadata = result.Value;
                SyncFromMetadata();
                _logger.LogInformation("Loaded KDP metadata for project {ProjectId}", CurrentProjectId);
            }
            else
            {
                ErrorMessage = result.Error;
            }

            // Also load series
            var seriesResult = await _kdpService.GetAllSeriesAsync(cancellationToken);
            if (seriesResult.IsSuccess)
            {
                AllSeries.Clear();
                foreach (var series in seriesResult.Value!)
                {
                    AllSeries.Add(series);
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load metadata: {ex.Message}";
            _logger.LogError(ex, "Error loading KDP metadata");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(IsNotBusy))]
    private async Task SaveMetadataAsync(CancellationToken cancellationToken)
    {
        if (CurrentProjectId == null || Metadata == null) return;

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            SyncToMetadata();

            // Validate first
            var validationResult = await _kdpService.ValidateMetadataAsync(Metadata, cancellationToken);
            if (validationResult.IsSuccess)
            {
                ValidationIssues.Clear();
                foreach (var issue in validationResult.Value!)
                {
                    ValidationIssues.Add(issue);
                }
                HasValidationErrors = validationResult.Value.Any(v => v.Severity == "Error");
            }

            var result = await _kdpService.SaveMetadataAsync(CurrentProjectId.Value, Metadata, cancellationToken);
            if (result.IsSuccess)
            {
                SuccessMessage = "Metadata saved successfully";
                _logger.LogInformation("Saved KDP metadata for project {ProjectId}", CurrentProjectId);
            }
            else
            {
                ErrorMessage = result.Error;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to save metadata: {ex.Message}";
            _logger.LogError(ex, "Error saving KDP metadata");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SearchCategoriesAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(CategorySearchQuery)) return;

        try
        {
            var result = await _kdpService.SearchCategoriesAsync(CategorySearchQuery, null, cancellationToken);
            if (result.IsSuccess)
            {
                CategorySearchResults.Clear();
                foreach (var category in result.Value!)
                {
                    CategorySearchResults.Add(category);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching categories");
        }
    }

    [RelayCommand(CanExecute = nameof(IsNotBusy))]
    private async Task SuggestCategoriesAsync(CancellationToken cancellationToken)
    {
        if (CurrentProjectId == null) return;

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var result = await _kdpService.SuggestCategoriesAsync(CurrentProjectId.Value, cancellationToken);
            if (result.IsSuccess)
            {
                SuggestedCategories.Clear();
                foreach (var category in result.Value!)
                {
                    SuggestedCategories.Add(category);
                }
                _logger.LogInformation("Suggested {Count} categories", result.Value.Count);
            }
            else
            {
                ErrorMessage = result.Error;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to suggest categories: {ex.Message}";
            _logger.LogError(ex, "Error suggesting categories");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(IsNotBusy))]
    private async Task GenerateKeywordsAsync(CancellationToken cancellationToken)
    {
        if (CurrentProjectId == null) return;

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var result = await _kdpService.GenerateKeywordsAsync(CurrentProjectId.Value, 7, cancellationToken);
            if (result.IsSuccess)
            {
                KeywordSuggestions.Clear();
                foreach (var keyword in result.Value!)
                {
                    KeywordSuggestions.Add(keyword);
                }
                _logger.LogInformation("Generated {Count} keyword suggestions", result.Value.Count);
            }
            else
            {
                ErrorMessage = result.Error;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to generate keywords: {ex.Message}";
            _logger.LogError(ex, "Error generating keywords");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void AddKeyword(KeywordSuggestion? suggestion)
    {
        if (suggestion == null || !CanAddMoreKeywords) return;
        if (SelectedKeywords.Contains(suggestion.Keyword)) return;

        SelectedKeywords.Add(suggestion.Keyword);
        OnPropertyChanged(nameof(RemainingKeywords));
        OnPropertyChanged(nameof(CanAddMoreKeywords));
    }

    [RelayCommand]
    private void AddCustomKeyword()
    {
        if (string.IsNullOrWhiteSpace(NewKeyword) || !CanAddMoreKeywords) return;
        if (SelectedKeywords.Contains(NewKeyword)) return;

        SelectedKeywords.Add(NewKeyword);
        NewKeyword = string.Empty;
        OnPropertyChanged(nameof(RemainingKeywords));
        OnPropertyChanged(nameof(CanAddMoreKeywords));
    }

    [RelayCommand]
    private void RemoveKeyword(string? keyword)
    {
        if (keyword == null) return;
        SelectedKeywords.Remove(keyword);
        OnPropertyChanged(nameof(RemainingKeywords));
        OnPropertyChanged(nameof(CanAddMoreKeywords));
    }

    [RelayCommand(CanExecute = nameof(IsNotBusy))]
    private async Task GenerateDescriptionAsync(CancellationToken cancellationToken)
    {
        if (CurrentProjectId == null) return;

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var result = await _kdpService.GenerateDescriptionVariationsAsync(
                CurrentProjectId.Value, 
                3, 
                cancellationToken);

            if (result.IsSuccess)
            {
                DescriptionSuggestions.Clear();
                foreach (var desc in result.Value!)
                {
                    DescriptionSuggestions.Add(desc);
                }
                if (result.Value.Count > 0)
                {
                    SelectedDescription = result.Value[0];
                    CurrentDescription = SelectedDescription.Description;
                    UpdateDescriptionCharacterCount();
                }
                _logger.LogInformation("Generated {Count} description variations", result.Value.Count);
            }
            else
            {
                ErrorMessage = result.Error;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to generate description: {ex.Message}";
            _logger.LogError(ex, "Error generating description");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void SelectDescription(BookDescriptionSuggestion? description)
    {
        if (description == null) return;
        SelectedDescription = description;
        CurrentDescription = description.Description;
        UpdateDescriptionCharacterCount();
    }

    [RelayCommand(CanExecute = nameof(IsNotBusy))]
    private async Task CalculatePrintSpecsAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var result = await _kdpService.CalculatePrintSpecsAsync(
                PageCount,
                SelectedTrimSize,
                SelectedPaperType,
                HasBleed,
                cancellationToken);

            if (result.IsSuccess)
            {
                PrintSpecifications = result.Value;
                _logger.LogInformation("Calculated print specifications");
            }
            else
            {
                ErrorMessage = result.Error;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to calculate print specs: {ex.Message}";
            _logger.LogError(ex, "Error calculating print specs");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(IsNotBusy))]
    private async Task CalculateRoyaltiesAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var result = await _kdpService.CalculateRoyaltiesAsync(
                ListPrice,
                SelectedMarketplace,
                SelectedFormatType,
                PrintSpecifications,
                cancellationToken);

            if (result.IsSuccess)
            {
                PricingInfo = result.Value;
                _logger.LogInformation("Calculated royalties: ${Royalty}", result.Value?.EstimatedRoyalty);
            }
            else
            {
                ErrorMessage = result.Error;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to calculate royalties: {ex.Message}";
            _logger.LogError(ex, "Error calculating royalties");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CreateSeriesAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(NewSeriesName)) return;

        try
        {
            var result = await _kdpService.GetOrCreateSeriesAsync(NewSeriesName, cancellationToken);
            if (result.IsSuccess)
            {
                if (!AllSeries.Any(s => s.Name == result.Value!.Name))
                {
                    AllSeries.Add(result.Value!);
                }
                SelectedSeries = result.Value;
                NewSeriesName = string.Empty;
                _logger.LogInformation("Created/retrieved series: {Name}", result.Value!.Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating series");
        }
    }

    [RelayCommand(CanExecute = nameof(IsNotBusy))]
    private async Task GenerateAPlusContentAsync(CancellationToken cancellationToken)
    {
        if (CurrentProjectId == null) return;

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var result = await _kdpService.GenerateAPlusContentAsync(
                CurrentProjectId.Value,
                SelectedModuleTypes.ToList(),
                cancellationToken);

            if (result.IsSuccess)
            {
                AplusContent = result.Value;
                _logger.LogInformation("Generated A+ content with {Count} modules", 
                    result.Value?.Modules.Count ?? 0);
            }
            else
            {
                ErrorMessage = result.Error;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to generate A+ content: {ex.Message}";
            _logger.LogError(ex, "Error generating A+ content");
        }
        finally
        {
            IsBusy = false;
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

    private void SyncFromMetadata()
    {
        if (Metadata == null) return;

        SelectedPrimaryCategory = Metadata.PrimaryCategory;
        SelectedSecondaryCategory = Metadata.SecondaryCategory;
        
        SelectedKeywords.Clear();
        foreach (var kw in Metadata.Keywords)
        {
            SelectedKeywords.Add(kw);
        }
        
        CurrentDescription = Metadata.Description ?? string.Empty;
        UpdateDescriptionCharacterCount();

        if (Metadata.Series != null)
        {
            SelectedSeries = AllSeries.FirstOrDefault(s => s.Name == Metadata.Series.Name);
            SeriesPosition = Metadata.Series.Position;
        }

        OnPropertyChanged(nameof(RemainingKeywords));
        OnPropertyChanged(nameof(CanAddMoreKeywords));
    }

    private void SyncToMetadata()
    {
        if (Metadata == null) return;

        Metadata.PrimaryCategory = SelectedPrimaryCategory;
        Metadata.SecondaryCategory = SelectedSecondaryCategory;
        
        Metadata.Keywords.Clear();
        foreach (var kw in SelectedKeywords)
        {
            Metadata.Keywords.Add(kw);
        }
        
        Metadata.Description = CurrentDescription;

        if (SelectedSeries != null)
        {
            Metadata.Series = new SeriesInfo
            {
                Name = SelectedSeries.Name,
                Position = SeriesPosition
            };
        }
    }

    private void UpdateDescriptionCharacterCount()
    {
        DescriptionCharacterCount = CurrentDescription?.Length ?? 0;
    }

    partial void OnCurrentDescriptionChanged(string value)
    {
        UpdateDescriptionCharacterCount();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Sets the current project context and loads metadata.
    /// </summary>
    public async Task SetProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        CurrentProjectId = projectId;
        Metadata = new KDPMetadata();
        await LoadMetadataAsync(cancellationToken);
    }

    #endregion
}
