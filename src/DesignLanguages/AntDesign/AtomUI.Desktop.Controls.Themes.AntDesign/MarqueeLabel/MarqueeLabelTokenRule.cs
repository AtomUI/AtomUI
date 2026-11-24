using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="MarqueeLabelToken"/>.</summary>
[ControlTokenRule]
internal sealed class MarqueeLabelTokenRule : IControlTokenRule<AntDesignToken, MarqueeLabelToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<MarqueeLabelToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        token.CycleSpace   = 200;
        token.DefaultSpeed = 150;
    }
}
