using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG values of the shared <see cref="AbstractLineTabToken"/> layer.</summary>
public static class PrimeNGAbstractLineTabDerivation
{
    public static void DeriveShared(in DerivationContext<PrimeNGToken> context, DesignToken global, AbstractLineTabToken token)
    {
        PrimeNGAbstractTabDerivation.DeriveShared(in context, global, token);
        token.InkBarColor = global.ColorPrimary;
        token.InkBarThickness = global.LineWidthBold;
        token.HorizontalItemGutter = 32;
        token.HorizontalItemPadding = new Thickness(0, global.UniformlyPaddingSM);
        token.HorizontalItemPaddingLG = new Thickness(0, global.UniformlyPadding);
        token.HorizontalItemPaddingSM = new Thickness(0, global.UniformlyPaddingXS);
        token.VerticalItemGutter = global.SpacingXXS;
    }
}
