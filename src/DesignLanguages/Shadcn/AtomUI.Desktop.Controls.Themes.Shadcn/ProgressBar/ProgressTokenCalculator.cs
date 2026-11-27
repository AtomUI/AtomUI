using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn values of the shared <see cref="AbstractProgressToken" /> layer.
/// </summary>
public static class ProgressTokenCalculator
{
    /// <summary>
    /// Assigns the shadcn values to <paramref name="token" />, the shared
    /// <see cref="AbstractProgressToken" /> layer: the default progress color and the color of the remaining
    /// part. The line and circle progress calculators start with it.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<ShadcnToken> context, DesignToken global, AbstractProgressToken token)
    {
        token.DefaultColor   = global.ColorInfo;
        token.RemainingColor = global.ColorFillSecondary;
    }
}
