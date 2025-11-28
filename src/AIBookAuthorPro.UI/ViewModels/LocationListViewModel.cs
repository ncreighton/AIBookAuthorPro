// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Collections.ObjectModel;
using AIBookAuthorPro.Core.Interfaces;
using AIBookAuthorPro.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.UI.ViewModels;

/// <summary>
/// ViewModel for the location list view.
/// </summary>
public partial class LocationListViewModel : ObservableObject
{
    private readonly IProjectService _projectService;
    private readonly ILogger<LocationListViewModel> _logger;

    [ObservableProperty]
    private Project? _project;

    [ObservableProperty]
    private ObservableCollection<Location> _locations = [];

    [ObservableProperty]
    private ObservableCollection<Location> _filteredLocations = [];

    [ObservableProperty]
    private Location? _selectedLocation;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "Locations";

    /// <summary>
    /// Event raised when a location should be edited.
    /// </summary>
    public event EventHandler<Location>? EditLocationRequested;

    /// <summary>
    /// Event raised when navigation back is requested.
    /// </summary>
    public event EventHandler? BackRequested;

    /// <summary>
    /// Initializes a new instance of LocationListViewModel.
    /// </summary>
    public LocationListViewModel(
        IProjectService projectService,
        ILogger<LocationListViewModel> logger)
    {
        _projectService = projectService;
        _logger = logger;
    }

    /// <summary>
    /// Loads locations from the project.
    /// </summary>
    public void LoadProject(Project project)
    {
        ArgumentNullException.ThrowIfNull(project);

        _logger.LogDebug("Loading locations for project {ProjectName}", project.Name);

        Project = project;
        RefreshLocationList();
        StatusMessage = $"{project.Locations.Count} locations";
    }

