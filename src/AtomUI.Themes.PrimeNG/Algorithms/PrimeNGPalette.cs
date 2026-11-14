using Avalonia.Media;

namespace AtomUI.Themes.PrimeNG.Algorithms;

/// <summary>
/// The PrimeNG colour scale of eleven stops (50 to 950) around a 500 seed.
/// </summary>
/// <remarks>
/// <see cref="Generate"/> reproduces the <c>palette</c>, <c>tint</c>, <c>shade</c> and <c>mix</c> functions of
/// <c>@primeuix/styled</c> 1.2.0, including the order of its floating-point operations and the JavaScript
/// <c>Math.round</c> (half up). <see cref="Resolve"/> prefers the hand-tuned Tailwind tables that PrimeNG ships.
/// </remarks>
internal static class PrimeNGPalette
{
    /// <summary>
    /// The stop numbers, in index order.
    /// </summary>
    internal static readonly int[] Stops = [50, 100, 200, 300, 400, 500, 600, 700, 800, 900, 950];

    /// <summary>
    /// The index of the 500 stop, which is the seed.
    /// </summary>
    internal const int SeedIndex = 5;

    /// <summary>
    /// Mixes two colours per sRGB channel; <paramref name="firstWeightPercent"/> is the share of
    /// <paramref name="first"/>. Each channel is rounded half up. The alpha of <paramref name="second"/> is kept.
    /// </summary>
    internal static Color Mix(Color first, Color second, double firstWeightPercent)
    {
        // Same expression as @primeuix/styled, so the doubles (and the rounding of x.5 cases) are bit-identical.
        var firstWeight  = (2 * (firstWeightPercent / 100) - 1 + 1) / 2;
        var secondWeight = 1 - firstWeight;
        return Color.FromArgb(
            second.A,
            RoundHalfUp(first.R * firstWeight + second.R * secondWeight),
            RoundHalfUp(first.G * firstWeight + second.G * secondWeight),
            RoundHalfUp(first.B * firstWeight + second.B * secondWeight));
    }

    /// <summary>
    /// Generates the eleven stops from a 500 seed: stops up to 500 mix in white (19% per step away from 500), the
    /// stops above 500 mix in black (15% per step). The seed's alpha is kept on every stop.
    /// </summary>
    internal static Color[] Generate(Color seed500)
    {
        var palette = new Color[Stops.Length];
        for (var index = 0; index < palette.Length; index++)
        {
            palette[index] = index <= SeedIndex
                ? Mix(Colors.White, seed500, 19 * (SeedIndex - index))
                : Mix(Colors.Black, seed500, 15 * (index - SeedIndex));
        }

        return palette;
    }

    /// <summary>
    /// Returns the Tailwind table whose 500 stop has the RGB of <paramref name="seed500"/> (with the seed's alpha),
    /// or <see cref="Generate"/> for any other seed. The result is a new array.
    /// </summary>
    internal static Color[] Resolve(Color seed500)
    {
        foreach (var table in PrimeNGTailwindPalettes.All)
        {
            var known = table[SeedIndex];
            if (known.R == seed500.R && known.G == seed500.G && known.B == seed500.B)
            {
                var palette = new Color[table.Length];
                for (var index = 0; index < palette.Length; index++)
                {
                    var stop = table[index];
                    palette[index] = Color.FromArgb(seed500.A, stop.R, stop.G, stop.B);
                }

                return palette;
            }
        }

        return Generate(seed500);
    }

    /// <summary>
    /// Returns the index of a stop number (50 to 950).
    /// </summary>
    internal static int IndexOf(int stop)
    {
        var index = Array.IndexOf(Stops, stop);
        return index >= 0
            ? index
            : throw new ArgumentOutOfRangeException(nameof(stop), stop, "Not a PrimeNG palette stop.");
    }

    /// <summary>
    /// JavaScript <c>Math.round</c> for the non-negative channel values: half up, never banker's rounding.
    /// </summary>
    internal static byte RoundHalfUp(double value) => (byte)Math.Clamp(Math.Floor(value + 0.5), 0, 255);
}
