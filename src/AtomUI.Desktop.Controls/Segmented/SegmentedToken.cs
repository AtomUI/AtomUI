using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class SegmentedToken : AbstractControlDesignToken, IControlTokenDefaults
{

    public SegmentedToken()

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

    [ControlTokenDefault(nameof(ItemColor))]
    private static Color DefaultForItemColor(in ControlTokenDefaultContext context) => context.Global.ColorTextLabel;

    [ControlTokenDefault(nameof(ItemHoverColor))]
    private static Color DefaultForItemHoverColor(in ControlTokenDefaultContext context) => context.Global.ColorText;

    [ControlTokenDefault(nameof(ItemHoverBg))]
    private static Color DefaultForItemHoverBg(in ControlTokenDefaultContext context) => context.Global.ColorFillSecondary;

    [ControlTokenDefault(nameof(ItemActiveBg))]
    private static Color DefaultForItemActiveBg(in ControlTokenDefaultContext context) => context.Global.ColorFill;

    [ControlTokenDefault(nameof(ItemSelectedBg))]
    private static Color DefaultForItemSelectedBg(in ControlTokenDefaultContext context) => context.Global.ColorBgElevated;

    [ControlTokenDefault(nameof(ItemSelectedColor))]
    private static Color DefaultForItemSelectedColor(in ControlTokenDefaultContext context) => context.Global.ColorText;

    [ControlTokenDefault(nameof(TrackPadding))]
    private static Thickness DefaultForTrackPadding(in ControlTokenDefaultContext context) =>
        new(context.Global.LineWidthBold);

    [ControlTokenDefault(nameof(ItemMinHeightLG), nameof(TrackPadding))]
    private static double DefaultForItemMinHeightLG(in ControlTokenDefaultContext context, Thickness trackPadding) =>
        Math.Max(context.Global.ControlHeightLG - trackPadding.Top - trackPadding.Bottom, 0);

    [ControlTokenDefault(nameof(ItemMinHeight), nameof(TrackPadding))]
    private static double DefaultForItemMinHeight(in ControlTokenDefaultContext context, Thickness trackPadding) =>
        Math.Max(context.Global.ControlHeight - trackPadding.Top - trackPadding.Bottom, 0);

    [ControlTokenDefault(nameof(ItemMinHeightSM), nameof(TrackPadding))]
    private static double DefaultForItemMinHeightSM(in ControlTokenDefaultContext context, Thickness trackPadding) =>
        Math.Max(context.Global.ControlHeightSM - trackPadding.Top - trackPadding.Bottom, 0);

    [ControlTokenDefault(nameof(TrackBg))]
    private static Color DefaultForTrackBg(in ControlTokenDefaultContext context) => context.Global.ColorBgLayout;

    [ControlTokenDefault(nameof(SegmentedItemPadding))]
    private static Thickness DefaultForSegmentedItemPadding(in ControlTokenDefaultContext context) =>
        new(Math.Max(context.Global.ControlPaddingHorizontal - context.Global.LineWidth, 0), 0,
            Math.Max(context.Global.ControlPaddingHorizontal - context.Global.LineWidth, 0), 0);

    [ControlTokenDefault(nameof(SegmentedItemPaddingSM))]
    private static Thickness DefaultForSegmentedItemPaddingSM(in ControlTokenDefaultContext context) =>
        new(Math.Max(context.Global.ControlPaddingHorizontalSM - context.Global.LineWidth, 0), 0,
            Math.Max(context.Global.ControlPaddingHorizontalSM - context.Global.LineWidth, 0), 0);

    [ControlTokenDefault(nameof(SegmentedItemContentMargin))]
    private static Thickness DefaultForSegmentedItemContentMargin(in ControlTokenDefaultContext context) =>
        new(context.Global.UniformlyPaddingXXS, 0, 0, 0);
    
}
