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
/// ViewModel for the prompt entry step of the guided creation wizard.
/// </summary>
public partial class PromptEntryViewModel : ObservableObject
{
    private readonly IPromptAnalysisService _promptAnalysisService;
    private readonly ILogger _logger;

    private const int MinimumPromptLength = 50;
    private const int RecommendedPromptLength = 200;

    #region Observable Properties

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    [NotifyPropertyChangedFor(nameof(CharacterCount))]
    [NotifyPropertyChangedFor(nameof(PromptStrength))]
    [NotifyPropertyChangedFor(nameof(PromptStrengthColor))]
    [NotifyPropertyChangedFor(nameof(ValidationMessage))]
    [NotifyCanExecuteChangedFor(nameof(ValidatePromptCommand))]
    private string _promptText = string.Empty;

    [ObservableProperty]
    private string _additionalContext = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _comparableTitles = new();

    [ObservableProperty]
    private ObservableCollection<string> _mustIncludeElements = new();

    [ObservableProperty]
    private ObservableCollection<string> _mustAvoidElements = new();

    [ObservableProperty]
    private PromptSource _selectedSource = PromptSource.Manual;

    [ObservableProperty]
    private bool _isImporting;

    [ObservableProperty]
    private bool _isValidating;

    [ObservableProperty]
    private PromptValidationResult? _validationResult;

    [ObservableProperty]
    private string _importText = string.Empty;

    [ObservableProperty]
    private bool _showImportDialog;

    [ObservableProperty]
    private bool _showAdvancedOptions;

    [ObservableProperty]
    private string _newComparableTitle = string.Empty;

    [ObservableProperty]
    private string _newMustInclude = string.Empty;

    [ObservableProperty]
    private string _newMustAvoid = string.Empty;

    #endregion

    #region Computed Properties

    public bool IsValid => PromptText.Length >= MinimumPromptLength;

    public int CharacterCount => PromptText.Length;

    public double PromptStrength
    {
        get
        {
            if (PromptText.Length < MinimumPromptLength) return 0;
            if (PromptText.Length >= RecommendedPromptLength * 2) return 100;
            
            var baseScore = Math.Min(50, (PromptText.Length - MinimumPromptLength) * 50 / (RecommendedPromptLength - MinimumPromptLength));
            
            // Bonus for additional elements
            if (ComparableTitles.Any()) baseScore += 10;
            if (MustIncludeElements.Any()) baseScore += 10;
            if (!string.IsNullOrWhiteSpace(AdditionalContext)) baseScore += 15;
            
            // Bonus for story elements
            if (ContainsCharacterReference()) baseScore += 5;
            if (ContainsConflictReference()) baseScore += 5;
            if (ContainsSettingReference()) baseScore += 5;
            
            return Math.Min(100, baseScore);
        }
    }

    public string PromptStrengthColor
    {
        get
        {
            return PromptStrength switch
            {
                < 25 => "#F44336",  // Red
                < 50 => "#FF9800",  // Orange
                < 75 => "#FFC107",  // Amber
                _ => "#4CAF50"      // Green
            };
        }
    }

    public string ValidationMessage
    {
        get
        {
            if (PromptText.Length == 0)
                return "Enter your book idea above";
            if (PromptText.Length < MinimumPromptLength)
                return $"Please add {MinimumPromptLength - PromptText.Length} more characters";
            if (PromptText.Length < RecommendedPromptLength)
                return "Good start! Adding more detail will help generate a better blueprint";
            return "Great! Your prompt has enough detail to generate a comprehensive blueprint";
        }
    }

    public IReadOnlyList<PromptSourceOption> SourceOptions { get; } = new List<PromptSourceOption>
    {
        new(PromptSource.Manual, "Write from scratch", "Edit"),
        new(PromptSource.ImportedFromClaude, "Import from Claude", "Forum"),
        new(PromptSource.ImportedFromChatGPT, "Import from ChatGPT", "SmartToy"),
        new(PromptSource.ImportedFromFile, "Import from file", "FileUpload"),
        new(PromptSource.Template, "Start from template", "ContentCopy"),
        new(PromptSource.Clipboard, "Paste from clipboard", "ContentPaste")
    };

    #endregion

