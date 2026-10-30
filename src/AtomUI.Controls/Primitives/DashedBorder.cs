using AtomUI.Controls.Utils;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Primitives;

public class DashedBorder : Decorator
{
    #region 公共属性定义
    public static readonly StyledProperty<IBrush?> BackgroundProperty =
            AvaloniaProperty.Register<DashedBorder, IBrush?>(nameof(Background));
    
    public static readonly StyledProperty<BackgroundSizing> BackgroundSizingProperty =
        AvaloniaProperty.Register<DashedBorder, BackgroundSizing>(
            nameof(BackgroundSizing),
            BackgroundSizing.CenterBorder);
    
    public static readonly StyledProperty<IBrush?> BorderBrushProperty =
        AvaloniaProperty.Register<DashedBorder, IBrush?>(nameof(BorderBrush));
        
    public static readonly StyledProperty<Thickness> BorderThicknessProperty =
        AvaloniaProperty.Register<DashedBorder, Thickness>(nameof(BorderThickness));
        
    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<DashedBorder, CornerRadius>(nameof(CornerRadius));
        
    public static readonly StyledProperty<BoxShadows> BoxShadowProperty =
        AvaloniaProperty.Register<DashedBorder, BoxShadows>(nameof(BoxShadow));
    
    public static readonly StyledProperty<IReadOnlyList<double>?> StrokeDashArrayProperty =
        AvaloniaProperty.Register<DashedBorder, IReadOnlyList<double>?>(nameof(StrokeDashArray));
    
    public static readonly StyledProperty<double> StrokeDaskOffsetProperty =
        AvaloniaProperty.Register<DashedBorder, double>(nameof(StrokeDaskOffset), 0.0);

    public static readonly StyledProperty<bool> ClipContentToCornerRadiusProperty =
        AvaloniaProperty.Register<DashedBorder, bool>(nameof(ClipContentToCornerRadius));

    /// <summary>
    /// Gets or sets a value indicating whether the child content is clipped to the
    /// inner edge of the rounded border. When enabled, this border owns the child's
    /// <see cref="Visual.Clip" />.
    /// </summary>
    public bool ClipContentToCornerRadius
    {
        get => GetValue(ClipContentToCornerRadiusProperty);
        set => SetValue(ClipContentToCornerRadiusProperty, value);
    }
    
    /// <summary>
    /// Gets or sets a brush with which to paint the background.
    /// </summary>
    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets how the background is drawn relative to the border.
    /// </summary>
    public BackgroundSizing BackgroundSizing
    {
        get => GetValue(BackgroundSizingProperty);
        set => SetValue(BackgroundSizingProperty, value);
    }

