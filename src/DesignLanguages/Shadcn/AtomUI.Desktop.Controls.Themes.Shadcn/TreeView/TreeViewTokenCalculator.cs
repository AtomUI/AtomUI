using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn values of the shared <see cref="AbstractTreeViewToken" /> layer.
/// </summary>
public static class TreeViewTokenCalculator
{
    /// <summary>
    /// Assigns the shadcn values to <paramref name="token" />, the shared
    /// <see cref="AbstractTreeViewToken" /> layer of the tree view and the transfer tree view. It starts with
    /// <see cref="TreePresentationLayerTokenCalculator.Calculate" />, which fills the presentation layer first.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<ShadcnToken> context, DesignToken global, AbstractTreeViewToken token)
    {
        TreePresentationLayerTokenCalculator.Calculate(in context, global, token);


        token.DragIndicatorLineWidth = global.LineWidthBold;
    }
}
