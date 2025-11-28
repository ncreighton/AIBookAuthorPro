// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Xaml.Behaviors;
using Panel = System.Windows.Controls.Panel;
using Point = System.Windows.Point;
using Color = System.Windows.Media.Color;
using Brushes = System.Windows.Media.Brushes;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using ProgressBar = System.Windows.Controls.ProgressBar;
using Application = System.Windows.Application;

namespace AIBookAuthorPro.UI.Behaviors;

/// <summary>
/// Behavior that shows a loading overlay on a control.
/// </summary>
public class LoadingOverlayBehavior : Behavior<FrameworkElement>
{
    private Grid? _overlayGrid;
    private Border? _overlay;

    /// <summary>
    /// Gets or sets whether loading is active.
    /// </summary>
    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(
            nameof(IsLoading),
            typeof(bool),
            typeof(LoadingOverlayBehavior),
            new PropertyMetadata(false, OnIsLoadingChanged));

    /// <summary>
    /// Gets or sets the loading message.
    /// </summary>
    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(
            nameof(Message),
            typeof(string),
            typeof(LoadingOverlayBehavior),
            new PropertyMetadata("Loading...", OnMessageChanged));

    /// <summary>
    /// Gets or sets whether the overlay should be semi-transparent.
    /// </summary>
    public static readonly DependencyProperty UseOverlayProperty =
        DependencyProperty.Register(
            nameof(UseOverlay),
            typeof(bool),
            typeof(LoadingOverlayBehavior),
            new PropertyMetadata(true));

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public bool UseOverlay
    {
        get => (bool)GetValue(UseOverlayProperty);
        set => SetValue(UseOverlayProperty, value);
    }

    private static void OnIsLoadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LoadingOverlayBehavior behavior)
        {
            behavior.UpdateOverlayVisibility((bool)e.NewValue);
        }
    }

    private static void OnMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LoadingOverlayBehavior behavior)
        {
            behavior.UpdateMessage((string)e.NewValue);
        }
    }

    /// <inheritdoc />
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.Loaded += OnLoaded;
    }

    /// <inheritdoc />
    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.Loaded -= OnLoaded;
        RemoveOverlay();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        EnsureOverlayCreated();
    }

    private void EnsureOverlayCreated()
    {
        if (_overlay != null) return;

        // Find or create adorner layer
        if (AssociatedObject.Parent is Panel parentPanel)
        {
            _overlayGrid = new Grid();

            _overlay = new Border
            {
                Background = UseOverlay
                    ? new SolidColorBrush(Color.FromArgb(180, 0, 0, 0))
                    : Brushes.Transparent,
                Visibility = Visibility.Collapsed,
                Child = CreateLoadingContent()
            };

            _overlayGrid.Children.Add(_overlay);

            // Add to parent
            var index = parentPanel.Children.IndexOf(AssociatedObject);
            Grid.SetRow(_overlayGrid, Grid.GetRow(AssociatedObject));
            Grid.SetColumn(_overlayGrid, Grid.GetColumn(AssociatedObject));
            Grid.SetRowSpan(_overlayGrid, Grid.GetRowSpan(AssociatedObject));
            Grid.SetColumnSpan(_overlayGrid, Grid.GetColumnSpan(AssociatedObject));

            parentPanel.Children.Insert(index + 1, _overlayGrid);
        }
    }

    private UIElement CreateLoadingContent()
    {
        var stack = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        // Spinner
        var spinner = new ProgressBar
        {
            IsIndeterminate = true,
            Width = 50,
            Height = 50,
            Style = (Style)Application.Current.FindResource("MaterialDesignCircularProgressBar")
        };
        stack.Children.Add(spinner);

        // Message
        var textBlock = new TextBlock
        {
            Name = "LoadingMessage",
            Text = Message,
            Foreground = Brushes.White,
            FontSize = 14,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 16, 0, 0)
        };
        stack.Children.Add(textBlock);

        return stack;
    }

    private void UpdateOverlayVisibility(bool isLoading)
    {
        if (_overlay == null) return;

        if (isLoading)
        {
            _overlay.Visibility = Visibility.Visible;

            // Fade in animation
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            _overlay.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        }
        else
        {
            // Fade out animation
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
            fadeOut.Completed += (s, e) => _overlay.Visibility = Visibility.Collapsed;
            _overlay.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }
    }

    private void UpdateMessage(string message)
    {
        if (_overlay?.Child is StackPanel stack)
        {
            foreach (var child in stack.Children)
            {
                if (child is TextBlock textBlock && textBlock.Name == "LoadingMessage")
                {
                    textBlock.Text = message;
                    break;
                }
            }
        }
    }

    private void RemoveOverlay()
    {
        if (_overlayGrid?.Parent is Panel parentPanel)
        {
            parentPanel.Children.Remove(_overlayGrid);
        }
        _overlay = null;
        _overlayGrid = null;
    }
}

/// <summary>
/// Behavior that adds a subtle pulse animation to an element.
/// </summary>
public class PulseAnimationBehavior : Behavior<UIElement>
{
    private Storyboard? _storyboard;

    /// <summary>
    /// Gets or sets whether the animation is active.
    /// </summary>
    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(
            nameof(IsActive),
            typeof(bool),
            typeof(PulseAnimationBehavior),
            new PropertyMetadata(false, OnIsActiveChanged));

    /// <summary>
    /// Gets or sets the animation duration in milliseconds.
    /// </summary>
    public static readonly DependencyProperty DurationProperty =
        DependencyProperty.Register(
            nameof(Duration),
            typeof(double),
            typeof(PulseAnimationBehavior),
            new PropertyMetadata(1000.0));

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public double Duration
    {
        get => (double)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PulseAnimationBehavior behavior)
        {
            if ((bool)e.NewValue)
            {
                behavior.StartAnimation();
            }
            else
            {
                behavior.StopAnimation();
            }
        }
    }

    /// <inheritdoc />
    protected override void OnAttached()
    {
        base.OnAttached();

        // Ensure transform exists
        if (AssociatedObject.RenderTransform is not ScaleTransform)
        {
            AssociatedObject.RenderTransform = new ScaleTransform(1, 1);
            AssociatedObject.RenderTransformOrigin = new Point(0.5, 0.5);
        }
    }

    private void StartAnimation()
    {
        if (AssociatedObject.RenderTransform is not ScaleTransform transform)
        {
            transform = new ScaleTransform(1, 1);
            AssociatedObject.RenderTransform = transform;
            AssociatedObject.RenderTransformOrigin = new Point(0.5, 0.5);
        }

        _storyboard = new Storyboard();

        var scaleXAnimation = new DoubleAnimation
        {
            From = 1.0,
            To = 1.05,
            Duration = TimeSpan.FromMilliseconds(Duration / 2),
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever
        };

        var scaleYAnimation = new DoubleAnimation
        {
            From = 1.0,
            To = 1.05,
            Duration = TimeSpan.FromMilliseconds(Duration / 2),
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever
        };

        Storyboard.SetTarget(scaleXAnimation, AssociatedObject);
        Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath("RenderTransform.ScaleX"));

        Storyboard.SetTarget(scaleYAnimation, AssociatedObject);
        Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("RenderTransform.ScaleY"));

        _storyboard.Children.Add(scaleXAnimation);
        _storyboard.Children.Add(scaleYAnimation);

        _storyboard.Begin();
    }

    private void StopAnimation()
    {
        _storyboard?.Stop();
        _storyboard = null;

        // Reset transform
        if (AssociatedObject.RenderTransform is ScaleTransform transform)
        {
            transform.ScaleX = 1;
            transform.ScaleY = 1;
        }
    }
}
