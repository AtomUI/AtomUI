using Avalonia;

namespace AtomUI.Icons;

/// <summary>
/// Base class of the graphic elements that <see cref="SvgParser" /> collects from an SVG document.
/// </summary>
/// <remarks>
/// The properties declared here come from the element itself or, when the element does not set the
/// attribute, from the enclosing <c>g</c> element. A string property holds the attribute text as written
/// in the source, or <see langword="null" /> when the attribute is absent from both. A numeric attribute
/// that is missing or is not a number contributes no value, so the property keeps the inherited value or
/// its default. Coordinates and sizes are in the coordinate system of the SVG source.
/// </remarks>
public class SvgGraphicElement
{
    /// <summary>
    /// Transformation of the element, from the <c>transform</c> attribute. The Ant Design generator applies
    /// it to each <see cref="PathElement" /> that has one.
    /// </summary>
    public string? Transform { get; set; }
    /// <summary>
    /// Fill color of the element, from the <c>fill</c> attribute. For a two-tone icon, the Ant Design
    /// generator emits a <see cref="PathElement" /> whose value is one of the secondary colors of the
    /// two-tone template with the fill brush, and every other path with the stroke brush.
    /// </summary>
    public string? FillColor { get; set; }
    /// <summary>
    /// Stroke color of the element, from the <c>stroke</c> attribute.
    /// </summary>
    public string? StrokeColor { get; set; }
    /// <summary>
    /// Stroke width of the element, from the <c>stroke-width</c> attribute.
    /// </summary>
    public string? StrokeWidth { get; set; }
    /// <summary>
    /// Shape of the ends of the element's open strokes, from the <c>stroke-linecap</c> attribute.
    /// </summary>
    public string? StrokeLineCap { get; set; }
    /// <summary>
    /// Shape of the corners where the element's strokes join, from the <c>stroke-linejoin</c> attribute.
    /// </summary>
    public string? StrokeLineJoin { get; set; }
    /// <summary>
    /// Opacity of the element, where 1 is fully opaque. The <c>fill-opacity</c> attribute of the element
    /// wins over its <c>opacity</c> attribute, which wins over the value inherited from the enclosing
    /// <c>g</c> element; the value is 1 when none of them is a number.
    /// </summary>
    public double Opacity { get; set; } = 1.0;
}

/// <summary>
/// A <c>path</c> element of an SVG document, defined by its <see cref="Data" />.
/// </summary>
public class PathElement : SvgGraphicElement
{
    /// <summary>
    /// The path data from the <c>d</c> attribute, read from the element itself; <see langword="null" /> when
    /// the element has no <c>d</c> attribute.
    /// </summary>
    public string? Data { get; set; }
}

/// <summary>
/// A <c>rect</c> element of an SVG document.
/// </summary>
public class RectElement : SvgGraphicElement
{
    /// <summary>
    /// Horizontal position of the left edge of the rectangle, from the <c>x</c> attribute.
    /// </summary>
    public double X { get; set; }
    /// <summary>
    /// Vertical position of the top edge of the rectangle, from the <c>y</c> attribute.
    /// </summary>
    public double Y { get; set; }
    /// <summary>
    /// Width of the rectangle, from the <c>width</c> attribute.
    /// </summary>
    public double Width { get; set; }
    /// <summary>
    /// Height of the rectangle, from the <c>height</c> attribute.
    /// </summary>
    public double Height { get; set; }
    /// <summary>
    /// Horizontal corner radius from the <c>rx</c> attribute; when the element sets only <c>ry</c>, both
    /// radii take that value.
    /// </summary>
    public double RadiusX { get; set; }
    /// <summary>
    /// Vertical corner radius from the <c>ry</c> attribute; when the element sets only <c>rx</c>, both
    /// radii take that value.
    /// </summary>
    public double RadiusY { get; set; }
}

/// <summary>
/// A <c>circle</c> element of an SVG document.
/// </summary>
public class CircleElement : SvgGraphicElement
{
    /// <summary>
    /// The radius of the circle, from the <c>r</c> attribute.
    /// </summary>
    public double Radius { get; set; }
    /// <summary>
    /// The horizontal position of the center, from the <c>cx</c> attribute.
    /// </summary>
    public double CenterX { get; set; }
    /// <summary>
    /// The vertical position of the center, from the <c>cy</c> attribute.
    /// </summary>
    public double CenterY { get; set; }
}

