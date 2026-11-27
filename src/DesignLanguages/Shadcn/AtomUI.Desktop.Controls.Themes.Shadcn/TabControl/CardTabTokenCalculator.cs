using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn values of the shared <see cref="AbstractCardTabToken" /> layer.
/// </summary>
public static class CardTabTokenCalculator
{
    /// <summary>
    /// Fills the <see cref="AbstractCardTabToken" /> layer with the shadcn values.
    /// <see cref="TabTokenCalculator.Calculate" /> fills the tab layer first; this method then sets the card
    /// background, the small and large card paddings, the vertical item padding, the gutter and the margins of the
    /// add-tab button.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<ShadcnToken> context, DesignToken global, AbstractCardTabToken token)
    {
        TabTokenCalculator.Calculate(in context, global, token);
        token.CardBg = global.ColorFillAlter;
        token.CardPaddingSM = new Thickness(global.UniformlyPadding, global.UniformlyPaddingXXS * 1.5);
        token.CardPaddingLG = new Thickness(top: global.UniformlyPaddingXS,
            bottom: global.UniformlyPaddingXXS * 1.5,
            left: global.UniformlyPadding,
            right: global.UniformlyPadding);
        token.VerticalItemPadding = new Thickness(global.UniformlyPaddingXS, global.UniformlyPaddingXS);
        token.CardGutter = global.UniformlyMarginXXS / 2;
        token.AddTabButtonMarginHorizontal = new Thickness(global.UniformlyMarginXXS, 0, 0, 0);
        token.AddTabButtonMarginVertical = new Thickness(0, global.UniformlyMarginXXS, 0, 0);
    }


    /// <summary>
    /// Provides the padding of a card tab item: the global <c>UniformlyPadding</c> on the left and right, and on the
    /// top and bottom half of the space that <paramref name="cardSize" /> leaves around one line of body text, less
    /// the border width. The <paramref name="token" /> is not read.
    /// </summary>
    public static Thickness CalculateCardPadding(AbstractCardTabToken token, DesignToken global, double cardSize) =>
        new(global.UniformlyPadding,
            (cardSize - Math.Round(global.FontSize * global.RelativeLineHeight)) / 2 - global.LineWidth);
}
