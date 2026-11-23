using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="DotBadgeToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnDotBadgeDerivation : IControlDerivation<ShadcnToken, DotBadgeToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<DotBadgeToken> view)
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
