using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract partial class AbstractSelectInputToken : AbstractControlDesignToken
{

    /// <summary>
    /// 多选标签背景色
    /// Background color of multiple tag
    /// </summary>
    public partial Color MultipleItemBg { get; set; }

    /// <summary>
    /// 多选标签高度
    /// Height of multiple tag
    /// </summary>
    public partial double MultipleItemHeight { get; set; }

    /// <summary>
    /// 小号多选标签高度
    /// Height of multiple tag with small size
    /// </summary>
    public partial double MultipleItemHeightSM { get; set; }

    /// <summary>
    /// 大号多选标签高度
    /// Height of multiple tag with large size
    /// </summary>
    public partial double MultipleItemHeightLG { get; set; }

    /// <summary>
    /// 多选框禁用背景
    /// Background color of multiple selector when disabled
    /// </summary>
    public partial Color MultipleSelectorBgDisabled { get; set; }

    /// <summary>
    /// 多选标签禁用文本颜色
    /// Text color of multiple tag when disabled
    /// </summary>
    public partial Color MultipleItemColorDisabled { get; set; }






    /// <summary>
    /// 选项字体大小
    /// Font size of option
    /// </summary>
    public partial double OptionFontSize { get; set; }





    /// <summary>
    /// 多选模式下的输入框内边距
    /// </summary>
    public partial Thickness MultiModePadding { get; set; }

    /// <summary>
    /// 多选模式下的小号输入框内边距
    /// </summary>
    public partial Thickness MultiModePaddingSM { get; set; }

    /// <summary>
    /// 多选模式下的大号输入框内边距
    /// </summary>
    public partial Thickness MultiModePaddingLG { get; set; }

    /// <summary>
    /// 输入框内边距
    /// </summary>
    public partial Thickness SingleModePadding { get; set; }

    /// <summary>
    /// 小号输入框内边距
    /// </summary>
    public partial Thickness SingleModePaddingSM { get; set; }

    /// <summary>
    /// 多选模式下的大号输入框内边距
    /// </summary>
    public partial Thickness SingleModePaddingLG { get; set; }

    /// <summary>
    /// 多选模式下前缀（ContentLeftAddOn）的额外左缩进，
    /// 补偿多选内边距与单选水平内边距的差值，使前缀与单选模式左对齐
    /// </summary>
    public partial Thickness MultiModePrefixIndent { get; set; }

    /// <summary>
    /// 多选模式下的小号前缀额外左缩进
    /// </summary>
    public partial Thickness MultiModePrefixIndentSM { get; set; }

    /// <summary>
    /// 多选模式下的大号前缀额外左缩进
    /// </summary>
    public partial Thickness MultiModePrefixIndentLG { get; set; }

protected override void CalculateTokenValues(bool isDarkMode)
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

        OptionFontSize =  EffectiveGlobalToken.FontSize;
        MultipleItemBg = EffectiveGlobalToken.ColorFillSecondary;
        MultipleItemHeight = multipleItemHeight - 2;
        MultipleItemHeightSM = multipleItemHeightSM + 4;
        MultipleItemHeightLG = multipleItemHeightLG;
        MultipleSelectorBgDisabled = EffectiveGlobalToken.ColorBgContainerDisabled;
        MultipleItemColorDisabled = EffectiveGlobalToken.ColorTextDisabled;


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
