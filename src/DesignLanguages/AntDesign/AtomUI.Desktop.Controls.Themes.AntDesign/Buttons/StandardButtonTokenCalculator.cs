using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign values of the shared <see cref="AbstractStandardButtonToken" /> layer.
/// </summary>
public static class StandardButtonTokenCalculator
{
    /// <summary>
    /// Assigns the Ant Design values of the shared <see cref="AbstractStandardButtonToken" /> layer to
    /// <paramref name="token" />. It first runs <c>ButtonGeometryTokenCalculator</c>, so the geometry and variant
    /// layers of the same Token are filled as well.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<AntDesignToken> context, DesignToken global, AbstractStandardButtonToken token)
    {
        ButtonGeometryTokenCalculator.Calculate(in context, global, token);
        token.FontWeight = FontWeight.Normal;
        token.DefaultBorderColorDisabled = global.ColorBorder;
    }
}
