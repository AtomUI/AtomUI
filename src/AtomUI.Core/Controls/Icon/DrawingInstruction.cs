using System.Diagnostics;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Controls;

/// <summary>
/// One shape of the vector glyph of an <see cref="Icon" />: a geometry painted with an optional fill and an
/// optional stroke. The base class holds the paint settings; a derived class supplies the shape through
/// <see cref="BuildGeometry" />.
/// </summary>
/// <remarks>
    /// <para>
    /// Coordinates, sizes and stroke widths are in the units of the icon's <c>Icon.ViewBox</c>, which the
    /// icon scales to its rendered size.
    /// </para>
    /// <para>
    /// Generated glyphs share their instructions between icon instances, and <see cref="Draw" /> builds the
    /// geometry on its first call and caches it. So configure an instruction before it is drawn the first time.
    /// </para>
/// </remarks>
public abstract class DrawingInstruction
{
    /// <summary>
    /// Optional transform of the shape, in view box units. It is applied before the global geometry matrix that
    /// <see cref="Draw" /> receives and before the icon scales the glyph to its rendered size.
    /// <see langword="null" /> applies no transform of its own; the global geometry matrix still applies.
    /// </summary>
    public Matrix? Transform { get; set; }
    /// <summary>
    /// Opacity of the drawn shape, fill and stroke together, as a factor from 0 (invisible) to 1 (fully opaque).
    /// </summary>
    public double Opacity { get; set; } = 1.0;

    /// <summary>
    /// The icon brush that strokes the shape. <see langword="null" /> strokes nothing, even when
    /// <see cref="IsStrokeEnabled" /> is <see langword="true" />.
    /// </summary>
    public IconBrushType? StrokeBrush;
    /// <summary>
    /// The icon brush that fills the enclosed area of the shape. <see langword="null" /> fills nothing, so the
    /// shape is then visible through its stroke only.
    /// </summary>
    public IconBrushType? FillBrush;
    
    private Geometry? _geometry;

    /// <summary>
    /// Enables the stroke. <see langword="true" /> strokes the shape with <see cref="StrokeBrush" />;
    /// <see langword="false" /> draws no stroke.
    /// </summary>
    public bool IsStrokeEnabled { get; set; } = false;
    /// <summary>
    /// Takes the stroke width from the icon. <see langword="true" /> strokes with <see cref="Icon.StrokeWidth" />;
    /// <see langword="false" /> keeps the default width of a <see cref="Pen" />, 1 view box unit.
    /// </summary>
    public bool IsStrokeWidthCustomizable { get; set; } = false;
    /// <summary>
    /// Takes the line join, the shape of the corners of the stroke, from the icon.
    /// <see langword="true" /> uses <see cref="Icon.StrokeLineJoin" />; <see langword="false" /> keeps the
    /// <see cref="Pen" /> default, miter.
    /// </summary>
    public bool IsStrokeLinejoinCustomizable { get; set; } = false;
    /// <summary>
    /// Takes the line cap, the shape of the ends of the stroke, from the icon.
    /// <see langword="true" /> uses <see cref="Icon.StrokeLineCap" />; <see langword="false" /> keeps the
    /// <see cref="Pen" /> default, flat.
    /// </summary>
    public bool IsStrokeLinecapCustomizable { get; set; } = false;

