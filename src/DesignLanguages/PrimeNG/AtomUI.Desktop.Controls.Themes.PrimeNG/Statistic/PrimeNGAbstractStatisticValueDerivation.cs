using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG values of the shared <see cref="AbstractStatisticValueToken"/> layer.</summary>
public static class PrimeNGAbstractStatisticValueDerivation
{
    public static void DeriveShared(in DerivationContext<PrimeNGToken> context, DesignToken global, AbstractStatisticValueToken token)
    {
        token.ContentFontSize = global.FontSizeHeading3;
    }
}
