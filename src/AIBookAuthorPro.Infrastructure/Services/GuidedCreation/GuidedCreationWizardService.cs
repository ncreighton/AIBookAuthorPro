// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Models.GuidedCreation;
using Microsoft.Extensions.Logging;

// Fully qualified types for Application layer to avoid ambiguity with Core types
using IGuidedCreationWizardService = AIBookAuthorPro.Application.Services.GuidedCreation.IGuidedCreationWizardService;
using IPromptAnalysisService = AIBookAuthorPro.Application.Services.GuidedCreation.IPromptAnalysisService;
using IBlueprintGeneratorService = AIBookAuthorPro.Application.Services.GuidedCreation.IBlueprintGeneratorService;
using IBookGenerationOrchestrator = AIBookAuthorPro.Application.Services.GuidedCreation.IBookGenerationOrchestrator;
using GenerationOptions = AIBookAuthorPro.Application.Services.GuidedCreation.GenerationOptions;
using DetailedBlueprintProgress = AIBookAuthorPro.Application.Services.GuidedCreation.DetailedBlueprintProgress;
using DetailedGenerationProgress = AIBookAuthorPro.Application.Services.GuidedCreation.DetailedGenerationProgress;

namespace AIBookAuthorPro.Infrastructure.Services.GuidedCreation;

/// <summary>
/// Service for managing the guided creation wizard workflow.
/// </summary>
public sealed class GuidedCreationWizardService : IGuidedCreationWizardService
{
    private readonly IPromptAnalysisService _promptAnalysisService;
    private readonly IBlueprintGeneratorService _blueprintGeneratorService;
    private readonly IBookGenerationOrchestrator _orchestrator;
    private readonly ILogger<GuidedCreationWizardService> _logger;

    private readonly Dictionary<Guid, GuidedCreationWizardSession> _sessions = new();

    public GuidedCreationWizardService(
        IPromptAnalysisService promptAnalysisService,
        IBlueprintGeneratorService blueprintGeneratorService,
        IBookGenerationOrchestrator orchestrator,
        ILogger<GuidedCreationWizardService> logger)
    {
        _promptAnalysisService = promptAnalysisService ?? throw new ArgumentNullException(nameof(promptAnalysisService));
        _blueprintGeneratorService = blueprintGeneratorService ?? throw new ArgumentNullException(nameof(blueprintGeneratorService));
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<Result<GuidedCreationWizardSession>> StartNewSessionAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting new guided creation wizard session");

        var session = new GuidedCreationWizardSession
        {
            Id = Guid.NewGuid(),
            CurrentStep = GuidedCreationStep.PromptEntry,
            LastUpdatedAt = DateTime.UtcNow,
            Status = Core.Models.GuidedCreation.WizardSessionStatus.InProgress,
            StepHistory = new List<GuidedCreationStep>
            {
                GuidedCreationStep.PromptEntry
            }
        };

        _sessions[session.Id] = session;

        return Result<GuidedCreationWizardSession>.Success(session);
    }

