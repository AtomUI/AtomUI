using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="TreeViewToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGTreeViewDerivation : IControlDerivation<PrimeNGToken, TreeViewToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<TreeViewToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGTreeViewTokens.EvaluateGlobal(in context, global);
        PrimeNGAbstractTreeViewDerivation.DeriveShared(in context, global, token);
        token.DirectoryNodeSelectedBg = global.ColorPrimary;
        token.DirectoryNodeSelectedColor = global.ColorTextLightSolid;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGTreeViewTokens.EvaluateOwn(in context, global, token);
    }
}
