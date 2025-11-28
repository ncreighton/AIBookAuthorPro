// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Windows.Controls;
using AIBookAuthorPro.UI.ViewModels;
using UserControl = System.Windows.Controls.UserControl;

namespace AIBookAuthorPro.UI.Views;

/// <summary>
/// Interaction logic for OutlineEditorView.xaml
/// </summary>
public partial class OutlineEditorView : UserControl
{
    private readonly OutlineEditorViewModel _viewModel;

    /// <summary>
    /// Initializes a new instance of OutlineEditorView.
    /// </summary>
    public OutlineEditorView(OutlineEditorViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = _viewModel;
        
        InitializeComponent();
    }

    private void TreeView_SelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is OutlineItemViewModel item)
        {
            _viewModel.SelectedItem = item;
        }
    }
}
