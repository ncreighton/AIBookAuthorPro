// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Windows.Controls;
using AIBookAuthorPro.UI.ViewModels;
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
    }
}
