using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;

namespace AtomUI.Toolkits.GalleryBase.TokenRules;

/// <summary>
/// The Generic view of <see cref="ShowCaseZoomOverlayToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class GenericShowCaseZoomOverlayTokenRule : IControlTokenRule<LanguageDesignToken, ShowCaseZoomOverlayToken>
{
    public void Apply(in TokenEvaluationContext<LanguageDesignToken> context, ControlTokenTarget<ShowCaseZoomOverlayToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        token.CardPadding        = new Thickness(
            global.SizeUnit * 10,
            global.SizeUnit * 12,
            global.SizeUnit * 10,
            global.SizeUnit * 8);
        token.HeaderMargin       = new Thickness(0, 0, 0, global.SizeUnit * 4);
        token.TitleFontWeight    = global.FontWeightStrong;
        token.DescriptionMargin  = new Thickness(0, global.SizeUnit * 2 + 2, 0, 0);
    }
}
