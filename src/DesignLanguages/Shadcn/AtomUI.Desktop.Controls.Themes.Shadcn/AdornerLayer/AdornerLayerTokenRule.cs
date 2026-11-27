using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="AdornerLayerToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class AdornerLayerTokenRule : IControlTokenRule<ShadcnToken, AdornerLayerToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<AdornerLayerToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        token.FocusOutlineOffset = global.LineWidth;
    }
}
