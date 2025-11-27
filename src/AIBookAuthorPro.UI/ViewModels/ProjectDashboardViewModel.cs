// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Collections.ObjectModel;
using AIBookAuthorPro.Core.Enums;
using AIBookAuthorPro.Core.Interfaces;
using AIBookAuthorPro.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.UI.ViewModels;

/// <summary>
/// ViewModel for the project dashboard view.
/// </summary>
public partial class ProjectDashboardViewModel : ObservableObject
{
    private readonly IProjectService _projectService;
    private readonly ISettingsService _settingsService;
    private readonly ILogger<ProjectDashboardViewModel> _logger;

    [ObservableProperty]
    private Project? _project;

    [ObservableProperty]
    private ProjectStatistics? _statistics;

    [ObservableProperty]
    private ObservableCollection<Chapter> _chapters = [];

    [ObservableProperty]
    private ObservableCollection<Chapter> _recentChapters = [];

    [ObservableProperty]
    private Chapter? _selectedChapter;

    [ObservableProperty]
    private ChapterStatus? _statusFilter;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    /// <summary>
    /// Gets the filtered chapters based on status and search.
    /// </summary>
    public ObservableCollection<Chapter> FilteredChapters { get; } = [];

    /// <summary>
    /// Event raised when a chapter should be opened.
    /// </summary>
    public event EventHandler<Chapter>? ChapterOpenRequested;

    /// <summary>
    /// Event raised when navigation is requested.
    /// </summary>
    public event EventHandler<string>? NavigationRequested;

    /// <summary>
    /// Initializes a new instance of the ProjectDashboardViewModel.
    /// </summary>
    public ProjectDashboardViewModel(
        IProjectService projectService,
        ISettingsService settingsService,
        ILogger<ProjectDashboardViewModel> logger)
    {
        _projectService = projectService;
        _settingsService = settingsService;
        _logger = logger;
    }

    /// <summary>
    /// Loads the project dashboard.
    /// </summary>
    public void LoadProject(Project project)
    {
        ArgumentNullException.ThrowIfNull(project);

        _logger.LogDebug("Loading project dashboard for {ProjectName}", project.Name);

        Project = project;
        RefreshStatistics();
        RefreshChapterList();
        LoadRecentChapters();

        StatusMessage = $"Loaded: {project.Name}";
    }

    /// <summary>
    /// Refreshes statistics from the project.
    /// </summary>
    public void RefreshStatistics()
    {
        if (Project == null) return;

        Statistics = ProjectStatistics.FromProject(Project);
    }

    /// <summary>
    /// Refreshes the chapter list.
    /// </summary>
    public void RefreshChapterList()
    {
        if (Project == null) return;

        Chapters.Clear();
        foreach (var chapter in Project.Chapters.OrderBy(c => c.Order))
        {
            Chapters.Add(chapter);
        }

        ApplyFilters();
    }

    private void LoadRecentChapters()
    {
        if (Project == null) return;

        RecentChapters.Clear();
        var recent = Project.Chapters
            .OrderByDescending(c => c.ModifiedAt)
            .Take(5);

        foreach (var chapter in recent)
        {
            RecentChapters.Add(chapter);
        }
    }

