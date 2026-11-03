using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class MessageCardToken : AbstractControlDesignToken, IControlTokenDefaults
{

    /// <summary>
    /// 提示框背景色
    /// </summary>
    public Color ContentBg { get; set; }

    /// <summary>
    /// 提示框内边距
    /// </summary>
    public Thickness ContentPadding { get; set; }

    /// <summary>
    /// 历史保留高度资源；无生产消费者，不约束自适应内容高度。
    /// </summary>
    public double CardHeight { get; set; }

    /// <summary>
    /// 提醒框图标尺寸
    /// </summary>
    public double MessageIconSize { get; set; }

    /// <summary>
    /// 提醒框图标外边距
    /// </summary>
    public Thickness MessageIconMargin { get; set; }
    public Thickness MessageTopMargin { get; set; }

    [ControlTokenDefault(nameof(ContentBg))]
    private static Color DefaultForContentBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgElevated;

    [ControlTokenDefault(nameof(ContentPadding))]
    private static Thickness DefaultForContentPadding(in ControlTokenDefaultContext context) =>
        new Thickness(
            (context.Global.ControlHeightLG - context.Global.FontSize * context.Global.RelativeLineHeight) / 2,
            context.Global.UniformlyPaddingXS);

    [ControlTokenDefault(nameof(CardHeight))]
    private static double DefaultForCardHeight(in ControlTokenDefaultContext context) => 0d;

    [ControlTokenDefault(nameof(MessageIconSize))]
    private static double DefaultForMessageIconSize(in ControlTokenDefaultContext context) =>
        context.Global.FontSizeSM * context.Global.RelativeLineHeightSM;

    [ControlTokenDefault(nameof(MessageIconMargin))]
    private static Thickness DefaultForMessageIconMargin(in ControlTokenDefaultContext context) =>
        new Thickness(0, 0, context.Global.UniformlyMarginXS, 0);

    [ControlTokenDefault(nameof(MessageTopMargin))]
    private static Thickness DefaultForMessageTopMargin(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyMarginXS);
}
