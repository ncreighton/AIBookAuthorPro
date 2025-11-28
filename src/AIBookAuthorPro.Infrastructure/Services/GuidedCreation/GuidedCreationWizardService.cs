// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Application.Services.GuidedCreation;
using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Models.GuidedCreation;
using Microsoft.Extensions.Logging;

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
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow,
            Status = WizardSessionStatus.InProgress,
            StepHistory = new List<WizardStepHistoryEntry>
            {
                new()
                {
                    Step = GuidedCreationStep.PromptEntry,
                    EnteredAt = DateTime.UtcNow
                }
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

        // Mark current step as completed
        var currentHistory = session.StepHistory?.LastOrDefault();
        if (currentHistory != null)
        {
            currentHistory.CompletedAt = DateTime.UtcNow;
            currentHistory.IsCompleted = true;
        }

        // Add new step to history
        session.StepHistory ??= new List<WizardStepHistoryEntry>();
        session.StepHistory.Add(new WizardStepHistoryEntry
        {
            Step = nextStep,
            EnteredAt = DateTime.UtcNow
        });

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
            GuidedCreationStep.CharacterEditor => session.Blueprint?.CharacterBible != null,
            GuidedCreationStep.PlotEditor => session.Blueprint?.PlotArchitecture != null,
            GuidedCreationStep.WorldEditor => session.Blueprint?.WorldBible != null,
            GuidedCreationStep.StyleEditor => session.Blueprint?.StyleGuide != null,
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

        var result = await _promptAnalysisService.AnalyzePromptAsync(
            session.SeedPrompt,
            progress,
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
    public async Task<Result<BookBlueprint>> GenerateBlueprintAsync(
        GuidedCreationWizardSession session,
        IProgress<BlueprintGenerationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (session?.AnalysisResult == null)
            return Result<BookBlueprint>.Failure("Prompt analysis must be completed first");
        if (session.ExpandedBrief == null)
            return Result<BookBlueprint>.Failure("Expanded brief must be created first");

        _logger.LogInformation("Generating blueprint for session {SessionId}", session.Id);

        var result = await _blueprintGeneratorService.GenerateBlueprintAsync(
            session.ExpandedBrief,
            session.ClarificationResponses ?? new Dictionary<string, string>(),
            progress,
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
    public async Task<Result<GenerationSession>> StartGenerationAsync(
        GuidedCreationWizardSession session,
        IProgress<GenerationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
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

        var result = await _orchestrator.StartFullGenerationAsync(
            session.Blueprint,
            options,
            progress,
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

        session.Status = WizardSessionStatus.Completed;
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

        session.Status = WizardSessionStatus.Cancelled;
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

        var completedSteps = session.StepHistory?
            .Where(h => h.IsCompleted)
            .Select(h => h.Step)
            .ToList() ?? new List<GuidedCreationStep>();

        var summary = new WizardProgressSummary
        {
            CurrentStep = session.CurrentStep,
            CurrentStepIndex = currentIndex,
            TotalSteps = totalSteps,
            OverallProgressPercentage = (double)currentIndex / totalSteps * 100,
            CompletedSteps = completedSteps,
            TimeSpentSoFar = DateTime.UtcNow - session.CreatedAt,
            HasAnalysisResult = session.AnalysisResult != null,
            HasBlueprint = session.Blueprint != null,
            IsBlueprintApproved = session.BlueprintApproved,
            HasGenerationSession = session.GenerationSession != null,
            GenerationProgress = session.GenerationSession != null
                ? CalculateGenerationProgress(session.GenerationSession)
                : 0
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

        var responses = session.ClarificationResponses ?? new Dictionary<string, string>();

        return requiredIds.All(id => 
            responses.ContainsKey(id.ToString()) && 
            !string.IsNullOrWhiteSpace(responses[id.ToString()]));
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

/// <summary>
/// Wizard session status.
/// </summary>
public enum WizardSessionStatus
{
    /// <summary>In progress.</summary>
    InProgress,
    /// <summary>Paused.</summary>
    Paused,
    /// <summary>Completed.</summary>
    Completed,
    /// <summary>Cancelled.</summary>
    Cancelled
}

/// <summary>
/// Wizard step history entry.
/// </summary>
public class WizardStepHistoryEntry
{
    public GuidedCreationStep Step { get; set; }
    public DateTime EnteredAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsCompleted { get; set; }
}

/// <summary>
/// Wizard progress summary.
/// </summary>
public class WizardProgressSummary
{
    public GuidedCreationStep CurrentStep { get; set; }
    public int CurrentStepIndex { get; set; }
    public int TotalSteps { get; set; }
    public double OverallProgressPercentage { get; set; }
    public List<GuidedCreationStep> CompletedSteps { get; set; } = new();
    public TimeSpan TimeSpentSoFar { get; set; }
    public bool HasAnalysisResult { get; set; }
    public bool HasBlueprint { get; set; }
    public bool IsBlueprintApproved { get; set; }
    public bool HasGenerationSession { get; set; }
    public double GenerationProgress { get; set; }
}

/// <summary>
/// Guided creation wizard session.
/// </summary>
public class GuidedCreationWizardSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public GuidedCreationStep CurrentStep { get; set; }
    public WizardSessionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<WizardStepHistoryEntry>? StepHistory { get; set; }

    // Data from each step
    public BookSeedPrompt? SeedPrompt { get; set; }
    public PromptAnalysisResult? AnalysisResult { get; set; }
    public ExpandedCreativeBrief? ExpandedBrief { get; set; }
    public List<ClarificationRequest>? RequiredClarifications { get; set; }
    public Dictionary<string, string>? ClarificationResponses { get; set; }
    public BookBlueprint? Blueprint { get; set; }
    public bool BlueprintApproved { get; set; }
    public GenerationConfiguration? Configuration { get; set; }
    public GenerationSession? GenerationSession { get; set; }
}
