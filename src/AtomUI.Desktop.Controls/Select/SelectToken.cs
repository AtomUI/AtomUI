using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class SelectToken : AbstractControlDesignToken
{

    /// <summary>
    /// 多选标签背景色
    /// Background color of multiple tag
    /// </summary>
    public Color MultipleItemBg { get; set; }

    /// <summary>
    /// 多选标签高度
    /// Height of multiple tag
    /// </summary>
    public double MultipleItemHeight { get; set; }

    /// <summary>
    /// 小号多选标签高度
    /// Height of multiple tag with small size
    /// </summary>
    public double MultipleItemHeightSM { get; set; }

    /// <summary>
    /// 大号多选标签高度
    /// Height of multiple tag with large size
    /// </summary>
    public double MultipleItemHeightLG { get; set; }

    /// <summary>
    /// 多选框禁用背景
    /// Background color of multiple selector when disabled
    /// </summary>
    public Color MultipleSelectorBgDisabled { get; set; }

    /// <summary>
    /// 多选标签禁用文本颜色
    /// Text color of multiple tag when disabled
    /// </summary>
    public Color MultipleItemColorDisabled { get; set; }

    /// <summary>
    /// 选项选中时文本颜色
    /// Text color when option is selected
    /// </summary>
    public Color OptionSelectedColor { get; set; }

    /// <summary>
    /// 选项选中时文本字重
    /// Font weight when option is selected
    /// </summary>
    public FontWeight OptionSelectedFontWeight { get; set; }

    /// <summary>
    /// 选项选中时背景色
    /// Font weight when option is selected
    /// </summary>
    public Color OptionSelectedBg { get; set; }

    /// <summary>
    /// 选项激活态时背景色
    /// Background color when option is active
    /// </summary>
    public Color OptionActiveBg { get; set; }

    /// <summary>
    /// 选项内间距
    /// Padding of option
    /// </summary>
    public Thickness OptionPadding { get; set; }

    /// <summary>
    /// 选项字体大小
    /// Font size of option
    /// </summary>
    public double OptionFontSize { get; set; }

    /// <summary>
    /// 选项高度
    /// Height of option
    /// </summary>
    public double OptionHeight { get; set; }

    /// <summary>
    /// 前后缀内间距
    /// Padding of the select affix (prefix / suffix)
    /// </summary>
    [ReservedToken("antd Select selectAffixPadding. The prefix is aligned by MultiModePrefixIndent* and the suffix sits " +
                   "inside the input frame padding, so no separate affix padding is applied yet.")]
    public Thickness SelectAffixPadding { get; set; }

    /// <summary>
    /// 多选标签固定外边距
    /// Fixed margin of multiple-mode tags
    /// </summary>
    [ReservedToken("antd Select INTERNAL_FIXED_ITEM_MARGIN (floor(paddingXXS / 2)). Multiple-mode tags are spaced by " +
                   "the tag panels (SpacingXXS) and sized by MultipleItemHeight*, so no tag margin is applied yet.")]
    public Thickness FixedItemMargin { get; set; }

    /// <summary>
    /// 菜单内容边距
    /// </summary>
    public Thickness PopupContentPadding { get; set; }

    /// <summary>
    /// 多选模式下的输入框内边距
    /// </summary>
    public Thickness MultiModePadding { get; set; }

    /// <summary>
    /// 多选模式下的小号输入框内边距
    /// </summary>
    public Thickness MultiModePaddingSM { get; set; }

    /// <summary>
    /// 多选模式下的大号输入框内边距
    /// </summary>
    public Thickness MultiModePaddingLG { get; set; }

    /// <summary>
    /// 输入框内边距
    /// </summary>
    public Thickness SingleModePadding { get; set; }

    /// <summary>
    /// 小号输入框内边距
    /// </summary>
    public Thickness SingleModePaddingSM { get; set; }

    /// <summary>
    /// 多选模式下的大号输入框内边距
    /// </summary>
    public Thickness SingleModePaddingLG { get; set; }

    /// <summary>
    /// 多选模式下前缀（ContentLeftAddOn）的额外左缩进，
    /// 补偿多选内边距与单选水平内边距的差值，使前缀与单选模式左对齐
    /// </summary>
    public Thickness MultiModePrefixIndent { get; set; }

    /// <summary>
    /// 多选模式下的小号前缀额外左缩进
    /// </summary>
    public Thickness MultiModePrefixIndentSM { get; set; }

    /// <summary>
    /// 多选模式下的大号前缀额外左缩进
    /// </summary>
    public Thickness MultiModePrefixIndentLG { get; set; }

    public SelectToken()

    {
    }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);

        // Item height default use `controlHeight - 2 * paddingXXS`,
        // but some case `paddingXXS=0`.
        // Let's fallback it.
        double dblPaddingXXS      = EffectiveGlobalToken.UniformlyPaddingXXS * 2;
        double dblLineWidth       = EffectiveGlobalToken.LineWidth * 2;
        double multipleItemHeight = Math.Min(EffectiveGlobalToken.ControlHeight - dblPaddingXXS, EffectiveGlobalToken.ControlHeight - dblLineWidth);
        double multipleItemHeightSM = Math.Min(EffectiveGlobalToken.ControlHeightSM - dblPaddingXXS, EffectiveGlobalToken.ControlHeightSM - dblLineWidth);
        double multipleItemHeightLG = Math.Min(EffectiveGlobalToken.ControlHeightLG - dblPaddingXXS, EffectiveGlobalToken.ControlHeightLG - dblLineWidth);
        FixedItemMargin = new Thickness(Math.Floor(EffectiveGlobalToken.UniformlyPaddingXXS / 2));

        OptionSelectedColor      = EffectiveGlobalToken.ColorText;
        OptionSelectedFontWeight = EffectiveGlobalToken.FontWeightStrong;
        OptionSelectedBg         = EffectiveGlobalToken.ControlItemBgActive;
        OptionActiveBg           = EffectiveGlobalToken.ControlItemBgHover;
        OptionPadding            = new Thickness(EffectiveGlobalToken.ControlPaddingHorizontal, (EffectiveGlobalToken.ControlHeight - EffectiveGlobalToken.FontHeight) / 2);
        OptionFontSize =  EffectiveGlobalToken.FontSize;
        OptionHeight = EffectiveGlobalToken.ControlHeight;
        MultipleItemBg = EffectiveGlobalToken.ColorFillSecondary;
        MultipleItemHeight = multipleItemHeight - 2;
        MultipleItemHeightSM = multipleItemHeightSM + 4;
        MultipleItemHeightLG = multipleItemHeightLG;
        MultipleSelectorBgDisabled = EffectiveGlobalToken.ColorBgContainerDisabled;
        MultipleItemColorDisabled = EffectiveGlobalToken.ColorTextDisabled;
        SelectAffixPadding = EffectiveGlobalToken.PaddingXXS;

        PopupContentPadding = new Thickness(EffectiveGlobalToken.UniformlyPaddingXXS / 2);

        var lineWidth    = EffectiveGlobalToken.LineWidth;

        var multiPaddingVertical = Math.Round((EffectiveGlobalToken.ControlHeight - EffectiveGlobalToken.FontHeight) / 2 * 10) / 10 - lineWidth;
        var multiPaddingVerticalSM = Math.Round((EffectiveGlobalToken.ControlHeightSM - EffectiveGlobalToken.FontHeight) / 2 * 10) / 10 - lineWidth;
        var multiPaddingVerticalLG = Math.Ceiling((EffectiveGlobalToken.ControlHeightLG - EffectiveGlobalToken.FontHeightLG) / 2 * 10) / 10 -
                                     lineWidth;

        var multiPaddingRight = EffectiveGlobalToken.UniformlyPaddingSM - lineWidth;
        MultiModePadding = new Thickness(multiPaddingVertical, multiPaddingVertical, multiPaddingRight, multiPaddingVertical);
        MultiModePrefixIndent = new Thickness(Math.Max(0, multiPaddingRight - multiPaddingVertical), 0, 0, 0);

        var multiPaddingRightSM = EffectiveGlobalToken.ControlPaddingHorizontalSM - lineWidth;
        MultiModePaddingSM = new Thickness(multiPaddingVerticalSM, multiPaddingVerticalSM, multiPaddingRightSM, multiPaddingVerticalSM);
        MultiModePrefixIndentSM = new Thickness(Math.Max(0, multiPaddingRightSM - multiPaddingVerticalSM), 0, 0, 0);

        var multiPaddingRightLG = EffectiveGlobalToken.ControlPaddingHorizontal - lineWidth;
        MultiModePaddingLG = new Thickness(multiPaddingVerticalLG, multiPaddingVerticalLG, multiPaddingRightLG, multiPaddingVerticalLG);
        MultiModePrefixIndentLG = new Thickness(Math.Max(0, multiPaddingRightLG - multiPaddingVerticalLG), 0, 0, 0);

        SingleModePadding   = new Thickness(multiPaddingRight, multiPaddingVertical);
        SingleModePaddingSM = new Thickness(multiPaddingRightSM, multiPaddingVerticalSM);
        SingleModePaddingLG = new Thickness(multiPaddingRightLG,  multiPaddingVerticalLG);
    }

}
