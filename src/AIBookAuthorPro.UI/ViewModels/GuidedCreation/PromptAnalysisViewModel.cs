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
/// ViewModel for the prompt analysis step of the guided creation wizard.
/// </summary>
public partial class PromptAnalysisViewModel : ObservableObject
{
    private readonly IPromptAnalysisService _promptAnalysisService;
    private readonly ILogger _logger;

    #region Observable Properties

    [ObservableProperty]
    private PromptAnalysisResult? _analysisResult;

    [ObservableProperty]
    private bool _isAnalyzing;

    [ObservableProperty]
    private double _analysisProgress;

    [ObservableProperty]
    private string _analysisStatus = string.Empty;

    [ObservableProperty]
    private string _detectedGenre = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _detectedSubGenres = new();

    [ObservableProperty]
    private ObservableCollection<string> _extractedThemes = new();

    [ObservableProperty]
    private string _targetAudience = string.Empty;

    [ObservableProperty]
    private string _coreConflict = string.Empty;

    [ObservableProperty]
    private string _tone = string.Empty;

    [ObservableProperty]
    private int _estimatedWordCount;

    [ObservableProperty]
    private string _suggestedStructure = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ExtractedCharacterViewModel> _extractedCharacters = new();

    [ObservableProperty]
    private ObservableCollection<ExtractedLocationViewModel> _extractedLocations = new();

    [ObservableProperty]
    private AnalysisConfidence? _confidence;

    [ObservableProperty]
    private ObservableCollection<ClarificationRequestViewModel> _clarificationRequests = new();

    [ObservableProperty]
    private ObservableCollection<EnhancementSuggestionViewModel> _enhancementSuggestions = new();

    [ObservableProperty]
    private bool _showDetailedAnalysis;

    #endregion

    #region Computed Properties

    public double OverallConfidence => Confidence != null
        ? (Confidence.GenreConfidence + Confidence.ThemeConfidence + 
           Confidence.StructureConfidence + Confidence.CharacterConfidence + 
           Confidence.WorldBuildingConfidence) / 5.0 * 100
        : 0;

    public string ConfidenceLevel => OverallConfidence switch
    {
        >= 80 => "High",
        >= 60 => "Good",
        >= 40 => "Moderate",
        _ => "Low"
    };

    public string ConfidenceColor => OverallConfidence switch
    {
        >= 80 => "#4CAF50",
        >= 60 => "#8BC34A",
        >= 40 => "#FFC107",
        _ => "#FF9800"
    };

    public bool HasClarifications => ClarificationRequests.Any();

    public bool HasEnhancements => EnhancementSuggestions.Any();

    #endregion

