using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn values of the shared <see cref="AbstractTransferToken" /> layer.
/// </summary>
public static class TransferTokenCalculator
{
    /// <summary>
    /// Assigns the shadcn values to <paramref name="token" />, the shared
    /// <see cref="AbstractTransferToken" /> layer of the list transfer and the tree transfer.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<ShadcnToken> context, DesignToken global, AbstractTransferToken token)
    {

        // Ant Design formula kept as is: shadcn has no Transfer, so the language has no values of its own.
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
