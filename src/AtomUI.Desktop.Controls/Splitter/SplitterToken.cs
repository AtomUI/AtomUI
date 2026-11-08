using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class SplitterToken : AbstractControlDesignToken
{
    
    public SplitterToken()
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
