// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

namespace AIBookAuthorPro.Core.Interfaces;

/// <summary>
/// Service for centralized error handling.
/// </summary>
public interface IErrorHandler
{
    /// <summary>
    /// Handles an exception with logging and optional user notification.
    /// </summary>
    void Handle(Exception exception, string? context = null, bool showNotification = true);

    /// <summary>
    /// Handles an exception and returns a Result with the error.
    /// </summary>
    Result<T> HandleWithResult<T>(Exception exception, string? context = null, bool showNotification = true);

    /// <summary>
    /// Wraps an action with error handling.
    /// </summary>
    Result TryExecute(Action action, string? context = null);

    /// <summary>
    /// Wraps a function with error handling.
    /// </summary>
    Result<T> TryExecute<T>(Func<T> func, string? context = null);

    /// <summary>
    /// Wraps an async action with error handling.
    /// </summary>
    Task<Result> TryExecuteAsync(Func<Task> action, string? context = null, CancellationToken ct = default);

    /// <summary>
    /// Wraps an async function with error handling.
    /// </summary>
    Task<Result<T>> TryExecuteAsync<T>(Func<Task<T>> func, string? context = null, CancellationToken ct = default);
}
