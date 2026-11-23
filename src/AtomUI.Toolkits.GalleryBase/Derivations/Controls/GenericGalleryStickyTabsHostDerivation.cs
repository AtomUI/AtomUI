using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Toolkits.GalleryBase.Derivations;

/// <summary>The Generic view of <see cref="GalleryStickyTabsHostToken"/>.</summary>
[ControlDerivation]
internal sealed class GenericGalleryStickyTabsHostDerivation : IControlDerivation<LanguageDesignToken, GalleryStickyTabsHostToken>
{
    public void Derive(in DerivationContext<LanguageDesignToken> context, ControlView<GalleryStickyTabsHostToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.StickyContentPadding = new Thickness(global.SizeUnit * 7, 0);
        token.StickyBackground     = new SolidColorBrush(global.ColorBgLayout);
        token.StickyBorderBrush    = new SolidColorBrush(global.ColorBorderSecondary);
    }
}
