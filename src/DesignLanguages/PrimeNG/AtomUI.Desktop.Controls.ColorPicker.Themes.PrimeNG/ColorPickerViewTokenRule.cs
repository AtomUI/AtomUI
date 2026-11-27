using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="ColorPickerViewToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class ColorPickerViewTokenRule : IControlTokenRule<PrimeNGToken, ColorPickerViewToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<ColorPickerViewToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        ColorPickerViewTokenCalculator.Calculate(in context, global, token);
        // Ant Design formula kept as is: the PrimeNG mapping has no preset values for the color picker view
        // (the PrimeUIX colorpicker panel), so the rule takes the Ant Design formula over the PrimeNG global
        // tokens.
        token.ColorPickerHandlerSizeSM = 12;
        token.ColorPickerPresetColorSize = 24;
        token.ColorPickerSliderTrackSize = 8;
        token.ColorPickerSliderSize = token.ColorPickerHandlerSizeSM;
        ColorPickerViewTokenCalculator.CalculateDependentSizes(token, global, token.ColorPickerHandlerSizeSM, token.ColorPickerPresetColorSize, token.ColorPickerSliderTrackSize);
    }
}
