using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign values of the shared <see cref="AbstractTimePickerToken" /> layer.
/// </summary>
public static class TimePickerTokenCalculator
{
    /// <summary>
    /// Assigns the Ant Design values of the shared <see cref="AbstractTimePickerToken" /> layer to
    /// <paramref name="token" />. It first runs <c>TimeViewTokenCalculator</c>, so the time view layer of the same
    /// Token is filled as well.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<AntDesignToken> context, DesignToken global, AbstractTimePickerToken token)
    {
        TimeViewTokenCalculator.Calculate(in context, global, token);
        token.ButtonsMargin = new Thickness(0, global.UniformlyMarginXS, 0, 0);
    }
}
