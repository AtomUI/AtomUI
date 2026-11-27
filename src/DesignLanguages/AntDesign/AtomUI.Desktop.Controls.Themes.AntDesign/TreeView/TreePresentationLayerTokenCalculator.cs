using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign values of the shared <see cref="AbstractTreePresentationToken" /> layer.
/// </summary>
public static class TreePresentationLayerTokenCalculator
{
    /// <summary>
    /// Assigns the Ant Design values of the shared <see cref="AbstractTreePresentationToken" /> layer to
    /// <paramref name="token" />, copying the values that <c>TreePresentationTokenCalculator</c> derives from
    /// <paramref name="global" />.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<AntDesignToken> context, DesignToken global, AbstractTreePresentationToken token)
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
