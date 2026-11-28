using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of the desktop segmented control: item colors per state, item heights and paddings per size
/// level, and the track fill and padding.
/// </summary>
[ControlDesignToken]
public sealed partial class SegmentedToken : AbstractControlDesignToken
{

    /// <summary>
    /// Label and icon foreground of an unselected item at rest.
    /// </summary>
    public partial Color ItemColor { get; set; }

    /// <summary>
    /// Label and icon foreground of an unselected item under the pointer, on <see cref="ItemHoverBg" />.
    /// </summary>
    public partial Color ItemHoverColor { get; set; }

    /// <summary>
    /// Fill of an unselected item under the pointer. Pair with <see cref="ItemHoverColor" />.
    /// </summary>
    public partial Color ItemHoverBg { get; set; }

    /// <summary>
    /// Fill of an unselected item while it is pressed.
    /// </summary>
    public partial Color ItemActiveBg { get; set; }

    /// <summary>
    /// Fill behind the selected item. Pair with <see cref="ItemSelectedColor" />.
    /// </summary>
    public partial Color ItemSelectedBg { get; set; }

    /// <summary>
    /// Foreground of the selected label and icon on <see cref="ItemSelectedBg" />.
    /// </summary>
    public partial Color ItemSelectedColor { get; set; }

    /// <summary>
    /// Space between the track edge and its segmented items.
    /// </summary>
    public partial Thickness TrackPadding { get; set; }

    /// <summary>
    /// Minimum height of a large segmented item.
    /// </summary>
    public partial double ItemMinHeightLG { get; set; }

    /// <summary>
    /// Minimum height of a regular segmented item.
    /// </summary>
    public partial double ItemMinHeight { get; set; }

    /// <summary>
    /// Minimum height of a small segmented item.
    /// </summary>
    public partial double ItemMinHeightSM { get; set; }

    /// <summary>
    /// Background of the track underneath all items and the selection indicator.
    /// </summary>
    public partial Color TrackBg { get; set; }

    /// <summary>
    /// Border color of the outer track frame. Defaults to transparent.
    /// </summary>
    public partial Color TrackBorderColor { get; set; }

    /// <summary>
    /// Border thickness of the outer track frame. Defaults to zero.
    /// </summary>
    public partial Thickness TrackBorderThickness { get; set; }

    /// <summary>
    /// Border color of the selected item thumb. Defaults to transparent.
    /// </summary>
    public partial Color ItemSelectedBorderColor { get; set; }

    /// <summary>
    /// Border thickness of the selected item thumb. Defaults to zero.
    /// </summary>
    public partial Thickness ItemSelectedBorderThickness { get; set; }

    // Internal tokens.
    /// <summary>
    /// Inset of the label and icon within a large or regular-size segmented item.
    /// </summary>
    public partial Thickness SegmentedItemPadding { get; set; }
    /// <summary>
    /// Inset of the label and icon within a small segmented item.
    /// </summary>
    public partial Thickness SegmentedItemPaddingSM { get; set; }
    /// <summary>
    /// Space around the content of a segmented item that has an icon. It does not apply to items without an icon.
    /// </summary>
    public partial Thickness SegmentedItemContentMargin { get; set; }
    
}
