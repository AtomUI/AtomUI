using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn values of the shared <see cref="AbstractProgressToken"/> layer.</summary>
public static class ProgressTokenCalculator
{
    public static void Calculate(in TokenEvaluationContext<ShadcnToken> context, DesignToken global, AbstractProgressToken token)
    {
        token.DefaultColor   = global.ColorInfo;
        token.RemainingColor = global.ColorFillSecondary;
    }
}
