using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="AutoCompleteTextAreaToken"/>.</summary>
[ControlTokenRule]
internal sealed class AutoCompleteTextAreaTokenRule : IControlTokenRule<AntDesignToken, AutoCompleteTextAreaToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<AutoCompleteTextAreaToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        AutoCompleteTokenCalculator.Calculate(in context, global, token);
    }
}
