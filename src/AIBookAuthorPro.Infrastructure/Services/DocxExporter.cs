// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Interfaces;
using AIBookAuthorPro.Core.Models;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.Infrastructure.Services;

/// <summary>
/// Exports projects to DOCX format using Open XML.
/// </summary>
public class DocxExporter : IDocxExporter
{
    private readonly ILogger<DocxExporter> _logger;

    private static readonly XNamespace W = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
    private static readonly XNamespace R = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private static readonly XNamespace Wp = "http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing";

    /// <summary>
    /// Initializes a new instance of DocxExporter.
    /// </summary>
    public DocxExporter(ILogger<DocxExporter> logger)
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
        _logger.LogDebug("Exporting to DOCX: {ChapterCount} chapters", chapters.Count);

        try
        {
            // Create DOCX file (which is a ZIP archive)
            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                // Add required parts
                await AddContentTypesAsync(archive, cancellationToken);
                await AddRelationshipsAsync(archive, cancellationToken);
                await AddDocumentRelationshipsAsync(archive, cancellationToken);
                await AddStylesAsync(archive, options, cancellationToken);
                await AddDocumentAsync(archive, project, chapters, options, cancellationToken);
            }

            // Write to file
            memoryStream.Position = 0;
            await using var fileStream = new FileStream(
                options.OutputPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None);
            await memoryStream.CopyToAsync(fileStream, cancellationToken);

