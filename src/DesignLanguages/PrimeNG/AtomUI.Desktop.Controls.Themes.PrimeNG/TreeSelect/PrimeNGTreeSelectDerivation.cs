using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="TreeSelectToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGTreeSelectDerivation : IControlDerivation<PrimeNGToken, TreeSelectToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<TreeSelectToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGTreeSelectTokens.EvaluateGlobal(in context, global);
        PrimeNGAbstractSelectInputDerivation.DeriveShared(in context, global, token);
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
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGTreeSelectTokens.EvaluateOwn(in context, global, token);
    }
}
