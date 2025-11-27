// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Enums;

namespace AIBookAuthorPro.Core.Models;

/// <summary>
/// Statistics for a project.
/// </summary>
public sealed class ProjectStatistics
{
    /// <summary>
    /// Gets or sets the total word count across all chapters.
    /// </summary>
    public int TotalWordCount { get; init; }

    /// <summary>
    /// Gets or sets the target word count for the entire book.
    /// </summary>
    public int TargetWordCount { get; init; }

    /// <summary>
    /// Gets the completion percentage.
    /// </summary>
    public double CompletionPercentage => TargetWordCount > 0
        ? Math.Min(100, (double)TotalWordCount / TargetWordCount * 100)
        : 0;

    /// <summary>
    /// Gets or sets the total chapter count.
    /// </summary>
    public int TotalChapters { get; init; }

    /// <summary>
    /// Gets or sets the count of completed chapters.
    /// </summary>
    public int CompletedChapters { get; init; }

    /// <summary>
    /// Gets or sets chapters by status.
    /// </summary>
    public Dictionary<ChapterStatus, int> ChaptersByStatus { get; init; } = new();

    /// <summary>
    /// Gets or sets the character count.
    /// </summary>
    public int CharacterCount { get; init; }

    /// <summary>
    /// Gets or sets the location count.
    /// </summary>
    public int LocationCount { get; init; }

    /// <summary>
    /// Gets or sets the outline item count.
    /// </summary>
    public int OutlineItemCount { get; init; }

    /// <summary>
    /// Gets or sets the research note count.
    /// </summary>
    public int ResearchNoteCount { get; init; }

    /// <summary>
    /// Gets or sets the estimated reading time in minutes.
    /// </summary>
    public int EstimatedReadingTimeMinutes { get; init; }

    /// <summary>
    /// Gets or sets the last modified date.
    /// </summary>
    public DateTime LastModified { get; init; }

    /// <summary>
    /// Gets or sets the project creation date.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets or sets the average words per chapter.
    /// </summary>
    public double AverageWordsPerChapter => TotalChapters > 0
        ? (double)TotalWordCount / TotalChapters
        : 0;

    /// <summary>
    /// Creates statistics from a project.
    /// </summary>
    public static ProjectStatistics FromProject(Project project)
    {
        ArgumentNullException.ThrowIfNull(project);

        var chaptersByStatus = project.Chapters
            .GroupBy(c => c.Status)
            .ToDictionary(g => g.Key, g => g.Count());

        // Ensure all statuses are represented
        foreach (ChapterStatus status in Enum.GetValues<ChapterStatus>())
        {
            chaptersByStatus.TryAdd(status, 0);
        }

        var totalWords = project.Chapters.Sum(c => CountWords(c.Content));
        var targetWords = project.Metadata?.TargetWordCount ?? project.Chapters.Sum(c => c.TargetWordCount);

        return new ProjectStatistics
        {
            TotalWordCount = totalWords,
            TargetWordCount = targetWords,
            TotalChapters = project.Chapters.Count,
            CompletedChapters = project.Chapters.Count(c => c.Status == ChapterStatus.Complete),
            ChaptersByStatus = chaptersByStatus,
            CharacterCount = project.Characters.Count,
            LocationCount = project.Locations.Count,
            OutlineItemCount = project.Outline?.Items.Count ?? 0,
            ResearchNoteCount = project.ResearchNotes.Count,
            EstimatedReadingTimeMinutes = totalWords / 250, // Average reading speed
            LastModified = project.ModifiedAt,
            CreatedAt = project.CreatedAt
        };
    }

    private static int CountWords(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        return text.Split([' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries).Length;
    }
}
