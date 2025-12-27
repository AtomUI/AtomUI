using System.Diagnostics;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Controls;

public abstract class DrawingInstruction
{
    public Matrix? Transform { get; set; }
    public double Opacity { get; set; } = 1.0;

    public IconBrushType? StrokeBrush;
    public IconBrushType? FillBrush;
    
    private Geometry? _geometry;

    public bool IsStrokeEnabled { get; set; } = false;
    public bool IsStrokeWidthCustomizable { get; set; } = false;
    public bool IsStrokeLinejoinCustomizable { get; set; } = false;
    public bool IsStrokeLinecapCustomizable { get; set; } = false;

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
    
    protected abstract Geometry BuildGeometry();

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

public class RectDrawingInstruction : DrawingInstruction
{
    public Rect Rect { get; set; }
    public double RadiusX { get; set; } = 0;
    public double RadiusY { get; set; } = 0;
    
    protected override Geometry BuildGeometry()
    {
        return new RectangleGeometry(Rect, RadiusX, RadiusY);
    }
}

public class CircleDrawingInstruction : DrawingInstruction
{
    public double Radius { get; set; }
    public Point Center { get; set; }
    
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

public class EllipseDrawingInstruction : DrawingInstruction
{
    public double RadiusX { get; set; }
    public double RadiusY { get; set; }
    public Point Center { get; set; }

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

public class LineDrawingInstruction : DrawingInstruction
{
    public Point StartPoint { get; set; }
    public Point EndPoint { get; set; }

    protected override Geometry BuildGeometry()
    {
        return new LineGeometry()
        {
            StartPoint = StartPoint,
            EndPoint   = EndPoint
        };
    }
}

public class PolylineDrawingInstruction : DrawingInstruction
{
    public IList<Point> Points { get; set; } = Array.Empty<Point>();

    protected override Geometry BuildGeometry()
    {
        return new PolylineGeometry()
        {
            Points   = Points,
            IsFilled = false,
        };
    }
}

public class PolygonDrawingInstruction : DrawingInstruction
{
    public IList<Point> Points { get; set; } = Array.Empty<Point>();

    protected override Geometry BuildGeometry()
    {
        return new PolylineGeometry()
        {
            Points   = Points,
            IsFilled = true,
        };
    }
}

public class PathDrawingInstruction : DrawingInstruction
{
    public Geometry? Data { get; set; }

    protected override Geometry BuildGeometry()
    {
        Debug.Assert(Data != null);
        return Data;
    }
}