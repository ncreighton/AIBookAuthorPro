// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Collections.ObjectModel;
using AIBookAuthorPro.Core.Enums;
using AIBookAuthorPro.Core.Interfaces;
using AIBookAuthorPro.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.UI.ViewModels;

/// <summary>
/// ViewModel for editing a single location.
/// </summary>
public partial class LocationEditorViewModel : ObservableObject
{
    private readonly IProjectService _projectService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<LocationEditorViewModel> _logger;

    private Project? _project;
    private Location? _originalLocation;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUnsavedChanges))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _name = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUnsavedChanges))]
    private string _description = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUnsavedChanges))]
    private LocationType _locationType = LocationType.Other;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUnsavedChanges))]
    private string _atmosphere = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUnsavedChanges))]
    private string _sensoryDetails = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUnsavedChanges))]
    private string _history = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUnsavedChanges))]
    private string _notes = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUnsavedChanges))]
    private Guid? _parentLocationId;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private bool _isNewLocation;

    public ObservableCollection<LocationType> AvailableLocationTypes { get; } = new(Enum.GetValues<LocationType>());
    
    public ObservableCollection<Location> AvailableParentLocations { get; } = new();

    public bool HasUnsavedChanges => _originalLocation != null && HasChanges();

    public LocationEditorViewModel(
        IProjectService projectService,
        INotificationService notificationService,
        ILogger<LocationEditorViewModel> logger)
    {
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Loads a location for editing.
    /// </summary>
    public void LoadLocation(Project project, Location location)
    {
        _project = project;
        _originalLocation = location;
        IsNewLocation = false;

        // Update available parent locations (exclude current location)
        UpdateAvailableParentLocations(location.Id);

        Name = location.Name;
        Description = location.Description;
        LocationType = location.Type;
        Atmosphere = location.Atmosphere;
        SensoryDetails = location.SensoryDetails;
        History = location.History ?? string.Empty;
        Notes = location.Notes;
        ParentLocationId = location.ParentLocationId;
    }

    /// <summary>
    /// Creates a new location for editing.
    /// </summary>
    public void CreateNewLocation(Project project)
    {
        _project = project;
        _originalLocation = new Location { Name = "New Location" };
        IsNewLocation = true;

        // Update available parent locations
        UpdateAvailableParentLocations(null);

        Name = string.Empty;
        Description = string.Empty;
        LocationType = LocationType.Other;
        Atmosphere = string.Empty;
        SensoryDetails = string.Empty;
        History = string.Empty;
        Notes = string.Empty;
        ParentLocationId = null;
    }

    private void UpdateAvailableParentLocations(Guid? excludeId)
    {
        AvailableParentLocations.Clear();
        
        if (_project == null) return;

        foreach (var location in _project.Locations)
        {
            if (excludeId == null || location.Id != excludeId)
            {
                AvailableParentLocations.Add(location);
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        if (_project == null || _originalLocation == null)
            return;

        IsSaving = true;

        try
        {
            _originalLocation.Name = Name;
            _originalLocation.Description = Description;
            _originalLocation.Type = LocationType;
            _originalLocation.Atmosphere = Atmosphere;
            _originalLocation.SensoryDetails = SensoryDetails;
            _originalLocation.History = History;
            _originalLocation.Notes = Notes;
            _originalLocation.ParentLocationId = ParentLocationId;

            if (IsNewLocation)
            {
                _project.AddLocation(_originalLocation);
                IsNewLocation = false;
            }

            await _projectService.SaveAsync(_project);
            
            _notificationService.ShowSuccess($"Location '{Name}' saved successfully.");
            _logger.LogInformation("Location {LocationId} saved", _originalLocation.Id);

            OnPropertyChanged(nameof(HasUnsavedChanges));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving location");
            _notificationService.ShowError($"Failed to save location: {ex.Message}");
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
        if (_originalLocation == null)
            return;

        LoadLocation(_project!, _originalLocation);
        _notificationService.ShowInfo("Changes reverted.");
    }

    private bool HasChanges()
    {
        if (_originalLocation == null) return false;

        return Name != _originalLocation.Name ||
               Description != _originalLocation.Description ||
               LocationType != _originalLocation.Type ||
               Atmosphere != _originalLocation.Atmosphere ||
               SensoryDetails != _originalLocation.SensoryDetails ||
               History != (_originalLocation.History ?? string.Empty) ||
               Notes != _originalLocation.Notes ||
               ParentLocationId != _originalLocation.ParentLocationId;
    }
}
