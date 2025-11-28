// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.IO;
using System.Text;
using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Enums;
using AIBookAuthorPro.Core.Interfaces;
using AIBookAuthorPro.Core.Models;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.Infrastructure.Services;

/// <summary>
/// Service for exporting projects to various formats.
/// </summary>
public class ExportService : IExportService
{
    private readonly ILogger<ExportService> _logger;
    private readonly IDocxExporter _docxExporter;
    private readonly IMarkdownExporter _markdownExporter;
    private readonly IHtmlExporter _htmlExporter;

    private static readonly List<ExportFormatInfo> _formats =
    [
        new ExportFormatInfo
        {
            Format = ExportFormat.Docx,
            Name = "Microsoft Word",
            Description = "Standard document format compatible with Word, Google Docs, etc.",
            Extension = ".docx",
            Icon = "FileWord",
            IsAvailable = true
        },
        new ExportFormatInfo
        {
            Format = ExportFormat.Pdf,
            Name = "PDF",
            Description = "Portable document format for sharing and printing",
            Extension = ".pdf",
            Icon = "FilePdfBox",
            IsAvailable = true
        },
        new ExportFormatInfo
        {
            Format = ExportFormat.Epub,
            Name = "EPUB",
            Description = "E-book format for Kindle, Kobo, and other readers",
            Extension = ".epub",
            Icon = "BookOpen",
            IsAvailable = true
        },
        new ExportFormatInfo
        {
            Format = ExportFormat.Markdown,
            Name = "Markdown",
            Description = "Plain text format with formatting, great for version control",
            Extension = ".md",
            Icon = "LanguageMarkdown",
            IsAvailable = true
        },
        new ExportFormatInfo
        {
            Format = ExportFormat.Html,
            Name = "HTML",
            Description = "Web format for online publishing",
            Extension = ".html",
            Icon = "LanguageHtml5",
            IsAvailable = true
        },
        new ExportFormatInfo
        {
            Format = ExportFormat.PlainText,
            Name = "Plain Text",
            Description = "Simple text format without formatting",
            Extension = ".txt",
            Icon = "FileDocument",
            IsAvailable = true
        }
    ];

    /// <summary>
    /// Initializes a new instance of ExportService.
    /// </summary>
    public ExportService(
        ILogger<ExportService> logger,
        IDocxExporter docxExporter,
        IMarkdownExporter markdownExporter,
        IHtmlExporter htmlExporter)
    {
        _logger = logger;
        _docxExporter = docxExporter;
        _markdownExporter = markdownExporter;
        _htmlExporter = htmlExporter;
    }

