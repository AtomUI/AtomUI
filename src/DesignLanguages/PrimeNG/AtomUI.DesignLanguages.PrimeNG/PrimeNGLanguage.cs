using AtomUI.DesignLanguages.PrimeNG.TokenEvaluation;
using AtomUI.DesignLanguages.PrimeNG.Presets;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.DesignLanguages.PrimeNG;

/// <summary>
/// The PrimeNG language (https://primeng.org/theming/styled, the standard presets of <c>@primeuix/themes</c> 3.1.1 at a
/// 16px root): the Aura (default), Lara and Nora styles, the default density and the compact density (an AtomUI
/// extension), the desktop idiom. The primary color is the seed an end user may adjust; its default is primeng.dev's
/// "noir" (slate 950), which takes the surface scale as the primary family.
/// </summary>
[DesignLanguage]
public sealed class PrimeNGLanguage : DesignLanguage<PrimeNGToken>
{
    private const string DefaultPrimary = "#020617";
    private static readonly Color s_defaultPrimary = Color.Parse(DefaultPrimary);

    /// <summary>
    /// The Aura preset: slate / zinc surfaces, a one-line focus ring (the default style).
    /// </summary>
    public static DesignStyle Aura { get; } = new("Aura");

    /// <summary>
    /// The Lara preset: a primary glow around focused fields instead of a ring, taller controls.
    /// </summary>
    public static DesignStyle Lara { get; } = new("Lara");

    /// <summary>
    /// The Nora preset: small radii, a two-line ring, solid highlights and instant state transitions.
    /// </summary>
    public static DesignStyle Nora { get; } = new("Nora");

    private static readonly DesignLanguageAxes s_axes = new(
        [Aura, Lara, Nora],
        Aura,
        [DesignDensity.Default, DesignDensity.Compact],
        [DesignIdiom.Desktop],
        [GlobalTokenSchema.Get(nameof(DesignToken.ColorPrimary))])
    {
        SeedDerivations = new Dictionary<string, IReadOnlySet<string>>(StringComparer.Ordinal)
        {
            [nameof(DesignToken.ColorPrimary)] = new HashSet<string>(StringComparer.Ordinal)
            {
                nameof(DesignToken.ColorPrimaryHover), nameof(DesignToken.ColorPrimaryActive),
                nameof(DesignToken.ColorPrimaryBg), nameof(DesignToken.ColorPrimaryBgHover),
                nameof(DesignToken.ColorPrimaryBorder), nameof(DesignToken.ColorPrimaryBorderHover),
                nameof(DesignToken.ColorPrimaryTextHover), nameof(DesignToken.ColorPrimaryText),
                nameof(DesignToken.ColorPrimaryTextActive), nameof(DesignToken.SelectionBackground),
                nameof(DesignToken.ColorLinkHover), nameof(DesignToken.ColorLinkActive)
            }
        }
    };

    /// <summary>
    /// The stable identity of the language, <c>PrimeNG</c>.
    /// </summary>
    public override string Id => "PrimeNG";

    /// <summary>
    /// The axes this language offers: the styles <see cref="Aura" />, <see cref="Lara" /> and <see cref="Nora" />,
    /// with <see cref="Aura" /> as the default, the default and the compact density, the desktop idiom, and
    /// <c>ColorPrimary</c> as the only adjustable seed, with the tokens that its derivation produces registered in
    /// <see cref="DesignLanguageAxes.SeedDerivations" />.
    /// </summary>
    public override DesignLanguageAxes Axes => s_axes;

    private static readonly IReadOnlyDictionary<string, string> s_defaultThemeSeeds =
        new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(
            new Dictionary<string, string> { [nameof(DesignToken.ColorPrimary)] = DefaultPrimary });

    /// <summary>
    /// The seed values of the language's default theme, read before any theme is evaluated: <c>ColorPrimary</c> is
    /// <c>#020617</c> (slate 950, the "noir" of primeng.dev), which the catalog also shows as the default theme's
    /// accent color.
    /// </summary>
    public override IReadOnlyDictionary<string, string> DefaultThemeSeeds => s_defaultThemeSeeds;

    /// <summary>
    /// The preset seeds of the previous PrimeNG theme profiles: the noir primary (slate 950) and the Tailwind status
    /// colors (red, green, orange and sky 500), no click wave, the preset's base radius (Aura and Lara 6, Nora 2) and
    /// control height (Aura and Nora 35, Lara 39). The appearance and density change no seed.
    /// </summary>
    public override void DefineSeeds(in DesignVariant variant, DesignToken seed, PrimeNGToken languageSeed)
    {
        var kind = PrimeNGPresets.KindOf(variant.Style);
        seed.ColorPrimary     = s_defaultPrimary;
        seed.ColorError       = Color.FromRgb(0xEF, 0x44, 0x44);
        seed.ColorSuccess     = Color.FromRgb(0x22, 0xC5, 0x5E);
        seed.ColorWarning     = Color.FromRgb(0xF9, 0x73, 0x16);
        seed.ColorInfo        = Color.FromRgb(0x0E, 0xA5, 0xE9);
        seed.EnableWaveSpirit = false;
        seed.BorderRadius     = new CornerRadius(kind == PrimeNGPresetKind.Nora ? 2 : 6);
        seed.ControlHeight    = kind == PrimeNGPresetKind.Lara ? 39 : 35;
    }

    /// <summary>
    /// The surface scale of the style and appearance (light slate, dark zinc in all three presets).
    /// </summary>
    public override void DeriveLanguage(in DesignVariant variant, DesignToken seed, PrimeNGToken language)
    {
        var surface = PrimeNGPresets.Of(variant.Style).For(variant.Appearance).Surface;
        language.Surface50  = surface[0];
        language.Surface100 = surface[1];
        language.Surface200 = surface[2];
        language.Surface300 = surface[3];
        language.Surface400 = surface[4];
        language.Surface500 = surface[5];
        language.Surface600 = surface[6];
        language.Surface700 = surface[7];
        language.Surface800 = surface[8];
        language.Surface900 = surface[9];
        language.Surface950 = surface[10];
    }

    /// <summary>
    /// The map tokens (the Ant Design base refined by the preset's colors, transition and size ladder, then the
    /// compact ladders for the compact density), then the alias tokens.
    /// </summary>
    public override void DeriveGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken token)
    {
        var preset = PrimeNGPresets.Of(context.Variant.Style);
        MapTokenRules.Apply(context.Variant, preset, context.Language, token);
        AliasTokenRules.Apply(context.Variant, preset, context.Language, token);
        // The role tokens controls read instead of the seeds: the medium radius and the focused input border.
        token.BorderRadiusMD         = token.BorderRadius;
        token.InputActiveBorderColor = token.ColorPrimary;
    }
}
