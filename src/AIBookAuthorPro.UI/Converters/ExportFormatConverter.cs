// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Globalization;
using System.Windows.Data;
using AIBookAuthorPro.Core.Enums;

namespace AIBookAuthorPro.UI.Converters;

/// <summary>
/// Converts ExportFormat to display name.
/// </summary>
public class ExportFormatToNameConverter : IValueConverter
{
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static ExportFormatToNameConverter Instance { get; } = new();

    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ExportFormat format)
        {
            return format switch
            {
                ExportFormat.Docx => "Word Document (.docx)",
                ExportFormat.Pdf => "PDF Document (.pdf)",
                ExportFormat.Epub => "EPUB eBook (.epub)",
                ExportFormat.Markdown => "Markdown (.md)",
                ExportFormat.Html => "HTML Web Page (.html)",
                ExportFormat.PlainText => "Plain Text (.txt)",
                _ => format.ToString()
            };
        }
        return string.Empty;
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

/// <summary>
/// Converts ExportFormat to icon name.
/// </summary>
public class ExportFormatToIconConverter : IValueConverter
{
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static ExportFormatToIconConverter Instance { get; } = new();

    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ExportFormat format)
        {
            return format switch
            {
                ExportFormat.Docx => "FileWord",
                ExportFormat.Pdf => "FilePdfBox",
                ExportFormat.Epub => "BookOpen",
                ExportFormat.Markdown => "LanguageMarkdown",
                ExportFormat.Html => "LanguageHtml5",
                ExportFormat.PlainText => "FileDocument",
                _ => "File"
            };
        }
        return "File";
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

/// <summary>
/// Converts file size in bytes to human-readable format.
/// </summary>
public class FileSizeConverter : IValueConverter
{
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static FileSizeConverter Instance { get; } = new();

    private static readonly string[] Suffixes = { "B", "KB", "MB", "GB" };

    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is long bytes)
        {
            if (bytes == 0) return "0 B";

            var magnitude = (int)Math.Floor(Math.Log(bytes, 1024));
            magnitude = Math.Min(magnitude, Suffixes.Length - 1);

            var adjustedSize = bytes / Math.Pow(1024, magnitude);
            return $"{adjustedSize:N1} {Suffixes[magnitude]}";
        }
        return "0 B";
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
