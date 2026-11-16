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
}
