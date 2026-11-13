using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class SegmentedToken : AbstractControlDesignToken
{

    public SegmentedToken()

    {
    }

    /// <summary>
    /// 选项文本颜色
    /// </summary>
    public partial Color ItemColor { get; set; }

    /// <summary>
    /// 选项悬浮态文本颜色
    /// </summary>
    public partial Color ItemHoverColor { get; set; }

    /// <summary>
    /// 选项悬浮态背景颜色
    /// </summary>
    public partial Color ItemHoverBg { get; set; }

    /// <summary>
    /// 选项激活态背景颜色
    /// </summary>
    public partial Color ItemActiveBg { get; set; }

    /// <summary>
    /// 选项选中时背景颜色
    /// </summary>
    public partial Color ItemSelectedBg { get; set; }

    /// <summary>
    /// 选项选中时文字颜色
    /// </summary>
    public partial Color ItemSelectedColor { get; set; }

    /// <summary>
    /// Segmented 控件容器的 padding
    /// </summary>
    public partial Thickness TrackPadding { get; set; }

    /// <summary>
    /// 大尺寸选项最小高度
    /// </summary>
    public partial double ItemMinHeightLG { get; set; }

    /// <summary>
    /// 选项最小高度
    /// </summary>
    public partial double ItemMinHeight { get; set; }

    /// <summary>
    /// 小尺寸选项最小高度
    /// </summary>
    public partial double ItemMinHeightSM { get; set; }

    /// <summary>
    /// Segmented 控件容器背景色
    /// </summary>
    public partial Color TrackBg { get; set; }

    // 内部 token
    public partial Thickness SegmentedItemPadding { get; set; }
    public partial Thickness SegmentedItemPaddingSM { get; set; }
    public partial Thickness SegmentedItemContentMargin { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);

        if (!IsExplicit(SegmentedTokenKind.TrackPadding))
        {
            TrackPadding      = new Thickness(EffectiveGlobalToken.LineWidthBold);
        }

        TrackBg           = EffectiveGlobalToken.ColorBgLayout;
        ItemColor         = EffectiveGlobalToken.ColorTextLabel;
        ItemHoverColor    = EffectiveGlobalToken.ColorText;
        ItemHoverBg       = EffectiveGlobalToken.ColorFillSecondary;
        ItemSelectedBg    = EffectiveGlobalToken.ColorBgElevated;
        ItemActiveBg      = EffectiveGlobalToken.ColorFill;
        ItemSelectedColor = EffectiveGlobalToken.ColorText;
        var lineWidth = EffectiveGlobalToken.LineWidth;
        SegmentedItemPadding = new Thickness(
            Math.Max(EffectiveGlobalToken.ControlPaddingHorizontal - lineWidth, 0),
            0,
            Math.Max(EffectiveGlobalToken.ControlPaddingHorizontal - lineWidth, 0),
            0);
        SegmentedItemPaddingSM = new Thickness(
            Math.Max(EffectiveGlobalToken.ControlPaddingHorizontalSM - lineWidth, 0),
            0,
            Math.Max(EffectiveGlobalToken.ControlPaddingHorizontalSM - lineWidth, 0),
            0);
        SegmentedItemContentMargin = new Thickness(EffectiveGlobalToken.UniformlyPaddingXXS, 0, 0, 0);

        ItemMinHeightLG = EffectiveGlobalToken.ControlHeightLG - TrackPadding.Top - TrackPadding.Bottom;
        ItemMinHeight   = EffectiveGlobalToken.ControlHeight - TrackPadding.Top - TrackPadding.Bottom;
        ItemMinHeightSM = EffectiveGlobalToken.ControlHeightSM - TrackPadding.Top - TrackPadding.Bottom;
    }
    
}
