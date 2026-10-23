using AtomUI.Media;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class DropdownButtonToken : AbstractControlDesignToken, IControlTokenDefaults
{
    public Color BorderColorDisabled { get; set; }
    public Thickness ContentPadding { get; set; }
    public Thickness ContentPaddingLG { get; set; }
    public Thickness ContentPaddingSM { get; set; }
    public Thickness ExtraContentMargin { get; set; }
    public Thickness ExtraContentMarginLG { get; set; }
    public Thickness ExtraContentMarginSM { get; set; }
    public Thickness CirclePadding { get; set; }
    public double OnlyIconSize { get; set; }
    public double OnlyIconSizeLG { get; set; }
    public double OnlyIconSizeSM { get; set; }
    public Thickness IconMargin { get; set; }
    public Thickness IconEndMargin { get; set; }
    public double ContentFontSize { get; set; }
    public double ContentFontSizeLG { get; set; }
    public double ContentFontSizeSM { get; set; }
    public double ContentLineHeight { get; set; }
    public double ContentLineHeightLG { get; set; }
    public double ContentLineHeightSM { get; set; }
    public double MarginToAnchor { get; set; }
    public Thickness IconOnyPadding { get; set; }
    public Thickness IconOnyPaddingLG { get; set; }
    public Thickness IconOnyPaddingSM { get; set; }

    public Color DangerColor { get; set; }
    public BoxShadows DangerShadow { get; set; }
    public Color DefaultActiveBg { get; set; }
    public Color DefaultActiveBorderColor { get; set; }
    public Color DefaultActiveColor { get; set; }
    public Color DefaultBg { get; set; }
    public Color DefaultBorderColor { get; set; }
    public Color DefaultColor { get; set; }
    public Color DefaultGhostBorderColor { get; set; }
    public Color DefaultGhostColor { get; set; }
    public Color DefaultHoverBg { get; set; }
    public Color DefaultHoverBorderColor { get; set; }
    public Color DefaultHoverColor { get; set; }
    public BoxShadows DefaultShadow { get; set; }
    public Color GhostBg { get; set; }
    public Color LinkHoverBg { get; set; }
    public Color PrimaryColor { get; set; }
    public BoxShadows PrimaryShadow { get; set; }
    public Color SolidTextColor { get; set; }
    public Color TextHoverBg { get; set; }
    public Color TextTextActiveColor { get; set; }
    public Color TextTextColor { get; set; }
    public Color TextTextHoverColor { get; set; }

    [ControlTokenDefault(nameof(DangerColor))]
    private static Color DefaultForDangerColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextLightSolid;

    [ControlTokenDefault(nameof(DangerShadow))]
    private static BoxShadows DefaultForDangerShadow(in ControlTokenDefaultContext context) =>
        new BoxShadows(new BoxShadow
        {
            OffsetX = 0,
            OffsetY = context.Global.ControlOutlineWidth,
            Blur = 3,
            Spread = 0,
            Color = context.Global.ColorErrorOutline
        });

    [ControlTokenDefault(nameof(DefaultActiveBg))]
    private static Color DefaultForDefaultActiveBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgContainer;

    [ControlTokenDefault(nameof(DefaultActiveBorderColor))]
    private static Color DefaultForDefaultActiveBorderColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimaryActive;

    [ControlTokenDefault(nameof(DefaultActiveColor))]
    private static Color DefaultForDefaultActiveColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimaryActive;

    [ControlTokenDefault(nameof(DefaultBg))]
    private static Color DefaultForDefaultBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgContainer;

    [ControlTokenDefault(nameof(DefaultBorderColor))]
    private static Color DefaultForDefaultBorderColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorBorder;

    [ControlTokenDefault(nameof(DefaultColor))]
    private static Color DefaultForDefaultColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorText;

    [ControlTokenDefault(nameof(DefaultGhostBorderColor))]
    private static Color DefaultForDefaultGhostBorderColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgContainer;

    [ControlTokenDefault(nameof(DefaultGhostColor))]
    private static Color DefaultForDefaultGhostColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgContainer;

    [ControlTokenDefault(nameof(DefaultHoverBg))]
    private static Color DefaultForDefaultHoverBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgContainer;

    [ControlTokenDefault(nameof(DefaultHoverBorderColor))]
    private static Color DefaultForDefaultHoverBorderColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimaryHover;

    [ControlTokenDefault(nameof(DefaultHoverColor))]
    private static Color DefaultForDefaultHoverColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimaryHover;

    [ControlTokenDefault(nameof(DefaultShadow))]
    private static BoxShadows DefaultForDefaultShadow(in ControlTokenDefaultContext context) =>
        new BoxShadows(new BoxShadow
        {
            OffsetX = 0,
            OffsetY = context.Global.ControlOutlineWidth,
            Blur = 3,
            Spread = 0,
            Color = context.Global.ColorControlOutline
        });

    [ControlTokenDefault(nameof(GhostBg))]
    private static Color DefaultForGhostBg(in ControlTokenDefaultContext context) =>
        Colors.Transparent;

    [ControlTokenDefault(nameof(LinkHoverBg))]
    private static Color DefaultForLinkHoverBg(in ControlTokenDefaultContext context) =>
        Colors.Transparent;

    [ControlTokenDefault(nameof(PrimaryColor))]
    private static Color DefaultForPrimaryColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextLightSolid;

    [ControlTokenDefault(nameof(PrimaryShadow))]
    private static BoxShadows DefaultForPrimaryShadow(in ControlTokenDefaultContext context) =>
        new BoxShadows(new BoxShadow
        {
            OffsetX = 0,
            OffsetY = context.Global.ControlOutlineWidth,
            Blur = 3,
            Spread = 0,
            Color = context.Global.ColorControlOutline
        });

    [ControlTokenDefault(nameof(SolidTextColor))]
    private static Color DefaultForSolidTextColor(in ControlTokenDefaultContext context) =>
        ColorUtils.IsBright(context.Global.ColorBgSolid, Colors.White) ? Colors.Black : Colors.White;

    [ControlTokenDefault(nameof(TextHoverBg))]
    private static Color DefaultForTextHoverBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorFillTertiary;

    [ControlTokenDefault(nameof(TextTextActiveColor))]
    private static Color DefaultForTextTextActiveColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorText;

    [ControlTokenDefault(nameof(TextTextColor))]
    private static Color DefaultForTextTextColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorText;

    [ControlTokenDefault(nameof(TextTextHoverColor))]
    private static Color DefaultForTextTextHoverColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorText;

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

    [ControlTokenDefault(nameof(ExtraContentMargin), nameof(ContentPadding))]
    private static Thickness DefaultForExtraContentMargin(in ControlTokenDefaultContext context, Thickness contentPadding) =>
        new Thickness(contentPadding.Left / 2, 0, 0, 0);

    [ControlTokenDefault(nameof(ExtraContentMarginLG), nameof(ContentPaddingLG))]
    private static Thickness DefaultForExtraContentMarginLG(in ControlTokenDefaultContext context, Thickness contentPadding) =>
        new Thickness(contentPadding.Left / 2, 0, 0, 0);

    [ControlTokenDefault(nameof(ExtraContentMarginSM), nameof(ContentPaddingSM))]
    private static Thickness DefaultForExtraContentMarginSM(in ControlTokenDefaultContext context, Thickness contentPadding) =>
        new Thickness(contentPadding.Left / 2, 0, 0, 0);

    [ControlTokenDefault(nameof(CirclePadding), nameof(ContentPaddingSM))]
    private static Thickness DefaultForCirclePadding(in ControlTokenDefaultContext context, Thickness contentPadding) =>
        new Thickness(contentPadding.Left / 2);

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

    [ControlTokenDefault(nameof(IconEndMargin))]
    private static Thickness DefaultForIconEndMargin(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyPaddingXXS, 0, 0, 0);

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

    [ControlTokenDefault(nameof(MarginToAnchor))]
    private static double DefaultForMarginToAnchor(in ControlTokenDefaultContext context) =>
        context.Global.UniformlyMarginXXS;

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
