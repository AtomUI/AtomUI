using System.Collections.Immutable;
using AtomUI.Theme;
using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.DesignLanguages.PrimeNG.Presets;

/// <summary>
/// The rows of the primary "noir" that differ from a preset's own rules: the primary family on the surface scale, the
/// link row and the contrast color.
/// </summary>
/// <param name="Primary">
/// The primary family's stops on the surface scale.
/// </param>
/// <param name="Link">
/// The link stops on the surface scale, used when the link seed is noir too.
/// </param>
/// <param name="SelectionForeground">
/// The contrast color (PrimeUIX <c>primary.contrastColor</c>).
/// </param>
internal sealed record PrimeNGNoirRule(
    PrimeNGFamilyRule Primary,
    PrimeNGLinkRule Link,
    PrimeNGShade SelectionForeground);

/// <summary>
/// The primary "noir" of primeng.dev (primeng 22.1.5 configurator, its default primary for Aura, Lara and Nora): the
/// primary palette is the surface palette, with the contrast and highlight rules of the site's noir preset extension.
/// One definition per appearance, shared by the three presets.
/// </summary>
/// <remarks>
/// <para>
/// A <c>ColorPrimary</c> seed is noir when its RGB equals the 950 stop of a Tailwind neutral family (slate, gray,
/// zinc, neutral, stone). The primary family then takes its stops from the surface scale of the appearance being
/// computed (slate in light, zinc in dark), so one remembered seed gives noir in both appearances. Any other seed
/// keeps the preset's rules.
/// </para>
/// <para>
/// The site's highlight is the solid primary: light <c>primary.950</c> (focus <c>primary.700</c>) with white text, dark
/// <c>primary.50</c> (focus <c>primary.300</c>) with <c>primary.950</c> text. It is the selected background
/// (<c>ControlItemBgActive</c>) and the selected-text tokens of the list controls, as in Nora; <c>ColorPrimaryBg</c>
/// stays the pale primary of filled buttons and tags.
/// </para>
/// </remarks>
internal static class PrimeNGNoir
{
    private const int White = PrimeNGShade.White;

    /// <summary>
    /// The neutral families whose 950 stop selects noir.
    /// </summary>
    private static readonly ImmutableArray<ImmutableArray<Color>> NeutralFamilies =
    [
        PrimeNGTailwindPalettes.Slate, PrimeNGTailwindPalettes.Gray, PrimeNGTailwindPalettes.Zinc,
        PrimeNGTailwindPalettes.Neutral, PrimeNGTailwindPalettes.Stone
    ];

    /// <summary>
    /// The light noir rows. Site: <c>primary.color</c> 950, <c>hoverColor</c> 800, <c>activeColor</c> 700,
    /// <c>contrastColor</c> white.
    /// </summary>
    internal static readonly PrimeNGNoirRule Light = new(
        Primary: new PrimeNGFamilyRule(
            Base: new(950), Hover: new(800), Active: new(700),
            // The pale primary (filled buttons, tags): PrimeUIX button.text hover / active background primary.50 /
            // primary.100; the borders one step on (Lara's focus glow, ColorPrimaryBorder, is primary.200 as on the
            // site). The solid highlight is the Highlight row.
            Bg: new(50), BgHover: new(100),
            Border: new(200), BorderHover: new(300),
            // AtomUI: primary-coloured text follows the primary states; it is drawn on the page background.
            TextHover: new(800), Text: new(950), TextActive: new(700)),
        // PrimeUIX button.link is primary.color; the states follow the primary states (20.2 / 14.6 / 10.4 on white).
        Link: new PrimeNGLinkRule(Base: new(950), Hover: new(800), Active: new(700)),
        SelectionForeground: new(White));

