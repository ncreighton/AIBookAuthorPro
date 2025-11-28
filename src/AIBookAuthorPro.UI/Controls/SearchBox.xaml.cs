// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using UserControl = System.Windows.Controls.UserControl;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace AIBookAuthorPro.UI.Controls;

/// <summary>
/// A search box control with debouncing.
/// </summary>
public partial class SearchBox : UserControl
{
    private DispatcherTimer? _debounceTimer;

    /// <summary>
    /// Event raised when search text changes (debounced).
    /// </summary>
    public event EventHandler<string>? SearchTextChanged;

    /// <summary>
    /// Event raised when Enter is pressed.
    /// </summary>
    public event EventHandler<string>? SearchSubmitted;

    #region Dependency Properties

    /// <summary>
    /// Gets or sets the search text.
    /// </summary>
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(SearchBox),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnTextChanged));

    /// <summary>
    /// Gets or sets the placeholder text.
    /// </summary>
    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.Register(
            nameof(Placeholder),
            typeof(string),
            typeof(SearchBox),
            new PropertyMetadata("Search...", OnPlaceholderChanged));

    /// <summary>
    /// Gets or sets the debounce delay in milliseconds.
    /// </summary>
    public static readonly DependencyProperty DebounceDelayProperty =
        DependencyProperty.Register(
            nameof(DebounceDelay),
            typeof(int),
            typeof(SearchBox),
            new PropertyMetadata(300));

    #endregion

    #region Properties

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public int DebounceDelay
    {
        get => (int)GetValue(DebounceDelayProperty);
        set => SetValue(DebounceDelayProperty, value);
    }

    #endregion

    /// <summary>
    /// Initializes a new instance of SearchBox.
    /// </summary>
    public SearchBox()
    {
        InitializeComponent();
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SearchBox searchBox)
        {
            searchBox.SearchTextBox.Text = (string)e.NewValue;
        }
    }

    private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SearchBox searchBox)
        {
            MaterialDesignThemes.Wpf.HintAssist.SetHint(searchBox.SearchTextBox, (string)e.NewValue);
        }
    }

    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var text = SearchTextBox.Text;
        Text = text;

        // Reset debounce timer
        _debounceTimer?.Stop();

        _debounceTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(DebounceDelay)
        };

        _debounceTimer.Tick += (s, args) =>
        {
            _debounceTimer.Stop();
            SearchTextChanged?.Invoke(this, text);
        };

        _debounceTimer.Start();
    }

    private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            _debounceTimer?.Stop();
            SearchSubmitted?.Invoke(this, SearchTextBox.Text);
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            SearchTextBox.Text = string.Empty;
            e.Handled = true;
        }
    }

    /// <summary>
    /// Clears the search text.
    /// </summary>
    public void Clear()
    {
        SearchTextBox.Text = string.Empty;
    }

    /// <summary>
    /// Focuses the search box.
    /// </summary>
    public new void Focus()
    {
        SearchTextBox.Focus();
    }
}
