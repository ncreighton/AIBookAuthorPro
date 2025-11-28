// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Collections.ObjectModel;
using OutlineItemTypeEnum = AIBookAuthorPro.Core.Enums.OutlineItemType;
using AIBookAuthorPro.Core.Enums;
using AIBookAuthorPro.Core.Interfaces;
using AIBookAuthorPro.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.UI.ViewModels;

/// <summary>
/// ViewModel for the outline editor view with drag-drop tree structure.
/// </summary>
public partial class OutlineEditorViewModel : ObservableObject
{
    private readonly IProjectService _projectService;
    private readonly IGenerationPipelineService _generationService;
    private readonly ILogger<OutlineEditorViewModel> _logger;

    [ObservableProperty]
    private Project? _project;

    [ObservableProperty]
    private ObservableCollection<OutlineItemViewModel> _outlineItems = [];

    [ObservableProperty]
    private OutlineItemViewModel? _selectedItem;

    [ObservableProperty]
    private bool _isModified;

    [ObservableProperty]
    private bool _isGenerating;

    [ObservableProperty]
    private string _statusMessage = "Outline Editor";

    [ObservableProperty]
    private string? _errorMessage;

    /// <summary>
    /// Gets the available outline item types.
    /// </summary>
    public OutlineItemTypeEnum[] ItemTypes { get; } = Enum.GetValues<OutlineItemTypeEnum>();

    /// <summary>
    /// Event raised when navigation back is requested.
    /// </summary>
    public event EventHandler? BackRequested;

    /// <summary>
    /// Initializes a new instance of OutlineEditorViewModel.
    /// </summary>
    public OutlineEditorViewModel(
        IProjectService projectService,
        IGenerationPipelineService generationService,
        ILogger<OutlineEditorViewModel> logger)
    {
        _projectService = projectService;
        _generationService = generationService;
        _logger = logger;
    }

    /// <summary>
    /// Loads the outline from a project.
    /// </summary>
    public void LoadProject(Project project)
    {
        ArgumentNullException.ThrowIfNull(project);

        _logger.LogDebug("Loading outline for project {ProjectName}", project.Name);

        Project = project;

        // Ensure outline exists
        project.Outline ??= new Outline();

        RefreshOutlineTree();
        IsModified = false;
        StatusMessage = $"{OutlineItems.Count} items";
    }

    /// <summary>
    /// Refreshes the outline tree from project data.
    /// </summary>
    public void RefreshOutlineTree()
    {
        if (Project?.Outline == null) return;

        OutlineItems.Clear();

        // Add root items directly (Outline.Items are root-level items)
        foreach (var item in Project.Outline.Items.OrderBy(i => i.Order))
        {
            var vm = CreateOutlineItemViewModel(item);
            OutlineItems.Add(vm);
        }
    }

