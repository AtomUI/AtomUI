using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG values of the shared <see cref="AbstractProgressToken" /> layer.
/// </summary>
public static class ProgressTokenCalculator
{
    /// <summary>
    /// Writes the PrimeNG values into the shared progress layer: the default bar color and the color of the
    /// remaining track.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<PrimeNGToken> context, DesignToken global, AbstractProgressToken token)
    {
        token.DefaultColor   = global.ColorInfo;
        token.RemainingColor = global.ColorFillSecondary;
    }
}
