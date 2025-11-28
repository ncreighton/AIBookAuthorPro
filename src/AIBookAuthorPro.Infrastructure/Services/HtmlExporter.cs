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
/// Exports projects to HTML format.
/// </summary>
public class HtmlExporter : IHtmlExporter
{
    private readonly ILogger<HtmlExporter> _logger;

    /// <summary>
    /// Initializes a new instance of HtmlExporter.
    /// </summary>
    public HtmlExporter(ILogger<HtmlExporter> logger)
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
        _logger.LogDebug("Exporting to HTML: {ChapterCount} chapters", chapters.Count);

        try
        {
            var sb = new StringBuilder();

            // HTML document start
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"en\">");
            sb.AppendLine("<head>");
            sb.AppendLine($"    <meta charset=\"UTF-8\">");
            sb.AppendLine($"    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            sb.AppendLine($"    <title>{EscapeHtml(project.Name)}</title>");

            // Meta tags
            if (!string.IsNullOrEmpty(project.Metadata?.Author))
            {
                sb.AppendLine($"    <meta name=\"author\" content=\"{EscapeHtml(project.Metadata.Author)}\">");
            }

            if (!string.IsNullOrEmpty(project.Metadata?.Genre))
            {
                sb.AppendLine($"    <meta name=\"genre\" content=\"{EscapeHtml(project.Metadata.Genre)}\">");
            }

            // Styles
            sb.AppendLine("    <style>");
            sb.AppendLine(GenerateCss(options));

            if (!string.IsNullOrEmpty(options.CustomCss))
            {
                sb.AppendLine("        /* Custom CSS */");
                sb.AppendLine($"        {options.CustomCss}");
            }

            sb.AppendLine("    </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div class=\"book\">");

            // Front matter
            if (options.IncludeFrontMatter)
            {
                sb.AppendLine("    <header class=\"front-matter\">");
                sb.AppendLine($"        <h1 class=\"book-title\">{EscapeHtml(project.Name)}</h1>");

                if (!string.IsNullOrEmpty(project.Metadata?.Author))
                {
                    sb.AppendLine($"        <p class=\"book-author\">by {EscapeHtml(project.Metadata.Author)}</p>");
                }

                if (!string.IsNullOrEmpty(project.Metadata?.Description))
                {
                    sb.AppendLine($"        <p class=\"book-description\">{EscapeHtml(project.Metadata.Description)}</p>");
                }

                sb.AppendLine("    </header>");
            }

            // Table of contents
            if (options.IncludeTableOfContents)
            {
                sb.AppendLine("    <nav class=\"toc\">");
                sb.AppendLine("        <h2>Table of Contents</h2>");
                sb.AppendLine("        <ol>");

                foreach (var chapter in chapters)
                {
                    var anchor = GenerateAnchor(chapter.Id);
                    var title = options.IncludeChapterNumbers
                        ? $"Chapter {chapter.Order}: {chapter.Title}"
                        : chapter.Title;

                    sb.AppendLine($"            <li><a href=\"#{anchor}\">{EscapeHtml(title)}</a></li>");
                }

                sb.AppendLine("        </ol>");
                sb.AppendLine("    </nav>");
            }

            // Chapters
            sb.AppendLine("    <main class=\"content\">");

            foreach (var chapter in chapters)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var anchor = GenerateAnchor(chapter.Id);
                var pageBreakClass = options.ChapterPageBreaks ? " page-break" : "";

                sb.AppendLine($"        <article class=\"chapter{pageBreakClass}\" id=\"{anchor}\">");

                if (options.IncludeChapterTitles)
                {
                    var title = options.IncludeChapterNumbers
                        ? $"Chapter {chapter.Order}: {chapter.Title}"
                        : chapter.Title;

                    sb.AppendLine($"            <h2 class=\"chapter-title\">{EscapeHtml(title)}</h2>");
                }

                // Convert content
                var content = ConvertToHtml(chapter.Content ?? string.Empty);
                sb.AppendLine($"            <div class=\"chapter-content\">");
                sb.AppendLine(content);
                sb.AppendLine("            </div>");
                sb.AppendLine("        </article>");
            }

            sb.AppendLine("    </main>");

            // Footer
            sb.AppendLine("    <footer class=\"back-matter\">");
            sb.AppendLine($"        <p>Generated by AI Book Author Pro on {DateTime.UtcNow:MMMM d, yyyy}</p>");
            sb.AppendLine("    </footer>");

            sb.AppendLine("</div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            await File.WriteAllTextAsync(options.OutputPath, sb.ToString(), cancellationToken);

            _logger.LogInformation("HTML export complete: {Path}", options.OutputPath);
            return Result<string>.Success(options.OutputPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HTML export failed");
            return Result<string>.Failure($"HTML export failed: {ex.Message}", ex);
        }
    }

