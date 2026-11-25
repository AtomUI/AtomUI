using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="ColorPickerToken"/>.</summary>
[ControlTokenRule]
internal sealed class ColorPickerTokenRule : IControlTokenRule<AntDesignToken, ColorPickerToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<ColorPickerToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        ColorPickerTokenCalculator.Calculate(in context, global, token);
    }
}
