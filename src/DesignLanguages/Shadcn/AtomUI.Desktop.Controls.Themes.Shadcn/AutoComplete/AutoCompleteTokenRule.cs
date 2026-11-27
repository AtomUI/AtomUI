using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="AutoCompleteToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class AutoCompleteTokenRule : IControlTokenRule<ShadcnToken, AutoCompleteToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<AutoCompleteToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        InputTokenCalculator.CalculateGlobal(in context, global);
        AutoCompleteTokenCalculator.Calculate(in context, global, token);
    }
}
