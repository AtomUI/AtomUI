using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;

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
        PrimeNGMotion.Restore(context, controlGlobal, mid: PrimeNGMotion.ToggleSlide);
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, ToggleSwitchToken token)
    {
        var rules   = PrimeNGPresets.Rules(context, effectiveGlobal);
        var surface = rules.Surface.AsSpan();
        var rule    = rules.Switch;
        var checkedHandle = rule.HandleChecked.Resolve(surface);

        // PrimeUIX toggleswitch geometry: width x height 2.25rem x 1.375rem (Lara 2.625rem x
        // 1.5rem), handle.size 0.875rem (Lara 1.125rem), gap 0.25rem from the outer edge, and no handle shadow.
        var lara = PrimeNGPresets.KindOf(context) == PrimeNGPresetKind.Lara;
        token.TrackHeight   = lara ? 24 : 22;
        token.TrackMinWidth = lara ? 42 : 36;
        token.HandleSize    = lara ? new Size(18, 18) : new Size(14, 14);
        // The handle sits TrackPadding from the track's top and side, so the track must be the handle plus twice the
        // padding to centre it: the 4px gap for Aura and Nora (22 = 14 + 8); Lara's 24px track around an 18px handle
        // centres with a 3px gap (PrimeUIX keeps 4px at the side and centres vertically, a 1px difference).
        token.TrackPadding  = lara ? 3 : 4;
        token.HandleShadow  = default;
        // PrimeUIX has one size; the small switch keeps a 4px gap around a 10px handle (18 x 28).
        token.TrackHeightSM   = 18;
        token.HandleSizeSM    = new Size(10, 10);
        token.TrackMinWidthSM = 28;
        // On / off text keeps half a handle from the free edge and clears the handle plus the gap on the other side.
        var handle = token.HandleSize.Width;
        token.InnerMinMargin   = handle / 2;
        token.InnerMaxMargin   = handle + token.TrackPadding + 4;
        token.InnerMinMarginSM = token.HandleSizeSM.Width / 2 - 1;
        token.InnerMaxMarginSM = token.HandleSizeSM.Width + 4 + 2;
        // On / off text: the contrast colour on the checked primary track (AtomUI's default), the body text on the
        // unchecked track (light surface 300 / dark surface 700, Nora the field background).
        token.UncheckedContentColor = effectiveGlobal.ColorText;
        token.ContentIconSize   = token.TrackHeightSM;
        token.ContentIconSizeSM = token.TrackHeightSM - 4;
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
