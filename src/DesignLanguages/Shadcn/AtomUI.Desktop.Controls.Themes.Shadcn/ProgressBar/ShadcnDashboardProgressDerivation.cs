using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="DashboardProgressToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnDashboardProgressDerivation : IControlDerivation<ShadcnToken, DashboardProgressToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<DashboardProgressToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnAbstractCircleProgressDerivation.DeriveShared(in context, global, token);
    }
}
