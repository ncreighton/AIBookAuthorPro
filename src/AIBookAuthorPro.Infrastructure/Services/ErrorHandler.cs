// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System;
using System.IO;
using System.Net.Http;
using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.Infrastructure.Services;

/// <summary>
/// Centralized error handling service.
/// </summary>
public sealed class ErrorHandler : IErrorHandler
{
    private readonly ILogger<ErrorHandler> _logger;
    private readonly INotificationService _notificationService;

    /// <summary>
    /// Initializes a new instance of ErrorHandler.
    /// </summary>
    public ErrorHandler(
        ILogger<ErrorHandler> logger,
        INotificationService notificationService)
    {
        _logger = logger;
        _notificationService = notificationService;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public Result<T> HandleWithResult<T>(Exception exception, string? context = null, bool showNotification = true)
    {
        Handle(exception, context, showNotification);
        return Result<T>.Failure(GetUserFriendlyMessage(exception), exception);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public Result<T> TryExecute<T>(Func<T> func, string? context = null)
    {
        try
        {
            var result = func();
            return Result<T>.Success(result);
        }
        catch (Exception ex)
        {
            Handle(ex, context);
            return Result<T>.Failure(GetUserFriendlyMessage(ex), ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result> TryExecuteAsync(Func<Task> action, string? context = null, CancellationToken ct = default)
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

    /// <inheritdoc />
    public async Task<Result<T>> TryExecuteAsync<T>(Func<Task<T>> func, string? context = null, CancellationToken ct = default)
    {
        try
        {
            var result = await func();
            return Result<T>.Success(result);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            _logger.LogInformation("Operation cancelled: {Context}", context);
            return Result<T>.Failure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            Handle(ex, context);
            return Result<T>.Failure(GetUserFriendlyMessage(ex), ex);
        }
    }

    /// <summary>
    /// Gets a user-friendly message for an exception.
    /// </summary>
    private static string GetUserFriendlyMessage(Exception exception)
    {
        return exception switch
        {
            // Network errors
            HttpRequestException => "Unable to connect to the server. Please check your internet connection.",
            TaskCanceledException => "The operation timed out. Please try again.",
            
            // File errors
            FileNotFoundException => "The requested file could not be found.",
            DirectoryNotFoundException => "The specified directory does not exist.",
            UnauthorizedAccessException => "Access denied. Please check your permissions.",
            IOException io when io.Message.Contains("disk") => "Disk error occurred. Please check available space.",
            IOException => "A file operation failed. The file may be in use.",
            
            // Validation errors
            ArgumentException => exception.Message,
            InvalidOperationException => exception.Message,
            
            // API errors
            _ when exception.Message.Contains("rate limit") => "API rate limit exceeded. Please wait a moment.",
            _ when exception.Message.Contains("unauthorized") || exception.Message.Contains("401") => 
                "Authentication failed. Please check your API key.",
            _ when exception.Message.Contains("forbidden") || exception.Message.Contains("403") =>
                "Access denied. Please check your permissions.",
            _ when exception.Message.Contains("not found") || exception.Message.Contains("404") =>
                "The requested resource was not found.",
            _ when exception.Message.Contains("500") || exception.Message.Contains("server error") =>
                "A server error occurred. Please try again later.",
            
            // Default
            _ => "An unexpected error occurred. Please try again."
        };
    }
}
