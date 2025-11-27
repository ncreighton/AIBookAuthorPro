// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Globalization;
using System.Windows.Data;

namespace AIBookAuthorPro.UI.Converters;

/// <summary>
/// Converts bool to width for navigation rail.
/// </summary>
public class BoolToWidthConverter : IValueConverter
{
    /// <summary>
    /// Gets or sets the width when true (expanded).
    /// </summary>
    public double ExpandedWidth { get; set; } = 200;

    /// <summary>
    /// Gets or sets the width when false (collapsed).
    /// </summary>
    public double CollapsedWidth { get; set; } = 72;

    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isExpanded)
        {
            return isExpanded ? ExpandedWidth : CollapsedWidth;
        }
        return CollapsedWidth;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double width)
        {
            return width > CollapsedWidth;
        }
        return false;
    }
}
