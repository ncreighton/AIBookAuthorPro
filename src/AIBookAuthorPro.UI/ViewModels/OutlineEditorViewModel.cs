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
        project.Outline ??= new Outline
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

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

        // Build tree from flat list
        var itemsByParent = Project.Outline.Items
            .GroupBy(i => i.ParentId)
            .ToDictionary(g => g.Key, g => g.OrderBy(i => i.Order).ToList());

        // Add root items (no parent)
        if (itemsByParent.TryGetValue(null, out var rootItems))
        {
            foreach (var item in rootItems)
            {
                var vm = CreateOutlineItemViewModel(item, itemsByParent);
                OutlineItems.Add(vm);
            }
        }
    }

    private OutlineItemViewModel CreateOutlineItemViewModel(
        OutlineItem item,
        Dictionary<Guid?, List<OutlineItem>> itemsByParent)
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
        if (itemsByParent.TryGetValue(item.Id, out var children))
        {
            foreach (var child in children)
            {
                var childVm = CreateOutlineItemViewModel(child, itemsByParent);
                vm.Children.Add(childVm);
            }
        }

        return vm;
    }

    [RelayCommand]
    private void AddItem(OutlineItemTypeEnum type)
    {
        if (Project?.Outline == null) return;

        var newItem = new OutlineItem
        {
            Id = Guid.NewGuid(),
            Type = type,
            Title = GetDefaultTitle(type),
            Order = GetNextOrder(SelectedItem?.Item.Id),
            ParentId = SelectedItem?.Item.Id,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        Project.Outline.Items.Add(newItem);
        Project.Outline.MarkAsModified();
        _projectService.MarkAsModified();

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
        if (Project?.Outline == null) return 0;

        var siblings = Project.Outline.Items
            .Where(i => i.ParentId == parentId)
            .ToList();

        return siblings.Count > 0 ? siblings.Max(i => i.Order) + 1 : 0;
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
        Project.Outline.Items.RemoveAll(i => idsToRemove.Contains(i.Id));
        Project.Outline.MarkAsModified();
        _projectService.MarkAsModified();

        RefreshOutlineTree();
        SelectedItem = null;
        IsModified = true;

        _logger.LogInformation("Deleted outline item: {Title}", item.Title);
    }

    private IEnumerable<Guid> GetAllDescendantIds(Guid parentId)
    {
        if (Project?.Outline == null) yield break;

        var children = Project.Outline.Items.Where(i => i.ParentId == parentId);
        foreach (var child in children)
        {
            yield return child.Id;
            foreach (var descendant in GetAllDescendantIds(child.Id))
            {
                yield return descendant;
            }
        }
    }

    [RelayCommand]
    private void MoveUp(OutlineItemViewModel? item)
    {
        if (item == null || Project?.Outline == null) return;

        var siblings = Project.Outline.Items
            .Where(i => i.ParentId == item.Item.ParentId)
            .OrderBy(i => i.Order)
            .ToList();

        var index = siblings.IndexOf(item.Item);
        if (index <= 0) return;

        // Swap orders
        var prevItem = siblings[index - 1];
        (item.Item.Order, prevItem.Order) = (prevItem.Order, item.Item.Order);

        Project.Outline.MarkAsModified();
        _projectService.MarkAsModified();
        RefreshOutlineTree();
        SelectItem(item.Item.Id);
        IsModified = true;
    }

    [RelayCommand]
    private void MoveDown(OutlineItemViewModel? item)
    {
        if (item == null || Project?.Outline == null) return;

        var siblings = Project.Outline.Items
            .Where(i => i.ParentId == item.Item.ParentId)
            .OrderBy(i => i.Order)
            .ToList();

        var index = siblings.IndexOf(item.Item);
        if (index < 0 || index >= siblings.Count - 1) return;

        // Swap orders
        var nextItem = siblings[index + 1];
        (item.Item.Order, nextItem.Order) = (nextItem.Order, item.Item.Order);

        Project.Outline.MarkAsModified();
        _projectService.MarkAsModified();
        RefreshOutlineTree();
        SelectItem(item.Item.Id);
        IsModified = true;
    }

    [RelayCommand]
    private void Indent(OutlineItemViewModel? item)
    {
        if (item == null || Project?.Outline == null) return;

        // Find previous sibling to become new parent
        var siblings = Project.Outline.Items
            .Where(i => i.ParentId == item.Item.ParentId)
            .OrderBy(i => i.Order)
            .ToList();

        var index = siblings.IndexOf(item.Item);
        if (index <= 0) return;

        var newParent = siblings[index - 1];
        item.Item.ParentId = newParent.Id;
        item.Item.Order = GetNextOrder(newParent.Id);

        Project.Outline.MarkAsModified();
        _projectService.MarkAsModified();
        RefreshOutlineTree();
        SelectItem(item.Item.Id);
        IsModified = true;
    }

    [RelayCommand]
    private void Outdent(OutlineItemViewModel? item)
    {
        if (item?.Item.ParentId == null || Project?.Outline == null) return;

        // Move to parent's level
        var parent = Project.Outline.Items.FirstOrDefault(i => i.Id == item.Item.ParentId);
        if (parent == null) return;

        item.Item.ParentId = parent.ParentId;
        item.Item.Order = parent.Order + 1;

        // Shift siblings down
        var siblings = Project.Outline.Items
            .Where(i => i.ParentId == parent.ParentId && i.Order >= item.Item.Order && i.Id != item.Item.Id)
            .ToList();
        foreach (var sibling in siblings)
        {
            sibling.Order++;
        }

        Project.Outline.MarkAsModified();
        _projectService.MarkAsModified();
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
                Id = Guid.NewGuid(),
                Type = (Core.Models.OutlineItemType)OutlineItemTypeEnum.Act,
                Title = actTitle,
                Order = order++,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            };
            Project.Outline.Items.Add(act);

            var chapterOrder = 0;
            foreach (var chapterTitle in chapters)
            {
                var chapter = new OutlineItem
                {
                    Id = Guid.NewGuid(),
                    Type = (Core.Models.OutlineItemType)OutlineItemTypeEnum.Chapter,
                    Title = chapterTitle,
                    ParentId = act.Id,
                    Order = chapterOrder++,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                };
                Project.Outline.Items.Add(chapter);
            }
        }

        Project.Outline.MarkAsModified();
        _projectService.MarkAsModified();
        RefreshOutlineTree();
        IsModified = true;
    }

    [RelayCommand]
    private async Task CreateChapterFromItemAsync(OutlineItemViewModel? item)
    {
        if (item?.Item.Type != (Core.Models.OutlineItemType)OutlineItemTypeEnum.Chapter || Project == null) return;

        // Check if chapter already exists
        if (item.Item.LinkedChapterId.HasValue)
        {
            StatusMessage = "Chapter already exists for this outline item";
            return;
        }

        // Create new chapter
        var chapter = new Chapter
        {
            Id = Guid.NewGuid(),
            Title = item.Item.Title,
            Order = Project.Chapters.Count + 1,
            Status = ChapterStatus.Outlined,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        Project.Chapters.Add(chapter);
        item.Item.LinkedChapterId = chapter.Id;

        _projectService.MarkAsModified();
        await _projectService.SaveProjectAsync();

        StatusMessage = $"Created chapter: {chapter.Title}";
        _logger.LogInformation("Created chapter from outline: {Title}", chapter.Title);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var result = await _projectService.SaveProjectAsync();

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
                Item.MarkAsModified();
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string? Description
    {
        get => Item.Description;
        set
        {
            if (Item.Description != value)
            {
                Item.Description = value;
                Item.MarkAsModified();
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets the item type.
    /// </summary>
    public OutlineItemTypeEnum Type => (OutlineItemTypeEnum)Item.Type;

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
    public string Icon => Item.Type switch
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
