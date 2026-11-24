using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="CheckableTagToken"/>.</summary>
[ControlTokenRule]
internal sealed class CheckableTagTokenRule : IControlTokenRule<ShadcnToken, CheckableTagToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<CheckableTagToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        TagTokenCalculator.Calculate(in context, global, token);
    }
}
