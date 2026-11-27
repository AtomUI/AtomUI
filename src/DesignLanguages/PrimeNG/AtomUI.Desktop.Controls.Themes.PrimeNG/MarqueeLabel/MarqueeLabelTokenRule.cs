using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="MarqueeLabelToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class MarqueeLabelTokenRule : IControlTokenRule<PrimeNGToken, MarqueeLabelToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<MarqueeLabelToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // Ant Design formula kept as is: PrimeUIX defines no values for this control, so the rule takes the Ant
        // Design formula over the PrimeNG global tokens.
        token.CycleSpace   = 200;
        token.DefaultSpeed = 150;
    }
}
