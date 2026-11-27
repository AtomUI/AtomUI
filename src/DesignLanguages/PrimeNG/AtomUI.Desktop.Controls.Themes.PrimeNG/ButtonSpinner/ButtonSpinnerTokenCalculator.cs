using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Media;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG values of the shared <see cref="AbstractButtonSpinnerToken" /> layer.
/// </summary>
public static class ButtonSpinnerTokenCalculator
{
    /// <summary>
    /// Writes the PrimeNG values of the handle buttons into the shared <see cref="AbstractButtonSpinnerToken" />
    /// layer: their width, icon size and state colors. <c>HandleBg</c> is not written here;
    /// <see cref="CalculateFilledHandleBackground" /> derives <c>FilledHandleBg</c> from it.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<PrimeNGToken> context, DesignToken global, AbstractButtonSpinnerToken token)
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
    /// Returns the filled handle background (<c>FilledHandleBg</c>): <c>ColorFillSecondary</c> composed over the
    /// resting handle background <paramref name="handleBg" />.
    /// </summary>
    public static Color CalculateFilledHandleBackground(AbstractButtonSpinnerToken token, DesignToken global, Color handleBg)
    {
        return ColorUtils.OnBackground(global.ColorFillSecondary, handleBg);
    }
}
