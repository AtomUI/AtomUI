using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using AtomUI.Theme;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG button owners (Button, DropdownButton, SubmitButton, ResetButton), PrimeUIX 3.1.1 <c>button</c>:
/// <list type="bullet">
/// <item>behaviour (the behaviour Tokens of <see cref="AbstractButtonGeometryToken"/>): the pressed colours on press (no movement), disabled buttons at
/// <c>disabled.opacity</c> 0.6 with their own colours, links underlined on hover, no click wave;</item>
/// <item>severities: the primary solid as the preset writes it; <c>danger</c> the error palette at the status stops
/// (light 500 / 600 / 700, Nora 600 / 700 / 800; dark 400 / 300 / 200, Nora 500 / 400 / 300) with the palette's 950
/// text in dark; <c>contrast</c> (AtomUI Default + Solid) surface.950 / 900 / 800 (dark white / surface.100 /
/// 200); <c>button.link</c> the primary colour in every state;</item>
/// <item>AtomUI Default (outlined) is <c>outlined.secondary</c>: a constant surface.200 border (Nora surface.600; dark
/// surface.700, Nora surface.400) and text.muted text on a transparent fill that hovers to surface.50 (dark white at
/// 4%) and presses to surface.100 (dark white at 16%); AtomUI Text is <c>text.secondary</c>;</item>
/// <item>geometry: one radius at every size (<c>border.radius.md</c>), <c>gap</c> 0.5rem between icon and label,
/// <c>sm.font.size</c> 0.75rem, no resting shadow, <c>label.font.weight</c> (Aura 500, Lara 600, Nora 700) and the
/// <c>padding.x</c> of each size (the field padding; Lara 0.875rem) plus the border, because a CSS button adds the
/// border to its padding while the AtomUI label inset includes it.</item>
/// </list>
/// The global link colours stay the deeper, page-readable stops for hyperlinks and other link text.
/// </summary>
/// <summary>The button family's link and danger colours, which the controls read from LinkColor / DangerAccentColor.</summary>
internal readonly record struct PrimeNGButtonColors(Color Link, Color Danger);

