using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class SelectToken : AbstractSelectInputToken
{







    /// <summary>
    /// 选项选中时文本颜色
    /// Text color when option is selected
    /// </summary>
    public partial Color OptionSelectedColor { get; set; }

    /// <summary>
    /// 选项选中时文本字重
    /// Font weight when option is selected
    /// </summary>
    public partial FontWeight OptionSelectedFontWeight { get; set; }

    /// <summary>
    /// 选项选中时背景色
    /// Font weight when option is selected
    /// </summary>
    public partial Color OptionSelectedBg { get; set; }

    /// <summary>
    /// 选项激活态时背景色
    /// Background color when option is active
    /// </summary>
    public partial Color OptionActiveBg { get; set; }

    /// <summary>
    /// 选项内间距
    /// Padding of option
    /// </summary>
    public partial Thickness OptionPadding { get; set; }


    /// <summary>
    /// 选项高度
    /// Height of option
    /// </summary>
    public partial double OptionHeight { get; set; }

    /// <summary>
    /// 前后缀内间距
    /// Padding of the select affix (prefix / suffix)
    /// </summary>
    [ReservedToken("antd Select selectAffixPadding. The prefix is aligned by MultiModePrefixIndent* and the suffix sits " +
                   "inside the input frame padding, so no separate affix padding is applied yet.")]
    public partial Thickness SelectAffixPadding { get; set; }

    /// <summary>
    /// 多选标签固定外边距
    /// Fixed margin of multiple-mode tags
    /// </summary>
    [ReservedToken("antd Select INTERNAL_FIXED_ITEM_MARGIN (floor(paddingXXS / 2)). Multiple-mode tags are spaced by " +
                   "the tag panels (SpacingXXS) and sized by MultipleItemHeight*, so no tag margin is applied yet.")]
    public partial Thickness FixedItemMargin { get; set; }

    /// <summary>
    /// 菜单内容边距
    /// </summary>
    public partial Thickness PopupContentPadding { get; set; }










    public SelectToken()

    {
    }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        OptionSelectedColor      = EffectiveGlobalToken.ColorText;
        OptionSelectedFontWeight = EffectiveGlobalToken.FontWeightStrong;
        OptionSelectedBg         = EffectiveGlobalToken.ControlItemBgActive;
        OptionActiveBg           = EffectiveGlobalToken.ControlItemBgHover;
        OptionPadding            = new Thickness(EffectiveGlobalToken.ControlPaddingHorizontal, (EffectiveGlobalToken.ControlHeight - EffectiveGlobalToken.FontHeight) / 2);
        OptionHeight = EffectiveGlobalToken.ControlHeight;
        SelectAffixPadding = EffectiveGlobalToken.PaddingXXS;
        FixedItemMargin = new Thickness(Math.Floor(EffectiveGlobalToken.UniformlyPaddingXXS / 2));
        PopupContentPadding = new Thickness(EffectiveGlobalToken.UniformlyPaddingXXS / 2);
    }
}
