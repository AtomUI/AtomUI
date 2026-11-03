using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class NotificationCardToken : AbstractControlDesignToken, IControlTokenDefaults
{

    /// <summary>
    /// 提醒框背景色
    /// </summary>
    public Color NotificationBg { get; set; }

    /// <summary>
    /// 提醒框内边距
    /// </summary>
    public Thickness NotificationPadding { get; set; }

    /// <summary>
    /// 提醒框图标尺寸
    /// </summary>
    public double NotificationIconSize { get; set; }

    /// <summary>
    /// 提醒框图标外边距
    /// </summary>
    public Thickness NotificationIconMargin { get; set; }

    /// <summary>
    /// 提醒框关闭按钮尺寸
    /// </summary>
    public double NotificationCloseButtonSize { get; set; }
    
    /// <summary>
    /// 提醒框关闭按钮内间距
    /// </summary>
    public Thickness NotificationCloseButtonPadding { get; set; }

    /// <summary>
    /// 内容区域标题与描述之间的间距
    /// </summary>
    public double NotificationSectionSpacing { get; set; }

    /// <summary>
    /// 标题与描述的右侧留白，避免文本压到关闭按钮
    /// </summary>
    public Thickness NotificationTitlePadding { get; set; }

    /// <summary>
    /// 操作组的上边距
    /// </summary>
    public Thickness NotificationActionsMargin { get; set; }

    /// <summary>
    /// 关闭按钮由卡片内容区左上角偏移到内容区右上角的覆盖层外边距
    /// </summary>
    public Thickness NotificationCloseButtonMargin { get; set; }

    /// <summary>
    /// 提醒框进度条背景色
    /// </summary>
    public IImmutableBrush? NotificationProgressBg { get; set; }

    /// <summary>
    /// 提醒框进度条底槽色。进度条先以该色铺满整条作为底槽，再叠加彩色进度值；
    /// 缺底槽时剩余时间在卡片背景上不可见。
    /// </summary>
    public Color NotificationProgressTrackBg { get; set; }

    /// <summary>
    /// 提醒框进度条高度
    /// </summary>
    public double NotificationProgressHeight { get; set; }

    /// <summary>
    /// 进度条外边距
    /// </summary>
    public Thickness NotificationProgressMargin { get; set; }

    /// <summary>
    /// 提醒框宽度
    /// </summary>
    public double NotificationWidth { get; set; }


    /// <summary>
    /// 提醒框上边缘外边距
    /// </summary>
    public Thickness NotificationTopMargin { get; set; }

    /// <summary>
    /// 提醒框下边缘外边距
    /// </summary>
    public Thickness NotificationBottomMargin { get; set; }

    [ControlTokenDefault(nameof(NotificationBg))]
    private static Color DefaultForNotificationBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgElevated;

    [ControlTokenDefault(nameof(NotificationPadding))]
    private static Thickness DefaultForNotificationPadding(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyPaddingLG, context.Global.UniformlyPaddingMD, context.Global.UniformlyPaddingLG, context.Global.UniformlyPaddingMD);

    [ControlTokenDefault(nameof(NotificationIconSize))]
    private static double DefaultForNotificationIconSize(in ControlTokenDefaultContext context) =>
        context.Global.FontSizeLG * context.Global.RelativeLineHeightLG;

    [ControlTokenDefault(nameof(NotificationIconMargin))]
    private static Thickness DefaultForNotificationIconMargin(in ControlTokenDefaultContext context) =>
        new Thickness(0, 0, context.Global.UniformlyMarginSM, 0);

    [ControlTokenDefault(nameof(NotificationCloseButtonSize))]
    private static double DefaultForNotificationCloseButtonSize(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeightLG * 0.55;

    [ControlTokenDefault(nameof(NotificationCloseButtonPadding))]
    private static Thickness DefaultForNotificationCloseButtonPadding(in ControlTokenDefaultContext context) =>
        context.Global.PaddingXXS;

    [ControlTokenDefault(nameof(NotificationSectionSpacing))]
    private static double DefaultForNotificationSectionSpacing(in ControlTokenDefaultContext context) =>
        context.Global.UniformlyMarginXS;

    [ControlTokenDefault(nameof(NotificationTitlePadding))]
    private static Thickness DefaultForNotificationTitlePadding(in ControlTokenDefaultContext context) =>
        new Thickness(0, 0, context.Global.UniformlyPaddingLG, 0);

    [ControlTokenDefault(nameof(NotificationActionsMargin))]
    private static Thickness DefaultForNotificationActionsMargin(in ControlTokenDefaultContext context) =>
        new Thickness(0, context.Global.UniformlyMarginSM, 0, 0);

    [ControlTokenDefault(nameof(NotificationCloseButtonMargin))]
    private static Thickness DefaultForNotificationCloseButtonMargin(in ControlTokenDefaultContext context) =>
        new Thickness(0, context.Global.UniformlyPaddingMD, context.Global.UniformlyPaddingLG, 0);

    [ControlTokenDefault(nameof(NotificationProgressTrackBg))]
    private static Color DefaultForNotificationProgressTrackBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorFillQuaternary;

    [ControlTokenDefault(nameof(NotificationProgressHeight))]
    private static double DefaultForNotificationProgressHeight(in ControlTokenDefaultContext context) =>
        2d;

    [ControlTokenDefault(nameof(NotificationProgressMargin))]
    private static Thickness DefaultForNotificationProgressMargin(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.BorderRadiusLG.TopLeft, 0, context.Global.BorderRadiusLG.TopRight, 0);

    [ControlTokenDefault(nameof(NotificationWidth))]
    private static double DefaultForNotificationWidth(in ControlTokenDefaultContext context) =>
        384d;

    [ControlTokenDefault(nameof(NotificationTopMargin))]
    private static Thickness DefaultForNotificationTopMargin(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyMarginLG, context.Global.UniformlyMarginLG, context.Global.UniformlyMarginLG, 0);

    [ControlTokenDefault(nameof(NotificationBottomMargin))]
    private static Thickness DefaultForNotificationBottomMargin(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyMarginLG, 0, context.Global.UniformlyMarginLG, context.Global.UniformlyMarginLG);

    [ControlTokenDefault(nameof(NotificationProgressBg))]
    private static IImmutableBrush? DefaultForNotificationProgressBg(in ControlTokenDefaultContext context) =>
        new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0.5, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 0.5, RelativeUnit.Relative),
            GradientStops = new GradientStops
            {
                new() { Color = context.Global.ColorPrimaryBorderHover, Offset = 0 },
                new() { Color = context.Global.ColorPrimary, Offset = 1 }
            }
        }.ToImmutable();
}
