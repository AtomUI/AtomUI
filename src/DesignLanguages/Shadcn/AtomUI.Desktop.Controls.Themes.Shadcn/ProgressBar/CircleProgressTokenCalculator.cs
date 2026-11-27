using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn values of the shared <see cref="AbstractCircleProgressToken" /> layer.
/// </summary>
public static class CircleProgressTokenCalculator
{
    /// <summary>
    /// Assigns the shadcn values to <paramref name="token" />, the shared
    /// <see cref="AbstractCircleProgressToken" /> layer of the circle progress and the dashboard progress. It
    /// starts with <see cref="ProgressTokenCalculator.Calculate" />, which fills the progress layer first.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<ShadcnToken> context, DesignToken global, AbstractCircleProgressToken token)
    {
        ProgressTokenCalculator.Calculate(in context, global, token);
        // Ant Design formula kept as is: shadcn has no circle progress, so the language has no values of its own.
        token.CircleTextColor           = global.ColorText;
        token.CircleMinimumIconSize     = global.SizeXS;
        token.CircleMinimumTextFontSize = global.FontSizeSM - 2;
    }
}
