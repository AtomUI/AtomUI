using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of the <see cref="Tour" /> control. They size the close button and the content view of the popup,
/// and color the title, the navigation buttons and the dots of the default indicator.
/// </summary>
[ControlDesignToken]
public sealed partial class TourToken : AbstractControlDesignToken
{
    
    /// <summary>
    /// Width and height of the close button of a tour step.
    /// </summary>
    public partial double CloseBtnSize { get; set; }
    
    /// <summary>
    /// In the Primary style type: border of the previous-step button, its background and border while the pointer
    /// is over it, and the <c>IndicatorColor</c> of the default tour indicator.
    /// </summary>
    public partial Color PrimaryPrevBtnBg { get; set; }
    
    /// <summary>
    /// Background of the next-step button while the pointer is over it, in the Primary style type.
    /// </summary>
    public partial Color PrimaryNextBtnHoverBg { get; set; }
    
    /// <summary>
    /// Minimum width of the tour content view.
    /// </summary>
    public partial double TourViewMinWidth { get; set; }
    
    /// <summary>
    /// Minimum height of the tour content view.
    /// </summary>
    public partial double TourViewMinHeight { get; set; }
    
    /// <summary>
    /// Foreground color of the title of a tour step.
    /// </summary>
    public partial Color HeaderColor { get; set; }

    #region 内部使用
    
    /// <summary>
    /// Width and height of a step dot in the default tour indicator.
    /// </summary>
    public partial double IndicatorSize { get; set; }
    /// <summary>
    /// Default gap between the tour popup and the highlighted target.
    /// </summary>
    public partial double PopupMarginToAnchor { get; set; }
    /// <summary>
    /// Corner radius of the tour popup.
    /// </summary>
    public partial CornerRadius TourBorderRadius { get; set; }
    
    #endregion
}
