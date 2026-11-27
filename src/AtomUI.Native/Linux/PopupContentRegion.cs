using System.Runtime.Versioning;
using Avalonia;

namespace AtomUI.Native.Linux;

/// <summary>
/// Rasterizes the content shape of a windowed Popup (rounded rectangle plus arrow triangle) into a union of
/// integer-coordinate rectangles, for submission as an X11 SHAPE input region (device pixels, scaled by
/// <c>RenderScaling</c>) or as a Wayland <c>wl_region</c> (surface logical coordinates, scale 1).
/// The shape is scanned row by pixel row: a pixel belongs to the input region when its center lies inside the
/// shape. Adjacent rows with the same span are merged, so the number of rectangles is about the number of rows
/// occupied by the rounded corners and the arrow.
/// </summary>
[SupportedOSPlatform("linux")]
internal static class PopupContentRegion
{
    /// <summary>
    /// Rasterizes <paramref name="shape" /> into the rectangles of an input region.
    /// </summary>
    /// <param name="shape">
    /// The content shape, in client-area DIPs.
    /// </param>
    /// <param name="scaling">
    /// The scale factor from DIPs to the target integer coordinate system.
    /// </param>
    /// <returns>
    /// The rectangles whose union is the rasterized shape, in the target integer coordinate system. Adjacent rows
    /// with the same span are merged into one rectangle. Empty when the body has no size or when
    /// <paramref name="scaling" /> is not a positive finite number.
    /// </returns>
    public static List<PixelRect> Rasterize(PopupContentShape shape, double scaling)
    {
        var result = new List<PixelRect>();
        if (shape.Body.Width <= 0 || shape.Body.Height <= 0 || !double.IsFinite(scaling) || scaling <= 0)
        {
            return result;
        }

        var body         = shape.Body * scaling;
        var arrowSide    = shape.ArrowSide;
        var arrowBounds  = shape.ArrowBounds * scaling;
        var hasArrow     = arrowSide != PopupArrowSide.None && arrowBounds.Width > 0 && arrowBounds.Height > 0;
        var cornerRadius = shape.CornerRadius;
        var maxRadius    = Math.Min(body.Width, body.Height) / 2;
        var topLeft      = Math.Clamp(cornerRadius.TopLeft * scaling, 0, maxRadius);
        var topRight     = Math.Clamp(cornerRadius.TopRight * scaling, 0, maxRadius);
        var bottomRight  = Math.Clamp(cornerRadius.BottomRight * scaling, 0, maxRadius);
        var bottomLeft   = Math.Clamp(cornerRadius.BottomLeft * scaling, 0, maxRadius);

        var shapeTop    = hasArrow ? Math.Min(body.Top, arrowBounds.Top) : body.Top;
        var shapeBottom = hasArrow ? Math.Max(body.Bottom, arrowBounds.Bottom) : body.Bottom;
        var firstRow    = (int)Math.Floor(shapeTop);
        var lastRow     = (int)Math.Ceiling(shapeBottom);

        for (var row = firstRow; row < lastRow; row++)
        {
            var y     = row + 0.5;
            var left  = double.PositiveInfinity;
            var right = double.NegativeInfinity;

            if (y >= body.Top && y < body.Bottom)
            {
                var leftInset = Math.Max(
                    CornerInset(topLeft, body.Top + topLeft - y),
                    CornerInset(bottomLeft, y - (body.Bottom - bottomLeft)));
                var rightInset = Math.Max(
                    CornerInset(topRight, body.Top + topRight - y),
                    CornerInset(bottomRight, y - (body.Bottom - bottomRight)));
                left  = body.Left + leftInset;
                right = body.Right - rightInset;
            }

            if (hasArrow && TryGetArrowSpan(arrowBounds, arrowSide, y, out var arrowLeft, out var arrowRight))
            {
                // The arrow base touches the body, so the two spans of the row are contiguous; the bounding span of
                // their union suffices.
                left  = Math.Min(left, arrowLeft);
                right = Math.Max(right, arrowRight);
            }

            var x0 = (int)Math.Ceiling(left - 0.5);
            var x1 = (int)Math.Floor(right - 0.5) + 1;
            if (x1 <= x0)
            {
                continue;
            }

            if (result.Count > 0)
            {
                var last = result[^1];
                if (last.X == x0 && last.Right == x1 && last.Bottom == row)
                {
                    result[^1] = last.WithHeight(last.Height + 1);
                    continue;
                }
            }

            result.Add(new PixelRect(x0, row, x1 - x0, 1));
        }

        return result;
    }

    // Horizontal inset of a rounded corner at the vertical distance dy from the corner center.
    // dy <= 0 means that the row is outside this corner.
    private static double CornerInset(double radius, double dy)
    {
        if (radius <= 0 || dy <= 0)
        {
            return 0;
        }

        dy = Math.Min(dy, radius);
        return radius - Math.Sqrt(radius * radius - dy * dy);
    }

    private static bool TryGetArrowSpan(Rect arrow, PopupArrowSide side, double y, out double left, out double right)
    {
        left  = 0;
        right = 0;
        if (y < arrow.Top || y >= arrow.Bottom)
        {
            return false;
        }

        switch (side)
        {
            case PopupArrowSide.Top:
            case PopupArrowSide.Bottom:
            {
                // The tip of the arrow is at the midpoint of the outer edge, and its base touches the body.
                var progress = side == PopupArrowSide.Top
                    ? (y - arrow.Top) / arrow.Height
                    : (arrow.Bottom - y) / arrow.Height;
                var halfWidth = progress * arrow.Width / 2;
                left  = arrow.Center.X - halfWidth;
                right = arrow.Center.X + halfWidth;
                return halfWidth > 0;
            }
            case PopupArrowSide.Left:
            case PopupArrowSide.Right:
            {
                var distance = Math.Abs(y - arrow.Center.Y) / (arrow.Height / 2);
                var cut      = Math.Clamp(distance, 0, 1) * arrow.Width;
                if (side == PopupArrowSide.Left)
                {
                    left  = arrow.Left + cut;
                    right = arrow.Right;
                }
                else
                {
                    left  = arrow.Left;
                    right = arrow.Right - cut;
                }
                return right > left;
            }
            default:
                return false;
        }
    }
}
