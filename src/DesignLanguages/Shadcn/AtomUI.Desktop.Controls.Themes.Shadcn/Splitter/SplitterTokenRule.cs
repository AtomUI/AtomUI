using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="SplitterToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class SplitterTokenRule : IControlTokenRule<ShadcnToken, SplitterToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<SplitterToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // Ant Design formula kept as is: registry `resizable` is merged only as the handle line and grip colors,
        // at the end of this method.
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
        // Replaced in place: the hover and drag colors of the grip below are derived from it.
        token.HandleGripColor = global.ColorTextSecondary;

        token.HandleGripHoverColor           = token.HandleGripColor;
        token.HandleGripDragColor            = token.HandleGripColor;
        token.HandleIconColor                = global.ColorText;

        token.HandleIconHoverColor           = token.HandleIconColor;
        token.HandleIconPressedColor         = token.HandleIconColor;
        token.HandleLineThickness            = token.SplitBarSize;
        token.HandleIconSize                 = global.FontSizeSM;
        // registry resizable: the handle line and the grip are foreground composites over the surface, in place of the
        // control-item fills of the formula.
        var splitterForeground = global.ColorText;
        var splitterSurface    = global.ColorBgContainer;
        token.HandleLineColor      = global.ColorSplit;
        token.HandleLineHoverColor = ShadcnRecipes.CompositeOver(splitterForeground, 0.25, splitterSurface);
        token.HandleLineDragColor  = ShadcnRecipes.CompositeOver(splitterForeground, 0.45, splitterSurface);
        token.HandleGripHoverColor = ShadcnRecipes.CompositeOver(splitterForeground, 0.8, splitterSurface);
        token.HandleGripDragColor  = splitterForeground;
    }
}
