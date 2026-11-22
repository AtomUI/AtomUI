using System.Collections.Immutable;
using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// MessageCard under the PrimeNG presets: the <c>@primeuix/themes</c> 3.1.1 <c>message</c> severity surfaces (tinted
/// under Aura and Lara, filled under Nora) and <c>toast</c> shadows — the neutral message takes
/// <c>overlay.popover.shadow</c> and, under Aura, each severity a single <c>0px 4px 8px 0px</c> layer in its 500 stop at
/// 4% (<c>color-mix(in srgb, {blue|green|yellow|red}.500, transparent 96%)</c>; the dark sections use the same stops).
/// The AtomUI-only <c>Loading</c> severity has no PrimeNG counterpart and takes the neutral value.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGMessageCardTokens : IControlTokenAlgorithm<MessageCardToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        PrimeNGMotion.Restore(context, controlGlobal, mid: PrimeNGMotion.Toast);
        PrimeNGMessageSeverity.ApplyForCard(context, controlGlobal);
        // PrimeUIX message.border.radius border.radius.md.
        controlGlobal.BorderRadiusLG = controlGlobal.BorderRadius;
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, MessageCardToken token)
    {
        var resting = PrimeNGPresets.Resolve(context).Controls.PopoverShadows;
        var kind    = PrimeNGPresets.KindOf(context);
        var aura    = kind == PrimeNGPresetKind.Aura;
        var nora    = kind == PrimeNGPresetKind.Nora;
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
        token.InformationShadows = aura ? AuraSeverityFrame(context, PrimeNGTailwindPalettes.Blue) : resting;
        token.SuccessShadows     = aura ? AuraSeverityFrame(context, PrimeNGTailwindPalettes.Green) : resting;
        token.WarningShadows     = aura ? AuraSeverityFrame(context, PrimeNGTailwindPalettes.Yellow) : resting;
        token.ErrorShadows       = aura ? AuraSeverityFrame(context, PrimeNGTailwindPalettes.Red) : resting;
        token.LoadingShadows     = NeutralFrame(context, effectiveGlobal, resting);
        // PrimeUIX message.icon.size 1rem (Lara 1.125rem).
        token.MessageIconSize = kind == PrimeNGPresetKind.Lara ? 18 : 16;
    }

    /// <summary>
    /// Aura toast: a 1px border all round (<c>toast.border.width</c>; Lara and Nora draw a 6px start bar instead), in
    /// <c>{color}.200</c> (dark <c>{color}.700</c> at 36%) for a severity, drawn as a ring ahead of the severity shadow.
    /// Measured on primeng.dev: the severity toasts carry a 1px <c>{color}.200</c> border, which is what outlines
    /// them on a white page; their 4% shadow alone leaves the edge invisible.
    /// </summary>
    internal static BoxShadows AuraSeverityFrame(in ThemeAlgorithmContext context, ImmutableArray<Color> palette)
    {
        var dark   = context.Appearance == AtomUI.Theme.ThemeAppearance.Dark;
        var border = dark ? Color.FromArgb(0x5C, palette[7].R, palette[7].G, palette[7].B) : palette[2];
        return PrimeNGPopupTokens.WithOutlineRing(SeverityShadow(palette), border, 1);
    }

    /// <summary>
    /// A plain (white) toast card outlined by <c>content.border.color</c> (Aura and Lara surface 200, Nora surface 400;
    /// dark surface 700 / 500) as a 1px ring ahead of <paramref name="shadows"/>: Aura's toast border for a card without
    /// a severity, and AtomUI's legibility addition for Lara and Nora, whose plain toast has no outline and blends into
    /// a white page (their 6px start bar only marks the leading edge).
    /// </summary>
    internal static BoxShadows NeutralFrame(in ThemeAlgorithmContext context, DesignToken global, BoxShadows shadows) =>
        PrimeNGPopupTokens.WithOutlineRing(shadows, PrimeNGPresets.ContentBorder(context), global.LineWidth);

    /// <summary>
    /// The PrimeUIX severity shadow: <c>0px 4px 8px 0px</c> in the palette's 500 stop at 4% opacity
    /// (96% transparent colour-mix; 4% of 255 rounds to alpha 0x0A, as the 10% layers round to 0x1A).
    /// </summary>
    internal static BoxShadows SeverityShadow(ImmutableArray<Color> palette) => new(new BoxShadow
    {
        OffsetX = 0,
        OffsetY = 4,
        Blur    = 8,
        Spread  = 0,
        Color   = Color.FromArgb(0x0A, palette[5].R, palette[5].G, palette[5].B)
    });
}
