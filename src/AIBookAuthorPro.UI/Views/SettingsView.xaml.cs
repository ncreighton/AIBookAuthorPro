// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Windows;
using System.Windows.Controls;
using AIBookAuthorPro.UI.ViewModels;
using MaterialDesignThemes.Wpf;
using UserControl = System.Windows.Controls.UserControl;

namespace AIBookAuthorPro.UI.Views;

/// <summary>
/// Interaction logic for SettingsView.xaml
/// </summary>
public partial class SettingsView : UserControl
{
    private readonly SettingsViewModel _viewModel;

    /// <summary>
    /// Initializes a new instance of SettingsView.
    /// </summary>
    public SettingsView(SettingsViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = _viewModel;
        
        InitializeComponent();

        // Subscribe to the close request from ViewModel
        _viewModel.CloseRequested += OnCloseRequested;

        // Load settings when the view is loaded
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Load settings into the ViewModel
        _viewModel.LoadSettings();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        // Unsubscribe to prevent memory leaks
        _viewModel.CloseRequested -= OnCloseRequested;
    }

    private void OnCloseRequested(object? sender, EventArgs e)
    {
        // Close the MaterialDesign DialogHost
        DialogHost.Close("MainDialogHost", false);
    }
}
