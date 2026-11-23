using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="SplitterToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnSplitterDerivation : IControlDerivation<ShadcnToken, SplitterToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<SplitterToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
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
        token.HandleGripColor = global.ColorTextSecondary;

        token.HandleGripHoverColor           = token.HandleGripColor;
        token.HandleGripDragColor            = token.HandleGripColor;
        token.HandleIconColor                = global.ColorText;

        token.HandleIconHoverColor           = token.HandleIconColor;
        token.HandleIconPressedColor         = token.HandleIconColor;
        token.HandleLineThickness            = token.SplitBarSize;
        token.HandleIconSize                 = global.FontSizeSM;
        var splitterForeground = global.ColorText;
        var splitterSurface    = global.ColorBgContainer;
        token.HandleLineColor      = global.ColorSplit;
        token.HandleLineHoverColor = ShadcnRecipes.CompositeOver(splitterForeground, 0.25, splitterSurface);
        token.HandleLineDragColor  = ShadcnRecipes.CompositeOver(splitterForeground, 0.45, splitterSurface);
        token.HandleGripHoverColor = ShadcnRecipes.CompositeOver(splitterForeground, 0.8, splitterSurface);
        token.HandleGripDragColor  = splitterForeground;
    }
}
