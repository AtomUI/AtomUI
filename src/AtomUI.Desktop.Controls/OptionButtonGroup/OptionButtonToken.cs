using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class OptionButtonToken : AbstractControlDesignToken, IControlTokenDefaults
{
    
    /// <summary>
    /// 单选框按钮背景色
    /// </summary>
    public Color ButtonBackground { get; set; }

    /// <summary>
    /// 单选框按钮选中背景色
    /// </summary>
    public Color ButtonCheckedBackground { get; set; }

    /// <summary>
    /// 单选框按钮文本颜色
    /// </summary>
    public Color ButtonColor { get; set; }

    /// <summary>
    /// 单选框按钮内间距
    /// </summary>
    public Thickness ButtonPadding { get; set; }

    /// <summary>
    /// 单选框按钮选中并禁用时的背景色
    /// </summary>
    public Color ButtonCheckedBgDisabled { get; set; }

    /// <summary>
    /// 单选框按钮选中并禁用时的文本颜色
    /// </summary>
    public Color ButtonCheckedColorDisabled { get; set; }

    /// <summary>
    /// 单选框实色按钮选中时的文本颜色
    /// </summary>
    public Color ButtonSolidCheckedColor { get; set; }

    /// <summary>
    /// 单选框实色按钮选中时的背景色
    /// </summary>
    public Color ButtonSolidCheckedBackground { get; set; }

    /// <summary>
    /// 单选框实色按钮选中时的悬浮态背景色
    /// </summary>
    public Color ButtonSolidCheckedHoverBackground { get; set; }

    /// <summary>
    /// 单选框实色按钮选中时的激活态背景色
    /// </summary>
    public Color ButtonSolidCheckedActiveBackground { get; set; }

    /// <summary>
    /// 按钮内容字体大小
    /// </summary>
    public double ContentFontSize { get; set; }

    /// <summary>
    /// 大号按钮内容字体大小
    /// </summary>
    public double ContentFontSizeLG { get; set; }

    /// <summary>
    /// 小号按钮内容字体大小
    /// </summary>
    public double ContentFontSizeSM { get; set; }

    /// <summary>
    /// 按钮内容字体行高
    /// </summary>
    public double ContentLineHeight { get; set; }

    /// <summary>
    /// 大号按钮内容字体行高
    /// </summary>
    public double ContentLineHeightLG { get; set; }

    /// <summary>
    /// 小号按钮内容字体行高
    /// </summary>
    public double ContentLineHeightSM { get; set; }

    /// <summary>
    /// 按钮内间距
    /// </summary>
    public Thickness ContentPadding { get; set; }

    /// <summary>
    /// 大号按钮内间距
    /// </summary>
    public Thickness ContentPaddingLG { get; set; }

    /// <summary>
    /// 小号按钮内间距
    /// </summary>
    public Thickness ContentPaddingSM { get; set; }

    [ControlTokenDefault(nameof(ButtonBackground))]
    private static Color DefaultForButtonBackground(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgContainer;

    [ControlTokenDefault(nameof(ButtonCheckedBackground))]
    private static Color DefaultForButtonCheckedBackground(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgContainer;

    [ControlTokenDefault(nameof(ButtonColor))]
    private static Color DefaultForButtonColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorText;

    [ControlTokenDefault(nameof(ButtonPadding))]
    private static Thickness DefaultForButtonPadding(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyPadding, 0);

    [ControlTokenDefault(nameof(ButtonCheckedBgDisabled))]
    private static Color DefaultForButtonCheckedBgDisabled(in ControlTokenDefaultContext context) =>
        context.Global.ControlItemBgActiveDisabled;

    [ControlTokenDefault(nameof(ButtonCheckedColorDisabled))]
    private static Color DefaultForButtonCheckedColorDisabled(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextDisabled;

    [ControlTokenDefault(nameof(ButtonSolidCheckedColor))]
    private static Color DefaultForButtonSolidCheckedColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextLightSolid;

    [ControlTokenDefault(nameof(ButtonSolidCheckedBackground))]
    private static Color DefaultForButtonSolidCheckedBackground(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimary;

    [ControlTokenDefault(nameof(ButtonSolidCheckedHoverBackground))]
    private static Color DefaultForButtonSolidCheckedHoverBackground(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimaryHover;

    [ControlTokenDefault(nameof(ButtonSolidCheckedActiveBackground))]
    private static Color DefaultForButtonSolidCheckedActiveBackground(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimaryActive;

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
    private static double DefaultForContentLineHeight(in ControlTokenDefaultContext context, double fontSize) =>
        CalculatorUtils.CalculateLineHeight(fontSize);

    [ControlTokenDefault(nameof(ContentLineHeightLG), nameof(ContentFontSizeLG))]
    private static double DefaultForContentLineHeightLG(in ControlTokenDefaultContext context, double fontSize) =>
        CalculatorUtils.CalculateLineHeight(fontSize);

    [ControlTokenDefault(nameof(ContentLineHeightSM), nameof(ContentFontSizeSM))]
    private static double DefaultForContentLineHeightSM(in ControlTokenDefaultContext context, double fontSize) =>
        CalculatorUtils.CalculateLineHeight(fontSize);

    [ControlTokenDefault(nameof(ContentPadding), nameof(ButtonPadding), nameof(ContentFontSize), nameof(ContentLineHeight))]
    private static Thickness DefaultForContentPadding(in ControlTokenDefaultContext context, Thickness basePadding, double fontSize, double lineHeight) =>
        CalculatePadding(basePadding, context.Global.ControlHeight, fontSize, lineHeight, context.Global.LineWidth, 1);

    [ControlTokenDefault(nameof(ContentPaddingLG), nameof(ButtonPadding), nameof(ContentFontSizeLG), nameof(ContentLineHeightLG))]
    private static Thickness DefaultForContentPaddingLG(in ControlTokenDefaultContext context, Thickness basePadding, double fontSize, double lineHeight) =>
        CalculatePadding(basePadding, context.Global.ControlHeightLG, fontSize, lineHeight, context.Global.LineWidth, 1);

    [ControlTokenDefault(nameof(ContentPaddingSM), nameof(ButtonPadding), nameof(ContentFontSizeSM), nameof(ContentLineHeightSM))]
    private static Thickness DefaultForContentPaddingSM(in ControlTokenDefaultContext context, Thickness basePadding, double fontSize, double lineHeight) =>
        CalculatePadding(basePadding, context.Global.ControlHeightSM, fontSize, lineHeight, context.Global.LineWidth, 0.5);

    private static Thickness CalculatePadding(Thickness basis, double height, double fontSize, double lineHeight,
        double lineWidth, double horizontalScale)
    {
        var vertical = Math.Max((height - fontSize * lineHeight) / 2 - lineWidth, 0);
        return new Thickness(Math.Max(basis.Left * horizontalScale - lineWidth, 0),
            Math.Max(basis.Top + vertical, 0), Math.Max(basis.Right * horizontalScale - lineWidth, 0),
            Math.Max(basis.Bottom + vertical, 0));
    }
}
