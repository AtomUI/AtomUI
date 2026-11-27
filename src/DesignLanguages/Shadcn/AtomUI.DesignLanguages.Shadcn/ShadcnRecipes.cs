using AtomUI.DesignLanguages.Shadcn.TokenEvaluation;
using Avalonia.Media;

namespace AtomUI.DesignLanguages.Shadcn;

/// <summary>
/// The shadcn color and shadow arithmetic the control token rules share: Tailwind opacity modifiers as opaque
/// composites, alpha strengths, the ring-then-shadow assembly of the floating surfaces and the Tailwind palette.
/// </summary>
public static class ShadcnRecipes
{
    /// <summary>
    /// Alpha-composites <paramref name="source" /> over <paramref name="background" /> into an opaque color, the AtomUI
    /// stand-in for a Tailwind <c>bg-primary/80</c>-style value.
    /// </summary>
    public static Color CompositeOver(Color source, double alpha, Color background) =>
        ShadcnBaseColors.CompositeOver(source, alpha, background);

    /// <summary>
    /// The color with the given alpha byte.
    /// </summary>
    public static Color WithAlpha(Color color, byte alpha) => Color.FromArgb(alpha, color.R, color.G, color.B);

    /// <summary>
    /// The color at a percent strength (<c>10</c> is <c>#1A</c>).
    /// </summary>
    public static Color WithPercent(Color color, double percent) => WithAlpha(color, AlphaByte(percent));

    /// <summary>
    /// Scales the alpha the color already carries (a CSS opacity modifier on an alpha color).
    /// </summary>
    public static Color MultiplyAlpha(Color color, double opacity) =>
        WithAlpha(color, (byte)Math.Round(color.A * opacity, MidpointRounding.AwayFromZero));

    /// <summary>
    /// Converts a percent strength to its alpha byte the way Tailwind rounds (25.5 → 26).
    /// </summary>
    public static byte AlphaByte(double percent) =>
        (byte)Math.Clamp(Math.Round(percent * 255 / 100d, MidpointRounding.AwayFromZero), 0, 255);

    /// <summary>
    /// The outline ring (<c>ring-1</c>: a 1px spread shadow) first, then the shadow level: CSS paints the ring above
    /// the shadow, and AtomUI paints the first shadow on top.
    /// </summary>
    public static BoxShadows RingThenShadow(Color ring, BoxShadows shadows)
    {
        var ringShadow = new BoxShadow { Spread = 1, Color = ring };
        if (shadows.Count == 0)
        {
            return new BoxShadows(ringShadow);
        }

        var rest = new BoxShadow[shadows.Count];
        for (var i = 0; i < shadows.Count; i++)
        {
            rest[i] = shadows[i];
        }

        return new BoxShadows(ringShadow, rest);
    }

    /// <summary>
    /// The official <c>shadow-xs</c> level.
    /// </summary>
    public static BoxShadows ShadowXS => ShadcnStyleTables.Shadow("xs");

    /// <summary>
    /// One stop (<c>50..950</c>) of a Tailwind v4.3.5 color family.
    /// </summary>
    public static Color TailwindStop(string family, int stop) => ShadcnTailwindPalette.Stop(family, stop);

    /// <summary>
    /// Mixes <paramref name="end" /> into <paramref name="start" /> in OKLCH (CSS <c>color-mix(in oklch, …)</c>) along the
    /// shorter hue arc; an achromatic endpoint borrows the other endpoint's hue.
    /// </summary>
    public static Color MixOklch(Color start, Color end, double endWeight)
    {
        var from = ShadcnBaseColors.OklchOf(start);
        var to = ShadcnBaseColors.OklchOf(end);
        var startHue = from.Chroma < 0.0001 ? to.Hue : from.Hue;
        var endHue = to.Chroma < 0.0001 ? startHue : to.Hue;
        return ShadcnBaseColors.ColorFromOklch(
            from.Lightness + (to.Lightness - from.Lightness) * endWeight,
            from.Chroma + (to.Chroma - from.Chroma) * endWeight,
            startHue + Math.IEEERemainder(endHue - startHue, Math.Tau) * endWeight);
    }
}
