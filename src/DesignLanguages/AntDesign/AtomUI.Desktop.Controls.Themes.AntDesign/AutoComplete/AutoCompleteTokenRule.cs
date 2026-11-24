using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="AutoCompleteToken"/>.</summary>
[ControlTokenRule]
internal sealed class AutoCompleteTokenRule : IControlTokenRule<AntDesignToken, AutoCompleteToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<AutoCompleteToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        AutoCompleteTokenCalculator.Calculate(in context, global, token);
    }
}
