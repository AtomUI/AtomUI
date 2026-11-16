using Avalonia.Media;

namespace AtomUI.Themes.Shadcn.Algorithms;

/// <summary>
/// The resolved shadcn base-colour variables of one appearance, by the official step rule: every variable is a
/// stop of one neutral family (or white, or an alpha white in dark mode).
/// </summary>
/// <param name="Background">Light <c>--background</c>: white; dark: stop 950.</param>
/// <param name="Foreground"><c>--foreground</c>: light stop 950, dark stop 50.</param>
/// <param name="Card">Light <c>--card</c>: white; dark: stop 900.</param>
/// <param name="Popover">Light <c>--popover</c>: white; dark: stop 900.</param>
/// <param name="Primary">Light: stop 900; dark: stop 200.</param>
/// <param name="PrimaryForeground">Light: stop 50; dark: stop 900.</param>
/// <param name="Secondary">Stop 100 / 800.</param>
/// <param name="SecondaryForeground">Stop 900 / 50.</param>
/// <param name="Muted">Stop 100 / 800.</param>
/// <param name="MutedForeground">Stop 500 / 400; the light value is the family's seed.</param>
/// <param name="Accent">Stop 100 / 800.</param>
/// <param name="AccentForeground">Stop 900 / 50.</param>
/// <param name="Destructive">red-600 / red-400.</param>
/// <param name="Border">Stop 200; dark: white at 10% (<c>#1AFFFFFF</c>).</param>
/// <param name="Input">Stop 200; dark: white at 15% (<c>#26FFFFFF</c>).</param>
/// <param name="Ring">Stop 400 / 500.</param>
internal sealed record ShadcnSurface(
    Color Background,
    Color Foreground,
    Color Card,
    Color Popover,
    Color Primary,
    Color PrimaryForeground,
    Color Secondary,
    Color SecondaryForeground,
    Color Muted,
    Color MutedForeground,
    Color Accent,
    Color AccentForeground,
    Color Destructive,
    Color Border,
    Color Input,
    Color Ring);

/// <summary>
/// Resolves the shadcn base-colour variables from a <see cref="DesignToken.ColorNeutral"/> seed: the seed is the
/// light <c>--muted-foreground</c>, i.e. the 500 stop of one neutral family. An unknown seed generates its own
/// eleven-stop ladder in OKLCH (the neutral lightness ladder at the seed's hue, zinc's chroma ladder scaled by
/// the seed's chroma) and applies the same step rule.
/// </summary>
internal static class ShadcnBaseColors
{
    /// <summary>neutral-500, the seed of the default base color.</summary>
    public static readonly Color DefaultSeed = ShadcnTailwindPalette.Stop("neutral", 500);

    /// <summary>
    /// Alpha-composites <paramref name="source"/> over <paramref name="background"/> into an opaque colour, the
    /// AtomUI stand-in for a Tailwind <c>bg-primary/80</c>-style value (design 6.2: compositing keeps the look on
    /// non-white parents).
    /// </summary>
    public static Color CompositeOver(Color source, double alpha, Color background)
    {
        return Color.FromRgb(
            (byte)Math.Clamp(Math.Round(background.R + (source.R - background.R) * alpha), 0, 255),
            (byte)Math.Clamp(Math.Round(background.G + (source.G - background.G) * alpha), 0, 255),
            (byte)Math.Clamp(Math.Round(background.B + (source.B - background.B) * alpha), 0, 255));
    }

    private static readonly Color s_darkBorder = Color.FromArgb(0x1A, 0xFF, 0xFF, 0xFF);
    private static readonly Color s_darkInput  = Color.FromArgb(0x26, 0xFF, 0xFF, 0xFF);

    /// <summary>
    /// Gets the neutral family whose 500 stop equals the seed, or <see langword="null"/> for a custom seed.
    /// </summary>
    public static string? FamilyOf(Color seed)
    {
        foreach (var family in ShadcnTailwindPalette.NeutralFamilies)
        {
            if (ShadcnTailwindPalette.Stop(family, 500) == seed)
            {
                return family;
            }
        }

        return null;
    }

    /// <summary>
    /// Resolves the base-colour variables; a <see langword="null"/> seed resolves the default (neutral) base color.
    /// </summary>
    public static ShadcnSurface Resolve(Color? neutralSeed, bool dark)
    {
        var seed = neutralSeed ?? DefaultSeed;
        var stops = StopsOf(seed);
        return dark ? ResolveDark(stops) : ResolveLight(stops);
    }

