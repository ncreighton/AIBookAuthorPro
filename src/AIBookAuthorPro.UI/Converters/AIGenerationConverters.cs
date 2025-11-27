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
/// Converts a decimal cost value to a formatted currency display string.
/// </summary>
public sealed class CostDisplayConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal cost)
        {
            return cost switch
            {
                0 => "Free",
                < 0.01m => "< $0.01",
                < 1m => $"${cost:F3}",
                _ => $"${cost:F2}"
            };
        }

        if (value is double doubleCost)
        {
            return doubleCost switch
            {
                0 => "Free",
                < 0.01 => "< $0.01",
                < 1 => $"${doubleCost:F3}",
                _ => $"${doubleCost:F2}"
            };
        }

        return "$0.00";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts a GenerationMode enum or mode name to a Material Design icon name.
/// </summary>
public sealed class GenerationModeToIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Handle enum
        if (value is GenerationMode mode)
        {
            return mode switch
            {
                GenerationMode.Standard => "SpeedometerMedium",
                GenerationMode.Fast => "Flash",
                GenerationMode.HighQuality => "Diamond",
                GenerationMode.Creative => "Creation",
                GenerationMode.Precise => "Target",
                _ => "Robot"
            };
        }

        // Handle string mode name
        if (value is string modeName)
        {
            return modeName.ToLowerInvariant() switch
            {
                "standard" => "SpeedometerMedium",
                "fast" => "Flash",
                "highquality" or "high quality" => "Diamond",
                "creative" => "Creation",
                "precise" => "Target",
                _ => "Robot"
            };
        }

        return "Robot";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts token count to estimated cost based on model pricing.
/// </summary>
public sealed class TokensToCostConverter : IMultiValueConverter
{
    // Default pricing per 1K tokens (Claude Sonnet 3.5 pricing)
    private const decimal DefaultInputCostPer1K = 0.003m;
    private const decimal DefaultOutputCostPer1K = 0.015m;

    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length < 2)
            return "$0.00";

        var inputTokens = values[0] as int? ?? 0;
        var outputTokens = values[1] as int? ?? 0;

        var inputCostPer1K = values.Length > 2 && values[2] is decimal icp ? icp : DefaultInputCostPer1K;
        var outputCostPer1K = values.Length > 3 && values[3] is decimal ocp ? ocp : DefaultOutputCostPer1K;

        var totalCost = (inputTokens / 1000m * inputCostPer1K) + (outputTokens / 1000m * outputCostPer1K);

        return totalCost switch
        {
            0 => "Free",
            < 0.01m => "< $0.01",
            < 1m => $"${totalCost:F3}",
            _ => $"${totalCost:F2}"
        };
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
