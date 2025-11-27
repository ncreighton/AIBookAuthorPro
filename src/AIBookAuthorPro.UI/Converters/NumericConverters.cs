// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AIBookAuthorPro.UI.Converters;

/// <summary>
/// Converts zero to Visibility.Collapsed and non-zero to Visibility.Visible.
/// </summary>
[ValueConversion(typeof(int), typeof(Visibility))]
public class ZeroToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int intValue)
        {
            return intValue == 0 ? Visibility.Collapsed : Visibility.Visible;
        }
        if (value is long longValue)
        {
            return longValue == 0 ? Visibility.Collapsed : Visibility.Visible;
        }
        if (value is double doubleValue)
        {
            return doubleValue == 0 ? Visibility.Collapsed : Visibility.Visible;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
