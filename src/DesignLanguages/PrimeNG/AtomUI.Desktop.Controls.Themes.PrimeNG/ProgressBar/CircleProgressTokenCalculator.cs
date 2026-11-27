using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG values of the shared <see cref="AbstractCircleProgressToken" /> layer.
/// </summary>
public static class CircleProgressTokenCalculator
{
    /// <summary>
    /// Calls <see cref="ProgressTokenCalculator.Calculate" /> and then writes the PrimeNG values into the shared
    /// circle progress layer: the color of the circle text and the minimum size of its icon and font.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<PrimeNGToken> context, DesignToken global, AbstractCircleProgressToken token)
    {
        ProgressTokenCalculator.Calculate(in context, global, token);
        token.CircleTextColor           = global.ColorText;
        token.CircleMinimumIconSize     = global.SizeXS;
        token.CircleMinimumTextFontSize = global.FontSizeSM - 2;
    }
}
