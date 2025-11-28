// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace AIBookAuthorPro.UI.Behaviors;

/// <summary>
/// Behavior that handles keyboard shortcuts for a window or control.
/// </summary>
public class KeyboardShortcutBehavior : Behavior<UIElement>
{
    /// <summary>
    /// Gets or sets the key to listen for.
    /// </summary>
    public static readonly DependencyProperty KeyProperty =
        DependencyProperty.Register(
            nameof(Key),
            typeof(Key),
            typeof(KeyboardShortcutBehavior),
            new PropertyMetadata(Key.None));

    /// <summary>
    /// Gets or sets the modifier keys.
    /// </summary>
    public static readonly DependencyProperty ModifiersProperty =
        DependencyProperty.Register(
            nameof(Modifiers),
            typeof(ModifierKeys),
            typeof(KeyboardShortcutBehavior),
            new PropertyMetadata(ModifierKeys.None));

    /// <summary>
    /// Gets or sets the command to execute.
    /// </summary>
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(
            nameof(Command),
            typeof(ICommand),
            typeof(KeyboardShortcutBehavior),
            new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the command parameter.
    /// </summary>
    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(
            nameof(CommandParameter),
            typeof(object),
            typeof(KeyboardShortcutBehavior),
            new PropertyMetadata(null));

    public Key Key
    {
        get => (Key)GetValue(KeyProperty);
        set => SetValue(KeyProperty, value);
    }

    public ModifierKeys Modifiers
    {
        get => (ModifierKeys)GetValue(ModifiersProperty);
        set => SetValue(ModifiersProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    /// <inheritdoc />
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.PreviewKeyDown += OnPreviewKeyDown;
    }

    /// <inheritdoc />
    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.PreviewKeyDown -= OnPreviewKeyDown;
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key && Keyboard.Modifiers == Modifiers)
        {
            if (Command?.CanExecute(CommandParameter) == true)
            {
                Command.Execute(CommandParameter);
                e.Handled = true;
            }
        }
    }
}

/// <summary>
/// Manages global keyboard shortcuts for the application.
/// </summary>
public static class KeyboardShortcuts
{
    /// <summary>
    /// Standard keyboard shortcuts used throughout the application.
    /// </summary>
    public static class StandardShortcuts
    {
        // File operations
        public static readonly KeyGesture NewProject = new(Key.N, ModifierKeys.Control);
        public static readonly KeyGesture OpenProject = new(Key.O, ModifierKeys.Control);
        public static readonly KeyGesture Save = new(Key.S, ModifierKeys.Control);
        public static readonly KeyGesture SaveAs = new(Key.S, ModifierKeys.Control | ModifierKeys.Shift);
        public static readonly KeyGesture Close = new(Key.W, ModifierKeys.Control);
        public static readonly KeyGesture Export = new(Key.E, ModifierKeys.Control | ModifierKeys.Shift);

        // Edit operations
        public static readonly KeyGesture Undo = new(Key.Z, ModifierKeys.Control);
        public static readonly KeyGesture Redo = new(Key.Y, ModifierKeys.Control);
        public static readonly KeyGesture Cut = new(Key.X, ModifierKeys.Control);
        public static readonly KeyGesture Copy = new(Key.C, ModifierKeys.Control);
        public static readonly KeyGesture Paste = new(Key.V, ModifierKeys.Control);
        public static readonly KeyGesture SelectAll = new(Key.A, ModifierKeys.Control);
        public static readonly KeyGesture Find = new(Key.F, ModifierKeys.Control);
        public static readonly KeyGesture Replace = new(Key.H, ModifierKeys.Control);

        // Navigation
        public static readonly KeyGesture NextChapter = new(Key.PageDown, ModifierKeys.Control);
        public static readonly KeyGesture PreviousChapter = new(Key.PageUp, ModifierKeys.Control);
        public static readonly KeyGesture GoToChapter = new(Key.G, ModifierKeys.Control);

        // AI Generation
        public static readonly KeyGesture GenerateContent = new(Key.Enter, ModifierKeys.Control | ModifierKeys.Shift);
        public static readonly KeyGesture ContinueWriting = new(Key.Enter, ModifierKeys.Control);
        public static readonly KeyGesture RewriteSelection = new(Key.R, ModifierKeys.Control | ModifierKeys.Shift);

        // View
        public static readonly KeyGesture ToggleFullScreen = new(Key.F11, ModifierKeys.None);
        public static readonly KeyGesture ZoomIn = new(Key.OemPlus, ModifierKeys.Control);
        public static readonly KeyGesture ZoomOut = new(Key.OemMinus, ModifierKeys.Control);
        public static readonly KeyGesture ResetZoom = new(Key.D0, ModifierKeys.Control);
        public static readonly KeyGesture ToggleSidebar = new(Key.B, ModifierKeys.Control);

        // Application
        public static readonly KeyGesture Settings = new(Key.OemComma, ModifierKeys.Control);
        public static readonly KeyGesture Help = new(Key.F1, ModifierKeys.None);
    }

    /// <summary>
    /// Gets a display string for a key gesture.
    /// </summary>
    public static string GetDisplayString(KeyGesture gesture)
    {
        var parts = new List<string>();

        if (gesture.Modifiers.HasFlag(ModifierKeys.Control))
            parts.Add("Ctrl");
        if (gesture.Modifiers.HasFlag(ModifierKeys.Shift))
            parts.Add("Shift");
        if (gesture.Modifiers.HasFlag(ModifierKeys.Alt))
            parts.Add("Alt");

        parts.Add(GetKeyDisplayName(gesture.Key));

        return string.Join("+", parts);
    }

    private static string GetKeyDisplayName(Key key) => key switch
    {
        Key.OemPlus => "+",
        Key.OemMinus => "-",
        Key.OemComma => ",",
        Key.OemPeriod => ".",
        Key.D0 => "0",
        Key.D1 => "1",
        Key.D2 => "2",
        Key.D3 => "3",
        Key.D4 => "4",
        Key.D5 => "5",
        Key.D6 => "6",
        Key.D7 => "7",
        Key.D8 => "8",
        Key.D9 => "9",
        _ => key.ToString()
    };
}
