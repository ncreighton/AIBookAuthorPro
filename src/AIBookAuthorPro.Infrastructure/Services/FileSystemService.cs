// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.Infrastructure.Services;

/// <summary>
/// Implementation of file system operations service.
/// </summary>
public sealed class FileSystemService : IFileSystemService
{
    private readonly ILogger<FileSystemService> _logger;

    public FileSystemService(ILogger<FileSystemService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<bool> FileExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(File.Exists(path));
    }

    /// <inheritdoc />
    public Task<bool> DirectoryExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Directory.Exists(path));
    }

    /// <inheritdoc />
    public Task EnsureDirectoryExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            _logger.LogDebug("Created directory: {Path}", path);
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default)
    {
        return await File.ReadAllTextAsync(path, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default)
    {
        return await File.ReadAllBytesAsync(path, cancellationToken);
    }

    /// <inheritdoc />
    public async Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            await EnsureDirectoryExistsAsync(directory, cancellationToken);
        }
        
        await File.WriteAllTextAsync(path, content, cancellationToken);
        _logger.LogDebug("Wrote text file: {Path}", path);
    }

    /// <inheritdoc />
    public async Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            await EnsureDirectoryExistsAsync(directory, cancellationToken);
        }
        
        await File.WriteAllBytesAsync(path, bytes, cancellationToken);
        _logger.LogDebug("Wrote binary file: {Path}", path);
    }

    /// <inheritdoc />
    public Task DeleteFileAsync(string path, CancellationToken cancellationToken = default)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
            _logger.LogDebug("Deleted file: {Path}", path);
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteDirectoryAsync(string path, bool recursive = false, CancellationToken cancellationToken = default)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive);
            _logger.LogDebug("Deleted directory: {Path}", path);
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task CopyFileAsync(string sourcePath, string destPath, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(destPath);
        if (!string.IsNullOrEmpty(directory))
        {
            await EnsureDirectoryExistsAsync(directory, cancellationToken);
        }
        
        File.Copy(sourcePath, destPath, overwrite: true);
        _logger.LogDebug("Copied file from {Source} to {Dest}", sourcePath, destPath);
    }

    /// <inheritdoc />
    public Task MoveFileAsync(string sourcePath, string destPath, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(destPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        File.Move(sourcePath, destPath, overwrite: true);
        _logger.LogDebug("Moved file from {Source} to {Dest}", sourcePath, destPath);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<string[]> GetFilesAsync(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly, CancellationToken cancellationToken = default)
    {
        var files = Directory.Exists(path)
            ? Directory.GetFiles(path, searchPattern, searchOption)
            : Array.Empty<string>();
        return Task.FromResult(files);
    }

    /// <inheritdoc />
    public Task<string[]> GetDirectoriesAsync(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly, CancellationToken cancellationToken = default)
    {
        var directories = Directory.Exists(path)
            ? Directory.GetDirectories(path, searchPattern, searchOption)
            : Array.Empty<string>();
        return Task.FromResult(directories);
    }

    /// <inheritdoc />
    public Task<FileInfo> GetFileInfoAsync(string path, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new FileInfo(path));
    }

    /// <inheritdoc />
    public Task<Stream> OpenReadAsync(string path, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Stream>(File.OpenRead(path));
    }

    /// <inheritdoc />
    public Task<Stream> OpenWriteAsync(string path, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        return Task.FromResult<Stream>(File.OpenWrite(path));
    }

    /// <inheritdoc />
    public string GetTempPath()
    {
        return Path.GetTempPath();
    }

    /// <inheritdoc />
    public string GetTempFileName()
    {
        return Path.GetTempFileName();
    }

    /// <inheritdoc />
    public string CombinePaths(params string[] paths)
    {
        return Path.Combine(paths);
    }

    /// <inheritdoc />
    public string GetAppDataPath()
    {
        var path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AIBookAuthorPro");
        
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        
        return path;
    }

    /// <inheritdoc />
    public string GetProjectsPath()
    {
        var path = Path.Combine(GetAppDataPath(), "Projects");
        
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        
        return path;
    }

    /// <inheritdoc />
    public string GetCachePath()
    {
        var path = Path.Combine(GetAppDataPath(), "Cache");
        
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        
        return path;
    }
}
