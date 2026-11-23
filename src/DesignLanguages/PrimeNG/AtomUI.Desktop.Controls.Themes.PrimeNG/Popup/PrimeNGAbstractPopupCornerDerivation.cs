using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG values of the shared <see cref="AbstractPopupCornerToken"/> layer.</summary>
public static class PrimeNGAbstractPopupCornerDerivation
{
    public static void DeriveShared(in DerivationContext<PrimeNGToken> context, DesignToken global, AbstractPopupCornerToken token)
    {
        token.PopupCornerRadius = global.BorderRadiusLG;
    }
}
