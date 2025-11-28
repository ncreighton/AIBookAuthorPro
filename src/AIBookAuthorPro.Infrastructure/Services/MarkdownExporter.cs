// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.IO;
using System.Text;
using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Interfaces;
using AIBookAuthorPro.Core.Models;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.Infrastructure.Services;

/// <summary>
/// Exports projects to Markdown format.
/// </summary>
public class MarkdownExporter : IMarkdownExporter
{
    private readonly ILogger<MarkdownExporter> _logger;

    /// <summary>
    /// Initializes a new instance of MarkdownExporter.
    /// </summary>
    public MarkdownExporter(ILogger<MarkdownExporter> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<string>> ExportAsync(
        Project project,
        List<Chapter> chapters,
        ExportOptions options,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Exporting to Markdown: {ChapterCount} chapters", chapters.Count);

        try
        {
            var sb = new StringBuilder();

            // Front matter
            if (options.IncludeFrontMatter)
            {
                sb.AppendLine("---");
                sb.AppendLine($"title: \"{EscapeYaml(project.Name)}\"");

                if (!string.IsNullOrEmpty(project.Metadata?.Author))
                {
                    sb.AppendLine($"author: \"{EscapeYaml(project.Metadata.Author)}\"");
                }

                if (!string.IsNullOrEmpty(project.Metadata?.Genre))
                {
                    sb.AppendLine($"genre: \"{EscapeYaml(project.Metadata.Genre)}\"");
                }

                sb.AppendLine($"date: \"{DateTime.UtcNow:yyyy-MM-dd}\"");
                sb.AppendLine("---");
                sb.AppendLine();

                // Title
                sb.AppendLine($"# {project.Name}");
                sb.AppendLine();

                if (!string.IsNullOrEmpty(project.Metadata?.Author))
                {
                    sb.AppendLine($"*by {project.Metadata.Author}*");
                    sb.AppendLine();
                }
            }

            // Table of contents
            if (options.IncludeTableOfContents)
            {
                sb.AppendLine("## Table of Contents");
                sb.AppendLine();

                foreach (var chapter in chapters)
                {
                    var anchor = GenerateAnchor(chapter.Title);
                    var title = options.IncludeChapterNumbers
                        ? $"Chapter {chapter.Order}: {chapter.Title}"
                        : chapter.Title;

                    sb.AppendLine($"- [{title}](#{anchor})");
                }

                sb.AppendLine();
                sb.AppendLine("---");
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

                    sb.AppendLine($"## {title}");
                    sb.AppendLine();
                }

                // Convert content
                var content = ConvertToMarkdown(chapter.Content ?? string.Empty);
                sb.AppendLine(content);
                sb.AppendLine();

                if (options.ChapterPageBreaks)
                {
                    sb.AppendLine("---");
                    sb.AppendLine();
                }
            }

            await File.WriteAllTextAsync(options.OutputPath, sb.ToString(), cancellationToken);

            _logger.LogInformation("Markdown export complete: {Path}", options.OutputPath);
            return Result<string>.Success(options.OutputPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Markdown export failed");
            return Result<string>.Failure($"Markdown export failed: {ex.Message}", ex);
        }
    }

    private static string EscapeYaml(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"");
    }

    private static string GenerateAnchor(string title)
    {
        return title
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("'", "")
            .Replace("\"", "");
    }

    private static string ConvertToMarkdown(string content)
    {
        if (string.IsNullOrEmpty(content)) return string.Empty;

        // Check if content is XAML FlowDocument
        if (content.Contains("<FlowDocument") || content.Contains("<Paragraph"))
        {
            return ConvertXamlToMarkdown(content);
        }

        // Check if content is HTML
        if (content.Contains("<p>") || content.Contains("<div>"))
        {
            return ConvertHtmlToMarkdown(content);
        }

        // Plain text - just return as is
        return content;
    }

    private static string ConvertXamlToMarkdown(string xaml)
    {
        var sb = new StringBuilder();

        // Extract paragraphs
        var paragraphMatches = System.Text.RegularExpressions.Regex.Matches(
            xaml,
            @"<Paragraph[^>]*>(.*?)</Paragraph>",
            System.Text.RegularExpressions.RegexOptions.Singleline);

        foreach (System.Text.RegularExpressions.Match match in paragraphMatches)
        {
            var text = match.Groups[1].Value;

            // Handle bold
            text = System.Text.RegularExpressions.Regex.Replace(
                text,
                @"<Bold>(.*?)</Bold>",
                "**$1**");

            // Handle italic
            text = System.Text.RegularExpressions.Regex.Replace(
                text,
                @"<Italic>(.*?)</Italic>",
                "*$1*");

            // Handle underline (no direct markdown equivalent, use emphasis)
            text = System.Text.RegularExpressions.Regex.Replace(
                text,
                @"<Underline>(.*?)</Underline>",
                "_$1_");

            // Strip remaining tags
            text = System.Text.RegularExpressions.Regex.Replace(
                text,
                @"<[^>]+>",
                string.Empty);

            if (!string.IsNullOrWhiteSpace(text))
            {
                sb.AppendLine(text.Trim());
                sb.AppendLine();
            }
        }

        return sb.ToString().TrimEnd();
    }

    private static string ConvertHtmlToMarkdown(string html)
    {
        var result = html;

        // Convert headings
        result = System.Text.RegularExpressions.Regex.Replace(result, @"<h1[^>]*>(.*?)</h1>", "# $1\n");
        result = System.Text.RegularExpressions.Regex.Replace(result, @"<h2[^>]*>(.*?)</h2>", "## $1\n");
        result = System.Text.RegularExpressions.Regex.Replace(result, @"<h3[^>]*>(.*?)</h3>", "### $1\n");

        // Convert paragraphs
        result = System.Text.RegularExpressions.Regex.Replace(result, @"<p[^>]*>(.*?)</p>", "$1\n\n");

        // Convert emphasis
        result = System.Text.RegularExpressions.Regex.Replace(result, @"<strong>(.*?)</strong>", "**$1**");
        result = System.Text.RegularExpressions.Regex.Replace(result, @"<b>(.*?)</b>", "**$1**");
        result = System.Text.RegularExpressions.Regex.Replace(result, @"<em>(.*?)</em>", "*$1*");
        result = System.Text.RegularExpressions.Regex.Replace(result, @"<i>(.*?)</i>", "*$1*");

        // Convert line breaks
        result = System.Text.RegularExpressions.Regex.Replace(result, @"<br\s*/?>", "  \n");

        // Strip remaining tags
        result = System.Text.RegularExpressions.Regex.Replace(result, @"<[^>]+>", string.Empty);

        // Decode entities
        result = System.Net.WebUtility.HtmlDecode(result);

        return result.Trim();
    }
}
