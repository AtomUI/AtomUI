using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn values of the shared <see cref="AbstractDatePickerToken" /> layer.
/// </summary>
public static class DatePickerTokenCalculator
{
    /// <summary>
    /// Assigns the shadcn values to <paramref name="token" />, the shared
    /// <see cref="AbstractDatePickerToken" /> layer of the date picker and the range date picker. It starts with
    /// <see cref="TimeViewTokenCalculator.Calculate" />, which fills the time view layer first.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<ShadcnToken> context, DesignToken global, AbstractDatePickerToken token)
    {
        TimeViewTokenCalculator.Calculate(in context, global, token);
        // Ant Design formula kept as is: no registry value is merged into these control tokens.
        token.HeaderMargin = new Thickness(0, 0, 0, 3);
        token.PanelContentPadding = global.PaddingSM;
        token.ButtonsPanelMargin = new Thickness(0, global.UniformlyMarginXS, 0, 0);
    }
}
