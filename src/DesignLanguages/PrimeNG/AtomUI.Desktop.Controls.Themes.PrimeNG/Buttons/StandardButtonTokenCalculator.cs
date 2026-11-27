using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG values of the shared <see cref="AbstractStandardButtonToken" /> layer.
/// </summary>
public static class StandardButtonTokenCalculator
{
    /// <summary>
    /// Calls <see cref="ButtonGeometryTokenCalculator.Calculate" />, which also fills the variant layer, and then
    /// writes the PrimeNG values into the shared standard button layer: the normal label weight and the border color
    /// of a disabled default button.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<PrimeNGToken> context, DesignToken global, AbstractStandardButtonToken token)
    {
        ButtonGeometryTokenCalculator.Calculate(in context, global, token);
        token.FontWeight = FontWeight.Normal;
        token.DefaultBorderColorDisabled = global.ColorBorder;
    }
}
