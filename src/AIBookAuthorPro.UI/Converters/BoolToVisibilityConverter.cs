// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AIBookAuthorPro.UI.Converters;

/// <summary>
/// Converts boolean values to Visibility.
/// </summary>
public sealed class BoolToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Gets or sets whether to invert the conversion.
    /// </summary>
    public bool Invert { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var boolValue = value is bool b && b;
        if (Invert)
        {
            boolValue = !boolValue;
        }
        return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var visibility = value is Visibility v ? v : Visibility.Collapsed;
        var result = visibility == Visibility.Visible;
        return Invert ? !result : result;
    }
}

