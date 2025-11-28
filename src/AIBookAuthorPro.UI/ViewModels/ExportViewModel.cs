// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Forms;
using AIBookAuthorPro.Core.Enums;
using AIBookAuthorPro.Core.Interfaces;
using AIBookAuthorPro.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.UI.ViewModels;

/// <summary>
/// ViewModel for the export dialog.
/// </summary>
public partial class ExportViewModel : ObservableObject
{
    private readonly IExportService _exportService;
    private readonly ISettingsService _settingsService;
    private readonly ILogger<ExportViewModel> _logger;

    [ObservableProperty]
    private Project? _project;

    [ObservableProperty]
    private ObservableCollection<ExportFormatInfo> _availableFormats = [];

    [ObservableProperty]
    private ExportFormatInfo? _selectedFormat;

    [ObservableProperty]
    private string _outputPath = string.Empty;

    [ObservableProperty]
    private string _fileName = string.Empty;

    [ObservableProperty]
    private bool _includeTableOfContents = true;

    [ObservableProperty]
    private bool _includeChapterTitles = true;

    [ObservableProperty]
    private bool _includeChapterNumbers = true;

    [ObservableProperty]
    private bool _chapterPageBreaks = true;

    [ObservableProperty]
    private bool _includeFrontMatter = true;

    [ObservableProperty]
    private bool _embedFonts = true;

    [ObservableProperty]
    private string _fontFamily = "Georgia";

    [ObservableProperty]
    private double _fontSize = 12;

    [ObservableProperty]
    private double _lineSpacing = 1.5;

    [ObservableProperty]
    private ObservableCollection<ChapterSelection> _chapters = [];

    [ObservableProperty]
    private bool _selectAllChapters = true;

    [ObservableProperty]
    private bool _isExporting;

    [ObservableProperty]
    private double _exportProgress;

    [ObservableProperty]
    private string _statusMessage = "Ready to export";

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private long _estimatedSize;

    /// <summary>
    /// Gets the available font families.
    /// </summary>
    public ObservableCollection<string> AvailableFonts { get; } =
    [
        "Georgia",
        "Cambria",
        "Times New Roman",
        "Palatino Linotype",
        "Garamond"
    ];

    /// <summary>
    /// Event raised when export is complete.
    /// </summary>
    public event EventHandler<ExportCompletedEventArgs>? ExportCompleted;

    /// <summary>
    /// Event raised when dialog should close.
    /// </summary>
    public event EventHandler? CloseRequested;

    /// <summary>
    /// Initializes a new instance of ExportViewModel.
    /// </summary>
    public ExportViewModel(
        IExportService exportService,
        ISettingsService settingsService,
        ILogger<ExportViewModel> logger)
    {
        _exportService = exportService;
        _settingsService = settingsService;
        _logger = logger;
    }

    /// <summary>
    /// Loads the project for export.
    /// </summary>
    public void LoadProject(Project project)
    {
        ArgumentNullException.ThrowIfNull(project);

        _logger.LogDebug("Loading project for export: {ProjectName}", project.Name);

        Project = project;
        FileName = SanitizeFileName(project.Name);

        // Load formats
        AvailableFormats.Clear();
        foreach (var format in _exportService.GetAvailableFormats())
        {
            AvailableFormats.Add(format);
        }

        // Load settings defaults
        var settings = _settingsService.CurrentSettings;
        SelectedFormat = AvailableFormats.FirstOrDefault(f => f.Format == settings.Export.DefaultFormat)
            ?? AvailableFormats.FirstOrDefault();
        OutputPath = settings.Export.DefaultOutputDirectory ?? GetDefaultOutputPath();
        IncludeTableOfContents = settings.Export.IncludeTableOfContents;
        IncludeChapterTitles = settings.Export.IncludeChapterTitles;
        ChapterPageBreaks = settings.Export.ChapterPageBreaks;
        EmbedFonts = settings.Export.EmbedFonts;

        // Load chapters
        Chapters.Clear();
        foreach (var chapter in project.Chapters.OrderBy(c => c.Order))
        {
            Chapters.Add(new ChapterSelection
            {
                Chapter = chapter,
                IsSelected = chapter.Status != ChapterStatus.Outlined
            });
        }

        UpdateEstimatedSize();
    }

    partial void OnSelectedFormatChanged(ExportFormatInfo? value)
    {
        UpdateEstimatedSize();
    }

    partial void OnSelectAllChaptersChanged(bool value)
    {
        foreach (var chapter in Chapters)
        {
            chapter.IsSelected = value;
        }
        UpdateEstimatedSize();
    }

