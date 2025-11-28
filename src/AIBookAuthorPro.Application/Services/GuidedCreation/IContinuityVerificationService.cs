// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Models.GuidedCreation;

namespace AIBookAuthorPro.Application.Services.GuidedCreation;

/// <summary>
/// Service for verifying continuity across generated content.
/// </summary>
public interface IContinuityVerificationService
{
    /// <summary>
    /// Performs comprehensive continuity verification on a chapter.
    /// </summary>
    /// <param name="chapter">The chapter to verify.</param>
    /// <param name="context">Verification context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Continuity report.</returns>
    Task<Result<ContinuityReport>> VerifyChapterContinuityAsync(
        GeneratedChapter chapter,
        ContinuityVerificationContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies character continuity.
    /// </summary>
    /// <param name="content">The content to verify.</param>
    /// <param name="expectedCharacters">Characters expected in the chapter.</param>
    /// <param name="characterStates">Current character states.</param>
    /// <param name="characterBible">The character bible.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Character continuity issues.</returns>
    Task<Result<List<CharacterContinuityIssue>>> VerifyCharacterContinuityAsync(
        string content,
        List<Guid> expectedCharacters,
        List<CharacterStateSnapshot> characterStates,
        CharacterBible characterBible,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies plot continuity.
    /// </summary>
    /// <param name="content">The content to verify.</param>
    /// <param name="blueprint">The chapter blueprint.</param>
    /// <param name="plotArchitecture">The plot architecture.</param>
    /// <param name="previousEvents">Events from previous chapters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Plot continuity issues.</returns>
    Task<Result<List<PlotContinuityIssue>>> VerifyPlotContinuityAsync(
        string content,
        ChapterBlueprint blueprint,
        PlotArchitecture plotArchitecture,
        List<string> previousEvents,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies timeline continuity.
    /// </summary>
    /// <param name="content">The content to verify.</param>
    /// <param name="timelinePosition">Expected timeline position.</param>
    /// <param name="previousTimeline">Previous timeline events.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Timeline continuity issues.</returns>
    Task<Result<List<TimelineContinuityIssue>>> VerifyTimelineContinuityAsync(
        string content,
        TimelinePosition timelinePosition,
        List<TimelineEvent> previousTimeline,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies setting continuity.
    /// </summary>
    /// <param name="content">The content to verify.</param>
    /// <param name="expectedLocations">Expected locations.</param>
    /// <param name="worldBible">The world bible.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Setting continuity issues.</returns>
    Task<Result<List<SettingContinuityIssue>>> VerifySettingContinuityAsync(
        string content,
        List<Guid> expectedLocations,
        WorldBible worldBible,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies object/item continuity.
    /// </summary>
    /// <param name="content">The content to verify.</param>
    /// <param name="trackedObjects">Objects being tracked.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Object continuity issues.</returns>
    Task<Result<List<ObjectContinuityIssue>>> VerifyObjectContinuityAsync(
        string content,
        List<TrackedObject> trackedObjects,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts character state from content.
    /// </summary>
    /// <param name="content">The content to extract from.</param>
    /// <param name="characters">Characters in the content.</param>
    /// <param name="characterBible">The character bible.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Character state snapshots.</returns>
    Task<Result<List<CharacterStateSnapshot>>> ExtractCharacterStatesAsync(
        string content,
        List<Guid> characters,
        CharacterBible characterBible,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts key events from content.
    /// </summary>
    /// <param name="content">The content to extract from.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of key events.</returns>
    Task<Result<List<string>>> ExtractKeyEventsAsync(
        string content,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds continuity context for next chapter.
    /// </summary>
    /// <param name="chapters">Generated chapters so far.</param>
    /// <param name="maxContextSize">Maximum context size in tokens.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Compressed continuity context.</returns>
    Task<Result<string>> BuildContinuityContextAsync(
        List<GeneratedChapter> chapters,
        int maxContextSize,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Context for continuity verification.
/// </summary>
public sealed record ContinuityVerificationContext
{
    /// <summary>
    /// Chapter blueprint.
    /// </summary>
    public required ChapterBlueprint Blueprint { get; init; }

    /// <summary>
    /// Character bible.
    /// </summary>
    public required CharacterBible CharacterBible { get; init; }

    /// <summary>
    /// World bible.
    /// </summary>
    public required WorldBible WorldBible { get; init; }

    /// <summary>
    /// Plot architecture.
    /// </summary>
    public required PlotArchitecture PlotArchitecture { get; init; }

    /// <summary>
    /// Previous chapters.
    /// </summary>
    public List<GeneratedChapter> PreviousChapters { get; init; } = new();

    /// <summary>
    /// Current character states.
    /// </summary>
    public List<CharacterStateSnapshot> CharacterStates { get; init; } = new();

    /// <summary>
    /// Key events from previous chapters.
    /// </summary>
    public List<string> PreviousEvents { get; init; } = new();

    /// <summary>
    /// Tracked objects.
    /// </summary>
    public List<TrackedObject> TrackedObjects { get; init; } = new();

    /// <summary>
    /// Timeline events.
    /// </summary>
    public List<TimelineEvent> TimelineEvents { get; init; } = new();
}

/// <summary>
/// Tracked object for continuity.
/// </summary>
public sealed record TrackedObject
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Object name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Object type.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Current state.
    /// </summary>
    public required string CurrentState { get; init; }

    /// <summary>
    /// Current location.
    /// </summary>
    public required string Location { get; init; }

    /// <summary>
    /// Current owner/holder.
    /// </summary>
    public Guid? CurrentOwnerId { get; init; }

    /// <summary>
    /// State history.
    /// </summary>
    public List<ObjectStateChange> StateHistory { get; init; } = new();
}

/// <summary>
/// Object state change.
/// </summary>
public sealed record ObjectStateChange
{
    /// <summary>
    /// Chapter where change occurred.
    /// </summary>
    public required int ChapterNumber { get; init; }

    /// <summary>
    /// Previous state.
    /// </summary>
    public required string PreviousState { get; init; }

    /// <summary>
    /// New state.
    /// </summary>
    public required string NewState { get; init; }

    /// <summary>
    /// Reason for change.
    /// </summary>
    public required string Reason { get; init; }
}
