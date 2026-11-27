using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG values of the shared <see cref="AbstractTimePickerToken" /> layer.
/// </summary>
public static class TimePickerTokenCalculator
{
    /// <summary>
    /// Calls <see cref="TimeViewTokenCalculator.Calculate" /> and then writes the PrimeNG value into the shared time
    /// picker layer: the top margin of the button panel.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<PrimeNGToken> context, DesignToken global, AbstractTimePickerToken token)
    {
        TimeViewTokenCalculator.Calculate(in context, global, token);
        token.ButtonsMargin = new Thickness(0, global.UniformlyMarginXS, 0, 0);
    }
}
