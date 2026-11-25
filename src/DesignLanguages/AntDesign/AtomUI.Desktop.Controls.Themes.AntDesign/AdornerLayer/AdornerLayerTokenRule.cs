using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="AdornerLayerToken"/>.</summary>
[ControlTokenRule]
internal sealed class AdornerLayerTokenRule : IControlTokenRule<AntDesignToken, AdornerLayerToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<AdornerLayerToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        token.FocusOutlineOffset = global.LineWidth;
    }
}
