// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

namespace AIBookAuthorPro.Core.Enums;

/// <summary>
/// Represents the type of research being performed.
/// </summary>
public enum ResearchType
{
    /// <summary>
    /// General research for any topic.
    /// </summary>
    General = 0,

    /// <summary>
    /// Historical research for period accuracy.
    /// </summary>
    Historical = 1,

    /// <summary>
    /// Location/geography research.
    /// </summary>
    Location = 2,

    /// <summary>
    /// Character name research.
    /// </summary>
    CharacterNames = 3,

    /// <summary>
    /// Cultural research for authenticity.
    /// </summary>
    Cultural = 4,

    /// <summary>
    /// Technical/scientific research.
    /// </summary>
    Technical = 5,

    /// <summary>
    /// Genre tropes and conventions.
    /// </summary>
    GenreTropes = 6,

    /// <summary>
    /// Language/dialect research.
    /// </summary>
    Language = 7,

    /// <summary>
    /// Profession/occupation research.
    /// </summary>
    Profession = 8,

    /// <summary>
    /// Fashion/clothing for a time period.
    /// </summary>
    Fashion = 9,

    /// <summary>
    /// Food and cuisine research.
    /// </summary>
    Food = 10,

    /// <summary>
    /// Weapons and combat research.
    /// </summary>
    Combat = 11,

    /// <summary>
    /// Medical/health research.
    /// </summary>
    Medical = 12,

    /// <summary>
    /// Legal/law research.
    /// </summary>
    Legal = 13,

    /// <summary>
    /// Mythology and folklore.
    /// </summary>
    Mythology = 14
}

/// <summary>
/// Represents KDP marketplace regions.
/// </summary>
public enum KDPMarketplace
{
    /// <summary>
    /// Amazon.com (US)
    /// </summary>
    US = 0,

    /// <summary>
    /// Amazon.co.uk (UK)
    /// </summary>
    UK = 1,

    /// <summary>
    /// Amazon.de (Germany)
    /// </summary>
    DE = 2,

    /// <summary>
    /// Amazon.fr (France)
    /// </summary>
    FR = 3,

    /// <summary>
    /// Amazon.es (Spain)
    /// </summary>
    ES = 4,

    /// <summary>
    /// Amazon.it (Italy)
    /// </summary>
    IT = 5,

    /// <summary>
    /// Amazon.nl (Netherlands)
    /// </summary>
    NL = 6,

    /// <summary>
    /// Amazon.co.jp (Japan)
    /// </summary>
    JP = 7,

    /// <summary>
    /// Amazon.com.br (Brazil)
    /// </summary>
    BR = 8,

    /// <summary>
    /// Amazon.ca (Canada)
    /// </summary>
    CA = 9,

    /// <summary>
    /// Amazon.com.mx (Mexico)
    /// </summary>
    MX = 10,

    /// <summary>
    /// Amazon.com.au (Australia)
    /// </summary>
    AU = 11,

    /// <summary>
    /// Amazon.in (India)
    /// </summary>
    IN = 12
}

/// <summary>
/// Represents book format types for publishing.
/// </summary>
public enum BookFormatType
{
    /// <summary>
    /// Kindle eBook format.
    /// </summary>
    Ebook = 0,

    /// <summary>
    /// Paperback print format.
    /// </summary>
    Paperback = 1,

    /// <summary>
    /// Hardcover print format.
    /// </summary>
    Hardcover = 2,

    /// <summary>
    /// Audio book format.
    /// </summary>
    Audiobook = 3
}

/// <summary>
/// Represents paperback trim sizes for KDP.
/// </summary>
public enum TrimSize
{
    /// <summary>
    /// 5" x 8" - Common trade paperback size.
    /// </summary>
    Size5x8 = 0,

    /// <summary>
    /// 5.25" x 8" - Slightly wider trade paperback.
    /// </summary>
    Size525x8 = 1,

    /// <summary>
    /// 5.5" x 8.5" - Popular fiction size.
    /// </summary>
    Size55x85 = 2,

    /// <summary>
    /// 6" x 9" - Standard trade paperback.
    /// </summary>
    Size6x9 = 3,

    /// <summary>
    /// 6.14" x 9.21" - Royal format.
    /// </summary>
    Size614x921 = 4,

    /// <summary>
    /// 6.69" x 9.61" - Large format.
    /// </summary>
    Size669x961 = 5,

    /// <summary>
    /// 7" x 10" - Technical/textbook size.
    /// </summary>
    Size7x10 = 6,

