using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="MarqueeLabelToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class MarqueeLabelTokenRule : IControlTokenRule<ShadcnToken, MarqueeLabelToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<MarqueeLabelToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // Ant Design formula kept as is: shadcn has no marquee label, so the language has no values of its own.
        token.CycleSpace   = 200;
        token.DefaultSpeed = 150;
    }
}
