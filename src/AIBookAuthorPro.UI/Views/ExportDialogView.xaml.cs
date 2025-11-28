// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Windows.Controls;
using AIBookAuthorPro.UI.ViewModels;
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
    }
}
