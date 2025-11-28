// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Windows;
using System.Windows.Controls;
using AIBookAuthorPro.Core.Interfaces;
using AIBookAuthorPro.Infrastructure.Services;
using AIBookAuthorPro.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MessageBox = System.Windows.MessageBox;

namespace AIBookAuthorPro.UI.Views;

/// <summary>
/// Main application window.
/// </summary>
public partial class MainWindow : Window
{
    private readonly ILogger<MainWindow> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly MainViewModel _viewModel;
    private readonly NotificationService _notificationService;

    /// <summary>
    /// Initializes a new instance of MainWindow.
    /// </summary>
    public MainWindow(
        MainViewModel viewModel,
        NotificationService notificationService,
        ILogger<MainWindow> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _viewModel = viewModel;
        _notificationService = notificationService;

        InitializeComponent();

        DataContext = _viewModel;

        // Initialize notification host
        NotificationHost.DataContext = _notificationService;

        // Subscribe to navigation events
        _viewModel.NavigationRequested += OnNavigationRequested;
        _viewModel.ShowExportDialogRequested += OnShowExportDialogRequested;
        _viewModel.ShowSettingsDialogRequested += OnShowSettingsDialogRequested;

        // Subscribe to navigation list selection
        NavigationList.SelectionChanged += OnNavigationSelectionChanged;

        Loaded += OnLoaded;
        Closing += OnClosing;

        _logger.LogInformation("MainWindow initialized");
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        _logger.LogDebug("MainWindow loaded");

<<<<<<< HEAD
        // Load recent projects asynchronously now that the UI is ready
        if (_viewModel.LoadRecentProjectsCommand.CanExecute(null))
        {
            await _viewModel.LoadRecentProjectsCommand.ExecuteAsync(null);
        }

        // Show welcome screen or last project
        await NavigateToViewAsync(NavigationDestination.Dashboard);
=======
        try
        {
            // Initialize ViewModel asynchronously - this prevents UI deadlock
            await _viewModel.InitializeAsync();

            // Navigate to dashboard
            await NavigateToViewAsync(NavigationDestination.Dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize main window");
            _notificationService.ShowError($"Failed to initialize: {ex.Message}");
        }
>>>>>>> 1b9798acd926b365f6d87a1deb8950c293c5732f
    }

    private async void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_viewModel.HasUnsavedChanges)
        {
            var result = MessageBox.Show(
                "You have unsaved changes. Do you want to save before closing?",
                "Unsaved Changes",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    e.Cancel = true;
                    if (_viewModel.SaveProjectCommand.CanExecute(null))
                    {
                        await _viewModel.SaveProjectCommand.ExecuteAsync(null);
                    }
                    Close();
                    break;
                case MessageBoxResult.Cancel:
                    e.Cancel = true;
                    break;
            }
        }
    }

    private void OnNavigationSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (NavigationList.SelectedItem is ListBoxItem item && item.Tag is string destinationString)
        {
            if (Enum.TryParse<NavigationDestination>(destinationString, out var destination))
            {
                _ = NavigateToViewAsync(destination);
            }
        }
    }

    private void OnNavigationRequested(object? sender, NavigationEventArgs e)
    {
        _ = NavigateToViewAsync(e.Destination, e.Parameter);
    }

    private async void OnShowExportDialogRequested(object? sender, EventArgs e)
    {
        await ShowExportDialogAsync();
    }

    private async void OnShowSettingsDialogRequested(object? sender, EventArgs e)
    {
        await ShowSettingsDialogAsync();
    }

    private async Task NavigateToViewAsync(NavigationDestination destination, object? parameter = null)
    {
        try
        {
            _logger.LogDebug("Navigating to {Destination}", destination);

            FrameworkElement? view = destination switch
            {
                NavigationDestination.Dashboard => CreateView<ProjectDashboardView>(),
                NavigationDestination.Editor => CreateView<ChapterEditorView>(),
                NavigationDestination.Characters => CreateView<CharacterListView>(),
                NavigationDestination.Locations => CreateView<LocationListView>(),
                NavigationDestination.Outline => CreateView<OutlineEditorView>(),
                NavigationDestination.Export => null, // Handled via dialog
                NavigationDestination.Settings => null, // Handled via dialog
                _ => CreateView<ProjectDashboardView>()
            };

            if (view != null)
            {
                // Initialize view's ViewModel if it has one
                if (view.DataContext is INavigationAware navAware)
                {
                    await navAware.OnNavigatedToAsync(parameter);
                }

                ContentFrame.Content = view;
                _viewModel.CurrentPageTitle = GetPageTitle(destination);
            }
            else if (destination == NavigationDestination.Export)
            {
                await ShowExportDialogAsync();
            }
            else if (destination == NavigationDestination.Settings)
            {
                await ShowSettingsDialogAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to navigate to {Destination}", destination);
            _notificationService.ShowError($"Failed to navigate: {ex.Message}");
        }
    }

    private T CreateView<T>() where T : FrameworkElement
    {
        return _serviceProvider.GetRequiredService<T>();
    }

    private static string GetPageTitle(NavigationDestination destination) => destination switch
    {
        NavigationDestination.Dashboard => "Project Dashboard",
        NavigationDestination.Editor => "Chapter Editor",
        NavigationDestination.Characters => "Characters",
        NavigationDestination.Locations => "Locations",
        NavigationDestination.Outline => "Outline",
        NavigationDestination.Export => "Export",
        NavigationDestination.Settings => "Settings",
        _ => "AI Book Author Pro"
    };

    private async Task ShowExportDialogAsync()
    {
        try
        {
            var dialog = _serviceProvider.GetRequiredService<ExportDialogView>();
            var result = await MaterialDesignThemes.Wpf.DialogHost.Show(dialog, "MainDialogHost");

            if (result is true)
            {
                _notificationService.ShowSuccess("Export completed successfully!");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to show export dialog");
            _notificationService.ShowError($"Failed to open export dialog: {ex.Message}");
        }
    }

    private async Task ShowSettingsDialogAsync()
    {
        try
        {
            var dialog = _serviceProvider.GetRequiredService<SettingsView>();
            await MaterialDesignThemes.Wpf.DialogHost.Show(dialog, "MainDialogHost");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to show settings dialog");
            _notificationService.ShowError($"Failed to open settings: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates the application status.
    /// </summary>
    public void SetStatus(Controls.IndicatorStatus status, string? message = null)
    {
        AppStatus.Status = status;
        if (!string.IsNullOrEmpty(message))
        {
            AppStatus.Text = message;
        }
    }
}

/// <summary>
/// Interface for ViewModels that need navigation awareness.
/// </summary>
public interface INavigationAware
{
    /// <summary>
    /// Called when navigated to this view.
    /// </summary>
    Task OnNavigatedToAsync(object? parameter);

    /// <summary>
    /// Called when navigating away from this view.
    /// </summary>
    Task OnNavigatingFromAsync();
}
