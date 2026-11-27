using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="SimplePaginationToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class SimplePaginationTokenRule : IControlTokenRule<PrimeNGToken, SimplePaginationToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<SimplePaginationToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides (UniformlyMarginXS), read by the formulas below
        // (PaginationTokenCalculator).
        PrimeNGSimplePaginationTokens.EvaluateGlobal(in context, global);
        PaginationTokenCalculator.Calculate(in context, global, token);
        token.ItemInputBg = global.ColorBgContainer;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGSimplePaginationTokens.EvaluateOwn(in context, global, token);
    }
}
