// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AIBookAuthorPro.Core.Enums;
using AIBookAuthorPro.Core.Models;
using AIBookAuthorPro.UI.ViewModels;
using UserControl = System.Windows.Controls.UserControl;

namespace AIBookAuthorPro.UI.Views;

/// <summary>
/// Interaction logic for CharacterListView.xaml
/// </summary>
public partial class CharacterListView : UserControl
{
    /// <summary>
    /// Initializes a new instance of CharacterListView.
    /// </summary>
    public CharacterListView()
    {
        InitializeComponent();
    }

    private void OnCharacterCardClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element &&
            element.DataContext is Character character &&
            DataContext is CharacterListViewModel vm)
        {
            vm.EditCharacterCommand.Execute(character);
        }
    }
}

/// <summary>
/// Static class for character role values in XAML binding.
/// </summary>
public static class CharacterRoleValue
{
    public static Core.Enums.CharacterRole Protagonist => Core.Enums.CharacterRole.Protagonist;
    public static Core.Enums.CharacterRole Antagonist => Core.Enums.CharacterRole.Antagonist;
    public static Core.Enums.CharacterRole Supporting => Core.Enums.CharacterRole.Supporting;
    public static Core.Enums.CharacterRole Minor => Core.Enums.CharacterRole.Minor;
    public static Core.Enums.CharacterRole Narrator => Core.Enums.CharacterRole.Narrator;
}
