using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Media;
using AtomUI.Theme;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG values of the shared <see cref="AbstractImagePreviewerToken"/> layer.</summary>
public static class ImagePreviewerTokenCalculator
{
    public static void Calculate(in TokenEvaluationContext<PrimeNGToken> context, DesignToken global, AbstractImagePreviewerToken token)
    {
        // Floating controls can cover light or dark image content. Pair a dark scrim with light icons.
        token.PreviewOperationColor         = global.ColorWhite.SetAlphaF(0.9);
        token.PreviewOperationHoverColor    = global.ColorWhite;
        token.PreviewOperationColorDisabled = global.ColorWhite.SetAlphaF(0.4);
        token.PreviewOperationSize          = global.FontSizeIcon * 1.5;

        token.ErrorIconSize                 = global.ControlHeightLG * 1.2;
        token.MaskBgColor                   = ColorUtils.FromRgbF(0.3, 0, 0, 0);
        token.DialogMinWidth                = 710;
        token.DialogMinHeight               = 240;
        token.CoverImageWidth               = 200;

        if ((context.Variant.Appearance == ThemeAppearance.Dark))
        {
            token.TitleBarBackgroundColor       = global.ColorBorderSecondary;
        }
        else
        {
            token.TitleBarBackgroundColor       = global.ColorBorderSecondary.Darken(1);
        }

        token.NavButtonBgColor             = global.ColorBlack.SetAlphaF(0.72);
        token.NavButtonBgHoverColor        = global.ColorBlack.SetAlphaF(0.84);
        token.FloatToolbarPadding          = new Thickness(global.UniformlyPaddingLG, 0);
        // The index indicator is transparent and centred above the actions, so only its block padding shows; the default
        // adds none (the former inline-only default never rendered).
        token.FloatToolbarIndicatorPadding = new Thickness(0);
    }
}
