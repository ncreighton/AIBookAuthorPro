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
    /// Gets or sets the generation settings.
    /// </summary>
    public GenerationSettings Generation { get; set; } = new();

    /// <summary>
    /// Gets or sets the editor settings.
    /// </summary>
    public EditorSettings Editor { get; set; } = new();

    /// <summary>
    /// Gets or sets the export settings.
    /// </summary>
    public ExportSettings Export { get; set; } = new();

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
    /// <summary>
    /// Gets or sets the Anthropic (Claude) API key.
    /// </summary>
    [JsonIgnore]
    public string? AnthropicApiKey { get; set; }

    /// <summary>
    /// Gets or sets the encrypted Anthropic API key for storage.
    /// </summary>
    public string? AnthropicApiKeyEncrypted { get; set; }

    /// <summary>
    /// Gets or sets the OpenAI API key.
    /// </summary>
    [JsonIgnore]
    public string? OpenAIApiKey { get; set; }

    /// <summary>
    /// Gets or sets the encrypted OpenAI API key for storage.
    /// </summary>
    public string? OpenAIApiKeyEncrypted { get; set; }

    /// <summary>
    /// Gets or sets the Google (Gemini) API key.
    /// </summary>
    [JsonIgnore]
    public string? GeminiApiKey { get; set; }

    /// <summary>
    /// Gets or sets the encrypted Gemini API key for storage.
    /// </summary>
    public string? GeminiApiKeyEncrypted { get; set; }
}

/// <summary>
/// Default generation settings.
/// </summary>
public sealed class GenerationSettings
{
    /// <summary>
    /// Gets or sets the default AI provider.
    /// </summary>
    public AIProviderType DefaultProvider { get; set; } = AIProviderType.Claude;

    /// <summary>
    /// Gets or sets the default generation mode.
    /// </summary>
    public GenerationMode DefaultMode { get; set; } = GenerationMode.Standard;

    /// <summary>
    /// Gets or sets the default temperature.
    /// </summary>
    public double DefaultTemperature { get; set; } = 0.7;

    /// <summary>
    /// Gets or sets whether to include character context by default.
    /// </summary>
    public bool IncludeCharacterContext { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to include location context by default.
    /// </summary>
    public bool IncludeLocationContext { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to include previous chapter summary by default.
    /// </summary>
    public bool IncludePreviousSummary { get; set; } = true;

    /// <summary>
    /// Gets or sets the default target word count per chapter.
    /// </summary>
    public int DefaultChapterWordCount { get; set; } = 3000;
}

/// <summary>
/// Editor settings.
/// </summary>
public sealed class EditorSettings
{
    /// <summary>
    /// Gets or sets the editor font family.
    /// </summary>
    public string FontFamily { get; set; } = "Georgia";

    /// <summary>
    /// Gets or sets the editor font size.
    /// </summary>
    public double FontSize { get; set; } = 14;

    /// <summary>
    /// Gets or sets the line height multiplier.
    /// </summary>
    public double LineHeight { get; set; } = 1.8;

    /// <summary>
    /// Gets or sets the auto-save interval in seconds (0 to disable).
    /// </summary>
    public int AutoSaveIntervalSeconds { get; set; } = 60;

    /// <summary>
    /// Gets or sets the maximum undo history size.
    /// </summary>
    public int MaxUndoHistory { get; set; } = 50;

    /// <summary>
    /// Gets or sets whether spell check is enabled.
    /// </summary>
    public bool SpellCheckEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to show word count in status bar.
    /// </summary>
    public bool ShowWordCount { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to show reading time estimate.
    /// </summary>
    public bool ShowReadingTime { get; set; } = true;
}

/// <summary>
/// Export settings.
/// </summary>
public sealed class ExportSettings
{
    /// <summary>
    /// Gets or sets the default export format.
    /// </summary>
    public ExportFormat DefaultFormat { get; set; } = ExportFormat.Docx;

    /// <summary>
    /// Gets or sets whether to include table of contents.
    /// </summary>
    public bool IncludeTableOfContents { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to include chapter titles.
    /// </summary>
    public bool IncludeChapterTitles { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to start each chapter on a new page.
    /// </summary>
    public bool ChapterPageBreaks { get; set; } = true;

    /// <summary>
    /// Gets or sets the default output directory.
    /// </summary>
    public string? DefaultOutputDirectory { get; set; }

    /// <summary>
    /// Gets or sets whether to embed fonts in output.
    /// </summary>
    public bool EmbedFonts { get; set; } = true;
}

/// <summary>
/// Appearance settings.
/// </summary>
public sealed class AppearanceSettings
{
    /// <summary>
    /// Gets or sets the theme (Light, Dark, System).
    /// </summary>
    public string Theme { get; set; } = "System";

    /// <summary>
    /// Gets or sets the primary color.
    /// </summary>
    public string PrimaryColor { get; set; } = "Blue";

    /// <summary>
    /// Gets or sets the accent color.
    /// </summary>
    public string AccentColor { get; set; } = "Amber";

    /// <summary>
    /// Gets or sets whether to use compact mode.
    /// </summary>
    public bool CompactMode { get; set; } = false;
}

/// <summary>
/// Window state settings.
/// </summary>
public sealed class WindowSettings
{
    /// <summary>
    /// Gets or sets the window width.
    /// </summary>
    public double Width { get; set; } = 1400;

    /// <summary>
    /// Gets or sets the window height.
    /// </summary>
    public double Height { get; set; } = 900;

    /// <summary>
    /// Gets or sets the window left position.
    /// </summary>
    public double Left { get; set; } = 100;

    /// <summary>
    /// Gets or sets the window top position.
    /// </summary>
    public double Top { get; set; } = 100;

    /// <summary>
    /// Gets or sets whether the window is maximized.
    /// </summary>
    public bool IsMaximized { get; set; } = false;

    /// <summary>
    /// Gets or sets the navigation rail expanded state.
    /// </summary>
    public bool NavigationExpanded { get; set; } = true;
}
