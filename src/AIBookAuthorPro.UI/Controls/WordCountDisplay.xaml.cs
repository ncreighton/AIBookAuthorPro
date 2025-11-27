// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AIBookAuthorPro.UI.Controls;

/// <summary>
/// Displays word count with optional progress towards a target.
/// </summary>
public partial class WordCountDisplay : UserControl
{
    #region Dependency Properties

    /// <summary>
    /// Gets or sets the current word count.
    /// </summary>
    public static readonly DependencyProperty WordCountProperty =
        DependencyProperty.Register(
            nameof(WordCount),
            typeof(int),
            typeof(WordCountDisplay),
            new PropertyMetadata(0, OnWordCountChanged));

    /// <summary>
    /// Gets or sets the target word count.
    /// </summary>
    public static readonly DependencyProperty TargetWordCountProperty =
        DependencyProperty.Register(
            nameof(TargetWordCount),
            typeof(int),
            typeof(WordCountDisplay),
            new PropertyMetadata(0, OnTargetChanged));

    /// <summary>
    /// Gets or sets whether to show the progress bar.
    /// </summary>
    public static readonly DependencyProperty ShowProgressProperty =
        DependencyProperty.Register(
            nameof(ShowProgress),
            typeof(bool),
            typeof(WordCountDisplay),
            new PropertyMetadata(true, OnShowProgressChanged));

    #endregion

    #region Properties

    public int WordCount
    {
        get => (int)GetValue(WordCountProperty);
        set => SetValue(WordCountProperty, value);
    }

    public int TargetWordCount
    {
        get => (int)GetValue(TargetWordCountProperty);
        set => SetValue(TargetWordCountProperty, value);
    }

    public bool ShowProgress
    {
        get => (bool)GetValue(ShowProgressProperty);
        set => SetValue(ShowProgressProperty, value);
    }

    #endregion

    /// <summary>
    /// Initializes a new instance of WordCountDisplay.
    /// </summary>
    public WordCountDisplay()
    {
        InitializeComponent();
    }

    private static void OnWordCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WordCountDisplay display)
        {
            display.UpdateDisplay();
        }
    }

    private static void OnTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WordCountDisplay display)
        {
            display.UpdateDisplay();
        }
    }

    private static void OnShowProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WordCountDisplay display)
        {
            display.UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        WordCountText.Text = WordCount.ToString("N0");

        if (TargetWordCount > 0 && ShowProgress)
        {
            var percentage = Math.Min(100, (WordCount * 100.0) / TargetWordCount);

            ProgressBar.Value = percentage;
            ProgressBar.Visibility = Visibility.Visible;

            // Color based on progress
            ProgressBar.Foreground = percentage switch
            {
                >= 100 => new SolidColorBrush(Color.FromRgb(76, 175, 80)),  // Green
                >= 75 => new SolidColorBrush(Color.FromRgb(33, 150, 243)),  // Blue
                >= 50 => new SolidColorBrush(Color.FromRgb(255, 152, 0)),   // Orange
                _ => new SolidColorBrush(Color.FromRgb(158, 158, 158))      // Gray
            };

            var remaining = TargetWordCount - WordCount;
            TargetText.Text = remaining > 0
                ? $"{remaining:N0} words to go ({percentage:F0}%)"
                : $"Target reached! (+{Math.Abs(remaining):N0})";
            TargetText.Visibility = Visibility.Visible;
        }
        else
        {
            ProgressBar.Visibility = Visibility.Collapsed;
            TargetText.Visibility = Visibility.Collapsed;
        }
    }

    /// <summary>
    /// Gets a formatted string for the word count.
    /// </summary>
    public string GetFormattedCount()
    {
        if (TargetWordCount > 0)
        {
            return $"{WordCount:N0} / {TargetWordCount:N0} words";
        }
        return $"{WordCount:N0} words";
    }
}
