// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Windows.Controls;
using AIBookAuthorPro.UI.ViewModels.GuidedCreation;
using UserControl = System.Windows.Controls.UserControl;

namespace AIBookAuthorPro.UI.Views.GuidedCreation;

/// <summary>
/// Interaction logic for GuidedCreationWizardView.xaml
/// </summary>
public partial class GuidedCreationWizardView : UserControl
{
    public GuidedCreationWizardView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is GuidedCreationWizardViewModel viewModel)
        {
            await viewModel.InitializeCommand.ExecuteAsync(null);
        }
    }
}
