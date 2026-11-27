using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign view of <see cref="SplitterToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class SplitterTokenRule : IControlTokenRule<AntDesignToken, SplitterToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<SplitterToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        token.SplitBarDraggableSize = 20;
        token.SplitBarSize          = 2;
        token.SplitTriggerSize      = 6;

        token.SplitBarHandleSize    = token.SplitTriggerSize + global.FontSizeSM * 2;
        var collapseOffset = token.SplitBarSize / 2d + 1d;
        token.SplitBarCollapseOffset         = collapseOffset;
        token.SplitBarCollapseOffsetNegative = -(global.FontSizeSM + collapseOffset);
        token.SplitBarCollapseCrossOffset    = -token.SplitBarDraggableSize / 2d;
        token.HandleLineColor                = global.ControlItemBgHover;
        token.HandleLineHoverColor           = global.ControlItemBgActive;
        token.HandleLineDragColor            = global.ControlItemBgActiveHover;
        token.HandleGripColor = global.ColorFill;

        token.HandleGripHoverColor           = token.HandleGripColor;
        token.HandleGripDragColor            = token.HandleGripColor;
        token.HandleIconColor                = global.ColorText;

        token.HandleIconHoverColor           = token.HandleIconColor;
        token.HandleIconPressedColor         = token.HandleIconColor;
        token.HandleLineThickness            = token.SplitBarSize;
        token.HandleIconSize                 = global.FontSizeSM;
    }
}
