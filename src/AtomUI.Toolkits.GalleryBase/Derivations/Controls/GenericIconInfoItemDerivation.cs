using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using AtomUI.Toolkits.GalleryBase.Controls;

namespace AtomUI.Toolkits.GalleryBase.Derivations;

/// <summary>The Generic view of <see cref="IconInfoItemToken"/>.</summary>
[ControlDerivation]
internal sealed class GenericIconInfoItemDerivation : IControlDerivation<LanguageDesignToken, IconInfoItemToken>
{
    public void Derive(in DerivationContext<LanguageDesignToken> context, ControlView<IconInfoItemToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.IconNameFontSize = 13;
    }
}
