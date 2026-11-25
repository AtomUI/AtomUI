using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn values of the shared <see cref="AbstractDatePickerToken"/> layer.</summary>
public static class DatePickerTokenCalculator
{
    public static void Calculate(in TokenEvaluationContext<ShadcnToken> context, DesignToken global, AbstractDatePickerToken token)
    {
        TimeViewTokenCalculator.Calculate(in context, global, token);
        token.HeaderMargin = new Thickness(0, 0, 0, 3);
        token.PanelContentPadding = global.PaddingSM;
        token.ButtonsPanelMargin = new Thickness(0, global.UniformlyMarginXS, 0, 0);
    }
}
