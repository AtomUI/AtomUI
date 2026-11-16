using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// ToggleSwitch under the PrimeNG presets: the checked handle <c>HandleCheckedBg</c> is PrimeUIX 3.1.1
/// <c>toggleswitch.handle.checked.background</c> (Aura / Lara <c>light-dark(surface.0, surface.900)</c>, Nora
/// <c>primary.contrast.color</c>), so it stays visible on a near-white primary track (noir dark). The unchecked handle
/// keeps the Ant Design white: PrimeUIX pairs its <c>handle.background</c> (Aura / Lara dark surface.400, Nora the
/// field border colour) with a track AtomUI does not map (<c>ColorTextQuaternary</c> here).
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGToggleSwitchTokens : IControlTokenAlgorithm<ToggleSwitchToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, ToggleSwitchToken token)
    {
        var rules = PrimeNGPresets.Rules(context, effectiveGlobal);
        token.HandleCheckedBg = rules.SwitchHandleChecked.Resolve(rules.Surface.AsSpan());
    }
}
