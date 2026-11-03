using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class MessageBoxToken : AbstractControlDesignToken, IControlTokenDefaults
{

    /// <summary>
    /// Style Icon 的大小
    /// </summary>
    public double StyleIconSize { get; set; }
    
    /// <summary>
    /// 最小宽度
    /// </summary>
    public double MinWidth { get; set; }

    [ControlTokenDefault(nameof(StyleIconSize))]
    private static double DefaultForStyleIconSize(in ControlTokenDefaultContext context) =>
        context.Global.SizeLG * 1.2;

    [ControlTokenDefault(nameof(MinWidth))]
    private static double DefaultForMinWidth(in ControlTokenDefaultContext context) => 410d;
}
