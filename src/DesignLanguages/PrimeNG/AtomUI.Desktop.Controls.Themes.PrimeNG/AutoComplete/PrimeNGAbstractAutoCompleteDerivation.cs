using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG values of the shared <see cref="AbstractAutoCompleteToken"/> layer.</summary>
public static class PrimeNGAbstractAutoCompleteDerivation
{
    public static void DeriveShared(in DerivationContext<PrimeNGToken> context, DesignToken global, AbstractAutoCompleteToken token)
    {
        token.OptionHeight        = global.ControlHeight;
        token.OptionMargin        = default;
        token.OptionBorderRadius  = default;
        token.PopupContentPadding = new Thickness(global.UniformlyPaddingXXS / 2);
        token.MinPopupWidth       = 120;
        token.OptionColor         = global.ColorTextSecondary;
        token.OptionSelectedColor = global.ColorText;
        token.MaxPopupWidth       = 200;
    }
}
