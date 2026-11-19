using AtomUI.Theme.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class ColorPickerViewToken : AbstractColorPickerViewToken
{
    protected override void CalculateTokenValues(bool isDarkMode)
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
        ColorPickerSliderSize = ColorPickerHandlerSizeSM;
        CalculateDependentSizes(ColorPickerHandlerSizeSM, ColorPickerPresetColorSize, ColorPickerSliderTrackSize);
    }
}
