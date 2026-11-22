using System.Collections.Immutable;
using AtomUI.Desktop.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// The PrimeUIX <c>message</c> / <c>toast</c> severity palettes: info is blue and warn yellow (unlike the
/// button, tag and badge severities, sky and orange); success and error follow the theme's status and error colours.
/// Aura and Lara tint the surface (light: the 50 stop at 95% with the 200 border and 600 text; dark: the 500 stop at
/// 16% with the 700 border at 36% and 500 text; Lara drops the border), Nora fills it (light 800, warn 600, with 50
/// text; dark 200 with 950 text). The info icon reads <c>ColorPrimary</c>, so the scope sets it to the info text colour.
/// </summary>
internal static class PrimeNGMessageSeverity
{
    internal static void Apply(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        var rules   = PrimeNGPresets.Rules(context, controlGlobal);
        var success = PrimeNGPalette.FromStop(controlGlobal.ColorSuccess, rules.Status.Base.Stop);
        var error   = PrimeNGPalette.FromStop(controlGlobal.ColorError, rules.Error.Base.Stop);
        Color text;
        (controlGlobal.ColorInfoBg, controlGlobal.ColorInfoBorder, text) =
            Severity(context, PrimeNGTailwindPalettes.Blue.ToArray());
        controlGlobal.ColorInfo    = text;
        controlGlobal.ColorPrimary = text;
        (controlGlobal.ColorSuccessBg, controlGlobal.ColorSuccessBorder, controlGlobal.ColorSuccess) =
            Severity(context, success);
        (controlGlobal.ColorWarningBg, controlGlobal.ColorWarningBorder, controlGlobal.ColorWarning) =
            Severity(context, PrimeNGTailwindPalettes.Yellow.ToArray(), noraLightFill: 600);
        (controlGlobal.ColorErrorBg, controlGlobal.ColorErrorBorder, controlGlobal.ColorError) =
            Severity(context, error);
    }

    /// <summary>
    /// The severity colours of the toast-like cards (Message, Notification), PrimeUIX <c>toast</c>: the surfaces of
    /// <see cref="Apply"/> (<c>Color*Bg</c>, Aura's 200 border), with the severity text kept in <c>Color*Text</c> for the
    /// card's title, icon and close icon. Lara's toast draws its start bar in the 500 stop (dark the 700 at 36%) where its
    /// inline message has no border, so <c>Color*Border</c> takes that stop. Nora's severity text is the light stop of its
    /// filled card, so <c>ColorPrimary</c> and the plain severity colours take the fill instead: they still paint the
    /// card's neutral parts (the remaining-time bar of a notification without a severity) on the white surface.
    /// </summary>
    internal static void ApplyForCard(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        var rules   = PrimeNGPresets.Rules(context, controlGlobal);
        var success = PrimeNGPalette.FromStop(controlGlobal.ColorSuccess, rules.Status.Base.Stop);
        var error   = PrimeNGPalette.FromStop(controlGlobal.ColorError, rules.Error.Base.Stop);
        Apply(context, controlGlobal);
        controlGlobal.ColorInfoText    = controlGlobal.ColorInfo;
        controlGlobal.ColorSuccessText = controlGlobal.ColorSuccess;
        controlGlobal.ColorWarningText = controlGlobal.ColorWarning;
        controlGlobal.ColorErrorText   = controlGlobal.ColorError;

        var kind = PrimeNGPresets.KindOf(context);
        if (kind == PrimeNGPresetKind.Lara)
        {
            controlGlobal.ColorInfoBorder    = StartBar(context, PrimeNGTailwindPalettes.Blue.ToArray());
            controlGlobal.ColorSuccessBorder = StartBar(context, success);
            controlGlobal.ColorWarningBorder = StartBar(context, PrimeNGTailwindPalettes.Yellow.ToArray());
            controlGlobal.ColorErrorBorder   = StartBar(context, error);
        }
        else if (kind == PrimeNGPresetKind.Nora)
        {
            controlGlobal.ColorInfo    = controlGlobal.ColorInfoBg;
            controlGlobal.ColorPrimary = controlGlobal.ColorInfoBg;
            controlGlobal.ColorSuccess = controlGlobal.ColorSuccessBg;
            controlGlobal.ColorWarning = controlGlobal.ColorWarningBg;
            controlGlobal.ColorError   = controlGlobal.ColorErrorBg;
        }
    }

    // Lara toast.{severity}.borderColor: the 500 stop, dark the 700 stop at 36%.
    private static Color StartBar(in ThemeAlgorithmContext context, Color[] palette) =>
        context.Appearance == ThemeAppearance.Dark
            ? PrimeNGPalette.WithOpacity(palette[PrimeNGPalette.IndexOf(700)], 0.36)
            : palette[PrimeNGPalette.IndexOf(500)];

