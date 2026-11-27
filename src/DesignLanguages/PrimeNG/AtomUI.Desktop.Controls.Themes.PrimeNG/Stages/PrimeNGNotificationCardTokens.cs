using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// NotificationCard under the PrimeNG presets, <c>@primeuix/themes</c> 3.1.1 <c>toast</c>. Surfaces: Aura and Lara tint
/// a severity card (the 50 stop at 95%, dark the 500 at 16%) with 600 (dark 500) title, icon and close icon and a
/// surface 700 (dark surface 0) detail; Nora fills it (800, warn 600; dark 200) with 50 (dark 950) text. Borders: Aura
/// outlines every card with 1px (a severity's 200 stop, dark the 700 at 36%; the default card
/// <c>content.border.color</c>), drawn as a ring ahead of the shadow; Lara and Nora draw a 6px start bar
/// (<c>border.width: 0 0 0 6px</c>) in the severity's 500 stop (Nora its fill; the default card
/// <c>content.border.color</c>). Shadows: the default notification takes <c>overlay.popover.shadow</c>; Aura gives each
/// severity a single <c>0px 4px 8px 0px</c> layer in its 500 stop at 4%
/// (see <see cref="PrimeNGMessageCardTokens.SeverityShadow" />), Lara and Nora the popover shadow.
/// </summary>
internal static class PrimeNGNotificationCardTokens
{
    internal static PrimeNGStatusColors EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
        controlGlobal.MotionDurationMid = PrimeNGMotion.Toast;
        var statuses = PrimeNGMessageSeverity.ApplyForCard(context, controlGlobal);
        // PrimeUIX toast: the md step of border.radius, summary.font.size 0.875rem, close.icon.size 0.875rem; the close
        // button is a circle (close.button.border.radius 50%).
        controlGlobal.BorderRadiusLG = controlGlobal.BorderRadius;
        controlGlobal.FontSizeLG     = controlGlobal.FontSize;
        controlGlobal.IconSizeSM     = 14;
        controlGlobal.BorderRadiusSM = new CornerRadius(context.Variant.Style == PrimeNGLanguage.Lara ? 14 : 12);
        return statuses;
    }
    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, NotificationCardToken token)
    {
        var neutral = context.Language.GetPopoverOverlayRecipe(effectiveGlobal, context.Variant).Shadows;
        var kind    = context.Variant.Style;
        var aura    = kind == PrimeNGLanguage.Aura;
        var nora    = kind == PrimeNGLanguage.Nora;
        // The default card is white: every preset outlines it with content.border.color (Aura's toast border; Lara and
        // Nora an AtomUI legibility addition, see NeutralFrame).
        var resting = PrimeNGMessageCardTokens.NeutralFrame(context, effectiveGlobal, neutral);

        // toast.{severity}.detailColor: Aura and Lara surface 700 (dark surface 0), Nora the severity text.
        var dark   = context.Variant.Appearance == AtomUI.Theme.ThemeAppearance.Dark;
        var detail = dark ? Colors.White : context.Language.Surface700;
        (token.InformationBg, token.InformationIconColor, token.InformationTitleColor,
            token.InformationDescriptionColor, token.InformationCloseIconColor, token.InformationCloseIconHoverColor) =
            Severity(effectiveGlobal.ColorInfoBg, effectiveGlobal.ColorInfoText);
        (token.SuccessBg, token.SuccessIconColor, token.SuccessTitleColor,
            token.SuccessDescriptionColor, token.SuccessCloseIconColor, token.SuccessCloseIconHoverColor) =
            Severity(effectiveGlobal.ColorSuccessBg, effectiveGlobal.ColorSuccessText);
        (token.WarningBg, token.WarningIconColor, token.WarningTitleColor,
            token.WarningDescriptionColor, token.WarningCloseIconColor, token.WarningCloseIconHoverColor) =
            Severity(effectiveGlobal.ColorWarningBg, effectiveGlobal.ColorWarningText);
        (token.ErrorBg, token.ErrorIconColor, token.ErrorTitleColor,
            token.ErrorDescriptionColor, token.ErrorCloseIconColor, token.ErrorCloseIconHoverColor) =
            Severity(effectiveGlobal.ColorErrorBg, effectiveGlobal.ColorErrorText);

        (Color, Color, Color, Color, Color, Color) Severity(Color background, Color text) =>
            (background, text, text, nora ? text : detail, text, text);

        // Shadows. Aura: a 1px ring (content.border.color; a severity its 200 stop, see AuraSeverityFrame) ahead of the
        // toast shadow. Lara and Nora: the 6px start bar (toast border.width 0 0 0 6px) in content.border.color, a
        // severity's 500 stop (Lara) or its fill (Nora, invisible on the filled card). The bar is a 6px inset shadow
        // along the leading edge rather than a 6,0,0,0 frame border, because Avalonia's Border draws no box shadow
        // with a non-uniform border thickness (the card would lose its outline and shadow); the content padding takes
        // the 6px instead.
        if (aura)
        {
            token.RestingShadows     = resting;
            token.InformationShadows = PrimeNGMessageCardTokens.AuraSeverityFrame(context, context.Language.GetFeedbackScale(PrimeNGFeedbackSeverity.Information));
            token.SuccessShadows     = PrimeNGMessageCardTokens.AuraSeverityFrame(context, context.Language.GetFeedbackScale(PrimeNGFeedbackSeverity.Success));
            token.WarningShadows     = PrimeNGMessageCardTokens.AuraSeverityFrame(context, context.Language.GetFeedbackScale(PrimeNGFeedbackSeverity.Warning));
            token.ErrorShadows       = PrimeNGMessageCardTokens.AuraSeverityFrame(context, context.Language.GetFeedbackScale(PrimeNGFeedbackSeverity.Error));
        }
        else
        {
            token.RestingShadows     = WithStartBar(resting, context.Language.GetContentBorder(context.Variant));
            token.InformationShadows = nora ? neutral : WithStartBar(neutral, effectiveGlobal.ColorInfoBorder);
            token.SuccessShadows     = nora ? neutral : WithStartBar(neutral, effectiveGlobal.ColorSuccessBorder);
            token.WarningShadows     = nora ? neutral : WithStartBar(neutral, effectiveGlobal.ColorWarningBorder);
            token.ErrorShadows       = nora ? neutral : WithStartBar(neutral, effectiveGlobal.ColorErrorBorder);
        }

        // PrimeUIX toast: content.padding 0.625rem (Lara 0.875rem), content.gap 0.5rem after a 1rem (Lara 1.125rem)
        // icon, text.gap 0.25rem (Lara and Nora 0.5rem), a 1.5rem (Lara 1.75rem) close button 0.25rem from the corner.
        var lara = kind == PrimeNGLanguage.Lara;
        var pad = lara ? 14 : 10;
        token.NotificationPadding          = new Thickness(pad + (aura ? 0 : StartBarWidth), pad, pad, pad);
        token.NotificationIconSize         = lara ? 18 : 16;
        token.NotificationIconMargin       = new Thickness(0, 1, 8, 0);
        token.NotificationSectionSpacing   = kind == PrimeNGLanguage.Aura ? 4 : 8;
        token.NotificationCloseButtonSize  = lara ? 28 : 24;
        token.NotificationCloseButtonMargin = new Thickness(0, 4, 4, 0);
    }

    private const double StartBarWidth = 6;

    private static BoxShadows WithStartBar(BoxShadows shadows, Color color)
    {
        var bar = new BoxShadow { IsInset = true, OffsetX = StartBarWidth, Color = color };
        var all = new BoxShadow[shadows.Count];
        for (var i = 0; i < shadows.Count; i++)
        {
            all[i] = shadows[i];
        }
        return all.Length == 0 ? new BoxShadows(bar) : new BoxShadows(bar, all);
    }
}
