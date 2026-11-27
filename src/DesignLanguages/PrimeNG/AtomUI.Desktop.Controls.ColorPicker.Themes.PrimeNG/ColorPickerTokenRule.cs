using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="ColorPickerToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class ColorPickerTokenRule : IControlTokenRule<PrimeNGToken, ColorPickerToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<ColorPickerToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        ColorPickerTokenCalculator.Calculate(in context, global, token);
    }
}
