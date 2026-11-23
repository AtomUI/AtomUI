using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG values of the shared <see cref="AbstractTitleBarActionToken"/> layer.</summary>
public static class PrimeNGAbstractTitleBarActionDerivation
{
    public static void DeriveShared(in DerivationContext<PrimeNGToken> context, DesignToken global, AbstractTitleBarActionToken token)
    {
        token.ActiveBgColor = global.ColorFillTertiary;
        token.ActiveColor = global.ColorTextSecondary;
        token.ActiveHoverBgColor = global.ColorFillSecondary;
        token.ActivePressedBgColor = global.ColorFill;
        token.CaptionButtonBackgroundInset = new Thickness(global.SizeUnit / 2);
        token.InactiveBgColor = global.ColorFillQuaternary;
        token.InactiveColor = global.ColorTextQuaternary;
        token.InactiveHoverBgColor = global.ColorFillTertiary;
    }


    public static CornerRadius CalculateCaptionCornerRadius(double iconSize, Thickness padding) =>
        new((iconSize + padding.Top + padding.Bottom) / 2);
}
