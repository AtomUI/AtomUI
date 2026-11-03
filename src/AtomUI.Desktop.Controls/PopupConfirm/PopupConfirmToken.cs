using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class PopupConfirmToken : AbstractControlDesignToken, IControlTokenDefaults
{
    
    /// <summary>
    /// 历史保留资源，默认240；无生产消费者，不作为弹层最小宽度入口。
    /// </summary>
    public double PopupMinWidth { get; set; }

    /// <summary>
    /// 历史保留资源，默认80；无生产消费者，不作为弹层最小高度入口。
    /// </summary>
    public double PopupMinHeight { get; set; }

    /// <summary>
    /// 按钮的外边距
    /// </summary>
    public double ButtonSpacing { get; set; }

    /// <summary>
    /// Icon 外边距
    /// </summary>
    public Thickness IconMargin { get; set; }

    /// <summary>
    /// 主内容区域外边距
    /// </summary>
    public Thickness ContentContainerMargin { get; set; }

    /// <summary>
    /// 按钮内容区域外边距
    /// </summary>
    public Thickness ButtonContainerMargin { get; set; }

    /// <summary>
    /// 标题栏外边距
    /// </summary>
    public Thickness TitleMargin { get; set; }

    [ControlTokenDefault(nameof(PopupMinWidth))]
    private static double DefaultForPopupMinWidth(in ControlTokenDefaultContext context) => 240d;

    [ControlTokenDefault(nameof(PopupMinHeight))]
    private static double DefaultForPopupMinHeight(in ControlTokenDefaultContext context) => 80d;

    [ControlTokenDefault(nameof(ButtonSpacing))]
    private static double DefaultForButtonSpacing(in ControlTokenDefaultContext context) =>
        context.Global.UniformlyMarginXS;

    [ControlTokenDefault(nameof(IconMargin))]
    private static Thickness DefaultForIconMargin(in ControlTokenDefaultContext context) =>
        new(0, context.Global.UniformlyMarginXS / 2, context.Global.UniformlyMarginXS, 0);

    [ControlTokenDefault(nameof(ContentContainerMargin))]
    private static Thickness DefaultForContentContainerMargin(in ControlTokenDefaultContext context) =>
        new(0, 0, 0, context.Global.UniformlyMarginXS);

    [ControlTokenDefault(nameof(TitleMargin))]
    private static Thickness DefaultForTitleMargin(in ControlTokenDefaultContext context) =>
        new(0, 0, 0, context.Global.UniformlyMarginXS);

    [ControlTokenDefault(nameof(ButtonContainerMargin))]
    private static Thickness DefaultForButtonContainerMargin(in ControlTokenDefaultContext context) =>
        new(0, context.Global.UniformlyMarginXS, 0, 0);
}
