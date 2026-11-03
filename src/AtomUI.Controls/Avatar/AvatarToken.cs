using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class AvatarToken : AbstractControlDesignToken, IControlTokenDefaults
{
    public double ContainerSize { get; set; }

    public double ContainerSizeLG { get; set; }

    public double ContainerSizeSM { get; set; }

    public double TextFontSize { get; set; }

    public double TextFontSizeLG { get; set; }

    public double TextFontSizeSM { get; set; }

    public double GroupSpace { get; set; }

    public double GroupOverlapping { get; set; }

    public Color GroupBorderColor { get; set; }

    public Color AvatarBg { get; set; }

    public Color AvatarColor { get; set; }

    [ControlTokenDefault(nameof(ContainerSize))]
    private static double DefaultForContainerSize(in ControlTokenDefaultContext context) => context.Global.ControlHeight;

    [ControlTokenDefault(nameof(ContainerSizeLG))]
    private static double DefaultForContainerSizeLG(in ControlTokenDefaultContext context) => context.Global.ControlHeightLG;

    [ControlTokenDefault(nameof(ContainerSizeSM))]
    private static double DefaultForContainerSizeSM(in ControlTokenDefaultContext context) => context.Global.ControlHeightSM;

    [ControlTokenDefault(nameof(TextFontSize))]
    private static double DefaultForTextFontSize(in ControlTokenDefaultContext context) =>
        Math.Round((context.Global.FontSizeLG + context.Global.FontSizeXL) / 2);

    [ControlTokenDefault(nameof(TextFontSizeLG))]
    private static double DefaultForTextFontSizeLG(in ControlTokenDefaultContext context) => context.Global.FontSizeHeading3;

    [ControlTokenDefault(nameof(TextFontSizeSM))]
    private static double DefaultForTextFontSizeSM(in ControlTokenDefaultContext context) => context.Global.FontSize;

    [ControlTokenDefault(nameof(GroupSpace))]
    private static double DefaultForGroupSpace(in ControlTokenDefaultContext context) => context.Global.UniformlyMarginXXS;

    [ControlTokenDefault(nameof(GroupOverlapping))]
    private static double DefaultForGroupOverlapping(in ControlTokenDefaultContext context) => context.Global.UniformlyMarginXS;

    [ControlTokenDefault(nameof(GroupBorderColor))]
    private static Color DefaultForGroupBorderColor(in ControlTokenDefaultContext context) => context.Global.ColorBorderBg;

    [ControlTokenDefault(nameof(AvatarBg))]
    private static Color DefaultForAvatarBg(in ControlTokenDefaultContext context) => context.Global.ColorTextPlaceholder;

    [ControlTokenDefault(nameof(AvatarColor))]
    private static Color DefaultForAvatarColor(in ControlTokenDefaultContext context) => context.Global.ColorTextLightSolid;
}
