using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign values of the shared <see cref="AbstractTreeViewToken"/> layer.</summary>
public static class TreeViewTokenCalculator
{
    public static void Calculate(in TokenEvaluationContext<AntDesignToken> context, DesignToken global, AbstractTreeViewToken token)
    {
        TreePresentationLayerTokenCalculator.Calculate(in context, global, token);


        token.DragIndicatorLineWidth = global.LineWidthBold;
    }
}
