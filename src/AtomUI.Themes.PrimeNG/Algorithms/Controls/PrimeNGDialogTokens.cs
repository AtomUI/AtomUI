using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// Dialog under the PrimeNG presets: the PrimeUIX <c>overlay.modal.border.radius</c> (<c>border.radius.xl</c>), the
/// padding of the baseline XML (20, the <c>aura-compat</c> and <c>nora-compat</c> <c>overlay.modal.padding</c>) and the title size of the
/// baseline XML (20, the <c>FontSizeXL</c> step). The title weight 600 is the global <c>FontWeightStrong</c>
/// already.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGDialogTokens : IControlTokenAlgorithm<DialogToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        controlGlobal.BorderRadiusLG = PrimeNGOverlayRadius.Of(controlGlobal, PrimeNGPresets.Resolve(context).Sizes);
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, DialogToken token)
    {
        var padding = new Thickness(PrimeNGPresets.Resolve(context).Controls.DialogPadding);
        token.HeaderPadding        = padding;
        token.ContentPadding       = padding;
        token.WindowContentPadding = padding;
        token.FooterPadding        = padding;
        token.HeaderFontSize       = effectiveGlobal.FontSizeXL;
    }
}
