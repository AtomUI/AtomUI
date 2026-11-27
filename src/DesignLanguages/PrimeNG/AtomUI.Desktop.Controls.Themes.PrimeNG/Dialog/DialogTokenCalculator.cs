using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG values of the shared <see cref="AbstractDialogToken" /> layer.
/// </summary>
public static class DialogTokenCalculator
{
    /// <summary>
    /// Writes the PrimeNG values into the shared <see cref="AbstractDialogToken" /> layer: the paddings of the header,
    /// content and footer, the title size, the close button size and the spacing of the footer buttons. Header and
    /// footer are transparent; only the content has a background (<c>ColorBgElevated</c>).
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<PrimeNGToken> context, DesignToken global, AbstractDialogToken token)
    {
        // Ant Design formula kept as is; the PrimeNG dialog stage (PrimeNGDialogTokens.EvaluateOwn) replaces
        // the paddings and margins of the header, content and footer, the footer button spacing, the close
        // button size and the title size.
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


    /// <summary>
    /// Returns <c>HeaderPadding</c>, the padding of the dialog header: <c>PaddingContentHorizontalLG</c> on the left,
    /// <c>UniformlyPaddingSM</c> on top, <c>PaddingContentHorizontalSM</c> on the right and none at the bottom.
    /// </summary>
    public static Thickness CalculateHeaderPadding(AbstractDialogToken token, DesignToken global) =>
        new(global.PaddingContentHorizontalLG, global.UniformlyPaddingSM,
            global.PaddingContentHorizontalSM, 0);


    /// <summary>
    /// Returns <c>MinHeight</c>, the minimum height of a dialog: the vertical part of
    /// <paramref name="headerPadding" />, plus <c>ControlHeightLG</c> and <c>UniformlyMarginXS</c>.
    /// </summary>
    public static double CalculateMinimumHeight(AbstractDialogToken token, DesignToken global, Thickness headerPadding) =>
        global.ControlHeightLG + headerPadding.Top + headerPadding.Bottom + global.UniformlyMarginXS;
}
