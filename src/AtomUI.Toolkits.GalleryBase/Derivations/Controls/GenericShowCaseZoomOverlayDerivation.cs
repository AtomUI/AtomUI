using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;

namespace AtomUI.Toolkits.GalleryBase.Derivations;

/// <summary>The Generic view of <see cref="ShowCaseZoomOverlayToken"/>.</summary>
[ControlDerivation]
internal sealed class GenericShowCaseZoomOverlayDerivation : IControlDerivation<LanguageDesignToken, ShowCaseZoomOverlayToken>
{
    public void Derive(in DerivationContext<LanguageDesignToken> context, ControlView<ShowCaseZoomOverlayToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
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
