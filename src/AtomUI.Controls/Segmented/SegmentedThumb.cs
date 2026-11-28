using AtomUI.Controls.Primitives;
using AtomUI.Media;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AtomUI.Controls.Commons;

/// <summary>
/// Renders the sliding selection indicator inside a segmented control template.
/// </summary>
public sealed class SegmentedThumb : Control
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="Background" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        AvaloniaProperty.Register<SegmentedThumb, IBrush?>(nameof(Background));

    /// <summary>
    /// Identifies the <see cref="BorderBrush" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> BorderBrushProperty =
        AvaloniaProperty.Register<SegmentedThumb, IBrush?>(nameof(BorderBrush));

    /// <summary>
    /// Identifies the <see cref="BorderThickness" /> property.
    /// </summary>
    public static readonly StyledProperty<Thickness> BorderThicknessProperty =
        AvaloniaProperty.Register<SegmentedThumb, Thickness>(nameof(BorderThickness));

    /// <summary>
    /// Identifies the <see cref="CornerRadius" /> property.
    /// </summary>
    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<SegmentedThumb, CornerRadius>(nameof(CornerRadius));

    /// <summary>
    /// Identifies the <see cref="BoxShadows" /> property.
    /// </summary>
    public static readonly StyledProperty<BoxShadows> BoxShadowsProperty =
        AvaloniaProperty.Register<SegmentedThumb, BoxShadows>(nameof(BoxShadows));

    /// <summary>
    /// Identifies the <see cref="ThumbBounds" /> property.
    /// </summary>
    public static readonly StyledProperty<Rect> ThumbBoundsProperty =
        AvaloniaProperty.Register<SegmentedThumb, Rect>(nameof(ThumbBounds));

    /// <summary>
    /// Identifies the <see cref="IsThumbVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsThumbVisibleProperty =
        AvaloniaProperty.Register<SegmentedThumb, bool>(nameof(IsThumbVisible));

    /// <summary>
    /// Fill brush of the selection thumb.
    /// </summary>
    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    /// <summary>
    /// Border brush drawn along the inner edge of the selection thumb when <see cref="BorderThickness" /> is positive.
    /// </summary>
    public IBrush? BorderBrush
    {
        get => GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    /// <summary>
    /// Border thickness drawn along the inner edge of the selection thumb.
    /// </summary>
    public Thickness BorderThickness
    {
        get => GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    /// <summary>
    /// Corner radius of the selection thumb, clamped to half of its shorter side while rendering.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <summary>
    /// Box shadows painted behind the selection thumb.
    /// </summary>
    public BoxShadows BoxShadows
    {
        get => GetValue(BoxShadowsProperty);
        set => SetValue(BoxShadowsProperty, value);
    }

    /// <summary>
    /// Bounding rectangle of the active selection thumb in this element's coordinate space.
    /// </summary>
    public Rect ThumbBounds
    {
        get => GetValue(ThumbBoundsProperty);
        set => SetValue(ThumbBoundsProperty, value);
    }

    /// <summary>
    /// Indicates whether a selection thumb is currently visible.
    /// </summary>
    public bool IsThumbVisible
    {
        get => GetValue(IsThumbVisibleProperty);
        set => SetValue(IsThumbVisibleProperty, value);
    }

    #endregion

    private IPen? _borderPen;

    static SegmentedThumb()
    {
        AffectsRender<SegmentedThumb>(
            BackgroundProperty,
            BorderBrushProperty,
            BorderThicknessProperty,
            CornerRadiusProperty,
            BoxShadowsProperty,
            ThumbBoundsProperty,
            IsThumbVisibleProperty);
    }

    /// <summary>
    /// Draws the selection thumb fill, shadows and optional inner border within <see cref="ThumbBounds" /> while
    /// <see cref="IsThumbVisible" /> is <see langword="true" />.
    /// </summary>
    public override void Render(DrawingContext context)
    {
        if (!IsThumbVisible)
        {
            return;
        }

        var bounds = ThumbBounds;
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return;
        }

        var clampedRadius = CornerRadiusUtils.ClampToSize(CornerRadius, bounds.Width, bounds.Height);
        context.DrawRectangle(Background, null, new RoundedRect(bounds, clampedRadius), BoxShadows);

        var borderBrush = BorderBrush;
        var borderThickness = BorderThickness;
        if (borderBrush is null ||
            (borderThickness.Left <= 0 && borderThickness.Top <= 0 &&
             borderThickness.Right <= 0 && borderThickness.Bottom <= 0))
        {
            return;
        }

        if (borderThickness.IsUniform)
        {
            var strokeWidth = borderThickness.Left;
            var strokeRect = bounds.Deflate(strokeWidth / 2);
            if (strokeRect.Width > 0 && strokeRect.Height > 0)
            {
                var innerRadius = new CornerRadius(
                    Math.Max(0, clampedRadius.TopLeft - strokeWidth / 2),
                    Math.Max(0, clampedRadius.TopRight - strokeWidth / 2),
                    Math.Max(0, clampedRadius.BottomRight - strokeWidth / 2),
                    Math.Max(0, clampedRadius.BottomLeft - strokeWidth / 2));
                var clampedInnerRadius = CornerRadiusUtils.ClampToSize(innerRadius, strokeRect.Width, strokeRect.Height);
                PenUtils.TryModifyOrCreate(ref _borderPen, borderBrush, strokeWidth);
                context.DrawRectangle(null, _borderPen, new RoundedRect(strokeRect, clampedInnerRadius));
            }
            return;
        }

        if (borderThickness.Top > 0)
        {
            var y = bounds.Top + borderThickness.Top / 2;
            context.DrawLine(new Pen(borderBrush, borderThickness.Top), new Point(bounds.Left, y), new Point(bounds.Right, y));
        }
        if (borderThickness.Bottom > 0)
        {
            var y = bounds.Bottom - borderThickness.Bottom / 2;
            context.DrawLine(new Pen(borderBrush, borderThickness.Bottom), new Point(bounds.Left, y), new Point(bounds.Right, y));
        }
        if (borderThickness.Left > 0)
        {
            var x = bounds.Left + borderThickness.Left / 2;
            context.DrawLine(new Pen(borderBrush, borderThickness.Left), new Point(x, bounds.Top), new Point(x, bounds.Bottom));
        }
        if (borderThickness.Right > 0)
        {
            var x = bounds.Right - borderThickness.Right / 2;
            context.DrawLine(new Pen(borderBrush, borderThickness.Right), new Point(x, bounds.Top), new Point(x, bounds.Bottom));
        }
    }
}