    private void UpdateEstimatedSize()
    {
        if (Project == null || SelectedFormat == null) return;

        EstimatedSize = _exportService.EstimateExportSize(Project, SelectedFormat.Format);
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Join("", name.Where(c => !invalid.Contains(c)));
    }

    private static string GetDefaultOutputPath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "AI Book Author Pro",
            "Exports");
    }

    [RelayCommand]
    private void BrowseOutputPath()
    {
        var dialog = new FolderBrowserDialog
        {
            Description = "Select export location",
            UseDescriptionForTitle = true,
            SelectedPath = OutputPath
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            OutputPath = dialog.SelectedPath;
        }
    }

    [RelayCommand]
    private void ToggleChapter(ChapterSelection? selection)
    {
        if (selection == null) return;

        selection.IsSelected = !selection.IsSelected;
        SelectAllChapters = Chapters.All(c => c.IsSelected);
        UpdateEstimatedSize();
    }

    [RelayCommand(CanExecute = nameof(CanExport))]
    private async Task ExportAsync(CancellationToken cancellationToken)
    {
        if (Project == null || SelectedFormat == null) return;

        IsExporting = true;
        ErrorMessage = null;
        ExportProgress = 0;
        StatusMessage = "Preparing export...";

        try
        {
            _logger.LogInformation(
                "Starting export of {ProjectName} to {Format}",
                Project.Name,
                SelectedFormat.Format);

            // Build options
            var options = new ExportOptions
            {
                Format = SelectedFormat.Format,
                OutputPath = GetFullOutputPath(),
                IncludeTableOfContents = IncludeTableOfContents,
                IncludeChapterTitles = IncludeChapterTitles,
                IncludeChapterNumbers = IncludeChapterNumbers,
                ChapterPageBreaks = ChapterPageBreaks,
                IncludeFrontMatter = IncludeFrontMatter,
                EmbedFonts = EmbedFonts,
                FontFamily = FontFamily,
                FontSize = FontSize,
                LineSpacing = LineSpacing,
                ChapterFilter = Chapters
                    .Where(c => c.IsSelected)
                    .Select(c => c.Chapter.Id)
                    .ToList()
            };

            // Ensure output directory exists
            var directory = Path.GetDirectoryName(options.OutputPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            ExportProgress = 20;
            StatusMessage = "Exporting...";

            var result = await _exportService.ExportAsync(Project, options, cancellationToken);

            ExportProgress = 100;

            if (result.IsSuccess)
            {
                StatusMessage = "Export complete!";
                _logger.LogInformation("Export successful: {Path}", result.Value);

                ExportCompleted?.Invoke(this, new ExportCompletedEventArgs
                {
                    FilePath = result.Value!,
                    Format = SelectedFormat.Format,
                    Success = true
                });
            }
            else
            {
                ErrorMessage = result.Error;
                StatusMessage = "Export failed";
                _logger.LogWarning("Export failed: {Error}", result.Error);
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Export cancelled";
            _logger.LogInformation("Export cancelled");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            StatusMessage = "Export failed";
            _logger.LogError(ex, "Export error");
        }
        finally
        {
            IsExporting = false;
        }
    }

    private bool CanExport()
    {
        return !IsExporting &&
               Project != null &&
               SelectedFormat != null &&
               !string.IsNullOrWhiteSpace(OutputPath) &&
               !string.IsNullOrWhiteSpace(FileName) &&
               Chapters.Any(c => c.IsSelected);
    }

    private string GetFullOutputPath()
    {
        var extension = SelectedFormat?.Extension ?? ".docx";
        var fileName = FileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase)
            ? FileName
            : FileName + extension;

        return Path.Combine(OutputPath, fileName);
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void OpenExportFolder()
    {
        if (string.IsNullOrEmpty(OutputPath)) return;

        try
        {
            if (Directory.Exists(OutputPath))
            {
                System.Diagnostics.Process.Start("explorer.exe", OutputPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not open export folder");
        }
    }
}

/// <summary>
/// Represents a chapter with selection state.
/// </summary>
public class ChapterSelection : ObservableObject
{
    private bool _isSelected = true;

    /// <summary>
    /// Gets or sets the chapter.
    /// </summary>
    public Chapter Chapter { get; set; } = null!;

    /// <summary>
    /// Gets or sets whether the chapter is selected for export.
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}

/// <summary>
/// Event args for export completion.
/// </summary>
public class ExportCompletedEventArgs : EventArgs
{
    /// <summary>
    /// Gets or sets the export file path.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the export format.
    /// </summary>
    public ExportFormat Format { get; set; }

    /// <summary>
    /// Gets or sets whether the export was successful.
    /// </summary>
    public bool Success { get; set; }
}
