using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class GroupBoxToken : AbstractControlDesignToken, IControlTokenDefaults
{
    


    /// <summary>
    /// Reserved：fieldset 文本横向内间距语义（em），当前无几何消费者。
    /// </summary>
    public double TextPaddingInline { get; set; }

    /// <summary>
    /// Reserved：标题与边缘距离的比例语义（0～1），当前无几何消费者。
    /// </summary>
    public double OrientationMarginPercent { get; set; }

    /// <summary>
    /// Reserved：纵向分隔线横向外间距语义，当前无几何消费者。
    /// </summary>
    public double VerticalMarginInline { get; set; }

    /// <summary>
    /// 内容区域内间距
    /// </summary>
    public Thickness ContentPadding { get; set; }

    /// <summary>
    /// Header 容器外间距
    /// </summary>
    public Thickness HeaderContainerMargin { get; set; }

    /// <summary>
    /// 头部图标的外间距
    /// </summary>
    public Thickness HeaderIconMargin { get; set; }

    /// <summary>
    /// Header 内容区域内间距
    /// </summary>
    public Thickness HeaderContentPadding { get; set; }

    [ControlTokenDefault(nameof(TextPaddingInline))]
    private static double DefaultForTextPaddingInline(in ControlTokenDefaultContext context) => 1.0;

    [ControlTokenDefault(nameof(OrientationMarginPercent))]
    private static double DefaultForOrientationMarginPercent(in ControlTokenDefaultContext context) => 0.05;

    [ControlTokenDefault(nameof(VerticalMarginInline))]
    private static double DefaultForVerticalMarginInline(in ControlTokenDefaultContext context) => context.Global.UniformlyMarginXS;

    [ControlTokenDefault(nameof(ContentPadding))]
    private static Thickness DefaultForContentPadding(in ControlTokenDefaultContext context) => context.Global.PaddingXS;

    [ControlTokenDefault(nameof(HeaderContainerMargin))]
    private static Thickness DefaultForHeaderContainerMargin(in ControlTokenDefaultContext context) => new Thickness(context.Global.UniformlyMargin, context.Global.UniformlyMarginXS, context.Global.UniformlyMargin, 0);

    [ControlTokenDefault(nameof(HeaderContentPadding))]
    private static Thickness DefaultForHeaderContentPadding(in ControlTokenDefaultContext context) => new Thickness(context.Global.UniformlyPaddingXXS, 0);

    [ControlTokenDefault(nameof(HeaderIconMargin))]
    private static Thickness DefaultForHeaderIconMargin(in ControlTokenDefaultContext context) => new Thickness(0, 0, context.Global.UniformlyMarginXXS, 0);
}
