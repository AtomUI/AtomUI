using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign values of the shared <see cref="AbstractTitleBarActionToken" /> layer.
/// </summary>
public static class TitleBarActionTokenCalculator
{
    /// <summary>
    /// Assigns the Ant Design values of the title bar action layer to <paramref name="token" />: the text and
    /// background colors of the active and inactive states, the hover background of both states, the pressed
    /// background of the active state and the inset of the caption button background.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<AntDesignToken> context, DesignToken global, AbstractTitleBarActionToken token)
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
    /// The corner radius that makes a caption button round: half of the button height, which is the icon size plus
    /// the vertical padding. The horizontal padding is not read.
    /// </summary>
    public static CornerRadius CalculateCaptionCornerRadius(double iconSize, Thickness padding) =>
        new((iconSize + padding.Top + padding.Bottom) / 2);
}
