using AtomUI.Theme.TokenSystem;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class SplitterToken : AbstractControlDesignToken
{
    public const string ID = "Splitter";

    public SplitterToken() : base(ID)
    {
    }

    /// <summary>
    /// Draggable indicator size.
    /// </summary>
    public double SplitBarDraggableSize { get; set; }

    /// <summary>
    /// Visible bar size.
    /// </summary>
    public double SplitBarSize { get; set; }

    /// <summary>
    /// Drag trigger area size.
    /// </summary>
    public double SplitTriggerSize { get; set; }

    public double SplitBarCollapseOffset { get; set; }
    public double SplitBarCollapseOffsetNegative { get; set; }
    public double SplitBarCollapseCrossOffset { get; set; }
    public double SplitBarHandleSize { get; set; }

    public Color HandleLineColor { get; set; }
    public Color HandleLineHoverColor { get; set; }
    public Color HandleLineDragColor { get; set; }
    public Color HandleIconColor { get; set; }
    public Color HandleIconHoverColor { get; set; }
    public Color HandleIconPressedColor { get; set; }
    public double HandleLineThickness { get; set; }
    public double HandleIconSize { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        SplitBarDraggableSize = 20;
        SplitBarSize          = 2;
        SplitTriggerSize      = 6;
        SplitBarHandleSize    = SplitTriggerSize + SharedToken.FontSizeSM * 2;
        var collapseOffset = SplitBarSize / 2d + 1d;
        SplitBarCollapseOffset = collapseOffset;
        SplitBarCollapseOffsetNegative = -(SharedToken.FontSizeSM + collapseOffset);
        SplitBarCollapseCrossOffset = -SplitBarDraggableSize / 2d;
        HandleLineColor       = SharedToken.ControlItemBgHover;
        HandleLineHoverColor  = SharedToken.ControlItemBgActive;
        HandleLineDragColor   = SharedToken.ControlItemBgActiveHover;
        HandleIconColor       = SharedToken.ColorText;
        HandleIconHoverColor  = HandleIconColor;
        HandleIconPressedColor = HandleIconColor;
        HandleLineThickness   = SplitBarSize;
        HandleIconSize        = SharedToken.FontSizeSM;
    }
}
