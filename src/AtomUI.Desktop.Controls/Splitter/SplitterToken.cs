using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class SplitterToken : AbstractControlDesignToken
{

    /// <summary>
    /// Draggable indicator size.
    /// </summary>
    public partial double SplitBarDraggableSize { get; set; }

    /// <summary>
    /// Visible bar size.
    /// </summary>
    public partial double SplitBarSize { get; set; }

    /// <summary>
    /// Drag trigger area size.
    /// </summary>
    public partial double SplitTriggerSize { get; set; }

    /// <summary>Offset along the split axis for the collapse affordance on the positive side of the bar.</summary>
    public partial double SplitBarCollapseOffset { get; set; }
    /// <summary>Offset along the split axis for the collapse affordance on the negative side of the bar.</summary>
    public partial double SplitBarCollapseOffsetNegative { get; set; }
    /// <summary>Cross-axis offset of the collapse affordance relative to the split bar.</summary>
    public partial double SplitBarCollapseCrossOffset { get; set; }
    /// <summary>Size reserved for interaction with the splitter handle.</summary>
    public partial double SplitBarHandleSize { get; set; }

    /// <summary>Resting color of the splitter divider line.</summary>
    public partial Color HandleLineColor { get; set; }
    /// <summary>Divider line color while the pointer is over the handle.</summary>
    public partial Color HandleLineHoverColor { get; set; }
    /// <summary>Divider line color while the splitter is being dragged.</summary>
    public partial Color HandleLineDragColor { get; set; }
    /// <summary>Resting color of the grip that marks the drag area.</summary>
    public partial Color HandleGripColor { get; set; }
    /// <summary>Grip color while the pointer is over the drag area.</summary>
    public partial Color HandleGripHoverColor { get; set; }
    /// <summary>Grip color while the splitter is being dragged.</summary>
    public partial Color HandleGripDragColor { get; set; }
    /// <summary>Resting foreground of the collapse or expand glyph.</summary>
    public partial Color HandleIconColor { get; set; }
    /// <summary>Foreground of the collapse or expand glyph under the pointer.</summary>
    public partial Color HandleIconHoverColor { get; set; }
    /// <summary>Foreground of the collapse or expand glyph while pressed.</summary>
    public partial Color HandleIconPressedColor { get; set; }
    /// <summary>Thickness of the painted divider line.</summary>
    public partial double HandleLineThickness { get; set; }
    /// <summary>Width and height of the collapse or expand glyph.</summary>
    public partial double HandleIconSize { get; set; }
    
}
