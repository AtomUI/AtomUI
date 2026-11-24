using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="AutoCompleteTextAreaToken"/>.</summary>
[ControlTokenRule]
internal sealed class AutoCompleteTextAreaTokenRule : IControlTokenRule<ShadcnToken, AutoCompleteTextAreaToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<AutoCompleteTextAreaToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        AutoCompleteTokenCalculator.Calculate(in context, global, token);
    }
}
