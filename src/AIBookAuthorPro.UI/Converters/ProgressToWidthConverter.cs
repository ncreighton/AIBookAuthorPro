// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AIBookAuthorPro.UI.Converters;

/// <summary>
/// Converts a progress percentage to a width value based on a maximum width.
/// </summary>
public sealed class ProgressToWidthConverter : IMultiValueConverter
{
    /// <summary>
    /// Converts progress percentage and container width to actual width.
    /// </summary>
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2)
            return 0.0;

        if (values[0] is not double progress || values[1] is not double maxWidth)
            return 0.0;

        // Clamp progress between 0 and 100
        progress = Math.Max(0, Math.Min(100, progress));

        return maxWidth * (progress / 100.0);
    }

    /// <summary>
    /// Not implemented - one-way binding only.
    /// </summary>
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts word count to formatted string with target.
/// </summary>
public sealed class WordCountDisplayConverter : IMultiValueConverter
{
    /// <inheritdoc />
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2)
            return "0 words";

        var current = values[0] is int c ? c : 0;
        var target = values[1] is int t ? t : 0;

        if (target > 0)
        {
            var percentage = Math.Min(100, (double)current / target * 100);
            return $"{current:N0} / {target:N0} words ({percentage:F0}%)";
        }

        return $"{current:N0} words";
    }

    /// <inheritdoc />
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts chapter status to color.
/// </summary>
public sealed class ChapterStatusToColorConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString() switch
        {
            "NotStarted" => "#9E9E9E",
            "InProgress" => "#2196F3",
            "Draft" => "#FF9800",
            "Review" => "#9C27B0",
            "Complete" => "#4CAF50",
            _ => "#9E9E9E"
        };
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
