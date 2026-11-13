using AtomUI.Controls;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Algorithms;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract partial class AbstractButtonGeometryToken : AbstractButtonVariantToken
{
    public partial Color BorderColorDisabled { get; set; }
    public partial Thickness ContentPadding { get; set; }
    public partial Thickness ContentPaddingLG { get; set; }
    public partial Thickness ContentPaddingSM { get; set; }
    public partial Thickness CirclePadding { get; set; }
    public partial double OnlyIconSize { get; set; }
    public partial double OnlyIconSizeLG { get; set; }
    public partial double OnlyIconSizeSM { get; set; }
    public partial Thickness IconMargin { get; set; }
    public partial Thickness IconEndMargin { get; set; }
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
