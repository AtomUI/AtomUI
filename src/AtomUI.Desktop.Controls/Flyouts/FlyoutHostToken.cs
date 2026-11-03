using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class FlyoutHostToken : AbstractControlDesignToken, IControlTokenDefaults
{
    
    /// <summary>
    /// 默认 Popup 和 PlacementTarget 的间距
    /// </summary>
    public double MarginToAnchor { get; set; }
    
    /// <summary>
    /// OverlayHost 类型的阴影
    /// </summary>
    public BoxShadows OverlayHostShadow { get; set; }
    
    /// <summary>
    /// PopupRoot 类型的阴影
    /// </summary>
    public BoxShadows PopupRootShadow { get; set; }
    
    /// <summary>
    /// Flyout 水平偏移
    /// </summary>
    public double HorizontalOffset { get; set; }
    
    /// <summary>
    /// Flyout 垂直偏移
    /// </summary>
    public double VerticalOffset { get; set; }


    
    [ControlTokenDefault(nameof(MarginToAnchor))]
    private static double DefaultForMarginToAnchor(in ControlTokenDefaultContext context) => context.Global.UniformlyMarginXXS;

    [ControlTokenDefault(nameof(OverlayHostShadow))]
    private static BoxShadows DefaultForOverlayHostShadow(in ControlTokenDefaultContext context) => context.Global.BoxShadowsSecondary;

    [ControlTokenDefault(nameof(PopupRootShadow))]
    private static BoxShadows DefaultForPopupRootShadow(in ControlTokenDefaultContext context) => context.Global.BoxShadowsSecondary;

    [ControlTokenDefault(nameof(HorizontalOffset))]
    private static double DefaultForHorizontalOffset(in ControlTokenDefaultContext context) => context.Global.UniformlyMarginXS;

    [ControlTokenDefault(nameof(VerticalOffset))]
    private static double DefaultForVerticalOffset(in ControlTokenDefaultContext context) => context.Global.UniformlyMarginXS;
}