    public PromptEntryViewModel(
        IPromptAnalysisService promptAnalysisService,
        ILogger logger)
    {
        _promptAnalysisService = promptAnalysisService ?? throw new ArgumentNullException(nameof(promptAnalysisService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Commands

    [RelayCommand(CanExecute = nameof(IsValid))]
    private async Task ValidatePromptAsync(CancellationToken cancellationToken)
    {
        IsValidating = true;

        try
        {
            var seedPrompt = CreateSeedPrompt();
            var result = _promptAnalysisService.ValidatePrompt(seedPrompt);
            
            if (result.IsSuccess)
            {
                ValidationResult = result.Value;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating prompt");
        }
        finally
        {
            IsValidating = false;
        }
    }

    [RelayCommand]
    private void SelectSource(PromptSource source)
    {
        SelectedSource = source;

        switch (source)
        {
            case PromptSource.ImportedFromClaude:
            case PromptSource.ImportedFromChatGPT:
                ShowImportDialog = true;
                break;
            case PromptSource.Clipboard:
                PasteFromClipboard();
                break;
            case PromptSource.Template:
                // Show template selector
                break;
        }
    }

    [RelayCommand]
    private async Task ImportConversationAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(ImportText)) return;

        IsImporting = true;

        try
        {
            var result = await _promptAnalysisService.ImportFromConversationAsync(
                ImportText, SelectedSource, cancellationToken);

            if (result.IsSuccess)
            {
                PromptText = result.Value!.RawPrompt;
                ShowImportDialog = false;
                ImportText = string.Empty;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing conversation");
        }
        finally
        {
            IsImporting = false;
        }
    }

    [RelayCommand]
    private void CancelImport()
    {
        ShowImportDialog = false;
        ImportText = string.Empty;
    }

    [RelayCommand]
    private void ToggleAdvancedOptions()
    {
        ShowAdvancedOptions = !ShowAdvancedOptions;
    }

    [RelayCommand]
    private void AddComparableTitle()
    {
        if (!string.IsNullOrWhiteSpace(NewComparableTitle) && 
            !ComparableTitles.Contains(NewComparableTitle))
        {
            ComparableTitles.Add(NewComparableTitle);
            NewComparableTitle = string.Empty;
        }
    }

    [RelayCommand]
    private void RemoveComparableTitle(string title)
    {
        ComparableTitles.Remove(title);
    }

    [RelayCommand]
    private void AddMustInclude()
    {
        if (!string.IsNullOrWhiteSpace(NewMustInclude) && 
            !MustIncludeElements.Contains(NewMustInclude))
        {
            MustIncludeElements.Add(NewMustInclude);
            NewMustInclude = string.Empty;
        }
    }

    [RelayCommand]
    private void RemoveMustInclude(string element)
    {
        MustIncludeElements.Remove(element);
    }

    [RelayCommand]
    private void AddMustAvoid()
    {
        if (!string.IsNullOrWhiteSpace(NewMustAvoid) && 
            !MustAvoidElements.Contains(NewMustAvoid))
        {
            MustAvoidElements.Add(NewMustAvoid);
            NewMustAvoid = string.Empty;
        }
    }

    [RelayCommand]
    private void RemoveMustAvoid(string element)
    {
        MustAvoidElements.Remove(element);
    }

    [RelayCommand]
    private void ClearAll()
    {
        PromptText = string.Empty;
        AdditionalContext = string.Empty;
        ComparableTitles.Clear();
        MustIncludeElements.Clear();
        MustAvoidElements.Clear();
        ValidationResult = null;
    }

    #endregion

    #region Public Methods

    public BookSeedPrompt CreateSeedPrompt()
    {
        return new BookSeedPrompt
        {
            RawPrompt = PromptText,
            Source = SelectedSource,
            AdditionalContext = string.IsNullOrWhiteSpace(AdditionalContext) ? null : AdditionalContext,
            ComparableTitles = ComparableTitles.ToList(),
            MustIncludeElements = MustIncludeElements.ToList(),
            MustAvoidElements = MustAvoidElements.ToList(),
            SubmittedAt = DateTime.UtcNow
        };
    }

    public void LoadFromPrompt(BookSeedPrompt prompt)
    {
        PromptText = prompt.RawPrompt;
        SelectedSource = prompt.Source;
        AdditionalContext = prompt.AdditionalContext ?? string.Empty;
        
        ComparableTitles.Clear();
        foreach (var title in prompt.ComparableTitles ?? Enumerable.Empty<string>())
            ComparableTitles.Add(title);

        MustIncludeElements.Clear();
        foreach (var element in prompt.MustIncludeElements ?? Enumerable.Empty<string>())
            MustIncludeElements.Add(element);

        MustAvoidElements.Clear();
        foreach (var element in prompt.MustAvoidElements ?? Enumerable.Empty<string>())
            MustAvoidElements.Add(element);
    }

    #endregion

    #region Private Methods

    private void PasteFromClipboard()
    {
        try
        {
            if (System.Windows.Clipboard.ContainsText())
            {
                PromptText = System.Windows.Clipboard.GetText();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error pasting from clipboard");
        }
    }

    private bool ContainsCharacterReference()
    {
        var keywords = new[] { "character", "protagonist", "hero", "heroine", "she", "he", "they", "woman", "man" };
        return keywords.Any(k => PromptText.Contains(k, StringComparison.OrdinalIgnoreCase));
    }

    private bool ContainsConflictReference()
    {
        var keywords = new[] { "conflict", "struggle", "fight", "must", "challenge", "threat", "danger", "against" };
        return keywords.Any(k => PromptText.Contains(k, StringComparison.OrdinalIgnoreCase));
    }

    private bool ContainsSettingReference()
    {
        var keywords = new[] { "world", "city", "kingdom", "future", "past", "century", "planet", "forest", "ocean" };
        return keywords.Any(k => PromptText.Contains(k, StringComparison.OrdinalIgnoreCase));
    }

    #endregion
}

/// <summary>
/// Option for prompt source selection.
/// </summary>
public sealed record PromptSourceOption(
    PromptSource Source,
    string DisplayName,
    string Icon);
