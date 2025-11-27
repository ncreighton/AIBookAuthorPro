// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Text.Json.Serialization;
using AIBookAuthorPro.Core.Enums;

namespace AIBookAuthorPro.Core.Models;

/// <summary>
/// User settings and preferences for the application.
/// </summary>
public sealed class UserSettings
{
    /// <summary>
    /// Gets or sets the API key settings.
    /// </summary>
    public ApiKeySettings ApiKeys { get; set; } = new();

    /// <summary>
    /// Gets or sets the default generation settings.
    /// </summary>
    public DefaultGenerationSettings Generation { get; set; } = new();

    /// <summary>
    /// Gets or sets the editor settings.
    /// </summary>
    public EditorSettings Editor { get; set; } = new();

    /// <summary>
    /// Gets or sets the export settings.
    /// </summary>
    public UserExportSettings Export { get; set; } = new();

    /// <summary>
    /// Gets or sets the appearance settings.
    /// </summary>
    public AppearanceSettings Appearance { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of recent project paths.
    /// </summary>
    public List<string> RecentProjects { get; set; } = [];

    /// <summary>
    /// Gets or sets the maximum number of recent projects to track.
    /// </summary>
    public int MaxRecentProjects { get; set; } = 10;

    /// <summary>
    /// Gets or sets the last used project path.
    /// </summary>
    public string? LastProjectPath { get; set; }

    /// <summary>
    /// Gets or sets the window state settings.
    /// </summary>
    public WindowSettings Window { get; set; } = new();
}

/// <summary>
/// API key settings for AI providers.
/// </summary>
public sealed class ApiKeySettings
{
    [JsonIgnore]
    public string? AnthropicApiKey { get; set; }
    public string? AnthropicApiKeyEncrypted { get; set; }
    [JsonIgnore]
    public string? OpenAIApiKey { get; set; }
    public string? OpenAIApiKeyEncrypted { get; set; }
    [JsonIgnore]
    public string? GeminiApiKey { get; set; }
    public string? GeminiApiKeyEncrypted { get; set; }
}

/// <summary>
/// Default generation settings for new projects.
/// </summary>
public sealed class DefaultGenerationSettings
{
    public AIProviderType DefaultProvider { get; set; } = AIProviderType.Claude;
    public GenerationMode DefaultMode { get; set; } = GenerationMode.Standard;
    public double DefaultTemperature { get; set; } = 0.7;
    public bool IncludeCharacterContext { get; set; } = true;
    public bool IncludeLocationContext { get; set; } = true;
    public bool IncludePreviousSummary { get; set; } = true;
    public int DefaultChapterWordCount { get; set; } = 3000;
}

/// <summary>
/// Editor settings.
/// </summary>
public sealed class EditorSettings
{
    public string FontFamily { get; set; } = "Georgia";
    public double FontSize { get; set; } = 14;
    public double LineHeight { get; set; } = 1.8;
    public int AutoSaveIntervalSeconds { get; set; } = 60;
    public int MaxUndoHistory { get; set; } = 50;
    public bool SpellCheckEnabled { get; set; } = true;
    public bool ShowWordCount { get; set; } = true;
    public bool ShowReadingTime { get; set; } = true;
}

/// <summary>
/// User export settings (defaults).
/// </summary>
public sealed class UserExportSettings
{
    public ExportFormat DefaultFormat { get; set; } = ExportFormat.Docx;
    public bool IncludeTableOfContents { get; set; } = true;
    public bool IncludeChapterTitles { get; set; } = true;
    public bool ChapterPageBreaks { get; set; } = true;
    public string? DefaultOutputDirectory { get; set; }
    public bool EmbedFonts { get; set; } = true;
}

/// <summary>
/// Appearance settings.
/// </summary>
public sealed class AppearanceSettings
{
    public string Theme { get; set; } = "System";
    public string PrimaryColor { get; set; } = "Blue";
    public string AccentColor { get; set; } = "Amber";
    public bool CompactMode { get; set; } = false;
}

/// <summary>
/// Window state settings.
/// </summary>
public sealed class WindowSettings
{
    public double Width { get; set; } = 1400;
    public double Height { get; set; } = 900;
    public double Left { get; set; } = 100;
    public double Top { get; set; } = 100;
    public bool IsMaximized { get; set; } = false;
    public bool NavigationExpanded { get; set; } = true;
}
