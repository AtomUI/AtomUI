using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// MessageCard under the PrimeNG presets: the <c>@primeuix/themes</c> 3.1.1 <c>message</c> severity surfaces (tinted
/// under Aura and Lara, filled under Nora) and <c>toast</c> shadows — the neutral message takes
/// <c>overlay.popover.shadow</c> and, under Aura, each severity a single <c>0px 4px 8px 0px</c> layer in its 500 stop at
/// 4% (<c>color-mix(in srgb, {blue|green|yellow|red}.500, transparent 96%)</c>; the dark sections use the same stops).
/// The AtomUI-only <c>Loading</c> severity has no PrimeNG counterpart and takes the neutral value.
/// </summary>
internal static class PrimeNGMessageCardTokens
{
    internal static PrimeNGStatusColors EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
        PrimeNGMotion.Restore(context, controlGlobal, mid: PrimeNGMotion.Toast);
        var statuses = PrimeNGMessageSeverity.ApplyForCard(context, controlGlobal);
        // PrimeUIX message.border.radius is the md step of border.radius.
        controlGlobal.BorderRadiusLG = controlGlobal.BorderRadius;
        return statuses;
    }
    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, MessageCardToken token)
    {
        var resting = context.Language.GetPopoverOverlayRecipe(effectiveGlobal, context.Variant).Shadows;
        var kind    = context.Variant.Style;
        var aura    = kind == PrimeNGLanguage.Aura;
        var nora    = kind == PrimeNGLanguage.Nora;
        // PrimeUIX message.{severity}: Aura and Lara tint the surface (the 50 stop at 95%, dark the 500 at 16%) with
        // 600 (dark 500) text and icon, Nora fills it (800, warn 600; dark 200) with 50 (dark 950) text; Aura adds a 1px
        // 200 border (dark 700 at 36%), drawn as a ring ahead of its tinted shadow.
        (token.InformationBg, token.InformationIconColor, token.InformationColor) =
            (effectiveGlobal.ColorInfoBg, effectiveGlobal.ColorInfoText, effectiveGlobal.ColorInfoText);
        (token.SuccessBg, token.SuccessIconColor, token.SuccessColor) =
            (effectiveGlobal.ColorSuccessBg, effectiveGlobal.ColorSuccessText, effectiveGlobal.ColorSuccessText);
        (token.WarningBg, token.WarningIconColor, token.WarningColor) =
            (effectiveGlobal.ColorWarningBg, effectiveGlobal.ColorWarningText, effectiveGlobal.ColorWarningText);
        (token.ErrorBg, token.ErrorIconColor, token.ErrorColor) =
            (effectiveGlobal.ColorErrorBg, effectiveGlobal.ColorErrorText, effectiveGlobal.ColorErrorText);
        // A tinted or filled severity card stands out by itself: Lara and Nora keep the popover shadow. The AtomUI-only
        // Loading message is a plain card (toast.normal), outlined in every preset (see NeutralFrame).
        token.InformationShadows = aura ? AuraSeverityFrame(context, context.Language.GetFeedbackScale(PrimeNGFeedbackSeverity.Information)) : resting;
        token.SuccessShadows     = aura ? AuraSeverityFrame(context, context.Language.GetFeedbackScale(PrimeNGFeedbackSeverity.Success)) : resting;
        token.WarningShadows     = aura ? AuraSeverityFrame(context, context.Language.GetFeedbackScale(PrimeNGFeedbackSeverity.Warning)) : resting;
        token.ErrorShadows       = aura ? AuraSeverityFrame(context, context.Language.GetFeedbackScale(PrimeNGFeedbackSeverity.Error)) : resting;
        token.LoadingShadows     = NeutralFrame(context, effectiveGlobal, resting);
        // PrimeUIX message.icon.size 1rem (Lara 1.125rem).
        token.MessageIconSize = kind == PrimeNGLanguage.Lara ? 18 : 16;
    }

    /// <summary>
    /// Aura toast: a 1px border all round (<c>toast.border.width</c>; Lara and Nora draw a 6px start bar instead), in
    /// <c>{color}.200</c> (dark <c>{color}.700</c> at 36%) for a severity, drawn as a ring ahead of the severity shadow.
    /// Measured on primeng.dev: the severity toasts carry a 1px <c>{color}.200</c> border, which is what outlines
    /// them on a white page; their 4% shadow alone leaves the edge invisible.
    /// </summary>
    internal static BoxShadows AuraSeverityFrame(in TokenEvaluationContext<PrimeNGToken> context, PrimeNGSeverityScale palette)
    {
        var dark   = context.Variant.Appearance == AtomUI.Theme.ThemeAppearance.Dark;
        var border = dark ? palette.AtOpacity(700, 0.36) : palette[200];
        return PrimeNGOverlayRecipe.WithOutlineRing(SeverityShadow(palette), border, 1);
    }

    /// <summary>
    /// A plain (white) toast card outlined by <c>content.border.color</c> (Aura and Lara surface 200, Nora surface 400;
    /// dark surface 700 / 500) as a 1px ring ahead of <paramref name="shadows" />: Aura's toast border for a card without
    /// a severity, and AtomUI's legibility addition for Lara and Nora, whose plain toast has no outline and blends into
    /// a white page (their 6px start bar only marks the leading edge).
    /// </summary>
    internal static BoxShadows NeutralFrame(in TokenEvaluationContext<PrimeNGToken> context, DesignToken global, BoxShadows shadows) =>
        PrimeNGOverlayRecipe.WithOutlineRing(shadows, context.Language.GetContentBorder(context.Variant), global.LineWidth);

    /// <summary>
    /// The PrimeUIX severity shadow: <c>0px 4px 8px 0px</c> in the palette's 500 stop at 4% opacity
    /// (96% transparent color-mix; 4% of 255 rounds to alpha 0x0A, as the 10% layers round to 0x1A).
    /// </summary>
    internal static BoxShadows SeverityShadow(PrimeNGSeverityScale palette) => new(new BoxShadow
    {
        OffsetX = 0,
        OffsetY = 4,
        Blur    = 8,
        Spread  = 0,
        Color   = palette.AtOpacity(500, 0.04)
    });
}
