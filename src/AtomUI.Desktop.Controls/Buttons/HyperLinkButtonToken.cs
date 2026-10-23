using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class HyperLinkButtonToken : AbstractControlDesignToken, IControlTokenDefaults
{
    public Color BorderColorDisabled { get; set; }
    public Thickness ContentPadding { get; set; }
    public Thickness ContentPaddingLG { get; set; }
    public Thickness ContentPaddingSM { get; set; }
    public double OnlyIconSize { get; set; }
    public double OnlyIconSizeLG { get; set; }
    public double OnlyIconSizeSM { get; set; }
    public Thickness IconMargin { get; set; }
    public double ContentFontSize { get; set; }
    public double ContentFontSizeLG { get; set; }
    public double ContentFontSizeSM { get; set; }
    public double ContentLineHeight { get; set; }
    public double ContentLineHeightLG { get; set; }
    public double ContentLineHeightSM { get; set; }
    public Thickness IconOnyPadding { get; set; }
    public Thickness IconOnyPaddingLG { get; set; }
    public Thickness IconOnyPaddingSM { get; set; }

    [ControlTokenDefault(nameof(BorderColorDisabled))]
    private static Color DefaultForBorderColorDisabled(in ControlTokenDefaultContext context) =>
        context.Global.ColorBorder;

    [ControlTokenDefault(nameof(ContentPadding), nameof(ContentLineHeight))]
    private static Thickness DefaultForContentPadding(in ControlTokenDefaultContext context, double contentLineHeight) =>
        new Thickness(context.Global.PaddingContentHorizontal - context.Global.LineWidth, Math.Max((context.Global.ControlHeight - contentLineHeight) / 2 - context.Global.LineWidth, 0));

    [ControlTokenDefault(nameof(ContentPaddingLG), nameof(ContentLineHeightLG))]
    private static Thickness DefaultForContentPaddingLG(in ControlTokenDefaultContext context, double contentLineHeight) =>
        new Thickness(context.Global.PaddingContentHorizontal - context.Global.LineWidth, Math.Max((context.Global.ControlHeightLG - contentLineHeight) / 2 - context.Global.LineWidth, 0));

    [ControlTokenDefault(nameof(ContentPaddingSM), nameof(ContentLineHeightSM))]
    private static Thickness DefaultForContentPaddingSM(in ControlTokenDefaultContext context, double contentLineHeight) =>
        new Thickness(8 - context.Global.LineWidth, Math.Max((context.Global.ControlHeightSM - contentLineHeight) / 2 - context.Global.LineWidth, 0));

    [ControlTokenDefault(nameof(OnlyIconSize))]
    private static double DefaultForOnlyIconSize(in ControlTokenDefaultContext context) =>
        context.Global.IconSizeLG;

    [ControlTokenDefault(nameof(OnlyIconSizeLG))]
    private static double DefaultForOnlyIconSizeLG(in ControlTokenDefaultContext context) =>
        context.Global.IconSizeLG;

    [ControlTokenDefault(nameof(OnlyIconSizeSM))]
    private static double DefaultForOnlyIconSizeSM(in ControlTokenDefaultContext context) =>
        context.Global.IconSize;

    [ControlTokenDefault(nameof(IconMargin))]
    private static Thickness DefaultForIconMargin(in ControlTokenDefaultContext context) =>
        new Thickness(0, 0, context.Global.UniformlyPaddingXXS, 0);

    [ControlTokenDefault(nameof(ContentFontSize))]
    private static double DefaultForContentFontSize(in ControlTokenDefaultContext context) =>
        context.Global.FontSize;

    [ControlTokenDefault(nameof(ContentFontSizeLG))]
    private static double DefaultForContentFontSizeLG(in ControlTokenDefaultContext context) =>
        context.Global.FontSizeLG;

    [ControlTokenDefault(nameof(ContentFontSizeSM))]
    private static double DefaultForContentFontSizeSM(in ControlTokenDefaultContext context) =>
        context.Global.FontSize;

    [ControlTokenDefault(nameof(ContentLineHeight), nameof(ContentFontSize))]
    private static double DefaultForContentLineHeight(in ControlTokenDefaultContext context, double contentFontSize) =>
        CalculatorUtils.CalculateLineHeight(contentFontSize) * contentFontSize;

    [ControlTokenDefault(nameof(ContentLineHeightLG), nameof(ContentFontSizeLG))]
    private static double DefaultForContentLineHeightLG(in ControlTokenDefaultContext context, double contentFontSize) =>
        CalculatorUtils.CalculateLineHeight(contentFontSize) * contentFontSize;

    [ControlTokenDefault(nameof(ContentLineHeightSM), nameof(ContentFontSizeSM))]
    private static double DefaultForContentLineHeightSM(in ControlTokenDefaultContext context, double contentFontSize) =>
        CalculatorUtils.CalculateLineHeight(contentFontSize) * contentFontSize;

    [ControlTokenDefault(nameof(IconOnyPadding), nameof(ContentLineHeight))]
    private static Thickness DefaultForIconOnyPadding(in ControlTokenDefaultContext context, double contentLineHeight) =>
        new Thickness(Math.Max((context.Global.ControlHeight - contentLineHeight) / 2 - context.Global.LineWidth, 0));

    [ControlTokenDefault(nameof(IconOnyPaddingLG), nameof(ContentLineHeightLG))]
    private static Thickness DefaultForIconOnyPaddingLG(in ControlTokenDefaultContext context, double contentLineHeight) =>
        new Thickness(Math.Max((context.Global.ControlHeightLG - contentLineHeight) / 2 - context.Global.LineWidth, 0));

    [ControlTokenDefault(nameof(IconOnyPaddingSM), nameof(ContentLineHeightSM))]
    private static Thickness DefaultForIconOnyPaddingSM(in ControlTokenDefaultContext context, double contentLineHeight) =>
        new Thickness(Math.Max((context.Global.ControlHeightSM - contentLineHeight) / 2 - context.Global.LineWidth, 0));
}
