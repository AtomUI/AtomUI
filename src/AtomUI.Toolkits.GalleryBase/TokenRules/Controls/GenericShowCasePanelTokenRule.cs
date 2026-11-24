using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;

namespace AtomUI.Toolkits.GalleryBase.TokenRules;

/// <summary>The Generic view of <see cref="ShowCasePanelToken"/>.</summary>
[ControlTokenRule]
internal sealed class GenericShowCasePanelTokenRule : IControlTokenRule<LanguageDesignToken, ShowCasePanelToken>
{
    public void Apply(in TokenEvaluationContext<LanguageDesignToken> context, ControlTokenTarget<ShowCasePanelToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        token.ContentMargin = new Thickness(global.SizeUnit * 6);
        token.MinItemWidth  = global.SizeUnit * 90;
        token.MaxColumns    = 2;
        token.ColumnGap     = global.SizeUnit * 4;
        token.RowGap        = global.SizeUnit * 4;
    }
}
