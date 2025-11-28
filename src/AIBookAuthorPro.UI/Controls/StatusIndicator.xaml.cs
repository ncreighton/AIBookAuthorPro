// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using UserControl = System.Windows.Controls.UserControl;

namespace AIBookAuthorPro.UI.Controls;

/// <summary>
/// Displays a status indicator with colored dot and text.
/// </summary>
public partial class StatusIndicator : UserControl
{
    #region Dependency Properties

    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    public static readonly DependencyProperty StatusProperty =
        DependencyProperty.Register(
            nameof(Status),
            typeof(IndicatorStatus),
            typeof(StatusIndicator),
            new PropertyMetadata(IndicatorStatus.Ready, OnStatusChanged));

    /// <summary>
    /// Gets or sets custom status text.
    /// </summary>
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(StatusIndicator),
            new PropertyMetadata(null, OnTextChanged));

    #endregion

    #region Properties

    public IndicatorStatus Status
    {
        get => (IndicatorStatus)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    #endregion

    /// <summary>
    /// Initializes a new instance of StatusIndicator.
    /// </summary>
    public StatusIndicator()
    {
        InitializeComponent();
        UpdateDisplay();
    }

    private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StatusIndicator indicator)
        {
            indicator.UpdateDisplay();
        }
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StatusIndicator indicator)
        {
            indicator.UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        var (color, text) = Status switch
        {
            IndicatorStatus.Ready => (Color.FromRgb(76, 175, 80), "Ready"),
            IndicatorStatus.Working => (Color.FromRgb(33, 150, 243), "Working..."),
            IndicatorStatus.Saving => (Color.FromRgb(255, 152, 0), "Saving..."),
            IndicatorStatus.Generating => (Color.FromRgb(156, 39, 176), "Generating..."),
            IndicatorStatus.Error => (Color.FromRgb(244, 67, 54), "Error"),
            IndicatorStatus.Offline => (Color.FromRgb(158, 158, 158), "Offline"),
            IndicatorStatus.Syncing => (Color.FromRgb(0, 188, 212), "Syncing..."),
            _ => (Color.FromRgb(158, 158, 158), "Unknown")
        };

        StatusDot.Fill = new SolidColorBrush(color);
        StatusText.Text = Text ?? text;
    }
}

/// <summary>
/// Status indicator states.
/// </summary>
public enum IndicatorStatus
{
    Ready,
    Working,
    Saving,
    Generating,
    Error,
    Offline,
    Syncing
}
