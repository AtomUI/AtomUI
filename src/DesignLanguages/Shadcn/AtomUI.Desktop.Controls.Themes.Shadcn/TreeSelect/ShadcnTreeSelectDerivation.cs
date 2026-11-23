using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="TreeSelectToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnTreeSelectDerivation : IControlDerivation<ShadcnToken, TreeSelectToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<TreeSelectToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnAbstractSelectInputDerivation.DeriveShared(in context, global, token);
        var presentation = TreePresentationTokenValues.Calculate(global);
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
