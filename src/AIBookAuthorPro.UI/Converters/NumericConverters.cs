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

/// <summary>
/// Converts file size in bytes to human-readable format.
/// </summary>
[ValueConversion(typeof(long), typeof(string))]
public class FileSizeConverter : IValueConverter
{
    private static readonly string[] SizeSuffixes = { "B", "KB", "MB", "GB", "TB" };

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not long bytes || bytes < 0)
        {
            return "0 B";
        }

        if (bytes == 0)
        {
            return "0 B";
        }

        var magnitude = (int)Math.Log(bytes, 1024);
        magnitude = Math.Min(magnitude, SizeSuffixes.Length - 1);
        var adjustedSize = bytes / Math.Pow(1024, magnitude);

        return $"{adjustedSize:N1} {SizeSuffixes[magnitude]}";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
