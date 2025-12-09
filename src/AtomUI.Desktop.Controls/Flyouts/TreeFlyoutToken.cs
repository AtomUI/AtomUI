using AtomUI.Theme.TokenSystem;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class TreeFlyoutToken : AbstractControlDesignToken
{
    public const string ID = "TreeFlyout";
    
    public TreeFlyoutToken()
        : base(ID)
    {
    }
    
    /// <summary>
    /// 弹出框背景色
    /// </summary>
    public Color PopupBgColor { get; set; }
    
    /// <summary>
    /// 菜单的圆角
    /// </summary>
    public CornerRadius PopupBorderRadius { get; set; }

    /// <summary>
    /// 菜单 Popup 阴影
    /// </summary>
    public BoxShadows PopupBoxShadows { get; set; }

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
        PopupBorderRadius = SharedToken.BorderRadiusLG;
        PopupMinWidth     = 120;
        PopupMaxWidth = 800;

        PopupMinHeight = SharedToken.ControlHeightSM * 3;
        PopupMaxHeight = SharedToken.ControlHeightSM * 30;
        
        PopupContentPadding = new Thickness(SharedToken.UniformlyPaddingXXS, PopupBorderRadius.TopLeft / 2);
        PopupBoxShadows     = SharedToken.BoxShadowsSecondary;
        PopupBgColor        = SharedToken.ColorBgElevated;
    }
}