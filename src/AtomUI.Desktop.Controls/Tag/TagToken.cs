using AtomUI.Media;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class TagToken : AbstractControlDesignToken, IControlTokenDefaults
{
    /// <summary>
    /// 默认背景色
    /// </summary>
    public Color DefaultBg { get; set; }

    /// <summary>
    /// 默认文字颜色
    /// </summary>
    public Color DefaultColor { get; set; }

    public double TagFontSize { get; set; }
    public double TagLineHeight { get; set; }
    public double TagIconSize { get; set; }
    public double TagCloseIconSize { get; set; }
    public Thickness TagPadding { get; set; }
    public Thickness TagTextPaddingInline { get; set; }
    public Color SolidTextColor { get; set; }

    [ControlTokenDefault(nameof(TagFontSize))]
    private static double DefaultForTagFontSize(in ControlTokenDefaultContext context) => context.Global.FontSizeSM;

    [ControlTokenDefault(nameof(TagLineHeight))]
    private static double DefaultForTagLineHeight(in ControlTokenDefaultContext context) => context.Global.FontHeightSM;

    [ControlTokenDefault(nameof(TagCloseIconSize))]
    private static double DefaultForTagCloseIconSize(in ControlTokenDefaultContext context) => context.Global.IconSizeXS;

    [ControlTokenDefault(nameof(TagIconSize))]
    private static double DefaultForTagIconSize(in ControlTokenDefaultContext context) => context.Global.FontSizeIcon;

    [ControlTokenDefault(nameof(TagPadding))]
    private static Thickness DefaultForTagPadding(in ControlTokenDefaultContext context) => new(context.Global.SizeXS - 1, 0);

    [ControlTokenDefault(nameof(DefaultBg))]
    private static Color DefaultForDefaultBg(in ControlTokenDefaultContext context) =>
        ColorUtils.OnBackground(context.Global.ColorFillTertiary, context.Global.ColorBgContainer);

    [ControlTokenDefault(nameof(DefaultColor))]
    private static Color DefaultForDefaultColor(in ControlTokenDefaultContext context) => context.Global.ColorText;

    [ControlTokenDefault(nameof(TagTextPaddingInline))]
    private static Thickness DefaultForTagTextPaddingInline(in ControlTokenDefaultContext context) => new(context.Global.UniformlyPaddingXXS, 0);

    [ControlTokenDefault(nameof(SolidTextColor))]
    private static Color DefaultForSolidTextColor(in ControlTokenDefaultContext context) =>
        ColorUtils.IsBright(context.Global.ColorBgSolid, Colors.White) ? Colors.Black : Colors.White;
}
