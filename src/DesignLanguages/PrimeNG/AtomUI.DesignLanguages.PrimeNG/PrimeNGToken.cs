using System.Collections.Immutable;
using AtomUI.DesignLanguages.PrimeNG.Presets;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.DesignLanguages.PrimeNG;

/// <summary>
/// PrimeNG's own vocabulary: the PrimeUIX <c>surface</c> scale of the current style and appearance (light slate, dark
/// zinc in all three presets), which the global tokens have no place for. The global derivation and the control
/// derivations take every surface stop from here, so a theme that overrides a stop changes every value derived from it.
/// <c>surface.0</c> is pure white and has no token.
/// </summary>
[LanguageDesignToken]
public partial class PrimeNGToken : LanguageDesignToken
{
    /// <summary>
    /// PrimeUIX <c>surface.50</c>.
    /// </summary>
    public partial Color Surface50 { get; set; }

    /// <summary>
    /// PrimeUIX <c>surface.100</c>.
    /// </summary>
    public partial Color Surface100 { get; set; }

    /// <summary>
    /// PrimeUIX <c>surface.200</c>.
    /// </summary>
    public partial Color Surface200 { get; set; }

    /// <summary>
    /// PrimeUIX <c>surface.300</c>.
    /// </summary>
    public partial Color Surface300 { get; set; }

    /// <summary>
    /// PrimeUIX <c>surface.400</c>.
    /// </summary>
    public partial Color Surface400 { get; set; }

    /// <summary>
    /// PrimeUIX <c>surface.500</c>.
    /// </summary>
    public partial Color Surface500 { get; set; }

    /// <summary>
    /// PrimeUIX <c>surface.600</c>.
    /// </summary>
    public partial Color Surface600 { get; set; }

    /// <summary>
    /// PrimeUIX <c>surface.700</c>.
    /// </summary>
    public partial Color Surface700 { get; set; }

    /// <summary>
    /// PrimeUIX <c>surface.800</c>.
    /// </summary>
    public partial Color Surface800 { get; set; }

    /// <summary>
    /// PrimeUIX <c>surface.900</c>.
    /// </summary>
    public partial Color Surface900 { get; set; }

    /// <summary>
    /// PrimeUIX <c>surface.950</c>.
    /// </summary>
    public partial Color Surface950 { get; set; }

    /// <summary>
    /// Backs <see cref="SurfaceScale" />: the surface stops 50 to 950 in palette order. It is the default array
    /// until the first read of the property fills it.
    /// </summary>
    private ImmutableArray<Color> _surfaceScale;

    /// <summary>
    /// The eleven surface stops in palette order (50 to 950). Read only after the language tokens are final: the first
    /// read keeps the scale (a token object recreated from frozen values computes it again from its properties).
    /// Concurrent first reads compute the same value.
    /// </summary>
    [NotTokenDefinition]
    internal ImmutableArray<Color> SurfaceScale
    {
        get
        {
            var scale = _surfaceScale;
            if (scale.IsDefault)
            {
                scale =
                [
                    Surface50, Surface100, Surface200, Surface300, Surface400, Surface500,
                    Surface600, Surface700, Surface800, Surface900, Surface950
                ];
                _surfaceScale = scale;
            }
            return scale;
        }
    }

    /// <summary>
    /// Provides table roles from the effective global tokens and the current surface scale.
    /// </summary>
    public PrimeNGTableRecipe GetTableRecipe(DesignToken global) => new(this, global);

    /// <summary>
    /// Provides the interactive list item roles from the control's effective global view.
    /// </summary>
    public PrimeNGListItemRecipe GetListItemRecipe(DesignToken global) => new(global);

    /// <summary>
    /// Provides the menu panel roles from the current theme.
    /// </summary>
    public PrimeNGOverlayRecipe GetMenuOverlayRecipe(DesignToken global, DesignVariant variant) =>
        new(this, global, variant, PrimeNGOverlayKind.Menu);

    /// <summary>
    /// Provides the select panel roles from the current theme.
    /// </summary>
    public PrimeNGOverlayRecipe GetSelectOverlayRecipe(DesignToken global, DesignVariant variant) =>
        new(this, global, variant, PrimeNGOverlayKind.Select);

