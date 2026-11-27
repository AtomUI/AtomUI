using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Media;
using AtomUI.Theme;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn values of the shared <see cref="AbstractImagePreviewerToken" /> layer.
/// </summary>
public static class ImagePreviewerTokenCalculator
{
    /// <summary>
    /// Assigns the shadcn values to <paramref name="token" />, the shared
    /// <see cref="AbstractImagePreviewerToken" /> layer of the image previewer and the image group previewer.
    /// The title bar background follows the appearance of the active variant in <paramref name="context" />.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<ShadcnToken> context, DesignToken global, AbstractImagePreviewerToken token)
    {
        // Floating controls can cover light or dark image content. Pair a dark scrim with light icons.
        token.PreviewOperationColor         = global.ColorWhite.SetAlphaF(0.9);
        token.PreviewOperationHoverColor    = global.ColorWhite;
        token.PreviewOperationColorDisabled = global.ColorWhite.SetAlphaF(0.4);
        token.PreviewOperationSize          = global.FontSizeIcon * 1.5;

        // Ant Design formula kept as is: shadcn has no image previewer, so the language has no values of its own.
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
        // The index indicator is transparent and centered above the actions, so only its block padding shows; the default
        // adds none (the former inline-only default never rendered).
        token.FloatToolbarIndicatorPadding = new Thickness(0);
    }
}
