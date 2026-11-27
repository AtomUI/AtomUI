using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using AtomUI.Media;
using Avalonia.Media;

namespace AtomUI.Controls;

/// <summary>
/// Names the edge of an arrow decorated box that carries the arrow, and so the side of the box where the target
/// element lies: <see cref="Top" /> puts the arrow on the top edge, for a box shown below the target.
/// </summary>
public enum ArrowPosition
{
    /// <summary>
    /// The arrow sits in the middle of the bottom edge.
    /// </summary>
    Bottom,

    /// <summary>
    /// The arrow sits in the middle of the right edge.
    /// </summary>
    Right,

    /// <summary>
    /// The arrow sits in the middle of the left edge.
    /// </summary>
    Left,

    /// <summary>
    /// The arrow sits in the middle of the top edge.
    /// </summary>
    Top,

    /// <summary>
    /// The arrow sits on the top edge, toward its left end; the box lies below the target, with the left edges
    /// aligned.
    /// </summary>
    TopEdgeAlignedLeft,

    /// <summary>
    /// The arrow sits on the top edge, toward its right end; the box lies below the target, with the right edges
    /// aligned.
    /// </summary>
    TopEdgeAlignedRight,

    /// <summary>
    /// The arrow sits on the bottom edge, toward its left end; the box lies above the target, with the left edges
    /// aligned.
    /// </summary>
    BottomEdgeAlignedLeft,

    /// <summary>
    /// The arrow sits on the bottom edge, toward its right end; the box lies above the target, with the right edges
    /// aligned.
    /// </summary>
    BottomEdgeAlignedRight,

    /// <summary>
    /// The arrow sits on the left edge, toward its top end; the box lies to the right of the target, with the top
    /// edges aligned.
    /// </summary>
    LeftEdgeAlignedTop,

    /// <summary>
    /// The arrow sits on the left edge, toward its bottom end; the box lies to the right of the target, with the
    /// bottom edges aligned.
    /// </summary>
    LeftEdgeAlignedBottom,

    /// <summary>
    /// The arrow sits on the right edge, toward its top end; the box lies to the left of the target, with the top
    /// edges aligned.
    /// </summary>
    RightEdgeAlignedTop,

    /// <summary>
    /// The arrow sits on the right edge, toward its bottom end; the box lies to the left of the target, with the
    /// bottom edges aligned.
    /// </summary>
    RightEdgeAlignedBottom
}

