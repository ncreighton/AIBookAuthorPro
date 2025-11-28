// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Globalization;
using System.Windows.Data;

namespace AIBookAuthorPro.UI.Converters;

/// <summary>
/// Inverts a boolean value.
/// </summary>
public class InvertedBoolConverter : IValueConverter
{
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static InvertedBoolConverter Instance { get; } = new();

    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return false;
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return false;
    }
}

