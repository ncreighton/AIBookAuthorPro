// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AIBookAuthorPro.UI.ViewModels;
using MaterialDesignThemes.Wpf;
using UserControl = System.Windows.Controls.UserControl;

namespace AIBookAuthorPro.UI.Views;

/// <summary>
/// Interaction logic for ExportDialogView.xaml
/// </summary>
public partial class ExportDialogView : UserControl
{
    private readonly ExportViewModel _viewModel;

    /// <summary>
    /// Initializes a new instance of ExportDialogView.
    /// </summary>
    public ExportDialogView(ExportViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = _viewModel;
        
        InitializeComponent();

        // Subscribe to the close request from ViewModel
        _viewModel.CloseRequested += OnCloseRequested;

        // Unsubscribe when unloaded
        Unloaded += OnUnloaded;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        // Unsubscribe to prevent memory leaks
        _viewModel.CloseRequested -= OnCloseRequested;
    }

    private void OnCloseRequested(object? sender, EventArgs e)
    {
        // Find the DialogHost that contains this UserControl and close it
        var dialogHost = FindParentDialogHost(this);
        
        if (dialogHost != null)
        {
            // Close the dialog using the instance method
            dialogHost.IsOpen = false;
        }
        else
        {
            // Fallback: Try to close using the DialogHost command
            // This works when the dialog was opened with DialogHost.Show()
            try
            {
                DialogHost.CloseDialogCommand.Execute(false, this);
            }
            catch
            {
                // Last resort: Try the static close method
                DialogHost.Close("MainDialogHost", false);
            }
        }
    }

    /// <summary>
    /// Traverses the visual tree to find the parent DialogHost.
    /// </summary>
    private static DialogHost? FindParentDialogHost(DependencyObject child)
    {
        var parent = VisualTreeHelper.GetParent(child);
        
        while (parent != null)
        {
            if (parent is DialogHost dialogHost)
            {
                return dialogHost;
            }
            parent = VisualTreeHelper.GetParent(parent);
        }
        
        return null;
    }
}
