using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign view of <see cref="TabStripToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class TabStripTokenRule : IControlTokenRule<AntDesignToken, TabStripToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<TabStripToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        LineTabTokenCalculator.Calculate(in context, global, token);
    }
}
