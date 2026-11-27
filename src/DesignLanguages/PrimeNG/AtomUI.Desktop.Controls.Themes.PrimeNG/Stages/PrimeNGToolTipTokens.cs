using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using AtomUI.Theme;
using Avalonia.Media;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// ToolTip under the PrimeNG presets: PrimeUIX <c>tooltip.background</c> (neutral 700, the global
/// <c>ColorBgSpotlight</c>), <c>tooltip.color</c>
/// (<c>surface.0</c>, white in both appearances), <c>overlay.popover.border.radius</c> (the global
/// <c>BorderRadius</c>) and <c>tooltip.padding</c> of the standard <c>@primeuix/themes</c> 3.1.1 presets (Aura and Nora
/// 10,6; Lara 10,8).
/// </summary>
internal static class PrimeNGToolTipTokens
{
    /// <summary>
    /// PrimeUIX <c>tooltip.gutter</c> 0.25rem.
    /// </summary>
    private const double PrimeNGTooltipGutter = 4;
    internal static void EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
    }
    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, ToolTipToken token)
    {
        // PrimeUIX tooltip.font.size 0.75rem (Lara 0.625rem) and no minimum height: the box is the padded text.
        var recipes = context.Language.GetVisualRecipes(effectiveGlobal, context.Variant);
        var padding = recipes.TooltipPadding;
        var fontSize = context.Variant.Style == PrimeNGLanguage.Lara ? 10 : 12;
        token.ToolTipFontSize  = fontSize;
        token.ToolTipMinHeight = fontSize * 1.5 + padding.Top + padding.Bottom;
        // The map derivation already wrote the neutral 700 to ColorBgSpotlight; reading it lets a theme's explicit value win.
        token.ToolTipBackground   = effectiveGlobal.ColorBgSpotlight;
        if (effectiveGlobal.EnableMotion && PrimeNGMotion.HasInstantTransitions(context))
        {
            // PrimeNG fades the tooltip in over 250ms in every preset (the token default reads MotionDurationFast).
            token.MotionDuration = PrimeNGMotion.ToolTipFade;
        }
        token.ToolTipColor        = recipes.TooltipForeground;
        token.ToolTipCornerRadius = effectiveGlobal.BorderRadius;
        token.ContentPadding      = padding;
        // PrimeUIX tooltip.max.width 12.5rem (Lara 10.9375rem) and tooltip.shadow (overlay.popover.shadow).
        token.ToolTipMaxWidth = context.Variant.Style == PrimeNGLanguage.Lara ? 175 : 200;
        token.ToolTipShadows  = context.Language.GetPopoverOverlayRecipe(effectiveGlobal, context.Variant).Shadows;
        // PrimeUIX tooltip.gutter 0.25rem: a sharp border triangle 2 * gutter wide and gutter high without an outline
        // (unlike the bordered popover arrows that share the arrow box), and the box gutter away from the anchor, so
        // the arrow spans the gap.
        token.ArrowSize            = 2 * PrimeNGTooltipGutter;
        token.ArrowTipRadius       = 0;
        token.ArrowBaseRadius      = 0;
        token.ArrowStrokeThickness = 0;
        token.MarginToAnchor       = PrimeNGPopoverArrow.MarginToAnchor(PrimeNGTooltipGutter);
        if (context.Variant.Style == PrimeNGLanguage.Nora)
        {
            // PrimeUIX nora tooltip.background surface.900 / white with white / surface.900 text.
            var dark = context.Variant.Appearance == ThemeAppearance.Dark;
            token.ToolTipBackground = dark ? Colors.White : context.Language.Surface900;
            token.ToolTipColor      = dark ? context.Language.Surface900 : Colors.White;
        }
    }
}
