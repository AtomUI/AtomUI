using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Media;
using AtomUI.Theme;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="ScrollViewerToken"/>.</summary>
[ControlTokenRule]
internal sealed class ScrollViewerTokenRule : IControlTokenRule<PrimeNGToken, ScrollViewerToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<ScrollViewerToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
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
