using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn values of the shared <see cref="AbstractTreePresentationToken" /> layer.
/// </summary>
public static class TreePresentationLayerTokenCalculator
{
    /// <summary>
    /// Assigns the shadcn values to <paramref name="token" />, the shared
    /// <see cref="AbstractTreePresentationToken" /> layer. Every value comes from
    /// <see cref="TreePresentationTokenCalculator.Calculate" />.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<ShadcnToken> context, DesignToken global, AbstractTreePresentationToken token)
    {
        var presentation = TreePresentationTokenCalculator.Calculate(global);
        token.HeaderHeight = presentation.HeaderHeight;
        token.NodeHoverBg = presentation.NodeHoverBg;
        token.NodeSelectedBg = presentation.NodeSelectedBg;
        token.NodeSelectedColor = presentation.NodeSelectedColor;
        token.TreeItemMargin = presentation.TreeItemMargin;
        token.TreeItemHeaderPadding = presentation.TreeItemHeaderPadding;
        token.TreeItemHeaderMargin = presentation.TreeItemHeaderMargin;
        token.TreeNodeSwitcherMargin = presentation.TreeNodeSwitcherMargin;
        token.TreeNodeIconMargin = presentation.TreeNodeIconMargin;
        token.FilterHighlightColor = presentation.FilterHighlightColor;
    }
}
