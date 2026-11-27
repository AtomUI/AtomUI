using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn values of the shared <see cref="AbstractTimePickerToken" /> layer.
/// </summary>
public static class TimePickerTokenCalculator
{
    /// <summary>
    /// Assigns the shadcn values to <paramref name="token" />, the shared
    /// <see cref="AbstractTimePickerToken" /> layer of the time picker and the range time picker. It starts with
    /// <see cref="TimeViewTokenCalculator.Calculate" />, which fills the time view layer first.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<ShadcnToken> context, DesignToken global, AbstractTimePickerToken token)
    {
        TimeViewTokenCalculator.Calculate(in context, global, token);
        token.ButtonsMargin = new Thickness(0, global.UniformlyMarginXS, 0, 0);
    }
}