    private static string GenerateCss(ExportOptions options)
    {
        return $@"
        :root {{
            --font-family: {options.FontFamily}, Georgia, serif;
            --font-size: {options.FontSize}pt;
            --line-height: {options.LineSpacing};
        }}

        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}

        body {{
            font-family: var(--font-family);
            font-size: var(--font-size);
            line-height: var(--line-height);
            color: #333;
            background: #fff;
        }}

        .book {{
            max-width: 800px;
            margin: 0 auto;
            padding: 2rem;
        }}

        .front-matter {{
            text-align: center;
            margin-bottom: 3rem;
            padding-bottom: 2rem;
            border-bottom: 1px solid #ddd;
        }}

        .book-title {{
            font-size: 2.5em;
            margin-bottom: 0.5rem;
        }}

        .book-author {{
            font-size: 1.2em;
            font-style: italic;
            color: #666;
        }}

        .book-description {{
            margin-top: 1rem;
            color: #666;
        }}

        .toc {{
            margin-bottom: 3rem;
            padding: 1.5rem;
            background: #f9f9f9;
            border-radius: 4px;
        }}

        .toc h2 {{
            margin-bottom: 1rem;
            font-size: 1.3em;
        }}

        .toc ol {{
            padding-left: 1.5rem;
        }}

        .toc li {{
            margin-bottom: 0.5rem;
        }}

        .toc a {{
            color: #1976D2;
            text-decoration: none;
        }}

        .toc a:hover {{
            text-decoration: underline;
        }}

        .chapter {{
            margin-bottom: 3rem;
        }}

        .chapter-title {{
            font-size: 1.8em;
            margin-bottom: 1.5rem;
            text-align: center;
        }}

        .chapter-content p {{
            text-indent: 2em;
            margin-bottom: 1em;
        }}

        .chapter-content p:first-of-type {{
            text-indent: 0;
        }}

        .scene-break {{
            text-align: center;
            margin: 2rem 0;
            color: #999;
        }}

        .back-matter {{
            margin-top: 3rem;
            padding-top: 2rem;
            border-top: 1px solid #ddd;
            text-align: center;
            font-size: 0.9em;
            color: #999;
        }}

        @media print {{
            .book {{
                max-width: none;
                padding: 0;
            }}

            .page-break {{
                page-break-before: always;
            }}

            .toc {{
                background: none;
            }}
        }}
        ";
    }

    private static string EscapeHtml(string text)
    {
        return System.Net.WebUtility.HtmlEncode(text);
    }

    private static string GenerateAnchor(Guid id)
    {
        return $"chapter-{id:N}";
    }

    private static string ConvertToHtml(string content)
    {
        if (string.IsNullOrEmpty(content)) return "<p></p>";

        // Check if content is XAML FlowDocument
        if (content.Contains("<FlowDocument") || content.Contains("<Paragraph"))
        {
            return ConvertXamlToHtml(content);
        }

        // Check if content is already HTML
        if (content.Contains("<p>") || content.Contains("<div>"))
        {
            return content;
        }

        // Plain text - convert paragraphs
        var paragraphs = content.Split(
            new[] { "\r\n\r\n", "\n\n" },
            StringSplitOptions.RemoveEmptyEntries);

        var sb = new StringBuilder();
        foreach (var para in paragraphs)
        {
            var trimmed = para.Trim();
            if (trimmed == "***" || trimmed == "---" || trimmed == "* * *")
            {
                sb.AppendLine("                <div class=\"scene-break\">* * *</div>");
            }
            else if (!string.IsNullOrWhiteSpace(trimmed))
            {
                sb.AppendLine($"                <p>{EscapeHtml(trimmed)}</p>");
            }
        }

        return sb.ToString();
    }

    private static string ConvertXamlToHtml(string xaml)
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
                "<strong>$1</strong>");

            // Handle italic
            text = System.Text.RegularExpressions.Regex.Replace(
                text,
                @"<Italic>(.*?)</Italic>",
                "<em>$1</em>");

            // Handle underline
            text = System.Text.RegularExpressions.Regex.Replace(
                text,
                @"<Underline>(.*?)</Underline>",
                "<u>$1</u>");

            // Handle Run elements
            text = System.Text.RegularExpressions.Regex.Replace(
                text,
                @"<Run[^>]*>(.*?)</Run>",
                "$1");

            // Strip remaining XAML tags
            text = System.Text.RegularExpressions.Regex.Replace(
                text,
                @"<[^>]+>",
                string.Empty);

            if (!string.IsNullOrWhiteSpace(text))
            {
                sb.AppendLine($"                <p>{text.Trim()}</p>");
            }
        }

        return sb.ToString();
    }
}
