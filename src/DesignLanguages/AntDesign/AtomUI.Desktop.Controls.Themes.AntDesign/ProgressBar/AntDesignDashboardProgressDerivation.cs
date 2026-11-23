using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="DashboardProgressToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignDashboardProgressDerivation : IControlDerivation<AntDesignToken, DashboardProgressToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<DashboardProgressToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractCircleProgressDerivation.DeriveShared(in context, global, token);
    }
}