    /// <inheritdoc />
    public async Task<Result<GuidedCreationWizardSession>> LoadSessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            return Result<GuidedCreationWizardSession>.Success(session);
        }

        return Result<GuidedCreationWizardSession>.Failure($"Session {sessionId} not found");
    }

    /// <inheritdoc />
    public async Task<Result> SaveSessionAsync(
        GuidedCreationWizardSession session,
        CancellationToken cancellationToken = default)
    {
        if (session == null)
            return Result.Failure("Session cannot be null");

        session.LastUpdatedAt = DateTime.UtcNow;
        _sessions[session.Id] = session;

        _logger.LogDebug("Session {SessionId} saved at step {Step}", session.Id, session.CurrentStep);

        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result<GuidedCreationWizardSession>> AdvanceToNextStepAsync(
        GuidedCreationWizardSession session,
        CancellationToken cancellationToken = default)
    {
        if (session == null)
            return Result<GuidedCreationWizardSession>.Failure("Session cannot be null");

        var currentStep = session.CurrentStep;
        var nextStep = GetNextStep(currentStep);

        if (nextStep == currentStep)
        {
            // Already at the end
            return Result<GuidedCreationWizardSession>.Success(session);
        }

        // Add new step to history if not already present
        // Note: StepHistory is init-only, so we modify the existing list if it exists
        if (session.StepHistory != null && !session.StepHistory.Contains(nextStep))
        {
            session.StepHistory.Add(nextStep);
        }

        session.CurrentStep = nextStep;
        session.LastUpdatedAt = DateTime.UtcNow;

        _sessions[session.Id] = session;

        _logger.LogInformation("Session {SessionId} advanced from {FromStep} to {ToStep}",
            session.Id, currentStep, nextStep);

        return Result<GuidedCreationWizardSession>.Success(session);
    }

    /// <inheritdoc />
    public async Task<Result<GuidedCreationWizardSession>> GoToPreviousStepAsync(
        GuidedCreationWizardSession session,
        CancellationToken cancellationToken = default)
    {
        if (session == null)
            return Result<GuidedCreationWizardSession>.Failure("Session cannot be null");

        var currentStep = session.CurrentStep;
        var previousStep = GetPreviousStep(currentStep);

        if (previousStep == currentStep)
        {
            // Already at the beginning
            return Result<GuidedCreationWizardSession>.Success(session);
        }

        session.CurrentStep = previousStep;
        session.LastUpdatedAt = DateTime.UtcNow;

        _sessions[session.Id] = session;

        _logger.LogInformation("Session {SessionId} went back from {FromStep} to {ToStep}",
            session.Id, currentStep, previousStep);

        return Result<GuidedCreationWizardSession>.Success(session);
    }

    /// <inheritdoc />
    public async Task<Result<GuidedCreationWizardSession>> GoToStepAsync(
        GuidedCreationWizardSession session,
        GuidedCreationStep targetStep,
        CancellationToken cancellationToken = default)
    {
        if (session == null)
            return Result<GuidedCreationWizardSession>.Failure("Session cannot be null");

        // Check if step is available
        var available = await IsStepAvailableAsync(session, targetStep, cancellationToken);
        if (!available)
        {
            return Result<GuidedCreationWizardSession>.Failure(
                $"Step {targetStep} is not available yet. Complete previous steps first.");
        }

        session.CurrentStep = targetStep;
        session.LastUpdatedAt = DateTime.UtcNow;

        _sessions[session.Id] = session;

        return Result<GuidedCreationWizardSession>.Success(session);
    }

    /// <inheritdoc />
    public async Task<Result<bool>> ValidateStepAsync(
        GuidedCreationWizardSession session,
        GuidedCreationStep step,
        CancellationToken cancellationToken = default)
    {
        if (session == null)
            return Result<bool>.Failure("Session cannot be null");

        var isValid = step switch
        {
            GuidedCreationStep.PromptEntry => ValidatePromptEntry(session),
            GuidedCreationStep.PromptAnalysis => session.AnalysisResult != null,
            GuidedCreationStep.Clarifications => ValidateClarifications(session),
            GuidedCreationStep.BlueprintReview => session.Blueprint != null,
            GuidedCreationStep.StructureEditor => session.Blueprint?.StructuralPlan != null,
            GuidedCreationStep.CharacterEditor => session.Blueprint?.Characters != null,
            GuidedCreationStep.PlotEditor => session.Blueprint?.Plot != null,
            GuidedCreationStep.WorldEditor => session.Blueprint?.World != null,
            GuidedCreationStep.StyleEditor => session.Blueprint?.Style != null,
            GuidedCreationStep.SettingsConfirmation => session.Configuration != null,
            GuidedCreationStep.Generation => session.BlueprintApproved,
            GuidedCreationStep.ReviewAndRefine => session.GenerationSession != null,
            GuidedCreationStep.Completion => IsGenerationComplete(session),
            _ => false
        };

        return Result<bool>.Success(isValid);
    }

    /// <inheritdoc />
    public async Task<bool> IsStepAvailableAsync(
        GuidedCreationWizardSession session,
        GuidedCreationStep step,
        CancellationToken cancellationToken = default)
    {
        if (session == null) return false;

        // First step is always available
        if (step == GuidedCreationStep.PromptEntry)
            return true;

        // Check if all previous steps are completed
        var previousStep = GetPreviousStep(step);
        var validationResult = await ValidateStepAsync(session, previousStep, cancellationToken);

        return validationResult.IsSuccess && validationResult.Value;
    }

    /// <inheritdoc />
    public async Task<Result<PromptAnalysisResult>> AnalyzePromptAsync(
        GuidedCreationWizardSession session,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (session?.SeedPrompt == null)
            return Result<PromptAnalysisResult>.Failure("No seed prompt provided");

        _logger.LogInformation("Analyzing prompt for session {SessionId}", session.Id);

        // Note: IPromptAnalysisService doesn't support progress reporting
        _ = progress; // Ignored
        var result = await _promptAnalysisService.AnalyzePromptAsync(
            session.SeedPrompt,
            cancellationToken);

        if (result.IsSuccess)
        {
            session.AnalysisResult = result.Value;
            session.LastUpdatedAt = DateTime.UtcNow;
            _sessions[session.Id] = session;
        }

        return result;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Progress reporting is not supported due to type differences between Core and Application namespaces.
    /// </remarks>
    public async Task<Result<BookBlueprint>> GenerateBlueprintAsync(
        GuidedCreationWizardSession session,
        IProgress<BlueprintGenerationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        _ = progress; // Intentionally ignored due to type mismatch between Core/Application
        
        if (session?.AnalysisResult == null)
            return Result<BookBlueprint>.Failure("Prompt analysis must be completed first");
        if (session.ExpandedBrief == null)
            return Result<BookBlueprint>.Failure("Expanded brief must be created first");

        _logger.LogInformation("Generating blueprint for session {SessionId}", session.Id);

        var result = await _blueprintGeneratorService.GenerateBlueprintAsync(
            session.ExpandedBrief,
            null,
            cancellationToken);

        if (result.IsSuccess)
        {
            session.Blueprint = result.Value;
            session.LastUpdatedAt = DateTime.UtcNow;
            _sessions[session.Id] = session;
        }

        return result;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Progress reporting is not supported due to type differences between Core and Application namespaces.
    /// </remarks>
    public async Task<Result<GenerationSession>> StartGenerationAsync(
        GuidedCreationWizardSession session,
        IProgress<GenerationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        _ = progress; // Intentionally ignored due to type mismatch between Core/Application
        
        if (session?.Blueprint == null)
            return Result<GenerationSession>.Failure("Blueprint must be approved first");
        if (!session.BlueprintApproved)
            return Result<GenerationSession>.Failure("Blueprint must be approved before generation");

        _logger.LogInformation("Starting generation for session {SessionId}", session.Id);

        var options = new GenerationOptions
        {
            UseConfigurationFromBlueprint = true,
            ConfigurationOverride = session.Configuration
        };

        // Note: Progress reporting from internal service to external interface is not currently supported
        // due to type differences between Core and Application progress types
        var result = await _orchestrator.StartFullGenerationAsync(
            session.Blueprint,
            options,
            null,  // Progress reporting disabled due to type mismatch
            cancellationToken);

        if (result.IsSuccess)
        {
            session.GenerationSession = result.Value;
            session.LastUpdatedAt = DateTime.UtcNow;
            _sessions[session.Id] = session;
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<Result> CompleteWizardAsync(
        GuidedCreationWizardSession session,
        CancellationToken cancellationToken = default)
    {
        if (session == null)
            return Result.Failure("Session cannot be null");

        session.Status = Core.Models.GuidedCreation.WizardSessionStatus.Completed;
        session.CompletedAt = DateTime.UtcNow;
        session.LastUpdatedAt = DateTime.UtcNow;

        _sessions[session.Id] = session;

        _logger.LogInformation("Wizard session {SessionId} completed", session.Id);

        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result> CancelWizardAsync(
        GuidedCreationWizardSession session,
        CancellationToken cancellationToken = default)
    {
        if (session == null)
            return Result.Failure("Session cannot be null");

        session.Status = Core.Models.GuidedCreation.WizardSessionStatus.Cancelled;
        session.LastUpdatedAt = DateTime.UtcNow;

        _sessions[session.Id] = session;

        _logger.LogInformation("Wizard session {SessionId} cancelled", session.Id);

        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result<WizardProgressSummary>> GetProgressSummaryAsync(
        GuidedCreationWizardSession session,
        CancellationToken cancellationToken = default)
    {
        if (session == null)
            return Result<WizardProgressSummary>.Failure("Session cannot be null");

        var allSteps = Enum.GetValues<GuidedCreationStep>();
        var currentIndex = (int)session.CurrentStep;
        var totalSteps = allSteps.Length;

        // StepHistory is List<GuidedCreationStep>, get all steps before current as completed
        var completedSteps = session.StepHistory?.ToList() ?? new List<GuidedCreationStep>();

        var genProgress = session.GenerationSession != null
            ? new GenerationProgress
            {
                CurrentChapter = session.GenerationSession.CurrentChapter,
                TotalChapters = session.GenerationSession.TotalChapters,
                OverallPercentage = CalculateGenerationProgress(session.GenerationSession)
            }
            : null;

        var summary = new WizardProgressSummary
        {
            CurrentStep = session.CurrentStep,
            CurrentStepNumber = currentIndex + 1,
            TotalSteps = totalSteps,
            OverallProgressPercentage = (double)currentIndex / totalSteps * 100,
            CompletedSteps = completedSteps,
            TimeElapsed = DateTime.UtcNow - session.StartedAt,
            HasAnalysisResult = session.AnalysisResult != null,
            HasBlueprint = session.Blueprint != null,
            IsBlueprintApproved = session.BlueprintApproved,
            HasGenerationSession = session.GenerationSession != null,
            GenerationProgress = genProgress
        };

        return Result<WizardProgressSummary>.Success(summary);
    }

    #region Private Helper Methods

    private static GuidedCreationStep GetNextStep(GuidedCreationStep current)
    {
        return current switch
        {
            GuidedCreationStep.PromptEntry => GuidedCreationStep.PromptAnalysis,
            GuidedCreationStep.PromptAnalysis => GuidedCreationStep.Clarifications,
            GuidedCreationStep.Clarifications => GuidedCreationStep.BlueprintReview,
            GuidedCreationStep.BlueprintReview => GuidedCreationStep.StructureEditor,
            GuidedCreationStep.StructureEditor => GuidedCreationStep.CharacterEditor,
            GuidedCreationStep.CharacterEditor => GuidedCreationStep.PlotEditor,
            GuidedCreationStep.PlotEditor => GuidedCreationStep.WorldEditor,
            GuidedCreationStep.WorldEditor => GuidedCreationStep.StyleEditor,
            GuidedCreationStep.StyleEditor => GuidedCreationStep.SettingsConfirmation,
            GuidedCreationStep.SettingsConfirmation => GuidedCreationStep.Generation,
            GuidedCreationStep.Generation => GuidedCreationStep.ReviewAndRefine,
            GuidedCreationStep.ReviewAndRefine => GuidedCreationStep.Completion,
            GuidedCreationStep.Completion => GuidedCreationStep.Completion,
            _ => current
        };
    }

    private static GuidedCreationStep GetPreviousStep(GuidedCreationStep current)
    {
        return current switch
        {
            GuidedCreationStep.PromptEntry => GuidedCreationStep.PromptEntry,
            GuidedCreationStep.PromptAnalysis => GuidedCreationStep.PromptEntry,
            GuidedCreationStep.Clarifications => GuidedCreationStep.PromptAnalysis,
            GuidedCreationStep.BlueprintReview => GuidedCreationStep.Clarifications,
            GuidedCreationStep.StructureEditor => GuidedCreationStep.BlueprintReview,
            GuidedCreationStep.CharacterEditor => GuidedCreationStep.StructureEditor,
            GuidedCreationStep.PlotEditor => GuidedCreationStep.CharacterEditor,
            GuidedCreationStep.WorldEditor => GuidedCreationStep.PlotEditor,
            GuidedCreationStep.StyleEditor => GuidedCreationStep.WorldEditor,
            GuidedCreationStep.SettingsConfirmation => GuidedCreationStep.StyleEditor,
            GuidedCreationStep.Generation => GuidedCreationStep.SettingsConfirmation,
            GuidedCreationStep.ReviewAndRefine => GuidedCreationStep.Generation,
            GuidedCreationStep.Completion => GuidedCreationStep.ReviewAndRefine,
            _ => current
        };
    }

    private static bool ValidatePromptEntry(GuidedCreationWizardSession session)
    {
        return session.SeedPrompt != null &&
               !string.IsNullOrWhiteSpace(session.SeedPrompt.RawPrompt) &&
               session.SeedPrompt.RawPrompt.Length >= 50;
    }

    private static bool ValidateClarifications(GuidedCreationWizardSession session)
    {
        // Check if required clarifications have been answered
        if (session.RequiredClarifications == null || !session.RequiredClarifications.Any())
            return true; // No clarifications needed

        var requiredIds = session.RequiredClarifications
            .Where(c => c.Priority == ClarificationPriority.Required)
            .Select(c => c.Id)
            .ToList();

        if (!requiredIds.Any())
            return true;

        var responses = session.ClarificationResponses ?? new Dictionary<Guid, string>();

        return requiredIds.All(id => 
            responses.TryGetValue(id, out var response) && 
            !string.IsNullOrWhiteSpace(response));
    }

    private static bool IsGenerationComplete(GuidedCreationWizardSession session)
    {
        return session.GenerationSession?.Status == GenerationSessionStatus.Completed ||
               session.GenerationSession?.Status == GenerationSessionStatus.CompletedWithErrors;
    }

    private static double CalculateGenerationProgress(GenerationSession session)
    {
        if (session.TotalChapters == 0) return 0;
        return (double)session.CompletedChapters / session.TotalChapters * 100;
    }

    #endregion
}

// WizardSessionStatus enum is defined in AIBookAuthorPro.Core.Models.GuidedCreation.WizardModels


