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
    /// <summary>
    /// Initializes a new instance of ProjectDashboardView.
    /// </summary>
    public ProjectDashboardView()
    {
        InitializeComponent();
    }

    private void OnCharactersCardClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is ProjectDashboardViewModel vm)
        {
            vm.NavigateToCharactersCommand.Execute(null);
        }
    }

    private void OnLocationsCardClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is ProjectDashboardViewModel vm)
        {
            vm.NavigateToLocationsCommand.Execute(null);
        }
    }
}
