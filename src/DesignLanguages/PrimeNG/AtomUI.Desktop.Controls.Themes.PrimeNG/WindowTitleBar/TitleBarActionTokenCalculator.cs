using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG values of the shared <see cref="AbstractTitleBarActionToken" /> layer.
/// </summary>
public static class TitleBarActionTokenCalculator
{
    /// <summary>
    /// Writes the PrimeNG state colors of the title bar's action buttons into the shared
    /// <see cref="AbstractTitleBarActionToken" /> layer, for the active and the inactive window, and the inset of the
    /// caption button background.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<PrimeNGToken> context, DesignToken global, AbstractTitleBarActionToken token)
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
    /// Returns <c>CaptionButtonCornerRadius</c>, the radius that rounds a caption button fully: half of its height,
    /// which is <paramref name="iconSize" /> plus the vertical part of <paramref name="padding" />.
    /// </summary>
    public static CornerRadius CalculateCaptionCornerRadius(double iconSize, Thickness padding) =>
        new((iconSize + padding.Top + padding.Bottom) / 2);
}
