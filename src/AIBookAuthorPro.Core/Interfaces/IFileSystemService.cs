// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.IO;

namespace AIBookAuthorPro.Core.Interfaces;

/// <summary>
/// Service for file system operations with async support.
/// </summary>
public interface IFileSystemService
{
    /// <summary>
    /// Checks if a file exists at the specified path.
    /// </summary>
    bool FileExists(string path);

    /// <summary>
    /// Checks if a directory exists at the specified path.
    /// </summary>
    bool DirectoryExists(string path);

    /// <summary>
    /// Creates a directory at the specified path.
    /// </summary>
    void CreateDirectory(string path);

    /// <summary>
    /// Reads all text from a file asynchronously.
    /// </summary>
    Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes all text to a file asynchronously.
    /// </summary>
    Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads all bytes from a file asynchronously.
    /// </summary>
    Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes all bytes to a file asynchronously.
    /// </summary>
    Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file at the specified path.
    /// </summary>
    void DeleteFile(string path);

    /// <summary>
    /// Deletes a directory at the specified path.
    /// </summary>
    void DeleteDirectory(string path, bool recursive = false);

    /// <summary>
    /// Copies a file from source to destination.
    /// </summary>
    void CopyFile(string sourcePath, string destinationPath, bool overwrite = false);

    /// <summary>
    /// Moves a file from source to destination.
    /// </summary>
    void MoveFile(string sourcePath, string destinationPath);

    /// <summary>
    /// Gets all files in a directory matching a pattern.
    /// </summary>
    IEnumerable<string> GetFiles(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly);

    /// <summary>
    /// Gets all directories in a directory.
    /// </summary>
    IEnumerable<string> GetDirectories(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly);

    /// <summary>
    /// Gets file information.
    /// </summary>
    FileInfo GetFileInfo(string path);

    /// <summary>
    /// Gets the application data folder path.
    /// </summary>
    string GetAppDataPath();

    /// <summary>
    /// Gets the user documents folder path.
    /// </summary>
    string GetDocumentsPath();

    /// <summary>
    /// Combines path segments.
    /// </summary>
    string CombinePath(params string[] paths);

    /// <summary>
    /// Gets the directory name from a path.
    /// </summary>
    string? GetDirectoryName(string path);

    /// <summary>
    /// Gets the file name from a path.
    /// </summary>
    string GetFileName(string path);

    /// <summary>
    /// Gets the file name without extension from a path.
    /// </summary>
    string GetFileNameWithoutExtension(string path);

    /// <summary>
    /// Gets the extension from a path.
    /// </summary>
    string GetExtension(string path);

    /// <summary>
    /// Creates a temporary file and returns its path.
    /// </summary>
    string CreateTempFile();

    /// <summary>
    /// Gets the temp folder path.
    /// </summary>
    string GetTempPath();
}
