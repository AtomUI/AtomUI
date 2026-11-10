using AtomUI.Theme.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class ColorPickerViewToken : AbstractColorPickerViewToken
{
    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        if (!IsExplicit(ColorPickerViewTokenKind.ColorPickerHandlerSizeSM))
        {
            ColorPickerHandlerSizeSM = 12;
        }
        if (!IsExplicit(ColorPickerViewTokenKind.ColorPickerPresetColorSize))
        {
            ColorPickerPresetColorSize = 24;
        }
        if (!IsExplicit(ColorPickerViewTokenKind.ColorPickerSliderTrackSize))
        {
            ColorPickerSliderTrackSize = 8;
        }
        CalculateDerivedValues(ColorPickerHandlerSizeSM, ColorPickerPresetColorSize, ColorPickerSliderTrackSize);
    }
}
