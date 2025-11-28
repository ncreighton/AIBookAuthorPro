// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Models.GuidedCreation;

namespace AIBookAuthorPro.Application.Services.GuidedCreation;

/// <summary>
/// Service for analyzing and expanding user prompts into comprehensive book concepts.
/// </summary>
public interface IPromptAnalysisService
{
    /// <summary>
    /// Analyzes a raw prompt using AI to extract all implicit and explicit book elements.
    /// </summary>
    /// <param name="prompt">The seed prompt to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Comprehensive analysis result.</returns>
    Task<Result<PromptAnalysisResult>> AnalyzePromptAsync(
        BookSeedPrompt prompt,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Expands a brief prompt into a comprehensive creative brief.
    /// </summary>
    /// <param name="prompt">The seed prompt.</param>
    /// <param name="analysis">The analysis result.</param>
    /// <param name="clarifications">User responses to clarification questions.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Expanded creative brief.</returns>
    Task<Result<ExpandedCreativeBrief>> ExpandPromptAsync(
        BookSeedPrompt prompt,
        PromptAnalysisResult analysis,
        Dictionary<Guid, string>? clarifications = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates clarifying questions for ambiguous prompt elements.
    /// </summary>
    /// <param name="analysis">The analysis result.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of clarification requests.</returns>
    Task<Result<List<ClarificationRequest>>> GenerateClarificationsAsync(
        PromptAnalysisResult analysis,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a prompt for minimum requirements.
    /// </summary>
    /// <param name="prompt">The prompt to validate.</param>
    /// <returns>Validation result with any issues.</returns>
    Result<PromptValidationResult> ValidatePrompt(BookSeedPrompt prompt);

    /// <summary>
    /// Suggests enhancements to the prompt based on analysis.
    /// </summary>
    /// <param name="analysis">The analysis result.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of enhancement suggestions.</returns>
    Task<Result<List<EnhancementSuggestion>>> SuggestEnhancementsAsync(
        PromptAnalysisResult analysis,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Imports a prompt from an external AI conversation (Claude, ChatGPT).
    /// </summary>
    /// <param name="conversationText">The conversation text to import.</param>
    /// <param name="source">The source of the conversation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Extracted book seed prompt.</returns>
    Task<Result<BookSeedPrompt>> ImportFromConversationAsync(
        string conversationText,
        PromptSource source,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of prompt validation.
/// </summary>
public sealed record PromptValidationResult
{
    /// <summary>
    /// Whether the prompt is valid.
    /// </summary>
    public required bool IsValid { get; init; }

    /// <summary>
    /// Validation issues found.
    /// </summary>
    public List<PromptValidationIssue> Issues { get; init; } = new();

    /// <summary>
    /// Suggestions for improvement.
    /// </summary>
    public List<string> Suggestions { get; init; } = new();

    /// <summary>
    /// Minimum word count met.
    /// </summary>
    public bool MeetsMinimumLength { get; init; }

    /// <summary>
    /// Estimated clarity score (0-100).
    /// </summary>
    public int ClarityScore { get; init; }
}

/// <summary>
/// Prompt validation issue.
/// </summary>
public sealed record PromptValidationIssue
{
    /// <summary>
    /// Issue type.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Issue message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Severity.
    /// </summary>
    public required ClarificationPriority Severity { get; init; }
}
