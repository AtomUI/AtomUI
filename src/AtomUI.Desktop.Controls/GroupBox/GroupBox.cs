using AtomUI.Controls.Utils;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

public enum GroupBoxTitlePosition
{
    Left,
    Right,
    Center
}

public partial class GroupBox : ContentControl
{
    #region 公共属性定义

    public static readonly StyledProperty<string?> HeaderTitleProperty =
        AvaloniaProperty.Register<GroupBox, string?>(nameof(HeaderTitle));

    public static readonly StyledProperty<IBrush?> HeaderTitleColorProperty =
        AvaloniaProperty.Register<GroupBox, IBrush?>(nameof(HeaderTitleColor));

    public static readonly StyledProperty<PathIcon?> HeaderIconProperty =
        AvaloniaProperty.Register<GroupBox, PathIcon?>(nameof(HeaderIcon));

    public static readonly StyledProperty<GroupBoxTitlePosition> HeaderTitlePositionProperty =
        AvaloniaProperty.Register<GroupBox, GroupBoxTitlePosition>(nameof(HeaderTitlePosition));

    public static readonly StyledProperty<double> HeaderFontSizeProperty =
        AvaloniaProperty.Register<GroupBox, double>(nameof(HeaderFontSize));

    public static readonly StyledProperty<FontStyle> HeaderFontStyleProperty =
        AvaloniaProperty.Register<GroupBox, FontStyle>(nameof(HeaderFontStyle));

    public static readonly StyledProperty<FontWeight> HeaderFontWeightProperty =
        AvaloniaProperty.Register<GroupBox, FontWeight>(nameof(HeaderFontWeight), FontWeight.Normal);

    public string? HeaderTitle
    {
        get => GetValue(HeaderTitleProperty);
        set => SetValue(HeaderTitleProperty, value);
    }

    public IBrush? HeaderTitleColor
    {
        get => GetValue(HeaderTitleColorProperty);
        set => SetValue(HeaderTitleColorProperty, value);
    }

    public PathIcon? HeaderIcon
    {
        get => GetValue(HeaderIconProperty);
        set => SetValue(HeaderIconProperty, value);
    }

    public GroupBoxTitlePosition HeaderTitlePosition
    {
        get => GetValue(HeaderTitlePositionProperty);
        set => SetValue(HeaderTitlePositionProperty, value);
    }

    public double HeaderFontSize
    {
        get => GetValue(HeaderFontSizeProperty);
        set => SetValue(HeaderFontSizeProperty, value);
    }

    public FontStyle HeaderFontStyle
    {
        get => GetValue(HeaderFontStyleProperty);
        set => SetValue(HeaderFontStyleProperty, value);
    }

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

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _headerContentContainer = e.NameScope.Find<Decorator>("PART_HeaderContent");
        _frame                  = e.NameScope.Find<Border>("PART_Frame");
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return LayoutHelper.MeasureChild(_frame, availableSize, default, BorderThickness);
    }

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
