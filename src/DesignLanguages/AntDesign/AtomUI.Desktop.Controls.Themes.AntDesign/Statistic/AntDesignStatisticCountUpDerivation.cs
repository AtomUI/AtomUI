using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="StatisticCountUpToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignStatisticCountUpDerivation : IControlDerivation<AntDesignToken, StatisticCountUpToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<StatisticCountUpToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractStatisticValueDerivation.DeriveShared(in context, global, token);
    }
}
