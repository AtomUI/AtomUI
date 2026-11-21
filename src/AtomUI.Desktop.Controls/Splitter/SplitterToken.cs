using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class SplitterToken : AbstractControlDesignToken
{
    
    public SplitterToken()
    {
    }

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

    public partial double SplitBarCollapseOffset { get; set; }
    public partial double SplitBarCollapseOffsetNegative { get; set; }
    public partial double SplitBarCollapseCrossOffset { get; set; }
    public partial double SplitBarHandleSize { get; set; }

    public partial Color HandleLineColor { get; set; }
    public partial Color HandleLineHoverColor { get; set; }
    public partial Color HandleLineDragColor { get; set; }
    public partial Color HandleGripColor { get; set; }
    public partial Color HandleGripHoverColor { get; set; }
    public partial Color HandleGripDragColor { get; set; }
    public partial Color HandleIconColor { get; set; }
    public partial Color HandleIconHoverColor { get; set; }
    public partial Color HandleIconPressedColor { get; set; }
    public partial double HandleLineThickness { get; set; }
    public partial double HandleIconSize { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);

        if (!IsExplicit(SplitterTokenKind.SplitBarDraggableSize))
        {
            SplitBarDraggableSize = 20;
        }

        if (!IsExplicit(SplitterTokenKind.SplitBarSize))
        {
            SplitBarSize          = 2;
        }

        if (!IsExplicit(SplitterTokenKind.SplitTriggerSize))
        {
            SplitTriggerSize      = 6;
        }

        SplitBarHandleSize    = SplitTriggerSize + EffectiveGlobalToken.FontSizeSM * 2;
        var collapseOffset = SplitBarSize / 2d + 1d;
        SplitBarCollapseOffset         = collapseOffset;
        SplitBarCollapseOffsetNegative = -(EffectiveGlobalToken.FontSizeSM + collapseOffset);
        SplitBarCollapseCrossOffset    = -SplitBarDraggableSize / 2d;
        HandleLineColor                = EffectiveGlobalToken.ControlItemBgHover;
        HandleLineHoverColor           = EffectiveGlobalToken.ControlItemBgActive;
        HandleLineDragColor            = EffectiveGlobalToken.ControlItemBgActiveHover;

        if (!IsExplicit(SplitterTokenKind.HandleGripColor))
        {
            HandleGripColor = EffectiveGlobalToken.ColorFill;
        }

        HandleGripHoverColor           = HandleGripColor;
        HandleGripDragColor            = HandleGripColor;

        if (!IsExplicit(SplitterTokenKind.HandleIconColor))
        {
            HandleIconColor                = EffectiveGlobalToken.ColorText;
        }

        HandleIconHoverColor           = HandleIconColor;
        HandleIconPressedColor         = HandleIconColor;
        HandleLineThickness            = SplitBarSize;
        HandleIconSize                 = EffectiveGlobalToken.FontSizeSM;
    }
    
}
