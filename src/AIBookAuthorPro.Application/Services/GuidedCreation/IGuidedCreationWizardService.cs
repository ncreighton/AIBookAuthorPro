// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Models.GuidedCreation;
using AIBookAuthorPro.Infrastructure.Services.GuidedCreation;

namespace AIBookAuthorPro.Application.Services.GuidedCreation;

/// <summary>
/// Service for managing the guided creation wizard workflow.
/// </summary>
public interface IGuidedCreationWizardService
{
    /// <summary>
    /// Starts a new wizard session.
    /// </summary>
    Task<Result<GuidedCreationWizardSession>> StartNewSessionAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads an existing session.
    /// </summary>
    Task<Result<GuidedCreationWizardSession>> LoadSessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves the current session state.
    /// </summary>
    Task<Result> SaveSessionAsync(
        GuidedCreationWizardSession session,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Advances to the next step.
    /// </summary>
    Task<Result<GuidedCreationWizardSession>> AdvanceToNextStepAsync(
        GuidedCreationWizardSession session,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Goes back to the previous step.
    /// </summary>
    Task<Result<GuidedCreationWizardSession>> GoToPreviousStepAsync(
        GuidedCreationWizardSession session,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Jumps to a specific step.
    /// </summary>
    Task<Result<GuidedCreationWizardSession>> GoToStepAsync(
        GuidedCreationWizardSession session,
        GuidedCreationStep targetStep,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a specific step.
    /// </summary>
    Task<Result<bool>> ValidateStepAsync(
        GuidedCreationWizardSession session,
        GuidedCreationStep step,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a step is available.
    /// </summary>
    Task<bool> IsStepAvailableAsync(
        GuidedCreationWizardSession session,
        GuidedCreationStep step,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes the prompt.
    /// </summary>
    Task<Result<PromptAnalysisResult>> AnalyzePromptAsync(
        GuidedCreationWizardSession session,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates the blueprint.
    /// </summary>
    Task<Result<BookBlueprint>> GenerateBlueprintAsync(
        GuidedCreationWizardSession session,
        IProgress<BlueprintGenerationProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts the generation process.
    /// </summary>
    Task<Result<GenerationSession>> StartGenerationAsync(
        GuidedCreationWizardSession session,
        IProgress<GenerationProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes the wizard.
    /// </summary>
    Task<Result> CompleteWizardAsync(
        GuidedCreationWizardSession session,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels the wizard.
    /// </summary>
    Task<Result> CancelWizardAsync(
        GuidedCreationWizardSession session,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a progress summary.
    /// </summary>
    Task<Result<WizardProgressSummary>> GetProgressSummaryAsync(
        GuidedCreationWizardSession session,
        CancellationToken cancellationToken = default);
}
