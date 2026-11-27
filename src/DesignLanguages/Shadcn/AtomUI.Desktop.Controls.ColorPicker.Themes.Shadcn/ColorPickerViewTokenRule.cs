using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="ColorPickerViewToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class ColorPickerViewTokenRule : IControlTokenRule<ShadcnToken, ColorPickerViewToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<ColorPickerViewToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        ColorPickerViewTokenCalculator.Calculate(in context, global, token);
        // Ant Design formula kept as is: shadcn has no color picker, so the language has no values of its own.
        token.ColorPickerHandlerSizeSM = 12;
        token.ColorPickerPresetColorSize = 24;
        token.ColorPickerSliderTrackSize = 8;
        token.ColorPickerSliderSize = token.ColorPickerHandlerSizeSM;
        ColorPickerViewTokenCalculator.CalculateDependentSizes(token, global, token.ColorPickerHandlerSizeSM, token.ColorPickerPresetColorSize, token.ColorPickerSliderTrackSize);
    }
}
