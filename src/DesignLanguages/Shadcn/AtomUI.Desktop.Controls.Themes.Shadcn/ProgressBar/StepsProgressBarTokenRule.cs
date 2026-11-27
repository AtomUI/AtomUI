using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="StepsProgressBarToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class StepsProgressBarTokenRule : IControlTokenRule<ShadcnToken, StepsProgressBarToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<StepsProgressBarToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        LineProgressTokenCalculator.Calculate(in context, global, token);
        token.ProgressStepMarginInlineEnd = global.UniformlyMarginXXS / 2;

        token.ProgressStepMinWidth = token.ProgressStepMarginInlineEnd;
    }
}
