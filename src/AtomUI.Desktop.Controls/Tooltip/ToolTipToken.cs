using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class ToolTipToken : AbstractControlDesignToken, IControlTokenDefaults
{
    
    /// <summary>
    /// tooltip 的最大宽度，超过了就换行
    /// </summary>
    public double ToolTipMaxWidth { get; set; }

    /// <summary>
    /// ToolTip 默认的前景色
    /// </summary>
    public Color ToolTipColor { get; set; }

    /// <summary>
    /// ToolTip 默认的背景色
    /// </summary>
    public Color ToolTipBackground { get; set; }

    /// <summary>
    /// ToolTip 默认的圆角
    /// </summary>
    public CornerRadius ToolTipCornerRadius { get; set; }

    /// <summary>
    /// ToolTip 默认的内间距
    /// </summary>
    public Thickness ContentPadding { get; set; }
    
    /// <summary>
    /// 历史保留资源；实际动效消费 Global.MotionDurationMid，不作为独立视觉入口。
    /// </summary>
    public TimeSpan MotionDuration { get; set; }

    [ControlTokenDefault(nameof(ToolTipMaxWidth))]
    private static double DefaultForToolTipMaxWidth(in ControlTokenDefaultContext context) => 250d;

    [ControlTokenDefault(nameof(ToolTipColor))]
    private static Color DefaultForToolTipColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextLightSolid;

    [ControlTokenDefault(nameof(ToolTipBackground))]
    private static Color DefaultForToolTipBackground(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgSpotlight;

    [ControlTokenDefault(nameof(ToolTipCornerRadius))]
    private static CornerRadius DefaultForToolTipCornerRadius(in ControlTokenDefaultContext context) => new(4);

    [ControlTokenDefault(nameof(ContentPadding))]
    private static Thickness DefaultForContentPadding(in ControlTokenDefaultContext context) =>
        new(context.Global.UniformlyPaddingSM, context.Global.UniformlyPaddingSM / 2 + 2);

    [ControlTokenDefault(nameof(MotionDuration))]
    private static TimeSpan DefaultForMotionDuration(in ControlTokenDefaultContext context) =>
        context.Global.MotionDurationMid;
}