    public PromptAnalysisViewModel(
        IPromptAnalysisService promptAnalysisService,
        ILogger logger)
    {
        _promptAnalysisService = promptAnalysisService ?? throw new ArgumentNullException(nameof(promptAnalysisService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Commands

    [RelayCommand]
    private async Task AnalyzeAsync(BookSeedPrompt prompt, CancellationToken cancellationToken)
    {
        IsAnalyzing = true;
        AnalysisProgress = 0;
        AnalysisStatus = "Starting analysis...";

        try
        {
            // Simulate progress updates
            AnalysisProgress = 10;
            AnalysisStatus = "Reading your prompt...";
            await Task.Delay(500, cancellationToken);

            AnalysisProgress = 30;
            AnalysisStatus = "Identifying genre and themes...";

            var result = await _promptAnalysisService.AnalyzePromptAsync(prompt, cancellationToken);

            AnalysisProgress = 70;
            AnalysisStatus = "Extracting characters and settings...";
            await Task.Delay(300, cancellationToken);

            if (result.IsSuccess)
            {
                AnalysisProgress = 90;
                AnalysisStatus = "Finalizing analysis...";
                
                LoadAnalysis(result.Value!);
                
                AnalysisProgress = 100;
                AnalysisStatus = "Analysis complete!";
            }
            else
            {
                AnalysisStatus = $"Analysis failed: {result.Error}";
            }
        }
        catch (OperationCanceledException)
        {
            AnalysisStatus = "Analysis cancelled";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during prompt analysis");
            AnalysisStatus = $"Error: {ex.Message}";
        }
        finally
        {
            IsAnalyzing = false;
        }
    }

    [RelayCommand]
    private void ToggleDetailedAnalysis()
    {
        ShowDetailedAnalysis = !ShowDetailedAnalysis;
    }

    [RelayCommand]
    private void AcceptSuggestion(EnhancementSuggestionViewModel suggestion)
    {
        suggestion.IsAccepted = true;
        OnPropertyChanged(nameof(EnhancementSuggestions));
    }

    [RelayCommand]
    private void DismissSuggestion(EnhancementSuggestionViewModel suggestion)
    {
        EnhancementSuggestions.Remove(suggestion);
    }

    #endregion

    #region Public Methods

    public void LoadAnalysis(PromptAnalysisResult analysis)
    {
        AnalysisResult = analysis;

        DetectedGenre = analysis.DetectedGenre ?? "Unknown";
        TargetAudience = analysis.TargetAudience ?? "General";
        CoreConflict = analysis.CoreConflict ?? "Not identified";
        Tone = analysis.ToneDescriptor ?? "Not identified";
        EstimatedWordCount = analysis.EstimatedWordCount;
        SuggestedStructure = analysis.SuggestedStructure.ToString();
        Confidence = analysis.AnalysisConfidence;

        // Load collections
        DetectedSubGenres.Clear();
        foreach (var subGenre in analysis.SubGenres ?? Enumerable.Empty<string>())
            DetectedSubGenres.Add(subGenre);

        ExtractedThemes.Clear();
        foreach (var theme in analysis.ExtractedThemes ?? Enumerable.Empty<string>())
            ExtractedThemes.Add(theme);

        ExtractedCharacters.Clear();
        foreach (var character in analysis.ImpliedCharacters ?? Enumerable.Empty<ExtractedCharacterSeed>())
        {
            ExtractedCharacters.Add(new ExtractedCharacterViewModel
            {
                Name = character.SuggestedName ?? "Unknown",
                Role = character.Role.ToString(),
                Description = character.Description ?? "",
                ArcType = character.PotentialArc ?? ""
            });
        }

        ExtractedLocations.Clear();
        foreach (var location in analysis.ImpliedLocations ?? Enumerable.Empty<ExtractedLocationSeed>())
        {
            ExtractedLocations.Add(new ExtractedLocationViewModel
            {
                Name = location.Name ?? "Unknown",
                Type = location.Type ?? "Unknown",
                Significance = location.Significance ?? ""
            });
        }

        ClarificationRequests.Clear();
        foreach (var request in analysis.ClarificationRequests ?? Enumerable.Empty<ClarificationRequest>())
        {
            ClarificationRequests.Add(new ClarificationRequestViewModel
            {
                Id = request.Id,
                Question = request.Question,
                Category = request.Category ?? "General",
                Priority = request.Priority,
                SuggestedOptions = new ObservableCollection<string>(request.SuggestedOptions ?? Enumerable.Empty<string>())
            });
        }

        EnhancementSuggestions.Clear();
        foreach (var suggestion in analysis.EnhancementSuggestions ?? Enumerable.Empty<EnhancementSuggestion>())
        {
            EnhancementSuggestions.Add(new EnhancementSuggestionViewModel
            {
                Category = suggestion.Category ?? "General",
                Suggestion = suggestion.Suggestion ?? "",
                Impact = suggestion.ImpactLevel.ToString(),
                Priority = suggestion.ImpactLevel
            });
        }

        OnPropertyChanged(nameof(OverallConfidence));
        OnPropertyChanged(nameof(ConfidenceLevel));
        OnPropertyChanged(nameof(ConfidenceColor));
        OnPropertyChanged(nameof(HasClarifications));
        OnPropertyChanged(nameof(HasEnhancements));
    }

    public Dictionary<Guid, string> GetClarificationResponses()
    {
        return ClarificationRequests
            .Where(c => !string.IsNullOrWhiteSpace(c.Response))
            .ToDictionary(c => c.Id, c => c.Response!);
    }

    #endregion
}

/// <summary>
/// ViewModel for an extracted character.
/// </summary>
public partial class ExtractedCharacterViewModel : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _role = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _arcType = string.Empty;
}

/// <summary>
/// ViewModel for an extracted location.
/// </summary>
public partial class ExtractedLocationViewModel : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _type = string.Empty;

    [ObservableProperty]
    private string _significance = string.Empty;
}

/// <summary>
/// ViewModel for a clarification request.
/// </summary>
public partial class ClarificationRequestViewModel : ObservableObject
{
    public Guid Id { get; init; }

    [ObservableProperty]
    private string _question = string.Empty;

    [ObservableProperty]
    private string _category = string.Empty;

    [ObservableProperty]
    private ClarificationPriority _priority;

    [ObservableProperty]
    private ObservableCollection<string> _suggestedOptions = new();

    [ObservableProperty]
    private string? _response;

    [ObservableProperty]
    private string? _selectedOption;

    partial void OnSelectedOptionChanged(string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            Response = value;
        }
    }
}

/// <summary>
/// ViewModel for an enhancement suggestion.
/// </summary>
public partial class EnhancementSuggestionViewModel : ObservableObject
{
    [ObservableProperty]
    private string _category = string.Empty;

    [ObservableProperty]
    private string _suggestion = string.Empty;

    [ObservableProperty]
    private string _impact = string.Empty;

    [ObservableProperty]
    private int _priority;

    [ObservableProperty]
    private bool _isAccepted;
}
