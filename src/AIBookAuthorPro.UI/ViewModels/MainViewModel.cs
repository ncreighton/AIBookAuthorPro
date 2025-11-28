// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Collections.ObjectModel;
using System.Windows.Input;
using AIBookAuthorPro.Core.Interfaces;
using AIBookAuthorPro.Core.Models;
using ProjectSummary = AIBookAuthorPro.Core.Interfaces.ProjectSummary;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.UI.ViewModels;

/// <summary>
/// Navigation destinations for the application.
/// </summary>
public enum NavigationDestination
{
    Dashboard,
    Editor,
    Characters,
    Locations,
    Outline,
    Research,
    Export,
    Settings
}

/// <summary>
/// Main view model for the application shell.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IProjectService _projectService;
    private readonly IAIProviderFactory _aiProviderFactory;
    private readonly ISettingsService _settingsService;
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
    private NavigationDestination _currentNavigation = NavigationDestination.Dashboard;

    [ObservableProperty]
    private string _selectedNavigationItem = "Dashboard";

    [ObservableProperty]
    private bool _isNavigationExpanded = true;

    [ObservableProperty]
    private bool _hasUnsavedChanges;

    [ObservableProperty]
    private string _currentPageTitle = "Dashboard";

    [ObservableProperty]
    private bool _isInitialized;

    /// <summary>
    /// Gets the recent projects list.
    /// </summary>
    public ObservableCollection<ProjectSummary> RecentProjects { get; } = new();

    /// <summary>
    /// Gets the chapters list.
    /// </summary>
    public ObservableCollection<Chapter> Chapters { get; } = new();

    /// <summary>
    /// Event raised when navigation to a view is requested.
    /// </summary>
    public event EventHandler<NavigationEventArgs>? NavigationRequested;

    /// <summary>
    /// Event raised when export dialog should be shown.
    /// </summary>
    public event EventHandler? ShowExportDialogRequested;

    /// <summary>
    /// Event raised when settings dialog should be shown.
    /// </summary>
    public event EventHandler? ShowSettingsDialogRequested;

    public MainViewModel(
        IProjectService projectService,
        IAIProviderFactory aiProviderFactory,
        ISettingsService settingsService,
        ILogger<MainViewModel> logger)
    {
        _projectService = projectService;
        _aiProviderFactory = aiProviderFactory;
        _settingsService = settingsService;
        _logger = logger;

<<<<<<< HEAD
        // Initialize recent projects asynchronously to avoid deadlock
        // This will be called after the window is loaded
        _ = Task.Run(async () =>
        {
            try
            {
                await LoadRecentProjectsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load recent projects during initialization");
            }
        });
=======
        _logger.LogDebug("MainViewModel created");
        
        // DO NOT call any commands here - they will be called after the window loads
        // This prevents UI thread deadlocks during startup
    }

    /// <summary>
    /// Initializes the view model asynchronously. Call this from Loaded event.
    /// </summary>
    [RelayCommand]
    public async Task InitializeAsync()
    {
        if (IsInitialized) return;

        _logger.LogDebug("Initializing MainViewModel");
        
        try
        {
            IsBusy = true;
            StatusMessage = "Loading...";

            // Load recent projects on background thread
            await LoadRecentProjectsAsync();

            IsInitialized = true;
            StatusMessage = "Ready";
            _logger.LogInformation("MainViewModel initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize MainViewModel");
            StatusMessage = "Failed to initialize";
        }
        finally
        {
            IsBusy = false;
        }
>>>>>>> 1b9798acd926b365f6d87a1deb8950c293c5732f
    }

    [RelayCommand]
    private async Task NewProjectAsync()
    {
        _logger.LogInformation("Creating new project");
        
        // In a real app, show a dialog to get project details
        var result = await _projectService.CreateAsync(
            "Untitled Project");

        if (result.IsSuccess && result.Value != null)
        {
            CurrentProject = result.Value;
            IsProjectOpen = true;
            UpdateChaptersList();
            StatusMessage = "New project created";
            NavigateTo(NavigationDestination.Dashboard);
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
            var result = await _projectService.LoadAsync(filePath);

            if (result.IsSuccess && result.Value != null)
            {
                CurrentProject = result.Value;
                IsProjectOpen = true;
                UpdateChaptersList();
                Title = $"AI Book Author Pro - {CurrentProject.Metadata.Title}";
                StatusMessage = "Project opened successfully";
                NavigateTo(NavigationDestination.Dashboard);
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
            var result = await _projectService.SaveAsync(CurrentProject);

            if (result.IsSuccess)
            {
                HasUnsavedChanges = false;
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
            var result = await _projectService.SaveAsync(CurrentProject, filePath);

            if (result.IsSuccess)
            {
                Title = $"AI Book Author Pro - {CurrentProject.Metadata.Title}";
                HasUnsavedChanges = false;
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

        // TODO: Check for unsaved changes and show save prompt dialog
        // For now, just close without saving
        
        CurrentProject = null;
        IsProjectOpen = false;
        Chapters.Clear();
        SelectedChapter = null;
        Title = "AI Book Author Pro";
        StatusMessage = "Project closed";
        HasUnsavedChanges = false;
        NavigateTo(NavigationDestination.Dashboard);
    }

    [RelayCommand]
    private async Task LoadRecentProjectsAsync()
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load recent projects");
            // Non-fatal - continue without recent projects
        }
    }

    [RelayCommand]
    private void AddChapter()
    {
        if (CurrentProject == null) return;

        var newChapter = new Chapter
        {
            Title = $"Chapter {CurrentProject.Chapters.Count + 1}"
        };

        CurrentProject.AddChapter(newChapter);
        Chapters.Add(newChapter);
        SelectedChapter = newChapter;
        HasUnsavedChanges = true;
        StatusMessage = $"Added {newChapter.Title}";
    }

    [RelayCommand]
    private void DeleteChapter(Chapter? chapter)
    {
        if (CurrentProject == null || chapter == null) return;

        if (CurrentProject.RemoveChapter(chapter.Id))
        {
            Chapters.Remove(chapter);
            
            if (SelectedChapter == chapter)
            {
                SelectedChapter = Chapters.FirstOrDefault();
            }
            
            HasUnsavedChanges = true;
        }

        StatusMessage = $"Deleted {chapter.Title}";
    }

    [RelayCommand]
    private void NavigateTo(NavigationDestination destination)
    {
        CurrentNavigation = destination;
        SelectedNavigationItem = destination.ToString();
        _logger.LogDebug("Navigating to: {Destination}", destination);

        // Raise navigation event for view to handle
        NavigationRequested?.Invoke(this, new NavigationEventArgs(destination));
    }

    [RelayCommand]
    private void NavigateToString(string destination)
    {
        if (Enum.TryParse<NavigationDestination>(destination, out var nav))
        {
            NavigateTo(nav);
        }
    }

    [RelayCommand]
    private void ShowExportDialog()
    {
        if (!IsProjectOpen) return;

        _logger.LogDebug("Showing export dialog");
        ShowExportDialogRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void ShowSettings()
    {
        _logger.LogDebug("Showing settings dialog");
        ShowSettingsDialogRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void ToggleNavigation()
    {
        IsNavigationExpanded = !IsNavigationExpanded;
    }

    [RelayCommand]
    private void EditChapter(Chapter? chapter)
    {
        if (chapter == null) return;

        SelectedChapter = chapter;
        NavigateTo(NavigationDestination.Editor);
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
            foreach (var chapter in CurrentProject.Chapters.OrderBy(c => c.Order))
            {
                Chapters.Add(chapter);
            }
        }
        SelectedChapter = Chapters.FirstOrDefault();
    }
}

/// <summary>
/// Event args for navigation requests.
/// </summary>
public class NavigationEventArgs : EventArgs
{
    /// <summary>
    /// Gets the navigation destination.
    /// </summary>
    public NavigationDestination Destination { get; }

    /// <summary>
    /// Gets optional navigation parameter.
    /// </summary>
    public object? Parameter { get; }

    /// <summary>
    /// Initializes a new instance of NavigationEventArgs.
    /// </summary>
    public NavigationEventArgs(NavigationDestination destination, object? parameter = null)
    {
        Destination = destination;
        Parameter = parameter;
    }
}
