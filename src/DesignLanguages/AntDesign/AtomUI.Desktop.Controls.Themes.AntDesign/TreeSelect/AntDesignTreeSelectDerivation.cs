using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="TreeSelectToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignTreeSelectDerivation : IControlDerivation<AntDesignToken, TreeSelectToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<TreeSelectToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractSelectInputDerivation.DeriveShared(in context, global, token);
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
