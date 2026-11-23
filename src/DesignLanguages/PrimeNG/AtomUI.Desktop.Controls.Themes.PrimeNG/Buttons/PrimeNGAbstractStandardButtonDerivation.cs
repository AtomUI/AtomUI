using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG values of the shared <see cref="AbstractStandardButtonToken"/> layer.</summary>
public static class PrimeNGAbstractStandardButtonDerivation
{
    public static void DeriveShared(in DerivationContext<PrimeNGToken> context, DesignToken global, AbstractStandardButtonToken token)
    {
        PrimeNGAbstractButtonGeometryDerivation.DeriveShared(in context, global, token);
        token.FontWeight = FontWeight.Normal;
        token.DefaultBorderColorDisabled = global.ColorBorder;
    }
}
