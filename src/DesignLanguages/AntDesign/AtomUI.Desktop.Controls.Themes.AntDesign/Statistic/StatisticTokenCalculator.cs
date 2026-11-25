using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign values of the shared <see cref="AbstractStatisticToken"/> layer.</summary>
public static class StatisticTokenCalculator
{
    public static void Calculate(in TokenEvaluationContext<AntDesignToken> context, DesignToken global, AbstractStatisticToken token)
    {
        StatisticValueTokenCalculator.Calculate(in context, global, token);
        token.TitleFontSize = global.FontSize;
    }
}
