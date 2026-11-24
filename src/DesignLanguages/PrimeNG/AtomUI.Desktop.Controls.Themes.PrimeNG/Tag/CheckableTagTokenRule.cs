using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="CheckableTagToken"/>.</summary>
[ControlTokenRule]
internal sealed class CheckableTagTokenRule : IControlTokenRule<PrimeNGToken, CheckableTagToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<CheckableTagToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        TagTokenCalculator.Calculate(in context, global, token);
    }
}
