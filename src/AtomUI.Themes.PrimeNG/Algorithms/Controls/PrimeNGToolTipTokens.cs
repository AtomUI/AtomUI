using AtomUI.Desktop.Controls;
using AtomUI.Theme;
using Avalonia.Media;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// ToolTip under the PrimeNG presets: PrimeUIX <c>tooltip.background</c> (neutral 700, the global
/// <c>ColorBgSpotlight</c>), <c>tooltip.color</c>
/// (<c>surface.0</c>, white in both appearances), <c>overlay.popover.border.radius</c> (the global
/// <c>BorderRadius</c>) and <c>tooltip.padding</c> of the standard <c>@primeuix/themes</c> 3.1.1 presets (Aura and Nora
/// 10,6; Lara 10,8).
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGToolTipTokens : IControlTokenAlgorithm<ToolTipToken>
{
    /// <summary>PrimeUIX <c>tooltip.gutter</c> 0.25rem.</summary>
    private const double PrimeNGTooltipGutter = 4;

    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        // PrimeUIX tooltip.font.size 0.75rem (Lara 0.625rem) and no minimum height: the box is the padded text.
        var controls = PrimeNGPresets.Resolve(context).Controls;
        var fontSize = PrimeNGPresets.KindOf(context) == PrimeNGPresetKind.Lara ? 10 : 12;
        controlGlobal.FontSize      = fontSize;
        controlGlobal.ControlHeight = fontSize * 1.5 + controls.ToolTipPadding.Top + controls.ToolTipPadding.Bottom;
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, ToolTipToken token)
    {
        var preset  = PrimeNGPresets.Resolve(context);
        var rules   = preset.For(context.Appearance);
        var surface = rules.Surface.AsSpan();
        // The Map stage already wrote the neutral 700 to ColorBgSpotlight; reading it lets a theme's explicit value win.
        token.ToolTipBackground   = effectiveGlobal.ColorBgSpotlight;
        if (effectiveGlobal.EnableMotion && PrimeNGMotion.HasInstantTransitions(context))
        {
            // PrimeNG fades the tooltip in over 250ms in every preset (the token default reads MotionDurationFast).
            token.MotionDuration = PrimeNGMotion.ToolTipFade;
        }
        token.ToolTipColor        = preset.Controls.ToolTipColor.Resolve(surface);
        token.ToolTipCornerRadius = effectiveGlobal.BorderRadius;
        token.ContentPadding      = preset.Controls.ToolTipPadding;
        // PrimeUIX tooltip.max.width 12.5rem (Lara 10.9375rem) and tooltip.shadow (overlay.popover.shadow).
        token.ToolTipMaxWidth = PrimeNGPresets.KindOf(context) == PrimeNGPresetKind.Lara ? 175 : 200;
        token.ToolTipShadows  = preset.Controls.PopoverShadows;
        // PrimeUIX tooltip.gutter 0.25rem: a sharp border triangle 2 * gutter wide and gutter high without an outline
        // (unlike the bordered popover arrows that share the arrow box), and the box gutter away from the anchor, so
        // the arrow spans the gap.
        token.ArrowSize            = 2 * PrimeNGTooltipGutter;
        token.ArrowTipRadius       = 0;
        token.ArrowBaseRadius      = 0;
        token.ArrowStrokeThickness = 0;
        token.MarginToAnchor       = PrimeNGPopoverArrow.MarginToAnchor(PrimeNGTooltipGutter);
        if (PrimeNGPresets.KindOf(context) == PrimeNGPresetKind.Nora)
        {
            // PrimeUIX nora tooltip.background surface.900 / white with white / surface.900 text.
            var dark = context.Appearance == ThemeAppearance.Dark;
            token.ToolTipBackground = dark ? Colors.White : new PrimeNGShade(900).Resolve(surface);
            token.ToolTipColor      = dark ? new PrimeNGShade(900).Resolve(surface) : Colors.White;
        }
    }
}
