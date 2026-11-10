using AtomUI.Controls;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Algorithms;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal abstract class AbstractButtonGeometryToken : AbstractButtonVariantToken
{
    public Color BorderColorDisabled { get; set; }
    public Thickness ContentPadding { get; set; }
    public Thickness ContentPaddingLG { get; set; }
    public Thickness ContentPaddingSM { get; set; }
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
    public Thickness IconOnyPadding { get; set; }
    public Thickness IconOnyPaddingLG { get; set; }
    public Thickness IconOnyPaddingSM { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        BorderColorDisabled = EffectiveGlobalToken.ColorBorder;
        OnlyIconSizeSM = EffectiveGlobalToken.IconSize;
        OnlyIconSize = EffectiveGlobalToken.IconSizeLG;
        OnlyIconSizeLG = EffectiveGlobalToken.IconSizeLG;
        IconMargin = new Thickness(0, 0, EffectiveGlobalToken.UniformlyPaddingXXS, 0);
        IconEndMargin = new Thickness(EffectiveGlobalToken.UniformlyPaddingXXS, 0, 0, 0);
    }

    protected static double CalculateContentLineHeight(double fontSize) => CalculatorUtils.CalculateLineHeight(fontSize) * fontSize;

    protected Thickness CalculateContentPadding(double lineHeight, CustomizableSizeType size)
    {
        var horizontal = size == CustomizableSizeType.Small ? 8 : EffectiveGlobalToken.PaddingContentHorizontal;
        var height = GetControlHeight(size);
        return new Thickness(horizontal - EffectiveGlobalToken.LineWidth,
            Math.Max((height - lineHeight) / 2 - EffectiveGlobalToken.LineWidth, 0));
    }

    protected Thickness CalculateIconPadding(double lineHeight, CustomizableSizeType size) =>
        new(Math.Max((GetControlHeight(size) - lineHeight) / 2 - EffectiveGlobalToken.LineWidth, 0));

    private double GetControlHeight(CustomizableSizeType size) => size switch
    {
        CustomizableSizeType.Small => EffectiveGlobalToken.ControlHeightSM,
        CustomizableSizeType.Large => EffectiveGlobalToken.ControlHeightLG,
        _ => EffectiveGlobalToken.ControlHeight
    };
}