    /// <summary>
    /// The dark noir rows. Site: <c>primary.color</c> 50, <c>hoverColor</c> 200, <c>activeColor</c> 300,
    /// <c>contrastColor</c> 950.
    /// </summary>
    internal static readonly PrimeNGNoirRule Dark = new(
        Primary: new PrimeNGFamilyRule(
            Base: new(50), Hover: new(200), Active: new(300),
            // The pale primary (filled buttons, tags): primary.color at 16% / 24%, the Aura dark shape (PrimeUIX
            // button.text hover / active background). The borders are opaque N600 / N500: the primary end of this
            // scale is the light one, so the Aura P700@36% would be a dark gray that vanishes on N950.
            Bg: new(50, 0.16), BgHover: new(50, 0.24),
            Border: new(600), BorderHover: new(500),
            // AtomUI: primary-coloured text follows the primary states; it is drawn on the page background (surface
            // 950 would vanish there).
            TextHover: new(200), Text: new(50), TextActive: new(300)),
        Link: new PrimeNGLinkRule(Base: new(50), Hover: new(200), Active: new(300)),
        SelectionForeground: new(950));

    /// <summary>
    /// Whether a seed selects noir: its RGB is the 950 stop of a neutral family. The alpha is not compared.
    /// </summary>
    internal static bool IsSeed(Color seed)
    {
        var index = PrimeNGPalette.IndexOf(950);
        foreach (var family in NeutralFamilies)
        {
            var stop = family[index];
            if (stop.R == seed.R && stop.G == seed.G && stop.B == seed.B)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// The noir rows of an appearance.
    /// </summary>
    internal static PrimeNGNoirRule For(bool dark) => dark ? Dark : Light;

    /// <summary>
    /// The surface scale with the seed's alpha, the primary palette of a noir seed.
    /// </summary>
    internal static Color[] Palette(ImmutableArray<Color> surface, Color seed)
    {
        var palette = new Color[surface.Length];
        for (var index = 0; index < palette.Length; index++)
        {
            var stop = surface[index];
            palette[index] = Color.FromArgb(seed.A, stop.R, stop.G, stop.B);
        }

        return palette;
    }

    /// <summary>
    /// A preset's rules with the noir rows: the primary family, the contrast color and the site's highlight
    /// (background <c>primary.color</c>, focus background <c>primary.activeColor</c> - light 950 / 700, dark 50 / 300 -
    /// with the contrast color as text). The link row is applied separately, because the link seed can differ from
    /// the primary seed. The checked switch handle keeps the preset's row (Aura / Lara surface.0 / surface.900, Nora
    /// the contrast color), which stays visible on either noir track.
    /// </summary>
    internal static PrimeNGAppearanceRules Apply(PrimeNGAppearanceRules rules, PrimeNGNoirRule noir) =>
        rules with
        {
            Primary = noir.Primary,
            SelectionForeground = noir.SelectionForeground,
            Highlight = static token => (token.ColorPrimary, token.ColorPrimaryActive),
            HighlightColor = static token => token.ColorTextLightSolid
        };

    /// <summary>
    /// For the Alias and control stages, which see the computed tokens and not the seed: the rules of the appearance,
    /// with the noir rows when the primary color is the noir primary of that appearance (light surface 950, dark
    /// surface 50), which is what the map derivation writes for a noir seed.
    /// </summary>
    internal static PrimeNGAppearanceRules Resolve(PrimeNGAppearanceRules rules, ThemeAppearance appearance, DesignToken token)
    {
        return IsNoirPrimary(rules, appearance, token.ColorPrimary)
            ? Apply(rules, For(appearance == ThemeAppearance.Dark))
            : rules;
    }

    /// <summary>
    /// Whether the computed primary color is the noir primary of the appearance.
    /// </summary>
    internal static bool IsNoirPrimary(PrimeNGAppearanceRules rules, ThemeAppearance appearance, Color primary)
    {
        var noir = For(appearance == ThemeAppearance.Dark).Primary.Base.Resolve(rules.Surface.AsSpan());
        return noir.R == primary.R && noir.G == primary.G && noir.B == primary.B;
    }
}
