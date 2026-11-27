using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="TreeSelectToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class TreeSelectTokenRule : IControlTokenRule<PrimeNGToken, TreeSelectToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<TreeSelectToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides of PrimeNGInputStage.Apply; the formulas below and the TreeSelect
        // theme read ColorTextDisabled; none of the others is read by the formulas below.
        PrimeNGTreeSelectTokens.EvaluateGlobal(in context, global);
        SelectInputTokenCalculator.Calculate(in context, global, token);
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
        // Ant Design formula kept as is: the PrimeNG tree select stage at the end of the method replaces only
        // PopupContentPadding of this group.
        token.FilterHighlightColor = global.ColorPrimary;
        token.MinPopupWidth = 300;
        token.PopupContentPadding = new Thickness(global.UniformlyPaddingXXS / 2);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGTreeSelectTokens.EvaluateOwn(in context, global, token);
    }
}
