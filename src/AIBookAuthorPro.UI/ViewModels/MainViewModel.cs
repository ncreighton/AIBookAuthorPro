// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Collections.ObjectModel;
using System.Windows.Input;
using AIBookAuthorPro.Core.Interfaces;
using AIBookAuthorPro.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.UI.ViewModels;

/// <summary>
/// Main view model for the application shell.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IProjectService _projectService;
    private readonly IAIProviderFactory _aiProviderFactory;
    private readonly ILogger<MainViewModel> _logger;

    [ObservableProperty]
    private string _title = "AI Book Author Pro";

    [ObservableProperty]
    private Project? _currentProject;

    [ObservableProperty]
    private Chapter? _selectedChapter;

    [ObservableProperty]
    private bool _isProjectOpen;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private double _progress;

    [ObservableProperty]
    private ObservableObject? _currentView;

    [ObservableProperty]
    private string _selectedNavigationItem = "Editor";

    public ObservableCollection<ProjectInfo> RecentProjects { get; } = new();
    public ObservableCollection<Chapter> Chapters { get; } = new();

    public MainViewModel(
        IProjectService projectService,
        IAIProviderFactory aiProviderFactory,
        ILogger<MainViewModel> logger)
    {
        _projectService = projectService;
        _aiProviderFactory = aiProviderFactory;
        _logger = logger;

        LoadRecentProjectsCommand.Execute(null);
    }

    [RelayCommand]
    private async Task NewProjectAsync()
    {
        _logger.LogInformation("Creating new project");
        
        // In a real app, show a dialog to get project details
        var result = await _projectService.CreateProjectAsync(
            "Untitled Project",
            BookType.Novel);

        if (result.IsSuccess && result.Value != null)
        {
            CurrentProject = result.Value;
            IsProjectOpen = true;
            UpdateChaptersList();
            StatusMessage = "New project created";
        }
        else
        {
            StatusMessage = $"Failed to create project: {result.Error}";
        }
    }

    [RelayCommand]
    private async Task OpenProjectAsync(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            // Show file dialog
            return;
        }

        _logger.LogInformation("Opening project: {FilePath}", filePath);
        IsBusy = true;
        StatusMessage = "Opening project...";

        try
        {
            var result = await _projectService.OpenProjectAsync(filePath);

            if (result.IsSuccess && result.Value != null)
            {
                CurrentProject = result.Value;
                IsProjectOpen = true;
                UpdateChaptersList();
                Title = $"AI Book Author Pro - {CurrentProject.Metadata.Title}";
                StatusMessage = "Project opened successfully";
            }
            else
            {
                StatusMessage = $"Failed to open project: {result.Error}";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanSaveProject))]
    private async Task SaveProjectAsync()
    {
        if (CurrentProject == null) return;

        _logger.LogInformation("Saving project");
        IsBusy = true;
        StatusMessage = "Saving project...";

        try
        {
            var result = await _projectService.SaveProjectAsync();

            if (result.IsSuccess)
            {
                StatusMessage = "Project saved successfully";
            }
            else
            {
                StatusMessage = $"Failed to save project: {result.Error}";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSaveProject() => IsProjectOpen && CurrentProject != null;

    [RelayCommand(CanExecute = nameof(CanSaveProject))]
    private async Task SaveProjectAsAsync(string? filePath)
    {
        if (CurrentProject == null || string.IsNullOrEmpty(filePath)) return;

        _logger.LogInformation("Saving project as: {FilePath}", filePath);
        IsBusy = true;
        StatusMessage = "Saving project...";

        try
        {
            var result = await _projectService.SaveProjectAsync(filePath);

            if (result.IsSuccess)
            {
                Title = $"AI Book Author Pro - {CurrentProject.Metadata.Title}";
                StatusMessage = "Project saved successfully";
            }
            else
            {
                StatusMessage = $"Failed to save project: {result.Error}";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CloseProjectAsync()
    {
        if (CurrentProject == null) return;

        // Check for unsaved changes
        if (_projectService.HasUnsavedChanges())
        {
            // Show save prompt dialog
            // For now, just save
            await SaveProjectAsync();
        }

        await _projectService.CloseProjectAsync();
        CurrentProject = null;
        IsProjectOpen = false;
        Chapters.Clear();
        SelectedChapter = null;
        Title = "AI Book Author Pro";
        StatusMessage = "Project closed";
    }

    [RelayCommand]
    private async Task LoadRecentProjectsAsync()
    {
        var result = await _projectService.GetRecentProjectsAsync();
        
        if (result.IsSuccess && result.Value != null)
        {
            RecentProjects.Clear();
            foreach (var project in result.Value)
            {
                RecentProjects.Add(project);
            }
        }
    }

    [RelayCommand]
    private void AddChapter()
    {
        if (CurrentProject == null) return;

        var newChapter = new Chapter
        {
            Id = Guid.NewGuid(),
            Title = $"Chapter {CurrentProject.Chapters.Count + 1}",
            Number = CurrentProject.Chapters.Count + 1,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        CurrentProject.Chapters.Add(newChapter);
        Chapters.Add(newChapter);
        SelectedChapter = newChapter;
        _projectService.MarkAsModified();
        StatusMessage = $"Added {newChapter.Title}";
    }

    [RelayCommand]
    private void DeleteChapter(Chapter? chapter)
    {
        if (CurrentProject == null || chapter == null) return;

        CurrentProject.Chapters.Remove(chapter);
        Chapters.Remove(chapter);
        
        if (SelectedChapter == chapter)
        {
            SelectedChapter = Chapters.FirstOrDefault();
        }

        // Renumber remaining chapters
        for (int i = 0; i < CurrentProject.Chapters.Count; i++)
        {
            CurrentProject.Chapters[i].Number = i + 1;
        }

        _projectService.MarkAsModified();
        StatusMessage = $"Deleted {chapter.Title}";
    }

    [RelayCommand]
    private void NavigateTo(string destination)
    {
        SelectedNavigationItem = destination;
        _logger.LogDebug("Navigating to: {Destination}", destination);
        // View switching is handled by the View based on SelectedNavigationItem
    }

    partial void OnSelectedChapterChanged(Chapter? value)
    {
        if (value != null)
        {
            _logger.LogDebug("Selected chapter: {ChapterTitle}", value.Title);
        }
    }

    partial void OnCurrentProjectChanged(Project? value)
    {
        IsProjectOpen = value != null;
        SaveProjectCommand.NotifyCanExecuteChanged();
        SaveProjectAsCommand.NotifyCanExecuteChanged();
    }

    private void UpdateChaptersList()
    {
        Chapters.Clear();
        if (CurrentProject?.Chapters != null)
        {
            foreach (var chapter in CurrentProject.Chapters.OrderBy(c => c.Number))
            {
                Chapters.Add(chapter);
            }
        }
        SelectedChapter = Chapters.FirstOrDefault();
    }
}