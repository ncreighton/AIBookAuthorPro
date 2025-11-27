// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Windows.Documents;
using AIBookAuthorPro.Core.Common;

namespace AIBookAuthorPro.Core.Interfaces;

/// <summary>
/// Defines the contract for FlowDocument conversion and manipulation services.
/// </summary>
public interface IFlowDocumentService
{
    /// <summary>
    /// Converts a FlowDocument to XAML string representation.
    /// </summary>
    /// <param name="document">The FlowDocument to convert.</param>
    /// <returns>Result containing the XAML string.</returns>
    Result<string> ToXaml(FlowDocument document);

    /// <summary>
    /// Converts a XAML string to a FlowDocument.
    /// </summary>
    /// <param name="xaml">The XAML string to convert.</param>
    /// <returns>Result containing the FlowDocument.</returns>
    Result<FlowDocument> FromXaml(string xaml);

    /// <summary>
    /// Converts a FlowDocument to Markdown format.
    /// </summary>
    /// <param name="document">The FlowDocument to convert.</param>
    /// <returns>Result containing the Markdown string.</returns>
    Result<string> ToMarkdown(FlowDocument document);

    /// <summary>
    /// Converts Markdown text to a FlowDocument.
    /// </summary>
    /// <param name="markdown">The Markdown string to convert.</param>
    /// <returns>Result containing the FlowDocument.</returns>
    Result<FlowDocument> FromMarkdown(string markdown);

    /// <summary>
    /// Extracts plain text content from a FlowDocument.
    /// </summary>
    /// <param name="document">The FlowDocument to extract text from.</param>
    /// <returns>Result containing the plain text.</returns>
    Result<string> ToPlainText(FlowDocument document);

    /// <summary>
    /// Creates an empty FlowDocument with default formatting.
    /// </summary>
    /// <returns>A new FlowDocument instance.</returns>
    FlowDocument CreateEmpty();

    /// <summary>
    /// Creates a FlowDocument from plain text.
    /// </summary>
    /// <param name="text">The plain text content.</param>
    /// <returns>A FlowDocument containing the text.</returns>
    FlowDocument FromPlainText(string text);

    /// <summary>
    /// Calculates the word count in a FlowDocument.
    /// </summary>
    /// <param name="document">The FlowDocument to count.</param>
    /// <returns>The word count.</returns>
    int GetWordCount(FlowDocument document);

    /// <summary>
    /// Gets the character count in a FlowDocument.
    /// </summary>
    /// <param name="document">The FlowDocument to count.</param>
    /// <returns>The character count.</returns>
    int GetCharacterCount(FlowDocument document);

    /// <summary>
    /// Merges content from one FlowDocument into another.
    /// </summary>
    /// <param name="target">The target FlowDocument.</param>
    /// <param name="source">The source FlowDocument to append.</param>
    /// <returns>Result indicating success or failure.</returns>
    Result MergeDocuments(FlowDocument target, FlowDocument source);

    /// <summary>
    /// Inserts text at the current selection or end of document.
    /// </summary>
    /// <param name="document">The FlowDocument to modify.</param>
    /// <param name="text">The text to insert.</param>
    /// <param name="position">Optional position to insert at.</param>
    /// <returns>Result indicating success or failure.</returns>
    Result InsertText(FlowDocument document, string text, int? position = null);
}
