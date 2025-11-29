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
/// ViewModel for the generation dashboard step of the guided creation wizard.
/// </summary>
public partial class GenerationDashboardViewModel : ObservableObject
{
    private readonly IBookGenerationOrchestrator _orchestrator;
    private readonly ILogger _logger;
    private CancellationTokenSource? _generationCts;

    #region Observable Properties

    [ObservableProperty]
    private BookBlueprint? _blueprint;

    [ObservableProperty]
    private GenerationSession? _session;

    [ObservableProperty]
    private bool _isGenerating;

    [ObservableProperty]
    private bool _isPaused;

    [ObservableProperty]
    private bool _isComplete;

    [ObservableProperty]
    private string _statusMessage = "Ready to generate";

    [ObservableProperty]
    private double _overallProgress;

    [ObservableProperty]
    private int _currentChapterNumber;

    [ObservableProperty]
    private string _currentOperation = string.Empty;

    [ObservableProperty]
    private int _totalChapters;

    [ObservableProperty]
    private int _completedChapters;

    [ObservableProperty]
    private int _wordsGenerated;

    [ObservableProperty]
    private int _targetWords;

    [ObservableProperty]
    private TimeSpan _elapsedTime;

    [ObservableProperty]
    private TimeSpan _estimatedRemaining;

    [ObservableProperty]
    private double _averageQualityScore;

    [ObservableProperty]
    private decimal _estimatedCost;

    [ObservableProperty]
    private ObservableCollection<ChapterProgressViewModel> _chapterProgress = new();

    [ObservableProperty]
    private ObservableCollection<ActivityLogItemViewModel> _activityLog = new();

    [ObservableProperty]
    private ChapterProgressViewModel? _selectedChapter;

    [ObservableProperty]
    private string _livePreviewContent = string.Empty;

    [ObservableProperty]
    private bool _showLivePreview = true;

    [ObservableProperty]
    private bool _showActivityLog = true;

    // Settings
    [ObservableProperty]
    private bool _autoApproveEnabled = true;

    [ObservableProperty]
    private int _autoApproveThreshold = 80;

    [ObservableProperty]
    private bool _pauseAtActBreaks;

    [ObservableProperty]
    private bool _soundAlertsEnabled = true;

    #endregion

    #region Computed Properties

    public bool CanStart => Blueprint != null && !IsGenerating && !IsComplete;
    public bool CanPause => IsGenerating && !IsPaused;
    public bool CanResume => IsPaused;
    public bool CanCancel => IsGenerating || IsPaused;

    public string ProgressText => $"{CompletedChapters} of {TotalChapters} chapters";
    public string WordsText => $"{WordsGenerated:N0} / {TargetWords:N0} words";
    public double WordsProgressPercentage => TargetWords > 0 ? (double)WordsGenerated / TargetWords * 100 : 0;

    public string ElapsedTimeText => FormatTimeSpan(ElapsedTime);
    public string RemainingTimeText => EstimatedRemaining > TimeSpan.Zero 
        ? $"~{FormatTimeSpan(EstimatedRemaining)} remaining" 
        : "Calculating...";

    public string QualityText => AverageQualityScore > 0 
        ? $"{AverageQualityScore:F1}/100" 
        : "--";

    public string CostText => $"${EstimatedCost:F2}";

    #endregion

