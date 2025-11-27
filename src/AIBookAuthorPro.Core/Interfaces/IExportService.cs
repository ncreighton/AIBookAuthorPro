// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Common;
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
    System.Threading.Tasks.Task<Result<string>> ExportAsync(
        Project project,
        ExportOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the available export formats.
    /// </summary>
    IReadOnlyList<ExportFormatInfo> GetAvailableFormats();

    /// <summary>
    /// Validates export options before export.
    /// </summary>
    Result ValidateExportOptions(Project project, ExportOptions options);

    /// <summary>
    /// Gets the estimated file size for an export.
    /// </summary>
    long EstimateExportSize(Project project, ExportFormat format);
}

/// <summary>
/// Export options for project export.
/// </summary>
public class ExportOptions
{
    public ExportFormat Format { get; set; } = ExportFormat.Docx;
    public string OutputPath { get; set; } = string.Empty;
    public bool IncludeTableOfContents { get; set; } = true;
    public bool IncludeChapterTitles { get; set; } = true;
    public bool IncludeChapterNumbers { get; set; } = true;
    public bool ChapterPageBreaks { get; set; } = true;
    public bool IncludeFrontMatter { get; set; } = true;
    public bool EmbedFonts { get; set; } = true;
    public List<Guid>? ChapterFilter { get; set; }
    public string FontFamily { get; set; } = "Georgia";
    public double FontSize { get; set; } = 12;
    public double LineSpacing { get; set; } = 1.5;
    public string? CustomCss { get; set; }
    public PageSize PageSize { get; set; } = PageSize.Letter;
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
    public double Top { get; set; } = 1.0;
    public double Bottom { get; set; } = 1.0;
    public double Left { get; set; } = 1.25;
    public double Right { get; set; } = 1.25;
}

/// <summary>
/// Information about an export format.
/// </summary>
public class ExportFormatInfo
{
    public ExportFormat Format { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
}
