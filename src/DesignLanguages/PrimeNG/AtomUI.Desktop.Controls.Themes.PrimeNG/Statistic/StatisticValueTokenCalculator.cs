using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG values of the shared <see cref="AbstractStatisticValueToken" /> layer.
/// </summary>
public static class StatisticValueTokenCalculator
{
    /// <summary>
    /// Writes the PrimeNG value into the shared statistic value layer: the value takes the heading 3 font size.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<PrimeNGToken> context, DesignToken global, AbstractStatisticValueToken token)
    {
        token.ContentFontSize = global.FontSizeHeading3;
    }
}
