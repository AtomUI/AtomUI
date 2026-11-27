using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign values of the shared <see cref="AbstractCircleProgressToken" /> layer.
/// </summary>
public static class CircleProgressTokenCalculator
{
    /// <summary>
    /// Assigns the Ant Design values of the shared <see cref="AbstractCircleProgressToken" /> layer to
    /// <paramref name="token" />. It first runs <c>ProgressTokenCalculator</c>, so the base progress layer of the
    /// same Token is filled as well.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<AntDesignToken> context, DesignToken global, AbstractCircleProgressToken token)
    {
        ProgressTokenCalculator.Calculate(in context, global, token);
        token.CircleTextColor           = global.ColorText;
        token.CircleMinimumIconSize     = global.SizeXS;
        token.CircleMinimumTextFontSize = global.FontSizeSM - 2;
    }
}
