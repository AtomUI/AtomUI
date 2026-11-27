using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="TabStripToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class TabStripTokenRule : IControlTokenRule<ShadcnToken, TabStripToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<TabStripToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        LineTabTokenCalculator.Calculate(in context, global, token);
    }
}