    /// <summary>
    /// Gets or sets a brush with which to paint the border.
    /// </summary>
    public IBrush? BorderBrush
    {
        get => GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the thickness of the border.
    /// </summary>
    public Thickness BorderThickness
    {
        get => GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets the radius of the border rounded corners.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <summary>
    /// Gets or sets the box shadow effect parameters
    /// </summary>
    public BoxShadows BoxShadow
    {
        get => GetValue(BoxShadowProperty);
        set => SetValue(BoxShadowProperty, value);
    }
    
    public IReadOnlyList<double>? StrokeDashArray
    {
        get => GetValue(StrokeDashArrayProperty);
        set => SetValue(StrokeDashArrayProperty, value);
    }

    public double StrokeDaskOffset
    {
        get => GetValue(StrokeDaskOffsetProperty);
        set => SetValue(StrokeDaskOffsetProperty, value);
    }
    
    #endregion
    
    // 不能使用 readonly + inline 初始化：win-x86 ReadyToRun 镜像会把这个字段错误地
    // 读成 null（见 https://github.com/AtomUI/AtomUI/issues/429），这里改为使用时惰性兜底。
    private BorderRenderHelper? _borderRenderHelper = new BorderRenderHelper();
    private Thickness? _renderThickness;
    private double _layoutScale;
    private Control? _managedClipChild;
    private Geometry? _managedClipGeometry;
    private TopLevel? _clipScaleRoot;

    internal virtual bool ClipTrailingEdgeAtFractionalScale => false;

    static DashedBorder()
    {
        AffectsRender<DashedBorder>(
            BackgroundProperty,
            BackgroundSizingProperty,
            BorderBrushProperty,
            BorderThicknessProperty,
            CornerRadiusProperty,
            BoxShadowProperty,
            StrokeDashArrayProperty,
            StrokeDaskOffsetProperty,
            UseLayoutRoundingProperty);
        AffectsMeasure<DashedBorder>(BorderThicknessProperty);
    }

    public sealed override void Render(DrawingContext context)
    {
        var borderRenderHelper = _borderRenderHelper ??= new BorderRenderHelper();
        var layoutScale = LayoutHelper.GetLayoutScale(this);
        var renderThickness = GetRenderThickness(layoutScale);
        var renderSize = CalculateRenderSize(Bounds.Size, layoutScale);

        borderRenderHelper.Render(
            context,
            renderSize,
            renderThickness,
            CornerRadius,
            BackgroundSizing,
            Background,
            BorderBrush,
            StrokeDashArray,
            StrokeDaskOffset,
            BoxShadow);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == ChildProperty)
        {
            ReleaseManagedClip();
        }

        base.OnPropertyChanged(change);
        if (change.Property == BorderThicknessProperty ||
            change.Property == UseLayoutRoundingProperty)
        {
            _renderThickness = null;
        }

        if (change.Property == ClipContentToCornerRadiusProperty ||
            change.Property == BorderThicknessProperty ||
            change.Property == CornerRadiusProperty ||
            change.Property == BackgroundSizingProperty)
        {
            if (change.Property == ClipContentToCornerRadiusProperty)
            {
                UpdateClipScaleSubscription();
            }

            UpdateClip();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateClipScaleSubscription();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ReleaseClipScaleSubscription();
        base.OnDetachedFromVisualTree(e);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return LayoutHelper.MeasureChild(Child, availableSize, Padding, BorderThickness);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = LayoutHelper.ArrangeChild(Child, finalSize, Padding, BorderThickness);
        UpdateClip();
        return size;
    }

    private void UpdateClip()
    {
        if (Child is null)
        {
            ReleaseManagedClip();
            return;
        }

        if (!ClipContentToCornerRadius)
        {
            ReleaseManagedClip();
            return;
        }

        var childSize = Child.Bounds.Size;
        if (childSize.Width <= 0 || childSize.Height <= 0)
        {
            ReleaseManagedClip();
            return;
        }

        var renderThickness = GetRenderThickness(LayoutHelper.GetLayoutScale(this));
        var figure = BuildRoundedRectClipFigure(
            new Rect(childSize),
            Padding,
            renderThickness,
            CornerRadius);
        if (!SupportsGeometryClipHitTesting(figure))
        {
            // A platform whose geometry containment cannot represent the rounded
            // figure would silently swallow every pointer over the content. The clip
            // degrades to not-applied so input keeps working; production backends
            // with correct rounded geometry hit-testing always take the clip path.
            ReleaseManagedClip();
            return;
        }

        ReleaseManagedClip();
        Child.Clip = figure;
        _managedClipChild = Child;
        _managedClipGeometry = figure;
    }

    private void ReleaseManagedClip()
    {
        if (_managedClipChild is not null &&
            _managedClipGeometry is not null &&
            ReferenceEquals(_managedClipChild.Clip, _managedClipGeometry))
        {
            _managedClipChild.Clip = null;
        }

        _managedClipChild = null;
        _managedClipGeometry = null;
    }

    /// <summary>
    /// Determines whether the platform's geometry containment can hit-test the given
    /// clip figure. The clip narrows pointer input to the rounded shape, so it must
    /// only be applied when the platform reports containment for the figure's
    /// interior and rejects its exterior.
    /// </summary>
    protected virtual bool SupportsGeometryClipHitTesting(Geometry figure)
    {
        var bounds = figure.Bounds;
        var interior = bounds.Center;
        var exterior = new Point(bounds.Right + 1000, bounds.Bottom + 1000);

        return figure.FillContains(interior) && !figure.FillContains(exterior);
    }

    private static Geometry BuildRoundedRectClipFigure(
        Rect childBounds,
        Thickness padding,
        Thickness borderThickness,
        CornerRadius cornerRadius)
    {
        // The clip lives in the child's coordinate space. The ring's inner edge is the
        // frame bounds deflated by the border thickness; the child itself is inset by
        // the padding plus the border thickness, so in child space the inner edge is
        // the child bounds expanded back by the padding. The WinUI keypoints builder
        // deflates the given bounds by the border thickness again, hence the inflate.
        var innerEdgeBounds = childBounds.Inflate(padding + borderThickness);

        var keypoints = RoundRectGeometryBuilder.CalculateRoundedCornersRectangleWinUI(
            innerEdgeBounds,
            borderThickness,
            cornerRadius,
            BackgroundSizing.InnerBorderEdge);

        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            RoundRectGeometryBuilder.DrawRoundedCornersRectangle(context, ref keypoints);
        }

        return geometry;
    }

    private void UpdateClipScaleSubscription()
    {
        var root = ClipContentToCornerRadius && this.IsAttachedToVisualTree()
            ? TopLevel.GetTopLevel(this)
            : null;
        if (ReferenceEquals(root, _clipScaleRoot))
        {
            return;
        }

        ReleaseClipScaleSubscription();
        _clipScaleRoot = root;
        if (_clipScaleRoot is not null)
        {
            _clipScaleRoot.ScalingChanged += HandleClipScaleChanged;
        }
    }

    private void ReleaseClipScaleSubscription()
    {
        if (_clipScaleRoot is not null)
        {
            _clipScaleRoot.ScalingChanged -= HandleClipScaleChanged;
            _clipScaleRoot = null;
        }
    }

    private void HandleClipScaleChanged(object? sender, EventArgs e)
    {
        // Scaling can change after layout but before drawing. Prepare the child
        // clip here so Render only updates its own numerical and drawing caches.
        UpdateClip();
    }

    private Thickness GetRenderThickness(double layoutScale)
    {
        if (!MathUtils.AreClose(layoutScale, _layoutScale))
        {
            _layoutScale     = layoutScale;
            _renderThickness = null;
        }

        _renderThickness ??= UseLayoutRounding
            ? BorderUtils.BuildRenderScaleAwareThickness(BorderThickness, layoutScale)
            : BorderThickness;
        return _renderThickness.Value;
    }

    private Size CalculateRenderSize(Size size, double layoutScale)
    {
        if (!UseLayoutRounding ||
            !ClipTrailingEdgeAtFractionalScale ||
            MathUtils.AreClose(layoutScale, Math.Floor(layoutScale)))
        {
            return size;
        }

        var physicalPixel = 1 / layoutScale;
        var renderSize    = size;

        for (Visual? ancestor = this.GetVisualParent(); ancestor is not null; ancestor = ancestor.GetVisualParent())
        {
            if (!ancestor.ClipToBounds && ancestor.Clip is null)
            {
                continue;
            }

            var transform = this.TransformToVisual(ancestor);
            if (transform is null)
            {
                continue;
            }

            var transformedBounds = new Rect(renderSize).TransformToAABB(transform.Value);
            var clipBounds        = new Rect(ancestor.Bounds.Size);
            var rightOverflow     = transformedBounds.Right - clipBounds.Right;
            var bottomOverflow    = transformedBounds.Bottom - clipBounds.Bottom;

            if (rightOverflow > 0 && rightOverflow <= physicalPixel + LayoutHelper.LayoutEpsilon)
            {
                renderSize = renderSize.WithWidth(Math.Max(0, renderSize.Width - rightOverflow));
            }

            if (bottomOverflow > 0 && bottomOverflow <= physicalPixel + LayoutHelper.LayoutEpsilon)
            {
                renderSize = renderSize.WithHeight(Math.Max(0, renderSize.Height - bottomOverflow));
            }
        }

        return renderSize;
    }
}
