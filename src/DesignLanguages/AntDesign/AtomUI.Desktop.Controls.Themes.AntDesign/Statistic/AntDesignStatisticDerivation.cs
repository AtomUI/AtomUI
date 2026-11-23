using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="StatisticToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignStatisticDerivation : IControlDerivation<AntDesignToken, StatisticToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<StatisticToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractStatisticDerivation.DeriveShared(in context, global, token);
    }
}
