// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

namespace AIBookAuthorPro.Core.Models.GuidedCreation;

/// <summary>
/// Represents the initial creative seed from which a book will be generated.
/// This is the user's raw input that initiates the entire guided creation process.
/// </summary>
public sealed record BookSeedPrompt
{
    /// <summary>
    /// Unique identifier for this prompt.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// The raw, unprocessed prompt text from the user.
    /// </summary>
    public required string RawPrompt { get; init; }

    /// <summary>
    /// When this prompt was submitted.
    /// </summary>
    public DateTime SubmittedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Where the prompt originated from.
    /// </summary>
    public PromptSource Source { get; init; } = PromptSource.Manual;

    /// <summary>
    /// Optional title if user provided one.
    /// </summary>
    public string? WorkingTitle { get; init; }

    /// <summary>
    /// Optional genre hint from user.
    /// </summary>
    public string? GenreHint { get; init; }

    /// <summary>
    /// Optional target word count if user has a preference.
    /// </summary>
    public int? TargetWordCount { get; init; }

    /// <summary>
    /// Optional target audience description.
    /// </summary>
    public string? TargetAudienceHint { get; init; }

    /// <summary>
    /// Additional metadata about the prompt.
    /// </summary>
    public Dictionary<string, string> Metadata { get; init; } = new();

    /// <summary>
    /// Reference files or content that inspired this prompt.
    /// </summary>
    public List<PromptReference> References { get; init; } = new();

    /// <summary>
    /// Tags for categorization and searchability.
    /// </summary>
    public List<string> Tags { get; init; } = new();

    /// <summary>
    /// Comparable titles the user wants the book to be similar to.
    /// </summary>
    public List<string> ComparableTitles { get; init; } = new();

    /// <summary>
    /// Specific elements the user definitely wants included.
    /// </summary>
    public List<string> MustIncludeElements { get; init; } = new();

    /// <summary>
    /// Elements the user explicitly wants to avoid.
    /// </summary>
    public List<string> MustAvoidElements { get; init; } = new();
}

/// <summary>
/// Reference material that informs the prompt.
/// </summary>
public sealed record PromptReference
{
    /// <summary>
    /// Type of reference (e.g., "Book", "Movie", "Article", "Image").
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Title or name of the reference.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Description of how this reference relates to the prompt.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Optional URL if applicable.
    /// </summary>
    public string? Url { get; init; }

    /// <summary>
    /// Optional file path if a local file.
    /// </summary>
    public string? FilePath { get; init; }
}
