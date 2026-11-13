using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class PopupToken : AbstractPopupCornerToken
{

    public partial BoxShadows OverlayHostShadow { get; set; }

    public partial BoxShadows PopupRootShadow { get; set; }

    public partial double MarginToAnchor { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);

        OverlayHostShadow = EffectiveGlobalToken.BoxShadowsSecondary;
        // 窗口型与 Overlay Popup 使用同一阴影；定位按残余阴影补偿，内容相对 anchor 的位置与阴影厚度无关
        PopupRootShadow = EffectiveGlobalToken.BoxShadowsSecondary;
        MarginToAnchor = EffectiveGlobalToken.UniformlyMarginXXS;
    }
}
