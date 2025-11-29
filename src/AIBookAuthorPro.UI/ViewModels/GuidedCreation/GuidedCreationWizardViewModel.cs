// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Collections.ObjectModel;
using AIBookAuthorPro.Application.Services.GuidedCreation;
using AIBookAuthorPro.Core.Models.GuidedCreation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.UI.ViewModels.GuidedCreation;

/// <summary>
/// Information about a step in the wizard.
/// </summary>
public sealed class WizardStepInfo : ObservableObject
{
    private bool _isCurrent;
    private bool _isCompleted;
    private bool _isAvailable;
    
    public WizardStep Step { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;
    
    public bool IsAvailable
    {
        get => _isAvailable;
        set => SetProperty(ref _isAvailable, value);
    }
    
    public bool IsCompleted
    {
        get => _isCompleted;
        set => SetProperty(ref _isCompleted, value);
    }
    
    public bool IsCurrent
    {
        get => _isCurrent;
        set => SetProperty(ref _isCurrent, value);
    }
    
    /// <summary>
    /// Maps GuidedCreationStep to WizardStep.
    /// </summary>
    public static WizardStep FromGuidedCreationStep(GuidedCreationStep step) => step switch
    {
        GuidedCreationStep.Welcome => WizardStep.PromptEntry,
        GuidedCreationStep.SeedPrompt => WizardStep.PromptEntry,
        GuidedCreationStep.Analysis => WizardStep.PromptAnalysis,
        GuidedCreationStep.BriefExpansion => WizardStep.PromptAnalysis,
        GuidedCreationStep.Clarifications => WizardStep.Clarifications,
        GuidedCreationStep.BlueprintGeneration => WizardStep.BlueprintReview,
        GuidedCreationStep.BlueprintReview => WizardStep.BlueprintReview,
        GuidedCreationStep.Configuration => WizardStep.SettingsConfirmation,
        GuidedCreationStep.Generation => WizardStep.Generation,
        GuidedCreationStep.Review => WizardStep.ReviewAndRefine,
        GuidedCreationStep.Export => WizardStep.Completion,
        GuidedCreationStep.Completed => WizardStep.Completion,
        _ => WizardStep.PromptEntry
    };
    
    /// <summary>
    /// Maps WizardStep to GuidedCreationStep.
    /// </summary>
    public static GuidedCreationStep ToGuidedCreationStep(WizardStep step) => step switch
    {
        WizardStep.PromptEntry => GuidedCreationStep.SeedPrompt,
        WizardStep.PromptAnalysis => GuidedCreationStep.Analysis,
        WizardStep.Clarifications => GuidedCreationStep.Clarifications,
        WizardStep.BlueprintReview => GuidedCreationStep.BlueprintReview,
        WizardStep.StructureEditor => GuidedCreationStep.BlueprintReview,
        WizardStep.CharacterEditor => GuidedCreationStep.BlueprintReview,
        WizardStep.PlotEditor => GuidedCreationStep.BlueprintReview,
        WizardStep.WorldEditor => GuidedCreationStep.BlueprintReview,
        WizardStep.StyleEditor => GuidedCreationStep.BlueprintReview,
        WizardStep.SettingsConfirmation => GuidedCreationStep.Configuration,
        WizardStep.Generation => GuidedCreationStep.Generation,
        WizardStep.ReviewAndRefine => GuidedCreationStep.Review,
        WizardStep.Completion => GuidedCreationStep.Completed,
        _ => GuidedCreationStep.Welcome
    };
}

/// <summary>
/// Main ViewModel for the guided creation wizard.
/// </summary>
public partial class GuidedCreationWizardViewModel : ObservableObject
{
    private readonly IGuidedCreationWizardService _wizardService;
    private readonly IPromptAnalysisService _promptAnalysisService;
    private readonly IBlueprintGeneratorService _blueprintGeneratorService;
    private readonly IBookGenerationOrchestrator _generationOrchestrator;
    private readonly ILogger<GuidedCreationWizardViewModel> _logger;

