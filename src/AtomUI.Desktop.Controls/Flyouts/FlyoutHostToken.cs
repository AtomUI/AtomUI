using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class FlyoutHostToken : AbstractControlDesignToken
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

    public FlyoutHostToken()

    {
    }
    
    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        MarginToAnchor    = EffectiveGlobalToken.UniformlyMarginXXS;
        OverlayHostShadow = EffectiveGlobalToken.BoxShadowsSecondary;
        PopupRootShadow   = EffectiveGlobalToken.BoxShadowsSecondary;
        HorizontalOffset = EffectiveGlobalToken.UniformlyMarginXS;
        VerticalOffset   = EffectiveGlobalToken.UniformlyMarginXS;
    }
    
}
