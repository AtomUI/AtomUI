using AtomUI.Desktop.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Themes.Shadcn.Algorithms.Controls;

/// <summary>
/// Card under the shadcn styles: the <c>--card</c> surface (its own control-global background — in dark it is a
/// step lighter than the page), the card ring and shadow from the style table, the card padding (design 5.5).
/// </summary>
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova.Dark", 1)]
internal sealed class ShadcnCardTokens : IControlTokenAlgorithm<CardToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        var dark    = context.Appearance == ThemeAppearance.Dark;
        var surface = ShadcnBaseColors.Resolve(controlGlobal.ColorNeutral, dark);
        controlGlobal.ColorBgContainer = surface.Card;
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, CardToken token)
    {
        var dark    = context.Appearance == ThemeAppearance.Dark;
        var style   = ShadcnStyles.Resolve(context);
        var surface = ShadcnBaseColors.Resolve(effectiveGlobal.ColorNeutral, dark);
        var ring = ShadcnStyles.WithAlpha(
            surface.Foreground, ShadcnStyles.AlphaByte(style.CardRingAlpha));
        token.CardShadows = ShadcnStyles.RingThenShadow(ring, style.CardShadow);
        token.BodyPadding = new Thickness(style.CardPadding);
    }
}
