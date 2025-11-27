// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Collections.ObjectModel;
using System.Text;
using AIBookAuthorPro.Core.Enums;
using AIBookAuthorPro.Core.Interfaces;
using AIBookAuthorPro.Core.Models;
using AIBookAuthorPro.Core.Models.AI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.UI.ViewModels;

/// <summary>
/// ViewModel for the AI generation dialog.
/// </summary>
public partial class AIGenerationViewModel : ObservableObject
{
    private readonly IGenerationPipelineService _generationService;
    private readonly IAIProviderFactory _providerFactory;
    private readonly IContextBuilderService _contextBuilder;
    private readonly ILogger<AIGenerationViewModel> _logger;

    private CancellationTokenSource? _cancellationTokenSource;

    [ObservableProperty]
    private Project? _project;

    [ObservableProperty]
    private Chapter? _chapter;

    [ObservableProperty]
    private GenerationMode _selectedMode = GenerationMode.Standard;

    [ObservableProperty]
    private AIProviderType _selectedProvider = AIProviderType.Claude;

    [ObservableProperty]
    private string _selectedModel = "claude-sonnet-4-20250514";

    [ObservableProperty]
    private double _temperature = 0.7;

    [ObservableProperty]
    private string? _customInstructions;

    [ObservableProperty]
    private bool _includeCharacterContext = true;

    [ObservableProperty]
    private bool _includeLocationContext = true;

    [ObservableProperty]
    private bool _includePreviousSummary = true;

    [ObservableProperty]
    private bool _isGenerating;

    [ObservableProperty]
    private bool _isComplete;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string _streamingContent = string.Empty;

    [ObservableProperty]
    private string _currentPhase = "Ready";

    [ObservableProperty]
    private double _progress;

    [ObservableProperty]
    private int _generatedWordCount;

    [ObservableProperty]
    private int _tokensUsed;

    [ObservableProperty]
    private decimal _estimatedCost;

    [ObservableProperty]
    private int _estimatedContextTokens;

    [ObservableProperty]
    private GenerationCostEstimate? _costEstimate;

    [ObservableProperty]
    private TimeSpan _elapsedTime;

    /// <summary>
    /// Gets the available generation modes.
    /// </summary>
    public ObservableCollection<GenerationModeInfo> AvailableModes { get; } = [];

    /// <summary>
    /// Gets the available AI providers.
    /// </summary>
    public ObservableCollection<ProviderInfo> AvailableProviders { get; } = [];

    /// <summary>
    /// Gets the available models for the selected provider.
    /// </summary>
    public ObservableCollection<string> AvailableModels { get; } = [];

    /// <summary>
    /// Gets or sets the final generated content.
    /// </summary>
    public string? FinalContent { get; private set; }

    /// <summary>
    /// Gets whether the generation can be accepted.
    /// </summary>
    public bool CanAccept => IsComplete && !HasError && !string.IsNullOrEmpty(FinalContent);

    /// <summary>
    /// Event raised when generation is accepted.
    /// </summary>
    public event EventHandler<GenerationAcceptedEventArgs>? GenerationAccepted;

    /// <summary>
    /// Event raised when dialog should close.
    /// </summary>
    public event EventHandler? RequestClose;

    /// <summary>
    /// Initializes a new instance of the AIGenerationViewModel.
    /// </summary>
    public AIGenerationViewModel(
        IGenerationPipelineService generationService,
        IAIProviderFactory providerFactory,
        IContextBuilderService contextBuilder,
        ILogger<AIGenerationViewModel> logger)
    {
        _generationService = generationService;
        _providerFactory = providerFactory;
        _contextBuilder = contextBuilder;
        _logger = logger;

        LoadAvailableModes();
        LoadAvailableProviders();
    }

    /// <summary>
    /// Initializes the dialog with project and chapter context.
    /// </summary>
    public void Initialize(Project project, Chapter chapter)
    {
        Project = project;
        Chapter = chapter;

        // Reset state
        StreamingContent = string.Empty;
        FinalContent = null;
        IsGenerating = false;
        IsComplete = false;
        HasError = false;
        ErrorMessage = null;
        Progress = 0;
        GeneratedWordCount = 0;
        TokensUsed = 0;
        EstimatedCost = 0;
        CurrentPhase = "Ready";

        // Calculate initial cost estimate
        UpdateCostEstimate();

        _logger.LogDebug("AI Generation dialog initialized for chapter {ChapterNumber}", chapter.Order);
    }

