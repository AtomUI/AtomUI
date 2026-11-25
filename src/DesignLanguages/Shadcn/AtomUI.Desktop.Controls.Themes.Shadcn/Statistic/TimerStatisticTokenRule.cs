using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="TimerStatisticToken"/>.</summary>
[ControlTokenRule]
internal sealed class TimerStatisticTokenRule : IControlTokenRule<ShadcnToken, TimerStatisticToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<TimerStatisticToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        StatisticTokenCalculator.Calculate(in context, global, token);
    }
}
