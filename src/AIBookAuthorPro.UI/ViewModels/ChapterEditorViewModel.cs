// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Windows.Documents;
using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Enums;
using AIBookAuthorPro.Core.Interfaces;
using AIBookAuthorPro.Core.Models;
using AIBookAuthorPro.UI.Behaviors;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.UI.ViewModels;

/// <summary>
/// ViewModel for the chapter editor view.
/// </summary>
public partial class ChapterEditorViewModel : ObservableObject, IHasFlowDocumentService
{
    private readonly IProjectService _projectService;
    private readonly IGenerationPipelineService _generationService;
    private readonly ILogger<ChapterEditorViewModel> _logger;

    // Undo/Redo stacks
    private readonly Stack<string> _undoStack = new();
    private readonly Stack<string> _redoStack = new();
    private const int MaxUndoHistory = 50;

    [ObservableProperty]
    private Chapter? _chapter;

    [ObservableProperty]
    private Project? _project;

    [ObservableProperty]
    private string _content = string.Empty;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string? _summary;

    [ObservableProperty]
    private string? _outline;

    [ObservableProperty]
    private string? _notes;

    [ObservableProperty]
    private int _wordCount;

    [ObservableProperty]
    private int _targetWordCount = 3000;

    [ObservableProperty]
    private double _completionPercentage;

    [ObservableProperty]
    private ChapterStatus _status = ChapterStatus.NotStarted;

    [ObservableProperty]
    private bool _isModified;

