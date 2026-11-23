using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG values of the shared <see cref="AbstractTransferToken"/> layer.</summary>
public static class PrimeNGAbstractTransferDerivation
{
    public static void DeriveShared(in DerivationContext<PrimeNGToken> context, DesignToken global, AbstractTransferToken token)
    {

        token.ListWidth    = 180;
        token.ListHeight   = 200;
        token.HeaderHeight = global.ControlHeightLG;
        token.HeaderPadding = new Thickness(
            global.UniformlyPaddingSM,
            Math.Ceiling((global.ControlHeightLG -
                          global.LineWidth -
                          global.FontHeight) / 2));
    }
}
