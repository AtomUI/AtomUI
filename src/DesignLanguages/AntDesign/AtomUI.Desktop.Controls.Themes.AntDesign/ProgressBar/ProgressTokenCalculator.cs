using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign values of the shared <see cref="AbstractProgressToken" /> layer.
/// </summary>
public static class ProgressTokenCalculator
{
    /// <summary>
    /// Assigns the Ant Design values of the shared <see cref="AbstractProgressToken" /> layer to
    /// <paramref name="token" />: the default progress color and the color of the remaining part.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<AntDesignToken> context, DesignToken global, AbstractProgressToken token)
    {
        token.DefaultColor   = global.ColorInfo;
        token.RemainingColor = global.ColorFillSecondary;
    }
}
