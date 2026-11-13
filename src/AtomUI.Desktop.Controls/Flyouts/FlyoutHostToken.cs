using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class FlyoutHostToken : AbstractControlDesignToken
{
    
    /// <summary>
    /// 默认 Popup 和 PlacementTarget 的间距
    /// </summary>
    public partial double MarginToAnchor { get; set; }
    
    /// <summary>
    /// OverlayHost 类型的阴影
    /// </summary>
    public partial BoxShadows OverlayHostShadow { get; set; }
    
    /// <summary>
    /// PopupRoot 类型的阴影
    /// </summary>
    public partial BoxShadows PopupRootShadow { get; set; }
    
    /// <summary>
    /// Flyout 水平偏移
    /// </summary>
    public partial double HorizontalOffset { get; set; }
    
    /// <summary>
    /// Flyout 垂直偏移
    /// </summary>
    public partial double VerticalOffset { get; set; }

    public FlyoutHostToken()

    {
    }
    
    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        MarginToAnchor    = EffectiveGlobalToken.UniformlyMarginXXS;
        OverlayHostShadow = EffectiveGlobalToken.BoxShadowsSecondary;
        PopupRootShadow   = EffectiveGlobalToken.BoxShadowsSecondary;
        HorizontalOffset = EffectiveGlobalToken.UniformlyMarginXS;
        VerticalOffset   = EffectiveGlobalToken.UniformlyMarginXS;
    }
    
}
