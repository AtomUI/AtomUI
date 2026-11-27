using System.Xml;
using Avalonia;

namespace AtomUI.Icons;

/// <summary>
/// Extracts the <c>viewBox</c> and the drawing elements of an SVG document into an
/// <see cref="SvgParsedInfo" />, which icon package generators turn into icon classes.
/// </summary>
public class SvgParser
{
    private const string SvgElementName = "svg";
    private const string PathElementName = "path";
    private const string RectElementName = "rect";
    private const string CircleElementName = "circle";
    private const string EllipseElementName = "ellipse";
    private const string LineElementName = "line";
    private const string PolygonElementName = "polygon";
    private const string PolylineElementName = "polyline";
    private const string ClipPathElementName = "clipPath";
    private const string GroupElementName = "g";
    private const string ViewBoxAttrName = "viewBox";
    
    // Attribute names of the SVG graphic elements.
    private const string XAttrName = "x";
    private const string YAttrName = "y";
    private const string WidthAttrName = "width";
    private const string HeightAttrName = "height";
    private const string RadiusAttrName = "r";
    private const string RXAttrName = "rx";
    private const string RYAttrName = "ry";
    private const string CXAttrName = "cx";
    private const string CYAttrName = "cy";
    private const string PointsAttrName = "points";
    private const string X1AttrName = "x1";
    private const string Y1AttrName = "y1";
    private const string X2AttrName = "x2";
    private const string Y2AttrName = "y2";
    private const string OpacityAttrName = "opacity";
    private const string FillOpacityAttrName = "fill-opacity";
    private const string FillAttrName = "fill";
    private const string StrokeAttrName = "stroke";
    private const string StrokeWidthAttrName = "stroke-width";
    private const string StrokeLineCapAttrName = "stroke-linecap";
    private const string StrokeLineJoinAttrName = "stroke-linejoin";
    private const string DataAttrName = "d";
    private const string TransformAttrName = "transform";
    
    private List<SvgGraphicElement>? _graphicElements;
    private ViewBox _viewBox = new ();
    private bool _parseFinished;
    // Only one level of g elements is supported for now.
    
    private Stack<GroupElement>? _groupInfoStack;

    // Parsing context: the names of the elements that are currently open.
    private Stack<string>? _currentElementNames;