    private OutlineItemViewModel CreateOutlineItemViewModel(OutlineItem item)
    {
        var vm = new OutlineItemViewModel(item);
        vm.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName != nameof(OutlineItemViewModel.IsExpanded))
            {
                IsModified = true;
            }
        };

        // Add children recursively
        foreach (var child in item.Children.OrderBy(c => c.Order))
        {
            var childVm = CreateOutlineItemViewModel(child);
            vm.Children.Add(childVm);
        }

        return vm;
    }

    [RelayCommand]
    private void AddItem(OutlineItemTypeEnum type)
    {
        if (Project?.Outline == null) return;

        var newItem = new OutlineItem
        {
            ItemType = (Core.Models.OutlineItemType)(int)type,
            Title = GetDefaultTitle(type),
            Order = GetNextOrder(SelectedItem?.Item.Id)
        };

        if (SelectedItem != null)
        {
            SelectedItem.Item.AddChild(newItem);
        }
        else
        {
            Project.Outline.AddItem(newItem);
        }

        RefreshOutlineTree();
        SelectItem(newItem.Id);
        IsModified = true;

        _logger.LogInformation("Added outline item: {Title} ({Type})", newItem.Title, type);
    }

    private static string GetDefaultTitle(OutlineItemTypeEnum type) => type switch
    {
        OutlineItemTypeEnum.Act => "New Act",
        OutlineItemTypeEnum.Part => "New Part",
        OutlineItemTypeEnum.Chapter => "New Chapter",
        OutlineItemTypeEnum.Scene => "New Scene",
        OutlineItemTypeEnum.Beat => "New Beat",
        OutlineItemTypeEnum.Note => "New Note",
        _ => "New Item"
    };

    private int GetNextOrder(Guid? parentId)
    {
        if (Project?.Outline == null) return 1;

        // For root items
        if (parentId == null)
        {
            return Project.Outline.Items.Count > 0 ? Project.Outline.Items.Max(i => i.Order) + 1 : 1;
        }

        // For child items, find the parent and count its children
        var parent = Project.Outline.GetAllItemsFlattened().FirstOrDefault(i => i.Id == parentId);
        return parent != null ? parent.Children.Count + 1 : 1;
    }

    private void SelectItem(Guid itemId)
    {
        SelectedItem = FindItemById(OutlineItems, itemId);
    }

    private static OutlineItemViewModel? FindItemById(
        IEnumerable<OutlineItemViewModel> items,
        Guid id)
    {
        foreach (var item in items)
        {
            if (item.Item.Id == id) return item;

            var found = FindItemById(item.Children, id);
            if (found != null) return found;
        }
        return null;
    }

    [RelayCommand]
    private void DeleteItem(OutlineItemViewModel? item)
    {
        if (item == null || Project?.Outline == null) return;

        // Remove item and all descendants
        var idsToRemove = GetAllDescendantIds(item.Item.Id).Append(item.Item.Id).ToHashSet();
        foreach (var idToRemove in idsToRemove)
        {
            // Try removing from root items first
            if (!Project.Outline.RemoveItem(idToRemove))
            {
                // If not in root, find and remove from parent's children
                var itemToRemove = Project.Outline.GetAllItemsFlattened().FirstOrDefault(i => i.Id == idToRemove);
                if (itemToRemove != null)
                {
                    // Find parent and remove child
                    var parent = Project.Outline.GetAllItemsFlattened().FirstOrDefault(p => p.Children.Any(c => c.Id == idToRemove));
                    parent?.RemoveChild(idToRemove);
                }
            }
        }

        RefreshOutlineTree();
        SelectedItem = null;
        IsModified = true;

        _logger.LogInformation("Deleted outline item: {Title}", item.Title);
    }

    private IEnumerable<Guid> GetAllDescendantIds(Guid parentId)
    {
        if (Project?.Outline == null) yield break;

        // Find the parent item and get all its descendants
        var parent = Project.Outline.GetAllItemsFlattened().FirstOrDefault(i => i.Id == parentId);
        if (parent == null) yield break;

        foreach (var descendant in parent.GetAllChildrenFlattened())
        {
            yield return descendant.Id;
        }
    }

    [RelayCommand]
    private void MoveUp(OutlineItemViewModel? item)
    {
        if (item == null || Project?.Outline == null) return;

        // TODO: Implement move up for hierarchical structure
        // For now, just refresh
        RefreshOutlineTree();
        SelectItem(item.Item.Id);
        IsModified = true;
    }

    [RelayCommand]
    private void MoveDown(OutlineItemViewModel? item)
    {
        if (item == null || Project?.Outline == null) return;

        // TODO: Implement move down for hierarchical structure
        // For now, just refresh
        RefreshOutlineTree();
        SelectItem(item.Item.Id);
        IsModified = true;
    }

    [RelayCommand]
    private void Indent(OutlineItemViewModel? item)
    {
        if (item == null || Project?.Outline == null) return;

        // TODO: Implement indent for hierarchical structure
        // For now, just refresh
        RefreshOutlineTree();
        SelectItem(item.Item.Id);
        IsModified = true;
    }

    [RelayCommand]
    private void Outdent(OutlineItemViewModel? item)
    {
        if (item == null || Project?.Outline == null) return;

        // TODO: Implement outdent for hierarchical structure
        // For now, just refresh
        RefreshOutlineTree();
        SelectItem(item.Item.Id);
        IsModified = true;
    }

    [RelayCommand]
    private void ExpandAll()
    {
        SetExpandedState(OutlineItems, true);
    }

    [RelayCommand]
    private void CollapseAll()
    {
        SetExpandedState(OutlineItems, false);
    }

    private static void SetExpandedState(IEnumerable<OutlineItemViewModel> items, bool expanded)
    {
        foreach (var item in items)
        {
            item.IsExpanded = expanded;
            SetExpandedState(item.Children, expanded);
        }
    }

    [RelayCommand]
    private async Task GenerateOutlineAsync(CancellationToken cancellationToken)
    {
        if (Project == null) return;

        IsGenerating = true;
        ErrorMessage = null;
        StatusMessage = "Generating outline...";

        try
        {
            // Build a prompt for outline generation
            var prompt = $@"Generate a detailed outline for a {Project.Metadata?.Genre ?? "fiction"} book titled ""{Project.Name}"".

Description: {Project.Metadata?.Description ?? "A compelling story."}

Create a three-act structure with:
- Act I (25%): Setup, inciting incident, first plot point
- Act II (50%): Rising action, midpoint, complications
- Act III (25%): Climax, resolution

For each act, provide 3-4 chapters with:
- Chapter title
- Brief summary (2-3 sentences)
- Key scenes or beats

Format as a hierarchical outline.";

            _logger.LogInformation("Would generate outline with prompt: {Prompt}", prompt[..Math.Min(100, prompt.Length)]);
            
            // For now, create a sample outline structure
            await Task.Delay(500, cancellationToken); // Simulate generation

            if (Project.Outline?.Items.Count == 0)
            {
                CreateSampleOutline();
            }

            StatusMessage = "Outline generated";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Generation cancelled";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            StatusMessage = "Generation failed";
            _logger.LogError(ex, "Outline generation failed");
        }
        finally
        {
            IsGenerating = false;
        }
    }

    private void CreateSampleOutline()
    {
        if (Project?.Outline == null) return;

        var acts = new[]
        {
            ("Act I: Setup", new[] { "Opening Hook", "Character Introduction", "Inciting Incident", "First Plot Point" }),
            ("Act II: Confrontation", new[] { "Fun and Games", "Midpoint", "Bad Guys Close In", "All Is Lost", "Dark Night of Soul" }),
            ("Act III: Resolution", new[] { "Break into Three", "Finale", "Resolution" })
        };

        var order = 0;
        foreach (var (actTitle, chapters) in acts)
        {
            var act = new OutlineItem
            {
                ItemType = Core.Models.OutlineItemType.Act,
                Title = actTitle,
                Order = order++
            };
            Project.Outline.AddItem(act);

            var chapterOrder = 1;
            foreach (var chapterTitle in chapters)
            {
                var chapter = new OutlineItem
                {
                    ItemType = Core.Models.OutlineItemType.Chapter,
                    Title = chapterTitle,
                    Order = chapterOrder++
                };
                act.AddChild(chapter);
            }
        }
        RefreshOutlineTree();
        IsModified = true;
    }

    [RelayCommand]
    private async Task CreateChapterFromItemAsync(OutlineItemViewModel? item)
    {
        if (item?.Item.ItemType != Core.Models.OutlineItemType.Chapter || Project == null) return;

        // Check if chapter already exists
        if (item.Item.LinkedChapterId.HasValue)
        {
            StatusMessage = "Chapter already exists for this outline item";
            return;
        }

        // Create new chapter
        var chapter = new Chapter
        {
            Title = item.Item.Title,
            Order = Project.Chapters.Count + 1,
            Status = ChapterStatus.Outlined
        };

        Project.AddChapter(chapter);
        item.Item.LinkedChapterId = chapter.Id;

        await _projectService.SaveAsync(Project);

        StatusMessage = $"Created chapter: {chapter.Title}";
        _logger.LogInformation("Created chapter from outline: {Title}", chapter.Title);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (Project == null) return;
        var result = await _projectService.SaveAsync(Project);

        if (result.IsSuccess)
        {
            IsModified = false;
            StatusMessage = "Outline saved";
        }
        else
        {
            ErrorMessage = result.Error;
            StatusMessage = "Save failed";
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        BackRequested?.Invoke(this, EventArgs.Empty);
    }
}

