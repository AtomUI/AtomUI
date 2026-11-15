using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// Dialog under the PrimeNG presets: the PrimeUIX <c>overlay.modal.border.radius</c> (<c>border.radius.xl</c>), the
/// <c>overlay.modal.padding</c> (Aura and Nora 18, Lara 20) and the <c>dialog.title.font.size</c> (18) of the standard
/// <c>@primeuix/themes</c> 3.1.1 presets. The title weight 600 is the global <c>FontWeightStrong</c> already.
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
        token.HeaderFontSize       = PrimeNGPresets.Resolve(context).Controls.DialogHeaderFontSize;
    }
}
