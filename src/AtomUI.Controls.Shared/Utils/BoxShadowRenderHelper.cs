using Avalonia;
using Avalonia.Media;

namespace AtomUI.Controls.Utils;

/// <summary>
/// Draws a rectangle with box shadows so that shadow offsets follow the render scaling.
/// </summary>
/// <remarks>
/// Avalonia's Skia backend translates a box shadow by its offset after the current transform has been
/// applied, so offsets are interpreted in device pixels while the rectangle, spread and blur follow the
/// render scaling and any render transform. At 200% scaling an offset shadow therefore moves half as far
/// as intended, and an inset shadow whose offset is canceled by a negative spread disappears.
/// Shadows whose result is affected are drawn in an equivalent form without an offset:
/// <list type="bullet">
/// <item>inset shadows become a zero-offset inset shadow on a larger rectangle, clipped back to the box;</item>
/// <item>outer shadows without blur become a filled geometry (shifted shadow shape minus the box) when the
/// corner radii are equal.</item>
/// </list>
/// Other shadows, including blurred outer shadows with an offset, are passed to Avalonia unchanged.
/// </remarks>
internal static class BoxShadowRenderHelper
{
    /// <summary>
    /// Equivalent of <see cref="DrawingContext.DrawRectangle(IBrush?, IPen?, RoundedRect, BoxShadows)" />
    /// with scale-independent shadow offsets. When a shadow has an offset, the stroke is the one other
    /// difference: Avalonia paints it after the inset shadows, this method with the background.
    /// </summary>
    public static void DrawRectangle(DrawingContext context, IBrush? brush, IPen? pen, RoundedRect rect,
                                     BoxShadows boxShadows)
    {
        if (!NeedsWorkaround(boxShadows))
        {
            context.DrawRectangle(brush, pen, rect, boxShadows);
            return;
        }

        // Same order as Avalonia's rectangle drawing: outer shadows, background, inset shadows.
        // Only the stroke differs: Avalonia paints it after the inset shadows, here it comes with the background.
        foreach (var shadow in boxShadows)
        {
            if (shadow != default && !shadow.IsInset)
            {
                DrawOuterShadow(context, rect, shadow);
            }
        }

        if (brush != null || pen != null)
        {
            context.DrawRectangle(brush, pen, rect);
        }

        foreach (var shadow in boxShadows)
        {
            if (shadow != default && shadow.IsInset)
            {
                DrawInsetShadow(context, rect, shadow);
            }
        }
    }

    private static bool NeedsWorkaround(BoxShadows boxShadows)
    {
        foreach (var shadow in boxShadows)
        {
            if (shadow != default && HasOffset(shadow))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasOffset(BoxShadow shadow) => shadow.OffsetX != 0 || shadow.OffsetY != 0;

    private static void DrawOuterShadow(DrawingContext context, RoundedRect rect, BoxShadow shadow)
    {
        if (!HasOffset(shadow) || shadow.Blur > 0 || !TryCreateGeometry(rect, out var boxGeometry))
        {
            context.DrawRectangle(null, null, rect, new BoxShadows(shadow));
            return;
        }

        var shadowRect = Translate(rect.Inflate(shadow.Spread, shadow.Spread), shadow.OffsetX, shadow.OffsetY);
        if (!TryCreateGeometry(shadowRect, out var shadowGeometry))
        {
            context.DrawRectangle(null, null, rect, new BoxShadows(shadow));
            return;
        }

        // An outer shadow without blur is the translated and spread shape of the box minus the box itself.
        context.DrawGeometry(new SolidColorBrush(shadow.Color), null,
            new CombinedGeometry(GeometryCombineMode.Exclude, shadowGeometry, boxGeometry));
    }

    private static void DrawInsetShadow(DrawingContext context, RoundedRect rect, BoxShadow shadow)
    {
        if (!HasOffset(shadow))
        {
            context.DrawRectangle(null, null, rect, new BoxShadows(shadow));
            return;
        }

        // In CSS semantics the hole of an inset shadow is the box deflated by the spread and then translated by the
        // offset; the blurred area outside the hole is painted inside the box only.
        // A zero-offset inset shadow on a rectangle that covers the whole box cuts out the same hole. Clipped back to
        // the box, it is equivalent to the original shadow.
        var box      = rect.Rect;
        var holeRect = Translate(rect.Deflate(shadow.Spread, shadow.Spread), shadow.OffsetX, shadow.OffsetY);
        var hole     = holeRect.Rect;
        var spread = Math.Max(0, Math.Max(Math.Max(hole.Left - box.Left, box.Right - hole.Right),
            Math.Max(hole.Top - box.Top, box.Bottom - hole.Bottom)));
        var equivalent = new BoxShadow
        {
            Blur    = shadow.Blur,
            Spread  = spread,
            IsInset = true,
            Color   = shadow.Color
        };
        using (context.PushClip(rect))
        {
            context.DrawRectangle(null, null, holeRect.Inflate(spread, spread), new BoxShadows(equivalent));
        }
    }

    private static RoundedRect Translate(RoundedRect rect, double offsetX, double offsetY) =>
        new(rect.Rect.Translate(new Vector(offsetX, offsetY)),
            rect.RadiiTopLeft, rect.RadiiTopRight, rect.RadiiBottomRight, rect.RadiiBottomLeft);

    private static bool TryCreateGeometry(RoundedRect rect, out Geometry geometry)
    {
        if (!rect.IsRounded)
        {
            geometry = new RectangleGeometry(rect.Rect);
            return true;
        }

        var radius = rect.RadiiTopLeft;
        if (rect.RadiiTopRight == radius && rect.RadiiBottomRight == radius && rect.RadiiBottomLeft == radius)
        {
            geometry = new RectangleGeometry(rect.Rect, radius.X, radius.Y);
            return true;
        }

        // No ready-made geometry exists for unequal corner radii, so the shadow is left to Avalonia.
        geometry = null!;
        return false;
    }
}