    private static Color[] StopsOf(Color seed)
    {
        if (FamilyOf(seed) is { } family)
        {
            return [.. ShadcnTailwindPalette.Families[family]];
        }

        // Custom ladder: the neutral lightness ladder at the seed's hue; zinc's chroma at each stop scaled by the
        // seed's chroma relative to zinc's own 500 stop (so a zinc-500 seed would reproduce zinc exactly).
        var seedOklch  = OklchOf(seed);
        var zinc500    = OklchOf(ShadcnTailwindPalette.Stop("zinc", 500));
        var chromaScale = seedOklch.Chroma / zinc500.Chroma;
        var stops = new Color[11];
        for (var i = 0; i < stops.Length; i++)
        {
            var neutralStop = OklchOf(ShadcnTailwindPalette.Stop("neutral", StopOfIndex(i)));
            var zincStop    = OklchOf(ShadcnTailwindPalette.Stop("zinc", StopOfIndex(i)));
            stops[i] = ColorFromOklch(neutralStop.Lightness, zincStop.Chroma * chromaScale, seedOklch.Hue);
        }

        return stops;
    }

    private static ShadcnSurface ResolveLight(Color[] stops)
    {
        return new ShadcnSurface(
            Background:         Color.Parse("#FFFFFF"),
            Foreground:         stops[10],
            Card:               Color.Parse("#FFFFFF"),
            Popover:            Color.Parse("#FFFFFF"),
            Primary:            stops[9],
            PrimaryForeground:  stops[0],
            Secondary:          stops[1],
            SecondaryForeground: stops[9],
            Muted:              stops[1],
            MutedForeground:    stops[5],
            Accent:             stops[1],
            AccentForeground:   stops[9],
            Destructive:        ShadcnTailwindPalette.Stop("red", 600),
            Border:             stops[2],
            Input:              stops[2],
            Ring:               stops[4]);
    }

    private static ShadcnSurface ResolveDark(Color[] stops)
    {
        return new ShadcnSurface(
            Background:         stops[10],
            Foreground:         stops[0],
            Card:               stops[9],
            Popover:            stops[9],
            Primary:            stops[2],
            PrimaryForeground:  stops[9],
            Secondary:          stops[8],
            SecondaryForeground: stops[0],
            Muted:              stops[8],
            MutedForeground:    stops[4],
            Accent:             stops[8],
            AccentForeground:   stops[0],
            Destructive:        ShadcnTailwindPalette.Stop("red", 400),
            Border:             s_darkBorder,
            Input:              s_darkInput,
            Ring:               stops[5]);
    }

    private static int StopOfIndex(int index) => index switch
    {
        0 => 50, 1 => 100, 2 => 200, 3 => 300, 4 => 400, 5 => 500,
        6 => 600, 7 => 700, 8 => 800, 9 => 900, 10 => 950,
        _ => throw new ArgumentOutOfRangeException(nameof(index))
    };

    internal readonly record struct Oklch(double Lightness, double Chroma, double Hue);

    /// <summary>sRGB to OKLCH (CSS Color 4), the inverse of <see cref="ColorFromOklch"/>.</summary>
    internal static Oklch OklchOf(Color color)
    {
        double Linear(byte value)
        {
            var c = value / 255d;
            return c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
        }

        var r = Linear(color.R);
        var g = Linear(color.G);
        var b = Linear(color.B);
        var l = Math.Cbrt(0.4122214708 * r + 0.5363325363 * g + 0.0514459929 * b);
        var m = Math.Cbrt(0.2119034982 * r + 0.6806995451 * g + 0.1073969566 * b);
        var s = Math.Cbrt(0.0883024619 * r + 0.2817188376 * g + 0.6299787005 * b);
        var lightness = 0.2104542553 * l + 0.7936177850 * m - 0.0040720468 * s;
        var a         = 1.9779984951 * l - 2.4285922050 * m + 0.4505937099 * s;
        var blue      = 0.0259040371 * l + 0.7827717662 * m - 0.8086757660 * s;
        return new Oklch(lightness, Math.Sqrt(a * a + blue * blue), Math.Atan2(blue, a));
    }

    /// <summary>OKLCH to sRGB (CSS Color 4), rounded like the alignment appendix generator.</summary>
    internal static Color ColorFromOklch(double lightness, double chroma, double hue)
    {
        var a = chroma * Math.Cos(hue);
        var b = chroma * Math.Sin(hue);
        var l_ = lightness + 0.3963377774 * a + 0.2158037573 * b;
        var m_ = lightness - 0.1055613458 * a - 0.0638541728 * b;
        var s_ = lightness - 0.0894841775 * a - 1.2914855480 * b;
        var l = l_ * l_ * l_;
        var m = m_ * m_ * m_;
        var s = s_ * s_ * s_;
        var r = 4.0767416621 * l - 3.3077115913 * m + 0.2309699292 * s;
        var g = -1.2684380046 * l + 2.6097574011 * m - 0.3413193965 * s;
        var blue = -0.0041960863 * l - 0.7034186147 * m + 1.7076147010 * s;

        byte Channel(double linear)
        {
            var encoded = linear <= 0.0031308 ? 12.92 * linear : 1.055 * Math.Pow(linear, 1 / 2.4) - 0.055;
            return (byte)Math.Clamp(Math.Round(Math.Max(0, Math.Min(1, encoded)) * 255), 0, 255);
        }

        return Color.FromRgb(Channel(r), Channel(g), Channel(blue));
    }
}
