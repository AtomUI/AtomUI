using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="DotBadgeToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignDotBadgeDerivation : IControlDerivation<AntDesignToken, DotBadgeToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<DotBadgeToken> view)
    {
        var token  = view.Token;
        var global = view.Global;

        token.DotSize     = global.FontSizeSM / 2;
        token.DotColor    = global.ColorError;
        token.ShadowSize  = global.LineWidth;
        token.ShadowColor = global.ColorBorderBg;
        token.LabelMargin = new Thickness(global.UniformlyMarginXS, 0, 0, 0);
    }
}