    /// <summary>
    /// Draws the shape: fills it with the icon brush that <see cref="FillBrush" /> names and strokes it with the
    /// pen built by <see cref="BuildPen" />, both under <see cref="Opacity" />.
    /// </summary>
    /// <param name="drawingContext">
    /// The context to draw into. The icon pushes the scale from view box units to its rendered size before it
    /// calls this method.
    /// </param>
    /// <param name="globalGeometryMatrix">
    /// Matrix applied to the shape after its own <see cref="Transform" />. The icon passes the result of
    /// <c>Icon.CalculateGlobalGeometryMatrix</c>.
    /// </param>
    /// <param name="icon">
    /// The icon that owns this instruction. It supplies the brushes and the stroke settings.
    /// </param>
    /// <remarks>
    /// The geometry comes from <see cref="BuildGeometry" /> and is cached after the first call. While it draws,
    /// this method replaces the <see cref="Geometry.Transform" /> of the cached geometry and restores it afterwards.
    /// </remarks>
    public void Draw(DrawingContext drawingContext, in Matrix globalGeometryMatrix, Icon icon)
    {
        _geometry ??= BuildGeometry();
        IBrush? fillBrush = null;
        if (FillBrush != null)
        {
            fillBrush = icon.FindIconBrush(FillBrush.Value);
        }

        var        pen             = BuildPen(icon);
        Transform? originTransform = null;
        try
        {
            originTransform = _geometry.Transform;
            if (Transform != null)
            {
                _geometry.Transform = new MatrixTransform(Transform.Value * globalGeometryMatrix);
            }
            else
            {
                _geometry.Transform = new MatrixTransform(globalGeometryMatrix);
            }

            using var opacityState = drawingContext.PushOpacity(Opacity);
            drawingContext.DrawGeometry(fillBrush, pen, _geometry);
        }
        finally
        {
            _geometry.Transform = originTransform;
        }
    }
    
    /// <summary>
    /// Creates the geometry of the shape. <see cref="Draw" /> calls it once, on the first draw, and caches the
    /// result for every later draw.
    /// </summary>
    /// <returns>
    /// The geometry to draw, in view box units. It must not be <see langword="null" />, because <see cref="Draw" />
    /// uses it without a null check.
    /// </returns>
    protected abstract Geometry BuildGeometry();

    /// <summary>
    /// Builds the pen that strokes the shape, from <see cref="StrokeBrush" /> and the stroke settings that the
    /// customizable flags take from the icon.
    /// </summary>
    /// <param name="icon">
    /// The icon that supplies the brush and the stroke settings of the flags that are set.
    /// </param>
    /// <returns>
    /// A new pen on each call. It is <see langword="null" /> when <see cref="IsStrokeEnabled" /> is
    /// <see langword="false" /> or <see cref="StrokeBrush" /> is <see langword="null" />, and a
    /// <see langword="null" /> pen draws no stroke.
    /// </returns>
    protected IPen? BuildPen(Icon icon)
    {
        if (!IsStrokeEnabled || StrokeBrush == null)
        {
            return null;
        }

        var pen = new Pen(icon.FindIconBrush(StrokeBrush.Value));
        if (IsStrokeWidthCustomizable)
        {
            pen.Thickness = icon.StrokeWidth;
        }

        if (IsStrokeLinecapCustomizable)
        {
            pen.LineCap = icon.StrokeLineCap;
        }

        if (IsStrokeLinejoinCustomizable)
        {
            pen.LineJoin = icon.StrokeLineJoin;
        }
        return pen;
    }
}

/// <summary>
/// A rectangle as one shape of a glyph, with optional rounded corners.
/// </summary>
public class RectDrawingInstruction : DrawingInstruction
{
    /// <summary>
    /// Position and size of the rectangle, in view box units.
    /// </summary>
    public Rect Rect { get; set; }
    /// <summary>
    /// Horizontal radius of the corner arcs, in view box units. 0 draws square corners.
    /// </summary>
    public double RadiusX { get; set; } = 0;
    /// <summary>
    /// Vertical radius of the corner arcs, in view box units. 0 draws square corners.
    /// </summary>
    public double RadiusY { get; set; } = 0;
    
    /// <inheritdoc />
    protected override Geometry BuildGeometry()
    {
        return new RectangleGeometry(Rect, RadiusX, RadiusY);
    }
}

/// <summary>
/// A circle as one shape of a glyph.
/// </summary>
public class CircleDrawingInstruction : DrawingInstruction
{
    /// <summary>
    /// Radius of the circle, in view box units.
    /// </summary>
    public double Radius { get; set; }
    /// <summary>
    /// Center of the circle, in view box units.
    /// </summary>
    public Point Center { get; set; }
    
