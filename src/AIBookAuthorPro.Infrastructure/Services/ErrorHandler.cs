// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.Infrastructure.Services;

/// &lt;summary&gt;
/// Centralized error handling service.
/// &lt;/summary&gt;
public sealed class ErrorHandler : IErrorHandler
{
    private readonly ILogger&lt;ErrorHandler&gt; _logger;
    private readonly INotificationService _notificationService;

    /// &lt;summary&gt;
    /// Initializes a new instance of ErrorHandler.
    /// &lt;/summary&gt;
    public ErrorHandler(
        ILogger&lt;ErrorHandler&gt; logger,
        INotificationService notificationService)
    {
        _logger = logger;
        _notificationService = notificationService;
    }

    /// &lt;inheritdoc /&gt;
    public void Handle(Exception exception, string? context = null, bool showNotification = true)
    {
        var message = GetUserFriendlyMessage(exception);
        var logContext = context ?? "Unknown context";

        _logger.LogError(exception, "Error in {Context}: {Message}", logContext, exception.Message);

        if (showNotification)
        {
            _notificationService.ShowError(message, "Error", exception);
        }
    }

    /// &lt;inheritdoc /&gt;
    public Result&lt;T&gt; HandleWithResult&lt;T&gt;(Exception exception, string? context = null, bool showNotification = true)
    {
        Handle(exception, context, showNotification);
        return Result&lt;T&gt;.Failure(GetUserFriendlyMessage(exception), exception);
    }

    /// &lt;inheritdoc /&gt;
    public Result TryExecute(Action action, string? context = null)
    {
        try
        {
            action();
            return Result.Success();
        }
        catch (Exception ex)
        {
            Handle(ex, context);
            return Result.Failure(GetUserFriendlyMessage(ex), ex);
        }
    }

    /// &lt;inheritdoc /&gt;
    public Result&lt;T&gt; TryExecute&lt;T&gt;(Func&lt;T&gt; func, string? context = null)
    {
        try
        {
            var result = func();
            return Result&lt;T&gt;.Success(result);
        }
        catch (Exception ex)
        {
            Handle(ex, context);
            return Result&lt;T&gt;.Failure(GetUserFriendlyMessage(ex), ex);
        }
    }

    /// &lt;inheritdoc /&gt;
    public async Task&lt;Result&gt; TryExecuteAsync(Func&lt;Task&gt; action, string? context = null, CancellationToken ct = default)
    {
        try
        {
            await action();
            return Result.Success();
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            _logger.LogInformation("Operation cancelled: {Context}", context);
            return Result.Failure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            Handle(ex, context);
            return Result.Failure(GetUserFriendlyMessage(ex), ex);
        }
    }

    /// &lt;inheritdoc /&gt;
    public async Task&lt;Result&lt;T&gt;&gt; TryExecuteAsync&lt;T&gt;(Func&lt;Task&lt;T&gt;&gt; func, string? context = null, CancellationToken ct = default)
    {
        try
        {
            var result = await func();
            return Result&lt;T&gt;.Success(result);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            _logger.LogInformation("Operation cancelled: {Context}", context);
            return Result&lt;T&gt;.Failure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            Handle(ex, context);
            return Result&lt;T&gt;.Failure(GetUserFriendlyMessage(ex), ex);
        }
    }

    /// &lt;summary&gt;
    /// Gets a user-friendly message for an exception.
    /// &lt;/summary&gt;
    private static string GetUserFriendlyMessage(Exception exception)
    {
        return exception switch
        {
            // Network errors
            HttpRequestException =&gt; "Unable to connect to the server. Please check your internet connection.",
            TaskCanceledException =&gt; "The operation timed out. Please try again.",
            
            // File errors
            FileNotFoundException =&gt; "The requested file could not be found.",
            DirectoryNotFoundException =&gt; "The specified directory does not exist.",
            UnauthorizedAccessException =&gt; "Access denied. Please check your permissions.",
            IOException io when io.Message.Contains("disk") =&gt; "Disk error occurred. Please check available space.",
            IOException =&gt; "A file operation failed. The file may be in use.",
            
            // Validation errors
            ArgumentException =&gt; exception.Message,
            InvalidOperationException =&gt; exception.Message,
            
            // API errors
            _ when exception.Message.Contains("rate limit") =&gt; "API rate limit exceeded. Please wait a moment.",
            _ when exception.Message.Contains("unauthorized") || exception.Message.Contains("401") =&gt; 
                "Authentication failed. Please check your API key.",
            _ when exception.Message.Contains("forbidden") || exception.Message.Contains("403") =&gt;
                "Access denied. Please check your permissions.",
            _ when exception.Message.Contains("not found") || exception.Message.Contains("404") =&gt;
                "The requested resource was not found.",
            _ when exception.Message.Contains("500") || exception.Message.Contains("server error") =&gt;
                "A server error occurred. Please try again later.",
            
            // Default
            _ =&gt; "An unexpected error occurred. Please try again."
        };
    }
}
