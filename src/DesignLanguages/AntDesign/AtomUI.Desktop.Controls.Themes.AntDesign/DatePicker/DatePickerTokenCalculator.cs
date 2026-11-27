using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign values of the shared <see cref="AbstractDatePickerToken" /> layer.
/// </summary>
public static class DatePickerTokenCalculator
{
    /// <summary>
    /// Assigns the Ant Design values of the shared <see cref="AbstractDatePickerToken" /> layer to
    /// <paramref name="token" />. It first runs <c>TimeViewTokenCalculator</c>, so the time view layer of the same
    /// Token is filled as well.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<AntDesignToken> context, DesignToken global, AbstractDatePickerToken token)
    {
        TimeViewTokenCalculator.Calculate(in context, global, token);
        token.HeaderMargin = new Thickness(0, 0, 0, 3);
        token.PanelContentPadding = global.PaddingSM;
        token.ButtonsPanelMargin = new Thickness(0, global.UniformlyMarginXS, 0, 0);
    }
}
