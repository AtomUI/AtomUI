using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign values of the shared <see cref="AbstractDialogToken" /> layer.
/// </summary>
public static class DialogTokenCalculator
{
    /// <summary>
    /// Assigns the Ant Design values of the dialog layer to <paramref name="token" />: the transparent header
    /// and footer backgrounds and the elevated content background, the heading of the header, the paddings,
    /// margins and spacing of the header, content and footer, and the sizes of the logo and the close button.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<AntDesignToken> context, DesignToken global, AbstractDialogToken token)
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
    /// The padding of the dialog header: the large content padding on the left, the small padding on top, the
    /// small content padding on the right and no padding at the bottom.
    /// </summary>
    /// <param name="token">
    /// Not read.
    /// </param>
    /// <param name="global">
    /// The global tokens the padding comes from.
    /// </param>
    public static Thickness CalculateHeaderPadding(AbstractDialogToken token, DesignToken global) =>
        new(global.PaddingContentHorizontalLG, global.UniformlyPaddingSM,
            global.PaddingContentHorizontalSM, 0);


    /// <summary>
    /// The minimum height of a dialog: the vertical header padding, the height of a large control and one small
    /// margin.
    /// </summary>
    /// <param name="token">
    /// Not read.
    /// </param>
    /// <param name="global">
    /// The global tokens the control height and the margin come from.
    /// </param>
    /// <param name="headerPadding">
    /// The header padding; only its top and bottom values are read.
    /// </param>
    public static double CalculateMinimumHeight(AbstractDialogToken token, DesignToken global, Thickness headerPadding) =>
        global.ControlHeightLG + headerPadding.Top + headerPadding.Bottom + global.UniformlyMarginXS;
}
