using AtomUI.Theme.TokenSystem;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class PopupHostToken : AbstractControlDesignToken
{
    public const string ID = "PopupHost";
    
    /// <summary>
    /// Popup 圆角
    /// </summary>
    public CornerRadius BorderRadius { get; set; }

    /// <summary>
    /// Popup 阴影
    /// </summary>
    public BoxShadows BoxShadows { get; set; }
    
    /// <summary>
    /// 顶层弹出菜单，距离顶层菜单项的边距
    /// </summary>
    public double MarginToAnchor { get; set; }
    
    public PopupHostToken()
        : base(ID)
    {
    }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        BoxShadows     = SharedToken.BoxShadowsSecondary;
        BorderRadius   = SharedToken.BorderRadiusLG;
        MarginToAnchor = SharedToken.UniformlyMarginXXS;
    }
}