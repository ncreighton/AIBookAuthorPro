// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using AIBookAuthorPro.Core.Enums;

namespace AIBookAuthorPro.UI.Converters;

/// <summary>
/// Converts a CharacterRole to a corresponding color for UI display.
/// </summary>
[ValueConversion(typeof(CharacterRole), typeof(Brush))]
public class CharacterRoleToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not CharacterRole role)
        {
            return Brushes.Gray;
        }

        return role switch
        {
            CharacterRole.Protagonist => new SolidColorBrush(Color.FromRgb(33, 150, 243)),   // Blue
            CharacterRole.Antagonist => new SolidColorBrush(Color.FromRgb(244, 67, 54)),    // Red
            CharacterRole.Supporting => new SolidColorBrush(Color.FromRgb(76, 175, 80)),    // Green
            CharacterRole.Minor => new SolidColorBrush(Color.FromRgb(158, 158, 158)),       // Gray
            CharacterRole.Mentor => new SolidColorBrush(Color.FromRgb(156, 39, 176)),       // Purple
            CharacterRole.LoveInterest => new SolidColorBrush(Color.FromRgb(233, 30, 99)),  // Pink
            CharacterRole.Sidekick => new SolidColorBrush(Color.FromRgb(0, 188, 212)),      // Cyan
            CharacterRole.Narrator => new SolidColorBrush(Color.FromRgb(255, 152, 0)),      // Orange
            _ => Brushes.Gray
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
