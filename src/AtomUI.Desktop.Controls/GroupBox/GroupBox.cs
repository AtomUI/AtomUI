using AtomUI.Controls.Utils;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The horizontal placement of the header block, icon and title together, inside a <see cref="GroupBox" />.
/// </summary>
public enum GroupBoxTitlePosition
{
    /// <summary>
    /// Header block at the left edge of the header row. This is the default.
    /// </summary>
    Left,
    /// <summary>
    /// Header block at the right edge of the header row.
    /// </summary>
    Right,
    /// <summary>
    /// Header block centered in the header row.
    /// </summary>
    Center
}

/// <summary>
/// A frame with a header that interrupts the top border, for grouping related content. The control paints the
/// frame itself in <see cref="Render" />: the background and the border start at the vertical middle of the
/// header, and the border is left out behind it. A replacement template must keep the <c>PART_HeaderContent</c>
/// and <c>PART_Frame</c> parts; without <c>PART_HeaderContent</c> the control paints neither background nor
/// border.
/// </summary>
public partial class GroupBox : ContentControl
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="HeaderTitle" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> HeaderTitleProperty =
        AvaloniaProperty.Register<GroupBox, string?>(nameof(HeaderTitle));

    /// <summary>
    /// Identifies the <see cref="HeaderTitleColor" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> HeaderTitleColorProperty =
        AvaloniaProperty.Register<GroupBox, IBrush?>(nameof(HeaderTitleColor));

    /// <summary>
    /// Identifies the <see cref="HeaderIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> HeaderIconProperty =
        AvaloniaProperty.Register<GroupBox, PathIcon?>(nameof(HeaderIcon));

    /// <summary>
    /// Identifies the <see cref="HeaderTitlePosition" /> property.
    /// </summary>
    public static readonly StyledProperty<GroupBoxTitlePosition> HeaderTitlePositionProperty =
        AvaloniaProperty.Register<GroupBox, GroupBoxTitlePosition>(nameof(HeaderTitlePosition));

    /// <summary>
    /// Identifies the <see cref="HeaderFontSize" /> property.
    /// </summary>
    public static readonly StyledProperty<double> HeaderFontSizeProperty =
        AvaloniaProperty.Register<GroupBox, double>(nameof(HeaderFontSize));

    /// <summary>
    /// Identifies the <see cref="HeaderFontStyle" /> property.
    /// </summary>
    public static readonly StyledProperty<FontStyle> HeaderFontStyleProperty =
        AvaloniaProperty.Register<GroupBox, FontStyle>(nameof(HeaderFontStyle));

    /// <summary>
    /// Identifies the <see cref="HeaderFontWeight" /> property.
    /// </summary>
    public static readonly StyledProperty<FontWeight> HeaderFontWeightProperty =
        AvaloniaProperty.Register<GroupBox, FontWeight>(nameof(HeaderFontWeight), FontWeight.Normal);

    /// <summary>
    /// Text of the title in the header. An empty or <see langword="null" /> value does not hide the header row.
    /// </summary>
    public string? HeaderTitle
    {
        get => GetValue(HeaderTitleProperty);
        set => SetValue(HeaderTitleProperty, value);
    }

    /// <summary>
    /// Brush of the title text. It does not recolor <see cref="HeaderIcon" />, and the theme applies the
    /// <c>ColorText</c> token while the value is unset.
    /// </summary>
    public IBrush? HeaderTitleColor
    {
        get => GetValue(HeaderTitleColorProperty);
        set => SetValue(HeaderTitleColorProperty, value);
    }

    /// <summary>
    /// Icon shown in front of the title in the header. <see langword="null" /> hides the icon element without
    /// removing it from the visual tree.
    /// </summary>
    public PathIcon? HeaderIcon
    {
        get => GetValue(HeaderIconProperty);
        set => SetValue(HeaderIconProperty, value);
    }

    /// <summary>
    /// Horizontal placement of the header block, the icon and the title together, within the header row. The default
    /// is <see cref="GroupBoxTitlePosition.Left" />, and the gap in the top border follows the block.
    /// </summary>
    public GroupBoxTitlePosition HeaderTitlePosition
    {
        get => GetValue(HeaderTitlePositionProperty);
        set => SetValue(HeaderTitlePositionProperty, value);
    }

    /// <summary>
    /// Font size, in DIPs, of the title text. The theme applies the <c>FontSize</c> token while the value is unset.
    /// </summary>
    public double HeaderFontSize
    {
        get => GetValue(HeaderFontSizeProperty);
        set => SetValue(HeaderFontSizeProperty, value);
    }

    /// <summary>
    /// Style of the title text. The theme supplies no default, so the value is <see cref="FontStyle.Normal" /> unless
    /// it is set.
    /// </summary>
    public FontStyle HeaderFontStyle
    {
        get => GetValue(HeaderFontStyleProperty);
        set => SetValue(HeaderFontStyleProperty, value);
    }

    /// <summary>
    /// Weight of the title text. The theme applies the <c>HeaderFontWeight</c> token while the value is unset.
    /// </summary>
    public FontWeight HeaderFontWeight
    {
        get => GetValue(HeaderFontWeightProperty);
        set => SetValue(HeaderFontWeightProperty, value);
    }

    #endregion
    
    private Control? _headerContentContainer;
    private Border? _frame;
    private Rect _borderBounds;
    private Geometry? _backgroundGeometryCache;
    private Geometry? _borderGeometryCache;
    private Rect _cachedBorderBounds;
    private Rect _cachedHeaderGapBounds;
    private Thickness _cachedBorderThickness;
    private CornerRadius _cachedCornerRadius;
    private bool _geometryCacheInitialized;
    
    static GroupBox()
    {
        AffectsMeasure<GroupBox>(HeaderIconProperty);
        AffectsRender<GroupBox>(
            BackgroundProperty,
            BorderBrushProperty,
            BorderThicknessProperty,
            CornerRadiusProperty,
            UseLayoutRoundingProperty);
    }

    public GroupBox()
    {
    }

    /// <summary>
    /// Looks up the <c>PART_HeaderContent</c> and <c>PART_Frame</c> parts that layout and rendering depend on.
    /// Overrides must call the base method; without the parts the content is not laid out and no frame is painted.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _headerContentContainer = e.NameScope.Find<Decorator>("PART_HeaderContent");
        _frame                  = e.NameScope.Find<Border>("PART_Frame");
    }

    /// <summary>
    /// Measures the <c>PART_Frame</c> part in the space that <c>BorderThickness</c> leaves. <c>Padding</c> is not
    /// applied here, because the template uses it as the margin of the content presenter.
    /// </summary>
    protected override Size MeasureOverride(Size availableSize)
    {
        return LayoutHelper.MeasureChild(_frame, availableSize, default, BorderThickness);
    }

    /// <summary>
    /// Arranges the <c>PART_Frame</c> part inside <c>BorderThickness</c> and records the bounds that
    /// <see cref="Render" /> paints: from the vertical middle of <c>PART_HeaderContent</c> down to the bottom edge.
    /// Overrides must call the base method; otherwise the recorded bounds stay empty and no frame is painted.
    /// </summary>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = LayoutHelper.ArrangeChild(_frame, finalSize, default, BorderThickness);
        if (_headerContentContainer is not null)
        {
            var headerOffset = _headerContentContainer.TranslatePoint(new Point(0, 0), this) ?? default;
            var offsetY      = headerOffset.Y + _headerContentContainer.DesiredSize.Height / 2;
            _borderBounds = new Rect(new Point(0, offsetY), new Size(finalSize.Width, finalSize.Height - offsetY));
        }

        return size;
    }

    /// <summary>
    /// Paints the frame: the <c>BorderBrush</c> border along the edge of the recorded bounds, the <c>Background</c>
    /// inside it, both shaped by <c>CornerRadius</c>, and the border left out behind <c>PART_HeaderContent</c>.
    /// </summary>
    public override void Render(DrawingContext context)
    {
        var headerGapBounds = CalculateHeaderGapBounds();
        EnsureRenderGeometryCache(headerGapBounds);

        if (_backgroundGeometryCache is not null)
        {
            context.DrawGeometry(Background, null, _backgroundGeometryCache);
        }

        if (_borderGeometryCache is not null)
        {
            context.DrawGeometry(BorderBrush, null, _borderGeometryCache);
        }
    }

    private Rect CalculateHeaderGapBounds()
    {
        if (_headerContentContainer is null)
        {
            return default;
        }

        var headerOffset = _headerContentContainer.TranslatePoint(default, this);
        return headerOffset is null
            ? default
            : new Rect(headerOffset.Value, _headerContentContainer.Bounds.Size);
    }

    private void EnsureRenderGeometryCache(Rect headerGapBounds)
    {
        var renderBorderThickness = BorderUtils.BuildRenderScaleAwareThickness(this, BorderThickness);
        if (_geometryCacheInitialized &&
            _cachedBorderBounds == _borderBounds &&
            _cachedHeaderGapBounds == headerGapBounds &&
            _cachedBorderThickness == renderBorderThickness &&
            _cachedCornerRadius == CornerRadius)
        {
            return;
        }

        _cachedBorderBounds       = _borderBounds;
        _cachedHeaderGapBounds    = headerGapBounds;
        _cachedBorderThickness    = renderBorderThickness;
        _cachedCornerRadius       = CornerRadius;
        _geometryCacheInitialized = true;

        _backgroundGeometryCache = CreateRoundedRectGeometry(
            _borderBounds,
            renderBorderThickness,
            CornerRadius,
            BackgroundSizing.InnerBorderEdge);
        _borderGeometryCache = CreateBorderGeometry(_borderBounds, headerGapBounds, renderBorderThickness);
    }

    private Geometry? CreateBorderGeometry(Rect borderBounds, Rect headerGapBounds, Thickness borderThickness)
    {
        if (borderBounds.Width <= 0 ||
            borderBounds.Height <= 0 ||
            !HasVisibleBorder(borderThickness))
        {
            return null;
        }

        var borderInnerGeometry = CreateRoundedRectGeometry(
            borderBounds,
            borderThickness,
            CornerRadius,
            BackgroundSizing.InnerBorderEdge);
        var borderOuterGeometry = CreateRoundedRectGeometry(
            borderBounds,
            borderThickness,
            CornerRadius,
            BackgroundSizing.OuterBorderEdge);

        if (borderOuterGeometry is null)
        {
            return null;
        }

        Geometry borderGeometry = borderInnerGeometry is null
            ? borderOuterGeometry
            : new CombinedGeometry(GeometryCombineMode.Exclude, borderOuterGeometry, borderInnerGeometry);

        if (headerGapBounds.Width > 0 && headerGapBounds.Height > 0)
        {
            borderGeometry = new CombinedGeometry(
                GeometryCombineMode.Exclude,
                borderGeometry,
                new RectangleGeometry(headerGapBounds));
        }

        return borderGeometry;
    }

    private static Geometry? CreateRoundedRectGeometry(
        Rect bounds,
        Thickness borderThickness,
        CornerRadius cornerRadius,
        BackgroundSizing backgroundSizing)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return null;
        }

        if (backgroundSizing == BackgroundSizing.InnerBorderEdge)
        {
            var innerBounds = bounds.Deflate(borderThickness);
            if (innerBounds.Width <= 0 || innerBounds.Height <= 0)
            {
                return null;
            }
        }

        var keypoints = RoundRectGeometryBuilder.CalculateRoundedCornersRectangleWinUI(
            bounds,
            borderThickness,
            cornerRadius,
            backgroundSizing);

        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            RoundRectGeometryBuilder.DrawRoundedCornersRectangle(ctx, ref keypoints);
        }

        return geometry;
    }

    private static bool HasVisibleBorder(Thickness thickness)
    {
        return thickness.Left > 0 ||
               thickness.Top > 0 ||
               thickness.Right > 0 ||
               thickness.Bottom > 0;
    }
}
