using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG values of the shared <see cref="AbstractAutoCompleteToken" /> layer.
/// </summary>
public static class AutoCompleteTokenCalculator
{
    /// <summary>
    /// Writes the PrimeNG values into the shared AutoComplete layer: the option height and colors, the padding of
    /// the popup content and the width limits of the popup (120 to 200).
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<PrimeNGToken> context, DesignToken global, AbstractAutoCompleteToken token)
    {
        // Ant Design formula kept as is; the PrimeNG stage (PrimeNGAutoCompleteTokens.EvaluateOwn) replaces
        // the option row, the popup padding and the option colors with the list option values. The width
        // limits 120 and 200 stay: no stage writes them.
        token.OptionHeight        = global.ControlHeight;
        token.OptionMargin        = default;
        token.OptionBorderRadius  = default;
        token.PopupContentPadding = new Thickness(global.UniformlyPaddingXXS / 2);
        token.MinPopupWidth       = 120;
        token.OptionColor         = global.ColorTextSecondary;
        token.OptionSelectedColor = global.ColorText;
        token.MaxPopupWidth       = 200;
    }
}
