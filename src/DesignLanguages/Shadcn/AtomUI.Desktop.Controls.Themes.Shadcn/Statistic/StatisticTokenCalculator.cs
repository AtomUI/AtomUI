using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn values of the shared <see cref="AbstractStatisticToken" /> layer.
/// </summary>
public static class StatisticTokenCalculator
{
    /// <summary>
    /// Assigns the shadcn values to <paramref name="token" />, the shared
    /// <see cref="AbstractStatisticToken" /> layer of the statistic and the timer statistic. It starts with
    /// <see cref="StatisticValueTokenCalculator.Calculate" />, which fills the value layer first.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<ShadcnToken> context, DesignToken global, AbstractStatisticToken token)
    {
        StatisticValueTokenCalculator.Calculate(in context, global, token);
        token.TitleFontSize = global.FontSize;
    }
}
