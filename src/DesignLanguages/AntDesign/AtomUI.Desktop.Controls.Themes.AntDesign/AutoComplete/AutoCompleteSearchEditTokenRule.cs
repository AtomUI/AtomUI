using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="AutoCompleteSearchEditToken"/>.</summary>
[ControlTokenRule]
internal sealed class AutoCompleteSearchEditTokenRule : IControlTokenRule<AntDesignToken, AutoCompleteSearchEditToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<AutoCompleteSearchEditToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        AutoCompleteTokenCalculator.Calculate(in context, global, token);
    }
}
