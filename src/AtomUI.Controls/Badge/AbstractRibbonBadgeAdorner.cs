using System.Globalization;
using AtomUI.Controls.Utils;
using AtomUI.Media;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Commons;

/// <summary>
/// Base class of the ribbon badge indicator, a ribbon with a text label placed at a corner of another control;
/// <c>RibbonBadgeAdorner</c> in AtomUI.Desktop.Controls derives from it.
/// </summary>
/// <remarks>
/// The label is <c>PART_LabelPart</c>. <c>AbstractRibbonBadge</c> creates the indicator through
/// <c>AbstractRibbonBadge.CreateBadgeAdorner</c>, which the concrete badge overrides.
/// </remarks>
[TemplatePart("PART_LabelPart",  typeof(TextBlock))]
internal abstract class AbstractRibbonBadgeAdorner : TemplatedControl
{
    #region 内部属性定义

    internal static readonly StyledProperty<IBrush?> RibbonColorProperty =
        AvaloniaProperty.Register<AbstractRibbonBadgeAdorner, IBrush?>(
            nameof(RibbonColor));

    internal static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<AbstractRibbonBadgeAdorner, string?>(
            nameof(Text));

    internal static readonly StyledProperty<RibbonBadgePlacement> PlacementProperty =
        AvaloniaProperty.Register<AbstractRibbonBadgeAdorner, RibbonBadgePlacement>(
            nameof(Placement));

    internal static readonly StyledProperty<Point> OffsetProperty =
        AvaloniaProperty.Register<AbstractRibbonBadgeAdorner, Point>(
            nameof(Offset));

    internal static readonly StyledProperty<bool> UseRibbonGeometryTokenResourcesProperty =
        AvaloniaProperty.Register<AbstractRibbonBadgeAdorner, bool>(nameof(UseRibbonGeometryTokenResources));

    internal static readonly DirectProperty<AbstractRibbonBadgeAdorner, bool> IsAdornerModeProperty =
        AvaloniaProperty.RegisterDirect<AbstractRibbonBadgeAdorner, bool>(
            nameof(IsAdornerMode),
            o => o.IsAdornerMode,
            (o, v) => o.IsAdornerMode = v);
    
    /// <summary>
    /// Background of the ribbon body. Only a solid brush is painted; <see langword="null" /> or a brush of another
    /// type leaves the body without a fill. The theme sets it from the <c>ColorPrimary</c> token of the ribbon badge.
    /// </summary>
    internal IBrush? RibbonColor
    {
        get => GetValue(RibbonColorProperty);
        set => SetValue(RibbonColorProperty, value);
    }

    /// <summary>
    /// Text shown on the ribbon. The theme binds the text block of the ribbon to it.
    /// </summary>
    internal string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    
    private bool _isAdornerMode;

    internal bool IsAdornerMode
    {
        get => _isAdornerMode;
        set => SetAndRaise(IsAdornerModeProperty, ref _isAdornerMode, value);
    }

    internal RibbonBadgePlacement Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    internal Point Offset
    {
        get => GetValue(OffsetProperty);
        set => SetValue(OffsetProperty, value);
    }

    /// <summary>
    /// While <see langword="true" />, the adorner observes the geometry tokens of the RibbonBadge design and takes
    /// the offset of the fold of the ribbon, the transform of the fold and the amount it is darkened from them;
    /// while <see langword="false" />, no tokens are observed and the fold is not transformed. The theme sets it to
    /// <see langword="true" />.
    /// </summary>
    internal bool UseRibbonGeometryTokenResources
    {
        get => GetValue(UseRibbonGeometryTokenResourcesProperty);
        set => SetValue(UseRibbonGeometryTokenResourcesProperty, value);
    }
    
    #endregion
    
    private TextBlock? _labelText;
    private Color? _cornerBrushSourceColor;
    private int _cornerBrushDarkenAmount;
    private IBrush? _cornerBrush;
    private Size _arrangedSize;
    private readonly BorderRenderHelper _borderRenderHelper;
    private OwnerTokenSet? _ribbonGeometryTokens;
    private Point _ribbonOffset;
    private Matrix _foldTransform = Matrix.Identity;
    private int _darkenAmount;

    private protected virtual OwnerTokenSet? CreateRibbonGeometryTokenSet(Action changed) => null;

