using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Media;
using AtomUI.Theme;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="ScrollViewerToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnScrollViewerDerivation : IControlDerivation<ShadcnToken, ScrollViewerToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<ScrollViewerToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.LiteModeThumbThickness   = global.LineWidthBold;
        token.NormalModeThumbThickness = global.SizeXS;

        if ((context.Variant.Appearance == ThemeAppearance.Dark))
        {
            token.ThumbBg       = global.ColorBorder;
            token.ThumbHoverBg  = token.ThumbBg.Lighten();

            token.ThumbActiveBg = token.ThumbHoverBg.Lighten();
        }
        else
        {
            token.ThumbBg       = global.ColorBorder;
            token.ThumbHoverBg  = token.ThumbBg.Darken();

            token.ThumbActiveBg = token.ThumbHoverBg.Darken();
        }
        token.ThumbCornerRadius        = new CornerRadius(token.NormalModeThumbThickness / 2);
        token.IndicatorThumbCornerRadius = new CornerRadius(token.LiteModeThumbThickness / 2);
        token.ScrollBarContentHPadding = new Thickness(global.UniformlyPaddingXXS, 0d);
        token.ScrollBarContentVPadding = new Thickness(0d, global.UniformlyPaddingXXS);
    }
}
