using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// SplitButton under the PrimeNG presets: <c>@primeuix/styles</c> 3.1.1 draws no seam — the primary half has
/// <c>border-inline-end: 0 none</c> (at rest and on hover/active) and the dropdown half supplies the edge through its own
/// border in <c>button.&lt;severity&gt;.borderColor</c>, which for a filled severity equals its fill. AtomUI paints the
/// seam itself in the gap between the two Buttons, so both filled forms take the fill colour there
/// (<c>ColorPrimary</c> / <c>ColorError</c>) and the seam disappears. The outline (default) form keeps no seam and is not
/// touched.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGSplitButtonTokens : IControlTokenAlgorithm<SplitButtonToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, SplitButtonToken token)
    {
        // The filled halves paint exactly these fills (the primary, and the button family's danger stop), so the seam
        // matches both halves, as PrimeUIX draws no divider between them.
        token.SolidSeparatorColor  = effectiveGlobal.ColorPrimary;
        token.DangerSeparatorColor = PrimeNGButtonFamilyTokens<ButtonToken>.DangerFills(context, effectiveGlobal).Base;
    }
}
