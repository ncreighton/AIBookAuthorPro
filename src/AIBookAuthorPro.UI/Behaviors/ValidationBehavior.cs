// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Xaml.Behaviors;
using TextBox = System.Windows.Controls.TextBox;

namespace AIBookAuthorPro.UI.Behaviors;

/// <summary>
/// Behavior that validates TextBox input and shows validation state.
/// </summary>
public class TextBoxValidationBehavior : Behavior<TextBox>
{
    /// <summary>
    /// Gets or sets whether the field is required.
    /// </summary>
    public static readonly DependencyProperty IsRequiredProperty =
        DependencyProperty.Register(
            nameof(IsRequired),
            typeof(bool),
            typeof(TextBoxValidationBehavior),
            new PropertyMetadata(false));

    /// <summary>
    /// Gets or sets the maximum length.
    /// </summary>
    public static readonly DependencyProperty MaxLengthProperty =
        DependencyProperty.Register(
            nameof(MaxLength),
            typeof(int),
            typeof(TextBoxValidationBehavior),
            new PropertyMetadata(int.MaxValue));

    /// <summary>
    /// Gets or sets the minimum length.
    /// </summary>
    public static readonly DependencyProperty MinLengthProperty =
        DependencyProperty.Register(
            nameof(MinLength),
            typeof(int),
            typeof(TextBoxValidationBehavior),
            new PropertyMetadata(0));

    /// <summary>
    /// Gets or sets the validation error message.
    /// </summary>
    public static readonly DependencyProperty ErrorMessageProperty =
        DependencyProperty.Register(
            nameof(ErrorMessage),
            typeof(string),
            typeof(TextBoxValidationBehavior),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// Gets or sets whether the input is valid.
    /// </summary>
    public static readonly DependencyProperty IsValidProperty =
        DependencyProperty.Register(
            nameof(IsValid),
            typeof(bool),
            typeof(TextBoxValidationBehavior),
            new PropertyMetadata(true));

    public bool IsRequired
    {
        get => (bool)GetValue(IsRequiredProperty);
        set => SetValue(IsRequiredProperty, value);
    }

    public int MaxLength
    {
        get => (int)GetValue(MaxLengthProperty);
        set => SetValue(MaxLengthProperty, value);
    }

    public int MinLength
    {
        get => (int)GetValue(MinLengthProperty);
        set => SetValue(MinLengthProperty, value);
    }

    public string ErrorMessage
    {
        get => (string)GetValue(ErrorMessageProperty);
        set => SetValue(ErrorMessageProperty, value);
    }

    public bool IsValid
    {
        get => (bool)GetValue(IsValidProperty);
        set => SetValue(IsValidProperty, value);
    }

    /// <inheritdoc />
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.TextChanged += OnTextChanged;
        AssociatedObject.LostFocus += OnLostFocus;
    }

    /// <inheritdoc />
    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.TextChanged -= OnTextChanged;
        AssociatedObject.LostFocus -= OnLostFocus;
    }

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        // Clear error on typing
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            Validate();
        }
    }

    private void OnLostFocus(object sender, RoutedEventArgs e)
    {
        Validate();
    }

    private void Validate()
    {
        var text = AssociatedObject.Text ?? string.Empty;
        IsValid = true;
        ErrorMessage = string.Empty;

        if (IsRequired && string.IsNullOrWhiteSpace(text))
        {
            IsValid = false;
            ErrorMessage = "This field is required";
            return;
        }

        if (text.Length < MinLength)
        {
            IsValid = false;
            ErrorMessage = $"Minimum length is {MinLength} characters";
            return;
        }

        if (text.Length > MaxLength)
        {
            IsValid = false;
            ErrorMessage = $"Maximum length is {MaxLength} characters";
        }
    }
}

/// <summary>
/// Behavior that validates numeric input.
/// </summary>
public class NumericValidationBehavior : Behavior<TextBox>
{
    /// <summary>
    /// Gets or sets the minimum value.
    /// </summary>
    public static readonly DependencyProperty MinValueProperty =
        DependencyProperty.Register(
            nameof(MinValue),
            typeof(double),
            typeof(NumericValidationBehavior),
            new PropertyMetadata(double.MinValue));

    /// <summary>
    /// Gets or sets the maximum value.
    /// </summary>
    public static readonly DependencyProperty MaxValueProperty =
        DependencyProperty.Register(
            nameof(MaxValue),
            typeof(double),
            typeof(NumericValidationBehavior),
            new PropertyMetadata(double.MaxValue));

    /// <summary>
    /// Gets or sets whether decimal values are allowed.
    /// </summary>
    public static readonly DependencyProperty AllowDecimalsProperty =
        DependencyProperty.Register(
            nameof(AllowDecimals),
            typeof(bool),
            typeof(NumericValidationBehavior),
            new PropertyMetadata(true));

    public double MinValue
    {
        get => (double)GetValue(MinValueProperty);
        set => SetValue(MinValueProperty, value);
    }

    public double MaxValue
    {
        get => (double)GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    public bool AllowDecimals
    {
        get => (bool)GetValue(AllowDecimalsProperty);
        set => SetValue(AllowDecimalsProperty, value);
    }

    /// <inheritdoc />
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.PreviewTextInput += OnPreviewTextInput;
        DataObject.AddPastingHandler(AssociatedObject, OnPasting);
    }

    /// <inheritdoc />
    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.PreviewTextInput -= OnPreviewTextInput;
        DataObject.RemovePastingHandler(AssociatedObject, OnPasting);
    }

    private void OnPreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
    {
        e.Handled = !IsValidInput(e.Text);
    }

    private void OnPasting(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(typeof(string)))
        {
            var text = (string)e.DataObject.GetData(typeof(string))!;
            if (!IsValidInput(text))
            {
                e.CancelCommand();
            }
        }
        else
        {
            e.CancelCommand();
        }
    }

    private bool IsValidInput(string input)
    {
        var currentText = AssociatedObject.Text ?? string.Empty;
        var newText = currentText.Insert(AssociatedObject.SelectionStart, input);

        if (AllowDecimals)
        {
            return double.TryParse(newText, out var value) &&
                   value >= MinValue &&
                   value <= MaxValue;
        }
        else
        {
            return int.TryParse(newText, out var value) &&
                   value >= MinValue &&
                   value <= MaxValue;
        }
    }
}
