using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign values of the shared <see cref="AbstractCardTabToken" /> layer.
/// </summary>
public static class CardTabTokenCalculator
{
    /// <summary>
    /// Assigns the Ant Design values of the card tab layer to <paramref name="token" />: the card background, the
    /// small and large card paddings, the padding of a vertical item, the card gutter and the margins of the
    /// add-tab button. It first runs <see cref="TabTokenCalculator.Calculate" />, so the base tab layer of the same
    /// Token is filled as well.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<AntDesignToken> context, DesignToken global, AbstractCardTabToken token)
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
    /// The padding of a tab card: the global padding on the left and right. The vertical padding centers one line
    /// of global text in a card of height <paramref name="cardSize" />, less the border width.
    /// </summary>
    /// <param name="token">
    /// Not read.
    /// </param>
    /// <param name="global">
    /// The global tokens the padding, the text line height and the border width come from.
    /// </param>
    /// <param name="cardSize">
    /// The height of the tab card. The line of text has the font size times the relative line height, rounded.
    /// </param>
    public static Thickness CalculateCardPadding(AbstractCardTabToken token, DesignToken global, double cardSize) =>
        new(global.UniformlyPadding,
            (cardSize - Math.Round(global.FontSize * global.RelativeLineHeight)) / 2 - global.LineWidth);
}