    // Nora fills the surface with the 800 stop in light (warn the 600 stop, PrimeUIX's yellow.600) and the 200 in dark.
    private static (Color Background, Color Border, Color Text) Severity(in ThemeAlgorithmContext context, Color[] palette,
                                                                        int noraLightFill = 800)
    {
        var kind = PrimeNGPresets.KindOf(context);
        var dark = context.Appearance == ThemeAppearance.Dark;
        Color Stop(int stop) => palette[PrimeNGPalette.IndexOf(stop)];
        if (kind == PrimeNGPresetKind.Nora)
        {
            var fill = Stop(dark ? 200 : noraLightFill);
            return (fill, fill, Stop(dark ? 950 : 50));
        }
        var background = dark ? PrimeNGPalette.WithOpacity(Stop(500), 0.16) : PrimeNGPalette.WithOpacity(Stop(50), 0.95);
        var border = kind == PrimeNGPresetKind.Lara
            ? Colors.Transparent
            : dark ? PrimeNGPalette.WithOpacity(Stop(700), 0.36) : Stop(200);
        return (background, border, Stop(dark ? 500 : 600));
    }
}

/// <summary>
/// Alert under the PrimeNG presets, PrimeUIX 3.1.1 <c>message</c>: the severity surfaces of
/// <see cref="PrimeNGMessageSeverity"/>, <c>border.radius</c> border.radius.md, <c>content.padding</c> 6px 10px (Lara
/// 10px 14px), <c>icon.size</c> 1rem (Lara 1.125rem) and <c>close.icon.size</c> 0.875rem.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGAlertTokens : IControlTokenAlgorithm<AlertToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        PrimeNGMessageSeverity.Apply(context, controlGlobal);
        controlGlobal.BorderRadiusLG = controlGlobal.BorderRadius;
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, AlertToken token)
    {
        var lara = PrimeNGPresets.KindOf(context) == PrimeNGPresetKind.Lara;
        token.DefaultPadding  = lara ? new Thickness(14, 10) : new Thickness(10, 6);
        token.DefaultIconSize = lara ? 18 : 16;
        token.CloseIconSize   = 14;
        // text.color is the severity colour, text.font.weight 500 (Nora 700).
        token.SuccessMessageColor = effectiveGlobal.ColorSuccess;
        token.InfoMessageColor    = effectiveGlobal.ColorInfo;
        token.WarningMessageColor = effectiveGlobal.ColorWarning;
        token.ErrorMessageColor   = effectiveGlobal.ColorError;
        token.TextFontWeight      = PrimeNGPresets.KindOf(context) == PrimeNGPresetKind.Nora
            ? FontWeight.Bold
            : FontWeight.Medium;
    }
}

/// <summary>
/// PopupConfirm under the PrimeNG presets, PrimeUIX 3.1.1 <c>confirmpopup</c>: <c>content.gap</c> 0.5rem (Lara
/// 0.875rem) after the 1.25rem icon, the footer one <c>content.padding</c> below the message and
/// <c>footer.gap</c> 6 (Lara 8) between the buttons.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGPopupConfirmTokens : IControlTokenAlgorithm<PopupConfirmToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        PrimeNGMotion.Restore(context, controlGlobal, mid: PrimeNGMotion.AnchoredOverlay);
        controlGlobal.IconSizeLG = 20;
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, PopupConfirmToken token)
    {
        var lara = PrimeNGPresets.KindOf(context) == PrimeNGPresetKind.Lara;
        token.IconMargin            = new Thickness(0, 0, lara ? 14 : 8, 0);
        token.ButtonContainerMargin = new Thickness(0, lara ? 14 : 10, 0, 0);
        token.ButtonSpacing         = lara ? 8 : 6;
    }
}

/// <summary>Tour under the PrimeNG presets (no PrimeUIX counterpart): the popover's border.radius.md corners.</summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGTourTokens : IControlTokenAlgorithm<TourToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, TourToken token)
    {
        token.TourBorderRadius = effectiveGlobal.BorderRadius;
    }
}

/// <summary>
/// Upload under the PrimeNG presets, PrimeUIX 3.1.1 <c>fileupload</c>: the drop area is the content background with
/// a <c>border.color</c> (content.border.color) frame and border.radius.md corners.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGUploadTokens : IControlTokenAlgorithm<UploadToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        controlGlobal.BorderRadiusLG = controlGlobal.BorderRadius;
        controlGlobal.ColorFillAlter = controlGlobal.ColorBgElevated;
        controlGlobal.ColorBorder    = PrimeNGPresets.ContentBorder(context);
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, UploadToken token)
    {
    }
}
