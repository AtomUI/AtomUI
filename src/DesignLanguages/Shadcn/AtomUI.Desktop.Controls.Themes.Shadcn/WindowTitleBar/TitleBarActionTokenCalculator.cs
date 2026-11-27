using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn values of the shared <see cref="AbstractTitleBarActionToken" /> layer.
/// </summary>
public static class TitleBarActionTokenCalculator
{
    /// <summary>
    /// Fills the <see cref="AbstractTitleBarActionToken" /> layer with the shadcn values: the background and
    /// foreground colors of a caption button in an active window at rest, hovered and pressed, in an inactive
    /// window at rest and hovered, and the inset of the button background.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<ShadcnToken> context, DesignToken global, AbstractTitleBarActionToken token)
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


    /// <summary>
    /// Provides the corner radius of a caption button, sized to round its background into a circle: half of the icon
    /// size together with the vertical padding.
    /// </summary>
    public static CornerRadius CalculateCaptionCornerRadius(double iconSize, Thickness padding) =>
        new((iconSize + padding.Top + padding.Bottom) / 2);
}
