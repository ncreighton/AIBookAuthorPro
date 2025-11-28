// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using AIBookAuthorPro.Core.Interfaces;
using Microsoft.Xaml.Behaviors;
using RichTextBox = System.Windows.Controls.RichTextBox;

namespace AIBookAuthorPro.UI.Behaviors;

/// <summary>
/// Behavior that enables two-way binding for RichTextBox content.
/// </summary>
public sealed class RichTextBoxBindingBehavior : Behavior<RichTextBox>
{
    private bool _isUpdating;
    private DispatcherTimer? _debounceTimer;
    private IFlowDocumentService? _flowDocumentService;

    /// <summary>
    /// Gets or sets the bound XAML content.
    /// </summary>
    public static readonly DependencyProperty XamlContentProperty =
        DependencyProperty.Register(
            nameof(XamlContent),
            typeof(string),
            typeof(RichTextBoxBindingBehavior),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnXamlContentChanged));

    /// <summary>
    /// Gets or sets the debounce delay in milliseconds.
    /// </summary>
    public static readonly DependencyProperty DebounceDelayProperty =
        DependencyProperty.Register(
            nameof(DebounceDelay),
            typeof(int),
            typeof(RichTextBoxBindingBehavior),
            new PropertyMetadata(500));

    /// <summary>
    /// Gets or sets the FlowDocument service instance.
    /// </summary>
    public static readonly DependencyProperty FlowDocumentServiceProperty =
        DependencyProperty.Register(
            nameof(FlowDocumentService),
            typeof(IFlowDocumentService),
            typeof(RichTextBoxBindingBehavior),
            new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the bound XAML content.
    /// </summary>
    public string XamlContent
    {
        get => (string)GetValue(XamlContentProperty);
        set => SetValue(XamlContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the debounce delay in milliseconds.
    /// </summary>
    public int DebounceDelay
    {
        get => (int)GetValue(DebounceDelayProperty);
        set => SetValue(DebounceDelayProperty, value);
    }

    /// <summary>
    /// Gets or sets the FlowDocument service.
    /// </summary>
    public IFlowDocumentService? FlowDocumentService
    {
        get => (IFlowDocumentService?)GetValue(FlowDocumentServiceProperty);
        set => SetValue(FlowDocumentServiceProperty, value);
    }

    /// <inheritdoc />
    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject.TextChanged += OnTextChanged;
        AssociatedObject.Loaded += OnLoaded;

        // Initialize debounce timer
        _debounceTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(DebounceDelay)
        };
        _debounceTimer.Tick += OnDebounceTimerTick;

        // Try to get service from DataContext if not set
        if (FlowDocumentService == null && AssociatedObject.DataContext is IHasFlowDocumentService serviceProvider)
        {
            _flowDocumentService = serviceProvider.FlowDocumentService;
        }
    }

    /// <inheritdoc />
    protected override void OnDetaching()
    {
        base.OnDetaching();

        AssociatedObject.TextChanged -= OnTextChanged;
        AssociatedObject.Loaded -= OnLoaded;

        _debounceTimer?.Stop();
        _debounceTimer = null;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Initial load from binding
        UpdateDocumentFromXaml();
    }

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating) return;

        // Reset debounce timer
        _debounceTimer?.Stop();
        _debounceTimer?.Start();
    }

    private void OnDebounceTimerTick(object? sender, EventArgs e)
    {
        _debounceTimer?.Stop();
        UpdateXamlFromDocument();
    }

    private static void OnXamlContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RichTextBoxBindingBehavior behavior && !behavior._isUpdating)
        {
            behavior.UpdateDocumentFromXaml();
        }
    }

    private void UpdateDocumentFromXaml()
    {
        if (AssociatedObject == null) return;

        var service = FlowDocumentService ?? _flowDocumentService;
        if (service == null)
        {
            // Fallback: create simple document from plain text
            UpdateDocumentFromPlainText();
            return;
        }

        _isUpdating = true;
        try
        {
            var xaml = XamlContent;

            if (string.IsNullOrEmpty(xaml))
            {
                AssociatedObject.Document = service.CreateEmpty();
                return;
            }

            var result = service.FromXaml(xaml);

            if (result.IsSuccess && result.Value != null)
            {
                AssociatedObject.Document = result.Value;
            }
            else
            {
                // Try loading as plain text
                AssociatedObject.Document = service.FromPlainText(xaml);
            }
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void UpdateDocumentFromPlainText()
    {
        _isUpdating = true;
        try
        {
            var text = XamlContent ?? string.Empty;
            AssociatedObject.Document = new FlowDocument(new Paragraph(new Run(text)));
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void UpdateXamlFromDocument()
    {
        if (AssociatedObject?.Document == null) return;

        var service = FlowDocumentService ?? _flowDocumentService;
        if (service == null)
        {
            // Fallback: extract plain text
            UpdateXamlFromPlainText();
            return;
        }

        _isUpdating = true;
        try
        {
            var result = service.ToXaml(AssociatedObject.Document);

            if (result.IsSuccess)
            {
                XamlContent = result.Value ?? string.Empty;
            }
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void UpdateXamlFromPlainText()
    {
        _isUpdating = true;
        try
        {
            var textRange = new TextRange(
                AssociatedObject.Document.ContentStart,
                AssociatedObject.Document.ContentEnd);

            XamlContent = textRange.Text;
        }
        finally
        {
            _isUpdating = false;
        }
    }
}

/// <summary>
/// Interface for ViewModels that provide FlowDocument service.
/// </summary>
public interface IHasFlowDocumentService
{
    /// <summary>
    /// Gets the FlowDocument service instance.
    /// </summary>
    IFlowDocumentService FlowDocumentService { get; }
}
