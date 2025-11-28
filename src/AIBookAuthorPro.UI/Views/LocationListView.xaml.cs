// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AIBookAuthorPro.Core.Models;
using AIBookAuthorPro.UI.ViewModels;
using UserControl = System.Windows.Controls.UserControl;

namespace AIBookAuthorPro.UI.Views;

/// <summary>
/// Interaction logic for LocationListView.xaml
/// </summary>
public partial class LocationListView : UserControl
{
    private readonly LocationListViewModel _viewModel;

    /// <summary>
    /// Initializes a new instance of LocationListView.
    /// </summary>
    public LocationListView(LocationListViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = _viewModel;
        
        InitializeComponent();
    }

    private void OnLocationCardClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element &&
            element.DataContext is Location location)
        {
            _viewModel.EditLocationCommand.Execute(location);
        }
    }
}
