using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace AtomUI.Controls.Commons;

/// <summary>
/// Draws a single Separator rail line segment. Two instances (start and end) are arranged around the
/// title by <see cref="AbstractSeparator" />; each instance renders one horizontal or vertical line
/// across its own bounds, honoring the separator variant (solid, dotted, dashed) and render scale.
/// </summary>
public class SeparatorRail : Control
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="LineColor" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> LineColorProperty =
        AvaloniaProperty.Register<SeparatorRail, IBrush?>(nameof(LineColor));

    /// <summary>
    /// Identifies the <see cref="LineWidth" /> property.
    /// </summary>
    public static readonly StyledProperty<double> LineWidthProperty =
        AvaloniaProperty.Register<SeparatorRail, double>(nameof(LineWidth), 1);

    /// <summary>
    /// Identifies the <see cref="Variant" /> property.
    /// </summary>
    public static readonly StyledProperty<SeparatorVariant> VariantProperty =
        AvaloniaProperty.Register<SeparatorRail, SeparatorVariant>(nameof(Variant), SeparatorVariant.Solid);

    /// <summary>
    /// Identifies the <see cref="Orientation" /> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<SeparatorRail, Orientation>(nameof(Orientation));

    /// <summary>
    /// Brush that paints this rail. The separator's template binds the value to its own <c>LineColor</c>; an
    /// explicit <see langword="null" /> leaves the rail unpainted.
    /// </summary>
    public IBrush? LineColor
    {
        get => GetValue(LineColorProperty);
        set => SetValue(LineColorProperty, value);
    }

    /// <summary>
    /// Design thickness of the line in DIPs, 1 by default. The rail scales it by the render scaling before drawing.
    /// The separator's template binds the value to its own <c>LineWidth</c>.
    /// </summary>
    public double LineWidth
    {
        get => GetValue(LineWidthProperty);
        set => SetValue(LineWidthProperty, value);
    }

    /// <summary>
    /// Line style of the rail, <see cref="SeparatorVariant.Solid" /> by default. The separator's template binds the
    /// value to its own <c>Variant</c>.
    /// </summary>
    public SeparatorVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    /// <summary>
    /// Direction of the line: across the width when <see cref="Avalonia.Layout.Orientation.Horizontal" />, the
    /// default, and down the height when <see cref="Avalonia.Layout.Orientation.Vertical" />, each at the center of
    /// the bounds. The separator's template binds the value to its own <c>Orientation</c>.
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    #endregion

    private Pen? _cachedLinePen;
    private SeparatorVariant _cachedVariant;
    private IBrush? _cachedLineColor;
    private double _cachedLineWidth;
    private static ImmutableDashStyle? s_dash;
    private static ImmutableDashStyle? s_dot;

    static SeparatorRail()
    {
        AffectsRender<SeparatorRail>(LineColorProperty,
            LineWidthProperty,
            VariantProperty,
            OrientationProperty,
            UseLayoutRoundingProperty);
    }

    /// <summary>
    /// Drops the cached pen when <see cref="LineColor" />, <see cref="LineWidth" /> or <see cref="Variant" />
    /// changes, so that the next render builds a new one. Derived overrides call the base member for that
    /// invalidation.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == LineColorProperty ||
            change.Property == LineWidthProperty ||
            change.Property == VariantProperty)
        {
            _cachedLinePen = null;
        }
    }

    /// <summary>
    /// Draws one aliased line through the center of the bounds, across the width or down the height by
    /// <see cref="Orientation" />, with the pen built from <see cref="LineColor" />, <see cref="LineWidth" /> and
    /// <see cref="Variant" />. Nothing is drawn while the bounds are empty.
    /// </summary>
    public override void Render(DrawingContext context)
    {
        var bounds = Bounds.Size;
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return;
        }

        using var state = context.PushRenderOptions(new RenderOptions
        {
            EdgeMode = EdgeMode.Aliased
        });

        var linePen = GetOrCreateLinePen();
        if (Orientation == Orientation.Horizontal)
        {
            var offsetY = bounds.Height / 2.0;
            context.DrawLine(linePen, new Point(0, offsetY), new Point(bounds.Width, offsetY));
        }
        else
        {
            var offsetX = bounds.Width / 2.0;
            context.DrawLine(linePen, new Point(offsetX, 0), new Point(offsetX, bounds.Height));
        }
    }

    /// <summary>
    /// Dash pattern of a <see cref="SeparatorVariant.Dashed" /> rail: dashes of 4 and gaps of 2, each a multiple of
    /// <see cref="LineWidth" />, with no phase offset.
    /// </summary>
    public static IDashStyle DashStyle => s_dash ??= new ImmutableDashStyle([4, 2], 0);

    /// <summary>
    /// Dash pattern of a <see cref="SeparatorVariant.Dotted" /> rail: dashes of 1 and gaps of 1, each a multiple of
    /// <see cref="LineWidth" />, with no phase offset.
    /// </summary>
    public static IDashStyle DotStyle => s_dot ??= new ImmutableDashStyle([1, 1], 0);

    private Pen GetOrCreateLinePen()
    {
        var variant   = Variant;
        var lineColor = LineColor;
        var lineWidth = BorderUtils.BuildRenderScaleAwareThickness(this, LineWidth);
        if (_cachedLinePen is not null &&
            _cachedVariant == variant &&
            ReferenceEquals(_cachedLineColor, lineColor) &&
            MathUtils.AreClose(_cachedLineWidth, lineWidth))
        {
            return _cachedLinePen;
        }

        IDashStyle? lineStyle = variant switch
        {
            SeparatorVariant.Dashed => DashStyle,
            SeparatorVariant.Dotted => DotStyle,
            _                       => null
        };
        _cachedLinePen   = new Pen(lineColor, lineWidth, lineStyle);
        _cachedVariant   = variant;
        _cachedLineColor = lineColor;
        _cachedLineWidth = lineWidth;
        return _cachedLinePen;
    }
}
