using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="GradientColorPickerToken"/>.</summary>
[ControlTokenRule]
internal sealed class GradientColorPickerTokenRule : IControlTokenRule<PrimeNGToken, GradientColorPickerToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<GradientColorPickerToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        ColorPickerTokenCalculator.Calculate(in context, global, token);
    }
}
