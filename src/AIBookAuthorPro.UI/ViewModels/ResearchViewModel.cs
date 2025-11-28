// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Collections.ObjectModel;
using AIBookAuthorPro.Core.Enums;
using AIBookAuthorPro.Core.Interfaces;
using AIBookAuthorPro.Core.Models.Research;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.UI.ViewModels;

/// <summary>
/// ViewModel for the Research Assistant feature.
/// </summary>
public partial class ResearchViewModel : ObservableObject
{
    private readonly IResearchService _researchService;
    private readonly IProjectService _projectService;
    private readonly ILogger<ResearchViewModel> _logger;

    #region Observable Properties

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SearchCommand))]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private ResearchType _selectedResearchType = ResearchType.General;

    [ObservableProperty]
    private string? _timePeriod;

    [ObservableProperty]
    private string? _geographicContext;

    [ObservableProperty]
    private string? _genreContext;

    [ObservableProperty]
    private int _maxResults = 10;

    [ObservableProperty]
    private bool _includeAISummary = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotSearching))]
    [NotifyCanExecuteChangedFor(nameof(SearchCommand))]
    [NotifyCanExecuteChangedFor(nameof(GenerateNamesCommand))]
    [NotifyCanExecuteChangedFor(nameof(GetTropesCommand))]
    [NotifyCanExecuteChangedFor(nameof(CheckAccuracyCommand))]
    [NotifyCanExecuteChangedFor(nameof(ResearchLocationCommand))]
    private bool _isSearching;

    [ObservableProperty]
    private ResearchResultSet? _currentResultSet;

    [ObservableProperty]
    private ResearchResult? _selectedResult;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private Guid? _currentProjectId;

    // Name Generator
    [ObservableProperty]
    private string? _nameGender;

    [ObservableProperty]
    private string? _nameOrigin;

    [ObservableProperty]
    private string? _nameTimePeriod;

    [ObservableProperty]
    private int _nameCount = 10;

    [ObservableProperty]
    private ObservableCollection<NameSuggestion> _nameSuggestions = new();

    // Genre Tropes
    [ObservableProperty]
    private string _tropeGenre = string.Empty;

    [ObservableProperty]
    private bool _includeSubversions = true;

    [ObservableProperty]
    private ObservableCollection<GenreTrope> _genreTropes = new();

    [ObservableProperty]
    private GenreTrope? _selectedTrope;

    // Historical Accuracy
    [ObservableProperty]
    private string _textToCheck = string.Empty;

    [ObservableProperty]
    private string _accuracyTimePeriod = string.Empty;

    [ObservableProperty]
    private string? _accuracyLocation;

    [ObservableProperty]
    private ObservableCollection<AccuracyIssue> _accuracyIssues = new();

    // Location Research
    [ObservableProperty]
    private string _locationToResearch = string.Empty;

    [ObservableProperty]
    private LocationResearchResult? _locationResult;

    // Saved Research
    [ObservableProperty]
    private ObservableCollection<ResearchResult> _savedResults = new();

    #endregion

    #region Computed Properties

    public bool IsNotSearching => !IsSearching;

    public IReadOnlyList<ResearchType> AvailableResearchTypes { get; } = 
        Enum.GetValues<ResearchType>().ToList();

    #endregion

    public ResearchViewModel(
        IResearchService researchService,
        IProjectService projectService,
        ILogger<ResearchViewModel> logger)
    {
        _researchService = researchService ?? throw new ArgumentNullException(nameof(researchService));
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Commands

    [RelayCommand(CanExecute = nameof(CanSearch))]
    private async Task SearchAsync(CancellationToken cancellationToken)
    {
        IsSearching = true;
        ErrorMessage = null;

        try
        {
            _logger.LogDebug("Performing research search: {Query}", SearchQuery);

            var query = new ResearchQuery
            {
                Query = SearchQuery,
                Type = SelectedResearchType,
                TimePeriod = TimePeriod,
                GeographicContext = GeographicContext,
                GenreContext = GenreContext,
                MaxResults = MaxResults,
                IncludeAISummary = IncludeAISummary,
                ProjectId = CurrentProjectId
            };

            var result = await _researchService.SearchAsync(query, cancellationToken);

            if (result.IsSuccess)
            {
                CurrentResultSet = result.Value;
                _logger.LogInformation("Research completed with {Count} results", 
                    result.Value?.Results.Count ?? 0);
            }
            else
            {
                ErrorMessage = result.Error;
                _logger.LogWarning("Research failed: {Error}", result.Error);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Research search cancelled");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Search failed: {ex.Message}";
            _logger.LogError(ex, "Error performing research search");
        }
        finally
        {
            IsSearching = false;
        }
    }

    private bool CanSearch() => !IsSearching && !string.IsNullOrWhiteSpace(SearchQuery);

    [RelayCommand(CanExecute = nameof(IsNotSearching))]
    private async Task GenerateNamesAsync(CancellationToken cancellationToken)
    {
        IsSearching = true;
        ErrorMessage = null;

        try
        {
            _logger.LogDebug("Generating character names");

            var result = await _researchService.GenerateCharacterNamesAsync(
                NameGender,
                NameOrigin,
                NameTimePeriod,
                NameCount,
                cancellationToken);

            if (result.IsSuccess)
            {
                NameSuggestions.Clear();
                foreach (var name in result.Value!)
                {
                    NameSuggestions.Add(name);
                }
                _logger.LogInformation("Generated {Count} name suggestions", result.Value.Count);
            }
            else
            {
                ErrorMessage = result.Error;
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Name generation cancelled");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Name generation failed: {ex.Message}";
            _logger.LogError(ex, "Error generating names");
        }
        finally
        {
            IsSearching = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanGetTropes))]
    private async Task GetTropesAsync(CancellationToken cancellationToken)
    {
        IsSearching = true;
        ErrorMessage = null;

        try
        {
            _logger.LogDebug("Getting genre tropes for: {Genre}", TropeGenre);

            var result = await _researchService.GetGenreTropesAsync(
                TropeGenre,
                IncludeSubversions,
                cancellationToken);

            if (result.IsSuccess)
            {
                GenreTropes.Clear();
                foreach (var trope in result.Value!)
                {
                    GenreTropes.Add(trope);
                }
                _logger.LogInformation("Retrieved {Count} genre tropes", result.Value.Count);
            }
            else
            {
                ErrorMessage = result.Error;
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Trope retrieval cancelled");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Trope retrieval failed: {ex.Message}";
            _logger.LogError(ex, "Error getting tropes");
        }
        finally
        {
            IsSearching = false;
        }
    }

    private bool CanGetTropes() => !IsSearching && !string.IsNullOrWhiteSpace(TropeGenre);

    [RelayCommand(CanExecute = nameof(CanCheckAccuracy))]
    private async Task CheckAccuracyAsync(CancellationToken cancellationToken)
    {
        IsSearching = true;
        ErrorMessage = null;

        try
        {
            _logger.LogDebug("Checking historical accuracy for period: {Period}", AccuracyTimePeriod);

            var result = await _researchService.CheckHistoricalAccuracyAsync(
                TextToCheck,
                AccuracyTimePeriod,
                AccuracyLocation,
                cancellationToken);

            if (result.IsSuccess)
            {
                AccuracyIssues.Clear();
                foreach (var issue in result.Value!)
                {
                    AccuracyIssues.Add(issue);
                }
                _logger.LogInformation("Found {Count} accuracy issues", result.Value.Count);
            }
            else
            {
                ErrorMessage = result.Error;
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Accuracy check cancelled");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Accuracy check failed: {ex.Message}";
            _logger.LogError(ex, "Error checking accuracy");
        }
        finally
        {
            IsSearching = false;
        }
    }

    private bool CanCheckAccuracy() => !IsSearching && 
        !string.IsNullOrWhiteSpace(TextToCheck) && 
        !string.IsNullOrWhiteSpace(AccuracyTimePeriod);

    [RelayCommand(CanExecute = nameof(CanResearchLocation))]
    private async Task ResearchLocationAsync(CancellationToken cancellationToken)
    {
        IsSearching = true;
        ErrorMessage = null;

        try
        {
            _logger.LogDebug("Researching location: {Location}", LocationToResearch);

            var result = await _researchService.ResearchLocationAsync(
                LocationToResearch,
                null, // Use default aspects
                cancellationToken);

            if (result.IsSuccess)
            {
                LocationResult = result.Value;
                _logger.LogInformation("Completed location research for: {Location}", LocationToResearch);
            }
            else
            {
                ErrorMessage = result.Error;
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Location research cancelled");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Location research failed: {ex.Message}";
            _logger.LogError(ex, "Error researching location");
        }
        finally
        {
            IsSearching = false;
        }
    }

    private bool CanResearchLocation() => !IsSearching && !string.IsNullOrWhiteSpace(LocationToResearch);

    [RelayCommand]
    private async Task SaveResultAsync(ResearchResult? result, CancellationToken cancellationToken)
    {
        if (result == null || CurrentProjectId == null) return;

        try
        {
            _logger.LogDebug("Saving research result to project");

            var saveResult = await _researchService.SaveToProjectAsync(
                CurrentProjectId.Value,
                result,
                cancellationToken);

            if (saveResult.IsSuccess)
            {
                SavedResults.Add(result);
                _logger.LogInformation("Saved research result: {Title}", result.Title);
            }
            else
            {
                ErrorMessage = saveResult.Error;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to save: {ex.Message}";
            _logger.LogError(ex, "Error saving research result");
        }
    }

    [RelayCommand]
    private void ClearResults()
    {
        CurrentResultSet = null;
        SelectedResult = null;
        ErrorMessage = null;
    }

    [RelayCommand]
    private void ClearNames()
    {
        NameSuggestions.Clear();
        NameGender = null;
        NameOrigin = null;
        NameTimePeriod = null;
    }

    [RelayCommand]
    private void ClearTropes()
    {
        GenreTropes.Clear();
        SelectedTrope = null;
        TropeGenre = string.Empty;
    }

    [RelayCommand]
    private void ClearAccuracy()
    {
        AccuracyIssues.Clear();
        TextToCheck = string.Empty;
    }

    [RelayCommand]
    private void ClearLocation()
    {
        LocationResult = null;
        LocationToResearch = string.Empty;
    }

    [RelayCommand]
    private void CopyToClipboard(string? text)
    {
        if (string.IsNullOrEmpty(text)) return;

        try
        {
            System.Windows.Clipboard.SetText(text);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to copy to clipboard");
        }
    }

    [RelayCommand]
    private void UseFollowUpQuery(string? query)
    {
        if (string.IsNullOrEmpty(query)) return;
        SearchQuery = query;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Sets the current project context.
    /// </summary>
    public void SetProject(Guid projectId)
    {
        CurrentProjectId = projectId;
        _logger.LogDebug("Set research context to project {ProjectId}", projectId);
    }

    /// <summary>
    /// Loads saved research for the current project.
    /// </summary>
    public async Task LoadSavedResearchAsync(CancellationToken cancellationToken = default)
    {
        if (CurrentProjectId == null) return;

        try
        {
            var projectResult = await _projectService.GetProjectAsync(CurrentProjectId.Value, cancellationToken);
            if (projectResult.IsSuccess && projectResult.Value != null)
            {
                SavedResults.Clear();
                foreach (var note in projectResult.Value.ResearchNotes)
                {
                    // Convert research notes back to research results for display
                    var result = new ResearchResult
                    {
                        Title = note.Title,
                        Content = note.Content,
                        SourceName = note.Source,
                        IsSaved = true,
                        RetrievedAt = note.CreatedAt
                    };
                    foreach (var tag in note.Tags)
                    {
                        result.KeyFacts.Add(tag);
                    }
                    SavedResults.Add(result);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading saved research");
        }
    }

    #endregion
}
