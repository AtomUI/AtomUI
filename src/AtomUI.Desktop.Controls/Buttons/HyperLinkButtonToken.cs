using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class HyperLinkButtonToken : AbstractControlDesignToken
{
    public partial Thickness ContentPadding { get; set; }
    public partial Thickness ContentPaddingLG { get; set; }
    public partial Thickness ContentPaddingSM { get; set; }
    public partial double OnlyIconSize { get; set; }
    public partial double OnlyIconSizeLG { get; set; }
    public partial double OnlyIconSizeSM { get; set; }
    public partial Thickness IconMargin { get; set; }
    public partial double ContentFontSize { get; set; }
    public partial double ContentFontSizeLG { get; set; }
    public partial double ContentFontSizeSM { get; set; }
    public partial double ContentLineHeight { get; set; }
    public partial double ContentLineHeightLG { get; set; }
    public partial double ContentLineHeightSM { get; set; }
    public partial Thickness IconOnyPadding { get; set; }
    public partial Thickness IconOnyPaddingLG { get; set; }
    public partial Thickness IconOnyPaddingSM { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);

        var fontSize = EffectiveGlobalToken.FontSize;
        var fontSizeLG = EffectiveGlobalToken.FontSizeLG;
        if (!IsExplicit(HyperLinkButtonTokenKind.ContentFontSize))
        {
            ContentFontSize = fontSize;
        }

        if (!IsExplicit(HyperLinkButtonTokenKind.ContentFontSizeSM))
        {
            ContentFontSizeSM = fontSize;
        }

        if (!IsExplicit(HyperLinkButtonTokenKind.ContentFontSizeLG))
        {
            ContentFontSizeLG = fontSizeLG;
        }

        if (!IsExplicit(HyperLinkButtonTokenKind.ContentLineHeight))
        {
            ContentLineHeight = CalculateLineHeight(ContentFontSize);
        }

        if (!IsExplicit(HyperLinkButtonTokenKind.ContentLineHeightSM))
        {
            ContentLineHeightSM = CalculateLineHeight(ContentFontSizeSM);
        }

        if (!IsExplicit(HyperLinkButtonTokenKind.ContentLineHeightLG))
        {
            ContentLineHeightLG = CalculateLineHeight(ContentFontSizeLG);
        }

        var lineWidth = EffectiveGlobalToken.LineWidth;
        var controlHeightSM = EffectiveGlobalToken.ControlHeightSM;
        var controlHeight = EffectiveGlobalToken.ControlHeight;
        var controlHeightLG = EffectiveGlobalToken.ControlHeightLG;
        ContentPaddingSM = new Thickness(
            8 - lineWidth,
            Math.Max((controlHeightSM - ContentLineHeightSM) / 2 - lineWidth, 0));
        ContentPadding = new Thickness(
            EffectiveGlobalToken.PaddingContentHorizontal - lineWidth,
            Math.Max((controlHeight - ContentLineHeight) / 2 - lineWidth, 0));
        ContentPaddingLG = new Thickness(
            EffectiveGlobalToken.PaddingContentHorizontal - lineWidth,
            Math.Max((controlHeightLG - ContentLineHeightLG) / 2 - lineWidth, 0));

        OnlyIconSizeSM = EffectiveGlobalToken.IconSize;
        OnlyIconSize = EffectiveGlobalToken.IconSizeLG;
        OnlyIconSizeLG = EffectiveGlobalToken.IconSizeLG;
        IconMargin = new Thickness(0, 0, EffectiveGlobalToken.UniformlyPaddingXXS, 0);
        IconOnyPadding = UniformIconPadding(controlHeight, ContentLineHeight, lineWidth);
        IconOnyPaddingLG = UniformIconPadding(controlHeightLG, ContentLineHeightLG, lineWidth);
        IconOnyPaddingSM = UniformIconPadding(controlHeightSM, ContentLineHeightSM, lineWidth);
    }

    private static double CalculateLineHeight(double fontSize)
    {
        return CalculatorUtils.CalculateLineHeight(fontSize) * fontSize;
    }

    private static Thickness UniformIconPadding(double height, double lineHeight, double lineWidth)
    {
        return new Thickness(Math.Max((height - lineHeight) / 2 - lineWidth, 0));
    }
}
