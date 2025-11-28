// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Windows.Controls;
using System.Windows.Input;
using AIBookAuthorPro.UI.ViewModels;
using UserControl = System.Windows.Controls.UserControl;

namespace AIBookAuthorPro.UI.Views;

/// <summary>
/// Interaction logic for ProjectDashboardView.xaml
/// </summary>
public partial class ProjectDashboardView : UserControl
{
    private readonly ProjectDashboardViewModel _viewModel;

    /// <summary>
    /// Initializes a new instance of ProjectDashboardView.
    /// </summary>
    public ProjectDashboardView(ProjectDashboardViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = _viewModel;
        
        InitializeComponent();
    }

    private void OnCharactersCardClick(object sender, MouseButtonEventArgs e)
    {
        _viewModel.NavigateToCharactersCommand.Execute(null);
    }

    private void OnLocationsCardClick(object sender, MouseButtonEventArgs e)
    {
        _viewModel.NavigateToLocationsCommand.Execute(null);
    }
}
