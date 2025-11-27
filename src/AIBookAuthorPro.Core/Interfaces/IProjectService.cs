// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Models;

namespace AIBookAuthorPro.Core.Interfaces;

/// <summary>
/// Defines the contract for project management operations.
/// </summary>
public interface IProjectService
{
    /// <summary>
    /// Creates a new project.
    /// </summary>
    System.Threading.Tasks.Task<Result<Project>> CreateAsync(
        string name,
        string? templateName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a project from file.
    /// </summary>
    System.Threading.Tasks.Task<Result<Project>> LoadAsync(
        string filePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a project to file.
    /// </summary>
    System.Threading.Tasks.Task<Result> SaveAsync(
        Project project,
        string? filePath = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent projects.
    /// </summary>
    System.Threading.Tasks.Task<Result<IReadOnlyList<ProjectSummary>>> GetRecentProjectsAsync(
        int count = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a project.
    /// </summary>
    System.Threading.Tasks.Task<Result> DeleteAsync(
        string filePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports a project to a different format.
    /// </summary>
    System.Threading.Tasks.Task<Result> ExportAsync(
        Project project,
        Enums.ExportFormat format,
        string outputPath,
        ExportOptions? options = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Summary information about a project for display in lists.
/// </summary>
public sealed class ProjectSummary
{
    public string FilePath { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Author { get; set; }
    public DateTime LastModified { get; set; }
    public int WordCount { get; set; }
    public int ChapterCount { get; set; }
    public double CompletionPercentage { get; set; }
}
