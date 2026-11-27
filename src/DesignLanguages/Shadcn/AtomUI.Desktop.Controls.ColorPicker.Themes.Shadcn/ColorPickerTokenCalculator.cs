using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn values of the shared <see cref="AbstractColorPickerToken" /> layer.
/// </summary>
public static class ColorPickerTokenCalculator
{
    /// <summary>
    /// Assigns the shadcn values to <paramref name="token" />, the shared
    /// <see cref="AbstractColorPickerToken" /> layer of the color picker and the gradient color picker.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<ShadcnToken> context, DesignToken global, AbstractColorPickerToken token)
    {
        // Ant Design formula kept as is: shadcn has no color picker, so the language has no values of its own.
        token.ColorBlockDisabledOpacity = 0.4;
        token.ColorBlockInnerShadows = new BoxShadows(new BoxShadow { IsInset = true, Spread = global.LineWidth, Color = global.ColorFillSecondary });
        token.TransparentBgSize = global.SizeXS;
        token.TriggerPadding = new Thickness(global.UniformlyPaddingXXS - global.LineWidth);
        token.TriggerTextMargin = new Thickness(global.UniformlyMarginXS, 0, global.UniformlyMarginXS - global.UniformlyPaddingXXS + global.LineWidth, 0);
    }
}
