using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign values of the shared <see cref="AbstractTransferToken" /> layer.
/// </summary>
public static class TransferTokenCalculator
{
    /// <summary>
    /// Assigns the Ant Design values of the shared <see cref="AbstractTransferToken" /> layer to
    /// <paramref name="token" />: the size of a transfer list and the height and padding of its header.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<AntDesignToken> context, DesignToken global, AbstractTransferToken token)
    {

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
