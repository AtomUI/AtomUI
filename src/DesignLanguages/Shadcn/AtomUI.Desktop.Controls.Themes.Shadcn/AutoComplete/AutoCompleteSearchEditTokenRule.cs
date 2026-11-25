using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="AutoCompleteSearchEditToken"/>.</summary>
[ControlTokenRule]
internal sealed class AutoCompleteSearchEditTokenRule : IControlTokenRule<ShadcnToken, AutoCompleteSearchEditToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<AutoCompleteSearchEditToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        AutoCompleteTokenCalculator.Calculate(in context, global, token);
    }
}
