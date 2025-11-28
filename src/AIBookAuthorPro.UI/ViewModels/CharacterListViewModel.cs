// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Collections.ObjectModel;
using CharacterRoleEnum = AIBookAuthorPro.Core.Enums.CharacterRole;
using AIBookAuthorPro.Core.Enums;
using AIBookAuthorPro.Core.Interfaces;
using AIBookAuthorPro.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.UI.ViewModels;

/// <summary>
/// ViewModel for the character list view.
/// </summary>
public partial class CharacterListViewModel : ObservableObject
{
    private readonly IProjectService _projectService;
    private readonly ILogger<CharacterListViewModel> _logger;

    [ObservableProperty]
    private Project? _project;

    [ObservableProperty]
    private ObservableCollection<Character> _characters = [];

    [ObservableProperty]
    private ObservableCollection<Character> _filteredCharacters = [];

    [ObservableProperty]
    private Character? _selectedCharacter;

    [ObservableProperty]
    private CharacterRoleEnum? _roleFilter;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string _statusMessage = "Characters";

    /// <summary>
    /// Event raised when a character should be edited.
    /// </summary>
    public event EventHandler<Character>? EditCharacterRequested;

    /// <summary>
    /// Event raised when navigation back is requested.
    /// </summary>
    public event EventHandler? BackRequested;

    /// <summary>
    /// Initializes a new instance of the CharacterListViewModel.
    /// </summary>
    public CharacterListViewModel(
        IProjectService projectService,
        ILogger<CharacterListViewModel> logger)
    {
        _projectService = projectService;
        _logger = logger;
    }

    /// <summary>
    /// Loads characters from the project.
    /// </summary>
    public void LoadProject(Project project)
    {
        ArgumentNullException.ThrowIfNull(project);

        _logger.LogDebug("Loading characters for project {ProjectName}", project.Name);

        Project = project;
        RefreshCharacterList();
        StatusMessage = $"{project.Characters.Count} characters";
    }

    /// <summary>
    /// Refreshes the character list.
    /// </summary>
    public void RefreshCharacterList()
    {
        if (Project == null) return;

        Characters.Clear();
        foreach (var character in Project.Characters.OrderBy(c => c.Name))
        {
            Characters.Add(character);
        }

        ApplyFilters();
    }

    partial void OnRoleFilterChanged(CharacterRoleEnum? value)
    {
        ApplyFilters();
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        FilteredCharacters.Clear();

        var filtered = Characters.AsEnumerable();

        if (RoleFilter.HasValue)
        {
            filtered = filtered.Where(c => c.Role == (Core.Models.CharacterRole)RoleFilter.Value);
        }

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(c =>
                c.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                (c.Description?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        foreach (var character in filtered)
        {
            FilteredCharacters.Add(character);
        }
    }

    [RelayCommand]
    private void ClearFilters()
    {
        RoleFilter = null;
        SearchText = string.Empty;
    }

    [RelayCommand]
    private void CreateNewCharacter()
    {
        if (Project == null) return;

        var newCharacter = new Character
        {
            Name = "New Character",
            Role = Core.Models.CharacterRole.Supporting
        };

        Project.AddCharacter(newCharacter);

        RefreshCharacterList();
        SelectedCharacter = newCharacter;

        EditCharacterRequested?.Invoke(this, newCharacter);

        _logger.LogInformation("Created new character");
    }

    [RelayCommand]
    private void EditCharacter(Character? character)
    {
        if (character == null) return;

        _logger.LogDebug("Editing character {CharacterName}", character.Name);
        EditCharacterRequested?.Invoke(this, character);
    }

    [RelayCommand]
    private async Task DeleteCharacterAsync(Character? character)
    {
        if (character == null || Project == null) return;

        _logger.LogDebug("Deleting character {CharacterName}", character.Name);

        // Remove character relationships
        foreach (var c in Project.Characters)
        {
            c.RemoveRelationship(character.Id);
        }

        if (Project.RemoveCharacter(character.Id))
        {
            await _projectService.SaveAsync(Project);
        }

        RefreshCharacterList();
        StatusMessage = $"Deleted: {character.Name}";
    }

    [RelayCommand]
    private void DuplicateCharacter(Character? character)
    {
        if (character == null || Project == null) return;

        var duplicate = new Character
        {
            Name = $"{character.Name} (Copy)",
            Role = character.Role,
            Age = character.Age,
            Gender = character.Gender,
            Occupation = character.Occupation,
            Description = character.Description,
            Traits = new List<string>(character.Traits),
            Backstory = character.Backstory,
            Goals = character.Goals,
            Fears = character.Fears,
            Notes = character.Notes
        };

        Project.AddCharacter(duplicate);

        RefreshCharacterList();
        StatusMessage = $"Duplicated: {character.Name}";
    }

    [RelayCommand]
    private void FilterByRole(CharacterRoleEnum role)
    {
        RoleFilter = RoleFilter == role ? null : role;
    }

    [RelayCommand]
    private void GoBack()
    {
        BackRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private async Task SaveAllAsync()
    {
        if (Project == null) return;

        var result = await _projectService.SaveAsync(Project);

        if (result.IsSuccess)
        {
            StatusMessage = "All characters saved";
        }
        else
        {
            StatusMessage = $"Save failed: {result.Error}";
        }
    }
}
