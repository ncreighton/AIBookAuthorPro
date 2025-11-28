// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Enums;
using AIBookAuthorPro.Core.Models.KDP;

namespace AIBookAuthorPro.Core.Interfaces;

/// <summary>
/// Service for KDP publishing operations.
/// </summary>
public interface IKDPService
{
    /// <summary>
    /// Gets the KDP metadata for a project.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>KDP metadata.</returns>
    Task<Result<KDPMetadata>> GetMetadataAsync(Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves KDP metadata for a project.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="metadata">The metadata to save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success or failure.</returns>
    Task<r> SaveMetadataAsync(Guid projectId, KDPMetadata metadata, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for KDP categories matching a query.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <param name="bookCategory">The book category (Fiction/NonFiction).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of matching categories.</returns>
    Task<Result<List<KDPCategory>>> SearchCategoriesAsync(
        string query,
        BookCategory? bookCategory = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets AI-suggested categories based on book content.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of suggested categories.</returns>
    Task<Result<List<KDPCategory>>> SuggestCategoriesAsync(Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates AI-powered keyword suggestions.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="count">Number of keywords to generate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of keyword suggestions.</returns>
    Task<Result<List<KeywordSuggestion>>> GenerateKeywordsAsync(
        Guid projectId,
        int count = 7,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates AI-powered book descriptions.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="style">The style of description (e.g., "Hook", "Mystery", "Action").</param>
    /// <param name="maxLength">Maximum character length.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Book description suggestion.</returns>
    Task<Result<BookDescriptionSuggestion>> GenerateDescriptionAsync(
        Guid projectId,
        string? style = null,
        int maxLength = 4000,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates multiple book description variations.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="count">Number of variations.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of description suggestions.</returns>
    Task<Result<List<BookDescriptionSuggestion>>> GenerateDescriptionVariationsAsync(
        Guid projectId,
        int count = 3,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates print specifications and pricing.
    /// </summary>
    /// <param name="pageCount">Number of pages.</param>
    /// <param name="trimSize">The trim size.</param>
    /// <param name="paperType">The paper type.</param>
    /// <param name="hasBleed">Whether cover has bleed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Print specifications with costs.</returns>
    Task<Result<PrintSpecifications>> CalculatePrintSpecsAsync(
        int pageCount,
        TrimSize trimSize = TrimSize.Size6x9,
        PaperType paperType = PaperType.Cream,
        bool hasBleed = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates royalties for a given price.
    /// </summary>
    /// <param name="listPrice">The list price.</param>
    /// <param name="marketplace">The marketplace.</param>
    /// <param name="formatType">The book format.</param>
    /// <param name="printSpecs">Print specifications (for paperback).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Pricing information with royalties.</returns>
    Task<Result<PricingInfo>> CalculateRoyaltiesAsync(
        decimal listPrice,
        KDPMarketplace marketplace = KDPMarketplace.US,
        BookFormatType formatType = BookFormatType.Ebook,
        PrintSpecifications? printSpecs = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets or creates series information.
    /// </summary>
    /// <param name="seriesName">The series name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Series information.</returns>
    Task<Result<SeriesInfo>> GetOrCreateSeriesAsync(string seriesName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all series for the user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of series.</returns>
    Task<Result<List<SeriesInfo>>> GetAllSeriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates A+ Content modules.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="moduleTypes">Types of modules to generate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Generated A+ content.</returns>
    Task<Result<APlusContent>> GenerateAPlusContentAsync(
        Guid projectId,
        List<APlusModuleType>? moduleTypes = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates KDP metadata for completeness.
    /// </summary>
    /// <param name="metadata">The metadata to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of validation issues.</returns>
    Task<Result<List<ValidationIssue>>> ValidateMetadataAsync(
        KDPMetadata metadata,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a validation issue.
/// </summary>
public sealed class ValidationIssue
{
    /// <summary>
    /// Gets or sets the field with the issue.
    /// </summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the issue message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the severity (Error, Warning, Info).
    /// </summary>
    public string Severity { get; set; } = "Error";

    /// <summary>
    /// Gets or sets the suggested fix.
    /// </summary>
    public string? SuggestedFix { get; set; }
}
