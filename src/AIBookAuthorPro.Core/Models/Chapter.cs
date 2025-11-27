// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Enums;

namespace AIBookAuthorPro.Core.Models;

/// <summary>
/// Represents a chapter in a book project.
/// </summary>
public sealed class Chapter : Entity
{
    private readonly List<Scene> _scenes = [];

    /// <summary>
    /// Gets or sets the chapter title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the chapter number/order (1-based).
    /// </summary>
    public int Order { get; set; } = 1;

    /// <summary>
    /// Gets or sets the chapter content in rich text format.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the chapter summary/synopsis.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Gets or sets the chapter outline/beats.
    /// </summary>
    public string? Outline { get; set; }

    /// <summary>
    /// Gets or sets the chapter status.
    /// </summary>
    public ChapterStatus Status { get; set; } = ChapterStatus.NotStarted;

    /// <summary>
    /// Gets or sets notes for this chapter.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets the collection of scenes in this chapter.
    /// </summary>
    public IReadOnlyList<Scene> Scenes => _scenes.AsReadOnly();

    /// <summary>
    /// Gets or sets the point of view character for this chapter.
    /// </summary>
    public Guid? PovCharacterId { get; set; }

    /// <summary>
    /// Gets or sets the primary location for this chapter.
    /// </summary>
    public Guid? PrimaryLocationId { get; set; }

    /// <summary>
    /// Gets the word count for this chapter.
    /// </summary>
    public int WordCount => CountWords(Content);

    /// <summary>
    /// Gets or sets the target word count for this chapter.
    /// </summary>
    public int TargetWordCount { get; set; } = 3000;

    /// <summary>
    /// Gets the completion percentage for this chapter.
    /// </summary>
    public double CompletionPercentage => 
        TargetWordCount > 0 ? Math.Min(100, (double)WordCount / TargetWordCount * 100) : 0;

    /// <summary>
    /// Gets or sets custom generation context for this specific chapter.
    /// </summary>
    public string? CustomContext { get; set; }

    /// <summary>
    /// Gets or sets the list of character IDs appearing in this chapter.
    /// </summary>
    public List<Guid> CharacterIds { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of location IDs used in this chapter.
    /// </summary>
    public List<Guid> LocationIds { get; set; } = [];

    /// <summary>
    /// Adds a scene to this chapter.
    /// </summary>
    /// <param name="scene">The scene to add.</param>
    public void AddScene(Scene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);
        scene.Order = _scenes.Count + 1;
        _scenes.Add(scene);
        MarkAsModified();
    }

    /// <summary>
    /// Removes a scene from this chapter.
    /// </summary>
    /// <param name="sceneId">The ID of the scene to remove.</param>
    /// <returns>True if the scene was removed; otherwise, false.</returns>
    public bool RemoveScene(Guid sceneId)
    {
        var scene = _scenes.FirstOrDefault(s => s.Id == sceneId);
        if (scene is null) return false;
        
        _scenes.Remove(scene);
        ReorderScenes();
        MarkAsModified();
        return true;
    }

    /// <summary>
    /// Updates the chapter content.
    /// </summary>
    /// <param name="content">The new content.</param>
    public void UpdateContent(string content)
    {
        Content = content ?? string.Empty;
        MarkAsModified();
    }

    private void ReorderScenes()
    {
        for (int i = 0; i < _scenes.Count; i++)
        {
            _scenes[i].Order = i + 1;
        }
    }

    private static int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        
        // Simple word count - split on whitespace
        return text.Split(
            [' ', '\t', '\n', '\r'], 
            StringSplitOptions.RemoveEmptyEntries).Length;
    }
}

/// <summary>
/// Represents a scene within a chapter.
/// </summary>
public sealed class Scene : Entity
{
    /// <summary>
    /// Gets or sets the scene title/heading.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the scene order within the chapter.
    /// </summary>
    public int Order { get; set; } = 1;

    /// <summary>
    /// Gets or sets the scene content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the scene summary/purpose.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Gets or sets the scene type (e.g., "action", "dialogue", "description").
    /// </summary>
    public string? SceneType { get; set; }

    /// <summary>
    /// Gets or sets the primary location for this scene.
    /// </summary>
    public Guid? LocationId { get; set; }

    /// <summary>
    /// Gets or sets the POV character for this scene.
    /// </summary>
    public Guid? PovCharacterId { get; set; }

    /// <summary>
    /// Gets or sets the characters present in this scene.
    /// </summary>
    public List<Guid> CharacterIds { get; set; } = [];

    /// <summary>
    /// Gets or sets notes for this scene.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets the word count for this scene.
    /// </summary>
    public int WordCount => string.IsNullOrWhiteSpace(Content) 
        ? 0 
        : Content.Split([' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries).Length;
}
