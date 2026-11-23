using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Media;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="DrawerToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGDrawerDerivation : IControlDerivation<PrimeNGToken, DrawerToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<DrawerToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGDrawerTokens.EvaluateGlobal(in context, global);

        token.BoxShadowDrawerLeft = new BoxShadows(new BoxShadow()
        {
            OffsetX = 6,
            OffsetY = 0,
            Blur    = 16,
            Spread  = 0,
            Color   = ColorUtils.FromRgbF(0.08, 0, 0, 0)
        }, [
            new BoxShadow()
            {
                OffsetX = 3,
                OffsetY = 0,
                Blur    = 6,
                Spread  = -4,
                Color   = ColorUtils.FromRgbF(0.12, 0, 0, 0)
            },
            new BoxShadow()
            {
                OffsetX = 9,
                OffsetY = 0,
                Blur    = 28,
                Spread  = 8,
                Color   = ColorUtils.FromRgbF(0.05, 0, 0, 0)
            }
        ]);

        token.BoxShadowDrawerRight = new BoxShadows(new BoxShadow()
        {
            OffsetX = -6,
            OffsetY = 0,
            Blur    = 16,
            Spread  = 0,
            Color   = ColorUtils.FromRgbF(0.08, 0, 0, 0)
        }, [
            new BoxShadow()
            {
                OffsetX = -3,
                OffsetY = 0,
                Blur    = 6,
                Spread  = -4,
                Color   = ColorUtils.FromRgbF(0.12, 0, 0, 0)
            },
            new BoxShadow()
            {
                OffsetX = -9,
                OffsetY = 0,
                Blur    = 28,
                Spread  = 8,
                Color   = ColorUtils.FromRgbF(0.05, 0, 0, 0)
            }
        ]);

        token.BoxShadowDrawerUp = new BoxShadows(new BoxShadow()
        {
            OffsetX = 0,
            OffsetY = 6,
            Blur    = 16,
            Spread  = 0,
            Color   = ColorUtils.FromRgbF(0.08, 0, 0, 0)
        }, [
            new BoxShadow()
            {
                OffsetX = 0,
                OffsetY = 3,
                Blur    = 6,
                Spread  = -4,
                Color   = ColorUtils.FromRgbF(0.12, 0, 0, 0)
            },
            new BoxShadow()
            {
                OffsetX = 0,
                OffsetY = 9,
                Blur    = 28,
                Spread  = 8,
                Color   = ColorUtils.FromRgbF(0.05, 0, 0, 0)
            }
        ]);

        token.BoxShadowDrawerDown = new BoxShadows(new BoxShadow()
        {
            OffsetX = 0,
            OffsetY = -6,
            Blur    = 16,
            Spread  = 0,
            Color   = ColorUtils.FromRgbF(0.08, 0, 0, 0)
        }, [
            new BoxShadow()
            {
                OffsetX = 0,
                OffsetY = -3,
                Blur    = 6,
                Spread  = -4,
                Color   = ColorUtils.FromRgbF(0.12, 0, 0, 0)
            },
            new BoxShadow()
            {
                OffsetX = 0,
                OffsetY = -9,
                Blur    = 28,
                Spread  = 8,
                Color   = ColorUtils.FromRgbF(0.05, 0, 0, 0)
            }
        ]);

        token.SmallSize         = new Dimension(378);
        token.MiddleSize        = new Dimension(520);
        token.LargeSize         = new Dimension(736);
        token.HeaderMargin      = new Thickness(global.UniformlyMarginLG, global.UniformlyMargin);
        token.FooterPadding     = new Thickness(global.UniformlyPadding, global.UniformlyPaddingXS);
        token.CloseIconPadding  = global.PaddingXXS;
        token.CloseIconMargin   = new Thickness(0, 0, global.UniformlyMarginXS, 0);
        token.ContentPadding    = global.PaddingLG;
        token.PushOffsetPercent = 0.4;
        token.EdgeBorderColor   = Colors.Transparent;
        token.EdgeBorderThickness = 0;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGDrawerTokens.EvaluateOwn(in context, global, token);
    }
}
