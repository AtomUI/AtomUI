using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// NavMenu under a solid highlight (Nora, or noir in any preset). The selected item draws <c>ItemSelectedColor</c> (the primary colour) on
/// <c>ItemSelectedBg</c>, which Ant Design takes from <c>ControlItemBgActive</c>: the solid primary there, so the
/// text would vanish. <c>ItemSelectedColor</c> cannot take the contrast colour instead, because the same token
/// colours the headers of the selected path, which sit on the menu background. The selected background therefore
/// takes the pale primary <c>ColorPrimaryBg</c> for the menu only (Ant Design's own shape); the text stays the
/// primary. It writes only while the
/// selected background really is the solid primary (<see cref="PrimeNGPresets.HasSolidHighlight"/>: Nora, and every
/// preset under a noir primary); under Aura / Lara's pale highlight or a mixed chain whose Map colours come from Ant
/// Design the Ant Design value stays.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGNavMenuTokens : IControlTokenAlgorithm<NavMenuToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, NavMenuToken token)
    {
        if (!PrimeNGPresets.HasSolidHighlight(effectiveGlobal))
        {
            return;
        }
        token.ItemSelectedBg = effectiveGlobal.ColorPrimaryBg;
    }
}
