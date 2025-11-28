// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Enums;
using AIBookAuthorPro.Core.Models.Covers;

namespace AIBookAuthorPro.Core.Interfaces;

/// <summary>
/// Service for book cover design and generation.
/// </summary>
public interface ICoverService
{
    /// <summary>
    /// Gets the active cover for a project.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The active book cover.</returns>
    Task<Result<BookCover?>> GetActiveCoverAsync(Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all covers for a project.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of book covers.</returns>
    Task<Result<List<BookCover>>> GetAllCoversAsync(Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a book cover.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="cover">The cover to save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success or failure.</returns>
    Task<Result> SaveCoverAsync(Guid projectId, BookCover cover, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a cover as the active cover.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="coverId">The cover ID to set as active.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success or failure.</returns>
    Task<Result> SetActiveCoverAsync(Guid projectId, Guid coverId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a cover.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="coverId">The cover ID to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success or failure.</returns>
    Task<Result> DeleteCoverAsync(Guid projectId, Guid coverId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates AI-powered cover images.
    /// </summary>
    /// <param name="request">The generation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Generation result with cover variations.</returns>
    Task<Result<CoverGenerationResult>> GenerateCoverAsync(
        CoverGenerationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates additional variations for an existing cover.
    /// </summary>
    /// <param name="coverId">The existing cover ID.</param>
    /// <param name="count">Number of variations.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of new variations.</returns>
    Task<Result<List<CoverVariation>>> GenerateVariationsAsync(
        Guid coverId,
        int count = 4,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates AI prompt suggestions for cover generation.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="style">The desired cover style.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of prompt suggestions.</returns>
    Task<Result<List<string>>> GeneratePromptSuggestionsAsync(
        Guid projectId,
        CoverStyle style,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets available cover templates.
    /// </summary>
    /// <param name="genre">Filter by genre (optional).</param>
    /// <param name="style">Filter by style (optional).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of templates.</returns>
    Task<Result<List<CoverTemplate>>> GetTemplatesAsync(
        string? genre = null,
        CoverStyle? style = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a cover from a template.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="templateId">The template ID.</param>
    /// <param name="customizations">Text element customizations.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created cover.</returns>
    Task<Result<BookCover>> CreateFromTemplateAsync(
        Guid projectId,
        Guid templateId,
        CoverTextElements? customizations = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates cover specifications for KDP.
    /// </summary>
    /// <param name="pageCount">Number of pages.</param>
    /// <param name="trimSize">The trim size.</param>
    /// <param name="paperType">The paper type.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Cover specifications.</returns>
    Task<Result<CoverSpecifications>> CalculateSpecificationsAsync(
        int pageCount,
        TrimSize trimSize = TrimSize.Size6x9,
        PaperType paperType = PaperType.Cream,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a full wraparound cover (front + spine + back).
    /// </summary>
    /// <param name="coverId">The cover ID.</param>
    /// <param name="specifications">Cover specifications.</param>
    /// <param name="backCoverContent">Content for back cover.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Path to the full cover image.</returns>
    Task<Result<string>> GenerateFullCoverAsync(
        Guid coverId,
        CoverSpecifications specifications,
        BackCoverContent? backCoverContent = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports a cover for KDP upload.
    /// </summary>
    /// <param name="coverId">The cover ID.</param>
    /// <param name="outputPath">The output file path.</param>
    /// <param name="formatType">The book format type.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success or failure.</returns>
    Task<Result> ExportForKDPAsync(
        Guid coverId,
        string outputPath,
        BookFormatType formatType = BookFormatType.Ebook,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a cover against KDP requirements.
    /// </summary>
    /// <param name="coverId">The cover ID.</param>
    /// <param name="formatType">The book format type.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of validation issues.</returns>
    Task<Result<List<CoverValidationIssue>>> ValidateCoverAsync(
        Guid coverId,
        BookFormatType formatType = BookFormatType.Ebook,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Imports an external cover image.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="imagePath">Path to the image file.</param>
    /// <param name="coverType">The type of cover (front, back, full).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created cover.</returns>
    Task<Result<BookCover>> ImportCoverAsync(
        Guid projectId,
        string imagePath,
        CoverType coverType = CoverType.Front,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the available AI image generation providers.
    /// </summary>
    /// <returns>List of configured providers.</returns>
    List<ImageGenerationProvider> GetAvailableProviders();
}

/// <summary>
/// Represents content for the back cover.
/// </summary>
public sealed class BackCoverContent
{
    /// <summary>
    /// Gets or sets the book blurb/description.
    /// </summary>
    public string? Blurb { get; set; }

    /// <summary>
    /// Gets or sets the author bio.
    /// </summary>
    public string? AuthorBio { get; set; }

    /// <summary>
    /// Gets or sets the author photo path.
    /// </summary>
    public string? AuthorPhotoPath { get; set; }

    /// <summary>
    /// Gets or sets review quotes.
    /// </summary>
    public List<ReviewQuote> Reviews { get; set; } = [];

    /// <summary>
    /// Gets or sets the barcode area (for ISBN).
    /// </summary>
    public bool IncludeBarcode { get; set; } = true;

    /// <summary>
    /// Gets or sets the price.
    /// </summary>
    public string? Price { get; set; }

    /// <summary>
    /// Gets or sets the category text.
    /// </summary>
    public string? Category { get; set; }
}

/// <summary>
/// Represents a review quote for the back cover.
/// </summary>
public sealed class ReviewQuote
{
    /// <summary>
    /// Gets or sets the quote text.
    /// </summary>
    public string Quote { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the reviewer name.
    /// </summary>
    public string ReviewerName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the reviewer publication/title.
    /// </summary>
    public string? ReviewerTitle { get; set; }
}

/// <summary>
/// Represents a cover validation issue.
/// </summary>
public sealed class CoverValidationIssue
{
    /// <summary>
    /// Gets or sets the issue type.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the issue message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the severity (Error, Warning).
    /// </summary>
    public string Severity { get; set; } = "Error";

    /// <summary>
    /// Gets or sets the current value.
    /// </summary>
    public string? CurrentValue { get; set; }

    /// <summary>
    /// Gets or sets the required value.
    /// </summary>
    public string? RequiredValue { get; set; }
}

/// <summary>
/// Represents the type of cover image.
/// </summary>
public enum CoverType
{
    /// <summary>
    /// Front cover only.
    /// </summary>
    Front = 0,

    /// <summary>
    /// Back cover only.
    /// </summary>
    Back = 1,

    /// <summary>
    /// Full wraparound cover.
    /// </summary>
    Full = 2,

    /// <summary>
    /// Spine only.
    /// </summary>
    Spine = 3
}