/// <summary>
/// An <c>ellipse</c> element of an SVG document.
/// </summary>
public class EllipseElement : SvgGraphicElement
{
    /// <summary>
    /// Horizontal radius from the <c>rx</c> attribute; when the element sets only <c>ry</c>, both radii
    /// take that value.
    /// </summary>
    public double RadiusX { get; set; }
    /// <summary>
    /// Vertical radius from the <c>ry</c> attribute; when the element sets only <c>rx</c>, both radii
    /// take that value.
    /// </summary>
    public double RadiusY { get; set; }
    /// <summary>
    /// The horizontal position of the center, from the <c>cx</c> attribute.
    /// </summary>
    public double CenterX { get; set; }
    /// <summary>
    /// The vertical position of the center, from the <c>cy</c> attribute.
    /// </summary>
    public double CenterY { get; set; }
}

/// <summary>
/// A <c>line</c> element of an SVG document: a segment from (<see cref="X1" />, <see cref="Y1" />) to
/// (<see cref="X2" />, <see cref="Y2" />).
/// </summary>
public class LineElement : SvgGraphicElement
{
    /// <summary>
    /// The horizontal position of the start point, from the <c>x1</c> attribute.
    /// </summary>
    public double X1 { get; set; }
    /// <summary>
    /// The vertical position of the start point, from the <c>y1</c> attribute.
    /// </summary>
    public double Y1 { get; set; }
    /// <summary>
    /// The horizontal position of the end point, from the <c>x2</c> attribute.
    /// </summary>
    public double X2 { get; set; }
    /// <summary>
    /// The vertical position of the end point, from the <c>y2</c> attribute.
    /// </summary>
    public double Y2 { get; set; }
}

/// <summary>
/// A <c>polyline</c> element of an SVG document: an open chain of line segments through its
/// <see cref="Points" />.
/// </summary>
public class PolylineElement : SvgGraphicElement
{
    /// <summary>
    /// The vertices from the <c>points</c> attribute in the order written, each as <c>x,y</c>; an empty
    /// list when the attribute is missing.
    /// </summary>
    public List<Point> Points { get; set; } = new();
}

/// <summary>
/// A <c>polygon</c> element of an SVG document: a closed shape through its <see cref="Points" />.
/// </summary>
public class PolygonElement : SvgGraphicElement
{
    /// <summary>
    /// The vertices from the <c>points</c> attribute in the order written, each as <c>x,y</c>; an empty
    /// list when the attribute is missing.
    /// </summary>
    public List<Point> Points { get; set; } = new();
}

/// <summary>
/// The <c>viewBox</c> of an SVG document: the rectangle of the SVG coordinate system that the icon is
/// drawn from.
/// </summary>
public class ViewBox
{
    /// <summary>
    /// Left edge of the view box, read as an integer from the first number of the <c>viewBox</c> attribute.
    /// </summary>
    public int X { get; set; }
    /// <summary>
    /// Top edge of the view box, read as an integer from the second number of the <c>viewBox</c> attribute.
    /// </summary>
    public int Y { get; set; }
    /// <summary>
    /// Width of the view box, read as an integer from the third number of the <c>viewBox</c> attribute.
    /// </summary>
    public int Width { get; set; }
    /// <summary>
    /// Height of the view box, read as an integer from the fourth number of the <c>viewBox</c> attribute.
    /// </summary>
    public int Height { get; set; }
}

/// <summary>
/// The attributes of a <c>g</c> element, kept while <see cref="SvgParser" /> reads the elements that the
/// group encloses; each of those elements starts with a copy of them as its inherited values.
/// </summary>
public class GroupElement : SvgGraphicElement
{
}

/// <summary>
/// The result of <see cref="SvgParser.Parse(string)" />: the graphic elements of one SVG document and
/// its view box.
/// </summary>
public record SvgParsedInfo
{
    /// <summary>
    /// The graphic elements in document order. The Ant Design generator emits only the
    /// <see cref="PathElement" /> items.
    /// </summary>
    public List<SvgGraphicElement> GraphicElements { get; set; } = new();
    /// <summary>
    /// The <c>viewBox</c> of the document. A document without one gets the view box of the document parsed
    /// before it on the same <see cref="SvgParser" /> instance; all values are 0 when there was none.
    /// </summary>
    public ViewBox ViewBox { get; set; } = new ();
}