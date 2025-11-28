// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Enums;

namespace AIBookAuthorPro.Core.Models.Covers;

/// <summary>
/// Represents a book cover design.
/// </summary>
public sealed class BookCover : Entity
{
    /// <summary>
    /// Gets or sets the cover title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the cover style.
    /// </summary>
    public CoverStyle Style { get; set; } = CoverStyle.Photographic;

    /// <summary>
    /// Gets or sets the front cover image path.
    /// </summary>
    public string? FrontCoverPath { get; set; }

    /// <summary>
    /// Gets or sets the back cover image path.
    /// </summary>
    public string? BackCoverPath { get; set; }

    /// <summary>
    /// Gets or sets the spine image path.
    /// </summary>
    public string? SpinePath { get; set; }

    /// <summary>
    /// Gets or sets the full cover (front + spine + back) image path.
    /// </summary>
    public string? FullCoverPath { get; set; }

    /// <summary>
    /// Gets or sets the cover specifications.
    /// </summary>
    public CoverSpecifications Specifications { get; set; } = new();

    /// <summary>
    /// Gets or sets the text elements on the cover.
    /// </summary>
    public CoverTextElements TextElements { get; set; } = new();

    /// <summary>
    /// Gets or sets the cover template used.
    /// </summary>
    public CoverTemplate? Template { get; set; }

    /// <summary>
    /// Gets or sets the generation prompt used.
    /// </summary>
    public string? GenerationPrompt { get; set; }

    /// <summary>
    /// Gets or sets the AI provider used for generation.
    /// </summary>
    public ImageGenerationProvider? GenerationProvider { get; set; }

    /// <summary>
    /// Gets or sets whether this is the active cover.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the version number.
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// Gets or sets cover variations.
    /// </summary>
    public List<CoverVariation> Variations { get; set; } = [];
}

/// <summary>
/// Represents cover specifications for printing.
/// </summary>
public sealed class CoverSpecifications
{
    /// <summary>
    /// Gets or sets the width in pixels.
    /// </summary>
    public int WidthPixels { get; set; } = 1600;

    /// <summary>
    /// Gets or sets the height in pixels.
    /// </summary>
    public int HeightPixels { get; set; } = 2560;

    /// <summary>
    /// Gets or sets the DPI (dots per inch).
    /// </summary>
    public int DPI { get; set; } = 300;

    /// <summary>
    /// Gets or sets the trim size.
    /// </summary>
    public TrimSize TrimSize { get; set; } = TrimSize.Size6x9;

    /// <summary>
    /// Gets or sets the spine width in inches.
    /// </summary>
    public double SpineWidthInches { get; set; }

    /// <summary>
    /// Gets or sets the bleed in inches.
    /// </summary>
    public double BleedInches { get; set; } = 0.125;

    /// <summary>
    /// Gets or sets whether this has bleed.
    /// </summary>
    public bool HasBleed { get; set; } = true;

    /// <summary>
    /// Gets or sets the paper type for spine calculation.
    /// </summary>
    public PaperType PaperType { get; set; } = PaperType.Cream;

    /// <summary>
    /// Gets or sets the page count for spine calculation.
    /// </summary>
    public int PageCount { get; set; }

    /// <summary>
    /// Gets the calculated spine width.
    /// </summary>
    public double CalculatedSpineWidth => PaperType == PaperType.Cream
        ? PageCount * 0.0025
        : PageCount * 0.002252;

    /// <summary>
    /// Gets the full cover width in inches (front + spine + back + bleeds).
    /// </summary>
    public double FullCoverWidthInches
    {
        get
        {
            var (width, _) = GetTrimSizeDimensions(TrimSize);
            return (width * 2) + CalculatedSpineWidth + (BleedInches * 2);
        }
    }

    /// <summary>
    /// Gets the full cover height in inches (height + bleeds).
    /// </summary>
    public double FullCoverHeightInches
    {
        get
        {
            var (_, height) = GetTrimSizeDimensions(TrimSize);
            return height + (BleedInches * 2);
        }
    }

    /// <summary>
    /// Gets the full cover width in pixels.
    /// </summary>
    public int FullCoverWidthPixels => (int)(FullCoverWidthInches * DPI);

    /// <summary>
    /// Gets the full cover height in pixels.
    /// </summary>
    public int FullCoverHeightPixels => (int)(FullCoverHeightInches * DPI);

