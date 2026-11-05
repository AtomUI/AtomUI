using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class AutoCompleteToken : AbstractControlDesignToken, IControlTokenDefaults
{
    
    /// <summary>
    /// 菜单内容边距
    /// </summary>
    public Thickness PopupContentPadding { get; set; }
    
    /// <summary>
    /// 选项高度
    /// Height of option
    /// </summary>
    public double OptionHeight { get; set; }
    
    /// <summary>
    /// 候选列表弹窗最小宽度
    /// </summary>
    public double MinPopupWidth { get; set; }
    
    /// <summary>
    /// 在不跟随 Anchor 宽度时候的 Popup 最大宽度
    /// </summary>
    public double MaxPopupWidth { get; set; }
    
    [ControlTokenDefault(nameof(OptionHeight))]
    private static double DefaultForOptionHeight(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeight;

    [ControlTokenDefault(nameof(PopupContentPadding))]
    private static Thickness DefaultForPopupContentPadding(in ControlTokenDefaultContext context) =>
        new(context.Global.UniformlyPaddingXXS / 2);

    [ControlTokenDefault(nameof(MinPopupWidth))]
    private static double DefaultForMinPopupWidth(in ControlTokenDefaultContext context) => 120;

    [ControlTokenDefault(nameof(MaxPopupWidth))]
    private static double DefaultForMaxPopupWidth(in ControlTokenDefaultContext context) => 200;
    
}
