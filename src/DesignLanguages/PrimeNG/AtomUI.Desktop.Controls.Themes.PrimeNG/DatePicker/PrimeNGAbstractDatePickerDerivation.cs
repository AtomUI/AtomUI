using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG values of the shared <see cref="AbstractDatePickerToken"/> layer.</summary>
public static class PrimeNGAbstractDatePickerDerivation
{
    public static void DeriveShared(in DerivationContext<PrimeNGToken> context, DesignToken global, AbstractDatePickerToken token)
    {
        PrimeNGAbstractTimeViewDerivation.DeriveShared(in context, global, token);
        token.HeaderMargin = new Thickness(0, 0, 0, 3);
        token.PanelContentPadding = global.PaddingSM;
        token.ButtonsPanelMargin = new Thickness(0, global.UniformlyMarginXS, 0, 0);
    }
}
