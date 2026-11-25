using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="CircleProgressToken"/>.</summary>
[ControlTokenRule]
internal sealed class CircleProgressTokenRule : IControlTokenRule<AntDesignToken, CircleProgressToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<CircleProgressToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        CircleProgressTokenCalculator.Calculate(in context, global, token);
    }
}
