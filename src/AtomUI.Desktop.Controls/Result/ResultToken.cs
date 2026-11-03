using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class ResultToken : AbstractControlDesignToken, IControlTokenDefaults
{

    /// <summary>
    /// 标题字体大小
    /// </summary>
    public double HeaderFontSize { get; set; }

    /// <summary>
    /// 副标题字体大小
    /// </summary>
    public double SubHeaderFontSize { get; set; }

    /// <summary>
    /// 图标大小
    /// </summary>
    public double StatusIconSize { get; set; }

    /// <summary>
    /// 额外区域外间距
    /// </summary>
    public Thickness ExtraMargin { get; set; }

    public double ImageWidth { get; set; }
    public double ImageHeight { get; set; }

    public Color ResultInfoIconColor { get; set; }
    public Color ResultSuccessIconColor { get; set; }
    public Color ResultWarningIconColor { get; set; }
    public Color ResultErrorIconColor { get; set; }
    public Thickness FramePadding { get; set; }
    public Thickness ContentPadding { get; set; }
    public Thickness ContentMargin { get; set; }
    public Thickness HeaderMargin { get; set; }
    public Thickness StatusImageMargin { get; set; }

    [ControlTokenDefault(nameof(HeaderFontSize))]
    private static double DefaultForHeaderFontSize(in ControlTokenDefaultContext context) => context.Global.FontSizeHeading3;

    [ControlTokenDefault(nameof(SubHeaderFontSize))]
    private static double DefaultForSubHeaderFontSize(in ControlTokenDefaultContext context) => context.Global.FontSize;

    [ControlTokenDefault(nameof(StatusIconSize))]
    private static double DefaultForStatusIconSize(in ControlTokenDefaultContext context) => context.Global.FontSizeHeading3 * 3;

    [ControlTokenDefault(nameof(ExtraMargin))]
    private static Thickness DefaultForExtraMargin(in ControlTokenDefaultContext context) => new(0, context.Global.UniformlyMargin, 0, 0);

    [ControlTokenDefault(nameof(ImageWidth))]
    private static double DefaultForImageWidth(in ControlTokenDefaultContext context) => 250;

    [ControlTokenDefault(nameof(ImageHeight))]
    private static double DefaultForImageHeight(in ControlTokenDefaultContext context) => 295;

    [ControlTokenDefault(nameof(ResultInfoIconColor))]
    private static Color DefaultForResultInfoIconColor(in ControlTokenDefaultContext context) => context.Global.ColorInfo;

    [ControlTokenDefault(nameof(ResultSuccessIconColor))]
    private static Color DefaultForResultSuccessIconColor(in ControlTokenDefaultContext context) => context.Global.ColorSuccess;

    [ControlTokenDefault(nameof(ResultWarningIconColor))]
    private static Color DefaultForResultWarningIconColor(in ControlTokenDefaultContext context) => context.Global.ColorWarning;

    [ControlTokenDefault(nameof(ResultErrorIconColor))]
    private static Color DefaultForResultErrorIconColor(in ControlTokenDefaultContext context) => context.Global.ColorError;

    [ControlTokenDefault(nameof(ContentPadding))]
    private static Thickness DefaultForContentPadding(in ControlTokenDefaultContext context) => new(context.Global.UniformlyPadding * 2.5, context.Global.UniformlyPaddingLG);

    [ControlTokenDefault(nameof(ContentMargin))]
    private static Thickness DefaultForContentMargin(in ControlTokenDefaultContext context) => new(0, context.Global.UniformlyPaddingLG, 0, 0);

    [ControlTokenDefault(nameof(StatusImageMargin))]
    private static Thickness DefaultForStatusImageMargin(in ControlTokenDefaultContext context) => new(0, 0, 0, context.Global.UniformlyMargin);

    [ControlTokenDefault(nameof(HeaderMargin))]
    private static Thickness DefaultForHeaderMargin(in ControlTokenDefaultContext context) => new(0, context.Global.UniformlyMarginXS);

    [ControlTokenDefault(nameof(FramePadding))]
    private static Thickness DefaultForFramePadding(in ControlTokenDefaultContext context) => new(context.Global.UniformlyPaddingLG * 2, context.Global.UniformlyMarginXL);
}
