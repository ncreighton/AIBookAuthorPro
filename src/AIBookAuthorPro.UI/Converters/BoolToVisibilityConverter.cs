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

/// <summary>
/// Converts null values to Visibility.
/// </summary>
public sealed class NullToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Gets or sets whether to show when null (true) or when not null (false).
    /// </summary>
    public bool ShowWhenNull { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var isNull = value == null || (value is string s && string.IsNullOrEmpty(s));
        var shouldShow = ShowWhenNull ? isNull : !isNull;
        return shouldShow ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts decimal cost values to display format.
/// </summary>
public sealed class CostDisplayConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal cost)
        {
            if (cost < 0.01m)
            {
                return "< $0.01";
            }
            return cost.ToString("C2", CultureInfo.GetCultureInfo("en-US"));
        }
        return "$0.00";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts generation mode name to icon.
/// </summary>
public sealed class GenerationModeToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string modeName)
        {
            return modeName.ToLowerInvariant() switch
            {
                "quick" or "quickdraft" or "quick draft" => "LightningBolt",
                "standard" => "Star",
                "premium" => "Crown",
                "custom" => "Cog",
                _ => "Robot"
            };
        }
        return "Robot";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
