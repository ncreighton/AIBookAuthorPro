// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Models;

namespace AIBookAuthorPro.Core.Interfaces;

/// <summary>
/// Service for managing user settings.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Gets the current settings.
    /// </summary>
    UserSettings CurrentSettings { get; }

    /// <summary>
    /// Loads settings from storage.
    /// </summary>
    Task<Result<UserSettings>> LoadSettingsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves current settings to storage.
    /// </summary>
    Task<Result> SaveSettingsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets settings to defaults.
    /// </summary>
    void ResetToDefaults();

    /// <summary>
    /// Gets a decrypted API key.
    /// </summary>
    string? GetApiKey(string providerName);

    /// <summary>
    /// Sets and encrypts an API key.
    /// </summary>
    void SetApiKey(string providerName, string? apiKey);

    /// <summary>
    /// Adds a project to recent projects list.
    /// </summary>
    void AddRecentProject(string projectPath);

    /// <summary>
    /// Removes a project from recent projects list.
    /// </summary>
    void RemoveRecentProject(string projectPath);

    /// <summary>
    /// Gets the settings file path.
    /// </summary>
    string SettingsFilePath { get; }

    /// <summary>
    /// Event raised when settings change.
    /// </summary>
    event EventHandler<SettingsChangedEventArgs>? SettingsChanged;
}

/// <summary>
/// Event args for settings changes.
/// </summary>
public sealed class SettingsChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the name of the changed setting section.
    /// </summary>
    public string Section { get; init; } = string.Empty;

    /// <summary>
    /// Gets the name of the changed property.
    /// </summary>
    public string? PropertyName { get; init; }
}
