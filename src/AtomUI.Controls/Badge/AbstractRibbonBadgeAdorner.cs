using AtomUI.Controls.Utils;
using AtomUI.Media;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace AtomUI.Controls.Commons;

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

    internal static readonly StyledProperty<Point> BadgeRibbonOffsetProperty =
        AvaloniaProperty.Register<AbstractRibbonBadgeAdorner, Point>(
            nameof(BadgeRibbonOffset));

    internal static readonly StyledProperty<int> BadgeRibbonCornerDarkenAmountProperty =
        AvaloniaProperty.Register<AbstractRibbonBadgeAdorner, int>(
            nameof(BadgeRibbonCornerDarkenAmount));

    internal static readonly StyledProperty<ImmutableTransform?> BadgeRibbonCornerTransformProperty =
        AvaloniaProperty.Register<AbstractRibbonBadgeAdorner, ImmutableTransform?>(
            nameof(BadgeRibbonCornerTransform));

    internal static readonly DirectProperty<AbstractRibbonBadgeAdorner, bool> IsAdornerModeProperty =
        AvaloniaProperty.RegisterDirect<AbstractRibbonBadgeAdorner, bool>(
            nameof(IsAdornerMode),
            o => o.IsAdornerMode,
            (o, v) => o.IsAdornerMode = v);
    
    internal IBrush? RibbonColor
    {
        get => GetValue(RibbonColorProperty);
        set => SetValue(RibbonColorProperty, value);
    }

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

    internal Point BadgeRibbonOffset
    {
        get => GetValue(BadgeRibbonOffsetProperty);
        set => SetValue(BadgeRibbonOffsetProperty, value);
    }

    internal int BadgeRibbonCornerDarkenAmount
    {
        get => GetValue(BadgeRibbonCornerDarkenAmountProperty);
        set => SetValue(BadgeRibbonCornerDarkenAmountProperty, value);
    }

    internal ImmutableTransform? BadgeRibbonCornerTransform
    {
        get => GetValue(BadgeRibbonCornerTransformProperty);
        set => SetValue(BadgeRibbonCornerTransformProperty, value);
    }
    
    #endregion
    
    private TextBlock? _labelText;
    private Color? _cornerBrushSourceColor;
    private int _cornerBrushDarkenAmount;
    private IBrush? _cornerBrush;
    private Size _arrangedSize;
    private readonly BorderRenderHelper _borderRenderHelper;

    static AbstractRibbonBadgeAdorner()
    {
        AffectsMeasure<AbstractRibbonBadgeAdorner>(TextProperty, IsAdornerModeProperty,
            PlacementProperty, BadgeRibbonOffsetProperty, BadgeRibbonCornerTransformProperty);
        AffectsArrange<AbstractRibbonBadgeAdorner>(OffsetProperty, PlacementProperty,
            BadgeRibbonOffsetProperty, BadgeRibbonCornerTransformProperty);
        AffectsRender<AbstractRibbonBadgeAdorner>(RibbonColorProperty, OffsetProperty,
            PlacementProperty, BadgeRibbonOffsetProperty, BadgeRibbonCornerDarkenAmountProperty,
            BadgeRibbonCornerTransformProperty);
    }

    public AbstractRibbonBadgeAdorner()
    {
        _borderRenderHelper = new BorderRenderHelper();
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
            : BadgeRibbonOffset.Y + size.Height + foldSize.Height;
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
        var darkenAmount = BadgeRibbonCornerDarkenAmount;
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
            offsetY += BadgeRibbonOffset.Y + Offset.Y;
            var targetWidth = finalSize.Width > 0 ? finalSize.Width : DesiredSize.Width;
            if (Placement == RibbonBadgePlacement.End)
            {
                offsetX = targetWidth - bodySize.Width + BadgeRibbonOffset.X + Offset.X;
            }
            else
            {
                offsetX = -BadgeRibbonOffset.X + Offset.X;
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
        var width  = Math.Max(0, BadgeRibbonOffset.X);
        var height = Math.Max(0, BadgeRibbonOffset.Y);
        var transform = BadgeRibbonCornerTransform?.Value ?? Matrix.Identity;
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
