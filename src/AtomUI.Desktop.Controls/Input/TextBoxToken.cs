using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class TextBoxToken : AbstractControlDesignToken, IControlTokenDefaults
{
    /// <summary>
    /// 默认内边距
    /// </summary>
    public Thickness ContentPadding { get; set; }

    /// <summary>
    /// 小号内边距
    /// </summary>
    public Thickness ContentPaddingSM { get; set; }

    /// <summary>
    /// 大号内边距
    /// </summary>
    public Thickness ContentPaddingLG { get; set; }

    [ControlTokenDefault(nameof(ContentPadding))]
    private static Thickness DefaultForContentPadding(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyPaddingSM - context.Global.LineWidth,
            Math.Round((context.Global.ControlHeight - context.Global.FontSize * context.Global.RelativeLineHeight) / 2 * 10) / 10 - context.Global.LineWidth);

    [ControlTokenDefault(nameof(ContentPaddingSM))]
    private static Thickness DefaultForContentPaddingSM(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.ControlPaddingHorizontalSM - context.Global.LineWidth,
            Math.Round((context.Global.ControlHeightSM - context.Global.FontSizeSM * context.Global.RelativeLineHeightSM) / 2 * 10) / 10 - context.Global.LineWidth);

    [ControlTokenDefault(nameof(ContentPaddingLG))]
    private static Thickness DefaultForContentPaddingLG(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.ControlPaddingHorizontal - context.Global.LineWidth,
            Math.Ceiling((context.Global.ControlHeightLG - context.Global.FontSizeLG * context.Global.RelativeLineHeightLG) / 2 * 10) / 10 - context.Global.LineWidth);
}
