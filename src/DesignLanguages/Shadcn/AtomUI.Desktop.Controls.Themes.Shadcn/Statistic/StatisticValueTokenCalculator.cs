using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn values of the shared <see cref="AbstractStatisticValueToken" /> layer.
/// </summary>
public static class StatisticValueTokenCalculator
{
    /// <summary>
    /// Assigns the shadcn values to <paramref name="token" />, the shared
    /// <see cref="AbstractStatisticValueToken" /> layer: the font size of the statistic value.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<ShadcnToken> context, DesignToken global, AbstractStatisticValueToken token)
    {
        token.ContentFontSize = global.FontSizeHeading3;
    }
}
