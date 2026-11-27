using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="SplitButtonToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class SplitButtonTokenRule : IControlTokenRule<PrimeNGToken, SplitButtonToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<SplitButtonToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: no control-global overrides; EvaluateGlobal writes nothing.
        PrimeNGSplitButtonTokens.EvaluateGlobal(in context, global);
        // Ant Design formula kept as is; the PrimeNG stage at the end of the method replaces the two separator
        // colors (PrimeUIX draws no seam between the filled halves).
        token.InsetSeparator         = false;
        token.GutterToFlyout         = global.UniformlyMarginXXS;
        token.SolidSeparatorColor    = global.ColorPrimaryHover;
        token.DangerSeparatorColor   = global.ColorErrorHover;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGSplitButtonTokens.EvaluateOwn(in context, global, token);
    }
}
