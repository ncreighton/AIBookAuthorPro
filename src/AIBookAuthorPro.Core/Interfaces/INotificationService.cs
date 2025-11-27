// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

namespace AIBookAuthorPro.Core.Interfaces;

/// <summary>
/// Service for displaying notifications to the user.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Shows an information notification.
    /// </summary>
    void ShowInfo(string message, string? title = null);

    /// <summary>
    /// Shows a success notification.
    /// </summary>
    void ShowSuccess(string message, string? title = null);

    /// <summary>
    /// Shows a warning notification.
    /// </summary>
    void ShowWarning(string message, string? title = null);

    /// <summary>
    /// Shows an error notification.
    /// </summary>
    void ShowError(string message, string? title = null, Exception? exception = null);

    /// <summary>
    /// Shows a notification with custom options.
    /// </summary>
    void Show(NotificationOptions options);

    /// <summary>
    /// Shows a confirmation dialog.
    /// </summary>
    Task<bool> ConfirmAsync(string message, string? title = null);

    /// <summary>
    /// Shows a prompt dialog.
    /// </summary>
    Task<string?> PromptAsync(string message, string? title = null, string? defaultValue = null);

    /// <summary>
    /// Clears all active notifications.
    /// </summary>
    void ClearAll();
}

/// <summary>
/// Options for displaying a notification.
/// </summary>
public class NotificationOptions
{
    /// <summary>
    /// Gets or sets the notification message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the notification title.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the notification type.
    /// </summary>
    public NotificationType Type { get; set; } = NotificationType.Info;

    /// <summary>
    /// Gets or sets how long the notification should be displayed (null for indefinite).
    /// </summary>
    public TimeSpan? Duration { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets or sets whether the notification can be dismissed.
    /// </summary>
    public bool IsDismissable { get; set; } = true;

    /// <summary>
    /// Gets or sets the action to perform when notification is clicked.
    /// </summary>
    public Action? OnClick { get; set; }

    /// <summary>
    /// Gets or sets the action button text.
    /// </summary>
    public string? ActionText { get; set; }

    /// <summary>
    /// Gets or sets the action to perform when action button is clicked.
    /// </summary>
    public Action? OnAction { get; set; }
}

/// <summary>
/// Types of notifications.
/// </summary>
public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error
}
