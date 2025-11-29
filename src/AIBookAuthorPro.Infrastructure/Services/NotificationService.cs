// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Collections.ObjectModel;
using System.Windows;
using AIBookAuthorPro.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.Infrastructure.Services;

/// <summary>
/// Service for managing application notifications.
/// </summary>
public sealed class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly ObservableCollection<NotificationItem> _notifications = [];

    /// <summary>
    /// Gets the active notifications.
    /// </summary>
    public IReadOnlyCollection<NotificationItem> ActiveNotifications => _notifications;

    /// <summary>
    /// Event raised when a notification is added.
    /// </summary>
    public event EventHandler<NotificationItem>? NotificationAdded;

    /// <summary>
    /// Event raised when a notification is removed.
    /// </summary>
    public event EventHandler<NotificationItem>? NotificationRemoved;

    /// <summary>
    /// Initializes a new instance of NotificationService.
    /// </summary>
    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public void ShowInfo(string message, string? title = null)
    {
        Show(new NotificationOptions
        {
            Message = message,
            Title = title ?? "Information",
            Type = NotificationType.Info
        });
    }

    /// <inheritdoc />
    public void ShowSuccess(string message, string? title = null)
    {
        Show(new NotificationOptions
        {
            Message = message,
            Title = title ?? "Success",
            Type = NotificationType.Success
        });
    }

    /// <inheritdoc />
    public void ShowWarning(string message, string? title = null)
    {
        Show(new NotificationOptions
        {
            Message = message,
            Title = title ?? "Warning",
            Type = NotificationType.Warning,
            Duration = TimeSpan.FromSeconds(8)
        });
    }

    /// <inheritdoc />
    public void ShowError(string message, string? title = null, Exception? exception = null)
    {
        if (exception != null)
        {
            _logger.LogError(exception, "Error notification: {Message}", message);
        }

        Show(new NotificationOptions
        {
            Message = message,
            Title = title ?? "Error",
            Type = NotificationType.Error,
            Duration = null // Errors don't auto-dismiss
        });
    }

    /// <inheritdoc />
    public void Show(NotificationOptions options)
    {
        var item = new NotificationItem
        {
            Id = Guid.NewGuid(),
            Message = options.Message,
            Title = options.Title,
            Type = options.Type,
            IsDismissable = options.IsDismissable,
            ActionText = options.ActionText,
            OnClick = options.OnClick,
            OnAction = options.OnAction,
            CreatedAt = DateTime.UtcNow
        };

        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            _notifications.Add(item);
            NotificationAdded?.Invoke(this, item);

            _logger.LogDebug("Notification shown: {Type} - {Message}", options.Type, options.Message);

            // Auto-dismiss after duration
            if (options.Duration.HasValue)
            {
                Task.Delay(options.Duration.Value).ContinueWith(_ =>
                {
                    System.Windows.Application.Current?.Dispatcher.Invoke(() => Dismiss(item.Id));
                });
            }
        });
    }

    /// <summary>
    /// Dismisses a notification by ID.
    /// </summary>
    public void Dismiss(Guid notificationId)
    {
        var item = _notifications.FirstOrDefault(n => n.Id == notificationId);
        if (item != null)
        {
            _notifications.Remove(item);
            NotificationRemoved?.Invoke(this, item);
        }
    }

    /// <inheritdoc />
    public void ClearAll()
    {
        var items = _notifications.ToList();
        _notifications.Clear();

        foreach (var item in items)
        {
            NotificationRemoved?.Invoke(this, item);
        }
    }

    /// <inheritdoc />
    public Task<bool> ConfirmAsync(string message, string? title = null)
    {
        var result = MessageBox.Show(
            message,
            title ?? "Confirm",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        return Task.FromResult(result == MessageBoxResult.Yes);
    }

    /// <inheritdoc />
    public Task<string?> PromptAsync(string message, string? title = null, string? defaultValue = null)
    {
        // For a real implementation, show a custom dialog
        // For now, use InputBox-style dialog
        var result = Microsoft.VisualBasic.Interaction.InputBox(
            message,
            title ?? "Input Required",
            defaultValue ?? string.Empty);

        return Task.FromResult<string?>(string.IsNullOrEmpty(result) ? null : result);
    }
}

/// <summary>
/// Represents a notification item.
/// </summary>
public class NotificationItem
{
    /// <summary>
    /// Gets or sets the notification ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the notification type.
    /// </summary>
    public NotificationType Type { get; set; }

    /// <summary>
    /// Gets or sets whether the notification can be dismissed.
    /// </summary>
    public bool IsDismissable { get; set; } = true;

    /// <summary>
    /// Gets or sets the action button text.
    /// </summary>
    public string? ActionText { get; set; }

    /// <summary>
    /// Gets or sets the click action.
    /// </summary>
    public Action? OnClick { get; set; }

    /// <summary>
    /// Gets or sets the action button handler.
    /// </summary>
    public Action? OnAction { get; set; }

    /// <summary>
    /// Gets or sets when the notification was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets the icon for this notification type.
    /// </summary>
    public string Icon => Type switch
    {
        NotificationType.Success => "CheckCircle",
        NotificationType.Warning => "Alert",
        NotificationType.Error => "AlertCircle",
        _ => "InformationOutline"
    };
}