/// <summary>
/// ViewModel wrapper for an outline item.
/// </summary>
public partial class OutlineItemViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isExpanded = true;

    [ObservableProperty]
    private bool _isSelected;

    /// <summary>
    /// Gets the underlying outline item.
    /// </summary>
    public OutlineItem Item { get; }

    /// <summary>
    /// Gets the child items.
    /// </summary>
    public ObservableCollection<OutlineItemViewModel> Children { get; } = [];

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    public string Title
    {
        get => Item.Title;
        set
        {
            if (Item.Title != value)
            {
                Item.Title = value;
                // Note: MarkAsModified is protected
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string? Description
    {
        get => Item.Summary;
        set
        {
            if (Item.Summary != value)
            {
                Item.Summary = value;
                // Note: MarkAsModified is protected, outline items update their own timestamps
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets the item type.
    /// </summary>
    public OutlineItemTypeEnum Type => (OutlineItemTypeEnum)(int)Item.ItemType;

    /// <summary>
    /// Gets whether this item has children.
    /// </summary>
    public bool HasChildren => Children.Count > 0;

    /// <summary>
    /// Gets whether this item has a linked chapter.
    /// </summary>
    public bool HasLinkedChapter => Item.LinkedChapterId.HasValue;

    /// <summary>
    /// Gets the icon for this item type.
    /// </summary>
    public string Icon => Item.ItemType switch
    {
        Core.Models.OutlineItemType.Act => "TheaterMasks",
        Core.Models.OutlineItemType.Part => "BookOpenPageVariant",
        Core.Models.OutlineItemType.Chapter => "FileDocument",
        Core.Models.OutlineItemType.Scene => "MovieOpen",
        Core.Models.OutlineItemType.Beat => "CircleSmall",
        Core.Models.OutlineItemType.Note => "Note",
        _ => "Circle"
    };

    /// <summary>
    /// Initializes a new instance of OutlineItemViewModel.
    /// </summary>
    public OutlineItemViewModel(OutlineItem item)
    {
        Item = item;
    }
}
