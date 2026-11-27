using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="TreeSelectToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class TreeSelectTokenRule : IControlTokenRule<ShadcnToken, TreeSelectToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<TreeSelectToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        SelectInputTokenCalculator.Calculate(in context, global, token);
        var presentation = TreePresentationTokenCalculator.Calculate(global);
        // Ant Design formula kept as is: shadcn has no TreeSelect, so the language has no values of its own.
        token.HeaderHeight = presentation.HeaderHeight;
        token.NodeHoverBg = presentation.NodeHoverBg;
        token.NodeSelectedBg = presentation.NodeSelectedBg;
        token.NodeSelectedColor = presentation.NodeSelectedColor;
        token.TreeItemMargin = presentation.TreeItemMargin;
        token.TreeItemHeaderPadding = presentation.TreeItemHeaderPadding;
        token.TreeItemHeaderMargin = presentation.TreeItemHeaderMargin;
        token.TreeNodeSwitcherMargin = presentation.TreeNodeSwitcherMargin;
        token.TreeNodeIconMargin = presentation.TreeNodeIconMargin;
        token.FilterHighlightColor = global.ColorPrimary;
        token.MinPopupWidth = 300;
        token.PopupContentPadding = new Thickness(global.UniformlyPaddingXXS / 2);
    }
}
