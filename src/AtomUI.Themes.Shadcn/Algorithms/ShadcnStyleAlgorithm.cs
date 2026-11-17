using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Themes.Shadcn.Algorithms;

/// <summary>
/// The shared implementation of the shadcn style algorithms (one per style, each in a light and a dark variant).
/// Each variant is the same code over its style's table, the resolved base color and the resolved theme.
/// </summary>
/// <remarks>
/// <para>
/// The Map stage first runs the matching Ant Design algorithm so every Map token has a value. The seeds the Ant
/// calculator sees are adapted for the call (and restored right after): the text and background base seeds follow
/// the resolved appearance of the base color, the error seed is the destructive variable and the primary seed is
/// the resolved theme primary — so the Ant derivations that the 5.4 table does not overwrite (the error family,
/// the fill gradients) come out in shadcn colours with the right light-on-dark direction.
/// </para>
/// <para>
/// A dark variant that follows another algorithm only recomputes the colours; the sizes of the previous Map stay,
/// as with <c>AtomUI:Dark</c>. Compact density is <c>AtomUI:Compact</c> appended to the chain. The Map also
/// preserves the <c>ColorNeutral</c> seed (writing the default when it was not set) for the later stages.
/// </para>
/// <para>
/// The Alias stage follows <see cref="ThemeAlgorithmContext.Appearance"/>, not the variant of this instance: a
/// mixed chain such as <c>[Shadcn:Nova, AtomUI:Dark]</c> gets the dark values. It re-resolves the base color and
/// theme from the computed <c>ColorNeutral</c> / <c>ColorPrimary</c> — the seed is no longer visible there.
/// </para>
/// </remarks>
public abstract class ShadcnStyleAlgorithm : IThemeAlgorithm, IThemeAliasAlgorithm
{
    private readonly ShadcnStyle _style;
    private readonly bool _dark;

    private protected ShadcnStyleAlgorithm(ShadcnStyle style, bool dark)
    {
        ArgumentNullException.ThrowIfNull(style);
        _style = style;
        _dark  = dark;
    }

    /// <summary>The style this algorithm family renders; the control stages read the same table.</summary>
    internal ShadcnStyle Style => _style;

    /// <inheritdoc />
    public void Evaluate(DesignToken effectiveSeed, DesignToken? previousMap, DesignToken nextMap)
    {
        ArgumentNullException.ThrowIfNull(effectiveSeed);
        ArgumentNullException.ThrowIfNull(nextMap);

        var surface = ShadcnBaseColors.Resolve(effectiveSeed.ColorNeutral, _dark);
        var theme   = ShadcnThemeColor.Resolve(effectiveSeed.ColorPrimary, surface, _dark);

        // The compiler passes one shared seed instance to every algorithm of the chain and never records writes on
        // it; the adaptation below is restored before this method returns, so no later stage can observe it.
        var textBase = effectiveSeed.ColorTextBase;
        var bgBase   = effectiveSeed.ColorBgBase;
        var error    = effectiveSeed.ColorError;
        var primary  = effectiveSeed.ColorPrimary;
        effectiveSeed.ColorTextBase = surface.Foreground;
        effectiveSeed.ColorBgBase   = surface.Background;
        effectiveSeed.ColorError    = surface.Destructive;
        effectiveSeed.ColorPrimary  = theme.Primary;
        try
        {
            if (_dark)
            {
                new DarkThemeVariantCalculator().Evaluate(effectiveSeed, previousMap, nextMap);
            }
            else
            {
                new DefaultThemeVariantCalculator().Evaluate(effectiveSeed, previousMap, nextMap);
            }
        }
        finally
        {
            effectiveSeed.ColorTextBase = textBase;
            effectiveSeed.ColorBgBase   = bgBase;
            effectiveSeed.ColorError    = error;
            effectiveSeed.ColorPrimary  = primary;
        }

        WriteColors(nextMap, surface, theme);
        if (!_dark || previousMap is null)
        {
            WriteSizes(nextMap, _style);
        }

        // The dialog mask strength of the style, as a Map token with the overlay table.
        nextMap.ColorBgMask = Color.FromArgb(ShadcnStyles.AlphaByte(_style.Overlay.MaskAlpha), 0, 0, 0);

        // The later stages resolve the base color and theme from these two computed tokens; the default seed makes
        // the neutral base explicit when nothing was chosen.
        nextMap.ColorNeutral = effectiveSeed.ColorNeutral ?? ShadcnBaseColors.DefaultSeed;
    }

