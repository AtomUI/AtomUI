using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn values of the shared <see cref="AbstractDialogToken" /> layer.
/// </summary>
public static class DialogTokenCalculator
{
    /// <summary>
    /// Fills the <see cref="AbstractDialogToken" /> layer with the shadcn values: transparent header and footer
    /// backgrounds, the elevated content background, the paddings and margins of the header, content and footer,
    /// the header font size, the button group spacing, the loading indicator margin, and the sizes of the logo
    /// and the close button.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<ShadcnToken> context, DesignToken global, AbstractDialogToken token)
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


    /// <summary>
    /// Provides the padding of the dialog header: the large content padding on the left, the small uniform padding
    /// on top, the small content padding on the right and no padding at the bottom. The
    /// <paramref name="token" /> is not read.
    /// </summary>
    public static Thickness CalculateHeaderPadding(AbstractDialogToken token, DesignToken global) =>
        new(global.PaddingContentHorizontalLG, global.UniformlyPaddingSM,
            global.PaddingContentHorizontalSM, 0);


    /// <summary>
    /// Provides the minimum height of a dialog: one large control for the header row, the vertical header padding
    /// and one small uniform margin. The <paramref name="token" /> is not read.
    /// </summary>
    public static double CalculateMinimumHeight(AbstractDialogToken token, DesignToken global, Thickness headerPadding) =>
        global.ControlHeightLG + headerPadding.Top + headerPadding.Bottom + global.UniformlyMarginXS;
}
