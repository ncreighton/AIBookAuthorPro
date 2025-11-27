// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Common;

namespace AIBookAuthorPro.Core.Interfaces;

/// &lt;summary&gt;
/// Service for centralized error handling.
/// &lt;/summary&gt;
public interface IErrorHandler
{
    /// &lt;summary&gt;
    /// Handles an exception with logging and optional user notification.
    /// &lt;/summary&gt;
    void Handle(Exception exception, string? context = null, bool showNotification = true);

    /// &lt;summary&gt;
    /// Handles an exception and returns a Result with the error.
    /// &lt;/summary&gt;
    Result&lt;T&gt; HandleWithResult&lt;T&gt;(Exception exception, string? context = null, bool showNotification = true);

    /// &lt;summary&gt;
    /// Wraps an action with error handling.
    /// &lt;/summary&gt;
    Result TryExecute(Action action, string? context = null);

    /// &lt;summary&gt;
    /// Wraps a function with error handling.
    /// &lt;/summary&gt;
    Result&lt;T&gt; TryExecute&lt;T&gt;(Func&lt;T&gt; func, string? context = null);

    /// &lt;summary&gt;
    /// Wraps an async action with error handling.
    /// &lt;/summary&gt;
    Task&lt;Result&gt; TryExecuteAsync(Func&lt;Task&gt; action, string? context = null, CancellationToken ct = default);

    /// &lt;summary&gt;
    /// Wraps an async function with error handling.
    /// &lt;/summary&gt;
    Task&lt;Result&lt;T&gt;&gt; TryExecuteAsync&lt;T&gt;(Func&lt;Task&lt;T&gt;&gt; func, string? context = null, CancellationToken ct = default);
}
