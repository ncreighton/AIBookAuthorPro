// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Interfaces;
using AIBookAuthorPro.Core.Models;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.Infrastructure.Services;

/// <summary>
/// Service for managing user settings with encrypted API key storage.
/// </summary>
public sealed class SettingsService : ISettingsService
{
    private readonly ILogger<SettingsService> _logger;
    private readonly string _settingsDirectory;
    private readonly string _settingsFilePath;
    private readonly byte[] _entropy;

    private UserSettings _currentSettings = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <inheritdoc />
    public UserSettings CurrentSettings => _currentSettings;

    /// <inheritdoc />
    public string SettingsFilePath => _settingsFilePath;

    /// <inheritdoc />
    public event EventHandler<SettingsChangedEventArgs>? SettingsChanged;

    /// <summary>
    /// Initializes a new instance of the SettingsService.
    /// </summary>
    public SettingsService(ILogger<SettingsService> logger)
    {
        _logger = logger;

        // Settings stored in AppData/Roaming/AIBookAuthorPro
        _settingsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AIBookAuthorPro");

        _settingsFilePath = Path.Combine(_settingsDirectory, "settings.json");

        // Create entropy for DPAPI encryption (machine-specific)
        _entropy = Encoding.UTF8.GetBytes($"AIBookAuthorPro-{Environment.MachineName}");

        // Ensure directory exists
        Directory.CreateDirectory(_settingsDirectory);
    }

    /// <inheritdoc />
    public async Task<Result<UserSettings>> LoadSettingsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Loading settings from {Path}", _settingsFilePath);

            if (!File.Exists(_settingsFilePath))
            {
                _logger.LogInformation("Settings file not found, using defaults");
                _currentSettings = new UserSettings();
                return Result<UserSettings>.Success(_currentSettings);
            }

            var json = await File.ReadAllTextAsync(_settingsFilePath, cancellationToken);
            var settings = JsonSerializer.Deserialize<UserSettings>(json, JsonOptions);

            if (settings == null)
            {
                _logger.LogWarning("Failed to deserialize settings, using defaults");
                _currentSettings = new UserSettings();
                return Result<UserSettings>.Success(_currentSettings);
            }

            // Decrypt API keys
            DecryptApiKeys(settings);

            _currentSettings = settings;
            _logger.LogInformation("Settings loaded successfully");

            return Result<UserSettings>.Success(_currentSettings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load settings");
            _currentSettings = new UserSettings();
            return Result<UserSettings>.Failure($"Failed to load settings: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result> SaveSettingsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Saving settings to {Path}", _settingsFilePath);

            // Create a copy for saving with encrypted keys
            var settingsToSave = CloneSettings(_currentSettings);
            EncryptApiKeys(settingsToSave);

            var json = JsonSerializer.Serialize(settingsToSave, JsonOptions);
            await File.WriteAllTextAsync(_settingsFilePath, json, cancellationToken);

            _logger.LogInformation("Settings saved successfully");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save settings");
            return Result.Failure($"Failed to save settings: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public void ResetToDefaults()
    {
        _logger.LogInformation("Resetting settings to defaults");
        _currentSettings = new UserSettings();
        SettingsChanged?.Invoke(this, new SettingsChangedEventArgs { Section = "All" });
    }

    /// <inheritdoc />
    public string? GetApiKey(string providerName)
    {
        return providerName.ToLowerInvariant() switch
        {
            "anthropic" or "claude" => _currentSettings.ApiKeys.AnthropicApiKey,
            "openai" => _currentSettings.ApiKeys.OpenAIApiKey,
            "gemini" or "google" => _currentSettings.ApiKeys.GeminiApiKey,
            _ => null
        };
    }

    /// <inheritdoc />
    public void SetApiKey(string providerName, string? apiKey)
    {
        switch (providerName.ToLowerInvariant())
        {
            case "anthropic":
            case "claude":
                _currentSettings.ApiKeys.AnthropicApiKey = apiKey;
                break;
            case "openai":
                _currentSettings.ApiKeys.OpenAIApiKey = apiKey;
                break;
            case "gemini":
            case "google":
                _currentSettings.ApiKeys.GeminiApiKey = apiKey;
                break;
        }

        SettingsChanged?.Invoke(this, new SettingsChangedEventArgs
        {
            Section = "ApiKeys",
            PropertyName = providerName
        });
    }

    /// <inheritdoc />
    public void AddRecentProject(string projectPath)
    {
        if (string.IsNullOrWhiteSpace(projectPath)) return;

        // Remove if already exists (to move to top)
        _currentSettings.RecentProjects.Remove(projectPath);

        // Add to beginning
        _currentSettings.RecentProjects.Insert(0, projectPath);

        // Trim to max
        while (_currentSettings.RecentProjects.Count > _currentSettings.MaxRecentProjects)
        {
            _currentSettings.RecentProjects.RemoveAt(_currentSettings.RecentProjects.Count - 1);
        }

        _currentSettings.LastProjectPath = projectPath;

        SettingsChanged?.Invoke(this, new SettingsChangedEventArgs
        {
            Section = "RecentProjects"
        });
    }

    /// <inheritdoc />
    public void RemoveRecentProject(string projectPath)
    {
        _currentSettings.RecentProjects.Remove(projectPath);

        SettingsChanged?.Invoke(this, new SettingsChangedEventArgs
        {
            Section = "RecentProjects"
        });
    }

    #region Private Methods

    private void EncryptApiKeys(UserSettings settings)
    {
        settings.ApiKeys.AnthropicApiKeyEncrypted = EncryptString(settings.ApiKeys.AnthropicApiKey);
        settings.ApiKeys.OpenAIApiKeyEncrypted = EncryptString(settings.ApiKeys.OpenAIApiKey);
        settings.ApiKeys.GeminiApiKeyEncrypted = EncryptString(settings.ApiKeys.GeminiApiKey);

        // Clear plain text keys
        settings.ApiKeys.AnthropicApiKey = null;
        settings.ApiKeys.OpenAIApiKey = null;
        settings.ApiKeys.GeminiApiKey = null;
    }

    private void DecryptApiKeys(UserSettings settings)
    {
        settings.ApiKeys.AnthropicApiKey = DecryptString(settings.ApiKeys.AnthropicApiKeyEncrypted);
        settings.ApiKeys.OpenAIApiKey = DecryptString(settings.ApiKeys.OpenAIApiKeyEncrypted);
        settings.ApiKeys.GeminiApiKey = DecryptString(settings.ApiKeys.GeminiApiKeyEncrypted);
    }

    private string? EncryptString(string? plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return null;

        try
        {
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var encryptedBytes = ProtectedData.Protect(plainBytes, _entropy, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedBytes);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to encrypt string");
            return null;
        }
    }

    private string? DecryptString(string? encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText)) return null;

        try
        {
            var encryptedBytes = Convert.FromBase64String(encryptedText);
            var plainBytes = ProtectedData.Unprotect(encryptedBytes, _entropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(plainBytes);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to decrypt string");
            return null;
        }
    }

    private static UserSettings CloneSettings(UserSettings source)
    {
        var json = JsonSerializer.Serialize(source, JsonOptions);
        return JsonSerializer.Deserialize<UserSettings>(json, JsonOptions) ?? new UserSettings();
    }

    #endregion
}