/// <summary>
/// A content box with an arrow on one of its edges: the body of popups such as tooltips and menus. It places the
/// arrow from <see cref="ArrowPosition" /> and <see cref="ArrowAnchorOffset" />, draws one outline around content
/// box and arrow while the box has a border, and supplies the rect, corner radius and background of the content
/// box for the shadow mask of the host.
/// </summary>
[TemplatePart("PART_ContentDecorator", typeof(Border))]
[TemplatePart("PART_ArrowIndicatorLayout", typeof(Control))]
[TemplatePart("PART_ArrowIndicator", typeof(ArrowIndicator))]
[PseudoClasses(BorderedPseudoClass)]
public abstract class AbstractArrowDecoratedBox : ContentControl,
                                                  IArrowAwareShadowMaskInfoProvider,
                                                  IMotionAwareControl
{
    /// <summary>
    /// The name under which the template of a host names its arrow decorated box. The host looks the box up by this
    /// name to reach its arrow and its shadow mask information.
    /// </summary>
    public const string ArrowDecoratorPart = "PART_ArrowDecorator";

    /// <summary>
    /// Set while <see cref="TemplatedControl.BorderThickness" /> is not zero. The box then draws its background and
    /// border itself as one outline around the content box and the arrow (no line across the arrow's base); the
    /// built-in theme hands the template's container border and arrow indicator over to that outline.
    /// </summary>
    public const string BorderedPseudoClass = ":bordered";

    protected AbstractArrowDecoratedBox()
    {
        PseudoClasses.Set(BorderedPseudoClass, BorderThickness != default);
    }

    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="IsArrowVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsArrowVisibleProperty =
        AvaloniaProperty.Register<AbstractArrowDecoratedBox, bool>(nameof(IsArrowVisible), true);

    /// <summary>
    /// Identifies the <see cref="ArrowPosition" /> property.
    /// </summary>
    public static readonly StyledProperty<ArrowPosition> ArrowPositionProperty =
        AvaloniaProperty.Register<AbstractArrowDecoratedBox, ArrowPosition>(
            nameof(ArrowPosition));

    /// <summary>
    /// Identifies the <see cref="ArrowAnchorOffset" /> property.
    /// </summary>
    public static readonly StyledProperty<double?> ArrowAnchorOffsetProperty =
        AvaloniaProperty.Register<AbstractArrowDecoratedBox, double?>(nameof(ArrowAnchorOffset));

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractArrowDecoratedBox>();

    /// <summary>
    /// Gets or sets whether the box shows its arrow. While it is <see langword="false" />, the arrow is not laid out
    /// and the outline of a bordered box leaves it out.
    /// </summary>
    public bool IsArrowVisible
    {
        get => GetValue(IsArrowVisibleProperty);
        set => SetValue(IsArrowVisibleProperty, value);
    }

    /// <summary>
    /// Gets or sets the edge of the box that carries the arrow and, for the edge-aligned values, the end of the edge
    /// it sits toward. <see cref="ArrowAnchorOffset" /> replaces the position along the edge when it is set. The value
    /// moves only the arrow; where the box lies next to its target is up to the host.
    /// </summary>
    public ArrowPosition ArrowPosition
    {
        get => GetValue(ArrowPositionProperty);
        set => SetValue(ArrowPositionProperty, value);
    }

    /// <summary>
    /// Gets or sets the arrow center in device-independent units from the left or top of its edge.
    /// A null or non-finite value uses the alignment specified by <see cref="ArrowPosition" />. The arrow is then
    /// moved, if needed, so that its base stays on the straight part of the edge and clear of the rounded corners,
    /// and is centered when the straight part is shorter than the arrow.
    /// </summary>
    public double? ArrowAnchorOffset
    {
        get => GetValue(ArrowAnchorOffsetProperty);
        set => SetValue(ArrowAnchorOffsetProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the box animates its state changes. The base box plays no motion of its own; a host such
    /// as a flyout relays its own value to it. The built-in theme takes the value from the <c>EnableMotion</c> token.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<double> ArrowSizeProperty =
        AvaloniaProperty.Register<AbstractArrowDecoratedBox, double>(nameof(ArrowSize));

    /// <summary>
    /// Identifies the <see cref="ArrowStrokeThickness" /> property.
    /// </summary>
    internal static readonly StyledProperty<double> ArrowStrokeThicknessProperty =
        AvaloniaProperty.Register<AbstractArrowDecoratedBox, double>(nameof(ArrowStrokeThickness), 1.0);

    /// <summary>
    /// Identifies the <see cref="ArrowTipRadius" /> property.
    /// </summary>
    internal static readonly StyledProperty<double> ArrowTipRadiusProperty =
        AvaloniaProperty.Register<AbstractArrowDecoratedBox, double>(nameof(ArrowTipRadius));

    /// <summary>
    /// Identifies the <see cref="ArrowBaseRadius" /> property.
    /// </summary>
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
    
    /// <summary>
    /// Size of the arrow, in DIPs; <c>0</c> by default. The template hands it to <c>PART_ArrowIndicator</c> as its
    /// <c>ArrowSize</c>. The built-in theme takes it from the <c>ArrowSize</c> Token of the arrow decorated box and
    /// the tooltip theme from its own <c>ArrowSize</c> Token.
    /// </summary>
    internal double ArrowSize
    {
        get => GetValue(ArrowSizeProperty);
        set => SetValue(ArrowSizeProperty, value);
    }

    /// <summary>
    /// Width of the outline of the arrow indicator, in DIPs; <c>1</c> by default. The template hands it to
    /// <c>PART_ArrowIndicator</c> as its stroke thickness. The built-in theme takes it from the
    /// <c>ArrowStrokeThickness</c> Token of the arrow decorated box and the tooltip theme from its own Token of that
    /// name, which lets a tooltip show its arrow without an outline. While the box has a border, the outline of the
    /// box includes the arrow and the indicator paints nothing, so this width has no visible effect.
    /// </summary>
    internal double ArrowStrokeThickness
    {
        get => GetValue(ArrowStrokeThicknessProperty);
        set => SetValue(ArrowStrokeThicknessProperty, value);
    }

    /// <summary>
    /// Radius of the rounded tip of the arrow, in DIPs; <c>0</c> by default. The template hands it to
    /// <c>PART_ArrowIndicator</c> as its <see cref="AtomUI.Controls.ArrowIndicator.TipRadius" />. The built-in theme
    /// takes it from the <c>ArrowTipRadius</c> Token of the arrow decorated box and the tooltip theme from its own
    /// Token of that name.
    /// </summary>
    internal double ArrowTipRadius
    {
        get => GetValue(ArrowTipRadiusProperty);
        set => SetValue(ArrowTipRadiusProperty, value);
    }

    /// <summary>
    /// Radius of the arcs that join the arrow to the edge of the box, in DIPs; <c>0</c> by default. The template
    /// hands it to <c>PART_ArrowIndicator</c> as its <see cref="AtomUI.Controls.ArrowIndicator.BaseRadius" />. The
    /// built-in theme takes it from the <c>ArrowBaseRadius</c> Token of the arrow decorated box and the tooltip
    /// theme from its own Token of that name.
    /// </summary>
    internal double ArrowBaseRadius
    {
        get => GetValue(ArrowBaseRadiusProperty);
        set => SetValue(ArrowBaseRadiusProperty, value);
    }

    /// <summary>
    /// Edge of the box that carries the arrow, as the direction the arrow points to. The box derives it from
    /// <see cref="ArrowPosition" /> through <see cref="GetDirection" /> when that changes and when the template is
    /// applied, so a value set from elsewhere lasts only until the next of those two. The theme selects on it to
    /// dock the arrow layout to that edge and to rotate the arrow into place. A change invalidates the measure of
    /// the box.
    /// </summary>
    internal Direction ArrowDirection
    {
        get => GetValue(ArrowDirectionProperty);
        set => SetValue(ArrowDirectionProperty, value);
    }
    
    /// <summary>
    /// Opacity of the arrow layout (<c>PART_ArrowIndicatorLayout</c>) from <c>0</c> to <c>1</c>; <c>1</c> by
    /// default. A host sets it through <see cref="IArrowAwareShadowMaskInfoProvider.SetArrowOpacity" />, as the
    /// tooltip does on the box of its template. While it is <c>0</c>, the outline of a bordered box leaves
    /// the arrow out.
    /// </summary>
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

    // The center of the arrow layout, as two distances along the edge that carries the arrow: from the left or top
    // end of this control, then from the opposite end. The layout is brought up to date first when it is stale.
    internal (double, double) ArrowVertexPoint => GetArrowVertexPoint();
    private Border? _contentDecorator;

    internal Border? ContentDecorator => _contentDecorator;
    private protected Control? ArrowIndicatorLayout;
    // ArrowIndicatorLayoutBounds is relative to the arrow layout panel of the template, not to this control;
    // transform through this part itself when its real position is needed.
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

    /// <summary>
    /// Returns the side of the box that carries the arrow; the edge-aligned values give the side of their base value.
    /// </summary>
    /// <param name="arrowPosition">
    /// The arrow position to convert.
    /// </param>
    /// <returns>
    /// The side of the box with the arrow, which is the direction the arrow points to.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="arrowPosition" /> is not a value of <see cref="AtomUI.Controls.ArrowPosition" />.
    /// </exception>
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

    /// <summary>
    /// Calls the base implementation first, then reacts to three properties of the box: it sets the <see
    /// cref="BorderedPseudoClass" /> pseudo-class from <see cref="TemplatedControl.BorderThickness" />, applies a
    /// changed <see cref="ArrowPosition" /> to the arrow direction, and records a flip of the arrow to the opposite
    /// edge until the next arrange pass has moved it.
    /// </summary>
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
            // A property change runs ahead of the layout pass, so GetMaskBounds can be asked for its rect while the
            // template still shows the old direction.
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

    /// <summary>
    /// Returns the corner radius that the shadow mask of the host follows: the one of the content decorator of the
    /// template, which can differ from <see cref="CornerRadius" />, or the <see cref="CornerRadius" /> of this
    /// control while the template is not applied.
    /// </summary>
    public CornerRadius GetMaskCornerRadius()
    {
        // The shadow mask must follow the content decorator that is actually visible. A semantic part style can
        // override the corner radius of that container alone, and the CornerRadius of the box then differs from the
        // visible shape. Using the visible radius keeps gaps out of the corners of the popup.
        return _contentDecorator?.CornerRadius ?? CornerRadius;
    }

    /// <summary>
    /// Returns the background of the box as the brush of the host's shadow mask.
    /// </summary>
    public IBrush? GetMaskBackground()
    {
        return Background;
    }

    /// <summary>
    /// Returns the bounds of the content decorator of the template, which leave out the arrow, for the shadow mask
    /// of the host. While a flip of the arrow to the opposite edge has not reached the layout pass, the rect is
    /// shifted by the size of the arrow to where that pass will put it. The template must be applied.
    /// </summary>
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

    /// <summary>
    /// Calls the base implementation first, then finds <c>PART_ContentDecorator</c>, <c>PART_ArrowIndicatorLayout</c>
    /// and <c>PART_ArrowIndicator</c>, and applies <see cref="ArrowPosition" /> to the arrow direction.
    /// </summary>
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

        // The edge is split at the arrow center into the distance to each of its ends.
        var direction = GetDirection(ArrowPosition);
        if (direction == Direction.Left || direction == Direction.Right)
        {
            return (center.Y, controlSize.Height - center.Y);
        }
    
        return (center.X, controlSize.Width - center.X);
    }

    /// <summary>
    /// Calls the base implementation first, then places the arrow when it is visible and refreshes the outline, so
    /// that a moved arrow is repainted even when the size of this control did not change.
    /// </summary>
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

    /// <summary>
    /// Calls the base implementation first, then, while the box has a border, draws the background and the border as
    /// one outline around the content box and the visible arrow. Without a border it draws nothing of its own.
    /// </summary>
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
    /// control's coordinates; <see langword="null" /> without a border.
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
    /// Walks the content box clockwise along the center line of its border and, on the arrow's edge, out around the
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
        // The base points move onto the border's center line: along the sides for a sharp base, straight across for a
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

    /// <summary>
    /// Lets a derived class move the arrow. It runs in the arrange pass, when the arrow is visible, with the position
    /// computed from <see cref="ArrowPosition" />, before the arrow is kept clear of the rounded corners. The base
    /// implementation returns <paramref name="offset" /> unchanged.
    /// </summary>
    /// <param name="offset">
    /// The default position of the arrow layout, in the coordinates of this control.
    /// </param>
    /// <param name="finalSize">
    /// The size allotted to this control in the arrange pass.
    /// </param>
    /// <param name="arrowSize">
    /// The desired size of the arrow layout.
    /// </param>
    /// <returns>
    /// The position of the arrow layout, in the coordinates of this control. Its coordinate along the arrow's edge is
    /// still replaced by <see cref="ArrowAnchorOffset" /> when that is set, and clamped to the straight part of the
    /// edge.
    /// </returns>
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
