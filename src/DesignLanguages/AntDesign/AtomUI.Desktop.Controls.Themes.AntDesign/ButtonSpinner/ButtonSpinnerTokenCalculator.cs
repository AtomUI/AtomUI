using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Media;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign values of the shared <see cref="AbstractButtonSpinnerToken"/> layer.</summary>
public static class ButtonSpinnerTokenCalculator
{
    public static void Calculate(in TokenEvaluationContext<AntDesignToken> context, DesignToken global, AbstractButtonSpinnerToken token)
    {

        token.HandleWidth    = global.ControlHeightSM;
        token.HandleIconSize = global.FontSize / 2;
        token.HandleActiveBg = global.ColorFillAlter;

        token.HandleHoverColor  = global.ColorPrimary;
        // Ant Design has no hover fill on the handle buttons (only the icon changes) and the resting icon is the
        // shared icon colour; both slots exist so a design language can give the handle its own surface.
        token.HandleHoverBg     = Colors.Transparent;
        token.HandleIconColor   = global.ColorIcon;
        token.HandlePressedColor = global.ColorPrimaryActive;
        token.HandleBorderColor = global.ColorBorder;
    }


    public static Color CalculateFilledHandleBackground(AbstractButtonSpinnerToken token, DesignToken global, Color handleBg)
    {
        return ColorUtils.OnBackground(global.ColorFillSecondary, handleBg);
    }
}
