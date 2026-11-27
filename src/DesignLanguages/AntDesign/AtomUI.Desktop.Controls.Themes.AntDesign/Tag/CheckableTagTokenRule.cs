using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign view of <see cref="CheckableTagToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class CheckableTagTokenRule : IControlTokenRule<AntDesignToken, CheckableTagToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<CheckableTagToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        TagTokenCalculator.Calculate(in context, global, token);
    }
}
