using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="SearchEditToken"/>.</summary>
[ControlTokenRule]
internal sealed class SearchEditTokenRule : IControlTokenRule<ShadcnToken, SearchEditToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<SearchEditToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        token.InputFontSizeSM = global.FontSize;
    }
}