    static AbstractRibbonBadgeAdorner()
    {
        AffectsMeasure<AbstractRibbonBadgeAdorner>(TextProperty, IsAdornerModeProperty, PlacementProperty);
        AffectsArrange<AbstractRibbonBadgeAdorner>(OffsetProperty, PlacementProperty);
        AffectsRender<AbstractRibbonBadgeAdorner>(RibbonColorProperty, OffsetProperty, PlacementProperty, CornerRadiusProperty);
    }

    public AbstractRibbonBadgeAdorner()
    {
        _borderRenderHelper = new BorderRenderHelper();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateRibbonGeometryTokenSubscription();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _ribbonGeometryTokens?.Dispose();
        _ribbonGeometryTokens = null;
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == UseRibbonGeometryTokenResourcesProperty && this.IsAttachedToVisualTree())
        {
            UpdateRibbonGeometryTokenSubscription();
        }
    }

    private void UpdateRibbonGeometryTokenSubscription()
    {
        _ribbonGeometryTokens?.Dispose();
        _ribbonGeometryTokens = UseRibbonGeometryTokenResources ? CreateRibbonGeometryTokenSet(RefreshRibbonGeometry) : null;
        RefreshRibbonGeometry();
    }

    private void RefreshRibbonGeometry()
    {
        var offset = ReadRibbonOffset(_ribbonGeometryTokens?.GetValue(0));
        var transform = _ribbonGeometryTokens?.GetValue(1) is ImmutableTransform fold ? fold.Value : Matrix.Identity;
        var darkenAmount = ReadDarkenAmount(_ribbonGeometryTokens?.GetValue(2));
        var layoutChanged = _ribbonOffset != offset || _foldTransform != transform;
        var colorChanged = _darkenAmount != darkenAmount;

        _ribbonOffset = offset;
        _foldTransform = transform;
        _darkenAmount = darkenAmount;
        if (layoutChanged)
        {
            InvalidateMeasure();
            InvalidateArrange();
        }
        if (layoutChanged || colorChanged)
        {
            InvalidateVisual();
        }
    }

    private static Point ReadRibbonOffset(object? value)
    {
        if (value is Point offset)
        {
            return offset;
        }
        if (value is string text)
        {
            try
            {
                return Point.Parse(text);
            }
            catch (FormatException)
            {
                return default;
            }
        }
        return default;
    }

    private static int ReadDarkenAmount(object? value)
    {
        if (value is int amount)
        {
            return amount;
        }
        try
        {
            return value is IConvertible convertible ? convertible.ToInt32(CultureInfo.CurrentCulture) : 0;
        }
        catch (Exception error) when (error is FormatException or InvalidCastException or OverflowException)
        {
            return 0;
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _labelText = e.NameScope.Find<TextBlock>("PART_LabelPart");
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var size     = base.MeasureOverride(availableSize);
        var foldSize = GetFold().Size;
        if (!IsAdornerMode)
        {
            return new Size(Math.Max(size.Width, foldSize.Width), size.Height + foldSize.Height);
        }

        var targetWidth = double.IsFinite(availableSize.Width)
            ? availableSize.Width
            : size.Width;
        var targetHeight = double.IsFinite(availableSize.Height)
            ? availableSize.Height
            : _ribbonOffset.Y + size.Height + foldSize.Height;
        return new Size(targetWidth, targetHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _arrangedSize = finalSize;
        if (_labelText is not null)
        {
            _labelText.Arrange(GetLayout(finalSize).BodyRect);
        }

        return finalSize;
    }

    public override void Render(DrawingContext context)
    {
        if (_labelText is null)
        {
            return;
        }

        var layout          = GetLayout(_arrangedSize);
        var backgroundBrush = RibbonColor as ISolidColorBrush;
        {
            using var state = context.PushTransform(Matrix.CreateTranslation(layout.BodyRect.X, layout.BodyRect.Y));

            _borderRenderHelper.Render(context,
                borderThickness: new Thickness(0),
                backgroundSizing: BackgroundSizing.OuterBorderEdge,
                finalSize: layout.BodyRect.Size,
                cornerRadius: new CornerRadius(CornerRadius.TopLeft,
                    CornerRadius.TopRight,
                    bottomLeft: Placement == RibbonBadgePlacement.Start
                        ? 0
                        : CornerRadius.BottomLeft,
                    bottomRight: Placement == RibbonBadgePlacement.End
                        ? 0
                        : CornerRadius.BottomRight),
                background: backgroundBrush,
                borderBrush: null,
                boxShadows: new BoxShadows());
        }
        {
            using var state           = context.PushTransform(Matrix.CreateTranslation(layout.FoldOrigin.X, layout.FoldOrigin.Y));
            var       backgroundColor = backgroundBrush?.Color;
            var       cornerBrush     = GetCornerBrush(backgroundColor);
            context.DrawGeometry(cornerBrush, null, BuildFoldGeometry(layout.Fold));
        }
    }

    private IBrush? GetCornerBrush(Color? backgroundColor)
    {
        if (backgroundColor is null)
        {
            _cornerBrushSourceColor = null;
            _cornerBrush            = null;
            return null;
        }

        var sourceColor  = backgroundColor.Value;
        var darkenAmount = _darkenAmount;
        if (_cornerBrushSourceColor == sourceColor &&
            _cornerBrushDarkenAmount == darkenAmount)
        {
            return _cornerBrush;
        }

        _cornerBrushSourceColor = sourceColor;
        _cornerBrushDarkenAmount = darkenAmount;
        _cornerBrush             = new SolidColorBrush(sourceColor.Darken(darkenAmount));
        return _cornerBrush;
    }

    private RibbonBadgeLayout GetLayout(Size finalSize)
    {
        if (_labelText is null)
        {
            return default;
        }

        var fold     = GetFold();
        var bodySize = _labelText.DesiredSize;
        var offsetX = 0d;
        var offsetY = 0d;
        if (IsAdornerMode)
        {
            offsetY += _ribbonOffset.Y + Offset.Y;
            var targetWidth = finalSize.Width > 0 ? finalSize.Width : DesiredSize.Width;
            if (Placement == RibbonBadgePlacement.End)
            {
                offsetX = targetWidth - bodySize.Width + _ribbonOffset.X + Offset.X;
            }
            else
            {
                offsetX = -_ribbonOffset.X + Offset.X;
            }
        }

        var bodyRect    = new Rect(new Point(offsetX, offsetY), bodySize);
        var foldOriginX = Placement == RibbonBadgePlacement.End
            ? bodyRect.Right - fold.Size.Width
            : bodyRect.Left;
        var foldOrigin = new Point(foldOriginX, bodyRect.Bottom);
        return new RibbonBadgeLayout(bodyRect, foldOrigin, fold);
    }

    private RibbonBadgeFold GetFold()
    {
        var width  = Math.Max(0, _ribbonOffset.X);
        var height = Math.Max(0, _ribbonOffset.Y);
        var transform = _foldTransform;
        if (Placement == RibbonBadgePlacement.Start)
        {
            transform = transform.Append(Matrix.CreateScale(-1, 1));
        }

        var p1 = transform.Transform(new Point(0, 0));
        var p2 = transform.Transform(new Point(0, height));
        var p3 = transform.Transform(new Point(width, 0));
        var left = Math.Min(Math.Min(p1.X, p2.X), p3.X);
        var top  = Math.Min(Math.Min(p1.Y, p2.Y), p3.Y);
        p1 = new Point(p1.X - left, p1.Y - top);
        p2 = new Point(p2.X - left, p2.Y - top);
        p3 = new Point(p3.X - left, p3.Y - top);
        var right  = Math.Max(Math.Max(p1.X, p2.X), p3.X);
        var bottom = Math.Max(Math.Max(p1.Y, p2.Y), p3.Y);
        return new RibbonBadgeFold(p1, p2, p3, new Size(right, bottom));
    }

    private static StreamGeometry BuildFoldGeometry(RibbonBadgeFold fold)
    {
        var geometry = new StreamGeometry();
        using var context = geometry.Open();
        context.BeginFigure(fold.P1, true);
        context.LineTo(fold.P2);
        context.LineTo(fold.P3);
        context.EndFigure(true);
        return geometry;
    }

    private readonly record struct RibbonBadgeLayout(Rect BodyRect, Point FoldOrigin, RibbonBadgeFold Fold);

    private readonly record struct RibbonBadgeFold(Point P1, Point P2, Point P3, Size Size);
}
