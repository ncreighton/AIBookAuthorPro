// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xml;
using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.Infrastructure.Services;

/// <summary>
/// Service for converting between FlowDocument and various text formats.
/// </summary>
public sealed partial class FlowDocumentService : IFlowDocumentService
{
    private readonly ILogger<FlowDocumentService> _logger;

    /// <summary>
    /// Initializes a new instance of the FlowDocumentService.
    /// </summary>
    public FlowDocumentService(ILogger<FlowDocumentService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Result<string> ToXaml(FlowDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        try
        {
            _logger.LogDebug("Converting FlowDocument to XAML");

            using var stringWriter = new StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                OmitXmlDeclaration = true
            });

            XamlWriter.Save(document, xmlWriter);
            var xaml = stringWriter.ToString();

            _logger.LogDebug("Converted FlowDocument to XAML ({Length} chars)", xaml.Length);
            return Result<string>.Success(xaml);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to convert FlowDocument to XAML");
            return Result<string>.Failure($"Failed to convert to XAML: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public Result<FlowDocument> FromXaml(string xaml)
    {
        if (string.IsNullOrWhiteSpace(xaml))
        {
            return Result<FlowDocument>.Success(CreateEmpty());
        }

        try
        {
            _logger.LogDebug("Converting XAML to FlowDocument ({Length} chars)", xaml.Length);

            using var stringReader = new StringReader(xaml);
            using var xmlReader = XmlReader.Create(stringReader);

            var document = (FlowDocument)XamlReader.Load(xmlReader);
            ApplyDefaultStyles(document);

            _logger.LogDebug("Converted XAML to FlowDocument");
            return Result<FlowDocument>.Success(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to convert XAML to FlowDocument");
            return Result<FlowDocument>.Failure($"Failed to convert from XAML: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public Result<string> ToMarkdown(FlowDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        try
        {
            _logger.LogDebug("Converting FlowDocument to Markdown");

            var sb = new StringBuilder();
            ConvertBlocksToMarkdown(document.Blocks, sb);

            var markdown = sb.ToString().TrimEnd();
            _logger.LogDebug("Converted FlowDocument to Markdown ({Length} chars)", markdown.Length);

            return Result<string>.Success(markdown);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to convert FlowDocument to Markdown");
            return Result<string>.Failure($"Failed to convert to Markdown: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public Result<FlowDocument> FromMarkdown(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return Result<FlowDocument>.Success(CreateEmpty());
        }

        try
        {
            _logger.LogDebug("Converting Markdown to FlowDocument ({Length} chars)", markdown.Length);

            var document = CreateEmpty();
            var lines = markdown.Split(['\n']);
            var currentParagraph = new Paragraph();
            var inCodeBlock = false;
            var codeBlockContent = new StringBuilder();

            foreach (var rawLine in lines)
            {
                var line = rawLine.TrimEnd('\r');

                // Handle code blocks
                if (line.StartsWith("```"))
                {
                    if (inCodeBlock)
                    {
                        // End code block
                        var codeSection = new Section();
                        codeSection.Blocks.Add(new Paragraph(new Run(codeBlockContent.ToString().TrimEnd()))
                        {
                            FontFamily = new System.Windows.Media.FontFamily("Cascadia Code, Consolas, monospace"),
                            Background = System.Windows.Media.Brushes.LightGray
                        });
                        document.Blocks.Add(codeSection);
                        codeBlockContent.Clear();
                        inCodeBlock = false;
                    }
                    else
                    {
                        // Start code block - flush current paragraph first
                        if (currentParagraph.Inlines.Count > 0)
                        {
                            document.Blocks.Add(currentParagraph);
                            currentParagraph = new Paragraph();
                        }
                        inCodeBlock = true;
                    }
                    continue;
                }

                if (inCodeBlock)
                {
                    codeBlockContent.AppendLine(line);
                    continue;
                }

                // Handle headers
                if (line.StartsWith("### "))
                {
                    FlushParagraph(document, ref currentParagraph);
                    var headerText = line[4..];
                    document.Blocks.Add(CreateHeader(headerText, 3));
                    continue;
                }
                if (line.StartsWith("## "))
                {
                    FlushParagraph(document, ref currentParagraph);
                    var headerText = line[3..];
                    document.Blocks.Add(CreateHeader(headerText, 2));
                    continue;
                }
                if (line.StartsWith("# "))
                {
                    FlushParagraph(document, ref currentParagraph);
                    var headerText = line[2..];
                    document.Blocks.Add(CreateHeader(headerText, 1));
                    continue;
                }

                // Handle horizontal rules
                if (line == "---" || line == "***" || line == "___")
                {
                    FlushParagraph(document, ref currentParagraph);
                    document.Blocks.Add(new Paragraph(new Run("─────────────────────────────────────────"))
                    {
                        TextAlignment = TextAlignment.Center,
                        Foreground = System.Windows.Media.Brushes.Gray
                    });
                    continue;
                }

                // Handle empty lines (paragraph break)
                if (string.IsNullOrWhiteSpace(line))
                {
                    FlushParagraph(document, ref currentParagraph);
                    continue;
                }

                // Handle regular text with inline formatting
                var formattedInlines = ParseInlineMarkdown(line);
                foreach (var inline in formattedInlines)
                {
                    currentParagraph.Inlines.Add(inline);
                }
                currentParagraph.Inlines.Add(new Run(" "));
            }

            // Flush any remaining content
            FlushParagraph(document, ref currentParagraph);

            ApplyDefaultStyles(document);
            _logger.LogDebug("Converted Markdown to FlowDocument");

            return Result<FlowDocument>.Success(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to convert Markdown to FlowDocument");
            return Result<FlowDocument>.Failure($"Failed to convert from Markdown: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public Result<string> ToPlainText(FlowDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        try
        {
            var textRange = new TextRange(
                document.ContentStart,
                document.ContentEnd);

            var text = textRange.Text;
            _logger.LogDebug("Extracted plain text ({Length} chars)", text.Length);

            return Result<string>.Success(text);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract plain text from FlowDocument");
            return Result<string>.Failure($"Failed to extract plain text: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public FlowDocument CreateEmpty()
    {
        var document = new FlowDocument();
        ApplyDefaultStyles(document);
        return document;
    }

    /// <inheritdoc />
    public FlowDocument FromPlainText(string text)
    {
        var document = CreateEmpty();

        if (string.IsNullOrEmpty(text))
            return document;

        var paragraphs = text.Split(["\r\n\r\n", "\n\n"], StringSplitOptions.None);

        foreach (var para in paragraphs)
        {
            var trimmed = para.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                document.Blocks.Add(new Paragraph(new Run(trimmed)));
            }
        }

        if (document.Blocks.Count == 0)
        {
            document.Blocks.Add(new Paragraph(new Run(text)));
        }

        return document;
    }

    /// <inheritdoc />
    public int GetWordCount(FlowDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var textResult = ToPlainText(document);
        if (textResult.IsFailure || string.IsNullOrWhiteSpace(textResult.Value))
            return 0;

        return textResult.Value.Split(
            [' ', '\t', '\n', '\r'],
            StringSplitOptions.RemoveEmptyEntries).Length;
    }

    /// <inheritdoc />
    public int GetCharacterCount(FlowDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var textResult = ToPlainText(document);
        if (textResult.IsFailure)
            return 0;

        return textResult.Value?.Length ?? 0;
    }

    /// <inheritdoc />
    public Result MergeDocuments(FlowDocument target, FlowDocument source)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(source);

        try
        {
            _logger.LogDebug("Merging FlowDocuments");

            // Serialize and deserialize to clone blocks
            var xamlResult = ToXaml(source);
            if (xamlResult.IsFailure)
                return Result.Failure(xamlResult.Error!);

            var clonedResult = FromXaml(xamlResult.Value!);
            if (clonedResult.IsFailure)
                return Result.Failure(clonedResult.Error!);

            foreach (var block in clonedResult.Value!.Blocks.ToList())
            {
                clonedResult.Value.Blocks.Remove(block);
                target.Blocks.Add(block);
            }

            _logger.LogDebug("Merged FlowDocuments successfully");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to merge FlowDocuments");
            return Result.Failure($"Failed to merge documents: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public Result InsertText(FlowDocument document, string text, int? position = null)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (string.IsNullOrEmpty(text))
            return Result.Success();

        try
        {
            _logger.LogDebug("Inserting text into FlowDocument ({Length} chars)", text.Length);

            if (document.Blocks.LastBlock is Paragraph lastParagraph)
            {
                lastParagraph.Inlines.Add(new Run(text));
            }
            else
            {
                document.Blocks.Add(new Paragraph(new Run(text)));
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to insert text into FlowDocument");
            return Result.Failure($"Failed to insert text: {ex.Message}");
        }
    }

    #region Private Methods

    private static void ApplyDefaultStyles(FlowDocument document)
    {
        document.FontFamily = new System.Windows.Media.FontFamily("Georgia, Cambria, serif");
        document.FontSize = 14;
        document.PagePadding = new Thickness(40);
        document.LineHeight = 24;
        document.TextAlignment = TextAlignment.Left;
    }

    private void ConvertBlocksToMarkdown(BlockCollection blocks, StringBuilder sb)
    {
        foreach (var block in blocks)
        {
            switch (block)
            {
                case Paragraph para:
                    ConvertParagraphToMarkdown(para, sb);
                    sb.AppendLine();
                    sb.AppendLine();
                    break;

                case List list:
                    ConvertListToMarkdown(list, sb);
                    sb.AppendLine();
                    break;

                case Section section:
                    ConvertBlocksToMarkdown(section.Blocks, sb);
                    break;

                case BlockUIContainer:
                    // Skip UI containers
                    break;

                default:
                    _logger.LogDebug("Unknown block type: {BlockType}", block.GetType().Name);
                    break;
            }
        }
    }

    private static void ConvertParagraphToMarkdown(Paragraph para, StringBuilder sb)
    {
        foreach (var inline in para.Inlines)
        {
            ConvertInlineToMarkdown(inline, sb);
        }
    }

    private static void ConvertInlineToMarkdown(Inline inline, StringBuilder sb)
    {
        switch (inline)
        {
            case Run run:
                var text = run.Text;
                if (run.FontWeight == FontWeights.Bold && run.FontStyle == FontStyles.Italic)
                {
                    sb.Append($"***{text}***");
                }
                else if (run.FontWeight == FontWeights.Bold)
                {
                    sb.Append($"**{text}**");
                }
                else if (run.FontStyle == FontStyles.Italic)
                {
                    sb.Append($"*{text}*");
                }
                else
                {
                    sb.Append(text);
                }
                break;

            case Bold bold:
                sb.Append("**");
                foreach (var child in bold.Inlines)
                {
                    ConvertInlineToMarkdown(child, sb);
                }
                sb.Append("**");
                break;

            case Italic italic:
                sb.Append('*');
                foreach (var child in italic.Inlines)
                {
                    ConvertInlineToMarkdown(child, sb);
                }
                sb.Append('*');
                break;

            case Underline underline:
                sb.Append("__");
                foreach (var child in underline.Inlines)
                {
                    ConvertInlineToMarkdown(child, sb);
                }
                sb.Append("__");
                break;

            case Span span:
                foreach (var child in span.Inlines)
                {
                    ConvertInlineToMarkdown(child, sb);
                }
                break;

            case LineBreak:
                sb.AppendLine();
                break;
        }
    }

    private static void ConvertListToMarkdown(List list, StringBuilder sb)
    {
        var index = 1;
        foreach (var item in list.ListItems)
        {
            var prefix = list.MarkerStyle == TextMarkerStyle.Decimal ? $"{index}. " : "- ";
            sb.Append(prefix);

            foreach (var block in item.Blocks)
            {
                if (block is Paragraph para)
                {
                    ConvertParagraphToMarkdown(para, sb);
                }
            }

            sb.AppendLine();
            index++;
        }
    }

    private static void FlushParagraph(FlowDocument document, ref Paragraph currentParagraph)
    {
        if (currentParagraph.Inlines.Count > 0)
        {
            document.Blocks.Add(currentParagraph);
            currentParagraph = new Paragraph();
        }
    }

    private static Paragraph CreateHeader(string text, int level)
    {
        var fontSize = level switch
        {
            1 => 28.0,
            2 => 22.0,
            3 => 18.0,
            _ => 16.0
        };

        return new Paragraph(new Run(text))
        {
            FontSize = fontSize,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, level == 1 ? 16 : 12, 0, 8)
        };
    }

    private static List<Inline> ParseInlineMarkdown(string line)
    {
        var inlines = new List<Inline>();

        // Process bold+italic (***text***)
        var boldItalicPattern = BoldItalicRegex();
        // Process bold (**text**)
        var boldPattern = BoldRegex();
        // Process italic (*text*)
        var italicPattern = ItalicRegex();

        var processed = line;
        var segments = new List<(string text, bool bold, bool italic)>();

        // Simple processing - split and mark
        var currentIndex = 0;
        while (currentIndex < processed.Length)
        {
            // Check for bold+italic
            var boldItalicMatch = boldItalicPattern.Match(processed, currentIndex);
            var boldMatch = boldPattern.Match(processed, currentIndex);
            var italicMatch = italicPattern.Match(processed, currentIndex);

            Match? firstMatch = null;
            var matchType = 0; // 0=none, 1=bold+italic, 2=bold, 3=italic

            if (boldItalicMatch.Success && (firstMatch == null || boldItalicMatch.Index < firstMatch.Index))
            {
                firstMatch = boldItalicMatch;
                matchType = 1;
            }
            if (boldMatch.Success && (firstMatch == null || boldMatch.Index < firstMatch.Index))
            {
                firstMatch = boldMatch;
                matchType = 2;
            }
            if (italicMatch.Success && (firstMatch == null || italicMatch.Index < firstMatch.Index))
            {
                firstMatch = italicMatch;
                matchType = 3;
            }

            if (firstMatch != null && firstMatch.Index == currentIndex)
            {
                var innerText = matchType switch
                {
                    1 => firstMatch.Groups[1].Value,
                    2 => firstMatch.Groups[1].Value,
                    3 => firstMatch.Groups[1].Value,
                    _ => firstMatch.Value
                };

                var run = new Run(innerText);
                if (matchType == 1 || matchType == 2)
                    run.FontWeight = FontWeights.Bold;
                if (matchType == 1 || matchType == 3)
                    run.FontStyle = FontStyles.Italic;

                inlines.Add(run);
                currentIndex = firstMatch.Index + firstMatch.Length;
            }
            else if (firstMatch != null)
            {
                // Add text before the match
                var before = processed.Substring(currentIndex, firstMatch.Index - currentIndex);
                if (!string.IsNullOrEmpty(before))
                    inlines.Add(new Run(before));
                currentIndex = firstMatch.Index;
            }
            else
            {
                // No more matches - add rest of text
                var rest = processed[currentIndex..];
                if (!string.IsNullOrEmpty(rest))
                    inlines.Add(new Run(rest));
                break;
            }
        }

        if (inlines.Count == 0)
        {
            inlines.Add(new Run(line));
        }

        return inlines;
    }

    [GeneratedRegex(@"\*\*\*(.+?)\*\*\*")]
    private static partial Regex BoldItalicRegex();

    [GeneratedRegex(@"\*\*(.+?)\*\*")]
    private static partial Regex BoldRegex();

    [GeneratedRegex(@"(?<!\*)\*([^*]+)\*(?!\*)")]
    private static partial Regex ItalicRegex();

    #endregion
}
