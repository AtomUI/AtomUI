using System.Globalization;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Data.Converters;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Fits the theme's default padding before native owner overrides are applied.
/// </summary>
internal sealed class CustomInputPaddingConverter : IMultiValueConverter
{
    public static CustomInputPaddingConverter Instance { get; } = new();

    private CustomInputPaddingConverter()
    {
    }

    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 6 ||
            values[0] is not Thickness padding ||
            values[1] is not Thickness compactPadding ||
            values[2] is not double minContentHeight ||
            values[3] is not double height ||
            values[4] is not Thickness border ||
            values[5] is not InputControlStyleVariant variant)
        {
            return AvaloniaProperty.UnsetValue;
        }

        border = variant switch
        {
            InputControlStyleVariant.Borderless => default,
            InputControlStyleVariant.Underlined => new Thickness(0, 0, 0, border.Bottom),
            _ => border
        };
        return CustomizableSizeLayoutHelper.ShouldUseCompactVerticalPadding(
            CustomizableSizeType.Custom, height, padding, border, minContentHeight)
            ? new Thickness(padding.Left, compactPadding.Top, padding.Right, compactPadding.Bottom)
            : padding;
    }
}
