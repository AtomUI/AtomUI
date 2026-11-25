using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="SearchEditToken"/>.</summary>
[ControlTokenRule]
internal sealed class SearchEditTokenRule : IControlTokenRule<AntDesignToken, SearchEditToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<SearchEditToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        token.InputFontSizeSM = global.FontSize;
    }
}
