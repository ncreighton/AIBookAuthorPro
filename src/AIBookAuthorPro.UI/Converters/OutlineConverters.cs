// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Globalization;
using System.Windows;
using System.Windows.Data;
using AIBookAuthorPro.Core.Enums;

namespace AIBookAuthorPro.UI.Converters;

/// <summary>
/// Converts OutlineItemType.Chapter to Visibility.Visible, otherwise Collapsed.
/// </summary>
public class ChapterTypeVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static ChapterTypeVisibilityConverter Instance { get; } = new();

    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is OutlineItemType type)
        {
            return type == OutlineItemType.Chapter ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

/// <summary>
/// Converts OutlineItemType to icon name.
/// </summary>
public class OutlineTypeToIconConverter : IValueConverter
{
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static OutlineTypeToIconConverter Instance { get; } = new();

    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is OutlineItemType type)
        {
            return type switch
            {
                OutlineItemType.Act => "TheaterMasks",
                OutlineItemType.Part => "BookOpenPageVariant",
                OutlineItemType.Chapter => "FileDocument",
                OutlineItemType.Scene => "MovieOpen",
                OutlineItemType.Beat => "CircleSmall",
                OutlineItemType.Note => "Note",
                _ => "Circle"
            };
        }
        return "Circle";
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

/// <summary>
/// Converts not null to Visibility.Visible.
/// </summary>
public class NotNullToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static NotNullToVisibilityConverter Instance { get; } = new();

    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value != null ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

/// <summary>
/// Converts string to OutlineItemType enum.
/// </summary>
public class StringToOutlineTypeConverter : IValueConverter
{
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static StringToOutlineTypeConverter Instance { get; } = new();

    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is OutlineItemType type)
        {
            return type.ToString();
        }
        return string.Empty;
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string str && Enum.TryParse<OutlineItemType>(str, out var type))
        {
            return type;
        }
        return OutlineItemType.Scene;
    }
}
