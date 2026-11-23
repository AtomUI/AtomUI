using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using AtomUI.Theme;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeUIX <c>message</c> / <c>toast</c> severity palettes: info is blue and warn yellow (unlike the
/// button, tag and badge severities, sky and orange); success and error follow the theme's status and error colours.
/// Aura and Lara tint the surface (light: the 50 stop at 95% with the 200 border and 600 text; dark: the 500 stop at
/// 16% with the 700 border at 36% and 500 text; Lara drops the border), Nora fills it (light 800, warn 600, with 50
/// text; dark 200 with 950 text). The info icon reads <c>ColorPrimary</c>, so the scope sets it to the info text colour.
/// </summary>
/// <summary>The severity colours a message-like control reads in place of the status seeds.</summary>
internal readonly record struct PrimeNGStatusColors(Color Info, Color Primary, Color Success, Color Warning, Color Error);

internal static class PrimeNGMessageSeverity
{
    /// <summary>
    /// Writes the severity surfaces (the Map tokens <c>Color*Bg</c>, <c>Color*Border</c>) and returns the severity
    /// colours the control reads in place of the status seeds; <c>Primary</c> is the info colour.
    /// </summary>
    internal static PrimeNGStatusColors Apply(in DerivationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
        var success = context.Language.GetStatusScale(context.Variant, controlGlobal.ColorSuccess);
        var error   = context.Language.GetStatusScale(context.Variant, controlGlobal.ColorError, error: true);
        Color info, successText, warningText, errorText;
        (controlGlobal.ColorInfoBg, controlGlobal.ColorInfoBorder, info) =
            Severity(context, context.Language.GetFeedbackScale(PrimeNGFeedbackSeverity.Information));
        (controlGlobal.ColorSuccessBg, controlGlobal.ColorSuccessBorder, successText) = Severity(context, success);
        (controlGlobal.ColorWarningBg, controlGlobal.ColorWarningBorder, warningText) =
            Severity(context, context.Language.GetFeedbackScale(PrimeNGFeedbackSeverity.Warning), noraLightFill: 600);
        (controlGlobal.ColorErrorBg, controlGlobal.ColorErrorBorder, errorText) = Severity(context, error);
        return new PrimeNGStatusColors(info, info, successText, warningText, errorText);
    }

    /// <summary>
    /// The severity colours of the toast-like cards (Message, Notification), PrimeUIX <c>toast</c>: the surfaces of
    /// <see cref="Apply"/> (<c>Color*Bg</c>, Aura's 200 border), with the severity text kept in <c>Color*Text</c> for the
    /// card's title, icon and close icon. Lara's toast draws its start bar in the 500 stop (dark the 700 at 36%) where its
    /// inline message has no border, so <c>Color*Border</c> takes that stop. Nora's severity text is the light stop of its
    /// filled card, so <c>ColorPrimary</c> and the plain severity colours take the fill instead: they still paint the
    /// card's neutral parts (the remaining-time bar of a notification without a severity) on the white surface.
    /// </summary>
    internal static PrimeNGStatusColors ApplyForCard(in DerivationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
        var success = context.Language.GetStatusScale(context.Variant, controlGlobal.ColorSuccess);
        var error   = context.Language.GetStatusScale(context.Variant, controlGlobal.ColorError, error: true);
        var statuses = Apply(context, controlGlobal);
        controlGlobal.ColorInfoText    = statuses.Info;
        controlGlobal.ColorSuccessText = statuses.Success;
        controlGlobal.ColorWarningText = statuses.Warning;
        controlGlobal.ColorErrorText   = statuses.Error;

        var kind = context.Variant.Style;
        if (kind == PrimeNGLanguage.Lara)
        {
            controlGlobal.ColorInfoBorder    = StartBar(context, context.Language.GetFeedbackScale(PrimeNGFeedbackSeverity.Information));
            controlGlobal.ColorSuccessBorder = StartBar(context, success);
            controlGlobal.ColorWarningBorder = StartBar(context, context.Language.GetFeedbackScale(PrimeNGFeedbackSeverity.Warning));
            controlGlobal.ColorErrorBorder   = StartBar(context, error);
        }
        else if (kind == PrimeNGLanguage.Nora)
        {
            statuses = new PrimeNGStatusColors(controlGlobal.ColorInfoBg, controlGlobal.ColorInfoBg,
                controlGlobal.ColorSuccessBg, controlGlobal.ColorWarningBg, controlGlobal.ColorErrorBg);
        }

        return statuses;
    }

    // Lara toast.{severity}.borderColor: the 500 stop, dark the 700 stop at 36%.
    private static Color StartBar(in DerivationContext<PrimeNGToken> context, PrimeNGSeverityScale palette) =>
        context.Variant.Appearance == ThemeAppearance.Dark
            ? palette.AtOpacity(700, 0.36)
            : palette[500];

    // Nora fills the surface with the 800 stop in light (warn the 600 stop, PrimeUIX's yellow.600) and the 200 in dark.
    private static (Color Background, Color Border, Color Text) Severity(in DerivationContext<PrimeNGToken> context, PrimeNGSeverityScale palette,
                                                                        int noraLightFill = 800)
    {
        var kind = context.Variant.Style;
        var dark = context.Variant.Appearance == ThemeAppearance.Dark;
        if (kind == PrimeNGLanguage.Nora)
        {
            var fill = palette[dark ? 200 : noraLightFill];
            return (fill, fill, palette[dark ? 950 : 50]);
        }
        var background = dark ? palette.AtOpacity(500, 0.16) : palette.AtOpacity(50, 0.95);
        var border = kind == PrimeNGLanguage.Lara
            ? Colors.Transparent
            : dark ? palette.AtOpacity(700, 0.36) : palette[200];
        return (background, border, palette[dark ? 500 : 600]);
    }
}

/// <summary>
/// Alert under the PrimeNG presets, PrimeUIX 3.1.1 <c>message</c>: the severity surfaces of
/// <see cref="PrimeNGMessageSeverity"/>, <c>border.radius</c> border.radius.md, <c>content.padding</c> 6px 10px (Lara
/// 10px 14px), <c>icon.size</c> 1rem (Lara 1.125rem) and <c>close.icon.size</c> 0.875rem.
/// </summary>
internal static class PrimeNGAlertTokens
{
    internal static PrimeNGStatusColors EvaluateGlobal(in DerivationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
        var statuses = PrimeNGMessageSeverity.Apply(context, controlGlobal);
        controlGlobal.BorderRadiusLG = controlGlobal.BorderRadius;
        return statuses;
    }
    internal static void EvaluateOwn(in DerivationContext<PrimeNGToken> context, DesignToken effectiveGlobal, AlertToken token,
        PrimeNGStatusColors statuses)
    {
        var lara = context.Variant.Style == PrimeNGLanguage.Lara;
        token.DefaultPadding  = lara ? new Thickness(14, 10) : new Thickness(10, 6);
        token.DefaultIconSize = lara ? 18 : 16;
        token.CloseIconSize   = 14;
        // text.color is the severity colour, text.font.weight 500 (Nora 700).
        token.SuccessMessageColor = statuses.Success;
        token.InfoMessageColor    = statuses.Info;
        token.WarningMessageColor = statuses.Warning;
        token.ErrorMessageColor   = statuses.Error;
        // The icons take the severity colour (the info icon is the info colour).
        token.SuccessIconColor = statuses.Success;
        token.InfoIconColor    = statuses.Primary;
        token.WarningIconColor = statuses.Warning;
        token.ErrorIconColor   = statuses.Error;
        token.TextFontWeight      = context.Variant.Style == PrimeNGLanguage.Nora
            ? FontWeight.Bold
            : FontWeight.Medium;
    }
}

/// <summary>
/// PopupConfirm under the PrimeNG presets, PrimeUIX 3.1.1 <c>confirmpopup</c>: <c>content.gap</c> 0.5rem (Lara
/// 0.875rem) after the 1.25rem icon, the footer one <c>content.padding</c> below the message and
/// <c>footer.gap</c> 6 (Lara 8) between the buttons.
/// </summary>
internal static class PrimeNGPopupConfirmTokens
{
    internal static void EvaluateGlobal(in DerivationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
        PrimeNGMotion.Restore(context, controlGlobal, mid: PrimeNGMotion.AnchoredOverlay);
        controlGlobal.IconSizeLG = 20;
    }
    internal static void EvaluateOwn(in DerivationContext<PrimeNGToken> context, DesignToken effectiveGlobal, PopupConfirmToken token)
    {
        var lara = context.Variant.Style == PrimeNGLanguage.Lara;
        token.IconMargin            = new Thickness(0, 0, lara ? 14 : 8, 0);
        token.ButtonContainerMargin = new Thickness(0, lara ? 14 : 10, 0, 0);
        token.ButtonSpacing         = lara ? 8 : 6;
    }
}

/// <summary>Tour under the PrimeNG presets (no PrimeUIX counterpart): the popover's border.radius.md corners.</summary>
internal static class PrimeNGTourTokens
{
    internal static void EvaluateGlobal(in DerivationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
    }
    internal static void EvaluateOwn(in DerivationContext<PrimeNGToken> context, DesignToken effectiveGlobal, TourToken token)
    {
        token.TourBorderRadius = effectiveGlobal.BorderRadius;
    }
}

/// <summary>
/// Upload under the PrimeNG presets, PrimeUIX 3.1.1 <c>fileupload</c>: the drop area is the content background with
/// a <c>border.color</c> (content.border.color) frame and border.radius.md corners.
/// </summary>
internal static class PrimeNGUploadTokens
{
    internal static void EvaluateGlobal(in DerivationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
        controlGlobal.BorderRadiusLG = controlGlobal.BorderRadius;
        controlGlobal.ColorFillAlter = controlGlobal.ColorBgElevated;
        controlGlobal.ColorBorder    = context.Language.GetContentBorder(context.Variant);
    }
    internal static void EvaluateOwn(in DerivationContext<PrimeNGToken> context, DesignToken effectiveGlobal, UploadToken token)
    {
    }
}
