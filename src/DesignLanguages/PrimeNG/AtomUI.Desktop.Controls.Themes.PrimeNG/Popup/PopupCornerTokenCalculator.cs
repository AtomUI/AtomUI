using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG values of the shared <see cref="AbstractPopupCornerToken" /> layer.
/// </summary>
public static class PopupCornerTokenCalculator
{
    /// <summary>
    /// Writes the PrimeNG value into the shared popup corner layer: the popup corners take the large border radius.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<PrimeNGToken> context, DesignToken global, AbstractPopupCornerToken token)
    {
        token.PopupCornerRadius = global.BorderRadiusLG;
    }
}