    private static (double width, double height) GetTrimSizeDimensions(TrimSize size) => size switch
    {
        TrimSize.Size5x8 => (5.0, 8.0),
        TrimSize.Size525x8 => (5.25, 8.0),
        TrimSize.Size55x85 => (5.5, 8.5),
        TrimSize.Size6x9 => (6.0, 9.0),
        TrimSize.Size614x921 => (6.14, 9.21),
        TrimSize.Size669x961 => (6.69, 9.61),
        TrimSize.Size7x10 => (7.0, 10.0),
        TrimSize.Size744x969 => (7.44, 9.69),
        TrimSize.Size75x925 => (7.5, 9.25),
        TrimSize.Size8x10 => (8.0, 10.0),
        TrimSize.Size825x6 => (8.25, 6.0),
        TrimSize.Size825x825 => (8.25, 8.25),
        TrimSize.Size85x85 => (8.5, 8.5),
        TrimSize.Size85x11 => (8.5, 11.0),
        _ => (6.0, 9.0)
    };
}

/// <summary>
/// Represents text elements on a book cover.
/// </summary>
public sealed class CoverTextElements
{
    /// <summary>
    /// Gets or sets the title configuration.
    /// </summary>
    public TextElement Title { get; set; } = new() { Text = string.Empty, FontSize = 72 };

    /// <summary>
    /// Gets or sets the subtitle configuration.
    /// </summary>
    public TextElement? Subtitle { get; set; }

    /// <summary>
    /// Gets or sets the author name configuration.
    /// </summary>
    public TextElement AuthorName { get; set; } = new() { Text = string.Empty, FontSize = 36 };

    /// <summary>
    /// Gets or sets the series name configuration.
    /// </summary>
    public TextElement? SeriesName { get; set; }

    /// <summary>
    /// Gets or sets the tagline configuration.
    /// </summary>
    public TextElement? Tagline { get; set; }

    /// <summary>
    /// Gets or sets the spine title configuration.
    /// </summary>
    public TextElement? SpineTitle { get; set; }

    /// <summary>
    /// Gets or sets the spine author configuration.
    /// </summary>
    public TextElement? SpineAuthor { get; set; }

    /// <summary>
    /// Gets or sets the back cover blurb.
    /// </summary>
    public TextElement? BackCoverBlurb { get; set; }

    /// <summary>
    /// Gets or sets the back cover author bio.
    /// </summary>
    public TextElement? BackCoverAuthorBio { get; set; }

    /// <summary>
    /// Gets or sets review quotes for the back cover.
    /// </summary>
    public List<TextElement> ReviewQuotes { get; set; } = [];
}

/// <summary>
/// Represents a text element on the cover.
/// </summary>
public sealed class TextElement
{
    /// <summary>
    /// Gets or sets the text content.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the font family.
    /// </summary>
    public string FontFamily { get; set; } = "Georgia";

    /// <summary>
    /// Gets or sets the font size in points.
    /// </summary>
    public double FontSize { get; set; } = 24;

    /// <summary>
    /// Gets or sets whether the text is bold.
    /// </summary>
    public bool IsBold { get; set; }

    /// <summary>
    /// Gets or sets whether the text is italic.
    /// </summary>
    public bool IsItalic { get; set; }

    /// <summary>
    /// Gets or sets the text color in hex.
    /// </summary>
    public string Color { get; set; } = "#FFFFFF";

    /// <summary>
    /// Gets or sets the text alignment.
    /// </summary>
    public string Alignment { get; set; } = "Center";

    /// <summary>
    /// Gets or sets the X position (percentage from left).
    /// </summary>
    public double PositionX { get; set; } = 50;

    /// <summary>
    /// Gets or sets the Y position (percentage from top).
    /// </summary>
    public double PositionY { get; set; } = 50;

    /// <summary>
    /// Gets or sets whether the text has a shadow.
    /// </summary>
    public bool HasShadow { get; set; }

    /// <summary>
    /// Gets or sets the shadow color in hex.
    /// </summary>
    public string ShadowColor { get; set; } = "#000000";

    /// <summary>
    /// Gets or sets the shadow offset.
    /// </summary>
    public double ShadowOffset { get; set; } = 2;
}

/// <summary>
/// Represents a cover template.
/// </summary>
public sealed class CoverTemplate : Entity
{
    /// <summary>
    /// Gets or sets the template name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the template description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the template category (genre).
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the preview image path.
    /// </summary>
    public string? PreviewPath { get; set; }

