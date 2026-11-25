using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="TreeViewToken"/>.</summary>
[ControlTokenRule]
internal sealed class TreeViewTokenRule : IControlTokenRule<PrimeNGToken, TreeViewToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<TreeViewToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGTreeViewTokens.EvaluateGlobal(in context, global);
        TreeViewTokenCalculator.Calculate(in context, global, token);
        token.DirectoryNodeSelectedBg = global.ColorPrimary;
        token.DirectoryNodeSelectedColor = global.ColorTextLightSolid;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGTreeViewTokens.EvaluateOwn(in context, global, token);
    }
}
