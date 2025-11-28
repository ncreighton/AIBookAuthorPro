// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Enums;

namespace AIBookAuthorPro.Core.Models.KDP;

/// <summary>
/// Represents KDP publishing metadata for a book.
/// </summary>
public sealed class KDPMetadata : Entity
{
    /// <summary>
    /// Gets or sets the ASIN (Amazon Standard Identification Number).
    /// </summary>
    public string? ASIN { get; set; }

    /// <summary>
    /// Gets or sets the ISBN-10 for print edition.
    /// </summary>
    public string? ISBN10 { get; set; }

    /// <summary>
    /// Gets or sets the ISBN-13 for print edition.
    /// </summary>
    public string? ISBN13 { get; set; }

    /// <summary>
    /// Gets or sets whether to use KDP-provided free ISBN.
    /// </summary>
    public bool UseKDPFreeISBN { get; set; } = true;

    /// <summary>
    /// Gets or sets the primary BISAC category.
    /// </summary>
    public KDPCategory? PrimaryCategory { get; set; }

    /// <summary>
    /// Gets or sets the secondary BISAC category.
    /// </summary>
    public KDPCategory? SecondaryCategory { get; set; }

    /// <summary>
    /// Gets or sets the search keywords (max 7).
    /// </summary>
    public List<string> Keywords { get; set; } = [];

    /// <summary>
    /// Gets or sets the book description/blurb (up to 4000 chars).
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the series information.
    /// </summary>
    public SeriesInfo? Series { get; set; }

    /// <summary>
    /// Gets or sets the edition number.
    /// </summary>
    public int EditionNumber { get; set; } = 1;

    /// <summary>
    /// Gets or sets the publication date.
    /// </summary>
    public DateTime? PublicationDate { get; set; }

    /// <summary>
    /// Gets or sets whether this book contains adult content.
    /// </summary>
    public bool AdultContent { get; set; }

    /// <summary>
    /// Gets or sets whether this is public domain content.
    /// </summary>
    public bool PublicDomain { get; set; }

    /// <summary>
    /// Gets or sets the primary language.
    /// </summary>
    public string Language { get; set; } = "English";

    /// <summary>
    /// Gets or sets whether this content is AI-generated.
    /// </summary>
    public bool AIGenerated { get; set; } = true;

    /// <summary>
    /// Gets or sets whether this book is part of KDP Select/Kindle Unlimited.
    /// </summary>
    public bool KDPSelect { get; set; }

    /// <summary>
    /// Gets or sets the publication status by marketplace.
    /// </summary>
    public Dictionary<KDPMarketplace, PublicationStatus> MarketplaceStatus { get; set; } = [];
}

/// <summary>
/// Represents a KDP browse category.
/// </summary>
public sealed class KDPCategory
{
    /// <summary>
    /// Gets or sets the category ID.
    /// </summary>
    public string CategoryId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category path (e.g., "Fiction > Fantasy > Epic").
    /// </summary>
    public string CategoryPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the BISAC code.
    /// </summary>
    public string? BISACCode { get; set; }

    /// <summary>
    /// Gets or sets the category name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this is a top-level category.
    /// </summary>
    public bool IsTopLevel { get; set; }

    /// <summary>
    /// Gets or sets the parent category ID.
    /// </summary>
    public string? ParentCategoryId { get; set; }
}

/// <summary>
/// Represents the publication status for a marketplace.
/// </summary>
public sealed class PublicationStatus
{
    /// <summary>
    /// Gets or sets whether published in this marketplace.
    /// </summary>
    public bool IsPublished { get; set; }

    /// <summary>
    /// Gets or sets the publication date.
    /// </summary>
    public DateTime? PublishedDate { get; set; }

    /// <summary>
    /// Gets or sets the price.
    /// </summary>
    public decimal? Price { get; set; }

    /// <summary>
    /// Gets or sets the currency code.
    /// </summary>
    public string? CurrencyCode { get; set; }

    /// <summary>
    /// Gets or sets the product URL.
    /// </summary>
    public string? ProductUrl { get; set; }

    /// <summary>
    /// Gets or sets the current sales rank.
    /// </summary>
    public int? SalesRank { get; set; }

    /// <summary>
    /// Gets or sets the number of reviews.
    /// </summary>
    public int ReviewCount { get; set; }

    /// <summary>
    /// Gets or sets the average rating (1-5).
    /// </summary>
    public double? AverageRating { get; set; }
}

/// <summary>
/// Represents book series information.
/// </summary>
public sealed class SeriesInfo : Entity
{
    /// <summary>
    /// Gets or sets the series name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the position in the series.
    /// </summary>
    public int Position { get; set; } = 1;

    /// <summary>
    /// Gets or sets the total planned books in series.
    /// </summary>
    public int? TotalBooks { get; set; }

    /// <summary>
    /// Gets or sets the series status.
    /// </summary>
    public SeriesStatus Status { get; set; } = SeriesStatus.InProgress;

    /// <summary>
    /// Gets or sets the series description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the series page/product page URL.
    /// </summary>
    public string? SeriesUrl { get; set; }

    /// <summary>
    /// Gets or sets book IDs in this series.
    /// </summary>
    public List<Guid> BookIds { get; set; } = [];
}

/// <summary>
/// Represents pricing information for a book.
/// </summary>
public sealed class PricingInfo
{
    /// <summary>
    /// Gets or sets the list price.
    /// </summary>
    public decimal ListPrice { get; set; }

