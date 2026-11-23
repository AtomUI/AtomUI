using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="FlyoutHostToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignFlyoutHostDerivation : IControlDerivation<AntDesignToken, FlyoutHostToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<FlyoutHostToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.MarginToAnchor    = global.UniformlyMarginXXS;
        token.OverlayHostShadow = global.BoxShadowsSecondary;
        token.PopupRootShadow   = global.BoxShadowsSecondary;
        token.HorizontalOffset = global.UniformlyMarginXS;
        token.VerticalOffset   = global.UniformlyMarginXS;
    }
}