    [ObservableProperty]
    private bool _isGenerating;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UndoCommand))]
    private bool _canUndo;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RedoCommand))]
    private bool _canRedo;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    /// <summary>
    /// Gets the FlowDocument service.
    /// </summary>
    public IFlowDocumentService FlowDocumentService { get; }

    /// <summary>
    /// Initializes a new instance of the ChapterEditorViewModel.
    /// </summary>
    public ChapterEditorViewModel(
        IProjectService projectService,
        IGenerationPipelineService generationService,
        IFlowDocumentService flowDocumentService,
        ILogger<ChapterEditorViewModel> logger)
    {
        _projectService = projectService;
        _generationService = generationService;
        FlowDocumentService = flowDocumentService;
        _logger = logger;
    }

    /// <summary>
    /// Loads a chapter for editing.
    /// </summary>
    public void LoadChapter(Project project, Chapter chapter)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(chapter);

        _logger.LogDebug("Loading chapter {ChapterNumber}: {Title}", chapter.Order, chapter.Title);

        Project = project;
        Chapter = chapter;

        // Load chapter data into properties
        Title = chapter.Title;
        Content = chapter.Content;
        Summary = chapter.Summary;
        Outline = chapter.Outline;
        Notes = chapter.Notes;
        TargetWordCount = chapter.TargetWordCount > 0 ? chapter.TargetWordCount : 3000;
        Status = chapter.Status;

        // Reset undo/redo
        _undoStack.Clear();
        _redoStack.Clear();
        CanUndo = false;
        CanRedo = false;
        IsModified = false;

        // Update word count
        UpdateWordCount();

        StatusMessage = $"Editing: {Title}";
    }

    /// <summary>
    /// Creates a new chapter.
    /// </summary>
    public void CreateNewChapter(Project project, int chapterNumber)
    {
        ArgumentNullException.ThrowIfNull(project);

        var chapter = new Chapter
        {
            Id = Guid.NewGuid(),
            Title = $"Chapter {chapterNumber}",
            Order = chapterNumber,
            TargetWordCount = 3000,
            Status = ChapterStatus.NotStarted,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        project.Chapters.Add(chapter);
        LoadChapter(project, chapter);
    }

    partial void OnContentChanged(string value)
    {
        if (Chapter != null && !IsGenerating)
        {
            PushUndo();
            UpdateWordCount();
            IsModified = true;
        }
    }

    partial void OnTitleChanged(string value)
    {
        if (Chapter != null)
        {
            IsModified = true;
        }
    }

    private void UpdateWordCount()
    {
        if (string.IsNullOrWhiteSpace(Content))
        {
            WordCount = 0;
        }
        else
        {
            // Simple word count from plain text
            var text = Content;
            // If content is XAML, try to extract text
            if (Content.StartsWith("<FlowDocument"))
            {
                var result = FlowDocumentService.FromXaml(Content);
                if (result.IsSuccess && result.Value != null)
                {
                    var textResult = FlowDocumentService.ToPlainText(result.Value);
                    if (textResult.IsSuccess)
                    {
                        text = textResult.Value ?? string.Empty;
                    }
                }
            }

            WordCount = text.Split([' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries).Length;
        }

        CompletionPercentage = TargetWordCount > 0
            ? Math.Min(100, (double)WordCount / TargetWordCount * 100)
            : 0;

        // Update status based on word count
        if (Status == ChapterStatus.NotStarted && WordCount > 0)
        {
            Status = ChapterStatus.Drafting;
        }
    }

    private void PushUndo()
    {
        if (_undoStack.Count >= MaxUndoHistory)
        {
            // Remove oldest items
            var temp = _undoStack.ToList();
            temp.RemoveAt(temp.Count - 1);
            _undoStack.Clear();
            foreach (var item in temp.AsEnumerable().Reverse())
            {
                _undoStack.Push(item);
            }
        }

        _undoStack.Push(Content);
        _redoStack.Clear();
        CanUndo = _undoStack.Count > 0;
        CanRedo = false;
    }

    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void Undo()
    {
        if (_undoStack.Count == 0) return;

        _redoStack.Push(Content);
        var previousContent = _undoStack.Pop();

        // Temporarily disable push to avoid infinite loop
        var temp = Content;
        Content = previousContent;

        CanUndo = _undoStack.Count > 0;
        CanRedo = true;

        _logger.LogDebug("Undo performed, {UndoCount} remaining", _undoStack.Count);
    }

    [RelayCommand(CanExecute = nameof(CanRedo))]
    private void Redo()
    {
        if (_redoStack.Count == 0) return;

        _undoStack.Push(Content);
        Content = _redoStack.Pop();

        CanUndo = true;
        CanRedo = _redoStack.Count > 0;

        _logger.LogDebug("Redo performed, {RedoCount} remaining", _redoStack.Count);
    }

    [RelayCommand]
    private async Task SaveChapterAsync()
    {
        if (Chapter == null || Project == null) return;

        IsSaving = true;
        StatusMessage = "Saving...";

        try
        {
            _logger.LogDebug("Saving chapter {ChapterNumber}", Chapter.Order);

            // Update chapter from properties
            Chapter.Title = Title;
            Chapter.Content = Content;
            Chapter.Summary = Summary;
            Chapter.Outline = Outline;
            Chapter.Notes = Notes;
            Chapter.TargetWordCount = TargetWordCount;
            Chapter.Status = Status;
            Chapter.MarkAsModified();

            // Mark project as modified
            _projectService.MarkAsModified();

            // Auto-save project
            var result = await _projectService.SaveProjectAsync();

            if (result.IsSuccess)
            {
                IsModified = false;
                StatusMessage = "Saved successfully";
                _logger.LogInformation("Chapter {ChapterNumber} saved", Chapter.Order);
            }
            else
            {
                StatusMessage = $"Save failed: {result.Error}";
                _logger.LogWarning("Failed to save chapter: {Error}", result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving chapter");
            StatusMessage = $"Save failed: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private void UpdateStatus(ChapterStatus newStatus)
    {
        Status = newStatus;
        IsModified = true;
        _logger.LogDebug("Chapter status changed to {Status}", newStatus);
    }

    [RelayCommand]
    private void InsertSceneBreak()
    {
        // Insert scene break marker
        var sceneBreak = "\n\n***\n\n";

        // If content is XAML, we need to handle it differently
        if (Content.StartsWith("<FlowDocument"))
        {
            var result = FlowDocumentService.FromXaml(Content);
            if (result.IsSuccess && result.Value != null)
            {
                FlowDocumentService.InsertText(result.Value, sceneBreak);
                var xamlResult = FlowDocumentService.ToXaml(result.Value);
                if (xamlResult.IsSuccess)
                {
                    Content = xamlResult.Value ?? Content;
                }
            }
        }
        else
        {
            Content += sceneBreak;
        }

        IsModified = true;
        _logger.LogDebug("Scene break inserted");
    }

    [RelayCommand]
    private void ClearContent()
    {
        if (string.IsNullOrEmpty(Content)) return;

        PushUndo();
        Content = string.Empty;
        IsModified = true;
        StatusMessage = "Content cleared";
    }

    [RelayCommand]
    private void CopyToClipboard()
    {
        try
        {
            var text = Content;

            // Extract plain text if XAML
            if (Content.StartsWith("<FlowDocument"))
            {
                var result = FlowDocumentService.FromXaml(Content);
                if (result.IsSuccess && result.Value != null)
                {
                    var textResult = FlowDocumentService.ToPlainText(result.Value);
                    if (textResult.IsSuccess)
                    {
                        text = textResult.Value ?? Content;
                    }
                }
            }

            System.Windows.Clipboard.SetText(text);
            StatusMessage = "Content copied to clipboard";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to copy to clipboard");
            StatusMessage = "Failed to copy to clipboard";
        }
    }

    [RelayCommand]
    private async Task ExportToMarkdownAsync()
    {
        if (string.IsNullOrEmpty(Content)) return;

        try
        {
            string markdown;

            if (Content.StartsWith("<FlowDocument"))
            {
                var result = FlowDocumentService.FromXaml(Content);
                if (result.IsSuccess && result.Value != null)
                {
                    var mdResult = FlowDocumentService.ToMarkdown(result.Value);
                    markdown = mdResult.IsSuccess ? mdResult.Value ?? Content : Content;
                }
                else
                {
                    markdown = Content;
                }
            }
            else
            {
                markdown = Content;
            }

            // Show save dialog (would normally use a dialog service)
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Markdown files (*.md)|*.md|All files (*.*)|*.*",
                FileName = $"{Title.Replace(" ", "_")}.md"
            };

            if (dialog.ShowDialog() == true)
            {
                await System.IO.File.WriteAllTextAsync(dialog.FileName, markdown);
                StatusMessage = $"Exported to {dialog.FileName}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export to markdown");
            StatusMessage = $"Export failed: {ex.Message}";
        }
    }

    /// <summary>
    /// Appends generated content to the editor.
    /// </summary>
    public void AppendGeneratedContent(string generatedContent)
    {
        if (string.IsNullOrEmpty(generatedContent)) return;

        PushUndo();

        if (string.IsNullOrEmpty(Content))
        {
            // If empty, convert markdown to FlowDocument
            var result = FlowDocumentService.FromMarkdown(generatedContent);
            if (result.IsSuccess && result.Value != null)
            {
                var xamlResult = FlowDocumentService.ToXaml(result.Value);
                if (xamlResult.IsSuccess)
                {
                    Content = xamlResult.Value ?? generatedContent;
                }
                else
                {
                    Content = generatedContent;
                }
            }
            else
            {
                Content = generatedContent;
            }
        }
        else
        {
            // Append to existing content
            if (Content.StartsWith("<FlowDocument"))
            {
                var docResult = FlowDocumentService.FromXaml(Content);
                var newDocResult = FlowDocumentService.FromMarkdown(generatedContent);

                if (docResult.IsSuccess && docResult.Value != null &&
                    newDocResult.IsSuccess && newDocResult.Value != null)
                {
                    var mergeResult = FlowDocumentService.MergeDocuments(docResult.Value, newDocResult.Value);
                    if (mergeResult.IsSuccess)
                    {
                        var xamlResult = FlowDocumentService.ToXaml(docResult.Value);
                        if (xamlResult.IsSuccess)
                        {
                            Content = xamlResult.Value ?? Content + "\n\n" + generatedContent;
                        }
                    }
                }
            }
            else
            {
                Content += "\n\n" + generatedContent;
            }
        }

        IsModified = true;
        UpdateWordCount();
        StatusMessage = "Generated content appended";
    }

    /// <summary>
    /// Replaces editor content with generated content.
    /// </summary>
    public void ReplaceWithGeneratedContent(string generatedContent)
    {
        if (string.IsNullOrEmpty(generatedContent)) return;

        PushUndo();

        var result = FlowDocumentService.FromMarkdown(generatedContent);
        if (result.IsSuccess && result.Value != null)
        {
            var xamlResult = FlowDocumentService.ToXaml(result.Value);
            if (xamlResult.IsSuccess)
            {
                Content = xamlResult.Value ?? generatedContent;
            }
            else
            {
                Content = generatedContent;
            }
        }
        else
        {
            Content = generatedContent;
        }

        IsModified = true;
        UpdateWordCount();
        StatusMessage = "Content replaced with generated text";
    }
}
