using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Own Tokens of the <see cref="ScrollViewer" />: thickness, corner radius and colors of the scroll bar thumbs in
/// lite and normal mode, and the insets of the scroll bar content frames.
/// </summary>
[ControlDesignToken]
public sealed partial class ScrollViewerToken : AbstractControlDesignToken
{
    
    /// <summary>
    /// Thickness of a scroll bar thumb in lite mode while it is not expanded. An expanded thumb uses
    /// <see cref="NormalModeThumbThickness" />.
    /// </summary>
    public partial double LiteModeThumbThickness { get; set; }
    
    /// <summary>
    /// Thickness of a scroll bar thumb in normal mode, and of an expanded thumb in lite mode, for example under the
    /// pointer or while pressed.
    /// </summary>
    public partial double NormalModeThumbThickness { get; set; }
    
    /// <summary>
    /// Corner radius of a scroll bar thumb.
    /// </summary>
    public partial CornerRadius ThumbCornerRadius { get; set; }

    /// <summary>
    /// Corner radius of an indicator scroll bar thumb.
    /// </summary>
    public partial CornerRadius IndicatorThumbCornerRadius { get; set; }

    /// <summary>
    /// Resting background color of a scroll bar thumb.
    /// </summary>
    public partial Color ThumbBg { get; set; }

    /// <summary>
    /// Background color of a scroll bar thumb under the pointer.
    /// </summary>
    public partial Color ThumbHoverBg { get; set; }
    
    /// <summary>
    /// Background color of a pressed scroll bar thumb.
    /// </summary>
    public partial Color ThumbActiveBg { get; set; }
    
    #region 内部 Token 定义
    /// <summary>
    /// Insets of the content frame in a horizontal scroll bar.
    /// </summary>
    public partial Thickness ScrollBarContentHPadding { get; set; }
    
    /// <summary>
    /// Insets of the content frame in a vertical scroll bar.
    /// </summary>
    public partial Thickness ScrollBarContentVPadding { get; set; }
    
    #endregion
}
