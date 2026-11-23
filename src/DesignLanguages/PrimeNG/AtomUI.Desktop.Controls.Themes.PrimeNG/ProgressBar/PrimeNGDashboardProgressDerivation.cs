using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="DashboardProgressToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGDashboardProgressDerivation : IControlDerivation<PrimeNGToken, DashboardProgressToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<DashboardProgressToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGProgressTokens<DashboardProgressToken>.EvaluateGlobal(in context, global);
        PrimeNGAbstractCircleProgressDerivation.DeriveShared(in context, global, token);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGProgressTokens<DashboardProgressToken>.EvaluateOwn(in context, global, token);
    }
}
