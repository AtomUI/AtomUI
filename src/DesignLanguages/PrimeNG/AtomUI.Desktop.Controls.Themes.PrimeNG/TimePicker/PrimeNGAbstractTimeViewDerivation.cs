using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG values of the shared <see cref="AbstractTimeViewToken"/> layer.</summary>
public static class PrimeNGAbstractTimeViewDerivation
{
    public static void DeriveShared(in DerivationContext<PrimeNGToken> context, DesignToken global, AbstractTimeViewToken token)
    {
        token.ItemMargin        = new Thickness(global.UniformlyMarginXXS, 0);
        token.PopupArrowVisible = true;
    }


    public static Thickness CalculateTimeItemPadding(double itemWidth, double itemHeight) =>
        new((itemWidth - itemHeight) / 2, 0, 0, 0);
}