    /// <inheritdoc />
    protected override Geometry BuildGeometry()
    {
        return new EllipseGeometry()
        {
            Center  = Center,
            RadiusX = Radius,
            RadiusY = Radius
        };
    }
}

/// <summary>
/// An ellipse as one shape of a glyph.
/// </summary>
public class EllipseDrawingInstruction : DrawingInstruction
{
    /// <summary>
    /// Horizontal radius of the ellipse, in view box units.
    /// </summary>
    public double RadiusX { get; set; }
    /// <summary>
    /// Vertical radius of the ellipse, in view box units.
    /// </summary>
    public double RadiusY { get; set; }
    /// <summary>
    /// Center of the ellipse, in view box units.
    /// </summary>
    public Point Center { get; set; }

    /// <inheritdoc />
    protected override Geometry BuildGeometry()
    {
        return new EllipseGeometry()
        {
            Center  = Center,
            RadiusX = RadiusX,
            RadiusY = RadiusY
        };
    }
}

/// <summary>
/// A straight line as one shape of a glyph. A line encloses no area, so it is visible through its stroke only.
/// </summary>
public class LineDrawingInstruction : DrawingInstruction
{
    /// <summary>
    /// Start point of the line, in view box units.
    /// </summary>
    public Point StartPoint { get; set; }
    /// <summary>
    /// End point of the line, in view box units.
    /// </summary>
    public Point EndPoint { get; set; }

    /// <inheritdoc />
    protected override Geometry BuildGeometry()
    {
        return new LineGeometry()
        {
            StartPoint = StartPoint,
            EndPoint   = EndPoint
        };
    }
}

/// <summary>
/// A sequence of connected line segments as one shape of a glyph. It is not filled, so it is visible through its
/// stroke only.
/// </summary>
public class PolylineDrawingInstruction : DrawingInstruction
{
    /// <summary>
    /// The vertices of the polyline in drawing order, in view box units. The default is an empty list, which draws
    /// nothing.
    /// </summary>
    public IList<Point> Points { get; set; } = Array.Empty<Point>();

    /// <inheritdoc />
    protected override Geometry BuildGeometry()
    {
        return new PolylineGeometry()
        {
            Points   = Points,
            IsFilled = false,
        };
    }
}

/// <summary>
/// A polygon as one shape of a glyph. Unlike <see cref="PolylineDrawingInstruction" />, its enclosed area is
/// filled when <see cref="DrawingInstruction.FillBrush" /> is set.
/// </summary>
public class PolygonDrawingInstruction : DrawingInstruction
{
    /// <summary>
    /// The vertices of the polygon in drawing order, in view box units. The default is an empty list, which draws
    /// nothing.
    /// </summary>
    public IList<Point> Points { get; set; } = Array.Empty<Point>();

    /// <inheritdoc />
    protected override Geometry BuildGeometry()
    {
        return new PolylineGeometry()
        {
            Points   = Points,
            IsFilled = true,
        };
    }
}

/// <summary>
/// A glyph shape whose geometry is supplied as is, for outlines that the other instructions cannot express.
/// </summary>
public class PathDrawingInstruction : DrawingInstruction
{
    /// <summary>
    /// The geometry of the shape, in view box units. It must be set before the first draw, because a
    /// <see langword="null" /> value makes <see cref="DrawingInstruction.Draw" /> fail.
    /// </summary>
    /// <remarks>
    /// <see cref="DrawingInstruction.Draw" /> uses this instance directly, so a transform set on the geometry itself
    /// is not applied. Use <see cref="DrawingInstruction.Transform" /> instead.
    /// </remarks>
    public Geometry? Data { get; set; }

    /// <inheritdoc />
    protected override Geometry BuildGeometry()
    {
        Debug.Assert(Data != null);
        return Data;
    }
}