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
            Id = Guid.NewGuid(),
            Name = "New Character",
            Role = (Core.Models.CharacterRole)CharacterRoleEnum.Supporting,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        Project.Characters.Add(newCharacter);
        _projectService.MarkAsModified();

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
            c.Relationships.RemoveAll(r => r.CharacterId == character.Id);
        }

        Project.Characters.Remove(character);
        _projectService.MarkAsModified();
        await _projectService.SaveProjectAsync();

        RefreshCharacterList();
        StatusMessage = $"Deleted: {character.Name}";
    }

    [RelayCommand]
    private void DuplicateCharacter(Character? character)
    {
        if (character == null || Project == null) return;

        var duplicate = new Character
        {
            Id = Guid.NewGuid(),
            Name = $"{character.Name} (Copy)",
            Role = character.Role,
            Age = character.Age,
            Gender = character.Gender,
            Occupation = character.Occupation,
            PhysicalDescription = character.PhysicalDescription,
            Personality = character.Personality,
            Backstory = character.Backstory,
            Goals = character.Goals,
            Fears = character.Fears,
            Quirks = character.Quirks,
            Notes = character.Notes,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        Project.Characters.Add(duplicate);
        _projectService.MarkAsModified();

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
        var result = await _projectService.SaveProjectAsync();

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
