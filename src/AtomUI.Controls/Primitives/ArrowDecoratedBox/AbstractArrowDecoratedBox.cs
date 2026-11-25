using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using AtomUI.Media;
using Avalonia.Media;

namespace AtomUI.Controls;

public enum ArrowPosition
{
    /// <summary>
    /// Preferred location is below the target element.
    /// </summary>
    Bottom,

    /// <summary>
    /// Preferred location is to the right of the target element.
    /// </summary>
    Right,

    /// <summary>
    /// Preferred location is to the left of the target element.
    /// </summary>
    Left,

    /// <summary>
    /// Preferred location is above the target element.
    /// </summary>
    Top,

    /// <summary>
    /// Preferred location is above the target element, with the left edge of the popup
    /// aligned with the left edge of the target element.
    /// </summary>
    TopEdgeAlignedLeft,

    /// <summary>
    /// Preferred location is above the target element, with the right edge of popup aligned with right edge of the target
    /// element.
    /// </summary>
    TopEdgeAlignedRight,

    /// <summary>
    /// Preferred location is below the target element, with the left edge of popup aligned with left edge of the target
    /// element.
    /// </summary>
    BottomEdgeAlignedLeft,

    /// <summary>
    /// Preferred location is below the target element, with the right edge of popup aligned with right edge of the target
    /// element.
    /// </summary>
    BottomEdgeAlignedRight,

    /// <summary>
    /// Preferred location is to the left of the target element, with the top edge of popup aligned with top edge of the
    /// target element.
    /// </summary>
    LeftEdgeAlignedTop,

    /// <summary>
    /// Preferred location is to the left of the target element, with the bottom edge of popup aligned with bottom edge of
    /// the target element.
    /// </summary>
    LeftEdgeAlignedBottom,

    /// <summary>
    /// Preferred location is to the right of the target element, with the top edge of popup aligned with top edge of the
    /// target element.
    /// </summary>
    RightEdgeAlignedTop,

    /// <summary>
    /// Preferred location is to the right of the target element, with the bottom edge of popup aligned with bottom edge of
    /// the target element.
    /// </summary>
    RightEdgeAlignedBottom
}

