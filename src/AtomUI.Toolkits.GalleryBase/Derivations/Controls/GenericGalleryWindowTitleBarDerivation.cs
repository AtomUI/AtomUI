using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Toolkits.GalleryBase.Derivations;

/// <summary>The Generic view of <see cref="GalleryWindowTitleBarToken"/>.</summary>
[ControlDerivation]
internal sealed class GenericGalleryWindowTitleBarDerivation : IControlDerivation<LanguageDesignToken, GalleryWindowTitleBarToken>
{
    public void Derive(in DerivationContext<LanguageDesignToken> context, ControlView<GalleryWindowTitleBarToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.MenuFontWeight = FontWeight.Normal;
        token.MenuMargin     = new Thickness(0, 0, global.SizeUnit * 2, 0);
    }
}
