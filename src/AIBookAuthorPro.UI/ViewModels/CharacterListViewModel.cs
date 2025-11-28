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
    private readonly IProjectService _projectService;
    private readonly IGenerationPipelineService _generationService;
    private readonly ILogger<CharacterEditorViewModel> _logger;

    [ObservableProperty]
    private Project? _project;

    [ObservableProperty]
    private Character? _character;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private CharacterRoleEnum _role = CharacterRoleEnum.Supporting;

    [ObservableProperty]
    private string? _age;

    [ObservableProperty]
    private string? _gender;

    [ObservableProperty]
    private string? _occupation;

    [ObservableProperty]
    private string? _physicalDescription;

    [ObservableProperty]
    private string? _personality;

    [ObservableProperty]
    private string? _backstory;

    [ObservableProperty]
    private string? _goals;

    [ObservableProperty]
    private string? _fears;

    [ObservableProperty]
    private string? _quirks;

    [ObservableProperty]
    private string? _notes;

    [ObservableProperty]
    private ObservableCollection<CharacterRelationship> _relationships = [];

    [ObservableProperty]
    private ObservableCollection<Character> _availableCharacters = [];

    [ObservableProperty]
    private bool _isModified;

    [ObservableProperty]
    private bool _isGenerating;

    [ObservableProperty]
    private string _statusMessage = "Edit Character";

    /// <summary>
    /// Gets the available character roles.
    /// </summary>
    public CharacterRoleEnum[] AvailableRoles { get; } = Enum.GetValues<CharacterRoleEnum>();

    /// <summary>
    /// Event raised when editing is complete.
    /// </summary>
    public event EventHandler? EditingComplete;

    /// <summary>
    /// Initializes a new instance of CharacterEditorViewModel.
    /// </summary>
    public CharacterEditorViewModel(
        IProjectService projectService,
        IGenerationPipelineService generationService,
        ILogger<CharacterEditorViewModel> logger)
    {
        _projectService = projectService;
        _generationService = generationService;
        _logger = logger;
    }

    /// <summary>
    /// Loads a character for editing.
    /// </summary>
    public void LoadCharacter(Project project, Character character)
    {
        Project = project;
        Character = character;

        // Load character data
        Name = character.Name;
        Role = (CharacterRoleEnum)character.Role;
        Age = character.Age;
        Gender = character.Gender;
        Occupation = character.Occupation;
        PhysicalDescription = character.PhysicalDescription;
        Personality = character.Personality;
        Backstory = character.Backstory;
        Goals = character.Goals;
        Fears = character.Fears;
        Quirks = character.Quirks;
        Notes = character.Notes;

        // Load relationships
        Relationships.Clear();
        foreach (var rel in character.Relationships)
        {
            Relationships.Add(rel);
        }

        // Load available characters for relationships
        AvailableCharacters.Clear();
        foreach (var c in project.Characters.Where(c => c.Id != character.Id))
        {
            AvailableCharacters.Add(c);
        }

        IsModified = false;
        StatusMessage = $"Editing: {character.Name}";
    }

    // Track modifications
    partial void OnNameChanged(string value) => IsModified = true;
    partial void OnRoleChanged(CharacterRoleEnum value) => IsModified = true;
    partial void OnPhysicalDescriptionChanged(string? value) => IsModified = true;
    partial void OnPersonalityChanged(string? value) => IsModified = true;
    partial void OnBackstoryChanged(string? value) => IsModified = true;

    [RelayCommand]
    private async Task SaveCharacterAsync()
    {
        if (Character == null) return;

        _logger.LogDebug("Saving character {CharacterName}", Name);

        // Update character from properties
        Character.Name = Name;
        Character.Role = Role;
        Character.Age = Age;
        Character.Gender = Gender;
        Character.Occupation = Occupation;
        Character.PhysicalDescription = PhysicalDescription;
        Character.Personality = Personality;
        Character.Backstory = Backstory;
        Character.Goals = Goals;
        Character.Fears = Fears;
        Character.Quirks = Quirks;
        Character.Notes = Notes;
        Character.Relationships = [.. Relationships];
        Character.MarkAsModified();

        _projectService.MarkAsModified();
        await _projectService.SaveProjectAsync();

        IsModified = false;
        StatusMessage = "Character saved";
    }

    [RelayCommand]
    private void AddRelationship()
    {
        if (AvailableCharacters.Count == 0) return;

        var newRelationship = new CharacterRelationship
        {
            CharacterId = AvailableCharacters[0].Id,
            RelationshipType = "Acquaintance"
        };

        Relationships.Add(newRelationship);
        IsModified = true;
    }

    [RelayCommand]
    private void RemoveRelationship(CharacterRelationship? relationship)
    {
        if (relationship == null) return;

        Relationships.Remove(relationship);
        IsModified = true;
    }

    [RelayCommand]
    private async Task GenerateDescriptionAsync(string field)
    {
        if (Character == null || Project == null) return;

        IsGenerating = true;
        StatusMessage = $"Generating {field}...";

        try
        {
            var prompt = field switch
            {
                "physical" => $"Generate a vivid physical description for a {Role} character named {Name} in a {Project.Metadata?.Genre} story. Age: {Age ?? "unspecified"}. Gender: {Gender ?? "unspecified"}.",
                "personality" => $"Generate a personality profile for a {Role} character named {Name} in a {Project.Metadata?.Genre} story. Include traits, mannerisms, and speaking style.",
                "backstory" => $"Generate a compelling backstory for a {Role} character named {Name} in a {Project.Metadata?.Genre} story. Goals: {Goals ?? "unknown"}. Fears: {Fears ?? "unknown"}.",
                _ => $"Describe the character {Name}"
            };

            // For now, just show what would be generated
            StatusMessage = $"AI generation would use prompt: {prompt[..Math.Min(50, prompt.Length)]}...";

            // In a real implementation, call the generation service
            // var result = await _generationService.RefineContentAsync(prompt, "Generate", AIProviderType.Claude, CancellationToken.None);
        }
        finally
        {
            IsGenerating = false;
        }
    }

    [RelayCommand]
    private async Task SaveAndCloseAsync()
    {
        await SaveCharacterAsync();
        EditingComplete?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void Cancel()
    {
        EditingComplete?.Invoke(this, EventArgs.Empty);
    }
}