    /// <summary>
    /// Parses one SVG document and returns its graphic elements and its <c>viewBox</c>.
    /// </summary>
    /// <param name="svg">
    /// The SVG source text.
    /// </param>
    /// <returns>
    /// The graphic elements in document order and the <c>viewBox</c>.
    /// </returns>
    /// <exception cref="XmlException">
    /// The source is not well-formed XML.
    /// </exception>
    /// <exception cref="FormatException">
    /// A <c>points</c> entry holds a coordinate that is not two numbers, or a <c>viewBox</c> number is not an
    /// integer.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Supported elements are <c>path</c>, <c>g</c>, <c>rect</c>, <c>circle</c>, <c>ellipse</c>, <c>line</c>,
    /// <c>polyline</c> and <c>polygon</c>. Other elements are not collected, and a <c>clipPath</c> element
    /// is skipped with its content. An element inherits the paint attributes of the <c>g</c> element that
    /// encloses it, and its own attributes override the inherited values.
    /// </para>
    /// <para>
    /// The <c>viewBox</c> values are read as integers. The parser keeps the <c>viewBox</c> across calls: a
    /// document without one yields the <c>viewBox</c> of the document parsed before it on the same instance,
    /// or the default value for the first document.
    /// </para>
    /// </remarks>
    public SvgParsedInfo Parse(string svg)
    {
        try
        {
            _graphicElements     = new List<SvgGraphicElement>();
            _parseFinished       = false;
            _currentElementNames = new Stack<string>();
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Ignore
            };
            using (var xmlReader = XmlReader.Create(new StringReader(svg), settings))
            {
                while (xmlReader.Read() && !_parseFinished)
                {
                    switch (xmlReader.NodeType)
                    {
                        case XmlNodeType.Element:
                            HandleStartElement(xmlReader);
                            break;
                        case XmlNodeType.EndElement:
                            HandleEndElement(xmlReader.Name);
                            break;
                    }
                }
            }

            return new SvgParsedInfo
            {
                GraphicElements = _graphicElements,
                ViewBox         = _viewBox
            };
        }
        finally
        {
            _currentElementNames = null;
        }
    }

    private bool HandleStartElement(XmlReader reader)
    {
        var name = reader.Name;
        _currentElementNames!.Push(name);
        if (name == SvgElementName)
        {
            return HandleStartSvgElement(reader);
        }

        if (name == PathElementName)
        {
            return HandleStartPathElement(reader);
        }

        if (name == GroupElementName)
        {
            return HandleStartGroupElement(reader);
        }

        if (name == RectElementName)
        {
            return HandleStartRectElement(reader);
        }

        if (name == CircleElementName)
        {
            return HandleStartCircleElement(reader);
        }

        if (name == EllipseElementName)
        {
            return HandleStartEllipseElement(reader);
        }

        if (name == LineElementName)
        {
            return HandleLineElement(reader);
        }

        if (name == PolylineElementName)
        {
            return HandlePolylineElement(reader);
        }

        if (name == PolygonElementName)
        {
            return HandlePolygonElement(reader);
        }

        if (name == ClipPathElementName)
        {
            reader.Skip();
            return true;
        }
        
        return false;
    }

    private bool HandleEndElement(string name)
    {
        _currentElementNames!.Pop();
        if (name == GroupElementName)
        {
            HandleEndGroupElement();
        }
        return name == SvgElementName;
    }

    private bool HandleStartSvgElement(XmlReader reader)
    {
        var viewBox = reader.GetAttribute(ViewBoxAttrName);
        if (viewBox is not null)
        {
            var parts = viewBox.Split([' '], StringSplitOptions.RemoveEmptyEntries);
            // No error handling for a malformed viewBox yet.
            _viewBox.X      = int.Parse(parts[0]);
            _viewBox.Y      = int.Parse(parts[1]);
            _viewBox.Width  = int.Parse(parts[2]);
            _viewBox.Height = int.Parse(parts[3]);
        }

        return false;
    }

    private bool HandleStartPathElement(XmlReader reader)
    {
        var data        = reader.GetAttribute(DataAttrName);
        var pathElement = new PathElement();
        pathElement.Data = data;
        ParseElementCommonAttributes(pathElement, reader);
        _graphicElements?.Add(pathElement);
        return false;
    }
    
    private bool HandleStartGroupElement(XmlReader reader)
    {
        _groupInfoStack ??= new Stack<GroupElement>();
        var groupInfo = new GroupElement();
        ParseElementCommonAttributes(groupInfo, reader);
        var transform = reader.GetAttribute(TransformAttrName);
        groupInfo.Transform = transform;
        _groupInfoStack.Push(groupInfo);
        return false;
    }

    private bool HandleStartRectElement(XmlReader reader)
    {
        var rectElement = new RectElement();
        ParseElementCommonAttributes(rectElement, reader);
        var widthStr  = reader.GetAttribute(WidthAttrName);
        var heightStr = reader.GetAttribute(HeightAttrName);
        if (!string.IsNullOrEmpty(widthStr) && double.TryParse(widthStr, out var width))
        {
            rectElement.Width = width;
        }

        if (!string.IsNullOrEmpty(heightStr) && double.TryParse(heightStr, out var height))
        {
            rectElement.Height = height;
        }
        var xStr =  reader.GetAttribute(XAttrName);
        var yStr =  reader.GetAttribute(YAttrName);
        if (!string.IsNullOrEmpty(xStr) && double.TryParse(xStr, out var x))
        {
            rectElement.X = x;
        }

        if (!string.IsNullOrEmpty(yStr) && double.TryParse(yStr, out var y))
        {
            rectElement.Y = y;
        }
        
        var rxStr = reader.GetAttribute(RXAttrName);
        var ryStr = reader.GetAttribute(RYAttrName);

        double? rx = null;
        double? ry = null;

        {
            if (!string.IsNullOrEmpty(rxStr) && double.TryParse(rxStr, out var dvalue))
            {
                rx = dvalue;
            }
        }

        {
            if (!string.IsNullOrEmpty(ryStr) && double.TryParse(ryStr, out var dvalue))
            {
                ry = dvalue;
            }
        }

        if (rx == null && ry != null)
        {
            rx = ry;
        }
        else if (rx != null && ry == null)
        {
            ry = rx;
        }
        rectElement.RadiusX = rx ?? 0.0;
        rectElement.RadiusY = ry ?? 0.0;

        _graphicElements?.Add(rectElement);
        return false;
    }

    private bool HandleStartCircleElement(XmlReader reader)
    {
        var circleElement = new CircleElement();
        ParseElementCommonAttributes(circleElement, reader);

        var radiusStr = reader.GetAttribute(RadiusAttrName);
        if (!string.IsNullOrEmpty(radiusStr) && double.TryParse(radiusStr, out var radius))
        {
            circleElement.Radius = radius;
        }
        
        var cxStr = reader.GetAttribute(CXAttrName);
        var cyStr = reader.GetAttribute(CYAttrName);
        if (!string.IsNullOrEmpty(cxStr) && double.TryParse(cxStr, out var cx))
        {
            circleElement.CenterX = cx;
        }

        if (!string.IsNullOrEmpty(cyStr) && double.TryParse(cyStr, out var cy))
        {
            circleElement.CenterY = cy;
        }
        
        _graphicElements?.Add(circleElement);
        return false;
    }
    
    private bool HandleStartEllipseElement(XmlReader reader)
    {
        var ellipseElement = new EllipseElement();
        ParseElementCommonAttributes(ellipseElement, reader);

        double? rx = null;
        double? ry = null;
        
        var rxStr = reader.GetAttribute(RXAttrName);
        {
            if (!string.IsNullOrEmpty(rxStr) && double.TryParse(rxStr, out var dvalue))
            {
                rx = dvalue;
            }
        }
        
        var ryStr = reader.GetAttribute(RYAttrName);
        {
            if (!string.IsNullOrEmpty(ryStr) && double.TryParse(ryStr, out var dvalue))
            {
                ry = dvalue;
            }
        }
        
        if (rx == null && ry != null)
        {
            rx = ry;
        }
        else if (rx != null && ry == null)
        {
            ry = rx;
        }
        ellipseElement.RadiusX = rx ?? 0.0;
        ellipseElement.RadiusY = ry ?? 0.0;
        
        var cxStr = reader.GetAttribute(CXAttrName);
        var cyStr = reader.GetAttribute(CYAttrName);
        if (!string.IsNullOrEmpty(cxStr) && double.TryParse(cxStr, out var cx))
        {
            ellipseElement.CenterX = cx;
        }

        if (!string.IsNullOrEmpty(cyStr) && double.TryParse(cyStr, out var cy))
        {
            ellipseElement.CenterY = cy;
        }
        
        _graphicElements?.Add(ellipseElement);
        return false;
    }

    private bool HandleLineElement(XmlReader reader)
    {
        var lineElement = new LineElement();
        ParseElementCommonAttributes(lineElement, reader);
        var x1Str = reader.GetAttribute(X1AttrName);
        if (!string.IsNullOrEmpty(x1Str) && double.TryParse(x1Str, out var x1))
        {
            lineElement.X1 = x1;
        }
        var y1Str = reader.GetAttribute(Y1AttrName);
        if (!string.IsNullOrEmpty(y1Str) && double.TryParse(y1Str, out var y1))
        {
            lineElement.Y1 = y1;
        }
        
        var x2Str = reader.GetAttribute(X2AttrName);
        if (!string.IsNullOrEmpty(x2Str) && double.TryParse(x2Str, out var x2))
        {
            lineElement.X2 = x2;
        }
        var y2Str = reader.GetAttribute(Y2AttrName);
        if (!string.IsNullOrEmpty(y2Str) && double.TryParse(y2Str, out var y2))
        {
            lineElement.Y2 = y2;
        }
        _graphicElements?.Add(lineElement);
        return false;
    }
    
    private bool HandlePolylineElement(XmlReader reader)
    {
        var polylineElement = new PolylineElement();
        ParseElementCommonAttributes(polylineElement, reader);
        
        var pointsStr = reader.GetAttribute(PointsAttrName);

        if (!string.IsNullOrEmpty(pointsStr))
        {
            polylineElement.Points =  pointsStr.Split(' ')
                                               .Select(coord => 
                                               {
                                                   var parts = coord.Split(',');
                                                   if (parts.Length == 2 && 
                                                       double.TryParse(parts[0], out var x) && 
                                                       double.TryParse(parts[1], out var y))
                                                   {
                                                       return new Point(x, y);
                                                   }
                                                   throw new FormatException($"Invalid coordinate format: {coord}");
                                               })
                                               .ToList();
        }
        _graphicElements?.Add(polylineElement);
        return false;
    }
    
    private bool HandlePolygonElement(XmlReader reader)
    {
        var polygonElement = new PolygonElement();
        ParseElementCommonAttributes(polygonElement, reader);
        
        var pointsStr = reader.GetAttribute(PointsAttrName);

        if (!string.IsNullOrEmpty(pointsStr))
        {
            polygonElement.Points =  pointsStr.Split(' ')
                                              .Select(coord => 
                                              {
                                                  var parts = coord.Split(',');
                                                  if (parts.Length == 2 && 
                                                      double.TryParse(parts[0], out var x) && 
                                                      double.TryParse(parts[1], out var y))
                                                  {
                                                      return new Point(x, y);
                                                  }
                                                  throw new FormatException($"Invalid coordinate format: {coord}");
                                              })
                                              .ToList();
        }
        _graphicElements?.Add(polygonElement);
        return false;
    }

    private void ParseElementCommonAttributes(SvgGraphicElement element, XmlReader reader)
    {
        if (_groupInfoStack?.Count > 0)
        {
            var groupInfo = _groupInfoStack.Peek();
            element.Transform      = groupInfo.Transform;
            element.FillColor      = groupInfo.FillColor;
            element.StrokeColor    = groupInfo.StrokeColor;
            element.StrokeLineCap  = groupInfo.StrokeLineCap;
            element.StrokeLineJoin = groupInfo.StrokeLineJoin;
            element.StrokeWidth    = groupInfo.StrokeWidth;
            element.Opacity    = groupInfo.Opacity;
        }
        // For now, an attribute on the element itself takes precedence over the value inherited from the group.
        var transform = reader.GetAttribute(TransformAttrName);
        if (!string.IsNullOrEmpty(transform))
        {
            element.Transform = transform;
        }
        
        var fillColor   = reader.GetAttribute(FillAttrName);
        if (!string.IsNullOrEmpty(fillColor))
        {
            element.FillColor = fillColor;
        }
        
        var strokeColor   = reader.GetAttribute(StrokeAttrName);
        if (!string.IsNullOrEmpty(strokeColor))
        {
            element.StrokeColor = strokeColor;
        }
        
        var strokeWidth   = reader.GetAttribute(StrokeWidthAttrName);
        if (!string.IsNullOrEmpty(strokeWidth))
        {
            element.StrokeWidth = strokeWidth;
        }
        
        var strokeLineCap   = reader.GetAttribute(StrokeLineCapAttrName);
        if (!string.IsNullOrEmpty(strokeLineCap))
        {
            element.StrokeLineCap = strokeLineCap;
        }
        
        var strokeLineJoin   = reader.GetAttribute(StrokeLineJoinAttrName);
        if (!string.IsNullOrEmpty(strokeLineJoin))
        {
            element.StrokeLineJoin = strokeLineJoin;
        }
        
        var opacityStr = reader.GetAttribute(OpacityAttrName);
        if (!string.IsNullOrEmpty(opacityStr) && double.TryParse(opacityStr, out var opacity))
        {
            element.Opacity = opacity;
        }
        
        var fillOpacityStr = reader.GetAttribute(FillOpacityAttrName);
        if (!string.IsNullOrEmpty(fillOpacityStr) && double.TryParse(fillOpacityStr, out var fillOpacity))
        {
            element.Opacity = fillOpacity;
        }
    }
    
    private void HandleEndGroupElement()
    {
        _groupInfoStack?.Pop();
    }
}