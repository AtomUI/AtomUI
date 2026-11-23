using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class SegmentedToken : AbstractControlDesignToken
{

    /// <summary>Label and icon foreground of an unselected item at rest.</summary>
    public partial Color ItemColor { get; set; }

    /// <summary>Label and icon foreground on <see cref="ItemHoverBg"/> under the pointer.</summary>
    public partial Color ItemHoverColor { get; set; }

    /// <summary>Fill of an unselected item under the pointer. Pair with <see cref="ItemHoverColor"/>.</summary>
    public partial Color ItemHoverBg { get; set; }

    /// <summary>Fill of an item while it is pressed.</summary>
    public partial Color ItemActiveBg { get; set; }

    /// <summary>Fill behind the selected item. Pair with <see cref="ItemSelectedColor"/>.</summary>
    public partial Color ItemSelectedBg { get; set; }

    /// <summary>Foreground of the selected label and icon on <see cref="ItemSelectedBg"/>.</summary>
    public partial Color ItemSelectedColor { get; set; }

    /// <summary>Space between the track edge and its segmented items.</summary>
    public partial Thickness TrackPadding { get; set; }

    /// <summary>Minimum height of a large segmented item.</summary>
    public partial double ItemMinHeightLG { get; set; }

    /// <summary>Minimum height of a regular segmented item.</summary>
    public partial double ItemMinHeight { get; set; }

    /// <summary>Minimum height of a small segmented item.</summary>
    public partial double ItemMinHeightSM { get; set; }

    /// <summary>Background of the track underneath all items and the selection indicator.</summary>
    public partial Color TrackBg { get; set; }

    // 内部 token
    /// <summary>Inset of the label and icon within a segmented item.</summary>
    public partial Thickness SegmentedItemPadding { get; set; }
    /// <summary>Inset of the label and icon within a small segmented item.</summary>
    public partial Thickness SegmentedItemPaddingSM { get; set; }
    /// <summary>Space around the item's custom content.</summary>
    public partial Thickness SegmentedItemContentMargin { get; set; }
    
}