    private void LoadAvailableModes()
    {
        AvailableModes.Clear();
        foreach (var mode in _generationService.GetAvailableModes())
        {
            AvailableModes.Add(mode);
        }
    }

    private void LoadAvailableProviders()
    {
        AvailableProviders.Clear();

        foreach (var provider in _providerFactory.GetAllProviders())
        {
            var providerType = provider.ProviderName switch
            {
                "Claude (Anthropic)" => AIProviderType.Claude,
                "OpenAI" => AIProviderType.OpenAI,
                "Gemini (Google)" => AIProviderType.Gemini,
                _ => AIProviderType.Claude
            };

            AvailableProviders.Add(new ProviderInfo
            {
                Type = providerType,
                Name = provider.ProviderName,
                IsConfigured = provider.IsConfigured,
                SupportsStreaming = provider.SupportsStreaming,
                MaxContextTokens = provider.MaxContextTokens
            });
        }

        // Set default provider to first configured one
        var configuredProvider = AvailableProviders.FirstOrDefault(p => p.IsConfigured);
        if (configuredProvider != null)
        {
            SelectedProvider = configuredProvider.Type;
        }
    }

    partial void OnSelectedProviderChanged(AIProviderType value)
    {
        UpdateAvailableModels();
        UpdateCostEstimate();
    }

    partial void OnSelectedModeChanged(GenerationMode value)
    {
        UpdateAvailableModels();
        UpdateCostEstimate();
    }