    /// <summary>
    /// Refreshes the location list.
    /// </summary>
    public void RefreshLocationList()
    {
        if (Project == null) return;

        Locations.Clear();
        foreach (var location in Project.Locations.OrderBy(l => l.Name))
        {
            Locations.Add(location);
        }

        ApplyFilters();
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        FilteredLocations.Clear();

        var filtered = Locations.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(l =>
                l.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                (l.Description?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (l.Type?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        foreach (var location in filtered)
        {
            FilteredLocations.Add(location);
        }
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = string.Empty;
    }

    [RelayCommand]
    private void CreateNewLocation()
    {
        if (Project == null) return;

        var newLocation = new Location
        {
            Id = Guid.NewGuid(),
            Name = "New Location",
            Type = "General",
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        Project.Locations.Add(newLocation);
        _projectService.MarkAsModified();

        RefreshLocationList();
        SelectedLocation = newLocation;

        EditLocationRequested?.Invoke(this, newLocation);

        _logger.LogInformation("Created new location");
    }

    [RelayCommand]
    private void EditLocation(Location? location)
    {
        if (location == null) return;

        _logger.LogDebug("Editing location {LocationName}", location.Name);
        EditLocationRequested?.Invoke(this, location);
    }

    [RelayCommand]
    private async Task DeleteLocationAsync(Location? location)
    {
        if (location == null || Project == null) return;

        _logger.LogDebug("Deleting location {LocationName}", location.Name);

        Project.Locations.Remove(location);
        _projectService.MarkAsModified();
        await _projectService.SaveProjectAsync();

        RefreshLocationList();
        StatusMessage = $"Deleted: {location.Name}";
    }

    [RelayCommand]
    private void DuplicateLocation(Location? location)
    {
        if (location == null || Project == null) return;

        var duplicate = new Location
        {
            Id = Guid.NewGuid(),
            Name = $"{location.Name} (Copy)",
            Type = location.Type,
            Description = location.Description,
            Significance = location.Significance,
            SensoryDetails = location.SensoryDetails,
            Notes = location.Notes,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        Project.Locations.Add(duplicate);
        _projectService.MarkAsModified();

        RefreshLocationList();
        StatusMessage = $"Duplicated: {location.Name}";
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
            StatusMessage = "All locations saved";
        }
        else
        {
            StatusMessage = $"Save failed: {result.Error}";
        }
    }
}
    private readonly IProjectService _projectService;
    private readonly IGenerationPipelineService _generationService;
    private readonly ILogger<LocationEditorViewModel> _logger;

    [ObservableProperty]
    private Project? _project;

    [ObservableProperty]
    private Location? _location;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string? _type;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private string? _significance;

    [ObservableProperty]
    private string? _sensoryDetails;

    [ObservableProperty]
    private string? _notes;

    [ObservableProperty]
    private bool _isModified;

    [ObservableProperty]
    private bool _isGenerating;

    [ObservableProperty]
    private string _statusMessage = "Edit Location";

    /// <summary>
    /// Gets the common location types.
    /// </summary>
    public string[] CommonTypes { get; } =
    [
        "City",
        "Town",
        "Village",
        "Building",
        "House",
        "Room",
        "Forest",
        "Mountain",
        "Ocean",
        "Desert",
        "Cave",
        "Castle",
        "Ship",
        "Spaceship",
        "Planet",
        "Virtual",
        "Other"
    ];

    /// <summary>
    /// Event raised when editing is complete.
    /// </summary>
    public event EventHandler? EditingComplete;

    /// <summary>
    /// Initializes a new instance of LocationEditorViewModel.
    /// </summary>
    public LocationEditorViewModel(
        IProjectService projectService,
        IGenerationPipelineService generationService,
        ILogger<LocationEditorViewModel> logger)
    {
        _projectService = projectService;
        _generationService = generationService;
        _logger = logger;
    }

    /// <summary>
    /// Loads a location for editing.
    /// </summary>
    public void LoadLocation(Project project, Location location)
    {
        Project = project;
        Location = location;

        // Load location data
        Name = location.Name;
        Type = location.Type;
        Description = location.Description;
        Significance = location.Significance;
        SensoryDetails = location.SensoryDetails;
        Notes = location.Notes;

        IsModified = false;
        StatusMessage = $"Editing: {location.Name}";
    }

    // Track modifications
    partial void OnNameChanged(string value) => IsModified = true;
    partial void OnTypeChanged(string? value) => IsModified = true;
    partial void OnDescriptionChanged(string? value) => IsModified = true;
    partial void OnSignificanceChanged(string? value) => IsModified = true;
    partial void OnSensoryDetailsChanged(string? value) => IsModified = true;

    [RelayCommand]
    private async Task SaveLocationAsync()
    {
        if (Location == null) return;

        _logger.LogDebug("Saving location {LocationName}", Name);

        // Update location from properties
        Location.Name = Name;
        Location.Type = Type;
        Location.Description = Description;
        Location.Significance = Significance;
        Location.SensoryDetails = SensoryDetails;
        Location.Notes = Notes;
        Location.MarkAsModified();

        _projectService.MarkAsModified();
        await _projectService.SaveProjectAsync();

        IsModified = false;
        StatusMessage = "Location saved";
    }

    [RelayCommand]
    private async Task GenerateDescriptionAsync(string field)
    {
        if (Location == null || Project == null) return;

        IsGenerating = true;
        StatusMessage = $"Generating {field}...";

        try
        {
            var prompt = field switch
            {
                "description" => $"Generate a vivid description of a {Type ?? "location"} called \"{Name}\" for a {Project.Metadata?.Genre} story. Focus on visual details and atmosphere.",
                "sensory" => $"Generate sensory details (sights, sounds, smells, textures) for a {Type ?? "location"} called \"{Name}\" in a {Project.Metadata?.Genre} story.",
                "significance" => $"Explain the narrative significance of a {Type ?? "location"} called \"{Name}\" in a {Project.Metadata?.Genre} story. How might it affect the plot or characters?",
                _ => $"Describe the location {Name}"
            };

            // For now, just show what would be generated
            StatusMessage = $"AI generation would use prompt: {prompt[..Math.Min(50, prompt.Length)]}...";
        }
        finally
        {
            IsGenerating = false;
        }
    }

    [RelayCommand]
    private async Task SaveAndCloseAsync()
    {
        await SaveLocationAsync();
        EditingComplete?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void Cancel()
    {
        EditingComplete?.Invoke(this, EventArgs.Empty);
    }
}
