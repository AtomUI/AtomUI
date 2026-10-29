using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class TreeSelectToken : AbstractControlDesignToken, IControlTokenDefaults
{
    
    /// <summary>
    /// 最小的弹窗的宽度
    /// </summary>
    public double MinPopupWidth { get; set; }

    [ControlTokenDefault(nameof(MinPopupWidth))]
    private static double DefaultForMinPopupWidth(in ControlTokenDefaultContext context) => 300;
    
}
