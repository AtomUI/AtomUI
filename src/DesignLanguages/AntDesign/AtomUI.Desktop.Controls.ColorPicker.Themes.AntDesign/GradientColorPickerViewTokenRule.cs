using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="GradientColorPickerViewToken"/>.</summary>
[ControlTokenRule]
internal sealed class GradientColorPickerViewTokenRule : IControlTokenRule<AntDesignToken, GradientColorPickerViewToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<GradientColorPickerViewToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        ColorPickerViewTokenCalculator.Calculate(in context, global, token);
        token.ColorPickerHandlerSizeSM = 12;
        token.ColorPickerPresetColorSize = 24;
        token.ColorPickerSliderTrackSize = 8;
        token.ColorPickerSliderSize = token.ColorPickerHandlerSizeSM;
        ColorPickerViewTokenCalculator.CalculateDependentSizes(token, global, token.ColorPickerHandlerSizeSM, token.ColorPickerPresetColorSize, token.ColorPickerSliderTrackSize);
    }
}
