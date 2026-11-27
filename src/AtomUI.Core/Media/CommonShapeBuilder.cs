using AtomUI.Utils;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Media;

/// <summary>
/// Builds the geometries of shapes that controls draw themselves: arcs, check marks and arrow heads.
/// </summary>
public static class CommonShapeBuilder
{
    /// <summary>
    /// Builds a check mark as an open polyline of two segments, meant to be stroked.
    /// </summary>
    /// <param name="size">
    /// The size the mark is scaled to. The mark does not fill it: it spans 25% to 70% of the width and 30% to 70% of
    /// the height.
    /// </param>
    /// <returns>
    /// The geometry of the mark.
    /// </returns>
    public static Geometry BuildCheckMark(Size size)
    {
        var       checkMark = new StreamGeometry();
        using var context   = checkMark.Open();

        var startPoint = new Point(size.Width * 0.25, size.Height * 0.5);
        var midPoint   = new Point(size.Width * 0.4, size.Height * 0.7);
        var endPoint   = new Point(size.Width * 0.7, size.Height * 0.3);

        context.BeginFigure(startPoint, true);
        context.LineTo(midPoint);
        context.LineTo(endPoint);
        context.EndFigure(false);

        return checkMark;
    }

    /// <summary>
    /// Builds an arc of the ellipse inscribed in a rectangle. The arc starts at one angle and spans another.
    /// </summary>
    /// <param name="rect">
    /// The bounds of the ellipse; its center is the center of the arc and half its width and height are the radii.
    /// </param>
    /// <param name="startAngle">
    /// The angle of the first point in degrees, clockwise from the right; -90 is at the top.
    /// </param>
    /// <param name="sweepAngle">
    /// The length of the arc in degrees, clockwise when positive and counter-clockwise when negative. A full turn
    /// gives the complete ellipse.
    /// </param>
    /// <returns>
    /// The arc, or an empty geometry when <paramref name="sweepAngle" /> is zero.
    /// </returns>
    public static Geometry BuildArc(Rect rect, double startAngle, double sweepAngle)
    {
        if (MathUtils.IsZero(sweepAngle))
        {
            return new StreamGeometry();
        }

        var angle1 = MathUtils.Deg2Rad(startAngle);
        var angle2 = angle1 + MathUtils.Deg2Rad(sweepAngle);

        startAngle = Math.Min(angle1, angle2);
        sweepAngle = Math.Max(angle1, angle2);

        var normStart = RadToNormRad(startAngle);
        var normEnd   = RadToNormRad(sweepAngle);

        if (MathUtils.AreClose(normStart, normEnd) && !MathUtils.AreClose(startAngle, sweepAngle))
        {
            // Complete ring.
            return new EllipseGeometry(rect);
        }

        // Partial arc.
        var deflatedRect = rect;

        var centerX = rect.Center.X;
        var centerY = rect.Center.Y;

        var radiusX = deflatedRect.Width / 2;
        var radiusY = deflatedRect.Height / 2;

        var angleGap = RadToNormRad(sweepAngle - startAngle);

        var startPoint = GetRingPoint(radiusX, radiusY, centerX, centerY, startAngle);
        var endPoint   = GetRingPoint(radiusX, radiusY, centerX, centerY, sweepAngle);

        var arcGeometry = new StreamGeometry();

        using (var context = arcGeometry.Open())
        {
            context.BeginFigure(startPoint, false);
            context.ArcTo(
                endPoint,
                new Size(radiusX, radiusY),
                angleGap,
                angleGap >= Math.PI,
                SweepDirection.Clockwise);
            context.EndFigure(false);
        }

        return arcGeometry;
    }

    private static double RadToNormRad(double inAngle)
    {
        return (inAngle % (Math.PI * 2) + Math.PI * 2) % (Math.PI * 2);
    }

    private static Point GetRingPoint(double radiusX, double radiusY, double centerX, double centerY, double angle)
    {
        return new Point(radiusX * Math.Cos(angle) + centerX, radiusY * Math.Sin(angle) + centerY);
    }

    /// <summary>
    /// Builds an arrow head: a triangle that points up and has a rounded tip. The figure is closed.
    /// </summary>
    /// <param name="size">
    /// The width and height of the box the triangle is laid out in. The base is the bottom edge of the box; the tip
    /// lies at <c>size / 2 + 1</c>, just below the center.
    /// </param>
    /// <param name="radius">
    /// The half-width of the rounded tip. The rounding starts this far left of the tip and ends this far right of
    /// it, with the control point of the curve this far above the tip.
    /// </param>
    /// <returns>
    /// The geometry of the arrow head.
    /// </returns>
    public static Geometry BuildArrow(double size, double radius)
    {
        var       geometryStream = new StreamGeometry();
        using var context        = geometryStream.Open();

        // p1 is the apex of the triangle before rounding; p2 and p3 are the two base corners.
        var p1 = new Point(size / 2.0, size / 2.0 + 1);
        var p2 = new Point(0, size);
        var p3 = new Point(size, size);
        // The control point of the rounding lies `radius` above the apex.
        var controlPoint = new Point(p1.X, p1.Y - radius);
        // Start the figure at the bottom-left base corner.
        context.BeginFigure(p2, true);
        // Draw the left edge up to where the rounding begins.
        context.LineTo(new Point(p1.X - radius, p1.Y));
        // Draw the rounded tip.
        context.QuadraticBezierTo(controlPoint, new Point(p1.X + radius, p1.Y));
        // Draw the right edge down to the bottom-right base corner.
        context.LineTo(p3);
        // Draw the base and close the figure.
        context.LineTo(p2);
        context.EndFigure(true);

        return geometryStream;
    }
}