    /// <inheritdoc />
    public void EvaluateAlias(in ThemeAlgorithmContext context, DesignToken token)
    {
        ArgumentNullException.ThrowIfNull(token);
        var dark    = context.Appearance == ThemeAppearance.Dark;
        var style   = ShadcnStyles.Resolve(context);
        var surface = ShadcnBaseColors.Resolve(token.ColorNeutral, dark);
        // The Map wrote the appearance-adjusted primary; resolve from the computed value, not the seed.
        var theme   = ShadcnThemeColor.ResolveComputed(token.ColorPrimary, surface, dark);

        // ring-ring/50: the focus ring is the ring variable at the style's focus alpha; the width comes with it.
        token.ColorControlOutline = ShadcnStyles.WithAlpha(surface.Ring, ShadcnStyles.AlphaByte(style.FocusRingAlpha));
        token.ControlOutlineWidth = style.FocusRingWidth;
        token.ColorTextLightSolid = theme.Foreground;
        token.ColorTextHeading    = surface.Foreground;

        // The Ant Design alias formulas re-derive these from the Map values after the chain; the shadcn variables
        // have the last word here (as with the PrimeNG presets).
        token.ColorIcon                = surface.Foreground;
        token.ColorIconHover           = surface.Foreground;
        token.ColorFillContent         = surface.Muted;
        token.ColorSplit               = surface.Border;
        token.ControlItemBgHover       = surface.Accent;
        token.ControlItemBgActive      = surface.Accent;
        token.ControlItemColorActive   = surface.AccentForeground;
        token.ColorTextDescription     = surface.MutedForeground;
        token.ColorTextPlaceholder     = surface.MutedForeground;
        token.ColorBgContainerDisabled = surface.Muted;
        token.ColorTextDisabled        = ShadcnBaseColors.CompositeOver(surface.MutedForeground, 0.5, surface.Background);

        // Global shadow pair: the elevated surface carries the style's card ring and shadow; the secondary keeps
        // the xs level. The floating surfaces get their own assembly in the control stages.
        var cardRing = ShadcnStyles.WithAlpha(
            surface.Foreground, ShadcnStyles.AlphaByte(style.CardRingAlpha));
        token.BoxShadows         = ShadcnStyles.RingThenShadow(cardRing, style.CardShadow);
        token.BoxShadowsSecondary = ShadcnStyles.Shadow("xs");
    }

    private static void WriteColors(DesignToken nextMap, ShadcnSurface surface, ShadcnPrimary theme)
    {
        // Design 5.4: every base-color variable lands on its AtomUI token, in its own stage — the Map-stage part
        // here, the Alias-stage part after the Ant Design alias formulas in EvaluateAlias.
        nextMap.ColorBgLayout   = surface.Background;
        nextMap.ColorBgContainer = surface.Background;
        nextMap.ColorBgElevated = surface.Popover;
        nextMap.ColorBgSpotlight = surface.Foreground;

        nextMap.ColorText           = surface.Foreground;
        nextMap.ColorTextSecondary  = surface.MutedForeground;
        var mutedComposite = ShadcnBaseColors.CompositeOver(surface.MutedForeground, 0.5, surface.Background);
        nextMap.ColorTextTertiary   = mutedComposite;
        nextMap.ColorTextQuaternary = mutedComposite;

        nextMap.ColorFill          = surface.Accent;
        nextMap.ColorFillSecondary = surface.Secondary;
        nextMap.ColorFillTertiary  = surface.Muted;
        nextMap.ColorFillQuaternary = surface.Muted;

        nextMap.ColorBorder         = surface.Border;
        nextMap.ColorBorderSecondary = surface.Border;

        nextMap.ColorError = surface.Destructive;

        nextMap.ColorPrimary = theme.Primary;
        // The brand family (design 5.3.3): hover:bg-primary/80 with the press being a translate, not a colour, so
        // the hover and active pair are the same 80% composite; the pale surfaces and borders are AtomUI proposals
        // (shadcn has no such variables).
        nextMap.ColorPrimaryHover       = ShadcnBaseColors.CompositeOver(theme.Primary, 0.8, surface.Background);
        nextMap.ColorPrimaryActive      = nextMap.ColorPrimaryHover;
        nextMap.ColorPrimaryBg          = ShadcnBaseColors.CompositeOver(theme.Primary, 0.1, surface.Background);
        nextMap.ColorPrimaryBgHover     = ShadcnBaseColors.CompositeOver(theme.Primary, 0.2, surface.Background);
        nextMap.ColorPrimaryBorder      = ShadcnBaseColors.CompositeOver(theme.Primary, 0.3, surface.Background);
        nextMap.ColorPrimaryBorderHover = ShadcnBaseColors.CompositeOver(theme.Primary, 0.5, surface.Background);
        nextMap.ColorPrimaryText        = theme.Primary;
        nextMap.ColorPrimaryTextHover   = nextMap.ColorPrimaryHover;
        nextMap.ColorPrimaryTextActive  = nextMap.ColorPrimaryActive;
        // The solid surfaces (Ant solid buttons) take the theme primary and its 80% hover composite.
        nextMap.ColorBgSolid       = theme.Primary;
        nextMap.ColorBgSolidHover  = nextMap.ColorPrimaryHover;
        nextMap.ColorBgSolidActive = nextMap.ColorBgSolidHover;
    }

    private static void WriteSizes(DesignToken nextMap, ShadcnStyle style)
    {
        nextMap.ControlHeight   = style.ControlHeight;
        nextMap.ControlHeightSM = style.ControlHeightSM;
        nextMap.ControlHeightLG = style.ControlHeightLG;
        nextMap.ControlHeightXS = style.ControlHeightXS;
        // The full radius ladder (many controls read SM/LG/XS, e.g. the Segmented thumb and IconButton): the
        // per-style table at every size, mirroring the official one-radius-per-style classes.
        nextMap.BorderRadius    = new CornerRadius(style.ButtonRadius);
        nextMap.BorderRadiusSM  = new CornerRadius(style.ButtonRadiusSM);
        nextMap.BorderRadiusLG  = new CornerRadius(style.ButtonRadiusLG);
        nextMap.BorderRadiusXS  = new CornerRadius(style.ButtonRadiusSM);
        nextMap.FontSize        = style.FontSize;
        nextMap.FontSizeSM      = style.FontSizeSM;
    }
}
