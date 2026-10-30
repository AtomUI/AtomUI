using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace AtomUI.Controls.Commons;

/// <summary>
/// Draws a single Separator rail line segment. Two instances (start and end) are arranged around the
/// title by <see cref="AbstractSeparator"/>; each instance renders one horizontal or vertical line
/// across its own bounds, honoring the separator variant (solid, dotted, dashed) and render scale.
/// </summary>
public class SeparatorRail : Control
{
    #region 公共属性定义

    public static readonly StyledProperty<IBrush?> LineColorProperty =
        AvaloniaProperty.Register<SeparatorRail, IBrush?>(nameof(LineColor));

    public static readonly StyledProperty<double> LineWidthProperty =
        AvaloniaProperty.Register<SeparatorRail, double>(nameof(LineWidth), 1);

    public static readonly StyledProperty<SeparatorVariant> VariantProperty =
        AvaloniaProperty.Register<SeparatorRail, SeparatorVariant>(nameof(Variant), SeparatorVariant.Solid);

    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<SeparatorRail, Orientation>(nameof(Orientation));

    /// <summary>
    /// Gets or sets the brush used to paint the rail line.
    /// </summary>
    public IBrush? LineColor
    {
        get => GetValue(LineColorProperty);
        set => SetValue(LineColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the design thickness of the rail line; the actual rendered thickness is adjusted
    /// to the current render scale.
    /// </summary>
    public double LineWidth
    {
        get => GetValue(LineWidthProperty);
        set => SetValue(LineWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the rail line variant (solid, dotted, or dashed).
    /// </summary>
    public SeparatorVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    /// <summary>
    /// Gets or sets the orientation of the rail line.
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

    public static IDashStyle DashStyle => s_dash ??= new ImmutableDashStyle([4, 2], 0);

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
