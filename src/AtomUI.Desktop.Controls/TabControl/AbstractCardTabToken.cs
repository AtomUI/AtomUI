using AtomUI.Theme.DesignTokens;
using AtomUI.Media;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract partial class AbstractCardTabToken : AbstractTabToken
{
    public partial Color CardBg { get; set; }
    public partial double CardSize { get; set; }
    public partial Thickness CardPadding { get; set; }
    public partial Thickness CardPaddingSM { get; set; }
    public partial Thickness CardPaddingLG { get; set; }
    public partial Thickness VerticalItemPadding { get; set; }
    public partial double CardGutter { get; set; }
    public partial Thickness AddTabButtonMarginHorizontal { get; set; }
    public partial Thickness AddTabButtonMarginVertical { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        CardBg = EffectiveGlobalToken.ColorFillAlter;
        CardPaddingSM = new Thickness(EffectiveGlobalToken.UniformlyPadding, EffectiveGlobalToken.UniformlyPaddingXXS * 1.5);
        CardPaddingLG = new Thickness(top: EffectiveGlobalToken.UniformlyPaddingXS,
            bottom: EffectiveGlobalToken.UniformlyPaddingXXS * 1.5,
            left: EffectiveGlobalToken.UniformlyPadding,
            right: EffectiveGlobalToken.UniformlyPadding);
        VerticalItemPadding = new Thickness(EffectiveGlobalToken.UniformlyPaddingXS, EffectiveGlobalToken.UniformlyPaddingXS);
        CardGutter = EffectiveGlobalToken.UniformlyMarginXXS / 2;
        AddTabButtonMarginHorizontal = new Thickness(EffectiveGlobalToken.UniformlyMarginXXS, 0, 0, 0);
        AddTabButtonMarginVertical = new Thickness(0, EffectiveGlobalToken.UniformlyMarginXXS, 0, 0);
    }

    protected Thickness CalculateCardPadding(double cardSize) =>
        new(EffectiveGlobalToken.UniformlyPadding,
            (cardSize - Math.Round(EffectiveGlobalToken.FontSize * EffectiveGlobalToken.RelativeLineHeight)) / 2 - EffectiveGlobalToken.LineWidth);
}
