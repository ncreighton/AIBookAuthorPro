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
    /// <summary>
    /// Initializes a new instance of OutlineEditorView.
    /// </summary>
    public OutlineEditorView()
    {
        InitializeComponent();
    }

    private void TreeView_SelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is OutlineEditorViewModel vm && e.NewValue is OutlineItemViewModel item)
        {
            vm.SelectedItem = item;
        }
    }
}
