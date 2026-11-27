using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG values of the shared <see cref="AbstractDatePickerToken" /> layer.
/// </summary>
public static class DatePickerTokenCalculator
{
    /// <summary>
    /// Calls <see cref="TimeViewTokenCalculator.Calculate" /> and then writes the PrimeNG values into the shared date
    /// picker layer: the header margin, the panel content padding and the top margin of the button panel.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<PrimeNGToken> context, DesignToken global, AbstractDatePickerToken token)
    {
        TimeViewTokenCalculator.Calculate(in context, global, token);
        token.HeaderMargin = new Thickness(0, 0, 0, 3);
        token.PanelContentPadding = global.PaddingSM;
        token.ButtonsPanelMargin = new Thickness(0, global.UniformlyMarginXS, 0, 0);
    }
}
