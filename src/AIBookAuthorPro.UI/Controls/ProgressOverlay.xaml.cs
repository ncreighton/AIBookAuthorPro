// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using UserControl = System.Windows.Controls.UserControl;

namespace AIBookAuthorPro.UI.Controls;

/// <summary>
/// A reusable progress overlay control.
/// </summary>
public partial class ProgressOverlay : UserControl
{
    private CancellationTokenSource? _cancellationTokenSource;

    /// <summary>
    /// Event raised when cancel is requested.
    /// </summary>
    public event EventHandler? CancelRequested;

    #region Dependency Properties

    /// <summary>
    /// Gets or sets whether the overlay is visible.
    /// </summary>
    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(
            nameof(IsActive),
            typeof(bool),
            typeof(ProgressOverlay),
            new PropertyMetadata(false, OnIsActiveChanged));

    /// <summary>
    /// Gets or sets the progress message.
    /// </summary>
    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(
            nameof(Message),
            typeof(string),
            typeof(ProgressOverlay),
            new PropertyMetadata("Loading...", OnMessageChanged));

    /// <summary>
    /// Gets or sets the secondary message.
    /// </summary>
    public static readonly DependencyProperty SecondaryMessageProperty =
        DependencyProperty.Register(
            nameof(SecondaryMessage),
            typeof(string),
            typeof(ProgressOverlay),
            new PropertyMetadata(null, OnSecondaryMessageChanged));

    /// <summary>
    /// Gets or sets whether to show determinate progress.
    /// </summary>
    public static readonly DependencyProperty IsDeterminateProperty =
        DependencyProperty.Register(
            nameof(IsDeterminate),
            typeof(bool),
            typeof(ProgressOverlay),
            new PropertyMetadata(false, OnIsDeterminateChanged));

    /// <summary>
    /// Gets or sets the progress value (0-100).
    /// </summary>
    public static readonly DependencyProperty ProgressValueProperty =
        DependencyProperty.Register(
            nameof(ProgressValue),
            typeof(double),
            typeof(ProgressOverlay),
            new PropertyMetadata(0.0, OnProgressValueChanged));

    /// <summary>
    /// Gets or sets whether the operation can be cancelled.
    /// </summary>
    public static readonly DependencyProperty CanCancelProperty =
        DependencyProperty.Register(
            nameof(CanCancel),
            typeof(bool),
            typeof(ProgressOverlay),
            new PropertyMetadata(false, OnCanCancelChanged));

    #endregion

    #region Properties

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public string? SecondaryMessage
    {
        get => (string?)GetValue(SecondaryMessageProperty);
        set => SetValue(SecondaryMessageProperty, value);
    }

    public bool IsDeterminate
    {
        get => (bool)GetValue(IsDeterminateProperty);
        set => SetValue(IsDeterminateProperty, value);
    }

    public double ProgressValue
    {
        get => (double)GetValue(ProgressValueProperty);
        set => SetValue(ProgressValueProperty, value);
    }

    public bool CanCancel
    {
        get => (bool)GetValue(CanCancelProperty);
        set => SetValue(CanCancelProperty, value);
    }

    /// <summary>
    /// Gets the cancellation token for the current operation.
    /// </summary>
    public CancellationToken CancellationToken => _cancellationTokenSource?.Token ?? CancellationToken.None;

    #endregion

    /// <summary>
    /// Initializes a new instance of ProgressOverlay.
    /// </summary>
    public ProgressOverlay()
    {
        InitializeComponent();
    }

    #region Property Changed Handlers

    private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ProgressOverlay overlay)
        {
            overlay.UpdateVisibility((bool)e.NewValue);
        }
    }

    private static void OnMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ProgressOverlay overlay)
        {
            overlay.MessageText.Text = (string)e.NewValue;
        }
    }

    private static void OnSecondaryMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ProgressOverlay overlay)
        {
            var message = (string?)e.NewValue;
            overlay.SecondaryMessageText.Text = message ?? string.Empty;
            overlay.SecondaryMessageText.Visibility = string.IsNullOrEmpty(message)
                ? Visibility.Collapsed
                : Visibility.Visible;
        }
    }

    private static void OnIsDeterminateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ProgressOverlay overlay)
        {
            var isDeterminate = (bool)e.NewValue;
            overlay.CircularProgress.Visibility = isDeterminate ? Visibility.Collapsed : Visibility.Visible;
            overlay.LinearProgress.Visibility = isDeterminate ? Visibility.Visible : Visibility.Collapsed;
            overlay.CircularProgress.IsIndeterminate = !isDeterminate;
        }
    }

    private static void OnProgressValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ProgressOverlay overlay)
        {
            overlay.LinearProgress.Value = (double)e.NewValue;
        }
    }

    private static void OnCanCancelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ProgressOverlay overlay)
        {
            overlay.CancelButton.Visibility = (bool)e.NewValue
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }

    #endregion

    private void UpdateVisibility(bool isActive)
    {
        if (isActive)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            OverlayBorder.Visibility = Visibility.Visible;

            var fadeIn = (Storyboard)Resources["FadeInAnimation"];
            OverlayBorder.BeginStoryboard(fadeIn);
        }
        else
        {
            var fadeOut = (Storyboard)Resources["FadeOutAnimation"];
            fadeOut.Completed += (s, e) =>
            {
                OverlayBorder.Visibility = Visibility.Collapsed;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            };
            OverlayBorder.BeginStoryboard(fadeOut);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        _cancellationTokenSource?.Cancel();
        CancelRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Shows the overlay with the specified message.
    /// </summary>
    public void Show(string message, bool canCancel = false)
    {
        Message = message;
        CanCancel = canCancel;
        IsDeterminate = false;
        IsActive = true;
    }

    /// <summary>
    /// Shows determinate progress.
    /// </summary>
    public void ShowProgress(string message, double progress, string? secondaryMessage = null)
    {
        Message = message;
        SecondaryMessage = secondaryMessage;
        IsDeterminate = true;
        ProgressValue = progress;
        IsActive = true;
    }

    /// <summary>
    /// Hides the overlay.
    /// </summary>
    public void Hide()
    {
        IsActive = false;
    }
}
