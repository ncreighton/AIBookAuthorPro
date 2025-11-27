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
/// Converts chapter type to visibility for type-specific UI elements.
/// </summary>
[ValueConversion(typeof(ChapterType), typeof(Visibility))]
public class ChapterTypeVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ChapterType chapterType || parameter is not string targetTypeString)
        {
            return Visibility.Collapsed;
        }

        if (Enum.TryParse<ChapterType>(targetTypeString, out var targetType2))
        {
            return chapterType == targetType2 ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

/// <summary>
/// Converts outline item type to Material Design icon kind.
/// </summary>
[ValueConversion(typeof(OutlineItemType), typeof(string))]
public class OutlineTypeToIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not OutlineItemType itemType)
        {
            return "FileDocument";
        }

        return itemType switch
        {
            OutlineItemType.Act => "TheaterMasks",
            OutlineItemType.Part => "BookOpenPageVariant",
            OutlineItemType.Chapter => "FileDocument",
            OutlineItemType.Scene => "MovieOpenOutline",
            OutlineItemType.Beat => "Circle",
            OutlineItemType.Note => "NoteText",
            _ => "FileDocument"
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

/// <summary>
/// Converts string to OutlineItemType enum.
/// </summary>
[ValueConversion(typeof(string), typeof(OutlineItemType))]
public class StringToOutlineTypeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is OutlineItemType itemType)
        {
            return itemType.ToString();
        }
        return "Chapter";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string stringValue && Enum.TryParse<OutlineItemType>(stringValue, out var result))
        {
            return result;
        }
        return OutlineItemType.Chapter;
    }
}
