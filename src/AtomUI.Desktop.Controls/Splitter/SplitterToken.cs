using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design token of the <see cref="Splitter" /> split bar: the width of the bar between two panes, the size
/// of the drag area, the grip and the collapse buttons, and the colors of the divider line, the grip and the
/// collapse glyph in the resting, pointer-over, pressed and dragging states.
/// </summary>
[ControlDesignToken]
public sealed partial class SplitterToken : AbstractControlDesignToken
{

    /// <summary>
    /// Length of the drag grip along the split bar, and of the two collapse buttons in the same direction.
    /// </summary>
    public partial double SplitBarDraggableSize { get; set; }

    /// <summary>
    /// Visible width of the split bar, the space between two adjacent panes, measured along the split axis.
    /// </summary>
    public partial double SplitBarSize { get; set; }

    /// <summary>
    /// Thickness of the drag bar measured across the split bar. It is the width of the area that accepts a drag.
    /// </summary>
    public partial double SplitTriggerSize { get; set; }

    /// <summary>
    /// Offset along the split axis for the collapse affordance on the positive side of the bar.
    /// </summary>
    public partial double SplitBarCollapseOffset { get; set; }
    /// <summary>
    /// Offset along the split axis for the collapse affordance on the negative side of the bar.
    /// </summary>
    public partial double SplitBarCollapseOffsetNegative { get; set; }
    /// <summary>
    /// Cross-axis offset of the collapse affordance relative to the split bar.
    /// </summary>
    public partial double SplitBarCollapseCrossOffset { get; set; }
    /// <summary>
    /// Size reserved for interaction with the splitter handle.
    /// </summary>
    public partial double SplitBarHandleSize { get; set; }

    /// <summary>
    /// Resting color of the splitter divider line.
    /// </summary>
    public partial Color HandleLineColor { get; set; }
    /// <summary>
    /// Divider line color while the pointer is over the handle.
    /// </summary>
    public partial Color HandleLineHoverColor { get; set; }
    /// <summary>
    /// Divider line color while the splitter is being dragged.
    /// </summary>
    public partial Color HandleLineDragColor { get; set; }
    /// <summary>
    /// Resting color of the grip that marks the drag area.
    /// </summary>
    public partial Color HandleGripColor { get; set; }
    /// <summary>
    /// Grip color while the pointer is over the drag area.
    /// </summary>
    public partial Color HandleGripHoverColor { get; set; }
    /// <summary>
    /// Grip color while the splitter is being dragged.
    /// </summary>
    public partial Color HandleGripDragColor { get; set; }
    /// <summary>
    /// Resting foreground of the collapse or expand glyph.
    /// </summary>
    public partial Color HandleIconColor { get; set; }
    /// <summary>
    /// Foreground of the collapse or expand glyph under the pointer.
    /// </summary>
    public partial Color HandleIconHoverColor { get; set; }
    /// <summary>
    /// Foreground of the collapse or expand glyph while pressed.
    /// </summary>
    public partial Color HandleIconPressedColor { get; set; }
    /// <summary>
    /// Thickness of the painted divider line.
    /// </summary>
    public partial double HandleLineThickness { get; set; }
    /// <summary>
    /// Width and height of the collapse or expand glyph.
    /// </summary>
    public partial double HandleIconSize { get; set; }
    
}