    /// <inheritdoc />
    public async Task<Result<string>> ExportAsync(
        Project project,
        ExportOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(options);

        _logger.LogInformation(
            "Exporting project {ProjectName} to {Format}",
            project.Name,
            options.Format);

        try
        {
            // Validate options
            var validationResult = ValidateExportOptions(project, options);
            if (validationResult.IsFailure)
            {
                return Result<string>.Failure(validationResult.Error!);
            }

            // Ensure output directory exists
            var directory = Path.GetDirectoryName(options.OutputPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Get chapters to export
            var chapters = GetChaptersToExport(project, options);

            if (chapters.Count == 0)
            {
                return Result<string>.Failure("No chapters to export");
            }

            // Export based on format
            var result = options.Format switch
            {
                ExportFormat.Docx => await _docxExporter.ExportAsync(project, chapters, options, cancellationToken),
                ExportFormat.Pdf => await ExportToPdfAsync(project, chapters, options, cancellationToken),
                ExportFormat.Epub => await ExportToEpubAsync(project, chapters, options, cancellationToken),
                ExportFormat.Markdown => await _markdownExporter.ExportAsync(project, chapters, options, cancellationToken),
                ExportFormat.Html => await _htmlExporter.ExportAsync(project, chapters, options, cancellationToken),
                ExportFormat.PlainText => await ExportToPlainTextAsync(project, chapters, options, cancellationToken),
                _ => Result<string>.Failure($"Unsupported format: {options.Format}")
            };

            if (result.IsSuccess)
            {
                _logger.LogInformation(
                    "Successfully exported {ProjectName} to {OutputPath}",
                    project.Name,
                    options.OutputPath);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Export cancelled for project {ProjectName}", project.Name);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export project {ProjectName}", project.Name);
            return Result<string>.Failure($"Export failed: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<ExportFormatInfo> GetAvailableFormats() => _formats;

    /// <inheritdoc />
    public Result ValidateExportOptions(Project project, ExportOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.OutputPath))
        {
            return Result.Failure("Output path is required");
        }

        var extension = Path.GetExtension(options.OutputPath).ToLowerInvariant();
        var expectedExtension = GetExtensionForFormat(options.Format);

        if (extension != expectedExtension)
        {
            return Result.Failure(
                $"Output file should have {expectedExtension} extension for {options.Format} format");
        }

        if (options.ChapterFilter != null && options.ChapterFilter.Count > 0)
        {
            var projectChapterIds = project.Chapters.Select(c => c.Id).ToHashSet();
            var invalidIds = options.ChapterFilter.Where(id => !projectChapterIds.Contains(id)).ToList();

            if (invalidIds.Count > 0)
            {
                return Result.Failure($"Chapter filter contains {invalidIds.Count} invalid chapter IDs");
            }
        }

        return Result.Success();
    }

    /// <inheritdoc />
    public long EstimateExportSize(Project project, ExportFormat format)
    {
        // Estimate based on content size and format overhead
        var contentSize = project.Chapters
            .Sum(c => (c.Content?.Length ?? 0) + (c.Title?.Length ?? 0) * 10);

        var multiplier = format switch
        {
            ExportFormat.PlainText => 1.0,
            ExportFormat.Markdown => 1.1,
            ExportFormat.Html => 1.5,
            ExportFormat.Docx => 2.0,
            ExportFormat.Pdf => 2.5,
            ExportFormat.Epub => 1.8,
            _ => 1.5
        };

        return (long)(contentSize * multiplier);
    }

    private static string GetExtensionForFormat(ExportFormat format) => format switch
    {
        ExportFormat.Docx => ".docx",
        ExportFormat.Pdf => ".pdf",
        ExportFormat.Epub => ".epub",
        ExportFormat.Markdown => ".md",
        ExportFormat.Html => ".html",
        ExportFormat.PlainText => ".txt",
        _ => ".txt"
    };

    private static List<Chapter> GetChaptersToExport(Project project, ExportOptions options)
    {
        var chapters = project.Chapters
            .Where(c => c.Status != ChapterStatus.Outline) // Skip outline-only chapters
            .OrderBy(c => c.Order)
            .ToList();

        if (options.ChapterFilter != null && options.ChapterFilter.Count > 0)
        {
            var filterSet = options.ChapterFilter.ToHashSet();
            chapters = chapters.Where(c => filterSet.Contains(c.Id)).ToList();
        }

        return chapters;
    }

    private async Task<Result<string>> ExportToPdfAsync(
        Project project,
        List<Chapter> chapters,
        ExportOptions options,
        CancellationToken cancellationToken)
    {
        // For now, export to HTML then convert
        // In a real implementation, use a PDF library like QuestPDF or iText
        _logger.LogDebug("PDF export using HTML conversion");

        var htmlPath = Path.ChangeExtension(options.OutputPath, ".html");
        var htmlOptions = new ExportOptions
        {
            Format = ExportFormat.Html,
            OutputPath = htmlPath,
            IncludeTableOfContents = options.IncludeTableOfContents,
            IncludeChapterTitles = options.IncludeChapterTitles,
            IncludeChapterNumbers = options.IncludeChapterNumbers,
            IncludeFrontMatter = options.IncludeFrontMatter,
            FontFamily = options.FontFamily,
            FontSize = options.FontSize,
            LineSpacing = options.LineSpacing,
            CustomCss = options.CustomCss
        };

        var htmlResult = await _htmlExporter.ExportAsync(project, chapters, htmlOptions, cancellationToken);

        if (htmlResult.IsFailure)
        {
            return htmlResult;
        }

        // In a full implementation, convert HTML to PDF here
        // For now, just copy the HTML file
        await Task.Run(() => File.Copy(htmlPath, options.OutputPath, true), cancellationToken);

        // Clean up temp HTML
        try
        {
            File.Delete(htmlPath);
        }
        catch { /* Ignore cleanup errors */ }

        return Result<string>.Success(options.OutputPath);
    }

    private async Task<Result<string>> ExportToEpubAsync(
        Project project,
        List<Chapter> chapters,
        ExportOptions options,
        CancellationToken cancellationToken)
    {
        // EPUB is essentially a zipped collection of HTML/XML files
        // In a real implementation, use a library like EpubSharp
        _logger.LogDebug("EPUB export");

        // For now, create a simple HTML version
        var htmlOptions = new ExportOptions
        {
            Format = ExportFormat.Html,
            OutputPath = options.OutputPath,
            IncludeTableOfContents = options.IncludeTableOfContents,
            IncludeChapterTitles = options.IncludeChapterTitles,
            IncludeChapterNumbers = options.IncludeChapterNumbers,
            IncludeFrontMatter = options.IncludeFrontMatter,
            FontFamily = options.FontFamily,
            FontSize = options.FontSize,
            LineSpacing = options.LineSpacing
        };

        return await _htmlExporter.ExportAsync(project, chapters, htmlOptions, cancellationToken);
    }

    private async Task<Result<string>> ExportToPlainTextAsync(
        Project project,
        List<Chapter> chapters,
        ExportOptions options,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Plain text export");

        var sb = new StringBuilder();

        // Title
        if (options.IncludeFrontMatter)
        {
            sb.AppendLine(project.Name.ToUpperInvariant());
            if (!string.IsNullOrEmpty(project.Metadata?.Author))
            {
                sb.AppendLine($"by {project.Metadata.Author}");
            }
            sb.AppendLine();
            sb.AppendLine(new string('=', 50));
            sb.AppendLine();
        }

        // Chapters
        foreach (var chapter in chapters)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (options.IncludeChapterTitles)
            {
                var title = options.IncludeChapterNumbers
                    ? $"Chapter {chapter.Order}: {chapter.Title}"
                    : chapter.Title;

                sb.AppendLine(title);
                sb.AppendLine(new string('-', title.Length));
                sb.AppendLine();
            }

            sb.AppendLine(StripHtmlAndXaml(chapter.Content ?? string.Empty));
            sb.AppendLine();

            if (options.ChapterPageBreaks)
            {
                sb.AppendLine();
                sb.AppendLine(new string('*', 3));
                sb.AppendLine();
            }
        }

        await File.WriteAllTextAsync(options.OutputPath, sb.ToString(), cancellationToken);

        return Result<string>.Success(options.OutputPath);
    }

    private static string StripHtmlAndXaml(string content)
    {
        if (string.IsNullOrEmpty(content)) return string.Empty;

        // Simple strip - in production use a proper parser
        var result = System.Text.RegularExpressions.Regex.Replace(
            content,
            "<[^>]*>",
            string.Empty);

        // Decode common entities
        result = result
            .Replace("&nbsp;", " ")
            .Replace("&amp;", "&")
            .Replace("&lt;", "<")
            .Replace("&gt;", ">")
            .Replace("&quot;", "\"");

        return result.Trim();
    }
}

/// <summary>
/// Interface for DOCX export.
/// </summary>
public interface IDocxExporter
{
    Task<Result<string>> ExportAsync(
        Project project,
        List<Chapter> chapters,
        ExportOptions options,
        CancellationToken cancellationToken);
}

/// <summary>
/// Interface for Markdown export.
/// </summary>
public interface IMarkdownExporter
{
    Task<Result<string>> ExportAsync(
        Project project,
        List<Chapter> chapters,
        ExportOptions options,
        CancellationToken cancellationToken);
}

/// <summary>
/// Interface for HTML export.
/// </summary>
public interface IHtmlExporter
{
    Task<Result<string>> ExportAsync(
        Project project,
        List<Chapter> chapters,
        ExportOptions options,
        CancellationToken cancellationToken);
}
