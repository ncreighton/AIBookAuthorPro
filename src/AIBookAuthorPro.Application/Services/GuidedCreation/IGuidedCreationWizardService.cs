// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Models.GuidedCreation;

namespace AIBookAuthorPro.Application.Services.GuidedCreation;

/// <summary>
/// Service for managing the guided creation wizard workflow.
/// </summary>
public interface IGuidedCreationWizardService
{
    /// <summary>
    /// Starts a new guided creation session.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>New wizard session.</returns>
    Task<Result<GuidedCreationWizardSession>> StartNewSessionAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads an existing wizard session.
    /// </summary>
    /// <param name="sessionId">Session ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The wizard session.</returns>
    Task<Result<GuidedCreationWizardSession>> LoadSessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves the current wizard session state.
    /// </summary>
    /// <param name="session">The session to save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result of save operation.</returns>
    Task<Result> SaveSessionAsync(
        GuidedCreationWizardSession session,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Moves to the next step in the wizard.
    /// </summary>
    /// <param name="session">The current session.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated session with next step.</returns>
    Task<Result<GuidedCreationWizardSession>> MoveToNextStepAsync(
        GuidedCreationWizardSession session,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Moves to a previous step in the wizard.
    /// </summary>
    /// <param name="session">The current session.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated session with previous step.</returns>
    Task<Result<GuidedCreationWizardSession>> MoveToPreviousStepAsync(
        GuidedCreationWizardSession session,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Moves to a specific step in the wizard.
    /// </summary>
    /// <param name="session">The current session.</param>
    /// <param name="step">Target step.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated session.</returns>
    Task<Result<GuidedCreationWizardSession>> MoveToStepAsync(
        GuidedCreationWizardSession session,
        WizardStep step,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the current step.
    /// </summary>
    /// <param name="session">The current session.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result.</returns>
    Task<Result<StepValidationResult>> ValidateCurrentStepAsync(
        GuidedCreationWizardSession session,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets available steps based on current progress.
    /// </summary>
    /// <param name="session">The current session.</param>
    /// <returns>Available steps.</returns>
    IReadOnlyList<WizardStepInfo> GetAvailableSteps(
        GuidedCreationWizardSession session);

    /// <summary>
    /// Cancels and deletes a wizard session.
    /// </summary>
    /// <param name="sessionId">Session ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result of cancellation.</returns>
    Task<Result> CancelSessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes the wizard and creates the project.
    /// </summary>
    /// <param name="session">The completed session.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created project ID.</returns>
    Task<Result<Guid>> CompleteWizardAsync(
        GuidedCreationWizardSession session,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Guided creation wizard session state.
/// </summary>
public sealed class GuidedCreationWizardSession
{
    /// <summary>
    /// Session ID.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Created at.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Last modified.
    /// </summary>
    public DateTime LastModifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Current wizard step.
    /// </summary>
    public WizardStep CurrentStep { get; set; } = WizardStep.PromptEntry;

    /// <summary>
    /// Steps completed.
    /// </summary>
    public HashSet<WizardStep> CompletedSteps { get; init; } = new();

    /// <summary>
    /// The seed prompt.
    /// </summary>
    public BookSeedPrompt? SeedPrompt { get; set; }

    /// <summary>
    /// Analysis result.
    /// </summary>
    public PromptAnalysisResult? AnalysisResult { get; set; }

    /// <summary>
    /// User clarification responses.
    /// </summary>
    public Dictionary<Guid, string> ClarificationResponses { get; init; } = new();

    /// <summary>
    /// Expanded creative brief.
    /// </summary>
    public ExpandedCreativeBrief? CreativeBrief { get; set; }

    /// <summary>
    /// Generated blueprint.
    /// </summary>
    public BookBlueprint? Blueprint { get; set; }

    /// <summary>
    /// User modifications to blueprint.
    /// </summary>
    public List<BlueprintModification> BlueprintModifications { get; init; } = new();

    /// <summary>
    /// Generation configuration.
    /// </summary>
    public GenerationConfiguration? GenerationConfig { get; set; }

    /// <summary>
    /// Is blueprint approved.
    /// </summary>
    public bool IsBlueprintApproved { get; set; }

    /// <summary>
    /// Generation session if started.
    /// </summary>
    public GenerationSession? GenerationSession { get; set; }

    /// <summary>
    /// Overall progress percentage.
    /// </summary>
    public double OverallProgress { get; set; }

    /// <summary>
    /// Notes/comments by user.
    /// </summary>
    public List<string> UserNotes { get; init; } = new();
}

/// <summary>
/// Blueprint modification record.
/// </summary>
public sealed record BlueprintModification
{
    /// <summary>
    /// When modification was made.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Section modified.
    /// </summary>
    public required string Section { get; init; }

    /// <summary>
    /// Path to modified element.
    /// </summary>
    public required string ElementPath { get; init; }

    /// <summary>
    /// Previous value.
    /// </summary>
    public string? PreviousValue { get; init; }

    /// <summary>
    /// New value.
    /// </summary>
    public required string NewValue { get; init; }

    /// <summary>
    /// Reason for modification.
    /// </summary>
    public string? Reason { get; init; }
}

/// <summary>
/// Step validation result.
/// </summary>
public sealed record StepValidationResult
{
    /// <summary>
    /// Whether the step is valid.
    /// </summary>
    public required bool IsValid { get; init; }

    /// <summary>
    /// Can proceed to next step.
    /// </summary>
    public required bool CanProceed { get; init; }

    /// <summary>
    /// Validation errors.
    /// </summary>
    public List<string> Errors { get; init; } = new();

    /// <summary>
    /// Validation warnings.
    /// </summary>
    public List<string> Warnings { get; init; } = new();

    /// <summary>
    /// Suggestions for improvement.
    /// </summary>
    public List<string> Suggestions { get; init; } = new();
}

/// <summary>
/// Wizard step information.
/// </summary>
public sealed record WizardStepInfo
{
    /// <summary>
    /// Step.
    /// </summary>
    public required WizardStep Step { get; init; }

    /// <summary>
    /// Display name.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Description.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Icon name.
    /// </summary>
    public required string Icon { get; init; }

    /// <summary>
    /// Whether this step is available.
    /// </summary>
    public required bool IsAvailable { get; init; }

    /// <summary>
    /// Whether this step is completed.
    /// </summary>
    public required bool IsCompleted { get; init; }

    /// <summary>
    /// Whether this is the current step.
    /// </summary>
    public required bool IsCurrent { get; init; }

    /// <summary>
    /// Estimated time to complete.
    /// </summary>
    public TimeSpan? EstimatedDuration { get; init; }
}
