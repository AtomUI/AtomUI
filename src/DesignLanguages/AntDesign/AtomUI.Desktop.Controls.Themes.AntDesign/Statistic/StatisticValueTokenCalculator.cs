using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign values of the shared <see cref="AbstractStatisticValueToken" /> layer.
/// </summary>
public static class StatisticValueTokenCalculator
{
    /// <summary>
    /// Assigns the Ant Design values of the shared <see cref="AbstractStatisticValueToken" /> layer to
    /// <paramref name="token" />, here the font size of the statistic value.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<AntDesignToken> context, DesignToken global, AbstractStatisticValueToken token)
    {
        token.ContentFontSize = global.FontSizeHeading3;
    }
}
