// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.IO;

namespace AIBookAuthorPro.Core.Interfaces;

/// <summary>
/// Interface for file system operations.
/// </summary>
public interface IFileSystemService
{
    /// <summary>
    /// Checks if a file exists.
    /// </summary>
    Task<bool> FileExistsAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a directory exists.
    /// </summary>
    Task<bool> DirectoryExistsAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensures a directory exists, creating it if necessary.
    /// </summary>
    Task EnsureDirectoryExistsAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads all text from a file.
    /// </summary>
    Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads all bytes from a file.
    /// </summary>
    Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes text to a file.
    /// </summary>
    Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes bytes to a file.
    /// </summary>
    Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file.
    /// </summary>
    Task DeleteFileAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a directory.
    /// </summary>
    Task DeleteDirectoryAsync(string path, bool recursive = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Copies a file.
    /// </summary>
    Task CopyFileAsync(string sourcePath, string destPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Moves a file.
    /// </summary>
    Task MoveFileAsync(string sourcePath, string destPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets files in a directory.
    /// </summary>
    Task<string[]> GetFilesAsync(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets directories in a path.
    /// </summary>
    Task<string[]> GetDirectoriesAsync(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets file info.
    /// </summary>
    Task<FileInfo> GetFileInfoAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens a file for reading.
    /// </summary>
    Task<Stream> OpenReadAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens a file for writing.
    /// </summary>
    Task<Stream> OpenWriteAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the temp path.
    /// </summary>
    string GetTempPath();

    /// <summary>
    /// Gets a temp file name.
    /// </summary>
    string GetTempFileName();

    /// <summary>
    /// Combines paths.
    /// </summary>
    string CombinePaths(params string[] paths);

    /// <summary>
    /// Gets the application data path.
    /// </summary>
    string GetAppDataPath();

    /// <summary>
    /// Gets the projects path.
    /// </summary>
    string GetProjectsPath();

    /// <summary>
    /// Gets the cache path.
    /// </summary>
    string GetCachePath();
}
