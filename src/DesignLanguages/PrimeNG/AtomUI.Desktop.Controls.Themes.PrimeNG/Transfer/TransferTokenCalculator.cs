using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG values of the shared <see cref="AbstractTransferToken" /> layer.
/// </summary>
public static class TransferTokenCalculator
{
    /// <summary>
    /// Writes the PrimeNG values into the shared transfer layer: the width and height of a list and the height and
    /// padding of its header, which centres one text line vertically.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<PrimeNGToken> context, DesignToken global, AbstractTransferToken token)
    {

        // Ant Design formula kept as is: no PrimeNG stage writes the transfer layout token (the preset's transfer
        // stages cover its list and tree views), so the list and header sizes are the Ant Design values.
        token.ListWidth    = 180;
        token.ListHeight   = 200;
        token.HeaderHeight = global.ControlHeightLG;
        token.HeaderPadding = new Thickness(
            global.UniformlyPaddingSM,
            Math.Ceiling((global.ControlHeightLG -
                          global.LineWidth -
                          global.FontHeight) / 2));
    }
}
