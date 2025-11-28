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
/// ViewModel for editing a single character.
/// </summary>
public partial class CharacterEditorViewModel : ObservableObject
{
    private readonly IProjectService _projectService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<CharacterEditorViewModel> _logger;

    private Project? _project;
    private Character? _originalCharacter;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUnsavedChanges))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _name = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUnsavedChanges))]
    private string _description = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUnsavedChanges))]
    private CharacterRoleEnum _role = CharacterRoleEnum.Supporting;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUnsavedChanges))]
    private int? _age;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUnsavedChanges))]
    private string _physicalDescription = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUnsavedChanges))]
    private string _personality = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUnsavedChanges))]
    private string _background = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUnsavedChanges))]
    private string _motivation = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUnsavedChanges))]
    private string _internalConflict = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUnsavedChanges))]
    private string _externalConflict = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUnsavedChanges))]
    private string _characterArc = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUnsavedChanges))]
    private string _notes = string.Empty;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private bool _isNewCharacter;

    public ObservableCollection<CharacterRoleEnum> AvailableRoles { get; } = new(Enum.GetValues<CharacterRoleEnum>());

    public bool HasUnsavedChanges => _originalCharacter != null && HasChanges();

    public CharacterEditorViewModel(
        IProjectService projectService,
        INotificationService notificationService,
        ILogger<CharacterEditorViewModel> logger)
    {
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Loads a character for editing.
    /// </summary>
    public void LoadCharacter(Project project, Character character)
    {
        _project = project;
        _originalCharacter = character;
        IsNewCharacter = false;

        Name = character.Name;
        Description = character.Description;
        Role = character.Role;
        Age = character.Age;
        PhysicalDescription = character.PhysicalDescription;
        Personality = character.Personality;
        Background = character.Background;
        Motivation = character.Motivation;
        InternalConflict = character.InternalConflict;
        ExternalConflict = character.ExternalConflict;
        CharacterArc = character.CharacterArc;
        Notes = character.Notes;
    }

    /// <summary>
    /// Creates a new character for editing.
    /// </summary>
    public void CreateNewCharacter(Project project)
    {
        _project = project;
        _originalCharacter = new Character { Name = "New Character" };
        IsNewCharacter = true;

        Name = string.Empty;
        Description = string.Empty;
        Role = CharacterRoleEnum.Supporting;
        Age = null;
        PhysicalDescription = string.Empty;
        Personality = string.Empty;
        Background = string.Empty;
        Motivation = string.Empty;
        InternalConflict = string.Empty;
        ExternalConflict = string.Empty;
        CharacterArc = string.Empty;
        Notes = string.Empty;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        if (_project == null || _originalCharacter == null)
            return;

        IsSaving = true;

        try
        {
            _originalCharacter.Name = Name;
            _originalCharacter.Description = Description;
            _originalCharacter.Role = Role;
            _originalCharacter.Age = Age;
            _originalCharacter.PhysicalDescription = PhysicalDescription;
            _originalCharacter.Personality = Personality;
            _originalCharacter.Background = Background;
            _originalCharacter.Motivation = Motivation;
            _originalCharacter.InternalConflict = InternalConflict;
            _originalCharacter.ExternalConflict = ExternalConflict;
            _originalCharacter.CharacterArc = CharacterArc;
            _originalCharacter.Notes = Notes;
            _originalCharacter.ModifiedAt = DateTime.UtcNow;

            if (IsNewCharacter)
            {
                _project.Characters.Add(_originalCharacter);
                IsNewCharacter = false;
            }

            await _projectService.SaveProjectAsync(_project);
            
            _notificationService.ShowSuccess($"Character '{Name}' saved successfully.");
            _logger.LogInformation("Character {CharacterId} saved", _originalCharacter.Id);

            OnPropertyChanged(nameof(HasUnsavedChanges));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving character");
            _notificationService.ShowError($"Failed to save character: {ex.Message}");
        }
        finally
        {
            IsSaving = false;
        }
    }

    private bool CanSave() => !string.IsNullOrWhiteSpace(Name) && !IsSaving;

    [RelayCommand]
    private void RevertChanges()
    {
        if (_originalCharacter == null)
            return;

        LoadCharacter(_project!, _originalCharacter);
        _notificationService.ShowInfo("Changes reverted.");
    }

    private bool HasChanges()
    {
        if (_originalCharacter == null) return false;

        return Name != _originalCharacter.Name ||
               Description != _originalCharacter.Description ||
               Role != _originalCharacter.Role ||
               Age != _originalCharacter.Age ||
               PhysicalDescription != _originalCharacter.PhysicalDescription ||
               Personality != _originalCharacter.Personality ||
               Background != _originalCharacter.Background ||
               Motivation != _originalCharacter.Motivation ||
               InternalConflict != _originalCharacter.InternalConflict ||
               ExternalConflict != _originalCharacter.ExternalConflict ||
               CharacterArc != _originalCharacter.CharacterArc ||
               Notes != _originalCharacter.Notes;
    }
}
