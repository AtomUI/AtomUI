using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="AdornerLayerToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignAdornerLayerDerivation : IControlDerivation<AntDesignToken, AdornerLayerToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<AdornerLayerToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.FocusOutlineOffset = global.LineWidth;
    }
}
