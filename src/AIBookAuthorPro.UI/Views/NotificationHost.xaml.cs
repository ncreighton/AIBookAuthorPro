// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Windows;
using System.Windows.Controls;
using AIBookAuthorPro.Infrastructure.Services;

namespace AIBookAuthorPro.UI.Views;

/// <summary>
/// Host control for displaying notifications.
/// </summary>
public partial class NotificationHost : UserControl
{
    private readonly NotificationService? _notificationService;

    /// <summary>
    /// Initializes a new instance of NotificationHost.
    /// </summary>
    public NotificationHost()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Initializes with a notification service.
    /// </summary>
    public NotificationHost(NotificationService notificationService) : this()
    {
        _notificationService = notificationService;
        NotificationList.ItemsSource = _notificationService.ActiveNotifications;

        _notificationService.NotificationAdded += OnNotificationAdded;
        _notificationService.NotificationRemoved += OnNotificationRemoved;
    }

    private void OnNotificationAdded(object? sender, NotificationItem e)
    {
        // Refresh binding
        NotificationList.Items.Refresh();
    }

    private void OnNotificationRemoved(object? sender, NotificationItem e)
    {
        // Refresh binding
        NotificationList.Items.Refresh();
    }

    private void DismissButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is NotificationItem item)
        {
            _notificationService?.Dismiss(item.Id);
        }
    }

    private void ActionButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is NotificationItem item)
        {
            item.OnAction?.Invoke();
            _notificationService?.Dismiss(item.Id);
        }
    }
}
