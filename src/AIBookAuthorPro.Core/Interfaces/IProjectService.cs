// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Models;

namespace AIBookAuthorPro.Core.Interfaces;

/// <summary>
/// Defines the contract for project management operations.
/// </summary>
public interface IProjectService
{
    /// <summary>
    /// Creates a new project.
    /// </summary>
    /// <param name="name">The project name.</param>
    /// <param name="templateName">Optional template to use.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created project.</returns>
    Task<Result<Project>> CreateAsync(
        string name,
        string? templateName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a project from file.
    /// </summary>
    /// <param name="filePath">The path to the project file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The loaded project.</returns>
    Task<Result<Project>> LoadAsync(
        string filePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a project to file.
    /// </summary>
    /// <param name="project">The project to save.</param>
    /// <param name="filePath">Optional path (uses project's FilePath if not specified).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result> SaveAsync(
        Project project,
        string? filePath = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent projects.
    /// </summary>
    /// <param name="count">Maximum number of projects to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of recent project summaries.</returns>
    Task<Result<IReadOnlyList<ProjectSummary>>> GetRecentProjectsAsync(
        int count = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a project.
    /// </summary>
    /// <param name="filePath">The path to the project file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result> DeleteAsync(
        string filePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports a project to a different format.
    /// </summary>
    /// <param name="project">The project to export.</param>
    /// <param name="format">The export format.</param>
    /// <param name="outputPath">The output file path.</param>
    /// <param name="options">Export options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result> ExportAsync(
        Project project,
        Enums.ExportFormat format,
        string outputPath,
        ExportOptions? options = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Summary information about a project for display in lists.
/// </summary>
public sealed class ProjectSummary
{
    /// <summary>
    /// Gets or sets the project file path.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the book title.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the author name.
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// Gets or sets the last modified date.
    /// </summary>
    public DateTime LastModified { get; set; }

    /// <summary>
    /// Gets or sets the word count.
    /// </summary>
    public int WordCount { get; set; }

    /// <summary>
    /// Gets or sets the chapter count.
    /// </summary>
    public int ChapterCount { get; set; }

    /// <summary>
    /// Gets or sets the completion percentage.
    /// </summary>
    public double CompletionPercentage { get; set; }
}

/// <summary>
/// Options for exporting a project.
/// </summary>
public sealed class ExportOptions
{
    /// <summary>
    /// Gets or sets whether to include the title page.
    /// </summary>
    public bool IncludeTitlePage { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to include the table of contents.
    /// </summary>
    public bool IncludeTableOfContents { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to include chapter numbers.
    /// </summary>
    public bool IncludeChapterNumbers { get; set; } = true;

    /// <summary>
    /// Gets or sets the chapter number format (e.g., "Chapter {0}", "{0}").
    /// </summary>
    public string ChapterNumberFormat { get; set; } = "Chapter {0}";

    /// <summary>
    /// Gets or sets the font family to use.
    /// </summary>
    public string FontFamily { get; set; } = "Georgia";

    /// <summary>
    /// Gets or sets the font size in points.
    /// </summary>
    public int FontSize { get; set; } = 12;

    /// <summary>
    /// Gets or sets the line spacing multiplier.
    /// </summary>
    public double LineSpacing { get; set; } = 1.5;

    /// <summary>
    /// Gets or sets the page size for PDF export.
    /// </summary>
    public string PageSize { get; set; } = "Letter";

    /// <summary>
    /// Gets or sets custom CSS for EPUB export.
    /// </summary>
    public string? CustomCss { get; set; }
}
