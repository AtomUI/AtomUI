using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG values of the shared <see cref="AbstractStatisticToken"/> layer.</summary>
public static class PrimeNGAbstractStatisticDerivation
{
    public static void DeriveShared(in DerivationContext<PrimeNGToken> context, DesignToken global, AbstractStatisticToken token)
    {
        PrimeNGAbstractStatisticValueDerivation.DeriveShared(in context, global, token);
        token.TitleFontSize = global.FontSize;
    }
}
