using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign values of the shared <see cref="AbstractPopupCornerToken"/> layer.</summary>
public static class AntDesignAbstractPopupCornerDerivation
{
    public static void DeriveShared(in DerivationContext<AntDesignToken> context, DesignToken global, AbstractPopupCornerToken token)
    {
        token.PopupCornerRadius = global.BorderRadiusLG;
    }
}
