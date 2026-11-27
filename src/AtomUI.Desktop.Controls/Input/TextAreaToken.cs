using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of the text area: the resize grip, the spacing of the leading and trailing add-ons, and the
/// switches that give the input frame a transparent or bottom-only border.
/// </summary>
[ControlDesignToken]
public sealed partial class TextAreaToken : AbstractControlDesignToken
{
    
    /// <summary>
    /// Stroke of the resize grip at the trailing corner of a resizable text area.
    /// </summary>
    public partial Color ResizeIndicatorLineColor { get; set; }
    
    /// <summary>
    /// Size of the resize grip in the resizable text area.
    /// </summary>
    public partial double ResizeHandleSize { get; set; }
    
    /// <summary>
    /// Padding inside the trailing addon of a regular text area.
    /// </summary>
    public partial Thickness RightAddOnPadding { get; set; }

    /// <summary>
    /// Padding inside the trailing addon of a small text area.
    /// </summary>
    public partial Thickness RightAddOnPaddingSM { get; set; }

    /// <summary>
    /// Padding inside the trailing addon of a large text area.
    /// </summary>
    public partial Thickness RightAddOnPaddingLG { get; set; }

    /// <summary>
    /// Space around the leading addon inside the text area frame.
    /// </summary>
    public partial Thickness LeftInnerAddOnMargin { get; set; }

    /// <summary>
    /// Whether the input frame keeps a transparent border at rest and on hover.
    /// </summary>
    public partial bool TransparentOutlineBorder { get; set; }

    /// <summary>
    /// Whether the input frame draws only its bottom border.
    /// </summary>
    public partial bool BottomOutlineBorder { get; set; }
    
}
