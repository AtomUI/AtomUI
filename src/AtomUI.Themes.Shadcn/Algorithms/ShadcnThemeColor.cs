using Avalonia.Media;

namespace AtomUI.Themes.Shadcn.Algorithms;

/// <summary>
/// A resolved theme primary: <see cref="Primary"/> and its <see cref="Foreground"/>, plus the theme identity —
/// <c>"neutral"</c> follows the base color, a family name is an official theme, <see langword="null"/> is custom.
/// </summary>
internal readonly record struct ShadcnPrimary(Color Primary, Color Foreground, string? Theme);

/// <summary>
/// Resolves a <see cref="DesignToken.ColorPrimary"/> seed into the theme primary by the official rule: the seed a
/// gallery swatch writes is the light <c>--primary</c>, i.e. a theme family's 700 stop (lime and yellow: 400).
/// The dark primary is the 800 stop (lime and yellow: 500), the foreground the 50 stop (lime and yellow: 900).
/// </summary>
internal static class ShadcnThemeColor
{
    // Official average light→dark derivation, computed from the palette: dark primary = lightness minus the mean
    // lightness drop, chroma times the mean chroma ratio, of the seventeen official themes.
    private static readonly (double LightnessDrop, double ChromaScale) s_darkDerivation = ComputeDarkDerivation();

    public static ShadcnPrimary Resolve(Color seed, ShadcnSurface surface, bool dark)
    {
        // A base color's light --primary is its 900 stop: the theme follows the base color in both appearances.
        foreach (var family in ShadcnTailwindPalette.NeutralFamilies)
        {
            if (ShadcnTailwindPalette.Stop(family, 900) == seed)
            {
                return new ShadcnPrimary(surface.Primary, surface.PrimaryForeground, "neutral");
            }
        }

        foreach (var family in ShadcnTailwindPalette.ThemeFamilies)
        {
            var followsLightness = family is "lime" or "yellow";
            var lightStop = followsLightness ? 400 : 700;
            if (ShadcnTailwindPalette.Stop(family, lightStop) != seed)
            {
                continue;
            }

            var darkStop = followsLightness ? 500 : 800;
            var foregroundStop = followsLightness ? 900 : 50;
            return new ShadcnPrimary(
                dark ? ShadcnTailwindPalette.Stop(family, darkStop) : seed,
                ShadcnTailwindPalette.Stop(family, foregroundStop),
                family);
        }

        // Custom seed: light keeps the seed; dark derives from the official average. The foreground prefers the
        // official look (near-white at the seed's hue) and otherwise darkens adaptively at the same hue until the
        // 4.5 contrast floor holds (mid-luminance seeds defeat both fixed official-looking candidates).
        var seedOklch = ShadcnBaseColors.OklchOf(seed);
        var primary = dark
            ? ShadcnBaseColors.ColorFromOklch(
                seedOklch.Lightness - s_darkDerivation.LightnessDrop,
                seedOklch.Chroma * s_darkDerivation.ChromaScale,
                seedOklch.Hue)
            : seed;
        var foreground = ReadableForeground(primary, seedOklch.Hue);
        return new ShadcnPrimary(primary, foreground, null);
    }

    private static (double LightnessDrop, double ChromaScale) ComputeDarkDerivation()
    {
        double lightnessSum = 0;
        double chromaRatioSum = 0;
        const int count = 17;
        foreach (var family in ShadcnTailwindPalette.ThemeFamilies)
        {
            var followsLightness = family is "lime" or "yellow";
            var lightStop = followsLightness ? 400 : 700;
            var darkStop = followsLightness ? 500 : 800;
            var light = ShadcnBaseColors.OklchOf(ShadcnTailwindPalette.Stop(family, lightStop));
            var dark = ShadcnBaseColors.OklchOf(ShadcnTailwindPalette.Stop(family, darkStop));
            lightnessSum += light.Lightness - dark.Lightness;
            chromaRatioSum += light.Chroma > 0 ? dark.Chroma / light.Chroma : 1;
        }

        return (lightnessSum / count, chromaRatioSum / count);
    }

    private static Color ReadableForeground(Color primary, double hue)
    {
        var light = ShadcnBaseColors.ColorFromOklch(0.97, 0.015, hue);
        if (Contrast(light, primary) >= 4.5)
        {
            return light;
        }

        // The dark tone starts at the official-looking L 0.42 and darkens until the floor holds; the first passing
        // lightness keeps the tone as light as possible.
        var chroma = ShadcnBaseColors.OklchOf(primary).Chroma * 0.45;
        for (var lightness = 0.42; lightness >= 0; lightness -= 0.02)
        {
            var candidate = ShadcnBaseColors.ColorFromOklch(lightness, chroma, hue);
            if (Contrast(candidate, primary) >= 4.5)
            {
                return candidate;
            }
        }

        // A mid-luminance primary can sit in its own hue's dead zone: fall back to plain white, preferring the
        // light look, then plain black.
        if (Contrast(Colors.White, primary) >= 4.5)
        {
            return Colors.White;
        }

        return Colors.Black;
    }

    private static double Contrast(Color foreground, Color background)
    {
        double Channel(byte value)
        {
            var linear = value / 255d;
            return linear <= 0.03928 ? linear / 12.92 : Math.Pow((linear + 0.055) / 1.055, 2.4);
        }

        double Luminance(Color color) =>
            0.2126 * Channel(color.R) + 0.7152 * Channel(color.G) + 0.0722 * Channel(color.B);

        var first = Luminance(foreground);
        var second = Luminance(background);
        return (Math.Max(first, second) + 0.05) / (Math.Min(first, second) + 0.05);
    }
}
