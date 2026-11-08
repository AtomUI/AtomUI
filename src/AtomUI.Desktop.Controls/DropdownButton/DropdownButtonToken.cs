using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class DropdownButtonToken : AbstractControlDesignToken
{
    public Color BorderColorDisabled { get; set; }
    public Thickness ContentPadding { get; set; }
    public Thickness ContentPaddingLG { get; set; }
    public Thickness ContentPaddingSM { get; set; }
    public Thickness ExtraContentMargin { get; set; }
    public Thickness ExtraContentMarginLG { get; set; }
    public Thickness ExtraContentMarginSM { get; set; }
    public Thickness CirclePadding { get; set; }
    public double OnlyIconSize { get; set; }
    public double OnlyIconSizeLG { get; set; }
    public double OnlyIconSizeSM { get; set; }
    public Thickness IconMargin { get; set; }
    public Thickness IconEndMargin { get; set; }
    public double ContentFontSize { get; set; }
    public double ContentFontSizeLG { get; set; }
    public double ContentFontSizeSM { get; set; }
    public double ContentLineHeight { get; set; }
    public double ContentLineHeightLG { get; set; }
    public double ContentLineHeightSM { get; set; }
    public double MarginToAnchor { get; set; }
    public Thickness IconOnyPadding { get; set; }
    public Thickness IconOnyPaddingLG { get; set; }
    public Thickness IconOnyPaddingSM { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);

        var fontSize = EffectiveGlobalToken.FontSize;
        var fontSizeLG = EffectiveGlobalToken.FontSizeLG;
        if (!IsExplicit(DropdownButtonTokenKind.ContentFontSize))
        {
            ContentFontSize = fontSize;
        }

        if (!IsExplicit(DropdownButtonTokenKind.ContentFontSizeSM))
        {
            ContentFontSizeSM = fontSize;
        }

        if (!IsExplicit(DropdownButtonTokenKind.ContentFontSizeLG))
        {
            ContentFontSizeLG = fontSizeLG;
        }

        if (!IsExplicit(DropdownButtonTokenKind.ContentLineHeight))
        {
            ContentLineHeight = CalculateLineHeight(ContentFontSize);
        }

        if (!IsExplicit(DropdownButtonTokenKind.ContentLineHeightSM))
        {
            ContentLineHeightSM = CalculateLineHeight(ContentFontSizeSM);
        }

        if (!IsExplicit(DropdownButtonTokenKind.ContentLineHeightLG))
        {
            ContentLineHeightLG = CalculateLineHeight(ContentFontSizeLG);
        }

        var lineWidth = EffectiveGlobalToken.LineWidth;
        var controlHeightSM = EffectiveGlobalToken.ControlHeightSM;
        var controlHeight = EffectiveGlobalToken.ControlHeight;
        var controlHeightLG = EffectiveGlobalToken.ControlHeightLG;

        if (!IsExplicit(DropdownButtonTokenKind.ContentPaddingSM))
        {
            ContentPaddingSM = new Thickness(
                8 - lineWidth,
                Math.Max((controlHeightSM - ContentLineHeightSM) / 2 - lineWidth, 0));
        }

        if (!IsExplicit(DropdownButtonTokenKind.ContentPadding))
        {
            ContentPadding = new Thickness(
                EffectiveGlobalToken.PaddingContentHorizontal - lineWidth,
                Math.Max((controlHeight - ContentLineHeight) / 2 - lineWidth, 0));
        }

        if (!IsExplicit(DropdownButtonTokenKind.ContentPaddingLG))
        {
            ContentPaddingLG = new Thickness(
                EffectiveGlobalToken.PaddingContentHorizontal - lineWidth,
                Math.Max((controlHeightLG - ContentLineHeightLG) / 2 - lineWidth, 0));
        }

        ExtraContentMarginSM = new Thickness(ContentPaddingSM.Left / 2, 0, 0, 0);
        ExtraContentMargin = new Thickness(ContentPadding.Left / 2, 0, 0, 0);
        ExtraContentMarginLG = new Thickness(ContentPaddingLG.Left / 2, 0, 0, 0);
        CirclePadding = new Thickness(ContentPaddingSM.Left / 2);
        OnlyIconSizeSM = EffectiveGlobalToken.IconSize;
        OnlyIconSize = EffectiveGlobalToken.IconSizeLG;
        OnlyIconSizeLG = EffectiveGlobalToken.IconSizeLG;
        IconMargin = new Thickness(0, 0, EffectiveGlobalToken.UniformlyPaddingXXS, 0);
        IconEndMargin = new Thickness(EffectiveGlobalToken.UniformlyPaddingXXS, 0, 0, 0);
        IconOnyPadding = UniformIconPadding(controlHeight, ContentLineHeight, lineWidth);
        IconOnyPaddingLG = UniformIconPadding(controlHeightLG, ContentLineHeightLG, lineWidth);
        IconOnyPaddingSM = UniformIconPadding(controlHeightSM, ContentLineHeightSM, lineWidth);
        BorderColorDisabled = EffectiveGlobalToken.ColorBorder;
        MarginToAnchor = EffectiveGlobalToken.UniformlyMarginXXS;
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
