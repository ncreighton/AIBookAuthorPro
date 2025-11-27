// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Globalization;
using System.Windows.Data;

namespace AIBookAuthorPro.UI.Converters;

/// <summary>
/// Converts a name to initials (e.g., "John Smith" -> "JS").
/// </summary>
public class InitialsConverter : IValueConverter
{
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static InitialsConverter Instance { get; } = new();

    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string name || string.IsNullOrWhiteSpace(name))
        {
            return "?";
        }

        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
        {
            return "?";
        }

        if (parts.Length == 1)
        {
            return parts[0][..Math.Min(2, parts[0].Length)].ToUpperInvariant();
        }

        return $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[^1][0])}";
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

/// <summary>
/// Converts null to a boolean value.
/// </summary>
public class NullToBoolConverter : IValueConverter
{
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static NullToBoolConverter Instance { get; } = new();

    /// <summary>
    /// Gets or sets whether to invert the result.
    /// </summary>
    public bool Invert { get; set; }

    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isNull = value == null;
        return Invert ? !isNull : isNull;
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

/// <summary>
/// Converts zero to visibility.
/// </summary>
public class ZeroToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static ZeroToVisibilityConverter Instance { get; } = new();

    /// <summary>
    /// Gets or sets the visibility when value is zero.
    /// </summary>
    public System.Windows.Visibility ZeroVisibility { get; set; } = System.Windows.Visibility.Visible;

    /// <summary>
    /// Gets or sets the visibility when value is non-zero.
    /// </summary>
    public System.Windows.Visibility NonZeroVisibility { get; set; } = System.Windows.Visibility.Collapsed;

    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isZero = value switch
        {
            int i => i == 0,
            long l => l == 0,
            double d => d == 0,
            decimal m => m == 0,
            _ => false
        };

        return isZero ? ZeroVisibility : NonZeroVisibility;
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

/// <summary>
/// Converts a character role to a color brush.
/// </summary>
public class CharacterRoleToColorConverter : IValueConverter
{
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static CharacterRoleToColorConverter Instance { get; } = new();

    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not AIBookAuthorPro.Core.Enums.CharacterRole role)
        {
            return System.Windows.Media.Brushes.Gray;
        }

        return role switch
        {
            Core.Enums.CharacterRole.Protagonist => new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0x21, 0x96, 0xF3)), // Blue
            Core.Enums.CharacterRole.Antagonist => new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0xF4, 0x43, 0x36)), // Red
            Core.Enums.CharacterRole.Supporting => new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0x4C, 0xAF, 0x50)), // Green
            Core.Enums.CharacterRole.Minor => new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0x9E, 0x9E, 0x9E)), // Gray
            Core.Enums.CharacterRole.Narrator => new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0x9C, 0x27, 0xB0)), // Purple
            _ => System.Windows.Media.Brushes.Gray
        };
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