    /// <summary>
    /// Gets or sets the template style.
    /// </summary>
    public CoverStyle Style { get; set; }

    /// <summary>
    /// Gets or sets the color scheme.
    /// </summary>
    public ColorScheme Colors { get; set; } = new();

    /// <summary>
    /// Gets or sets default text elements.
    /// </summary>
    public CoverTextElements DefaultTextElements { get; set; } = new();

    /// <summary>
    /// Gets or sets the base prompt for AI generation.
    /// </summary>
    public string? BasePrompt { get; set; }

    /// <summary>
    /// Gets or sets whether this is a built-in template.
    /// </summary>
    public bool IsBuiltIn { get; set; }

    /// <summary>
    /// Gets or sets the compatible trim sizes.
    /// </summary>
    public List<TrimSize> CompatibleTrimSizes { get; set; } = [];
}

/// <summary>
/// Represents a color scheme for covers.
/// </summary>
public sealed class ColorScheme
{
    /// <summary>
    /// Gets or sets the primary color.
    /// </summary>
    public string PrimaryColor { get; set; } = "#1976D2";

    /// <summary>
    /// Gets or sets the secondary color.
    /// </summary>
    public string SecondaryColor { get; set; } = "#FF6F00";

    /// <summary>
    /// Gets or sets the background color.
    /// </summary>
    public string BackgroundColor { get; set; } = "#000000";

    /// <summary>
    /// Gets or sets the text color.
    /// </summary>
    public string TextColor { get; set; } = "#FFFFFF";

    /// <summary>
    /// Gets or sets the accent color.
    /// </summary>
    public string AccentColor { get; set; } = "#FFD700";
}

/// <summary>
/// Represents a variation of a cover design.
/// </summary>
public sealed class CoverVariation
{
    /// <summary>
    /// Gets or sets the variation ID.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the variation name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the image path.
    /// </summary>
    public string ImagePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the generation prompt.
    /// </summary>
    public string? Prompt { get; set; }

    /// <summary>
    /// Gets or sets when this was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the rating (1-5).
    /// </summary>
    public int? Rating { get; set; }

    /// <summary>
    /// Gets or sets notes about this variation.
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Represents a request for AI cover generation.
/// </summary>
public sealed class CoverGenerationRequest
{
    /// <summary>
    /// Gets or sets the book title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the author name.
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the genre.
    /// </summary>
    public string Genre { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the book description/summary.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the desired cover style.
    /// </summary>
    public CoverStyle Style { get; set; } = CoverStyle.Photographic;

    /// <summary>
    /// Gets or sets the mood/atmosphere.
    /// </summary>
    public string? Mood { get; set; }

    /// <summary>
    /// Gets or sets specific elements to include.
    /// </summary>
    public List<string> IncludeElements { get; set; } = [];

    /// <summary>
    /// Gets or sets elements to exclude.
    /// </summary>
    public List<string> ExcludeElements { get; set; } = [];

    /// <summary>
    /// Gets or sets the color scheme preference.
    /// </summary>
    public ColorScheme? ColorScheme { get; set; }

    /// <summary>
    /// Gets or sets the image generation provider.
    /// </summary>
    public ImageGenerationProvider Provider { get; set; } = ImageGenerationProvider.DallE;

    /// <summary>
    /// Gets or sets the number of variations to generate.
    /// </summary>
    public int VariationCount { get; set; } = 4;

    /// <summary>
    /// Gets or sets the cover specifications.
    /// </summary>
    public CoverSpecifications Specifications { get; set; } = new();

    /// <summary>
    /// Gets or sets additional prompt instructions.
    /// </summary>
    public string? AdditionalInstructions { get; set; }

    /// <summary>
    /// Gets or sets reference images for style matching.
    /// </summary>
    public List<string> ReferenceImages { get; set; } = [];
}

/// <summary>
/// Represents the result of cover generation.
/// </summary>
public sealed class CoverGenerationResult
{
    /// <summary>
    /// Gets or sets whether generation was successful.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Gets or sets the error message if failed.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Gets or sets the generated variations.
    /// </summary>
    public List<CoverVariation> Variations { get; set; } = [];

    /// <summary>
    /// Gets or sets the prompt used for generation.
    /// </summary>
    public string? UsedPrompt { get; set; }

    /// <summary>
    /// Gets or sets the generation duration.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets the cost of generation.
    /// </summary>
    public decimal? Cost { get; set; }
}
