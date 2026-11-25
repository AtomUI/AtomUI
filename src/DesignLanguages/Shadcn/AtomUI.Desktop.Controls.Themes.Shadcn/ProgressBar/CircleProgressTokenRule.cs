using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="CircleProgressToken"/>.</summary>
[ControlTokenRule]
internal sealed class CircleProgressTokenRule : IControlTokenRule<ShadcnToken, CircleProgressToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<CircleProgressToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        CircleProgressTokenCalculator.Calculate(in context, global, token);
    }
}
