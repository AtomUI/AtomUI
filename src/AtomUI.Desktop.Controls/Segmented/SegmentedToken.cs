using AtomUI.Theme.TokenSystem;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class SegmentedToken : AbstractControlDesignToken
{
    public const string ID = "Segmented";

    public SegmentedToken()
        : base(ID)
    {
    }

    /// <summary>
    /// 选项文本颜色
    /// </summary>
    public Color ItemColor { get; set; }

    /// <summary>
    /// 选项悬浮态文本颜色
    /// </summary>
    public Color ItemHoverColor { get; set; }

    /// <summary>
    /// 选项悬浮态背景颜色
    /// </summary>
    public Color ItemHoverBg { get; set; }

    /// <summary>
    /// 选项激活态背景颜色
    /// </summary>
    public Color ItemActiveBg { get; set; }

    /// <summary>
    /// 选项选中时背景颜色
    /// </summary>
    public Color ItemSelectedBg { get; set; }

    /// <summary>
    /// 选项选中时文字颜色
    /// </summary>
    public Color ItemSelectedColor { get; set; }

    /// <summary>
    /// Segmented 控件容器的 padding
    /// </summary>
    public Thickness TrackPadding { get; set; }

    /// <summary>
    /// 大尺寸选项最小高度
    /// </summary>
    public double ItemMinHeightLG { get; set; }

    /// <summary>
    /// 选项最小高度
    /// </summary>
    public double ItemMinHeight { get; set; }

    /// <summary>
    /// 小尺寸选项最小高度
    /// </summary>
    public double ItemMinHeightSM { get; set; }

    /// <summary>
    /// Segmented 控件容器背景色
    /// </summary>
    public Color TrackBg { get; set; }

    // 内部 token
    public Thickness SegmentedItemPadding { get; set; }
    public Thickness SegmentedItemPaddingSM { get; set; }
    public Thickness SegmentedItemContentMargin { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        TrackPadding      = new Thickness(SharedToken.LineWidthBold);
        TrackBg           = SharedToken.ColorBgLayout;
        ItemColor         = SharedToken.ColorTextLabel;
        ItemHoverColor    = SharedToken.ColorText;
        ItemHoverBg       = SharedToken.ColorFillSecondary;
        ItemSelectedBg    = SharedToken.ColorBgElevated;
        ItemActiveBg      = SharedToken.ColorFill;
        ItemSelectedColor = SharedToken.ColorText;
        var lineWidth = SharedToken.LineWidth;
        SegmentedItemPadding = new Thickness(
            Math.Max(SharedToken.ControlPaddingHorizontal - lineWidth, 0),
            0,
            Math.Max(SharedToken.ControlPaddingHorizontal - lineWidth, 0),
            0);
        SegmentedItemPaddingSM = new Thickness(
            Math.Max(SharedToken.ControlPaddingHorizontalSM - lineWidth, 0),
            0,
            Math.Max(SharedToken.ControlPaddingHorizontalSM - lineWidth, 0),
            0);
        SegmentedItemContentMargin = new Thickness(SharedToken.UniformlyPaddingXXS, 0, 0, 0);

        ItemMinHeightLG = SharedToken.ControlHeightLG - TrackPadding.Top - TrackPadding.Bottom;
        ItemMinHeight   = SharedToken.ControlHeight - TrackPadding.Top - TrackPadding.Bottom;
        ItemMinHeightSM = SharedToken.ControlHeightSM - TrackPadding.Top - TrackPadding.Bottom;
    }
}