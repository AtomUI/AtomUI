using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using AtomUI.Toolkits.GalleryBase.Controls;

namespace AtomUI.Toolkits.GalleryBase.Derivations;

/// <summary>The Generic view of <see cref="ColorItemControlToken"/>.</summary>
[ControlDerivation]
internal sealed class GenericColorItemControlDerivation : IControlDerivation<LanguageDesignToken, ColorItemControlToken>
{
    public void Derive(in DerivationContext<LanguageDesignToken> context, ControlView<ColorItemControlToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.HexFontSize = 10;
        token.HexOpacity = 0.8;
    }
}
