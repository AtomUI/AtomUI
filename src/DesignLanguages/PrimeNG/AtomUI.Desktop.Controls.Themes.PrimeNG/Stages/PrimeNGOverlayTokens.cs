using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The arrow box of flyouts, confirm popups, date pickers and dropdown menus under the PrimeNG presets: PrimeUIX
/// 3.1.1 <c>overlay.popover</c>. The box has a one-line border in <c>overlay.popover.border.color</c> (Aura and Lara
/// surface 200 in light, Nora transparent; surface 700 in dark) that also runs along the arrow's sides, the
/// <c>overlay.popover.padding</c> (Aura and Nora 10, Lara 14) and no minimum height. The arrow spans
/// <c>popover.gutter</c> like PrimeNG's border triangle (see <see cref="PrimeNGPopoverArrow" />). Its radius is the global
/// <c>BorderRadius</c> (<c>overlay.popover.border.radius</c>, md / xs). The ToolTip, which shares the box, keeps no
/// border (PrimeUIX tooltip has none).
/// </summary>
internal static class PrimeNGArrowDecoratedBoxTokens
{
    internal static void EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
    }
    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, ArrowDecoratedBoxToken token)
    {
        var popover  = context.Language.GetPopoverOverlayRecipe(effectiveGlobal, context.Variant);
        var recipes = context.Language.GetVisualRecipes(effectiveGlobal, context.Variant);
        token.ContentBorderColor     = popover.BorderColor;
        // PrimeUIX .p-popover:before draws the arrow edge in popover.border.color too.
        token.ArrowStrokeColor       = token.ContentBorderColor;
        token.ContentBorderThickness = new Thickness(effectiveGlobal.LineWidth);
        token.ContentPadding         = popover.Padding;
        // PrimeUIX popover has no minimum height; the Ant baseline takes ControlHeight here.
        token.ContentMinHeight       = 0;
        token.ArrowSize              = 2 * recipes.PopoverGutter;
        // A plain border triangle: no rounded tip and no flare into the box.
        token.ArrowTipRadius         = 0;
        token.ArrowBaseRadius        = 0;
    }
}

/// <summary>
/// The Popover (FlyoutHost) and other arrow flyouts under the PrimeNG presets: PrimeUIX 3.1.1
/// <c>overlay.popover.shadow</c> (Aura and Nora the two-layer shadow of select panels, Lara a lighter
/// <c>0 1 3</c>) without an outline ring (the arrow box draws the border), and <c>popover.gutter</c>: 10 between the
/// anchor and the box, which the arrow spans.
/// </summary>
internal static class PrimeNGFlyoutHostTokens
{
    internal static void EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
        controlGlobal.MotionDurationMid = PrimeNGMotion.AnchoredOverlay;
    }
    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, FlyoutHostToken token)
    {
        var recipes = context.Language.GetVisualRecipes(effectiveGlobal, context.Variant);
        var popover  = context.Language.GetPopoverOverlayRecipe(effectiveGlobal, context.Variant);
        token.PopupRootShadow   = popover.Shadows;
        token.OverlayHostShadow = popover.Shadows;
        token.MarginToAnchor    = PrimeNGPopoverArrow.MarginToAnchor(recipes.PopoverGutter);
    }
}

/// <summary>
/// Overlays without an arrow under the PrimeNG presets (select, cascader, combo box, auto-complete and mention
/// panels, context and sub menus): PrimeUIX 3.1.1 <c>overlay.select</c>. The <c>border.color</c> becomes a one-line
/// <c>0 0 0 1</c> ring ahead of the shadow, since those panels have no border of their own (Nora light:
/// PrimeUIX's is transparent; AtomUI draws surface 200 for legibility, see the Nora rules); the shadow is the
/// global <c>BoxShadowsSecondary</c> (<c>overlay.select.shadow</c>, which equals
/// <c>overlay.navigation.shadow</c>), the radius the global <c>BorderRadius</c>
/// (<c>overlay.select.border.radius</c>). The gap to the anchor departs from PrimeUIX's <c>anchor.gutter</c>
/// (Aura and Lara 2, Nora 0): every preset takes the global <c>UniformlyMarginXXS</c> plus one line, so that every
/// drop-down in an app opens at one gap. An arrow box hosted in such a popup draws its own border and does not paint
/// the ring.
/// </summary>
internal static class PrimeNGPopupTokens
{
    internal static void EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
        controlGlobal.MotionDurationMid = PrimeNGMotion.AnchoredOverlay;
    }
    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, PopupToken token)
    {
        var panel   = context.Language.GetSelectOverlayRecipe(effectiveGlobal, context.Variant);
        var shadows = panel.ShadowsWithOutline;
        token.PopupRootShadow   = shadows;
        token.OverlayHostShadow = shadows;
        token.PopupCornerRadius = panel.CornerRadius;
        // PrimeUIX anchor.gutter is 2 (Nora 0, which stacks the panel border on the field border). AtomUI keeps the panels
        // the same distance from their field as the DropdownButton's menu (MarginXXS) in every preset, a deliberate
        // deviation so every drop-down in an app opens at one gap. The panel border is the 1px ring drawn outside the
        // frame, so the frame sits one line further away for the visible edges to match the menu's.
        token.MarginToAnchor    = effectiveGlobal.UniformlyMarginXXS + effectiveGlobal.LineWidth;
    }
}

/// <summary>
/// The arrow of PrimeNG popovers and tooltips: PrimeNG draws a sharp triangle <c>2 × gutter</c> wide and
/// <c>gutter</c> long in the gap between the anchor and the box. An AtomUI arrow of size <c>s</c> is <c>s / 2</c> long
/// and overlaps the box by half a pixel, so a size of <c>2 × gutter</c> and a 0.5 gap before its tip put the box
/// exactly <c>gutter</c> away from the anchor.
/// </summary>
internal static class PrimeNGPopoverArrow
{
    /// <summary>
    /// The arrow box overlaps the arrow's base with the box edge by this much (ArrangeArrow).
    /// </summary>
    private const double BaseOverlap = 0.5;

    /// <summary>
    /// The gap between the anchor and the tip of a <c>2 × gutter</c> arrow that leaves <c>gutter</c> to the box.
    /// </summary>
    internal static double MarginToAnchor(double gutter) => Math.Max(0, gutter - (gutter - BaseOverlap));
}
