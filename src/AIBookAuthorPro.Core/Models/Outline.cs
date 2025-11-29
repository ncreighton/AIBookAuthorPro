// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Enums;

namespace AIBookAuthorPro.Core.Models;

/// <summary>
/// Represents the book outline with hierarchical structure.
/// </summary>
public sealed class Outline : Entity
{
    private readonly List<OutlineItem> _items = [];

    /// <summary>
    /// Gets or sets the outline title.
    /// </summary>
    public string Title { get; set; } = "Book Outline";

    /// <summary>
    /// Gets or sets the beat sheet type (Three Act, Hero's Journey, etc.).
    /// </summary>
    public string? BeatSheetType { get; set; }

    /// <summary>
    /// Gets or sets the overall story synopsis.
    /// </summary>
    public string? Synopsis { get; set; }

    /// <summary>
    /// Gets or sets the main theme of the book.
    /// </summary>
    public string? Theme { get; set; }

    /// <summary>
    /// Gets or sets the central conflict.
    /// </summary>
    public string? CentralConflict { get; set; }

    /// <summary>
    /// Gets the outline items (acts, chapters, scenes).
    /// </summary>
    public IReadOnlyList<OutlineItem> Items => _items.AsReadOnly();

    /// <summary>
    /// Adds an outline item.
    /// </summary>
    /// <param name="item">The item to add.</param>
    public void AddItem(OutlineItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        item.Order = _items.Count + 1;
        _items.Add(item);
        MarkAsModified();
    }

    /// <summary>
    /// Removes an outline item.
    /// </summary>
    /// <param name="itemId">The ID of the item to remove.</param>
    /// <returns>True if the item was removed; otherwise, false.</returns>
    public bool RemoveItem(Guid itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item is null) return false;
        
        _items.Remove(item);
        ReorderItems();
        MarkAsModified();
        return true;
    }

    /// <summary>
    /// Gets all items flattened (including nested children).
    /// </summary>
    /// <returns>All outline items in order.</returns>
    public IEnumerable<OutlineItem> GetAllItemsFlattened()
    {
        foreach (var item in _items.OrderBy(i => i.Order))
        {
            yield return item;
            foreach (var child in item.GetAllChildrenFlattened())
            {
                yield return child;
            }
        }
    }

    private void ReorderItems()
    {
        for (int i = 0; i < _items.Count; i++)
        {
            _items[i].Order = i + 1;
        }
    }
}

/// <summary>
/// Represents an item in the outline (act, chapter, scene, beat).
/// </summary>
public sealed class OutlineItem : Entity
{
    private readonly List<OutlineItem> _children = [];

    /// <summary>
    /// Gets or sets the item title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the item type (Act, Chapter, Scene, Beat).
    /// </summary>
    public OutlineItemType ItemType { get; set; } = OutlineItemType.Chapter;

    /// <summary>
    /// Gets or sets the order within the parent.
    /// </summary>
    public int Order { get; set; } = 1;

    /// <summary>
    /// Gets or sets the summary/description.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Gets or sets the purpose of this section.
    /// </summary>
    public string? Purpose { get; set; }

    /// <summary>
    /// Gets or sets key beats/events in this section.
    /// </summary>
    public List<string> Beats { get; set; } = [];

    /// <summary>
    /// Gets or sets notes for this item.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets the estimated word count.
    /// </summary>
    public int? EstimatedWordCount { get; set; }

    /// <summary>
    /// Gets or sets the percentage of the book this represents.
    /// </summary>
    public double? PercentageOfBook { get; set; }

    /// <summary>
    /// Gets or sets whether this item is complete.
    /// </summary>
    public bool IsComplete { get; set; }

    /// <summary>
    /// Gets or sets the linked chapter ID (if this outline item maps to a chapter).
    /// </summary>
    public Guid? LinkedChapterId { get; set; }

    /// <summary>
    /// Gets the child items.
    /// </summary>
    public IReadOnlyList<OutlineItem> Children => _children.AsReadOnly();

    /// <summary>
    /// Adds a child item.
    /// </summary>
    /// <param name="child">The child item to add.</param>
    public void AddChild(OutlineItem child)
    {
        ArgumentNullException.ThrowIfNull(child);
        child.Order = _children.Count + 1;
        _children.Add(child);
    }

    /// <summary>
    /// Removes a child item.
    /// </summary>
    /// <param name="childId">The ID of the child to remove.</param>
    /// <returns>True if removed; otherwise, false.</returns>
    public bool RemoveChild(Guid childId)
    {
        var child = _children.FirstOrDefault(c => c.Id == childId);
        if (child is null) return false;
        
        _children.Remove(child);
        ReorderChildren();
        return true;
    }

    /// <summary>
    /// Gets all children flattened recursively.
    /// </summary>
    /// <returns>All descendant items.</returns>
    public IEnumerable<OutlineItem> GetAllChildrenFlattened()
    {
        foreach (var child in _children.OrderBy(c => c.Order))
        {
            yield return child;
            foreach (var grandchild in child.GetAllChildrenFlattened())
            {
                yield return grandchild;
            }
        }
    }

    private void ReorderChildren()
    {
        for (int i = 0; i < _children.Count; i++)
        {
            _children[i].Order = i + 1;
        }
    }
}
