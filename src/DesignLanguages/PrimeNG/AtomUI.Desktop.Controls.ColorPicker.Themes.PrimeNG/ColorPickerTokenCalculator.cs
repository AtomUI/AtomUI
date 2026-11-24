using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG values of the shared <see cref="AbstractColorPickerToken"/> layer.</summary>
public static class ColorPickerTokenCalculator
{
    public static void Calculate(in TokenEvaluationContext<PrimeNGToken> context, DesignToken global, AbstractColorPickerToken token)
    {
        token.ColorBlockDisabledOpacity = 0.4;
        token.ColorBlockInnerShadows = new BoxShadows(new BoxShadow { IsInset = true, Spread = global.LineWidth, Color = global.ColorFillSecondary });
        token.TransparentBgSize = global.SizeXS;
        token.TriggerPadding = new Thickness(global.UniformlyPaddingXXS - global.LineWidth);
        token.TriggerTextMargin = new Thickness(global.UniformlyMarginXS, 0, global.UniformlyMarginXS - global.UniformlyPaddingXXS + global.LineWidth, 0);
    }
}
