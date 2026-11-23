using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="RibbonBadgeToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGRibbonBadgeDerivation : IControlDerivation<PrimeNGToken, RibbonBadgeToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<RibbonBadgeToken> view)
    {
        var token  = view.Token;
        var global = view.Global;

        token.Offset             = new Point(global.UniformlyMarginXS, global.UniformlyMarginXS);
        token.CornerTransform    = new ScaleTransform(1, 0.75).ToImmutable();
        token.CornerDarkenAmount = 15;
        token.TextPadding        = new Thickness(global.UniformlyPaddingXS, 0);
    }
}
