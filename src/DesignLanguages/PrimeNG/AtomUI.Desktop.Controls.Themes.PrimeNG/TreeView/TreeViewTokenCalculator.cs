using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG values of the shared <see cref="AbstractTreeViewToken" /> layer.
/// </summary>
public static class TreeViewTokenCalculator
{
    /// <summary>
    /// Calls <see cref="TreePresentationLayerTokenCalculator.Calculate" /> and then writes the PrimeNG value into the
    /// shared tree view layer: the line width of the drag indicator.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<PrimeNGToken> context, DesignToken global, AbstractTreeViewToken token)
    {
        TreePresentationLayerTokenCalculator.Calculate(in context, global, token);


        token.DragIndicatorLineWidth = global.LineWidthBold;
    }
}