    #region Observable Properties

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGoBack))]
    [NotifyPropertyChangedFor(nameof(CanGoNext))]
    [NotifyPropertyChangedFor(nameof(CurrentStepTitle))]
    [NotifyPropertyChangedFor(nameof(CurrentStepDescription))]
    [NotifyCanExecuteChangedFor(nameof(GoBackCommand))]
    [NotifyCanExecuteChangedFor(nameof(GoNextCommand))]
    private WizardStep _currentStep = WizardStep.PromptEntry;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OverallProgressPercentage))]
    private GuidedCreationWizardSession? _session;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private ObservableCollection<WizardStepInfo> _steps = new();

    // Step-specific ViewModels
    [ObservableProperty]
    private PromptEntryViewModel? _promptEntryViewModel;

    [ObservableProperty]
    private PromptAnalysisViewModel? _promptAnalysisViewModel;

    [ObservableProperty]
    private BlueprintReviewViewModel? _blueprintReviewViewModel;

    [ObservableProperty]
    private GenerationDashboardViewModel? _generationDashboardViewModel;

    #endregion

    #region Computed Properties

    public bool CanGoBack => CurrentStep != WizardStep.PromptEntry && !IsProcessing;
    
    public bool CanGoNext => !IsProcessing && CanProceedFromCurrentStep();

    public double OverallProgressPercentage => Session?.ProgressPercentage ?? 0;

    public string CurrentStepTitle => CurrentStep switch
    {
        WizardStep.PromptEntry => "Enter Your Book Idea",
        WizardStep.PromptAnalysis => "Analyzing Your Concept",
        WizardStep.Clarifications => "Clarify Details",
        WizardStep.BlueprintReview => "Review Your Blueprint",
        WizardStep.StructureEditor => "Edit Structure",
        WizardStep.CharacterEditor => "Edit Characters",
        WizardStep.PlotEditor => "Edit Plot",
        WizardStep.WorldEditor => "Edit World",
        WizardStep.StyleEditor => "Edit Style",
        WizardStep.SettingsConfirmation => "Confirm Settings",
        WizardStep.Generation => "Generating Your Book",
        WizardStep.ReviewAndRefine => "Review & Refine",
        WizardStep.Completion => "Complete!",
        _ => "Guided Creation"
    };

    public string CurrentStepDescription => CurrentStep switch
    {
        WizardStep.PromptEntry => "Describe your book idea, paste a conversation from Claude or ChatGPT, or start from a template.",
        WizardStep.PromptAnalysis => "AI is analyzing your concept to extract themes, characters, and structure.",
        WizardStep.Clarifications => "Help us understand your vision better by answering a few questions.",
        WizardStep.BlueprintReview => "Review the generated blueprint for your book. Make any adjustments needed.",
        WizardStep.StructureEditor => "Fine-tune the chapter structure and pacing of your book.",
        WizardStep.CharacterEditor => "Develop your characters' personalities, arcs, and relationships.",
        WizardStep.PlotEditor => "Refine your plot points, subplots, and story beats.",
        WizardStep.WorldEditor => "Build out your world's locations, history, and rules.",
        WizardStep.StyleEditor => "Define the writing style, voice, and tone for your book.",
        WizardStep.SettingsConfirmation => "Review generation settings before we start writing.",
        WizardStep.Generation => "Your book is being written! Watch the progress in real-time.",
        WizardStep.ReviewAndRefine => "Review generated chapters and request revisions as needed.",
        WizardStep.Completion => "Congratulations! Your book is ready.",
        _ => string.Empty
    };

    #endregion

    public GuidedCreationWizardViewModel(
        IGuidedCreationWizardService wizardService,
        IPromptAnalysisService promptAnalysisService,
        IBlueprintGeneratorService blueprintGeneratorService,
        IBookGenerationOrchestrator generationOrchestrator,
        ILogger<GuidedCreationWizardViewModel> logger)
    {
        _wizardService = wizardService ?? throw new ArgumentNullException(nameof(wizardService));
        _promptAnalysisService = promptAnalysisService ?? throw new ArgumentNullException(nameof(promptAnalysisService));
        _blueprintGeneratorService = blueprintGeneratorService ?? throw new ArgumentNullException(nameof(blueprintGeneratorService));
        _generationOrchestrator = generationOrchestrator ?? throw new ArgumentNullException(nameof(generationOrchestrator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        InitializeSteps();
    }

    #region Commands

    [RelayCommand]
    private async Task InitializeAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        ClearError();

        try
        {
            var result = await _wizardService.StartNewSessionAsync(cancellationToken);
            if (result.IsSuccess)
            {
                Session = result.Value;
                CurrentStep = WizardStep.PromptEntry;
                InitializeStepViewModels();
                UpdateStepStates();
                _logger.LogInformation("Wizard session initialized: {SessionId}", Session!.Id);
            }
            else
            {
                ShowError(result.Error ?? "Failed to initialize wizard session");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing wizard");
            ShowError($"Initialization error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanGoBack))]
    private async Task GoBackAsync(CancellationToken cancellationToken)
    {
        if (Session == null) return;

        IsProcessing = true;
        ClearError();

        try
        {
            var result = await _wizardService.GoToPreviousStepAsync(Session, cancellationToken);
            if (result.IsSuccess)
            {
                Session = result.Value;
                CurrentStep = WizardStepInfo.FromGuidedCreationStep(Session!.CurrentStep);
                UpdateStepStates();
            }
            else
            {
                ShowError(result.Error ?? "Failed to go back");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error going back");
            ShowError($"Navigation error: {ex.Message}");
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private async Task GoNextAsync(CancellationToken cancellationToken)
    {
        if (Session == null) return;

        IsProcessing = true;
        ClearError();

        try
        {
            // Process current step before moving
            var processResult = await ProcessCurrentStepAsync(cancellationToken);
            if (!processResult)
            {
                return;
            }

            // Move to next step
            var result = await _wizardService.AdvanceToNextStepAsync(Session, cancellationToken);
            if (result.IsSuccess)
            {
                Session = result.Value;
                CurrentStep = WizardStepInfo.FromGuidedCreationStep(Session!.CurrentStep);
                UpdateStepStates();
                
                // Initialize next step if needed
                await InitializeCurrentStepAsync(cancellationToken);
            }
            else
            {
                ShowError(result.Error ?? "Failed to proceed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error going next");
            ShowError($"Navigation error: {ex.Message}");
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private async Task GoToStepAsync(WizardStep step, CancellationToken cancellationToken)
    {
        if (Session == null) return;
        if (!IsStepAccessible(step)) return;

        IsProcessing = true;
        ClearError();

        try
        {
            var result = await _wizardService.GoToStepAsync(Session, WizardStepInfo.ToGuidedCreationStep(step), cancellationToken);
            if (result.IsSuccess)
            {
                Session = result.Value;
                CurrentStep = WizardStepInfo.FromGuidedCreationStep(Session!.CurrentStep);
                UpdateStepStates();
                await InitializeCurrentStepAsync(cancellationToken);
            }
            else
            {
                ShowError(result.Error ?? "Failed to navigate");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to step {Step}", step);
            ShowError($"Navigation error: {ex.Message}");
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private async Task CancelWizardAsync(CancellationToken cancellationToken)
    {
        if (Session == null) return;

        IsProcessing = true;

        try
        {
            // Mark session as cancelled and save
            Session.Status = WizardSessionStatus.Cancelled;
            await _wizardService.SaveSessionAsync(Session, cancellationToken);
            Session = null;
            // Navigate away from wizard
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error canceling wizard");
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private async Task SaveAndExitAsync(CancellationToken cancellationToken)
    {
        if (Session == null) return;

        IsProcessing = true;
        StatusMessage = "Saving progress...";

        try
        {
            var result = await _wizardService.SaveSessionAsync(Session, cancellationToken);
            if (result.IsSuccess)
            {
                StatusMessage = "Progress saved!";
                // Navigate away
            }
            else
            {
                ShowError(result.Error ?? "Failed to save");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving wizard session");
            ShowError($"Save error: {ex.Message}");
        }
        finally
        {
            IsProcessing = false;
        }
    }

    #endregion

    #region Private Methods

    private void InitializeSteps()
    {
        Steps = new ObservableCollection<WizardStepInfo>
        {
            CreateStepInfo(WizardStep.PromptEntry, "Enter Idea", "Edit", true),
            CreateStepInfo(WizardStep.PromptAnalysis, "Analysis", "Analytics", false),
            CreateStepInfo(WizardStep.Clarifications, "Clarify", "QuestionAnswer", false),
            CreateStepInfo(WizardStep.BlueprintReview, "Blueprint", "Description", false),
            CreateStepInfo(WizardStep.StructureEditor, "Structure", "AccountTree", false),
            CreateStepInfo(WizardStep.CharacterEditor, "Characters", "People", false),
            CreateStepInfo(WizardStep.PlotEditor, "Plot", "Timeline", false),
            CreateStepInfo(WizardStep.WorldEditor, "World", "Public", false),
            CreateStepInfo(WizardStep.StyleEditor, "Style", "Brush", false),
            CreateStepInfo(WizardStep.SettingsConfirmation, "Settings", "Settings", false),
            CreateStepInfo(WizardStep.Generation, "Generate", "AutoAwesome", false),
            CreateStepInfo(WizardStep.ReviewAndRefine, "Review", "RateReview", false),
            CreateStepInfo(WizardStep.Completion, "Complete", "CheckCircle", false)
        };
    }

    private WizardStepInfo CreateStepInfo(WizardStep step, string name, string icon, bool available)
    {
        return new WizardStepInfo
        {
            Step = step,
            DisplayName = name,
            Description = GetStepDescription(step),
            Icon = icon,
            IsAvailable = available,
            IsCompleted = false,
            IsCurrent = step == WizardStep.PromptEntry
        };
    }

    private string GetStepDescription(WizardStep step)
    {
        return step switch
        {
            WizardStep.PromptEntry => "Describe your book idea",
            WizardStep.PromptAnalysis => "AI analyzes your concept",
            WizardStep.Clarifications => "Answer clarifying questions",
            WizardStep.BlueprintReview => "Review generated blueprint",
            WizardStep.StructureEditor => "Edit chapter structure",
            WizardStep.CharacterEditor => "Develop characters",
            WizardStep.PlotEditor => "Refine plot points",
            WizardStep.WorldEditor => "Build your world",
            WizardStep.StyleEditor => "Define writing style",
            WizardStep.SettingsConfirmation => "Confirm settings",
            WizardStep.Generation => "Generate book content",
            WizardStep.ReviewAndRefine => "Review and refine",
            WizardStep.Completion => "Finish and export",
            _ => string.Empty
        };
    }

    private void InitializeStepViewModels()
    {
        PromptEntryViewModel = new PromptEntryViewModel(_promptAnalysisService, _logger);
        PromptAnalysisViewModel = new PromptAnalysisViewModel(_promptAnalysisService, _logger);
        BlueprintReviewViewModel = new BlueprintReviewViewModel(_blueprintGeneratorService, _logger);
        GenerationDashboardViewModel = new GenerationDashboardViewModel(_generationOrchestrator, _logger);
    }

    private void UpdateStepStates()
    {
        if (Session == null) return;

        foreach (var step in Steps)
        {
            step.IsCurrent = step.Step == CurrentStep;
            step.IsCompleted = Session.StepHistory.Contains(WizardStepInfo.ToGuidedCreationStep(step.Step));
            step.IsAvailable = IsStepAccessible(step.Step);
        }
    }

    private bool IsStepAccessible(WizardStep step)
    {
        if (Session == null) return false;

        // First step is always accessible
        if (step == WizardStep.PromptEntry) return true;

        // Step is accessible if all previous steps are completed
        var stepIndex = (int)step;
        for (int i = 0; i < stepIndex; i++)
        {
            if (!Session.StepHistory.Contains((GuidedCreationStep)i))
                return false;
        }

        return true;
    }

    private bool CanProceedFromCurrentStep()
    {
        if (Session == null) return false;

        return CurrentStep switch
        {
            WizardStep.PromptEntry => PromptEntryViewModel?.IsValid ?? false,
            WizardStep.PromptAnalysis => Session.AnalysisResult != null,
            WizardStep.Clarifications => true, // Clarifications are optional
            WizardStep.BlueprintReview => Session.Blueprint != null,
            WizardStep.StructureEditor => true,
            WizardStep.CharacterEditor => true,
            WizardStep.PlotEditor => true,
            WizardStep.WorldEditor => true,
            WizardStep.StyleEditor => true,
            WizardStep.SettingsConfirmation => Session.BlueprintApproved,
            WizardStep.Generation => Session.GenerationSession?.Status == GenerationSessionStatus.Completed,
            WizardStep.ReviewAndRefine => true,
            WizardStep.Completion => true,
            _ => false
        };
    }

    private async Task<bool> ProcessCurrentStepAsync(CancellationToken cancellationToken)
    {
        if (Session == null) return false;

        switch (CurrentStep)
        {
            case WizardStep.PromptEntry:
                return await ProcessPromptEntryAsync(cancellationToken);

            case WizardStep.PromptAnalysis:
                return await ProcessPromptAnalysisAsync(cancellationToken);

            case WizardStep.BlueprintReview:
                return await ProcessBlueprintReviewAsync(cancellationToken);

            case WizardStep.SettingsConfirmation:
                return await ProcessSettingsConfirmationAsync(cancellationToken);

            default:
                return true;
        }
    }

    private async Task<bool> ProcessPromptEntryAsync(CancellationToken cancellationToken)
    {
        if (PromptEntryViewModel == null || Session == null) return false;

        StatusMessage = "Saving your prompt...";

        var prompt = PromptEntryViewModel.CreateSeedPrompt();
        Session.SeedPrompt = prompt;

        return true;
    }

    private async Task<bool> ProcessPromptAnalysisAsync(CancellationToken cancellationToken)
    {
        if (Session?.SeedPrompt == null) return false;

        StatusMessage = "Analyzing your concept...";

        var analysisResult = await _promptAnalysisService.AnalyzePromptAsync(
            Session.SeedPrompt, cancellationToken);

        if (analysisResult.IsSuccess)
        {
            Session.AnalysisResult = analysisResult.Value;
            return true;
        }

        ShowError(analysisResult.Error ?? "Analysis failed");
        return false;
    }

    private async Task<bool> ProcessBlueprintReviewAsync(CancellationToken cancellationToken)
    {
        if (Session == null) return false;

        // Blueprint should already be generated and reviewed
        Session.BlueprintApproved = true;
        return true;
    }

    private async Task<bool> ProcessSettingsConfirmationAsync(CancellationToken cancellationToken)
    {
        if (Session == null) return false;

        // Create default generation config if not set
        Session.Configuration ??= new GenerationConfiguration();

        return true;
    }

    private async Task InitializeCurrentStepAsync(CancellationToken cancellationToken)
    {
        switch (CurrentStep)
        {
            case WizardStep.PromptAnalysis:
                if (Session?.SeedPrompt != null && Session.AnalysisResult == null)
                {
                    StatusMessage = "Analyzing your concept...";
                    var result = await _promptAnalysisService.AnalyzePromptAsync(
                        Session.SeedPrompt, cancellationToken);
                    if (result.IsSuccess)
                    {
                        Session.AnalysisResult = result.Value;
                        PromptAnalysisViewModel?.LoadAnalysis(result.Value);
                    }
                }
                break;

            case WizardStep.BlueprintReview:
                if (Session?.ExpandedBrief != null && Session.Blueprint == null)
                {
                    StatusMessage = "Generating blueprint...";
                    var progress = new Progress<DetailedBlueprintProgress>(p =>
                    {
                        StatusMessage = p.CurrentOperation;
                        Session.ProgressPercentage = p.OverallProgress;
                    });

                    var result = await _blueprintGeneratorService.GenerateBlueprintAsync(
                        Session.ExpandedBrief, progress, cancellationToken);
                    if (result.IsSuccess)
                    {
                        Session.Blueprint = result.Value;
                        BlueprintReviewViewModel?.LoadBlueprint(result.Value);
                    }
                }
                break;

            case WizardStep.Generation:
                if (Session?.Blueprint != null && Session.GenerationSession == null)
                {
                    GenerationDashboardViewModel?.Initialize(Session.Blueprint);
                }
                break;
        }

        StatusMessage = string.Empty;
    }

    private void ShowError(string message)
    {
        ErrorMessage = message;
        HasError = true;
        _logger.LogWarning("Wizard error: {Error}", message);
    }

    private void ClearError()
    {
        ErrorMessage = string.Empty;
        HasError = false;
    }

    #endregion
}
