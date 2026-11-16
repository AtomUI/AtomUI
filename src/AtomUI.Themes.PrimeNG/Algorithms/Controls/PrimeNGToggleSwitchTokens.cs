using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// ToggleSwitch under the PrimeNG presets: PrimeUIX 3.1.1 <c>toggleswitch</c> (the preset's <c>Switch</c> row). Aura and
/// Lara: an unchecked track of surface 300 / 400 (hover), dark 700 / 600, a white handle (dark surface 400 / 300) and a
/// white checked handle (dark surface 900); the checked track is the primary with <c>primary.hover.color</c> under the
/// pointer (AtomUI's default). Nora: a field-like switch, the track the field background (surface 0 / 950) with a
/// one-line field border (surface 500, checked the primary), the handle the field border colour, the checked handle
/// the contrast colour, and a checked track that stays the primary under the pointer. The checked handle stays visible
/// on a near-white primary track (noir dark).
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
        var rules   = PrimeNGPresets.Rules(context, effectiveGlobal);
        var surface = rules.Surface.AsSpan();
        var rule    = rules.Switch;
        var checkedHandle = rule.HandleChecked.Resolve(surface);

        token.TrackBg              = rule.Track.Resolve(surface);
        token.TrackHoverBg         = rule.TrackHover.Resolve(surface);
        token.HandleBg             = rule.Handle.Resolve(surface);
        token.HandleHoverBg        = rule.HandleHover.Resolve(surface);
        token.HandleCheckedBg      = checkedHandle;
        token.HandleCheckedHoverBg = checkedHandle;
        if (rule.CheckedHoverKeepsPrimary)
        {
            token.TrackCheckedHoverBg = effectiveGlobal.ColorPrimary;
        }

        if (rule.TrackBorder is { } border)
        {
            token.TrackBorderColor        = border.Resolve(surface);
            token.TrackCheckedBorderColor = effectiveGlobal.ColorPrimary;
            token.TrackBorderThickness    = effectiveGlobal.LineWidth;
        }
    }
}