[TemplatePart("PART_ContentDecorator", typeof(Border))]
[TemplatePart("PART_ArrowIndicatorLayout", typeof(Control))]
[TemplatePart("PART_ArrowIndicator", typeof(ArrowIndicator))]
[PseudoClasses(BorderedPseudoClass)]
public abstract class AbstractArrowDecoratedBox : ContentControl,
                                                  IArrowAwareShadowMaskInfoProvider,
                                                  IMotionAwareControl
{
    public const string ArrowDecoratorPart = "PART_ArrowDecorator";

    /// <summary>
    /// Set while <see cref="TemplatedControl.BorderThickness"/> is not zero. The box then draws its background and
    /// border itself as one outline around the content box and the arrow (no line across the arrow's base); the
    /// built-in theme hands the template's container border and arrow indicator over to that outline.
    /// </summary>
    public const string BorderedPseudoClass = ":bordered";

    protected AbstractArrowDecoratedBox()
    {
        PseudoClasses.Set(BorderedPseudoClass, BorderThickness != default);
    }

    #region 公共属性定义

    public static readonly StyledProperty<bool> IsArrowVisibleProperty =
        AvaloniaProperty.Register<AbstractArrowDecoratedBox, bool>(nameof(IsArrowVisible), true);

    public static readonly StyledProperty<ArrowPosition> ArrowPositionProperty =
        AvaloniaProperty.Register<AbstractArrowDecoratedBox, ArrowPosition>(
            nameof(ArrowPosition));

    public static readonly StyledProperty<double?> ArrowAnchorOffsetProperty =
        AvaloniaProperty.Register<AbstractArrowDecoratedBox, double?>(nameof(ArrowAnchorOffset));

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractArrowDecoratedBox>();

    /// <summary>
    /// 是否显示指示箭头
    /// </summary>
    public bool IsArrowVisible
    {
        get => GetValue(IsArrowVisibleProperty);
        set => SetValue(IsArrowVisibleProperty, value);
    }

    /// <summary>
    /// 箭头渲染的位置
    /// </summary>
    public ArrowPosition ArrowPosition
    {
        get => GetValue(ArrowPositionProperty);
        set => SetValue(ArrowPositionProperty, value);
    }

    /// <summary>
    /// Arrow center in device-independent units from the left or top of its edge.
    /// A null value uses the alignment specified by <see cref="ArrowPosition"/>.
    /// </summary>
    public double? ArrowAnchorOffset
    {
        get => GetValue(ArrowAnchorOffsetProperty);
        set => SetValue(ArrowAnchorOffsetProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<double> ArrowSizeProperty =
        AvaloniaProperty.Register<AbstractArrowDecoratedBox, double>(nameof(ArrowSize));

    /// <summary>The width of the arrow indicator's outline; a host such as the tooltip may draw it without one.</summary>
    internal static readonly StyledProperty<double> ArrowStrokeThicknessProperty =
        AvaloniaProperty.Register<AbstractArrowDecoratedBox, double>(nameof(ArrowStrokeThickness), 1.0);

    /// <summary>Radius of the arrow's rounded tip (see <see cref="AtomUI.Controls.ArrowIndicator.TipRadius"/>).</summary>
    internal static readonly StyledProperty<double> ArrowTipRadiusProperty =
        AvaloniaProperty.Register<AbstractArrowDecoratedBox, double>(nameof(ArrowTipRadius));

    /// <summary>Radius of the arcs joining the arrow to the box edge (see <see cref="AtomUI.Controls.ArrowIndicator.BaseRadius"/>).</summary>
    internal static readonly StyledProperty<double> ArrowBaseRadiusProperty =
        AvaloniaProperty.Register<AbstractArrowDecoratedBox, double>(nameof(ArrowBaseRadius));

    internal static readonly StyledProperty<Direction> ArrowDirectionProperty = 
        AvaloniaProperty.Register<AbstractArrowDecoratedBox, Direction>(nameof(ArrowDirection));
    
    internal static readonly StyledProperty<double> ArrowOpacityProperty =
        AvaloniaProperty.Register<AbstractArrowDecoratedBox, double>(nameof(ArrowOpacity), 1.0);
    
    internal static readonly DirectProperty<AbstractArrowDecoratedBox, Rect> ArrowIndicatorBoundsProperty =
        AvaloniaProperty.RegisterDirect<AbstractArrowDecoratedBox, Rect>(
            nameof(ArrowIndicatorBounds),
            o => o.ArrowIndicatorBounds,
            (o, v) => o.ArrowIndicatorBounds = v);
    
    internal static readonly DirectProperty<AbstractArrowDecoratedBox, Rect> ArrowIndicatorLayoutBoundsProperty =
        AvaloniaProperty.RegisterDirect<AbstractArrowDecoratedBox, Rect>(
            nameof(ArrowIndicatorLayoutBounds),
            o => o.ArrowIndicatorLayoutBounds,
            (o, v) => o.ArrowIndicatorLayoutBounds = v);
    
    internal double ArrowSize
    {
        get => GetValue(ArrowSizeProperty);
        set => SetValue(ArrowSizeProperty, value);
    }

    internal double ArrowStrokeThickness
    {
        get => GetValue(ArrowStrokeThicknessProperty);
        set => SetValue(ArrowStrokeThicknessProperty, value);
    }

    internal double ArrowTipRadius
    {
        get => GetValue(ArrowTipRadiusProperty);
        set => SetValue(ArrowTipRadiusProperty, value);
    }

    internal double ArrowBaseRadius
    {
        get => GetValue(ArrowBaseRadiusProperty);
        set => SetValue(ArrowBaseRadiusProperty, value);
    }

    internal Direction ArrowDirection
    {
        get => GetValue(ArrowDirectionProperty);
        set => SetValue(ArrowDirectionProperty, value);
    }
    
    internal double ArrowOpacity
    {
        get => GetValue(ArrowOpacityProperty);
        set => SetValue(ArrowOpacityProperty, value);
    }
    
    private Rect _arrowIndicatorBounds;

    internal Rect ArrowIndicatorBounds
    {
        get => _arrowIndicatorBounds;
        set => SetAndRaise(ArrowIndicatorBoundsProperty, ref _arrowIndicatorBounds, value);
    }
    
    private Rect _arrowIndicatorLayoutBounds;

    internal Rect ArrowIndicatorLayoutBounds
    {
        get => _arrowIndicatorLayoutBounds;
        set => SetAndRaise(ArrowIndicatorLayoutBoundsProperty, ref _arrowIndicatorLayoutBounds, value);
    }

    #endregion

    // 指针最顶点位置
    // 相对坐标
    internal (double, double) ArrowVertexPoint => GetArrowVertexPoint();
    private Border? _contentDecorator;

    internal Border? ContentDecorator => _contentDecorator;
    private protected Control? ArrowIndicatorLayout;
    // ArrowIndicatorLayoutBounds 相对模板内的箭头定位面板，不是本控件；需要真实位置时按此部件自身做坐标变换
    internal Control? ArrowIndicatorLayoutPart => ArrowIndicatorLayout;
    private protected ArrowIndicator? ArrowIndicator;
    private protected bool ArrowPlacementFlipped;

    private StreamGeometry? _outlineGeometry;
    private OutlineShape _outlineShape;
    private IPen? _outlinePen;

    static AbstractArrowDecoratedBox()
    {
        AffectsMeasure<AbstractArrowDecoratedBox>(IsArrowVisibleProperty, ArrowDirectionProperty);
        AffectsArrange<AbstractArrowDecoratedBox>(ArrowPositionProperty, ArrowAnchorOffsetProperty, CornerRadiusProperty);
        AffectsRender<AbstractArrowDecoratedBox>(BackgroundProperty, ArrowOpacityProperty, BorderBrushProperty,
            BorderThicknessProperty, CornerRadiusProperty);
    }

    public static Direction GetDirection(ArrowPosition arrowPosition)
    {
        return arrowPosition switch
        {
            ArrowPosition.Left => Direction.Left,
            ArrowPosition.LeftEdgeAlignedBottom => Direction.Left,
            ArrowPosition.LeftEdgeAlignedTop => Direction.Left,

            ArrowPosition.Top => Direction.Top,
            ArrowPosition.TopEdgeAlignedLeft => Direction.Top,
            ArrowPosition.TopEdgeAlignedRight => Direction.Top,

            ArrowPosition.Right => Direction.Right,
            ArrowPosition.RightEdgeAlignedBottom => Direction.Right,
            ArrowPosition.RightEdgeAlignedTop => Direction.Right,

            ArrowPosition.Bottom => Direction.Bottom,
            ArrowPosition.BottomEdgeAlignedLeft => Direction.Bottom,
            ArrowPosition.BottomEdgeAlignedRight => Direction.Bottom,
            _ => throw new ArgumentOutOfRangeException(nameof(arrowPosition), arrowPosition,
                "Invalid value for ArrowPosition")
        };
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == BorderThicknessProperty)
        {
            PseudoClasses.Set(BorderedPseudoClass, BorderThickness != default);
        }

        if (change.Property == ArrowPositionProperty)
        {
            SetArrowDirectionIfChanged(GetDirection(ArrowPosition));
        }
        else if (change.Property == ArrowDirectionProperty)
        {
            // 因为属性更新比布局更新快，我们计算 GetMaskBounds 时候等不及布局更新就要计算坐标了
            var oldDirection = change.GetOldValue<Direction>();
            var newDirection = change.GetNewValue<Direction>();
            if ((oldDirection == Direction.Left && newDirection == Direction.Right) ||
                (oldDirection == Direction.Right && newDirection == Direction.Left) ||
                (oldDirection == Direction.Top && newDirection == Direction.Bottom) ||
                (oldDirection == Direction.Bottom && newDirection == Direction.Top))
            {
                ArrowPlacementFlipped = true;
            }
        }
    }

    public CornerRadius GetMaskCornerRadius()
    {
        // 阴影蒙版圆角必须跟随真正可见的内容装饰器（容器 Border）：语义部件样式可以单独
        // 覆盖容器 Border 的圆角，此时 ArrowDecoratedBox 自身的 CornerRadius 与可见形状
        // 不一致，按可见形状取圆角才能避免弹层角落出现白底空隙。
        return _contentDecorator?.CornerRadius ?? CornerRadius;
    }

    public IBrush? GetMaskBackground()
    {
        return Background;
    }

    public Rect GetMaskBounds()
    {
        Debug.Assert(ArrowIndicatorLayout != null && _contentDecorator != null);
        var targetRect = _contentDecorator.Bounds;
        var arrowSize  = ArrowIndicatorLayout.DesiredSize;
        if (ArrowPlacementFlipped)
        {
            if (ArrowDirection == Direction.Top)
            {
                targetRect = targetRect.WithY(targetRect.Y + arrowSize.Height);
            }
            else if (ArrowDirection == Direction.Bottom)
            {
                targetRect = targetRect.WithY(targetRect.Y - arrowSize.Height);
            }
            else if (ArrowDirection == Direction.Left)
            {
                targetRect = targetRect.WithX(targetRect.X + arrowSize.Width);
            }
            else
            {
                targetRect = targetRect.WithX(targetRect.X - arrowSize.Width);
            }
        }

        return targetRect;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _contentDecorator    = e.NameScope.Get<Border>("PART_ContentDecorator");
        ArrowIndicatorLayout = e.NameScope.Get<Control>("PART_ArrowIndicatorLayout");
        ArrowIndicator       = e.NameScope.Get<ArrowIndicator>("PART_ArrowIndicator");
        SetArrowDirectionIfChanged(GetDirection(ArrowPosition));
    }

    private (double, double) GetArrowVertexPoint()
    {
        if (ArrowIndicatorLayout is null)
        {
            return default;
        }

        if (!ArrowIndicatorLayout.IsMeasureValid)
        {
            LayoutHelper.MeasureChild(this, Size.Infinity, Padding);
        }

        if (!ArrowIndicatorLayout.IsArrangeValid)
        {
            Arrange(new Rect(DesiredSize));
        }
        
        var targetRect  = ArrowIndicatorLayout.Bounds;
        var center      = targetRect.Center;
        var controlSize = Bounds.Size;

        // 计算中点
        var direction = GetDirection(ArrowPosition);
        if (direction == Direction.Left || direction == Direction.Right)
        {
            return (center.Y, controlSize.Height - center.Y);
        }
    
        return (center.X, controlSize.Width - center.X);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = base.ArrangeOverride(finalSize);
        if (IsArrowVisible)
        {
            ArrangeArrow(finalSize);
        }

        // The outline follows the arranged box and arrow; a move of either without a size change of this control
        // would not repaint it otherwise.
        if (UpdateOutline())
        {
            InvalidateVisual();
        }

        return size;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        UpdateOutline();
        if (_outlineGeometry is null)
        {
            return;
        }

        PenUtils.TryModifyOrCreate(ref _outlinePen, BorderBrush, _outlineShape.Thickness);
        context.DrawGeometry(Background, _outlinePen, _outlineGeometry);
    }

    /// <summary>
    /// The outline drawn while the box has a border (the content box united with the visible arrow), in this
    /// control's coordinates; <see langword="null"/> without a border.
    /// </summary>
    internal Geometry? OutlineGeometry
    {
        get
        {
            UpdateOutline();
            return _outlineGeometry;
        }
    }

    /// <summary>
    /// Rebuilds the cached outline when the box, the arrow, the radius or the border width changed. Returns whether
    /// it changed.
    /// </summary>
    private bool UpdateOutline()
    {
        var shape = ResolveOutlineShape();
        if (shape == _outlineShape)
        {
            return false;
        }

        _outlineShape    = shape;
        _outlineGeometry = shape.Thickness > 0 && shape.Box.Width > 0 && shape.Box.Height > 0
            ? BuildOutline(shape)
            : null;
        return true;
    }

    private OutlineShape ResolveOutlineShape()
    {
        var border = BorderThickness;
        if (border == default || _contentDecorator is null ||
            _contentDecorator.TransformToVisual(this) is not { } boxTransform)
        {
            return default;
        }

        // A uniform border is the supported case; a non-uniform one is drawn with its widest side.
        var thickness = Math.Max(Math.Max(border.Left, border.Right), Math.Max(border.Top, border.Bottom));
        var box       = new Rect(_contentDecorator.Bounds.Size).TransformToAABB(boxTransform);
        var shape     = new OutlineShape(box, GetMaskCornerRadius(), thickness, false, default, default);
        if (IsArrowVisible && ArrowOpacity > 0 &&
            ArrowIndicator is { } arrow &&
            arrow.Bounds.Width > 0 && arrow.Bounds.Height > 0 &&
            arrow.TryGetOutline(out var local) &&
            arrow.TransformToVisual(this) is { } arrowTransform)
        {
            var outline = local.Transform(arrowTransform);
            shape = shape with { HasArrow = true, Direction = ArrowDirection, Arrow = outline };
        }

        return shape;
    }

    /// <summary>
    /// Walks the content box clockwise along the centre line of its border and, on the arrow's edge, out around the
    /// arrow and back, so one stroke outlines both and nothing crosses the arrow's base.
    /// </summary>
    private static StreamGeometry BuildOutline(in OutlineShape shape)
    {
        var half   = shape.Thickness / 2;
        var rect   = shape.Box.Deflate(half);
        var limit  = Math.Max(0, Math.Min(rect.Width, rect.Height) / 2);
        var radius = shape.CornerRadius;
        var tl     = Math.Clamp(radius.TopLeft - half, 0, limit);
        var tr     = Math.Clamp(radius.TopRight - half, 0, limit);
        var br     = Math.Clamp(radius.BottomRight - half, 0, limit);
        var bl     = Math.Clamp(radius.BottomLeft - half, 0, limit);
        var arrow  = shape.HasArrow ? shape.Direction : (Direction?)null;

        var geometry = new StreamGeometry();
        using var context = geometry.Open();
        context.BeginFigure(new Point(rect.Left + tl, rect.Top), true);
        if (arrow == Direction.Top)
        {
            AppendArrow(context, shape.Arrow, Direction.Top, rect.Top);
        }
        context.LineTo(new Point(rect.Right - tr, rect.Top));
        AppendCorner(context, new Point(rect.Right, rect.Top + tr), tr);
        if (arrow == Direction.Right)
        {
            AppendArrow(context, shape.Arrow, Direction.Right, rect.Right);
        }
        context.LineTo(new Point(rect.Right, rect.Bottom - br));
        AppendCorner(context, new Point(rect.Right - br, rect.Bottom), br);
        if (arrow == Direction.Bottom)
        {
            AppendArrow(context, shape.Arrow, Direction.Bottom, rect.Bottom);
        }
        context.LineTo(new Point(rect.Left + bl, rect.Bottom));
        AppendCorner(context, new Point(rect.Left, rect.Bottom - bl), bl);
        if (arrow == Direction.Left)
        {
            AppendArrow(context, shape.Arrow, Direction.Left, rect.Left);
        }
        context.LineTo(new Point(rect.Left, rect.Top + tl));
        AppendCorner(context, new Point(rect.Left + tl, rect.Top), tl);
        context.EndFigure(true);
        return geometry;
    }

    private static void AppendCorner(StreamGeometryContext context, Point end, double radius)
    {
        if (radius > 0)
        {
            context.ArcTo(end, new Size(radius, radius), 0, false, SweepDirection.Clockwise);
        }
    }

    private static void AppendArrow(StreamGeometryContext context, in ArrowOutline arrow, Direction edge, double edgeLine)
    {
        var horizontal = edge is Direction.Top or Direction.Bottom;
        // The base points move onto the border's centre line: along the sides for a sharp base, straight across for a
        // flared one (its arcs leave the edge tangentially).
        var start = OnEdgeLine(arrow.BaseStart, arrow.BaseRadius > 0 ? null : arrow.TipStart, horizontal, edgeLine);
        var end   = OnEdgeLine(arrow.BaseEnd, arrow.BaseRadius > 0 ? null : arrow.TipEnd, horizontal, edgeLine);
        // Clockwise: left to right on top, top to bottom on the right, right to left at the bottom, bottom to top on
        // the left. The layout transform of the arrow can hand the points over in either order.
        var reversed = edge switch
        {
            Direction.Top => start.X > end.X,
            Direction.Right => start.Y > end.Y,
            Direction.Bottom => start.X < end.X,
            _ => start.Y < end.Y
        };

        context.LineTo(reversed ? end : start);
        arrow.AppendSides(context, reversed, reversed ? start : end);
    }

    private static Point OnEdgeLine(Point basePoint, Point? sidePoint, bool horizontal, double edgeLine)
    {
        if (sidePoint is not { } side)
        {
            return horizontal ? basePoint.WithY(edgeLine) : basePoint.WithX(edgeLine);
        }

        var from = horizontal ? basePoint.Y : basePoint.X;
        var to   = horizontal ? side.Y : side.X;
        if (Math.Abs(to - from) < double.Epsilon)
        {
            return basePoint;
        }

        var t = (edgeLine - from) / (to - from);
        return basePoint + (side - basePoint) * t;
    }

    private readonly record struct OutlineShape(
        Rect Box,
        CornerRadius CornerRadius,
        double Thickness,
        bool HasArrow,
        Direction Direction,
        ArrowOutline Arrow);

    private void ArrangeArrow(Size finalSize)
    {
        if (ArrowIndicatorLayout is null)
        {
            return;
        }

        var offsetX  = 0d;
        var offsetY  = 0d;
        var position = ArrowPosition;
        var size     = ArrowIndicatorLayout.DesiredSize;

        var minValue = Math.Min(size.Width, size.Height);
        var maxValue = Math.Max(size.Width, size.Height);
        if (position == ArrowPosition.Left ||
            position == ArrowPosition.LeftEdgeAlignedTop ||
            position == ArrowPosition.LeftEdgeAlignedBottom)
        {
            offsetX = 0.5d;
            if (position == ArrowPosition.Left)
            {
                offsetY = (finalSize.Height - maxValue) / 2;
            }
            else if (position == ArrowPosition.LeftEdgeAlignedTop)
            {
                if (maxValue * 2 > finalSize.Height / 2)
                {
                    offsetY = minValue;
                }
                else
                {
                    offsetY = maxValue;
                }
            }
            else
            {
                if (maxValue * 2 > finalSize.Height / 2)
                {
                    offsetY = finalSize.Height - minValue - maxValue;
                }
                else
                {
                    offsetY = finalSize.Height - maxValue * 2;
                }
            }
        }
        else if (position == ArrowPosition.Top ||
                 position == ArrowPosition.TopEdgeAlignedLeft ||
                 position == ArrowPosition.TopEdgeAlignedRight)
        {
            offsetY = 0.5d;
            if (position == ArrowPosition.TopEdgeAlignedLeft)
            {
                offsetX = maxValue;
            }
            else if (position == ArrowPosition.Top)
            {
                offsetX = (finalSize.Width - maxValue) / 2;
            }
            else
            {
                offsetX = finalSize.Width - maxValue * 2;
            }
        }
        else if (position == ArrowPosition.Right ||
                 position == ArrowPosition.RightEdgeAlignedTop ||
                 position == ArrowPosition.RightEdgeAlignedBottom)
        {
            offsetX = -0.5d;
            if (position == ArrowPosition.Right)
            {
                offsetY = (finalSize.Height - maxValue) / 2;
            }
            else if (position == ArrowPosition.RightEdgeAlignedTop)
            {
                if (maxValue * 2 > finalSize.Height / 2)
                {
                    offsetY = minValue;
                }
                else
                {
                    offsetY = maxValue;
                }
            }
            else
            {
                if (maxValue * 2 > finalSize.Height / 2)
                {
                    offsetY = finalSize.Height - minValue - maxValue;
                }
                else
                {
                    offsetY = finalSize.Height - maxValue * 2;
                }
            }
        }
        else
        {
            offsetY = -0.5d;
            if (position == ArrowPosition.BottomEdgeAlignedLeft)
            {
                offsetX = maxValue;
            }
            else if (position == ArrowPosition.Bottom)
            {
                offsetX = (finalSize.Width - maxValue) / 2;
            }
            else
            {
                offsetX = finalSize.Width - maxValue * 2;
            }
        }

        var adjustedOffset = AdjustArrowOffset(new Point(offsetX, offsetY), finalSize, size);
        adjustedOffset = ResolveCornerSafeArrowOffset(adjustedOffset, finalSize, size);

        ArrowIndicatorLayout.Arrange(new Rect(adjustedOffset, size));
        if (ArrowIndicator != null)
        {
            SetArrowIndicatorBoundsIfChanged(ArrowIndicator.Bounds);
        }

        SetArrowIndicatorLayoutBoundsIfChanged(ArrowIndicatorLayout.Bounds);
        ArrowPlacementFlipped     = false;
    }

    private Point ResolveCornerSafeArrowOffset(Point offset, Size finalSize, Size arrowSize)
    {
        var direction = ArrowDirection;
        var horizontal = direction is Direction.Top or Direction.Bottom;
        var start = horizontal ? offset.X : offset.Y;
        if (ArrowAnchorOffset is { } center && double.IsFinite(center))
        {
            var arrowExtent = horizontal ? arrowSize.Width : arrowSize.Height;
            start = center - arrowExtent / 2;
        }

        // The arrow's entire base must sit on the straight edge for the outline to follow it without backtracking.
        var radius = GetMaskCornerRadius();
        return direction switch
        {
            Direction.Top => new Point(ClampArrowStart(start, finalSize.Width, arrowSize.Width, radius.TopLeft, radius.TopRight), offset.Y),
            Direction.Bottom => new Point(ClampArrowStart(start, finalSize.Width, arrowSize.Width, radius.BottomLeft, radius.BottomRight), offset.Y),
            Direction.Left => new Point(offset.X, ClampArrowStart(start, finalSize.Height, arrowSize.Height, radius.TopLeft, radius.BottomLeft)),
            _ => new Point(offset.X, ClampArrowStart(start, finalSize.Height, arrowSize.Height, radius.TopRight, radius.BottomRight))
        };
    }

    private static double ClampArrowStart(double start, double edge, double arrow, double startRadius, double endRadius)
    {
        var min = Math.Max(0, startRadius);
        var max = edge - arrow - Math.Max(0, endRadius);
        return min <= max ? Math.Clamp(start, min, max) : (edge - arrow) / 2;
    }

    private void SetArrowDirectionIfChanged(Direction direction)
    {
        if (ArrowDirection != direction)
        {
            ArrowDirection = direction;
        }
    }

    private void SetArrowIndicatorBoundsIfChanged(Rect bounds)
    {
        if (ArrowIndicatorBounds != bounds)
        {
            ArrowIndicatorBounds = bounds;
        }
    }

    private void SetArrowIndicatorLayoutBoundsIfChanged(Rect bounds)
    {
        if (ArrowIndicatorLayoutBounds != bounds)
        {
            ArrowIndicatorLayoutBounds = bounds;
        }
    }

    protected virtual Point AdjustArrowOffset(Point offset, Size finalSize, Size arrowSize)
    {
        return offset;
    }

    ArrowPosition IArrowAwareShadowMaskInfoProvider.GetArrowPosition()
    {
        return ArrowPosition;
    }
    
    bool IArrowAwareShadowMaskInfoProvider.IsArrowVisible()
    {
        return IsArrowVisible;
    }

    void IArrowAwareShadowMaskInfoProvider.SetArrowOpacity(double opacity)
    {
        if (Math.Abs(ArrowOpacity - opacity) > 0.0001)
        {
            ArrowOpacity = opacity;
        }
    }

    Rect IArrowAwareShadowMaskInfoProvider.GetArrowIndicatorBounds()
    {
        return ArrowIndicatorBounds;
    }
    
    Rect IArrowAwareShadowMaskInfoProvider.GetArrowIndicatorLayoutBounds()
    {
        return ArrowIndicatorLayoutBounds;
    }

    AbstractArrowDecoratedBox? IArrowAwareShadowMaskInfoProvider.GetArrowDecoratedBox()
    {
        return this;
    }
}
