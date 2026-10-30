using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class PopupToken : AbstractControlDesignToken
{
    public CornerRadius PopupCornerRadius { get; set; }

    public BoxShadows OverlayHostShadow { get; set; }

    public BoxShadows PopupRootShadow { get; set; }

    public double MarginToAnchor { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);

        OverlayHostShadow = EffectiveGlobalToken.BoxShadowsSecondary;
        // 窗口型与 Overlay Popup 使用同一阴影；定位按残余阴影补偿，内容相对 anchor 的位置与阴影厚度无关
        PopupRootShadow = EffectiveGlobalToken.BoxShadowsSecondary;
        PopupCornerRadius = EffectiveGlobalToken.BorderRadiusLG;
        MarginToAnchor = EffectiveGlobalToken.UniformlyMarginXXS;
    }
}
