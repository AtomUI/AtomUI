using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class TourToken : AbstractControlDesignToken
{
    
    /// <summary>
    /// 关闭按钮尺寸
    /// Close button size
    /// </summary>
    public partial double CloseBtnSize { get; set; }
    
    /// <summary>
    /// Primary 模式上一步按钮背景色
    /// Background color of previous button in primary type
    /// </summary>
    public partial Color PrimaryPrevBtnBg { get; set; }
    
    /// <summary>
    /// Primary 模式下一步按钮悬浮背景色
    /// Hover background color of next button in primary type
    /// </summary>
    public partial Color PrimaryNextBtnHoverBg { get; set; }
    
    /// <summary>Minimum width of the tour content view.</summary>
    public partial double TourViewMinWidth { get; set; }
    
    /// <summary>Minimum height of the tour content view.</summary>
    public partial double TourViewMinHeight { get; set; }
    
    /// <summary>
    /// 标题字体颜色
    /// Font color of title
    /// </summary>
    public partial Color HeaderColor { get; set; }

    #region 内部使用
    
    /// <summary>Width and height of a step dot in the default tour indicator.</summary>
    public partial double IndicatorSize { get; set; }
    /// <summary>Default gap between the tour popup and the highlighted target.</summary>
    public partial double PopupMarginToAnchor { get; set; }
    /// <summary>Corner radius of the tour popup.</summary>
    public partial CornerRadius TourBorderRadius { get; set; }
    
    #endregion
}
