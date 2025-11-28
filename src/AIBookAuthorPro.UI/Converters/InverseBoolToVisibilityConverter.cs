// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AIBookAuthorPro.UI.Converters;

/// <summary>
/// Converts boolean or numeric values to visibility, inverted.
/// - true/non-zero = Collapsed
/// - false/zero/null = Visible
/// </summary>
public sealed class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Handle null
        if (value is null)
        {
            return Visibility.Visible;
        }
        
        // Handle bool
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Collapsed : Visibility.Visible;
        }
        
        // Handle int (for Count properties)
        if (value is int intValue)
        {
            return intValue > 0 ? Visibility.Collapsed : Visibility.Visible;
        }
        
        // Handle other numeric types
        if (value is IConvertible convertible)
        {
            try
            {
                var numericValue = convertible.ToDouble(CultureInfo.InvariantCulture);
                return numericValue > 0 ? Visibility.Collapsed : Visibility.Visible;
            }
            catch
            {
                // Fall through to default
            }
        }
        
        // Default: treat as "truthy" if not null
        return Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility != Visibility.Visible;
        }
        return false;
    }
}
