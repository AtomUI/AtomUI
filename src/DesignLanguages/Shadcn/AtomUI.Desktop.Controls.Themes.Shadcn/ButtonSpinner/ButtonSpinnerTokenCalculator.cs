using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Media;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn values of the shared <see cref="AbstractButtonSpinnerToken" /> layer.
/// </summary>
public static class ButtonSpinnerTokenCalculator
{
    /// <summary>
    /// Assigns the shadcn values to <paramref name="token" />, the shared
    /// <see cref="AbstractButtonSpinnerToken" /> layer of the button spinner.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<ShadcnToken> context, DesignToken global, AbstractButtonSpinnerToken token)
    {

        token.HandleWidth    = global.ControlHeightSM;
        token.HandleIconSize = global.FontSize / 2;
        token.HandleActiveBg = global.ColorFillAlter;

        token.HandleHoverColor  = global.ColorPrimary;
        // Ant Design has no hover fill on the handle buttons (only the icon changes) and the resting icon is the
        // shared icon color; both slots exist so a design language can give the handle its own surface.
        token.HandleHoverBg     = Colors.Transparent;
        token.HandleIconColor   = global.ColorIcon;
        token.HandlePressedColor = global.ColorPrimaryActive;
        token.HandleBorderColor = global.ColorBorder;
    }


    /// <summary>
    /// Returns the color for <c>FilledHandleBg</c>: the secondary fill color (<c>ColorFillSecondary</c>)
    /// composed over <paramref name="handleBg" />.
    /// </summary>
    /// <param name="token">
    /// The button spinner token. Not read.
    /// </param>
    /// <param name="global">
    /// The global token that supplies the secondary fill color.
    /// </param>
    /// <param name="handleBg">
    /// The background of the handle, given as <c>HandleBg</c> by the token rules.
    /// </param>
    public static Color CalculateFilledHandleBackground(AbstractButtonSpinnerToken token, DesignToken global, Color handleBg)
    {
        return ColorUtils.OnBackground(global.ColorFillSecondary, handleBg);
    }
}
