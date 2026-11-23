using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="SplitterToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGSplitterDerivation : IControlDerivation<PrimeNGToken, SplitterToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<SplitterToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGSplitterTokens.EvaluateGlobal(in context, global);
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
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGSplitterTokens.EvaluateOwn(in context, global, token);
    }
}
