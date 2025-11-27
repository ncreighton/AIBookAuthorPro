// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Enums;
using AIBookAuthorPro.Core.Models;

namespace AIBookAuthorPro.Core.Interfaces;

/// <summary>
/// Service for exporting projects to various formats.
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Exports a project to the specified format.
    /// </summary>
    /// <param name="project">The project to export.</param>
    /// <param name="options">Export options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing the export file path.</returns>
    Task<Result<string>> ExportAsync(
        Project project,
        ExportOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the available export formats.
    /// </summary>
    /// <returns>List of supported export formats.</returns>
    IReadOnlyList<ExportFormatInfo> GetAvailableFormats();

    /// <summary>
    /// Validates export options before export.
    /// </summary>
    /// <param name="project">The project to export.</param>
    /// <param name="options">Export options to validate.</param>
    /// <returns>Validation result.</returns>
    Result ValidateExportOptions(Project project, ExportOptions options);

    /// <summary>
    /// Gets the estimated file size for an export.
    /// </summary>
    /// <param name="project">The project to export.</param>
    /// <param name="format">Target format.</param>
    /// <returns>Estimated size in bytes.</returns>
    long EstimateExportSize(Project project, ExportFormat format);
}

/// <summary>
/// Export options for project export.
/// </summary>
public class ExportOptions
{
    /// <summary>
    /// Gets or sets the target format.
    /// </summary>
    public ExportFormat Format { get; set; } = ExportFormat.Docx;

    /// <summary>
    /// Gets or sets the output file path.
    /// </summary>
    public string OutputPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether to include table of contents.
    /// </summary>
    public bool IncludeTableOfContents { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to include chapter titles.
    /// </summary>
    public bool IncludeChapterTitles { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to include chapter numbers.
    /// </summary>
    public bool IncludeChapterNumbers { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to start each chapter on a new page.
    /// </summary>
    public bool ChapterPageBreaks { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to include front matter (title page, copyright).
    /// </summary>
    public bool IncludeFrontMatter { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to embed fonts in the document.
    /// </summary>
    public bool EmbedFonts { get; set; } = true;

    /// <summary>
    /// Gets or sets the chapter filter (null = all chapters).
    /// </summary>
    public List<Guid>? ChapterFilter { get; set; }

    /// <summary>
    /// Gets or sets the font family for the export.
    /// </summary>
    public string FontFamily { get; set; } = "Georgia";

    /// <summary>
    /// Gets or sets the font size in points.
    /// </summary>
    public double FontSize { get; set; } = 12;

    /// <summary>
    /// Gets or sets the line spacing multiplier.
    /// </summary>
    public double LineSpacing { get; set; } = 1.5;

    /// <summary>
    /// Gets or sets custom CSS for HTML/EPUB export.
    /// </summary>
    public string? CustomCss { get; set; }

    /// <summary>
    /// Gets or sets the page size for PDF export.
    /// </summary>
    public PageSize PageSize { get; set; } = PageSize.Letter;

    /// <summary>
    /// Gets or sets page margins in inches.
    /// </summary>
    public PageMargins Margins { get; set; } = new();
}

/// <summary>
/// Page size options.
/// </summary>
public enum PageSize
{
    Letter,
    A4,
    A5,
    Custom
}

/// <summary>
/// Page margins configuration.
/// </summary>
public class PageMargins
{
    /// <summary>
    /// Gets or sets the top margin in inches.
    /// </summary>
    public double Top { get; set; } = 1.0;

    /// <summary>
    /// Gets or sets the bottom margin in inches.
    /// </summary>
    public double Bottom { get; set; } = 1.0;

    /// <summary>
    /// Gets or sets the left margin in inches.
    /// </summary>
    public double Left { get; set; } = 1.25;

    /// <summary>
    /// Gets or sets the right margin in inches.
    /// </summary>
    public double Right { get; set; } = 1.25;
}

/// <summary>
/// Information about an export format.
/// </summary>
public class ExportFormatInfo
{
    /// <summary>
    /// Gets or sets the format.
    /// </summary>
    public ExportFormat Format { get; set; }

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file extension.
    /// </summary>
    public string Extension { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the icon name.
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this format is available.
    /// </summary>
    public bool IsAvailable { get; set; } = true;
}
