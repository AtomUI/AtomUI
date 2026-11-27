using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG values of the shared <see cref="AbstractStatisticToken" /> layer.
/// </summary>
public static class StatisticTokenCalculator
{
    /// <summary>
    /// Calls <see cref="StatisticValueTokenCalculator.Calculate" /> and then writes the PrimeNG value into the
    /// shared statistic layer: the title takes the body font size.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<PrimeNGToken> context, DesignToken global, AbstractStatisticToken token)
    {
        StatisticValueTokenCalculator.Calculate(in context, global, token);
        token.TitleFontSize = global.FontSize;
    }
}
