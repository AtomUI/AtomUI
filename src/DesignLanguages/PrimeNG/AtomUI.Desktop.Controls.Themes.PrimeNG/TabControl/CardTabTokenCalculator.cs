using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG values of the shared <see cref="AbstractCardTabToken" /> layer.
/// </summary>
public static class CardTabTokenCalculator
{
    /// <summary>
    /// Calls <see cref="TabTokenCalculator.Calculate" /> and then writes the PrimeNG values into the shared card tab
    /// layer: the card background, the bar padding of each size, the padding of items in the vertical layout, the
    /// gutter between the items and the margins of the add-tab button. <see cref="CalculateCardPadding" /> derives the
    /// padding that is in effect from the card size.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<PrimeNGToken> context, DesignToken global, AbstractCardTabToken token)
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
    /// Returns <c>CardPadding</c>, the padding of the card tab bar for a card of <paramref name="cardSize" />:
    /// <c>UniformlyPadding</c> on the left and right; top and bottom each take half of what is left of the size by one
    /// text line (<c>FontSize</c> times <c>RelativeLineHeight</c>, rounded) and one border width.
    /// </summary>
    public static Thickness CalculateCardPadding(AbstractCardTabToken token, DesignToken global, double cardSize) =>
        new(global.UniformlyPadding,
            (cardSize - Math.Round(global.FontSize * global.RelativeLineHeight)) / 2 - global.LineWidth);
}
