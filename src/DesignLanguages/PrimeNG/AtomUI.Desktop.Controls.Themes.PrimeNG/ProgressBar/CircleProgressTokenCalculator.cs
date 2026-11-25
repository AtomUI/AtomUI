using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG values of the shared <see cref="AbstractCircleProgressToken"/> layer.</summary>
public static class CircleProgressTokenCalculator
{
    public static void Calculate(in TokenEvaluationContext<PrimeNGToken> context, DesignToken global, AbstractCircleProgressToken token)
    {
        ProgressTokenCalculator.Calculate(in context, global, token);
        token.CircleTextColor           = global.ColorText;
        token.CircleMinimumIconSize     = global.SizeXS;
        token.CircleMinimumTextFontSize = global.FontSizeSM - 2;
    }
}
