using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract partial class AbstractPaginationToken : AbstractControlDesignToken
{
    public partial double ItemSize { get; set; }
    public partial double ItemSizeSM { get; set; }
    public partial Color ItemActiveColorDisabled { get; set; }
    public partial double PaginationLayoutSpacing { get; set; }
    public partial double PaginationLayoutMiniSpacing { get; set; }
    public partial double PaginationQuickJumperInputWidth { get; set; }
    public partial double PaginationMiniQuickJumperInputWidth { get; set; }

    /// <summary>
    /// 页码选项文本颜色
    /// Text color of a Pagination item at rest (the previous and next buttons included)
    /// </summary>
    public partial Color ItemColor { get; set; }

    /// <summary>
    /// 页码选项悬停文本颜色
    /// Text color of a hovered Pagination item
    /// </summary>
    public partial Color ItemHoverColor { get; set; }

    /// <summary>
    /// 页码选项悬停背景色
    /// Background color of a hovered Pagination item
    /// </summary>
    public partial Color ItemHoverBg { get; set; }

    /// <summary>
    /// 页码选项圆角
    /// Corner radius of a Pagination item
    /// </summary>
    public partial CornerRadius ItemBorderRadius { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        ItemSize                  = EffectiveGlobalToken.ControlHeight;
        ItemSizeSM                = EffectiveGlobalToken.ControlHeightSM;
        ItemActiveColorDisabled   = EffectiveGlobalToken.ColorTextDisabled;
        PaginationLayoutSpacing             = EffectiveGlobalToken.UniformlyMarginXS;
        PaginationLayoutMiniSpacing         = Math.Max(EffectiveGlobalToken.UniformlyMarginXXS / 2,
                                                       EffectiveGlobalToken.ControlOutlineWidth);
        PaginationQuickJumperInputWidth     = EffectiveGlobalToken.ControlHeightLG * 1.25;
        PaginationMiniQuickJumperInputWidth = EffectiveGlobalToken.ControlHeightLG * 1.1;
        ItemColor                           = EffectiveGlobalToken.ColorText;
        ItemHoverColor                      = EffectiveGlobalToken.ColorText;
        ItemHoverBg                         = EffectiveGlobalToken.ColorBgTextHover;
        ItemBorderRadius                    = EffectiveGlobalToken.BorderRadius;
    }
}
