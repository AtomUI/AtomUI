using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// NotificationCard under the PrimeNG presets, <c>@primeuix/themes</c> 3.1.1 <c>toast</c>. Surfaces: Aura and Lara tint
/// a severity card (the 50 stop at 95%, dark the 500 at 16%) with 600 (dark 500) title, icon and close icon and a
/// surface 700 (dark surface 0) detail; Nora fills it (800, warn 600; dark 200) with 50 (dark 950) text. Borders: Aura
/// outlines every card with 1px (a severity's 200 stop, dark the 700 at 36%; the default card
/// <c>content.border.color</c>), drawn as a ring ahead of the shadow; Lara and Nora draw a 6px start bar
/// (<c>border.width: 0 0 0 6px</c>) in the severity's 500 stop (Nora its fill; the default card
/// <c>content.border.color</c>). Shadows: the default notification takes <c>overlay.popover.shadow</c>; Aura gives each
/// severity a single <c>0px 4px 8px 0px</c> layer in its 500 stop at 4%
/// (see <see cref="PrimeNGMessageCardTokens.SeverityShadow"/>), Lara and Nora the popover shadow.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGNotificationCardTokens : IControlTokenAlgorithm<NotificationCardToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        PrimeNGMotion.Restore(context, controlGlobal, mid: PrimeNGMotion.Toast);
        PrimeNGMessageSeverity.ApplyForCard(context, controlGlobal);
        // PrimeUIX toast: border.radius.md, summary.font.size 0.875rem, close.icon.size 0.875rem; the close button
        // is a circle (close.button.border.radius 50%).
        controlGlobal.BorderRadiusLG = controlGlobal.BorderRadius;
        controlGlobal.FontSizeLG     = controlGlobal.FontSize;
        controlGlobal.IconSizeSM     = 14;
        controlGlobal.BorderRadiusSM = new CornerRadius(PrimeNGPresets.KindOf(context) == PrimeNGPresetKind.Lara ? 14 : 12);
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, NotificationCardToken token)
    {
        var neutral = PrimeNGPresets.Resolve(context).Controls.PopoverShadows;
        var kind    = PrimeNGPresets.KindOf(context);
        var aura    = kind == PrimeNGPresetKind.Aura;
        var nora    = kind == PrimeNGPresetKind.Nora;
        // The default card is white: every preset outlines it with content.border.color (Aura's toast border; Lara and
        // Nora an AtomUI legibility addition, see NeutralFrame).
        var resting = PrimeNGMessageCardTokens.NeutralFrame(context, effectiveGlobal, neutral);
        var rules   = PrimeNGPresets.Resolve(context).For(context.Appearance);

        // toast.{severity}.detailColor: Aura and Lara surface 700 (dark surface 0), Nora the severity text.
        var dark   = context.Appearance == AtomUI.Theme.ThemeAppearance.Dark;
        var detail = dark ? Colors.White : rules.Surface[PrimeNGPalette.IndexOf(700)];
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
            token.InformationShadows = PrimeNGMessageCardTokens.AuraSeverityFrame(context, PrimeNGTailwindPalettes.Blue);
            token.SuccessShadows     = PrimeNGMessageCardTokens.AuraSeverityFrame(context, PrimeNGTailwindPalettes.Green);
            token.WarningShadows     = PrimeNGMessageCardTokens.AuraSeverityFrame(context, PrimeNGTailwindPalettes.Yellow);
            token.ErrorShadows       = PrimeNGMessageCardTokens.AuraSeverityFrame(context, PrimeNGTailwindPalettes.Red);
        }
        else
        {
            token.RestingShadows     = WithStartBar(resting, PrimeNGPresets.ContentBorder(context));
            token.InformationShadows = nora ? neutral : WithStartBar(neutral, effectiveGlobal.ColorInfoBorder);
            token.SuccessShadows     = nora ? neutral : WithStartBar(neutral, effectiveGlobal.ColorSuccessBorder);
            token.WarningShadows     = nora ? neutral : WithStartBar(neutral, effectiveGlobal.ColorWarningBorder);
            token.ErrorShadows       = nora ? neutral : WithStartBar(neutral, effectiveGlobal.ColorErrorBorder);
        }

        // PrimeUIX toast: content.padding 0.625rem (Lara 0.875rem), content.gap 0.5rem after a 1rem (Lara 1.125rem)
        // icon, text.gap 0.25rem (Lara and Nora 0.5rem), a 1.5rem (Lara 1.75rem) close button 0.25rem from the corner.
        var lara = kind == PrimeNGPresetKind.Lara;
        var pad = lara ? 14 : 10;
        token.NotificationPadding          = new Thickness(pad + (aura ? 0 : StartBarWidth), pad, pad, pad);
        token.NotificationIconSize         = lara ? 18 : 16;
        token.NotificationIconMargin       = new Thickness(0, 1, 8, 0);
        token.NotificationSectionSpacing   = kind == PrimeNGPresetKind.Aura ? 4 : 8;
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
