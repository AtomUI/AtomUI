using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="AdornerLayerToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class AdornerLayerTokenRule : IControlTokenRule<PrimeNGToken, AdornerLayerToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<AdornerLayerToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: no control-global overrides; EvaluateGlobal writes nothing.
        PrimeNGAdornerLayerTokens.EvaluateGlobal(in context, global);
        token.FocusOutlineOffset = global.LineWidth;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGAdornerLayerTokens.EvaluateOwn(in context, global, token);
    }
}