    /// <summary>
    /// Gets or sets the currency.
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Gets or sets the royalty percentage (35% or 70%).
    /// </summary>
    public int RoyaltyPercentage { get; set; } = 70;

    /// <summary>
    /// Gets or sets the delivery cost (for 70% royalty).
    /// </summary>
    public decimal DeliveryCost { get; set; }

    /// <summary>
    /// Gets or sets the estimated royalty per sale.
    /// </summary>
    public decimal EstimatedRoyalty { get; set; }

    /// <summary>
    /// Gets or sets whether price matching is enabled.
    /// </summary>
    public bool PriceMatchingEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets marketplace-specific pricing.
    /// </summary>
    public Dictionary<KDPMarketplace, decimal> MarketplacePrices { get; set; } = [];
}

/// <summary>
/// Represents print book specifications.
/// </summary>
public sealed class PrintSpecifications
{
    /// <summary>
    /// Gets or sets the book format type.
    /// </summary>
    public BookFormatType FormatType { get; set; } = BookFormatType.Paperback;

    /// <summary>
    /// Gets or sets the trim size.
    /// </summary>
    public TrimSize TrimSize { get; set; } = TrimSize.Size6x9;

    /// <summary>
    /// Gets or sets bleed settings (for images extending to edge).
    /// </summary>
    public bool HasBleed { get; set; }

    /// <summary>
    /// Gets or sets the paper type.
    /// </summary>
    public PaperType PaperType { get; set; } = PaperType.Cream;

    /// <summary>
    /// Gets or sets the cover finish.
    /// </summary>
    public CoverFinish CoverFinish { get; set; } = CoverFinish.Matte;

    /// <summary>
    /// Gets or sets the page count.
    /// </summary>
    public int PageCount { get; set; }

    /// <summary>
    /// Gets or sets the calculated spine width in inches.
    /// </summary>
    public double SpineWidth => CalculateSpineWidth();

    /// <summary>
    /// Gets or sets the printing cost per book.
    /// </summary>
    public decimal PrintingCost { get; set; }

    /// <summary>
    /// Gets or sets the minimum list price.
    /// </summary>
    public decimal MinimumListPrice { get; set; }

    private double CalculateSpineWidth()
    {
        // KDP spine calculation:
        // White paper: Page count × 0.002252"
        // Cream paper: Page count × 0.0025"
        return PaperType == PaperType.Cream
            ? PageCount * 0.0025
            : PageCount * 0.002252;
    }
}

/// <summary>
/// Represents an AI-generated book description suggestion.
/// </summary>
public sealed class BookDescriptionSuggestion
{
    /// <summary>
    /// Gets or sets the generated description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the style of the description.
    /// </summary>
    public string Style { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the character count.
    /// </summary>
    public int CharacterCount => Description.Length;

    /// <summary>
    /// Gets or sets whether it contains HTML formatting.
    /// </summary>
    public bool HasHtmlFormatting { get; set; }

    /// <summary>
    /// Gets or sets the hook/opening line.
    /// </summary>
    public string? Hook { get; set; }

    /// <summary>
    /// Gets or sets the call to action.
    /// </summary>
    public string? CallToAction { get; set; }
}

/// <summary>
/// Represents AI-generated keyword suggestions.
/// </summary>
public sealed class KeywordSuggestion
{
    /// <summary>
    /// Gets or sets the keyword.
    /// </summary>
    public string Keyword { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the relevance score (0-1).
    /// </summary>
    public double RelevanceScore { get; set; }

    /// <summary>
    /// Gets or sets estimated search volume (if available).
    /// </summary>
    public int? EstimatedSearchVolume { get; set; }

    /// <summary>
    /// Gets or sets competition level (Low, Medium, High).
    /// </summary>
    public string? CompetitionLevel { get; set; }

    /// <summary>
    /// Gets or sets the category this keyword relates to.
    /// </summary>
    public string? RelatedCategory { get; set; }

    /// <summary>
    /// Gets or sets whether this is a long-tail keyword.
    /// </summary>
    public bool IsLongTail { get; set; }
}

/// <summary>
/// Represents A+ Content (Enhanced Brand Content) for Amazon.
/// </summary>
public sealed class APlusContent : Entity
{
    /// <summary>
    /// Gets or sets the content name/title.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the modules in this A+ content.
    /// </summary>
    public List<APlusModule> Modules { get; set; } = [];

    /// <summary>
    /// Gets or sets the ASINs this content applies to.
    /// </summary>
    public List<string> AppliedASINs { get; set; } = [];

    /// <summary>
    /// Gets or sets whether this content is published.
    /// </summary>
    public bool IsPublished { get; set; }

    /// <summary>
    /// Gets or sets the status message.
    /// </summary>
    public string? Status { get; set; }
}

/// <summary>
/// Represents a module within A+ Content.
/// </summary>
public sealed class APlusModule
{
    /// <summary>
    /// Gets or sets the module type.
    /// </summary>
    public APlusModuleType Type { get; set; }

    /// <summary>
    /// Gets or sets the headline text.
    /// </summary>
    public string? Headline { get; set; }

    /// <summary>
    /// Gets or sets the body text.
    /// </summary>
    public string? BodyText { get; set; }

    /// <summary>
    /// Gets or sets image URLs for this module.
    /// </summary>
    public List<string> ImageUrls { get; set; } = [];

    /// <summary>
    /// Gets or sets the alt text for images.
    /// </summary>
    public List<string> ImageAltTexts { get; set; } = [];

    /// <summary>
    /// Gets or sets the order of this module.
    /// </summary>
    public int Order { get; set; }
}