    public GenerationDashboardViewModel(
        IBookGenerationOrchestrator orchestrator,
        ILogger logger)
    {
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Commands

    [RelayCommand(CanExecute = nameof(CanStart))]
    private async Task StartGenerationAsync()
    {
        if (Blueprint == null) return;

        _generationCts = new CancellationTokenSource();
        IsGenerating = true;
        IsPaused = false;
        IsComplete = false;
        StatusMessage = "Starting generation...";

        try
        {
            AddActivityLog("Generation started", "AutoAwesome");

            var progress = new Progress<DetailedGenerationProgress>(UpdateProgress);

            var result = await _orchestrator.StartFullGenerationAsync(
                Blueprint,
                new GenerationOptions { UseConfigurationFromBlueprint = true },
                progress,
                _generationCts.Token);

            if (result.IsSuccess)
            {
                Session = result.Value;
                IsComplete = Session!.Status == GenerationSessionStatus.Completed;
                StatusMessage = IsComplete ? "Generation complete!" : "Generation finished";
                AddActivityLog("Generation completed successfully", "CheckCircle");
            }
            else
            {
                StatusMessage = $"Generation failed: {result.Error}";
                AddActivityLog($"Generation failed: {result.Error}", "Error");
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Generation cancelled";
            AddActivityLog("Generation cancelled by user", "Cancel");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during generation");
            StatusMessage = $"Error: {ex.Message}";
            AddActivityLog($"Error: {ex.Message}", "Error");
        }
        finally
        {
            IsGenerating = false;
            UpdateCommandStates();
        }
    }

    [RelayCommand(CanExecute = nameof(CanPause))]
    private async Task PauseGenerationAsync()
    {
        if (Session == null) return;

        StatusMessage = "Pausing generation...";

        try
        {
            var result = await _orchestrator.PauseGenerationAsync(Session.Id);
            if (result.IsSuccess)
            {
                IsPaused = true;
                StatusMessage = "Generation paused";
                AddActivityLog("Generation paused", "Pause");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing generation");
            StatusMessage = $"Error pausing: {ex.Message}";
        }

        UpdateCommandStates();
    }

    [RelayCommand(CanExecute = nameof(CanResume))]
    private async Task ResumeGenerationAsync()
    {
        if (Session == null) return;

        _generationCts = new CancellationTokenSource();
        StatusMessage = "Resuming generation...";

        try
        {
            var progress = new Progress<DetailedGenerationProgress>(UpdateProgress);

            var result = await _orchestrator.ResumeGenerationAsync(
                Session.Id, progress, _generationCts.Token);

            if (result.IsSuccess)
            {
                Session = result.Value;
                IsPaused = false;
                IsGenerating = true;
                StatusMessage = "Generation resumed";
                AddActivityLog("Generation resumed", "PlayArrow");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming generation");
            StatusMessage = $"Error resuming: {ex.Message}";
        }

        UpdateCommandStates();
    }

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private async Task CancelGenerationAsync()
    {
        _generationCts?.Cancel();

        if (Session != null)
        {
            await _orchestrator.CancelGenerationAsync(Session.Id);
        }

        IsGenerating = false;
        IsPaused = false;
        StatusMessage = "Generation cancelled";
        AddActivityLog("Generation cancelled", "Cancel");
        UpdateCommandStates();
    }

    [RelayCommand]
    private async Task RegenerateChapterAsync(int chapterNumber)
    {
        if (Session == null || Blueprint == null) return;

        var chapterVm = ChapterProgress.FirstOrDefault(c => c.ChapterNumber == chapterNumber);
        if (chapterVm == null) return;

        chapterVm.Status = "Regenerating...";
        chapterVm.StatusColor = "#FF9800";

        try
        {
            AddActivityLog($"Regenerating Chapter {chapterNumber}", "Refresh");

            var result = await _orchestrator.RegenerateChapterAsync(
                Session.Id, chapterNumber, null, CancellationToken.None);

            if (result.IsSuccess)
            {
                chapterVm.Status = "Regenerated";
                chapterVm.StatusColor = "#4CAF50";
                chapterVm.QualityScore = result.Value!.QualityScore ?? 0;
                chapterVm.WordCount = result.Value.WordCount;
                AddActivityLog($"Chapter {chapterNumber} regenerated successfully", "CheckCircle");
            }
            else
            {
                chapterVm.Status = "Failed";
                chapterVm.StatusColor = "#F44336";
                AddActivityLog($"Chapter {chapterNumber} regeneration failed", "Error");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error regenerating chapter {ChapterNumber}", chapterNumber);
            chapterVm.Status = "Error";
            chapterVm.StatusColor = "#F44336";
        }
    }

    [RelayCommand]
    private async Task ApproveChapterAsync(int chapterNumber)
    {
        if (Session == null) return;

        try
        {
            var result = await _orchestrator.ApproveChapterAsync(Session.Id, chapterNumber);
            if (result.IsSuccess)
            {
                var chapterVm = ChapterProgress.FirstOrDefault(c => c.ChapterNumber == chapterNumber);
                if (chapterVm != null)
                {
                    chapterVm.IsApproved = true;
                    chapterVm.Status = "Approved";
                    chapterVm.StatusColor = "#4CAF50";
                }
                AddActivityLog($"Chapter {chapterNumber} approved", "ThumbUp");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving chapter {ChapterNumber}", chapterNumber);
        }
    }

    [RelayCommand]
    private void SelectChapter(ChapterProgressViewModel chapter)
    {
        SelectedChapter = chapter;
    }

    [RelayCommand]
    private void ToggleLivePreview()
    {
        ShowLivePreview = !ShowLivePreview;
    }

    [RelayCommand]
    private void ToggleActivityLog()
    {
        ShowActivityLog = !ShowActivityLog;
    }

    [RelayCommand]
    private void ClearActivityLog()
    {
        ActivityLog.Clear();
    }

    #endregion

    #region Public Methods

    public void Initialize(BookBlueprint blueprint)
    {
        Blueprint = blueprint;
        TotalChapters = blueprint.ChapterBlueprints?.Count ?? 0;
        TargetWords = blueprint.Identity?.TargetWordCount ?? 80000;

        ChapterProgress.Clear();
        for (int i = 1; i <= TotalChapters; i++)
        {
            var chapterBlueprint = blueprint.ChapterBlueprints?[i - 1];
            ChapterProgress.Add(new ChapterProgressViewModel
            {
                ChapterNumber = i,
                Title = chapterBlueprint?.Title ?? $"Chapter {i}",
                TargetWordCount = chapterBlueprint?.TargetWordCount ?? 3500,
                Status = "Pending",
                StatusColor = "#9E9E9E"
            });
        }

        ActivityLog.Clear();
        AddActivityLog("Dashboard initialized", "Info");

        ResetProgress();
        UpdateCommandStates();
    }

    #endregion

    #region Private Methods

    private void UpdateProgress(DetailedGenerationProgress progress)
    {
        OverallProgress = progress.OverallPercentage;
        CurrentChapterNumber = progress.CurrentChapter ?? 0;
        CurrentOperation = progress.CurrentOperation;
        WordsGenerated = progress.WordsGenerated;
        ElapsedTime = progress.ElapsedTime;
        EstimatedRemaining = progress.EstimatedRemaining ?? TimeSpan.Zero;
        AverageQualityScore = progress.AverageQualityScore ?? 0;
        EstimatedCost = progress.CostSoFar;
        StatusMessage = progress.CurrentOperation;

        // Update chapter progress
        if (CurrentChapterNumber > 0 && CurrentChapterNumber <= ChapterProgress.Count)
        {
            var currentChapterVm = ChapterProgress[CurrentChapterNumber - 1];
            currentChapterVm.Status = "Generating...";
            currentChapterVm.StatusColor = "#2196F3";
            currentChapterVm.Progress = progress.PhaseProgress;
        }

        // Mark completed chapters
        CompletedChapters = ChapterProgress.Count(c => c.IsComplete || c.IsApproved);

        // Notify computed properties
        OnPropertyChanged(nameof(ProgressText));
        OnPropertyChanged(nameof(WordsText));
        OnPropertyChanged(nameof(WordsProgressPercentage));
        OnPropertyChanged(nameof(ElapsedTimeText));
        OnPropertyChanged(nameof(RemainingTimeText));
        OnPropertyChanged(nameof(QualityText));
        OnPropertyChanged(nameof(CostText));
    }

    private void AddActivityLog(string message, string icon)
    {
        var logItem = new ActivityLogItemViewModel
        {
            Timestamp = DateTime.Now,
            Message = message,
            Icon = icon
        };

        // Add to beginning of list
        ActivityLog.Insert(0, logItem);

        // Keep only last 100 entries
        while (ActivityLog.Count > 100)
        {
            ActivityLog.RemoveAt(ActivityLog.Count - 1);
        }
    }

    private void ResetProgress()
    {
        OverallProgress = 0;
        CurrentChapterNumber = 0;
        CompletedChapters = 0;
        WordsGenerated = 0;
        ElapsedTime = TimeSpan.Zero;
        EstimatedRemaining = TimeSpan.Zero;
        AverageQualityScore = 0;
        EstimatedCost = 0;
        LivePreviewContent = string.Empty;
    }

    private void UpdateCommandStates()
    {
        StartGenerationCommand.NotifyCanExecuteChanged();
        PauseGenerationCommand.NotifyCanExecuteChanged();
        ResumeGenerationCommand.NotifyCanExecuteChanged();
        CancelGenerationCommand.NotifyCanExecuteChanged();
    }

    private static string FormatTimeSpan(TimeSpan span)
    {
        if (span.TotalHours >= 1)
            return $"{(int)span.TotalHours}h {span.Minutes}m";
        if (span.TotalMinutes >= 1)
            return $"{(int)span.TotalMinutes}m {span.Seconds}s";
        return $"{span.Seconds}s";
    }

    private static string GetIconForActivity(string activityType)
    {
        return activityType switch
        {
            "ChapterStarted" => "PlayArrow",
            "ChapterCompleted" => "CheckCircle",
            "ChapterFailed" => "Error",
            "QualityCheck" => "Assessment",
            "Revision" => "Edit",
            "Pause" => "Pause",
            "Resume" => "PlayArrow",
            _ => "Info"
        };
    }

    #endregion
}

#region Supporting ViewModels

public partial class ChapterProgressViewModel : ObservableObject
{
    [ObservableProperty]
    private int _chapterNumber;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private int _targetWordCount;

    [ObservableProperty]
    private int _wordCount;

    [ObservableProperty]
    private double _progress;

    [ObservableProperty]
    private string _status = "Pending";

    [ObservableProperty]
    private string _statusColor = "#9E9E9E";

    [ObservableProperty]
    private double _qualityScore;

    [ObservableProperty]
    private bool _isComplete;

    [ObservableProperty]
    private bool _isApproved;

    [ObservableProperty]
    private bool _needsReview;

    [ObservableProperty]
    private int _issueCount;

    public double ProgressPercentage => TargetWordCount > 0 
        ? (double)WordCount / TargetWordCount * 100 
        : Progress;
}

public partial class ActivityLogItemViewModel : ObservableObject
{
    [ObservableProperty]
    private DateTime _timestamp;

    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private string _icon = "Info";

    public string TimeText => Timestamp.ToString("HH:mm:ss");
}

#endregion
