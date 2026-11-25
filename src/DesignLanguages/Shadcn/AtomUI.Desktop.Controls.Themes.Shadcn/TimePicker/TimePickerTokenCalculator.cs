using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn values of the shared <see cref="AbstractTimePickerToken"/> layer.</summary>
public static class TimePickerTokenCalculator
{
    public static void Calculate(in TokenEvaluationContext<ShadcnToken> context, DesignToken global, AbstractTimePickerToken token)
    {
        TimeViewTokenCalculator.Calculate(in context, global, token);
        token.ButtonsMargin = new Thickness(0, global.UniformlyMarginXS, 0, 0);
    }
}
