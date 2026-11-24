using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="MarqueeLabelToken"/>.</summary>
[ControlTokenRule]
internal sealed class MarqueeLabelTokenRule : IControlTokenRule<PrimeNGToken, MarqueeLabelToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<MarqueeLabelToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        token.CycleSpace   = 200;
        token.DefaultSpeed = 150;
    }
}
