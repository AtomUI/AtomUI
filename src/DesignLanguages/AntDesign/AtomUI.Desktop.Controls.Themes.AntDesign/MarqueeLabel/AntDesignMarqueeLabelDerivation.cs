using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="MarqueeLabelToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignMarqueeLabelDerivation : IControlDerivation<AntDesignToken, MarqueeLabelToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<MarqueeLabelToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.CycleSpace   = 200;
        token.DefaultSpeed = 150;
    }
}
