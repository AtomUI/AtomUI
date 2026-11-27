using Avalonia;
using Avalonia.Layout;
using Avalonia.Media;

namespace AtomUI.Media;

/// <summary>
/// Drawing helpers for controls that render shapes in their own <c>Render</c> method.
/// </summary>
public static class DrawingContextExtensions
{
    /// <summary>
    /// Draws a rectangle whose ends are fully rounded, so that it becomes a pill.
    /// </summary>
    /// <param name="context">
    /// The context to draw on.
    /// </param>
    /// <param name="brush">
    /// The brush that fills the shape, or <see langword="null" /> to leave it unfilled.
    /// </param>
    /// <param name="pen">
    /// The pen that strokes the outline, or <see langword="null" /> to draw no outline.
    /// </param>
    /// <param name="rect">
    /// The bounds of the shape.
    /// </param>
    /// <param name="orientation">
    /// The direction of the long axis: <see cref="Orientation.Horizontal" /> rounds the ends with half the height and
    /// <see cref="Orientation.Vertical" /> with half the width.
    /// </param>
    /// <param name="boxShadows">
    /// The shadows drawn with the shape.
    /// </param>
    public static void DrawPilledRect(this DrawingContext context, IBrush? brush, IPen? pen, Rect rect,
                                      Orientation orientation = Orientation.Horizontal,
                                      BoxShadows boxShadows = default)
    {
        double pillRadius;
        if (orientation == Orientation.Horizontal)
        {
            pillRadius = rect.Height / 2;
        }
        else
        {
            pillRadius = rect.Width / 2;
        }

        context.DrawRectangle(brush, pen, rect, pillRadius, pillRadius, boxShadows);
    }

    /// <summary>
    /// Strokes an arc of the ellipse inscribed in a rectangle.
    /// </summary>
    /// <param name="context">
    /// The context to draw on.
    /// </param>
    /// <param name="pen">
    /// The pen to stroke with, or <see langword="null" /> to draw nothing.
    /// </param>
    /// <param name="rect">
    /// The bounds of the ellipse, as in <see cref="CommonShapeBuilder.BuildArc(Rect, double, double)" />.
    /// </param>
    /// <param name="startAngle">
    /// The angle of the first point in degrees, clockwise from the right; -90 is at the top.
    /// </param>
    /// <param name="sweepAngle">
    /// The length of the arc in degrees, clockwise when positive and counter-clockwise when negative.
    /// </param>
    public static void DrawArc(this DrawingContext context, IPen? pen,
                               Rect rect, double startAngle, double sweepAngle)
    {
        var geometry = CommonShapeBuilder.BuildArc(rect, startAngle, sweepAngle);
        context.DrawGeometry(null, pen, geometry);
    }
}