    private void UpdateAvailableModels()
    {
        AvailableModels.Clear();

        var provider = _providerFactory.GetProvider(SelectedProvider);
        foreach (var model in provider.GetAvailableModels())
        {
            AvailableModels.Add(model);
        }

        // Select appropriate model for mode
        SelectedModel = SelectedMode switch
        {
            GenerationMode.Quick => AvailableModels.FirstOrDefault(m =>
                m.Contains("haiku", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("mini", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("flash", StringComparison.OrdinalIgnoreCase)) ?? AvailableModels.FirstOrDefault() ?? string.Empty,

            GenerationMode.Standard => AvailableModels.FirstOrDefault(m =>
                m.Contains("sonnet", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("4o", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("pro", StringComparison.OrdinalIgnoreCase)) ?? AvailableModels.FirstOrDefault() ?? string.Empty,

            GenerationMode.Premium => AvailableModels.FirstOrDefault(m =>
                m.Contains("opus", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("o1", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("2.0", StringComparison.OrdinalIgnoreCase)) ?? AvailableModels.FirstOrDefault() ?? string.Empty,

            _ => AvailableModels.FirstOrDefault() ?? string.Empty
        };
    }

    private void UpdateCostEstimate()
    {
        if (Project == null || Chapter == null) return;

        var request = CreatePipelineRequest();
        CostEstimate = _generationService.EstimateCost(request);
        EstimatedContextTokens = CostEstimate.EstimatedInputTokens;
        EstimatedCost = CostEstimate.EstimatedCostUsd;
    }

    private ChapterGenerationPipelineRequest CreatePipelineRequest()
    {
        return new ChapterGenerationPipelineRequest
        {
            Project = Project!,
            Chapter = Chapter!,
            Mode = SelectedMode,
            Provider = SelectedProvider,
            ModelId = SelectedModel,
            Temperature = Temperature,
            CustomInstructions = CustomInstructions,
            IncludeCharacterContext = IncludeCharacterContext,
            IncludeLocationContext = IncludeLocationContext,
            IncludePreviousSummary = IncludePreviousSummary
        };
    }

    [RelayCommand(CanExecute = nameof(CanStartGeneration))]
    private async Task StartGenerationAsync()
    {
        if (Project == null || Chapter == null) return;

        IsGenerating = true;
        IsComplete = false;
        HasError = false;
        ErrorMessage = null;
        StreamingContent = string.Empty;
        FinalContent = null;
        Progress = 0;
        GeneratedWordCount = 0;
        TokensUsed = 0;

        _cancellationTokenSource = new CancellationTokenSource();
        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogInformation("Starting AI generation for chapter {ChapterNumber}, Mode: {Mode}, Provider: {Provider}",
                Chapter.Order, SelectedMode, SelectedProvider);

            var request = CreatePipelineRequest();

            // Use streaming for real-time updates
            var contentBuilder = new StringBuilder();

            await foreach (var chunk in _generationService.GenerateChapterStreamingAsync(
                request,
                _cancellationTokenSource.Token))
            {
                if (chunk.Text != null)
                {
                    contentBuilder.Append(chunk.Text);
                    StreamingContent = contentBuilder.ToString();
                    GeneratedWordCount = CountWords(StreamingContent);

                    // Update progress based on word count vs target
                    if (Chapter.TargetWordCount > 0)
                    {
                        Progress = Math.Min(95, (double)GeneratedWordCount / Chapter.TargetWordCount * 100);
                    }
                }

                if (chunk.IsFinal)
                {
                    FinalContent = contentBuilder.ToString();

                    if (chunk.Usage != null)
                    {
                        TokensUsed = chunk.Usage.TotalTokens;
                        EstimatedCost = chunk.Usage.EstimatedCost;
                    }

                    CurrentPhase = "Complete";
                    Progress = 100;
                }

                ElapsedTime = DateTime.UtcNow - startTime;
            }

            IsComplete = true;
            CurrentPhase = "Generation complete!";
            OnPropertyChanged(nameof(CanAccept));

            _logger.LogInformation("Generation complete: {WordCount} words, {Tokens} tokens, {Duration}",
                GeneratedWordCount, TokensUsed, ElapsedTime);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Generation cancelled by user");
            CurrentPhase = "Cancelled";
            HasError = true;
            ErrorMessage = "Generation was cancelled.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Generation failed");
            HasError = true;
            ErrorMessage = $"Generation failed: {ex.Message}";
            CurrentPhase = "Error";
        }
        finally
        {
            IsGenerating = false;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    private bool CanStartGeneration() => !IsGenerating && Project != null && Chapter != null;

    [RelayCommand(CanExecute = nameof(IsGenerating))]
    private void CancelGeneration()
    {
        _logger.LogDebug("Cancelling generation");
        _cancellationTokenSource?.Cancel();
        CurrentPhase = "Cancelling...";
    }

    [RelayCommand]
    private async Task RegenerateAsync()
    {
        // Reset and regenerate
        await StartGenerationAsync();
    }

    [RelayCommand(CanExecute = nameof(CanAccept))]
    private void Accept()
    {
        if (string.IsNullOrEmpty(FinalContent)) return;

        _logger.LogDebug("Generation accepted: {WordCount} words", GeneratedWordCount);

        GenerationAccepted?.Invoke(this, new GenerationAcceptedEventArgs
        {
            Content = FinalContent,
            WordCount = GeneratedWordCount,
            TokensUsed = TokensUsed,
            Cost = EstimatedCost,
            Duration = ElapsedTime,
            Mode = SelectedMode,
            Provider = SelectedProvider,
            Model = SelectedModel
        });

        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand(CanExecute = nameof(CanAccept))]
    private void AcceptAndEdit()
    {
        // Same as accept but signals that user wants to edit
        Accept();
    }

    [RelayCommand]
    private void Close()
    {
        if (IsGenerating)
        {
            CancelGeneration();
        }

        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    private static int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        return text.Split([' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries).Length;
    }
}

/// <summary>
/// Information about an AI provider.
/// </summary>
public sealed class ProviderInfo
{
    /// <summary>
    /// Gets or sets the provider type.
    /// </summary>
    public AIProviderType Type { get; init; }

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the provider is configured.
    /// </summary>
    public bool IsConfigured { get; init; }

    /// <summary>
    /// Gets or sets whether the provider supports streaming.
    /// </summary>
    public bool SupportsStreaming { get; init; }

    /// <summary>
    /// Gets or sets the maximum context tokens.
    /// </summary>
    public int MaxContextTokens { get; init; }
}

/// <summary>
/// Event args for when generation is accepted.
/// </summary>
public sealed class GenerationAcceptedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the generated content.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Gets the word count.
    /// </summary>
    public int WordCount { get; init; }

    /// <summary>
    /// Gets the tokens used.
    /// </summary>
    public int TokensUsed { get; init; }

    /// <summary>
    /// Gets the cost.
    /// </summary>
    public decimal Cost { get; init; }

    /// <summary>
    /// Gets the duration.
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Gets the generation mode used.
    /// </summary>
    public GenerationMode Mode { get; init; }

    /// <summary>
    /// Gets the provider used.
    /// </summary>
    public AIProviderType Provider { get; init; }

    /// <summary>
    /// Gets the model used.
    /// </summary>
    public string Model { get; init; } = string.Empty;
}