internal static class PrimeNGButtonFamilyTokens<TToken>
    where TToken : AbstractButtonGeometryToken
{
    internal static PrimeNGButtonColors EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
        var kind = context.Variant.Style;
        var dark = context.Variant.Appearance == ThemeAppearance.Dark;

        // button.link is the primary colour in every state.
        controlGlobal.ColorLinkHover  = controlGlobal.ColorPrimary;
        controlGlobal.ColorLinkActive = controlGlobal.ColorPrimary;
        var link = controlGlobal.ColorPrimary;

        Color danger;
        (danger, controlGlobal.ColorErrorHover, controlGlobal.ColorErrorActive) = DangerFills(context, controlGlobal);

        controlGlobal.ColorBgSolid       = context.Language.GetSurfaceColor(dark ? 0 : 950);
        controlGlobal.ColorBgSolidHover  = context.Language.GetSurfaceColor(dark ? 100 : 900);
        controlGlobal.ColorBgSolidActive = context.Language.GetSurfaceColor(dark ? 200 : 800);

        controlGlobal.BorderRadiusSM   = controlGlobal.BorderRadius;
        controlGlobal.BorderRadiusLG   = controlGlobal.BorderRadius;
        controlGlobal.OpacityLoading   = 0.6;
        return new PrimeNGButtonColors(link, danger);
    }

    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, TToken token,
        PrimeNGButtonColors colors)
    {
        token.LinkColor         = colors.Link;
        token.DangerAccentColor = colors.Danger;
        var recipes = context.Language.GetVisualRecipes(effectiveGlobal, context.Variant);
        var kind     = context.Variant.Style;
        var dark     = context.Variant.Appearance == ThemeAppearance.Dark;
        token.DisabledByOpacity    = true;
        token.DisabledOpacity      = 0.6;
        token.UnderlineLinkOnHover = true;
        if (token is AbstractStandardButtonToken standard)
        {
            standard.FontWeight = recipes.ButtonFontWeight;
        }

        var border     = effectiveGlobal.LineWidth;
        var horizontal = recipes.ButtonPaddingHorizontal ?? effectiveGlobal.InputPadding.Left;
        token.ContentPadding      = WithHorizontal(token.ContentPadding, horizontal + border);
        token.ContentFontSizeSM   = effectiveGlobal.FontSizeSM;
        token.ContentLineHeightSM = effectiveGlobal.FontSizeSM * 1.5;
        var smVertical = Math.Max(0, (effectiveGlobal.ControlHeightSM - token.ContentLineHeightSM - 2 * border) / 2);
        token.ContentPaddingSM = new Thickness(effectiveGlobal.InputPaddingSM.Left + border, smVertical);
        token.ContentPaddingLG = WithHorizontal(token.ContentPaddingLG, effectiveGlobal.InputPaddingLG.Left + border);
        token.IconMargin       = new Thickness(0, 0, 8, 0);
        token.IconEndMargin    = new Thickness(8, 0, 0, 0);

        token.PrimaryShadow = default;
        token.DefaultShadow = default;
        token.DangerShadow  = default;
        if (dark)
        {
            token.DangerColor    = context.Language.GetStatusScale(context.Variant, colors.Danger, error: true)[950];
            token.SolidTextColor = context.Language.GetSurfaceColor(950);
        }

        var nora    = kind == PrimeNGLanguage.Nora;
        var outline = context.Language.GetSurfaceColor(nora ? (dark ? 400 : 600) : (dark ? 700 : 200));
        var muted   = context.Language.GetSurfaceColor(nora ? (dark ? 400 : 600) : (dark ? 400 : 500));
        token.DefaultBorderColor       = outline;
        token.DefaultHoverBorderColor  = outline;
        token.DefaultActiveBorderColor = outline;
        token.DefaultColor             = muted;
        token.DefaultHoverColor        = muted;
        token.DefaultActiveColor       = muted;
        token.DefaultBg                = Colors.Transparent;
        token.DefaultHoverBg  = dark ? Color.FromArgb(0x0A, 255, 255, 255) : context.Language.GetSurfaceColor(50);
        token.DefaultActiveBg = dark ? Color.FromArgb(0x29, 255, 255, 255) : context.Language.GetSurfaceColor(100);

        var lara      = kind == PrimeNGLanguage.Lara;
        var textColor = context.Language.GetSurfaceColor(dark ? (lara ? 300 : 400) : (kind == PrimeNGLanguage.Aura ? 500 : 600));
        token.TextTextColor       = textColor;
        token.TextTextHoverColor  = textColor;
        token.TextTextActiveColor = textColor;
        token.TextHoverBg = context.Language.GetSurfaceColor(dark ? (lara ? 700 : 800) : (lara ? 100 : 50));
    }

    /// <summary>
    /// The danger button fills (rest, hover, active), PrimeUIX <c>button.danger</c>: the error 500 / 600 / 700 stops
    /// (dark 400 / 300 / 200; Nora 600 / 700 / 800, dark 500 / 400 / 300).
    /// </summary>
    internal static (Color Base, Color Hover, Color Active) DangerFills(in TokenEvaluationContext<PrimeNGToken> context, DesignToken global)
    {
        var kind  = context.Variant.Style;
        var dark  = context.Variant.Appearance == ThemeAppearance.Dark;
        var error = PrimeNGSeverity.Error(context, global);
        int[] stops = kind == PrimeNGLanguage.Nora
            ? dark ? [500, 400, 300] : [600, 700, 800]
            : dark ? [400, 300, 200] : [500, 600, 700];
        return (error[stops[0]], error[stops[1]], error[stops[2]]);
    }

    private static Thickness WithHorizontal(Thickness padding, double horizontal) =>
        new(horizontal, padding.Top, horizontal, padding.Bottom);
}
