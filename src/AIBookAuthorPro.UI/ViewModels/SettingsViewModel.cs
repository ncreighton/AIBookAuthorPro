// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Collections.ObjectModel;
using System.Windows.Forms;
using AIBookAuthorPro.Core.Enums;
using AIBookAuthorPro.Core.Interfaces;
using AIBookAuthorPro.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.UI.ViewModels;

/// <summary>
/// ViewModel for the settings view.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly IAIProviderFactory _providerFactory;
    private readonly ILogger<SettingsViewModel> _logger;

    // API Keys
    [ObservableProperty]
    private string _anthropicApiKey = string.Empty;

    [ObservableProperty]
    private string _openAIApiKey = string.Empty;

    [ObservableProperty]
    private string _geminiApiKey = string.Empty;

    [ObservableProperty]
    private bool _anthropicConfigured;

    [ObservableProperty]
    private bool _openAIConfigured;

    [ObservableProperty]
    private bool _geminiConfigured;

    [ObservableProperty]
    private string? _testingProvider;

    [ObservableProperty]
    private string? _testResult;

    // Generation Settings
    [ObservableProperty]
    private AIProviderType _defaultProvider = AIProviderType.Claude;

    [ObservableProperty]
    private GenerationMode _defaultMode = GenerationMode.Standard;

    [ObservableProperty]
    private double _defaultTemperature = 0.7;

    [ObservableProperty]
    private bool _includeCharacterContext = true;

    [ObservableProperty]
    private bool _includeLocationContext = true;

    [ObservableProperty]
    private bool _includePreviousSummary = true;

    [ObservableProperty]
    private int _defaultChapterWordCount = 3000;

    // Editor Settings
    [ObservableProperty]
    private string _fontFamily = "Georgia";

    [ObservableProperty]
    private double _fontSize = 14;

    [ObservableProperty]
    private double _lineHeight = 1.8;

    [ObservableProperty]
    private int _autoSaveInterval = 60;

    [ObservableProperty]
    private int _maxUndoHistory = 50;

    [ObservableProperty]
    private bool _spellCheckEnabled = true;

    [ObservableProperty]
    private bool _showWordCount = true;

    [ObservableProperty]
    private bool _showReadingTime = true;

    // Export Settings
    [ObservableProperty]
    private ExportFormat _defaultExportFormat = ExportFormat.Docx;

    [ObservableProperty]
    private bool _includeTableOfContents = true;

    [ObservableProperty]
    private bool _includeChapterTitles = true;

    [ObservableProperty]
    private bool _chapterPageBreaks = true;

    [ObservableProperty]
    private string? _defaultOutputDirectory;

    [ObservableProperty]
    private bool _embedFonts = true;

    // Appearance Settings
    [ObservableProperty]
    private string _theme = "System";

    [ObservableProperty]
    private string _primaryColor = "Blue";

    [ObservableProperty]
    private string _accentColor = "Amber";

    [ObservableProperty]
    private bool _compactMode;

    // State
    [ObservableProperty]
    private bool _hasChanges;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private string _statusMessage = "Settings";

    [ObservableProperty]
    private int _selectedTabIndex;

    /// <summary>
    /// Gets the available font families.
    /// </summary>
    public ObservableCollection<string> AvailableFonts { get; } =
    [
        "Georgia",
        "Cambria",
        "Times New Roman",
        "Palatino Linotype",
        "Garamond",
        "Book Antiqua",
        "Segoe UI",
        "Calibri",
        "Arial"
    ];

    /// <summary>
    /// Gets the available themes.
    /// </summary>
    public ObservableCollection<string> AvailableThemes { get; } =
    [
        "Light",
        "Dark",
        "System"
    ];

    /// <summary>
    /// Gets the available colors.
    /// </summary>
    public ObservableCollection<string> AvailableColors { get; } =
    [
        "Blue",
        "Indigo",
        "Purple",
        "Teal",
        "Green",
        "Orange",
        "Red",
        "Pink"
    ];

    /// <summary>
    /// Gets the available providers.
    /// </summary>
    public ObservableCollection<AIProviderType> AvailableProviders { get; } =
    [
        AIProviderType.Claude,
        AIProviderType.OpenAI,
        AIProviderType.Ollama
    ];

    /// <summary>
    /// Gets the available generation modes.
    /// </summary>
    public ObservableCollection<GenerationMode> AvailableModes { get; } =
    [
        GenerationMode.Fast,
        GenerationMode.Standard,
        GenerationMode.HighQuality
    ];

    /// <summary>
    /// Gets the available export formats.
    /// </summary>
    public ObservableCollection<ExportFormat> AvailableExportFormats { get; } =
    [
        ExportFormat.Docx,
        ExportFormat.Pdf,
        ExportFormat.Epub,
        ExportFormat.Markdown,
        ExportFormat.Html
    ];

    /// <summary>
    /// Event raised when settings should close.
    /// </summary>
    public event EventHandler? CloseRequested;

    /// <summary>
    /// Initializes a new instance of the SettingsViewModel.
    /// </summary>
    public SettingsViewModel(
        ISettingsService settingsService,
        IAIProviderFactory providerFactory,
        ILogger<SettingsViewModel> logger)
    {
        _settingsService = settingsService;
        _providerFactory = providerFactory;
        _logger = logger;
    }

    /// <summary>
    /// Loads settings into the view model.
    /// </summary>
    public void LoadSettings()
    {
        _logger.LogDebug("Loading settings into view model");

        var settings = _settingsService.CurrentSettings;

        // API Keys (masked, check if configured)
        AnthropicApiKey = MaskApiKey(_settingsService.GetApiKey("anthropic"));
        OpenAIApiKey = MaskApiKey(_settingsService.GetApiKey("openai"));
        GeminiApiKey = MaskApiKey(_settingsService.GetApiKey("gemini"));

        AnthropicConfigured = !string.IsNullOrEmpty(_settingsService.GetApiKey("anthropic"));
        OpenAIConfigured = !string.IsNullOrEmpty(_settingsService.GetApiKey("openai"));
        GeminiConfigured = !string.IsNullOrEmpty(_settingsService.GetApiKey("gemini"));

        // Generation
        DefaultProvider = settings.Generation.DefaultProvider;
        DefaultMode = settings.Generation.DefaultMode;
        DefaultTemperature = settings.Generation.DefaultTemperature;
        IncludeCharacterContext = settings.Generation.IncludeCharacterContext;
        IncludeLocationContext = settings.Generation.IncludeLocationContext;
        IncludePreviousSummary = settings.Generation.IncludePreviousSummary;
        DefaultChapterWordCount = settings.Generation.DefaultChapterWordCount;

        // Editor
        FontFamily = settings.Editor.FontFamily;
        FontSize = settings.Editor.FontSize;
        LineHeight = settings.Editor.LineHeight;
        AutoSaveInterval = settings.Editor.AutoSaveIntervalSeconds;
        MaxUndoHistory = settings.Editor.MaxUndoHistory;
        SpellCheckEnabled = settings.Editor.SpellCheckEnabled;
        ShowWordCount = settings.Editor.ShowWordCount;
        ShowReadingTime = settings.Editor.ShowReadingTime;

        // Export
        DefaultExportFormat = settings.Export.DefaultFormat;
        IncludeTableOfContents = settings.Export.IncludeTableOfContents;
        IncludeChapterTitles = settings.Export.IncludeChapterTitles;
        ChapterPageBreaks = settings.Export.ChapterPageBreaks;
        DefaultOutputDirectory = settings.Export.DefaultOutputDirectory;
        EmbedFonts = settings.Export.EmbedFonts;

        // Appearance
        Theme = settings.Appearance.Theme;
        PrimaryColor = settings.Appearance.PrimaryColor;
        AccentColor = settings.Appearance.AccentColor;
        CompactMode = settings.Appearance.CompactMode;

        HasChanges = false;
        StatusMessage = "Settings loaded";
    }

    private static string MaskApiKey(string? key)
    {
        if (string.IsNullOrEmpty(key)) return string.Empty;
        if (key.Length <= 8) return new string('•', key.Length);
        return key[..4] + new string('•', key.Length - 8) + key[^4..];
    }

    // Track changes
    partial void OnDefaultProviderChanged(AIProviderType value) => HasChanges = true;
    partial void OnDefaultModeChanged(GenerationMode value) => HasChanges = true;
    partial void OnDefaultTemperatureChanged(double value) => HasChanges = true;
    partial void OnFontFamilyChanged(string value) => HasChanges = true;
    partial void OnFontSizeChanged(double value) => HasChanges = true;
    partial void OnThemeChanged(string value) => HasChanges = true;

    [RelayCommand]
    private void SetApiKey(string provider)
    {
        // In a real app, show a secure input dialog
        // For now, just mark as needing input
        _logger.LogDebug("Setting API key for {Provider}", provider);

        // This would show a dialog to input the API key
        // For demonstration, we'll just log it
        StatusMessage = $"Enter API key for {provider}...";
    }

    [RelayCommand]
    private async Task TestApiKeyAsync(string provider)
    {
        TestingProvider = provider;
        TestResult = null;

        try
        {
            _logger.LogDebug("Testing API key for {Provider}", provider);

            var providerType = provider.ToLowerInvariant() switch
            {
                "anthropic" => AIProviderType.Claude,
                "openai" => AIProviderType.OpenAI,
                "gemini" => AIProviderType.Ollama,
                _ => AIProviderType.Claude
            };

            var aiProvider = _providerFactory.GetProvider(providerType);

            if (!aiProvider.IsConfigured)
            {
                TestResult = "Not configured";
                return;
            }

            // Simple test - just check if we can get models
            var models = aiProvider.GetAvailableModels();

            if (models.Count > 0)
            {
                TestResult = $"✓ Connected ({models.Count} models)";
            }
            else
            {
                TestResult = "✓ Connected";
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "API key test failed for {Provider}", provider);
            TestResult = $"✗ Failed: {ex.Message}";
        }
        finally
        {
            TestingProvider = null;
        }
    }

    [RelayCommand]
    private void BrowseOutputDirectory()
    {
        var dialog = new FolderBrowserDialog
        {
            Description = "Select default export directory",
            UseDescriptionForTitle = true
        };

        if (!string.IsNullOrEmpty(DefaultOutputDirectory))
        {
            dialog.SelectedPath = DefaultOutputDirectory;
        }

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            DefaultOutputDirectory = dialog.SelectedPath;
            HasChanges = true;
        }
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        IsSaving = true;
        StatusMessage = "Saving settings...";

        try
        {
            _logger.LogDebug("Saving settings");

            var settings = _settingsService.CurrentSettings;

            // Update settings object
            settings.Generation.DefaultProvider = DefaultProvider;
            settings.Generation.DefaultMode = DefaultMode;
            settings.Generation.DefaultTemperature = DefaultTemperature;
            settings.Generation.IncludeCharacterContext = IncludeCharacterContext;
            settings.Generation.IncludeLocationContext = IncludeLocationContext;
            settings.Generation.IncludePreviousSummary = IncludePreviousSummary;
            settings.Generation.DefaultChapterWordCount = DefaultChapterWordCount;

            settings.Editor.FontFamily = FontFamily;
            settings.Editor.FontSize = FontSize;
            settings.Editor.LineHeight = LineHeight;
            settings.Editor.AutoSaveIntervalSeconds = AutoSaveInterval;
            settings.Editor.MaxUndoHistory = MaxUndoHistory;
            settings.Editor.SpellCheckEnabled = SpellCheckEnabled;
            settings.Editor.ShowWordCount = ShowWordCount;
            settings.Editor.ShowReadingTime = ShowReadingTime;

            settings.Export.DefaultFormat = DefaultExportFormat;
            settings.Export.IncludeTableOfContents = IncludeTableOfContents;
            settings.Export.IncludeChapterTitles = IncludeChapterTitles;
            settings.Export.ChapterPageBreaks = ChapterPageBreaks;
            settings.Export.DefaultOutputDirectory = DefaultOutputDirectory;
            settings.Export.EmbedFonts = EmbedFonts;

            settings.Appearance.Theme = Theme;
            settings.Appearance.PrimaryColor = PrimaryColor;
            settings.Appearance.AccentColor = AccentColor;
            settings.Appearance.CompactMode = CompactMode;

            var result = await _settingsService.SaveSettingsAsync();

            if (result.IsSuccess)
            {
                HasChanges = false;
                StatusMessage = "Settings saved successfully";
                _logger.LogInformation("Settings saved");
            }
            else
            {
                StatusMessage = $"Failed to save: {result.Error}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save settings");
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private void ResetToDefaults()
    {
        _logger.LogDebug("Resetting settings to defaults");

        _settingsService.ResetToDefaults();
        LoadSettings();
        HasChanges = true;
        StatusMessage = "Reset to defaults (save to apply)";
    }

    [RelayCommand]
    private void Cancel()
    {
        if (HasChanges)
        {
            // In a real app, show confirmation dialog
            _logger.LogDebug("Cancelling settings changes");
        }

        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private async Task SaveAndCloseAsync()
    {
        await SaveSettingsAsync();

        if (!HasChanges)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
