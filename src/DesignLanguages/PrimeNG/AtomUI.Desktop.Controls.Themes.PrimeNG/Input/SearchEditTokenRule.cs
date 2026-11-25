using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="SearchEditToken"/>.</summary>
[ControlTokenRule]
internal sealed class SearchEditTokenRule : IControlTokenRule<PrimeNGToken, SearchEditToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<SearchEditToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGSearchEditTokens.EvaluateGlobal(in context, global);
        token.InputFontSizeSM = global.FontSize;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGSearchEditTokens.EvaluateOwn(in context, global, token);
    }
}
