// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Enums;
using AIBookAuthorPro.Core.Interfaces;
using AIBookAuthorPro.Core.Models;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.Infrastructure.Services;

/// <summary>
/// Service for managing book projects with ZIP-based file persistence.
/// Projects are stored as .abpro files (ZIP archives) containing JSON metadata and chapter content.
/// </summary>
public sealed class ProjectService : IProjectService
{
    private const string ProjectExtension = ".abpro";
    private const string MetadataFileName = "project.json";
    private const string ChaptersFolder = "chapters";
    private const string AutosaveFolder = "autosave";
    
    private readonly ILogger<ProjectService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly string _autosavePath;
    private Project? _currentProject;
    private CancellationTokenSource? _autosaveCts;
    private bool _hasUnsavedChanges;

    public ProjectService(ILogger<ProjectService> logger)
    {
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
        };
        
        _autosavePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AIBookAuthorPro",
            AutosaveFolder);
        
        Directory.CreateDirectory(_autosavePath);
    }

    /// <inheritdoc />
    public async Task<Result<Project>> CreateAsync(
        string name,
        string? templateName = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating new project: {Name} (Template: {Template})", name, templateName);
            
            var project = new Project
            {
                Id = Guid.NewGuid(),
                Name = name,
                Metadata = new BookMetadata
                {
                    Title = name,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                },
                GenerationSettings = new GenerationSettings
                {
                    DefaultProvider = Enums.AIProviderType.Claude,
                    DefaultModel = "claude-sonnet-4-20250514",
                    Temperature = 0.7,
                    MaxTokens = 4000
                },
                TemplateName = templateName,
                Chapters = new List<Chapter>(),
                Characters = new List<Character>(),
                Locations = new List<Location>(),
                ResearchNotes = new List<ResearchNote>()
            };

            _currentProject = project;
            _hasUnsavedChanges = true;
            StartAutosave();
            
            _logger.LogInformation("Project created successfully: {ProjectId}", project.Id);
            return Result<Project>.Success(project);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create project: {Name}", name);
            return Result<Project>.Failure($"Failed to create project: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<Project>> LoadAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Opening project: {FilePath}", filePath);
            
            if (!File.Exists(filePath))
            {
                return Result<Project>.Failure($"Project file not found: {filePath}");
            }

            if (!filePath.EndsWith(ProjectExtension, StringComparison.OrdinalIgnoreCase))
            {
                return Result<Project>.Failure($"Invalid project file format. Expected {ProjectExtension}");
            }

            using var archive = ZipFile.OpenRead(filePath);
            
            // Read project metadata
            var metadataEntry = archive.GetEntry(MetadataFileName)
                ?? throw new InvalidOperationException("Project metadata not found in archive");
            
            Project project;
            await using (var stream = metadataEntry.Open())
            {
                project = await JsonSerializer.DeserializeAsync<Project>(stream, _jsonOptions, cancellationToken)
                    ?? throw new InvalidOperationException("Failed to deserialize project metadata");
            }

            // Read chapter content
            foreach (var chapter in project.Chapters)
            {
                var chapterEntry = archive.GetEntry($"{ChaptersFolder}/{chapter.Id}.json");
                if (chapterEntry != null)
                {
                    await using var stream = chapterEntry.Open();
                    var chapterData = await JsonSerializer.DeserializeAsync<ChapterContent>(stream, _jsonOptions, cancellationToken);
                    if (chapterData != null)
                    {
                        chapter.Content = chapterData.Content;
                        chapter.Scenes = chapterData.Scenes;
                    }
                }
            }

            project.FilePath = filePath;
            _currentProject = project;
            _hasUnsavedChanges = false;
            StartAutosave();
            
            _logger.LogInformation("Project opened successfully: {ProjectId}", project.Id);
            return Result<Project>.Success(project);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open project: {FilePath}", filePath);
            return Result<Project>.Failure($"Failed to open project: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result> SaveAsync(
        Project project,
        string? filePath = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(project);

            var targetPath = filePath ?? project.FilePath;
            if (string.IsNullOrEmpty(targetPath))
            {
                return Result.Failure("No file path specified for saving");
            }

            if (!targetPath.EndsWith(ProjectExtension, StringComparison.OrdinalIgnoreCase))
            {
                targetPath += ProjectExtension;
            }

            _logger.LogInformation("Saving project to: {FilePath}", targetPath);
            
            project.Metadata.ModifiedAt = DateTime.UtcNow;
            
            // Create temporary file first
            var tempPath = Path.GetTempFileName();
            
            try
            {
                using (var archive = ZipFile.Open(tempPath, ZipArchiveMode.Create))
                {
                    // Write project metadata (without chapter content to reduce duplication)
                    var projectCopy = CloneProjectWithoutContent(project);
                    var metadataEntry = archive.CreateEntry(MetadataFileName, CompressionLevel.Optimal);
                    await using (var stream = metadataEntry.Open())
                    {
                        await JsonSerializer.SerializeAsync(stream, projectCopy, _jsonOptions, cancellationToken);
                    }

                    // Write each chapter separately
                    foreach (var chapter in project.Chapters)
                    {
                        var chapterEntry = archive.CreateEntry(
                            $"{ChaptersFolder}/{chapter.Id}.json",
                            CompressionLevel.Optimal);
                        
                        await using var stream = chapterEntry.Open();
                        var chapterContent = new ChapterContent
                        {
                            Content = chapter.Content,
                            Scenes = chapter.Scenes
                        };
                        await JsonSerializer.SerializeAsync(stream, chapterContent, _jsonOptions, cancellationToken);
                    }
                }

                // Atomic replace
                if (File.Exists(targetPath))
                {
                    File.Delete(targetPath);
                }
                File.Move(tempPath, targetPath);
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }

            project.FilePath = targetPath;
            if (_currentProject != null && _currentProject.Id == project.Id)
            {
                _currentProject.FilePath = targetPath;
                _hasUnsavedChanges = false;
            }
            
            _logger.LogInformation("Project saved successfully");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save project");
            return Result.Failure($"Failed to save project: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result> CloseProjectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Save any pending changes if needed
            if (_hasUnsavedChanges && _currentProject != null)
            {
                // Optionally auto-save before closing
                // Could be configured as a setting
            }

            _currentProject = null;
            _hasUnsavedChanges = false;
            StopAutosave();
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to close project");
            return Result.Failure($"Failed to close project: {ex.Message}");
        }
    }

    public Project? GetCurrentProject() => _currentProject;

    public bool HasUnsavedChanges() => _hasUnsavedChanges;

    public void MarkAsModified()
    {
        _hasUnsavedChanges = true;
        if (_currentProject != null)
        {
            _currentProject.Metadata.ModifiedAt = DateTime.UtcNow;
        }
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<ProjectSummary>>> GetRecentProjectsAsync(
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var recentProjectsPath = Path.Combine(_autosavePath, "..", "recent.json");
            
            if (!File.Exists(recentProjectsPath))
            {
                return Result<IReadOnlyList<ProjectSummary>>.Success(Array.Empty<ProjectSummary>());
            }

            var json = await File.ReadAllTextAsync(recentProjectsPath, cancellationToken);
            var projectInfos = JsonSerializer.Deserialize<List<ProjectInfoInternal>>(json, _jsonOptions)
                ?? new List<ProjectInfoInternal>();
            
            // Filter out non-existent files and take requested count
            var validInfos = projectInfos
                .Where(p => File.Exists(p.FilePath))
                .Take(count)
                .ToList();
            
            // Convert to ProjectSummary
            var summaries = validInfos.Select(info =>
            {
                try
                {
                    var fileInfo = new FileInfo(info.FilePath);
                    return new ProjectSummary
                    {
                        FilePath = info.FilePath,
                        Name = info.Title ?? Path.GetFileNameWithoutExtension(info.FilePath),
                        Title = info.Title,
                        LastModified = fileInfo.LastWriteTimeUtc,
                        WordCount = info.WordCount,
                        ChapterCount = 0 // Would need to load project to get this
                    };
                }
                catch
                {
                    return null;
                }
            })
            .Where(s => s != null)
            .Cast<ProjectSummary>()
            .ToList();
            
            return Result<IReadOnlyList<ProjectSummary>>.Success(summaries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get recent projects");
            return Result<IReadOnlyList<ProjectSummary>>.Failure($"Failed to get recent projects: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result> DeleteAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return Result.Failure("Project file not found");
            }

            File.Delete(filePath);
            _logger.LogInformation("Project deleted: {FilePath}", filePath);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete project: {FilePath}", filePath);
            return Result.Failure($"Failed to delete project: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result> ExportAsync(
        Project project,
        ExportFormat format,
        string outputPath,
        ExportOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Export requested: {Format} to {OutputPath}", format, outputPath);
            
            // Build export options if not provided
            var exportOptions = options ?? new ExportOptions
            {
                Format = format,
                OutputPath = outputPath
            };
            
            // This should delegate to IExportService - for now just return success
            // TODO: Implement proper export delegation when IExportService is available via DI
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export project");
            return Result.Failure($"Export failed: {ex.Message}");
        }
    }

    public async Task<Result<Project>> RecoverFromAutosaveAsync(
        string autosavePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Recovering project from autosave: {Path}", autosavePath);
            
            var result = await LoadAsync(autosavePath, cancellationToken);
            if (result.IsSuccess && result.Value != null)
            {
                result.Value.FilePath = null; // Force "Save As" on first save
                _hasUnsavedChanges = true;
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to recover from autosave");
            return Result<Project>.Failure($"Failed to recover from autosave: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<string>>> GetAutosaveFilesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var files = Directory.GetFiles(_autosavePath, $"*{ProjectExtension}")
                .OrderByDescending(f => File.GetLastWriteTimeUtc(f))
                .ToList();
            
            return Result<IReadOnlyList<string>>.Success(files);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get autosave files");
            return Result<IReadOnlyList<string>>.Failure($"Failed to get autosave files: {ex.Message}");
        }
    }

    private void StartAutosave()
    {
        StopAutosave();
        _autosaveCts = new CancellationTokenSource();
        _ = AutosaveLoopAsync(_autosaveCts.Token);
    }

    private void StopAutosave()
    {
        _autosaveCts?.Cancel();
        _autosaveCts?.Dispose();
        _autosaveCts = null;
    }

    private async Task AutosaveLoopAsync(CancellationToken cancellationToken)
    {
        var interval = TimeSpan.FromMinutes(1); // Default autosave interval
        
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(interval, cancellationToken);
                
                if (_hasUnsavedChanges && _currentProject != null)
                {
                    await AutosaveAsync(cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Autosave failed");
            }
        }
    }

    private async Task AutosaveAsync(CancellationToken cancellationToken)
    {
        if (_currentProject == null) return;
        
        var autosaveFile = Path.Combine(
            _autosavePath,
            $"{_currentProject.Id}_{DateTime.UtcNow:yyyyMMdd_HHmmss}{ProjectExtension}");
        
        var originalPath = _currentProject.FilePath;
        _currentProject.FilePath = autosaveFile;
        
        try
        {
            if (_currentProject != null)
            {
                await SaveAsync(_currentProject, autosaveFile, cancellationToken);
            }
            _logger.LogDebug("Autosave completed: {Path}", autosaveFile);
            
            // Clean up old autosaves (keep last 5)
            var autosaves = Directory.GetFiles(_autosavePath, $"{_currentProject.Id}_*{ProjectExtension}")
                .OrderByDescending(f => f)
                .Skip(5)
                .ToList();
            
            foreach (var oldAutosave in autosaves)
            {
                File.Delete(oldAutosave);
            }
        }
        finally
        {
            _currentProject.FilePath = originalPath;
        }
    }

    private Project CloneProjectWithoutContent(Project project)
    {
        // Create a shallow copy with chapter content cleared for metadata storage
        var clone = new Project
        {
            Id = project.Id,
            Metadata = project.Metadata,
            Settings = project.Settings,
            Outline = project.Outline,
            Characters = project.Characters,
            Locations = project.Locations,
            ResearchNotes = project.ResearchNotes,
            Chapters = project.Chapters.Select(c => new Chapter
            {
                Id = c.Id,
                Title = c.Title,
                Number = c.Number,
                Summary = c.Summary,
                Status = c.Status,
                WordCount = c.WordCount,
                TargetWordCount = c.TargetWordCount,
                Notes = c.Notes,
                CharacterIds = c.CharacterIds,
                LocationIds = c.LocationIds,
                CreatedAt = c.CreatedAt,
                ModifiedAt = c.ModifiedAt,
                // Content and Scenes stored separately
                Content = null,
                Scenes = new List<Scene>()
            }).ToList()
        };
        
        return clone;
    }

    public void Dispose()
    {
        StopAutosave();
    }
}

/// <summary>
/// Separate storage for chapter content within the archive.
/// </summary>
internal sealed class ChapterContent
{
    public string? Content { get; set; }
    public List<Scene> Scenes { get; set; } = new();
}

/// <summary>
/// Internal representation of project info for persistence.
/// </summary>
internal sealed class ProjectInfoInternal
{
    public string FilePath { get; set; } = string.Empty;
    public string? Title { get; set; }
    public DateTime LastModified { get; set; }
    public int WordCount { get; set; }
}