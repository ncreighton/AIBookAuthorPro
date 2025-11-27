// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Text.RegularExpressions;
using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.Infrastructure.Export;

/// <summary>
/// Exports projects to Microsoft Word DOCX format.
/// </summary>
public sealed partial class DocxExporter : IDocumentExporter
{
    private readonly ILogger<DocxExporter> _logger;

    public ExportFormat Format => ExportFormat.Docx;
    public string FileExtension => ".docx";
    public string DisplayName => "Microsoft Word (.docx)";

    public DocxExporter(ILogger<DocxExporter> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result> ExportAsync(
        Project project,
        string outputPath,
        ExportOptions options,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Exporting project {Title} to DOCX: {Path}",
                project.Metadata.Title, outputPath);

            await using var stream = File.Create(outputPath);
            var result = await ExportToStreamAsync(project, options, cancellationToken);
            
            if (!result.IsSuccess || result.Value == null)
            {
                return Result.Failure(result.Error ?? "Export failed");
            }

            await result.Value.CopyToAsync(stream, cancellationToken);
            
            _logger.LogInformation("DOCX export completed successfully");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DOCX export failed");
            return Result.Failure($"Export failed: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<Stream>> ExportToStreamAsync(
        Project project,
        ExportOptions options,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var memoryStream = new MemoryStream();

            using (var doc = WordprocessingDocument.Create(
                memoryStream, WordprocessingDocumentType.Document, true))
            {
                // Create main document part
                var mainPart = doc.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = mainPart.Document.AppendChild(new Body());

                // Add styles
                AddStyles(mainPart, options);

                // Add section properties (page size, margins)
                AddSectionProperties(body, options);

                // Title page
                if (options.IncludeTitlePage)
                {
                    AddTitlePage(body, project, options);
                }

                // Copyright page
                if (options.IncludeCopyrightPage && !string.IsNullOrEmpty(project.Metadata.Copyright))
                {
                    AddCopyrightPage(body, project, options);
                }

                // Table of contents placeholder
                if (options.IncludeTableOfContents)
                {
                    AddTableOfContents(body);
                }

                // Chapters
                var sortedChapters = project.Chapters
                    .OrderBy(c => c.Number)
                    .ToList();

                foreach (var chapter in sortedChapters)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    AddChapter(body, chapter, options);
                }

                mainPart.Document.Save();
            }

            memoryStream.Position = 0;
            return await Task.FromResult(Result<Stream>.Success(memoryStream));
        }
        catch (OperationCanceledException)
        {
            return Result<Stream>.Failure("Export cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DOCX export to stream failed");
            return Result<Stream>.Failure($"Export failed: {ex.Message}");
        }
    }

    private void AddStyles(MainDocumentPart mainPart, ExportOptions options)
    {
        var stylesPart = mainPart.AddNewPart<StyleDefinitionsPart>();
        var styles = new Styles();

        // Normal style (body text)
        var normalStyle = new Style
        {
            Type = StyleValues.Paragraph,
            StyleId = "Normal",
            Default = true
        };
        normalStyle.AppendChild(new StyleName { Val = "Normal" });
        normalStyle.AppendChild(new PrimaryStyle());
        normalStyle.AppendChild(new StyleParagraphProperties(
            new SpacingBetweenLines
            {
                Line = ((int)(options.LineSpacing * 240)).ToString(),
                LineRule = LineSpacingRuleValues.Auto
            },
            new Indentation { FirstLine = "720" }));
        normalStyle.AppendChild(new StyleRunProperties(
            new RunFonts { Ascii = options.BodyFontFamily, HighAnsi = options.BodyFontFamily },
            new FontSize { Val = ((int)(options.BodyFontSize * 2)).ToString() }));
        styles.AppendChild(normalStyle);

        // Heading 1 style (chapter titles)
        var heading1Style = new Style
        {
            Type = StyleValues.Paragraph,
            StyleId = "Heading1"
        };
        heading1Style.AppendChild(new StyleName { Val = "heading 1" });
        heading1Style.AppendChild(new BasedOn { Val = "Normal" });
        heading1Style.AppendChild(new NextParagraphStyle { Val = "Normal" });
        heading1Style.AppendChild(new PrimaryStyle());
        heading1Style.AppendChild(new StyleParagraphProperties(
            new KeepNext(),
            new KeepLines(),
            new SpacingBetweenLines { Before = "480", After = "240" },
            new Justification { Val = JustificationValues.Center },
            new Indentation { FirstLine = "0" },
            new OutlineLevel { Val = 0 }));
        heading1Style.AppendChild(new StyleRunProperties(
            new RunFonts { Ascii = options.HeaderFontFamily, HighAnsi = options.HeaderFontFamily },
            new Bold(),
            new FontSize { Val = ((int)(options.HeaderFontSize * 2)).ToString() }));
        styles.AppendChild(heading1Style);

        // Scene break style
        var sceneBreakStyle = new Style
        {
            Type = StyleValues.Paragraph,
            StyleId = "SceneBreak"
        };
        sceneBreakStyle.AppendChild(new StyleName { Val = "Scene Break" });
        sceneBreakStyle.AppendChild(new BasedOn { Val = "Normal" });
        sceneBreakStyle.AppendChild(new StyleParagraphProperties(
            new SpacingBetweenLines { Before = "240", After = "240" },
            new Justification { Val = JustificationValues.Center },
            new Indentation { FirstLine = "0" }));
        styles.AppendChild(sceneBreakStyle);

        // Title style
        var titleStyle = new Style
        {
            Type = StyleValues.Paragraph,
            StyleId = "Title"
        };
        titleStyle.AppendChild(new StyleName { Val = "Title" });
        titleStyle.AppendChild(new PrimaryStyle());
        titleStyle.AppendChild(new StyleParagraphProperties(
            new Justification { Val = JustificationValues.Center },
            new SpacingBetweenLines { After = "480" }));
        titleStyle.AppendChild(new StyleRunProperties(
            new RunFonts { Ascii = options.HeaderFontFamily, HighAnsi = options.HeaderFontFamily },
            new Bold(),
            new FontSize { Val = "72" })); // 36pt
        styles.AppendChild(titleStyle);

        stylesPart.Styles = styles;
    }

    private void AddSectionProperties(Body body, ExportOptions options)
    {
        var (pageWidth, pageHeight) = GetPageDimensions(options);
        
        // Convert inches to twentieths of a point (1 inch = 1440 twips)
        var widthTwips = (int)(pageWidth * 1440);
        var heightTwips = (int)(pageHeight * 1440);
        var topMargin = (int)(options.Margins.Top * 1440);
        var bottomMargin = (int)(options.Margins.Bottom * 1440);
        var leftMargin = (int)((options.Margins.Left + options.Margins.Gutter) * 1440);
        var rightMargin = (int)(options.Margins.Right * 1440);

        var sectionProps = new SectionProperties(
            new PageSize
            {
                Width = (uint)widthTwips,
                Height = (uint)heightTwips
            },
            new PageMargin
            {
                Top = topMargin,
                Bottom = bottomMargin,
                Left = (uint)leftMargin,
                Right = (uint)rightMargin,
                Gutter = (uint)(options.Margins.Gutter * 1440)
            });

        if (options.IncludePageNumbers)
        {
            // Add page numbering
            sectionProps.AppendChild(new PageNumberType { Start = 1 });
        }

        body.AppendChild(sectionProps);
    }

    private (double width, double height) GetPageDimensions(ExportOptions options)
    {
        return options.PageSize switch
        {
            PageSizePreset.Letter => (8.5, 11),
            PageSizePreset.A4 => (8.27, 11.69),
            PageSizePreset.Trade6x9 => (6, 9),
            PageSizePreset.Trade5_5x8_5 => (5.5, 8.5),
            PageSizePreset.Trade5x8 => (5, 8),
            PageSizePreset.Custom => (options.CustomPageWidth, options.CustomPageHeight),
            _ => (8.5, 11)
        };
    }

    private void AddTitlePage(Body body, Project project, ExportOptions options)
    {
        // Add spacing from top
        for (int i = 0; i < 8; i++)
        {
            body.AppendChild(new Paragraph(new Run(new Text(""))));
        }

        // Title
        var titlePara = new Paragraph(
            new ParagraphProperties(new ParagraphStyleId { Val = "Title" }),
            new Run(new Text(project.Metadata.Title)));
        body.AppendChild(titlePara);

        // Subtitle if present
        if (!string.IsNullOrEmpty(project.Metadata.Subtitle))
        {
            var subtitlePara = new Paragraph(
                new ParagraphProperties(
                    new Justification { Val = JustificationValues.Center },
                    new SpacingBetweenLines { After = "480" }),
                new Run(
                    new RunProperties(
                        new Italic(),
                        new FontSize { Val = "36" }),
                    new Text(project.Metadata.Subtitle)));
            body.AppendChild(subtitlePara);
        }

        // Author
        var authorPara = new Paragraph(
            new ParagraphProperties(
                new Justification { Val = JustificationValues.Center },
                new SpacingBetweenLines { Before = "960" }),
            new Run(
                new RunProperties(new FontSize { Val = "28" }),
                new Text($"by {project.Metadata.Author}")));
        body.AppendChild(authorPara);

        // Page break
        body.AppendChild(new Paragraph(
            new Run(new Break { Type = BreakValues.Page })));
    }

    private void AddCopyrightPage(Body body, Project project, ExportOptions options)
    {
        var copyrightPara = new Paragraph(
            new ParagraphProperties(
                new Justification { Val = JustificationValues.Center },
                new SpacingBetweenLines { After = "240" }),
            new Run(new Text(project.Metadata.Copyright)));
        body.AppendChild(copyrightPara);

        if (!string.IsNullOrEmpty(project.Metadata.Publisher))
        {
            var publisherPara = new Paragraph(
                new ParagraphProperties(
                    new Justification { Val = JustificationValues.Center }),
                new Run(new Text(project.Metadata.Publisher)));
            body.AppendChild(publisherPara);
        }

        if (!string.IsNullOrEmpty(project.Metadata.Isbn))
        {
            var isbnPara = new Paragraph(
                new ParagraphProperties(
                    new Justification { Val = JustificationValues.Center },
                    new SpacingBetweenLines { Before = "480" }),
                new Run(new Text($"ISBN: {project.Metadata.Isbn}")));
            body.AppendChild(isbnPara);
        }

        body.AppendChild(new Paragraph(
            new Run(new Break { Type = BreakValues.Page })));
    }

    private void AddTableOfContents(Body body)
    {
        // TOC title
        var tocTitle = new Paragraph(
            new ParagraphProperties(new ParagraphStyleId { Val = "Heading1" }),
            new Run(new Text("Table of Contents")));
        body.AppendChild(tocTitle);

        // TOC field
        var tocPara = new Paragraph();
        var tocRun = new Run();
        
        // TOC field code
        tocRun.AppendChild(new FieldChar { FieldCharType = FieldCharValues.Begin });
        tocRun.AppendChild(new FieldCode(" TOC \\o \"1-1\" \\h \\z \\u ") { Space = SpaceProcessingModeValues.Preserve });
        tocRun.AppendChild(new FieldChar { FieldCharType = FieldCharValues.Separate });
        tocRun.AppendChild(new Text("(Update this field to generate table of contents)"));
        tocRun.AppendChild(new FieldChar { FieldCharType = FieldCharValues.End });
        
        tocPara.AppendChild(tocRun);
        body.AppendChild(tocPara);

        body.AppendChild(new Paragraph(
            new Run(new Break { Type = BreakValues.Page })));
    }

    private void AddChapter(Body body, Chapter chapter, ExportOptions options)
    {
        // Page break before chapter (except for first chapter after TOC)
        if (options.ChapterPageBreak)
        {
            body.AppendChild(new Paragraph(
                new Run(new Break { Type = BreakValues.Page })));
        }

        // Chapter title
        var titleText = options.IncludeChapterNumbers
            ? $"Chapter {chapter.Number}: {chapter.Title}"
            : chapter.Title;

        var chapterTitle = new Paragraph(
            new ParagraphProperties(
                new ParagraphStyleId { Val = "Heading1" }),
            new Run(new Text(titleText)));
        body.AppendChild(chapterTitle);

        // Chapter content
        if (!string.IsNullOrEmpty(chapter.Content))
        {
            AddFormattedContent(body, chapter.Content, options);
        }
        else if (chapter.Scenes?.Any() == true)
        {
            // Add scenes with scene breaks
            var isFirstScene = true;
            foreach (var scene in chapter.Scenes.OrderBy(s => s.Order))
            {
                if (!isFirstScene && options.IncludeSceneBreaks)
                {
                    // Scene break
                    var sceneBreak = new Paragraph(
                        new ParagraphProperties(new ParagraphStyleId { Val = "SceneBreak" }),
                        new Run(new Text(options.SceneBreakSymbol)));
                    body.AppendChild(sceneBreak);
                }

                if (!string.IsNullOrEmpty(scene.Content))
                {
                    AddFormattedContent(body, scene.Content, options);
                }

                isFirstScene = false;
            }
        }
    }

    private void AddFormattedContent(Body body, string content, ExportOptions options)
    {
        // Split content into paragraphs
        var paragraphs = content.Split(
            new[] { "\r\n\r\n", "\n\n" },
            StringSplitOptions.RemoveEmptyEntries);

        foreach (var paraText in paragraphs)
        {
            var cleanText = paraText.Trim();
            if (string.IsNullOrEmpty(cleanText)) continue;

            var paragraph = new Paragraph(
                new ParagraphProperties(new ParagraphStyleId { Val = "Normal" }));

            // Handle basic formatting (bold, italic)
            var formattedRuns = ParseFormattedText(cleanText);
            foreach (var run in formattedRuns)
            {
                paragraph.AppendChild(run);
            }

            body.AppendChild(paragraph);
        }
    }

    private IEnumerable<Run> ParseFormattedText(string text)
    {
        // Simple regex-based parsing for *italic* and **bold**
        var pattern = BoldItalicRegex();
        var lastIndex = 0;
        var runs = new List<Run>();

        foreach (Match match in pattern.Matches(text))
        {
            // Add text before match
            if (match.Index > lastIndex)
            {
                var beforeText = text.Substring(lastIndex, match.Index - lastIndex);
                runs.Add(new Run(new Text(beforeText) { Space = SpaceProcessingModeValues.Preserve }));
            }

            var content = match.Groups[2].Value;
            var marker = match.Groups[1].Value;

            var run = new Run();
            var runProps = new RunProperties();

            if (marker == "**")
            {
                runProps.AppendChild(new Bold());
            }
            else if (marker == "*" || marker == "_")
            {
                runProps.AppendChild(new Italic());
            }

            run.AppendChild(runProps);
            run.AppendChild(new Text(content) { Space = SpaceProcessingModeValues.Preserve });
            runs.Add(run);

            lastIndex = match.Index + match.Length;
        }

        // Add remaining text
        if (lastIndex < text.Length)
        {
            runs.Add(new Run(new Text(text.Substring(lastIndex)) { Space = SpaceProcessingModeValues.Preserve }));
        }

        // If no formatting found, return plain text
        if (runs.Count == 0)
        {
            runs.Add(new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve }));
        }

        return runs;
    }

    [GeneratedRegex(@"(\*\*|\*|_)([^*_]+)\1")]
    private static partial Regex BoldItalicRegex();
}