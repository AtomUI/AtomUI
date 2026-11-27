using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn values of the shared <see cref="AbstractStandardButtonToken" /> layer.
/// </summary>
public static class StandardButtonTokenCalculator
{
    /// <summary>
    /// Assigns the shadcn values to <paramref name="token" />, the shared
    /// <see cref="AbstractStandardButtonToken" /> layer. It starts with
    /// <see cref="ButtonGeometryTokenCalculator.Calculate" />, so the geometry and variant layers are filled as
    /// well.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<ShadcnToken> context, DesignToken global, AbstractStandardButtonToken token)
    {
        ButtonGeometryTokenCalculator.Calculate(in context, global, token);
        // Ant Design formula kept as is: ButtonTokenCalculator.CalculateOwn replaces the weight with the registry
        // `button` weight.
        token.FontWeight = FontWeight.Normal;
        token.DefaultBorderColorDisabled = global.ColorBorder;
    }
}
