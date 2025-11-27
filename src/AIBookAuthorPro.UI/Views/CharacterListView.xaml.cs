// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Windows.Controls;
using System.Windows.Input;
using AIBookAuthorPro.Core.Enums;
using AIBookAuthorPro.Core.Models;
using AIBookAuthorPro.UI.ViewModels;

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
    public static CharacterRole Protagonist => CharacterRole.Protagonist;
    public static CharacterRole Antagonist => CharacterRole.Antagonist;
    public static CharacterRole Supporting => CharacterRole.Supporting;
    public static CharacterRole Minor => CharacterRole.Minor;
    public static CharacterRole Narrator => CharacterRole.Narrator;
}
