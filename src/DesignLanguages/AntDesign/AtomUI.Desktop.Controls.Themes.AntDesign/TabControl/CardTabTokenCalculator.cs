using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign values of the shared <see cref="AbstractCardTabToken"/> layer.</summary>
public static class CardTabTokenCalculator
{
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


    public static Thickness CalculateCardPadding(AbstractCardTabToken token, DesignToken global, double cardSize) =>
        new(global.UniformlyPadding,
            (cardSize - Math.Round(global.FontSize * global.RelativeLineHeight)) / 2 - global.LineWidth);
}
