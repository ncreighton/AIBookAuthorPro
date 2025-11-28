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