    /// <summary>
    /// Provides the popover panel roles from the current theme.
    /// </summary>
    public PrimeNGOverlayRecipe GetPopoverOverlayRecipe(DesignToken global, DesignVariant variant) =>
        new(this, global, variant, PrimeNGOverlayKind.Popover);

    /// <summary>
    /// Provides the dialog panel roles from the current theme.
    /// </summary>
    public PrimeNGOverlayRecipe GetDialogOverlayRecipe(DesignToken global, DesignVariant variant) =>
        new(this, global, variant, PrimeNGOverlayKind.Dialog);

    /// <summary>
    /// Provides the form-field roles from the current theme.
    /// </summary>
    public PrimeNGFieldRecipe GetFieldRecipe(DesignToken global, DesignVariant variant) => new(this, global, variant);

    /// <summary>
    /// Provides the resolved roles of the active PrimeUIX preset.
    /// </summary>
    public PrimeNGVisualRecipes GetVisualRecipes(DesignToken global, DesignVariant variant) => new(this, global, variant);

    /// <summary>
    /// Whether PrimeUIX transition.duration is zero for this preset.
    /// </summary>
    public bool HasInstantTransitions(DesignVariant variant) => PrimeNGPresets.Of(variant.Style).TransitionDuration == TimeSpan.Zero;

    /// <summary>
    /// PrimeUIX content.border.color for the current variant.
    /// </summary>
    public Color GetContentBorder(DesignVariant variant)
    {
        var rules = PrimeNGPresets.For(PrimeNGPresets.Of(variant.Style), variant.Appearance, this);
        return rules.MenuBorder.Resolve(rules.Surface.AsSpan());
    }

    /// <summary>
    /// One stop of the current PrimeUIX surface scale. Stop 0 is white.
    /// </summary>
    public Color GetSurfaceColor(int stop) => new PrimeNGShade(stop).Resolve(SurfaceScale.AsSpan());

    /// <summary>
    /// The status scale used for tags, badges and severity buttons.
    /// </summary>
    public PrimeNGSeverityScale GetStatusScale(DesignVariant variant, Color color, bool error = false)
    {
        var rules = PrimeNGPresets.Of(variant.Style).For(variant.Appearance);
        return new PrimeNGSeverityScale(PrimeNGPalette.FromStop(color, error ? rules.Error.Base.Stop : rules.Status.Base.Stop));
    }

    /// <summary>
    /// The fixed severity scale for PrimeUIX messages and notifications.
    /// </summary>
    public PrimeNGSeverityScale GetFeedbackScale(PrimeNGFeedbackSeverity severity) => new((severity switch
    {
        PrimeNGFeedbackSeverity.Information => PrimeNGTailwindPalettes.Blue,
        PrimeNGFeedbackSeverity.Success => PrimeNGTailwindPalettes.Green,
        PrimeNGFeedbackSeverity.Warning => PrimeNGTailwindPalettes.Yellow,
        PrimeNGFeedbackSeverity.Error => PrimeNGTailwindPalettes.Red,
        _ => throw new ArgumentOutOfRangeException(nameof(severity))
    }).AsSpan());

    /// <summary>
    /// PrimeUIX tag's neutral foreground and background for Aura.
    /// </summary>
    public (Color Foreground, Color Background) GetNeutralTagColors(DesignVariant variant) => variant.IsDark
        ? (Surface300, PrimeNGPalette.WithOpacity(Surface500, 0.16))
        : (Surface700, Surface100);

    /// <summary>
    /// The two surface colors of PrimeUIX skeleton and its shimmer.
    /// </summary>
    public (Color Background, Color Shimmer) GetSkeletonColors(DesignVariant variant)
    {
        var nora = variant.Style == PrimeNGLanguage.Nora;
        if (variant.IsDark)
        {
            var alpha = nora ? 0.10 : 0.06;
            return (PrimeNGPalette.WithOpacity(Colors.White, alpha),
                PrimeNGPalette.WithOpacity(Colors.White, alpha + 0.04 * (1 - alpha)));
        }

        var background = nora ? Surface300 : Surface200;
        return (background, PrimeNGPalette.Mix(Colors.White, background, 40));
    }
}
