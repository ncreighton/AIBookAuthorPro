// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Models;

namespace AIBookAuthorPro.Infrastructure.Export;

/// <summary>
/// Interface for document exporters.
/// </summary>
public interface IDocumentExporter
{
    /// <summary>
    /// Gets the export format this exporter handles.
    /// </summary>
    ExportFormat Format { get; }
    
    /// <summary>
    /// Gets the file extension for exported files.
    /// </summary>
    string FileExtension { get; }
    
    /// <summary>
    /// Gets the display name for the export format.
    /// </summary>
    string DisplayName { get; }
    
    /// <summary>
    /// Exports the project to a file.
    /// </summary>
    Task<Result> ExportAsync(
        Project project,
        string outputPath,
        ExportOptions options,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Exports the project to a stream.
    /// </summary>
    Task<Result<Stream>> ExportToStreamAsync(
        Project project,
        ExportOptions options,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Supported export formats.
/// </summary>
public enum ExportFormat
{
    Docx,
    Pdf,
    Epub,
    Html,
    Markdown,
    PlainText
}

/// <summary>
/// Options for export operations.
/// </summary>
public sealed class ExportOptions
{
    /// <summary>
    /// Whether to include the table of contents.
    /// </summary>
    public bool IncludeTableOfContents { get; set; } = true;
    
    /// <summary>
    /// Whether to include chapter numbers.
    /// </summary>
    public bool IncludeChapterNumbers { get; set; } = true;
    
    /// <summary>
    /// Whether to include title page.
    /// </summary>
    public bool IncludeTitlePage { get; set; } = true;
    
    /// <summary>
    /// Whether to include copyright page.
    /// </summary>
    public bool IncludeCopyrightPage { get; set; } = true;
    
    /// <summary>
    /// Font family for body text.
    /// </summary>
    public string BodyFontFamily { get; set; } = "Georgia";
    
    /// <summary>
    /// Font size for body text in points.
    /// </summary>
    public double BodyFontSize { get; set; } = 12;
    
    /// <summary>
    /// Font family for chapter headers.
    /// </summary>
    public string HeaderFontFamily { get; set; } = "Times New Roman";
    
    /// <summary>
    /// Font size for chapter headers in points.
    /// </summary>
    public double HeaderFontSize { get; set; } = 24;
    
    /// <summary>
    /// Line spacing (1.0 = single, 1.5 = one and a half, 2.0 = double).
    /// </summary>
    public double LineSpacing { get; set; } = 1.5;
    
    /// <summary>
    /// Page margins in inches.
    /// </summary>
    public PageMargins Margins { get; set; } = new();
    
    /// <summary>
    /// Page size preset.
    /// </summary>
    public PageSizePreset PageSize { get; set; } = PageSizePreset.Letter;
    
    /// <summary>
    /// Custom page width in inches (used when PageSize is Custom).
    /// </summary>
    public double CustomPageWidth { get; set; } = 8.5;
    
    /// <summary>
    /// Custom page height in inches (used when PageSize is Custom).
    /// </summary>
    public double CustomPageHeight { get; set; } = 11;
    
    /// <summary>
    /// Whether to include page numbers.
    /// </summary>
    public bool IncludePageNumbers { get; set; } = true;
    
    /// <summary>
    /// Page number position.
    /// </summary>
    public PageNumberPosition PageNumberPosition { get; set; } = PageNumberPosition.BottomCenter;
    
    /// <summary>
    /// Whether to start each chapter on a new page.
    /// </summary>
    public bool ChapterPageBreak { get; set; } = true;
    
    /// <summary>
    /// Whether to add scene breaks between scenes.
    /// </summary>
    public bool IncludeSceneBreaks { get; set; } = true;
    
    /// <summary>
    /// Scene break symbol/text.
    /// </summary>
    public string SceneBreakSymbol { get; set; } = "* * *";
    
    /// <summary>
    /// EPUB-specific: Cover image path.
    /// </summary>
    public string? CoverImagePath { get; set; }
    
    /// <summary>
    /// EPUB-specific: Whether to embed fonts.
    /// </summary>
    public bool EmbedFonts { get; set; } = false;
}

/// <summary>
/// Page margin settings.
/// </summary>
public sealed class PageMargins
{
    public double Top { get; set; } = 1.0;
    public double Bottom { get; set; } = 1.0;
    public double Left { get; set; } = 1.25;
    public double Right { get; set; } = 1.25;
    public double Gutter { get; set; } = 0; // Additional inner margin for binding
}

/// <summary>
/// Preset page sizes.
/// </summary>
public enum PageSizePreset
{
    Letter,         // 8.5" x 11"
    A4,             // 8.27" x 11.69"
    Trade6x9,       // 6" x 9" (common trade paperback)
    Trade5_5x8_5,   // 5.5" x 8.5" (mass market)
    Trade5x8,       // 5" x 8"
    Custom
}

/// <summary>
/// Page number positioning.
/// </summary>
public enum PageNumberPosition
{
    TopLeft,
    TopCenter,
    TopRight,
    BottomLeft,
    BottomCenter,
    BottomRight
}