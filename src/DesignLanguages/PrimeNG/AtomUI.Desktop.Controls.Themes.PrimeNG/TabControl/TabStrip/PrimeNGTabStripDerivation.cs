using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="TabStripToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGTabStripDerivation : IControlDerivation<PrimeNGToken, TabStripToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<TabStripToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGTabStripTokens.EvaluateGlobal(in context, global);
        PrimeNGAbstractLineTabDerivation.DeriveShared(in context, global, token);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGTabStripTokens.EvaluateOwn(in context, global, token);
    }
}
