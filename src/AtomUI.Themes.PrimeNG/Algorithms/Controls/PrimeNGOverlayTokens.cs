using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// The arrow box of flyouts, confirm popups, date pickers and dropdown menus under the PrimeNG presets: PrimeUIX
/// 3.1.1 <c>overlay.popover</c>. The box has a one-line border in <c>overlay.popover.border.color</c> (Aura and Lara
/// surface 200 in light, Nora transparent; surface 700 in dark) that also runs along the arrow's sides, the
/// <c>overlay.popover.padding</c> (Aura and Nora 10, Lara 14) and no minimum height. The arrow spans
/// <c>popover.gutter</c> like PrimeNG's border triangle (see <see cref="PrimeNGPopoverArrow"/>). Its radius is the global
/// <c>BorderRadius</c> (<c>overlay.popover.border.radius</c>, md / xs). The ToolTip, which shares the box, keeps no
/// border (PrimeUIX tooltip has none).
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGArrowDecoratedBoxTokens : IControlTokenAlgorithm<ArrowDecoratedBoxToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, ArrowDecoratedBoxToken token)
    {
        var preset   = PrimeNGPresets.Resolve(context);
        var rules    = preset.For(context.Appearance);
        var controls = preset.Controls;
        token.ContentBorderColor     = rules.OverlayBorder.Resolve(rules.Surface.AsSpan());
        token.ContentBorderThickness = new Thickness(effectiveGlobal.LineWidth);
        token.ContentPadding         = new Thickness(controls.PopoverPadding);
        token.ContentMinHeight       = 0;
        token.ArrowSize              = PrimeNGPopoverArrow.Size(controls);
    }
}

/// <summary>
/// The Popover (FlyoutHost) and other arrow flyouts under the PrimeNG presets: PrimeUIX 3.1.1
/// <c>overlay.popover.shadow</c> (Aura and Nora the two-layer shadow of select panels, Lara a lighter
/// <c>0 1 3</c>) without an outline ring (the arrow box draws the border), and <c>popover.gutter</c>: 10 between the
/// anchor and the box, which the arrow spans.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGFlyoutHostTokens : IControlTokenAlgorithm<FlyoutHostToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, FlyoutHostToken token)
    {
        var controls = PrimeNGPresets.Resolve(context).Controls;
        token.PopupRootShadow   = controls.PopoverShadows;
        token.OverlayHostShadow = controls.PopoverShadows;
        token.MarginToAnchor    = PrimeNGPopoverArrow.MarginToAnchor(controls);
    }
}

/// <summary>
/// Overlays without an arrow under the PrimeNG presets (select, cascader, combo box, auto-complete and mention panels,
/// context and sub menus): PrimeUIX 3.1.1 <c>overlay.select</c>. The <c>border.color</c> becomes a one-line
/// <c>0 0 0 1</c> ring ahead of the shadow, since those panels have no border of their own (Nora light: transparent,
/// no ring); the shadow is the global <c>BoxShadowsSecondary</c> (<c>overlay.select.shadow</c>, which equals
/// <c>overlay.navigation.shadow</c>), the radius the global <c>BorderRadius</c> (<c>overlay.select.border.radius</c>)
/// and the gap to the anchor <c>anchor.gutter</c> (Aura and Lara 2, Nora 0). An arrow box hosted in such a popup
/// draws its own border and does not paint the ring.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGPopupTokens : IControlTokenAlgorithm<PopupToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, PopupToken token)
    {
        var preset  = PrimeNGPresets.Resolve(context);
        var rules   = preset.For(context.Appearance);
        var border  = rules.OverlayBorder.Resolve(rules.Surface.AsSpan());
        var shadows = WithOutlineRing(effectiveGlobal.BoxShadowsSecondary, border, effectiveGlobal.LineWidth);
        token.PopupRootShadow   = shadows;
        token.OverlayHostShadow = shadows;
        token.PopupCornerRadius = effectiveGlobal.BorderRadius;
        token.MarginToAnchor    = preset.Controls.OverlayAnchorGutter;
    }

    /// <summary>
    /// Puts a <c>0 0 0 <paramref name="width"/></c> ring in <paramref name="border"/> ahead of
    /// <paramref name="shadows"/>; a fully transparent border adds nothing.
    /// </summary>
    internal static BoxShadows WithOutlineRing(BoxShadows shadows, Color border, double width)
    {
        if (border.A == 0 || width <= 0)
        {
            return shadows;
        }

        var ring = new BoxShadow { Spread = width, Color = border };
        if (shadows.Count == 0)
        {
            return new BoxShadows(ring);
        }

        var rest = new BoxShadow[shadows.Count];
        for (var i = 0; i < shadows.Count; i++)
        {
            rest[i] = shadows[i];
        }

        return new BoxShadows(ring, rest);
    }
}

/// <summary>
/// The arrow of PrimeNG popovers: PrimeNG draws a triangle <c>2 × popover.gutter</c> wide and <c>popover.gutter</c>
/// long whose tip ends 1px before the anchor. An AtomUI arrow of size <c>s</c> lays out <c>s / 2 + 0.5</c> long (the
/// ArrowIndicator bounds, its rounded tip 0.75 inside them), so a size of <c>2 × gutter - 1</c> puts the box exactly
/// <c>gutter</c> away from the anchor with the tip 0.75 before it.
/// </summary>
internal static class PrimeNGPopoverArrow
{
    internal static double Size(PrimeNGControlRule controls) => 2 * controls.PopoverGutter - 1;

    /// <summary>
    /// The gap between the anchor and the arrow's layout that leaves <c>popover.gutter</c> between the anchor and the
    /// box.
    /// </summary>
    internal static double MarginToAnchor(PrimeNGControlRule controls) =>
        Math.Max(0, controls.PopoverGutter - (Size(controls) / 2 + 0.5));
}
