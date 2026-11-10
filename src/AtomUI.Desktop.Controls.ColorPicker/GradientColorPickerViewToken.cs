using AtomUI.Theme.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class GradientColorPickerViewToken : AbstractColorPickerViewToken
{
    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        if (!IsExplicit(GradientColorPickerViewTokenKind.ColorPickerHandlerSizeSM))
        {
            ColorPickerHandlerSizeSM = 12;
        }
        if (!IsExplicit(GradientColorPickerViewTokenKind.ColorPickerPresetColorSize))
        {
            ColorPickerPresetColorSize = 24;
        }
        if (!IsExplicit(GradientColorPickerViewTokenKind.ColorPickerSliderTrackSize))
        {
            ColorPickerSliderTrackSize = 8;
        }
        CalculateDerivedValues(ColorPickerHandlerSizeSM, ColorPickerPresetColorSize, ColorPickerSliderTrackSize);
    }
}
