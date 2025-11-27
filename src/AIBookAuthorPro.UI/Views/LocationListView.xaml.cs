// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Windows.Controls;
using System.Windows.Input;
using AIBookAuthorPro.Core.Models;
using AIBookAuthorPro.UI.ViewModels;

namespace AIBookAuthorPro.UI.Views;

/// <summary>
/// Interaction logic for LocationListView.xaml
/// </summary>
public partial class LocationListView : UserControl
{
    /// <summary>
    /// Initializes a new instance of LocationListView.
    /// </summary>
    public LocationListView()
    {
        InitializeComponent();
    }

    private void OnLocationCardClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element &&
            element.DataContext is Location location &&
            DataContext is LocationListViewModel vm)
        {
            vm.EditLocationCommand.Execute(location);
        }
    }
}