    /// <summary>
    /// 7.44" x 9.69" - Large format.
    /// </summary>
    Size744x969 = 7,

    /// <summary>
    /// 7.5" x 9.25" - US Trade.
    /// </summary>
    Size75x925 = 8,

    /// <summary>
    /// 8" x 10" - Full size.
    /// </summary>
    Size8x10 = 9,

    /// <summary>
    /// 8.25" x 6" - Landscape.
    /// </summary>
    Size825x6 = 10,

    /// <summary>
    /// 8.25" x 8.25" - Square.
    /// </summary>
    Size825x825 = 11,

    /// <summary>
    /// 8.5" x 8.5" - Square.
    /// </summary>
    Size85x85 = 12,

    /// <summary>
    /// 8.5" x 11" - Letter size.
    /// </summary>
    Size85x11 = 13
}

/// <summary>
/// Represents paper types for KDP printing.
/// </summary>
public enum PaperType
{
    /// <summary>
    /// White paper - standard.
    /// </summary>
    White = 0,

    /// <summary>
    /// Cream/off-white paper - easier on eyes for long reads.
    /// </summary>
    Cream = 1
}

/// <summary>
/// Represents cover finish types.
/// </summary>
public enum CoverFinish
{
    /// <summary>
    /// Glossy finish - shiny, reflective.
    /// </summary>
    Glossy = 0,

    /// <summary>
    /// Matte finish - non-reflective, modern look.
    /// </summary>
    Matte = 1
}

/// <summary>
/// Represents AI image generation providers.
/// </summary>
public enum ImageGenerationProvider
{
    /// <summary>
    /// OpenAI DALL-E.
    /// </summary>
    DallE = 0,

    /// <summary>
    /// Stability AI (Stable Diffusion).
    /// </summary>
    StabilityAI = 1,

    /// <summary>
    /// Ideogram AI.
    /// </summary>
    Ideogram = 2,

    /// <summary>
    /// Midjourney (via API).
    /// </summary>
    Midjourney = 3,

    /// <summary>
    /// Leonardo AI.
    /// </summary>
    Leonardo = 4
}

/// <summary>
/// Represents cover design styles.
/// </summary>
public enum CoverStyle
{
    /// <summary>
    /// Photographic/realistic imagery.
    /// </summary>
    Photographic = 0,

    /// <summary>
    /// Illustrated/artistic style.
    /// </summary>
    Illustrated = 1,

    /// <summary>
    /// Typography-focused design.
    /// </summary>
    Typography = 2,

    /// <summary>
    /// Minimalist design.
    /// </summary>
    Minimalist = 3,

    /// <summary>
    /// Abstract design.
    /// </summary>
    Abstract = 4,

    /// <summary>
    /// Vintage/retro style.
    /// </summary>
    Vintage = 5,

    /// <summary>
    /// Digital art style.
    /// </summary>
    DigitalArt = 6,

    /// <summary>
    /// 3D rendered style.
    /// </summary>
    ThreeD = 7,

    /// <summary>
    /// Collage style.
    /// </summary>
    Collage = 8,

    /// <summary>
    /// Hand-drawn/sketch style.
    /// </summary>
    HandDrawn = 9
}

/// <summary>
/// Represents A+ Content module types for Amazon.
/// </summary>
public enum APlusModuleType
{
    /// <summary>
    /// Standard text module.
    /// </summary>
    StandardText = 0,

    /// <summary>
    /// Standard image and text module.
    /// </summary>
    StandardImageText = 1,

    /// <summary>
    /// Standard single image.
    /// </summary>
    StandardSingleImage = 2,

    /// <summary>
    /// Standard four images and text.
    /// </summary>
    StandardFourImages = 3,

    /// <summary>
    /// Standard comparison chart.
    /// </summary>
    ComparisonChart = 4,

    /// <summary>
    /// Standard single image sidebar.
    /// </summary>
    ImageSidebar = 5,

    /// <summary>
    /// Standard image header.
    /// </summary>
    ImageHeader = 6
}

/// <summary>
/// Represents the status of a book series.
/// </summary>
public enum SeriesStatus
{
    /// <summary>
    /// Series is planned but not started.
    /// </summary>
    Planned = 0,

    /// <summary>
    /// Series is actively being written.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Series is complete.
    /// </summary>
    Complete = 2,

    /// <summary>
    /// Series is on hiatus.
    /// </summary>
    OnHiatus = 3,

    /// <summary>
    /// Series has been cancelled.
    /// </summary>
    Cancelled = 4
}
