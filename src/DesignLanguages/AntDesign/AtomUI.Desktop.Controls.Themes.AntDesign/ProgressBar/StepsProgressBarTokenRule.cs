using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="StepsProgressBarToken"/>.</summary>
[ControlTokenRule]
internal sealed class StepsProgressBarTokenRule : IControlTokenRule<AntDesignToken, StepsProgressBarToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<StepsProgressBarToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        LineProgressTokenCalculator.Calculate(in context, global, token);
        token.ProgressStepMarginInlineEnd = global.UniformlyMarginXXS / 2;

        token.ProgressStepMinWidth = token.ProgressStepMarginInlineEnd;
    }
}