    partial void OnStatusFilterChanged(ChapterStatus? value)
    {
        ApplyFilters();
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        FilteredChapters.Clear();

        var filtered = Chapters.AsEnumerable();

        if (StatusFilter.HasValue)
        {
            filtered = filtered.Where(c => c.Status == StatusFilter.Value);
        }

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(c =>
                c.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                (c.Summary?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        foreach (var chapter in filtered)
        {
            FilteredChapters.Add(chapter);
        }
    }

    [RelayCommand]
    private void ClearFilters()
    {
        StatusFilter = null;
        SearchText = string.Empty;
    }

    [RelayCommand]
    private void OpenChapter(Chapter? chapter)
    {
        if (chapter == null) return;

        _logger.LogDebug("Opening chapter {ChapterNumber}: {Title}", chapter.Order, chapter.Title);
        ChapterOpenRequested?.Invoke(this, chapter);
    }

    [RelayCommand]
    private void CreateNewChapter()
    {
        if (Project == null) return;

        var newOrder = Project.Chapters.Count > 0
            ? Project.Chapters.Max(c => c.Order) + 1
            : 1;

        var newChapter = new Chapter
        {
            Id = Guid.NewGuid(),
            Title = $"Chapter {newOrder}",
            Order = newOrder,
            TargetWordCount = _settingsService.CurrentSettings.Generation.DefaultChapterWordCount,
            Status = ChapterStatus.NotStarted,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        Project.Chapters.Add(newChapter);
        _projectService.MarkAsModified();

        RefreshChapterList();
        RefreshStatistics();

        // Open the new chapter
        ChapterOpenRequested?.Invoke(this, newChapter);

        _logger.LogInformation("Created new chapter {ChapterNumber}", newOrder);
    }

    [RelayCommand]
    private async Task DeleteChapterAsync(Chapter? chapter)
    {
        if (chapter == null || Project == null) return;

        // In a real app, show confirmation dialog
        _logger.LogDebug("Deleting chapter {ChapterNumber}: {Title}", chapter.Order, chapter.Title);

        Project.Chapters.Remove(chapter);

        // Reorder remaining chapters
        var order = 1;
        foreach (var c in Project.Chapters.OrderBy(c => c.Order))
        {
            c.Order = order++;
        }

        _projectService.MarkAsModified();
        await _projectService.SaveProjectAsync();

        RefreshChapterList();
        RefreshStatistics();

        StatusMessage = $"Deleted: {chapter.Title}";
    }

    [RelayCommand]
    private void DuplicateChapter(Chapter? chapter)
    {
        if (chapter == null || Project == null) return;

        var duplicate = new Chapter
        {
            Id = Guid.NewGuid(),
            Title = $"{chapter.Title} (Copy)",
            Order = Project.Chapters.Max(c => c.Order) + 1,
            Content = chapter.Content,
            Summary = chapter.Summary,
            Outline = chapter.Outline,
            Notes = chapter.Notes,
            TargetWordCount = chapter.TargetWordCount,
            Status = ChapterStatus.NotStarted,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        Project.Chapters.Add(duplicate);
        _projectService.MarkAsModified();

        RefreshChapterList();
        RefreshStatistics();

        StatusMessage = $"Duplicated: {chapter.Title}";
    }

    [RelayCommand]
    private void MoveChapterUp(Chapter? chapter)
    {
        if (chapter == null || Project == null || chapter.Order <= 1) return;

        var previousChapter = Project.Chapters.FirstOrDefault(c => c.Order == chapter.Order - 1);
        if (previousChapter != null)
        {
            previousChapter.Order++;
            chapter.Order--;

            _projectService.MarkAsModified();
            RefreshChapterList();
        }
    }

    [RelayCommand]
    private void MoveChapterDown(Chapter? chapter)
    {
        if (chapter == null || Project == null) return;

        var nextChapter = Project.Chapters.FirstOrDefault(c => c.Order == chapter.Order + 1);
        if (nextChapter != null)
        {
            nextChapter.Order--;
            chapter.Order++;

            _projectService.MarkAsModified();
            RefreshChapterList();
        }
    }

    [RelayCommand]
    private void NavigateToCharacters()
    {
        NavigationRequested?.Invoke(this, "Characters");
    }

    [RelayCommand]
    private void NavigateToLocations()
    {
        NavigationRequested?.Invoke(this, "Locations");
    }

    [RelayCommand]
    private void NavigateToOutline()
    {
        NavigationRequested?.Invoke(this, "Outline");
    }

    [RelayCommand]
    private void NavigateToExport()
    {
        NavigationRequested?.Invoke(this, "Export");
    }

    [RelayCommand]
    private void NavigateToSettings()
    {
        NavigationRequested?.Invoke(this, "Settings");
    }

    [RelayCommand]
    private async Task SaveProjectAsync()
    {
        if (Project == null) return;

        IsLoading = true;
        StatusMessage = "Saving...";

        try
        {
            var result = await _projectService.SaveProjectAsync();

            if (result.IsSuccess)
            {
                StatusMessage = "Project saved";
            }
            else
            {
                StatusMessage = $"Save failed: {result.Error}";
            }
        }
        finally
        {
            IsLoading = false;
        }
    }
}
