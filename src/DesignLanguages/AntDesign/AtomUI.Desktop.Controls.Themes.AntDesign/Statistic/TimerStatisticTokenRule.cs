using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="TimerStatisticToken"/>.</summary>
[ControlTokenRule]
internal sealed class TimerStatisticTokenRule : IControlTokenRule<AntDesignToken, TimerStatisticToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<TimerStatisticToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        StatisticTokenCalculator.Calculate(in context, global, token);
    }
}
