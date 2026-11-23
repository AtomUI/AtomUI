using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG values of the shared <see cref="AbstractDialogToken"/> layer.</summary>
public static class PrimeNGAbstractDialogDerivation
{
    public static void DeriveShared(in DerivationContext<PrimeNGToken> context, DesignToken global, AbstractDialogToken token)
    {
        token.HeaderBg       = Colors.Transparent;
        token.HeaderFontSize = global.FontSizeHeading5;
        token.ContentBg      = global.ColorBgElevated;
        token.HeaderColor    = global.ColorTextHeading;
        token.ContentPadding = new Thickness(global.PaddingContentHorizontalLG, 0,
            global.PaddingContentHorizontalLG, 0);
        token.WindowContentPadding = new Thickness(
            global.PaddingContentHorizontalLG,
            global.PaddingContentVerticalLG,
            global.PaddingContentHorizontalLG,
            global.PaddingContentVerticalLG);



        token.HeaderMarginBottom = new Thickness(0, 0, 0, global.UniformlyMarginXS);
        token.LogoSize           = global.SizeLG;

        token.CloseBtnSize = global.ControlHeight;
        token.FooterPadding = new Thickness(global.PaddingContentHorizontalLG, 0,
            global.PaddingContentHorizontalLG, global.UniformlyPaddingMD);
        token.FooterMarginTop    = new Thickness(0, global.UniformlyMarginXS, 0, 0);
        token.FooterBg           = Colors.Transparent;
        token.ButtonGroupSpacing = global.SpacingXS;
        token.LoadingIndicatorMargin = new Thickness(0, 0, 0, global.UniformlyMargin);
    }


    public static Thickness CalculateHeaderPadding(AbstractDialogToken token, DesignToken global) =>
        new(global.PaddingContentHorizontalLG, global.UniformlyPaddingSM,
            global.PaddingContentHorizontalSM, 0);


    public static double CalculateMinimumHeight(AbstractDialogToken token, DesignToken global, Thickness headerPadding) =>
        global.ControlHeightLG + headerPadding.Top + headerPadding.Bottom + global.UniformlyMarginXS;
}
