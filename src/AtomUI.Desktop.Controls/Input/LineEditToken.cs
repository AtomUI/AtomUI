using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class LineEditToken : AbstractControlDesignToken, IControlTokenDefaults
{
    /// <summary>
    /// 字体大小
    /// </summary>
    public double InputFontSize { get; set; }

    /// <summary>
    /// 大号字体大小
    /// </summary>
    public double InputFontSizeLG { get; set; }

    /// <summary>
    /// 小号字体大小
    /// </summary>
    public double InputFontSizeSM { get; set; }
    
    [ControlTokenDefault(nameof(InputFontSize))]
    private static double DefaultForInputFontSize(in ControlTokenDefaultContext context) =>
        context.Global.FontSize;

    [ControlTokenDefault(nameof(InputFontSizeLG))]
    private static double DefaultForInputFontSizeLG(in ControlTokenDefaultContext context) =>
        context.Global.FontSizeLG;

    [ControlTokenDefault(nameof(InputFontSizeSM))]
    private static double DefaultForInputFontSizeSM(in ControlTokenDefaultContext context) =>
        context.Global.FontSizeSM;
}
