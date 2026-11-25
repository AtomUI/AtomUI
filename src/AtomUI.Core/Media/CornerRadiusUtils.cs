using Avalonia;

namespace AtomUI.Media;

/// <summary>
/// CSS border-radius semantics for corner radii: a corner never exceeds half the shorter side of its rectangle.
/// Avalonia draws an oversized <see cref="CornerRadius"/> asymmetrically instead of scaling it down like the
/// browser does, so every self-drawn rounded rect whose radius comes from a theme token should clamp first.
/// </summary>
public static class CornerRadiusUtils
{
    public static CornerRadius ClampToSize(CornerRadius radius, double width, double height)
    {
        var limit = Math.Min(width, height) / 2;
        if (limit <= 0)
        {
            return default;
        }
        // CornerRadius ctor order: topLeft, topRight, bottomRight, bottomLeft.
        return new CornerRadius(
            Math.Min(radius.TopLeft, limit),
            Math.Min(radius.TopRight, limit),
            Math.Min(radius.BottomRight, limit),
            Math.Min(radius.BottomLeft, limit));
    }

    /// <summary>
    /// CSS border-radius scaling: when the radii of two adjacent corners overlap along an edge,
    /// every radius is reduced by the same factor f = min(edge length / sum of the adjacent
    /// radii) — the rule Skia's rounded rect (and the browser) applies. Unlike
    /// <see cref="ClampToSize" />, a corner whose neighbour is small enough for both to share the
    /// edge keeps its full radius.
    /// </summary>
    public static CornerRadius ScaleToSize(CornerRadius radius, double width, double height)
    {
        if (width <= 0 || height <= 0)
        {
            return default;
        }

        var factor = Math.Min(
            Math.Min(
                width / (radius.TopLeft + radius.TopRight),
                width / (radius.BottomLeft + radius.BottomRight)),
            Math.Min(
                height / (radius.TopLeft + radius.BottomLeft),
                height / (radius.TopRight + radius.BottomRight)));
        if (factor >= 1)
        {
            return radius;
        }

        // CornerRadius ctor order: topLeft, topRight, bottomRight, bottomLeft.
        return new CornerRadius(
            radius.TopLeft * factor,
            radius.TopRight * factor,
            radius.BottomRight * factor,
            radius.BottomLeft * factor);
    }
}
