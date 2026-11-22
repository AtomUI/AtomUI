using AtomUI.Desktop.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// The PrimeNG button owners (Button, DropdownButton, SubmitButton, ResetButton), PrimeUIX 3.1.1 <c>button</c>:
/// <list type="bullet">
/// <item>behaviour (<see cref="ButtonThemeRecipe"/>): the pressed colours on press (no movement), disabled buttons at
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
internal abstract class PrimeNGButtonFamilyTokens<TToken> : IControlTokenAlgorithm<TToken>
    where TToken : AbstractButtonGeometryToken
{
    private static readonly ButtonThemeRecipe s_recipe = new(
        OutlineOnlyDefaultShadow: false,
        UsesRecipeVariants: false,
        DisabledOpacity: 0.6,
        PressMoves: false,
        UnderlineLinkOnHover: true);

    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        var kind = PrimeNGPresets.KindOf(context);
        var dark = context.Appearance == ThemeAppearance.Dark;

        controlGlobal.ColorLink       = controlGlobal.ColorPrimary;
        controlGlobal.ColorLinkHover  = controlGlobal.ColorPrimary;
        controlGlobal.ColorLinkActive = controlGlobal.ColorPrimary;

        (controlGlobal.ColorError, controlGlobal.ColorErrorHover, controlGlobal.ColorErrorActive) =
            DangerFills(context, controlGlobal);

        controlGlobal.ColorBgSolid       = PrimeNGPresets.Surface(context, dark ? 0 : 950);
        controlGlobal.ColorBgSolidHover  = PrimeNGPresets.Surface(context, dark ? 100 : 900);
        controlGlobal.ColorBgSolidActive = PrimeNGPresets.Surface(context, dark ? 200 : 800);

        controlGlobal.BorderRadiusSM   = controlGlobal.BorderRadius;
        controlGlobal.BorderRadiusLG   = controlGlobal.BorderRadius;
        controlGlobal.EnableWaveSpirit = false;
        controlGlobal.OpacityLoading   = 0.6;
    }

    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, TToken token)
    {
        var controls = PrimeNGPresets.Resolve(context).Controls;
        var kind     = PrimeNGPresets.KindOf(context);
        var dark     = context.Appearance == ThemeAppearance.Dark;
        token.AppearanceSettings = s_recipe;
        if (token is AbstractStandardButtonToken standard)
        {
            standard.FontWeight = controls.ButtonFontWeight;
        }

        var border     = effectiveGlobal.LineWidth;
        var horizontal = controls.ButtonPaddingHorizontal?.Invoke(effectiveGlobal) ?? effectiveGlobal.InputPadding.Left;
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
            token.DangerColor    = PrimeNGSeverity.Stop(PrimeNGSeverity.Error(context, effectiveGlobal), 950);
            token.SolidTextColor = PrimeNGPresets.Surface(context, 950);
        }

        var nora    = kind == PrimeNGPresetKind.Nora;
        var outline = PrimeNGPresets.Surface(context, nora ? (dark ? 400 : 600) : (dark ? 700 : 200));
        var muted   = PrimeNGPresets.Surface(context, nora ? (dark ? 400 : 600) : (dark ? 400 : 500));
        token.DefaultBorderColor       = outline;
        token.DefaultHoverBorderColor  = outline;
        token.DefaultActiveBorderColor = outline;
        token.DefaultColor             = muted;
        token.DefaultHoverColor        = muted;
        token.DefaultActiveColor       = muted;
        token.DefaultBg                = Colors.Transparent;
        token.DefaultHoverBg  = dark ? PrimeNGPalette.WithOpacity(Colors.White, 0.04) : PrimeNGPresets.Surface(context, 50);
        token.DefaultActiveBg = dark ? PrimeNGPalette.WithOpacity(Colors.White, 0.16) : PrimeNGPresets.Surface(context, 100);

        var lara      = kind == PrimeNGPresetKind.Lara;
        var textColor = PrimeNGPresets.Surface(context, dark ? (lara ? 300 : 400) : (kind == PrimeNGPresetKind.Aura ? 500 : 600));
        token.TextTextColor       = textColor;
        token.TextTextHoverColor  = textColor;
        token.TextTextActiveColor = textColor;
        token.TextHoverBg = PrimeNGPresets.Surface(context, dark ? (lara ? 700 : 800) : (lara ? 100 : 50));
    }

    /// <summary>
    /// The danger button fills (rest, hover, active), PrimeUIX <c>button.danger</c>: the error 500 / 600 / 700 stops
    /// (dark 400 / 300 / 200; Nora 600 / 700 / 800, dark 500 / 400 / 300).
    /// </summary>
    internal static (Color Base, Color Hover, Color Active) DangerFills(in ThemeAlgorithmContext context, DesignToken global)
    {
        var kind  = PrimeNGPresets.KindOf(context);
        var dark  = context.Appearance == ThemeAppearance.Dark;
        var error = PrimeNGSeverity.Error(context, global);
        int[] stops = kind == PrimeNGPresetKind.Nora
            ? dark ? [500, 400, 300] : [600, 700, 800]
            : dark ? [400, 300, 200] : [500, 600, 700];
        return (PrimeNGSeverity.Stop(error, stops[0]), PrimeNGSeverity.Stop(error, stops[1]),
            PrimeNGSeverity.Stop(error, stops[2]));
    }

    private static Thickness WithHorizontal(Thickness padding, double horizontal) =>
        new(horizontal, padding.Top, horizontal, padding.Bottom);
}

/// <summary>PrimeUIX <c>button</c> for <see cref="ButtonToken"/>; see <see cref="PrimeNGButtonFamilyTokens{TToken}"/>.</summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGButtonTokens : PrimeNGButtonFamilyTokens<ButtonToken>;

/// <summary>PrimeUIX <c>button</c> for <see cref="DropdownButtonToken"/>; see <see cref="PrimeNGButtonFamilyTokens{TToken}"/>.</summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGDropdownButtonTokens : PrimeNGButtonFamilyTokens<DropdownButtonToken>;

/// <summary>PrimeUIX <c>button</c> for <see cref="SubmitButtonToken"/>; see <see cref="PrimeNGButtonFamilyTokens{TToken}"/>.</summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGSubmitButtonTokens : PrimeNGButtonFamilyTokens<SubmitButtonToken>;

/// <summary>PrimeUIX <c>button</c> for <see cref="ResetButtonToken"/>; see <see cref="PrimeNGButtonFamilyTokens{TToken}"/>.</summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGResetButtonTokens : PrimeNGButtonFamilyTokens<ResetButtonToken>;
