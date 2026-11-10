using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class TreeViewFlyoutPresenterToken : AbstractPopupCornerToken
{
    
    public TreeViewFlyoutPresenterToken()

    {
    }
    
    /// <summary>
    /// 弹出框背景色
    /// </summary>
    public Color PopupBgColor { get; set; }

    /// <summary>
    /// 菜单内容边距
    /// </summary>
    public Thickness PopupContentPadding { get; set; }

    /// <summary>
    /// 菜单 Popup 最小宽度
    /// </summary>
    public double PopupMinWidth { get; set; }

    /// <summary>
    /// 菜单 Popup 最大宽度
    /// </summary>
    public double PopupMaxWidth { get; set; }

    /// <summary>
    /// 菜单 Popup 最小高度
    /// </summary>
    public double PopupMinHeight { get; set; }

    /// <summary>
    /// 菜单 Popup 最大高度
    /// </summary>
    public double PopupMaxHeight { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        PopupMinWidth     = 120;
        PopupMaxWidth = 800;

        PopupMinHeight = EffectiveGlobalToken.ControlHeightSM * 3;
        PopupMaxHeight = EffectiveGlobalToken.ControlHeightSM * 30;
        
        PopupContentPadding = new Thickness(EffectiveGlobalToken.UniformlyPaddingXXS, EffectiveGlobalToken.BorderRadiusLG.TopLeft / 2);
        PopupBgColor        = EffectiveGlobalToken.ColorBgElevated;
    }
    
}