            _logger.LogInformation("DOCX export complete: {Path}", options.OutputPath);
            return Result<string>.Success(options.OutputPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DOCX export failed");
            return Result<string>.Failure($"DOCX export failed: {ex.Message}", ex);
        }
    }

    private static async Task AddContentTypesAsync(
        ZipArchive archive,
        CancellationToken cancellationToken)
    {
        var contentTypes = new XElement(
            XName.Get("Types", "http://schemas.openxmlformats.org/package/2006/content-types"),
            new XElement(
                XName.Get("Default", "http://schemas.openxmlformats.org/package/2006/content-types"),
                new XAttribute("Extension", "rels"),
                new XAttribute("ContentType", "application/vnd.openxmlformats-package.relationships+xml")),
            new XElement(
                XName.Get("Default", "http://schemas.openxmlformats.org/package/2006/content-types"),
                new XAttribute("Extension", "xml"),
                new XAttribute("ContentType", "application/xml")),
            new XElement(
                XName.Get("Override", "http://schemas.openxmlformats.org/package/2006/content-types"),
                new XAttribute("PartName", "/word/document.xml"),
                new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml")),
            new XElement(
                XName.Get("Override", "http://schemas.openxmlformats.org/package/2006/content-types"),
                new XAttribute("PartName", "/word/styles.xml"),
                new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.wordprocessingml.styles+xml")));

        var entry = archive.CreateEntry("[Content_Types].xml");
        await using var stream = entry.Open();
        await using var writer = new StreamWriter(stream, Encoding.UTF8);
        await writer.WriteAsync(contentTypes.ToString());
    }

    private static async Task AddRelationshipsAsync(
        ZipArchive archive,
        CancellationToken cancellationToken)
    {
        var relationships = new XElement(
            XName.Get("Relationships", "http://schemas.openxmlformats.org/package/2006/relationships"),
            new XElement(
                XName.Get("Relationship", "http://schemas.openxmlformats.org/package/2006/relationships"),
                new XAttribute("Id", "rId1"),
                new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"),
                new XAttribute("Target", "word/document.xml")));

        var entry = archive.CreateEntry("_rels/.rels");
        await using var stream = entry.Open();
        await using var writer = new StreamWriter(stream, Encoding.UTF8);
        await writer.WriteAsync(relationships.ToString());
    }

    private static async Task AddDocumentRelationshipsAsync(
        ZipArchive archive,
        CancellationToken cancellationToken)
    {
        var relationships = new XElement(
            XName.Get("Relationships", "http://schemas.openxmlformats.org/package/2006/relationships"),
            new XElement(
                XName.Get("Relationship", "http://schemas.openxmlformats.org/package/2006/relationships"),
                new XAttribute("Id", "rId1"),
                new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles"),
                new XAttribute("Target", "styles.xml")));

        var entry = archive.CreateEntry("word/_rels/document.xml.rels");
        await using var stream = entry.Open();
        await using var writer = new StreamWriter(stream, Encoding.UTF8);
        await writer.WriteAsync(relationships.ToString());
    }

    private static async Task AddStylesAsync(
        ZipArchive archive,
        ExportOptions options,
        CancellationToken cancellationToken)
    {
        // Convert font size to half-points (Word uses half-points)
        var fontSize = (int)(options.FontSize * 2);
        var lineSpacing = (int)(options.LineSpacing * 240); // 240 = single spacing

        var styles = new XElement(W + "styles",
            new XAttribute(XNamespace.Xmlns + "w", W.NamespaceName),
            // Normal style
            new XElement(W + "style",
                new XAttribute(W + "type", "paragraph"),
                new XAttribute(W + "styleId", "Normal"),
                new XAttribute(W + "default", "1"),
                new XElement(W + "name", new XAttribute(W + "val", "Normal")),
                new XElement(W + "pPr",
                    new XElement(W + "spacing",
                        new XAttribute(W + "after", "200"),
                        new XAttribute(W + "line", lineSpacing.ToString()),
                        new XAttribute(W + "lineRule", "auto"))),
                new XElement(W + "rPr",
                    new XElement(W + "rFonts",
                        new XAttribute(W + "ascii", options.FontFamily),
                        new XAttribute(W + "hAnsi", options.FontFamily)),
                    new XElement(W + "sz", new XAttribute(W + "val", fontSize.ToString())))),
            // Heading 1 (Title)
            new XElement(W + "style",
                new XAttribute(W + "type", "paragraph"),
                new XAttribute(W + "styleId", "Heading1"),
                new XElement(W + "name", new XAttribute(W + "val", "Heading 1")),
                new XElement(W + "basedOn", new XAttribute(W + "val", "Normal")),
                new XElement(W + "pPr",
                    new XElement(W + "jc", new XAttribute(W + "val", "center")),
                    new XElement(W + "spacing",
                        new XAttribute(W + "before", "480"),
                        new XAttribute(W + "after", "240"))),
                new XElement(W + "rPr",
                    new XElement(W + "b"),
                    new XElement(W + "sz", new XAttribute(W + "val", "48")))),
            // Heading 2 (Chapter title)
            new XElement(W + "style",
                new XAttribute(W + "type", "paragraph"),
                new XAttribute(W + "styleId", "Heading2"),
                new XElement(W + "name", new XAttribute(W + "val", "Heading 2")),
                new XElement(W + "basedOn", new XAttribute(W + "val", "Normal")),
                new XElement(W + "pPr",
                    new XElement(W + "jc", new XAttribute(W + "val", "center")),
                    new XElement(W + "spacing",
                        new XAttribute(W + "before", "360"),
                        new XAttribute(W + "after", "240"))),
                new XElement(W + "rPr",
                    new XElement(W + "b"),
                    new XElement(W + "sz", new XAttribute(W + "val", "36")))),
            // Scene break style
            new XElement(W + "style",
                new XAttribute(W + "type", "paragraph"),
                new XAttribute(W + "styleId", "SceneBreak"),
                new XElement(W + "name", new XAttribute(W + "val", "Scene Break")),
                new XElement(W + "basedOn", new XAttribute(W + "val", "Normal")),
                new XElement(W + "pPr",
                    new XElement(W + "jc", new XAttribute(W + "val", "center")),
                    new XElement(W + "spacing",
                        new XAttribute(W + "before", "480"),
                        new XAttribute(W + "after", "480")))));

        var entry = archive.CreateEntry("word/styles.xml");
        await using var stream = entry.Open();
        await using var writer = new StreamWriter(stream, Encoding.UTF8);
        await writer.WriteAsync(styles.ToString());
    }

    private static async Task AddDocumentAsync(
        ZipArchive archive,
        Project project,
        List<Chapter> chapters,
        ExportOptions options,
        CancellationToken cancellationToken)
    {
        var body = new XElement(W + "body");

        // Title page (front matter)
        if (options.IncludeFrontMatter)
        {
            body.Add(CreateParagraph(project.Name, "Heading1"));

            if (!string.IsNullOrEmpty(project.Metadata?.Author))
            {
                body.Add(CreateParagraph($"by {project.Metadata.Author}", "Normal", centered: true));
            }

            // Page break after title
            body.Add(CreatePageBreak());
        }

        // Chapters
        foreach (var chapter in chapters)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Page break before chapter (except first)
            if (options.ChapterPageBreaks && body.Elements().Any(e => e.Name == W + "p"))
            {
                body.Add(CreatePageBreak());
            }

            // Chapter title
            if (options.IncludeChapterTitles)
            {
                var title = options.IncludeChapterNumbers
                    ? $"Chapter {chapter.Order}"
                    : string.Empty;

                if (!string.IsNullOrEmpty(title))
                {
                    body.Add(CreateParagraph(title, "Heading2"));
                }

                if (!string.IsNullOrEmpty(chapter.Title))
                {
                    body.Add(CreateParagraph(chapter.Title, "Heading2"));
                }
            }

            // Chapter content
            var paragraphs = ExtractParagraphs(chapter.Content ?? string.Empty);
            foreach (var para in paragraphs)
            {
                if (para == "***" || para == "---" || para == "* * *")
                {
                    body.Add(CreateParagraph("* * *", "SceneBreak"));
                }
                else if (!string.IsNullOrWhiteSpace(para))
                {
                    body.Add(CreateParagraph(para, "Normal"));
                }
            }
        }

        var document = new XElement(W + "document",
            new XAttribute(XNamespace.Xmlns + "w", W.NamespaceName),
            new XAttribute(XNamespace.Xmlns + "r", R.NamespaceName),
            body);

        var entry = archive.CreateEntry("word/document.xml");
        await using var stream = entry.Open();
        await using var writer = new StreamWriter(stream, Encoding.UTF8);
        await writer.WriteAsync(document.ToString());
    }

    private static XElement CreateParagraph(string text, string style, bool centered = false)
    {
        var pPr = new XElement(W + "pPr",
            new XElement(W + "pStyle", new XAttribute(W + "val", style)));

        if (centered)
        {
            pPr.Add(new XElement(W + "jc", new XAttribute(W + "val", "center")));
        }

        return new XElement(W + "p",
            pPr,
            new XElement(W + "r",
                new XElement(W + "t", text)));
    }

    private static XElement CreatePageBreak()
    {
        return new XElement(W + "p",
            new XElement(W + "r",
                new XElement(W + "br", new XAttribute(W + "type", "page"))));
    }

    private static List<string> ExtractParagraphs(string content)
    {
        if (string.IsNullOrEmpty(content)) return [];

        // Check if content is XAML FlowDocument
        if (content.Contains("<FlowDocument") || content.Contains("<Paragraph"))
        {
            return ExtractFromXaml(content);
        }

        // Check if content is HTML
        if (content.Contains("<p>") || content.Contains("<div>"))
        {
            return ExtractFromHtml(content);
        }

        // Plain text
        return content
            .Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.None)
            .Select(p => p.Trim())
            .ToList();
    }

    private static List<string> ExtractFromXaml(string xaml)
    {
        var paragraphs = new List<string>();

        var matches = System.Text.RegularExpressions.Regex.Matches(
            xaml,
            @"<Paragraph[^>]*>(.*?)</Paragraph>",
            System.Text.RegularExpressions.RegexOptions.Singleline);

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            var text = match.Groups[1].Value;

            // Strip all XAML tags
            text = System.Text.RegularExpressions.Regex.Replace(
                text,
                @"<[^>]+>",
                string.Empty);

            paragraphs.Add(text.Trim());
        }

        return paragraphs;
    }

    private static List<string> ExtractFromHtml(string html)
    {
        var paragraphs = new List<string>();

        var matches = System.Text.RegularExpressions.Regex.Matches(
            html,
            @"<p[^>]*>(.*?)</p>",
            System.Text.RegularExpressions.RegexOptions.Singleline);

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            var text = match.Groups[1].Value;

            // Strip all HTML tags
            text = System.Text.RegularExpressions.Regex.Replace(
                text,
                @"<[^>]+>",
                string.Empty);

            // Decode entities
            text = System.Net.WebUtility.HtmlDecode(text);

            paragraphs.Add(text.Trim());
        }

        return paragraphs;
    }
}
