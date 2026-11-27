using System.Collections.Specialized;
using System.Globalization;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Draws a light beam that travels along a ring around its content, on top of it and without taking pointer
/// input. The ring comes from a content that implements <see cref="IBorderBeamAwareControl" />; for any other
/// content it is given by the <see cref="BorderThickness" /> and <see cref="CornerRadius" /> of this control.
/// </summary>
public class BorderBeam : ContentControl,
                          IMotionAwareControl
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="Color" /> property.
    /// </summary>
    public static readonly StyledProperty<Color?> ColorProperty =
        AvaloniaProperty.Register<BorderBeam, Color?>(nameof(Color));

    /// <summary>
    /// Identifies the <see cref="ColorStops" /> property.
    /// </summary>
    public static readonly DirectProperty<BorderBeam, AvaloniaList<BorderBeamColorStop>> ColorStopsProperty =
        AvaloniaProperty.RegisterDirect<BorderBeam, AvaloniaList<BorderBeamColorStop>>(
            nameof(ColorStops),
            o => o.ColorStops,
            (o, v) => o.ColorStops = v);

    /// <summary>
    /// Identifies the <see cref="Outset" /> property.
    /// </summary>
    public static readonly StyledProperty<Thickness?> OutsetProperty =
        AvaloniaProperty.Register<BorderBeam, Thickness?>(nameof(Outset));

    /// <summary>
    /// Identifies the <see cref="BorderThickness" /> property.
    /// </summary>
    public new static readonly StyledProperty<Thickness> BorderThicknessProperty =
        Border.BorderThicknessProperty.AddOwner<BorderBeam>();

    /// <summary>
    /// Identifies the <see cref="CornerRadius" /> property.
    /// </summary>
    public new static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        Border.CornerRadiusProperty.AddOwner<BorderBeam>();

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<BorderBeam>(
            new StyledPropertyMetadata<bool>(true, BindingMode.OneWay));

    /// <summary>
    /// Identifies the <see cref="Duration" /> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> DurationProperty =
        AvaloniaProperty.Register<BorderBeam, TimeSpan>(nameof(Duration), TimeSpan.FromSeconds(6));

    /// <summary>
    /// Identifies the <see cref="BeamSize" /> property.
    /// </summary>
    public static readonly StyledProperty<double> BeamSizeProperty =
        AvaloniaProperty.Register<BorderBeam, double>(nameof(BeamSize), 100d);

    /// <summary>
    /// Identifies the <see cref="Count" /> property.
    /// </summary>
    public static readonly StyledProperty<int> CountProperty =
        AvaloniaProperty.Register<BorderBeam, int>(nameof(Count), 1);

    /// <summary>
    /// Solid color of the beam. It is ignored while <see cref="ColorStops" /> holds at least one stop.
    /// When it is <see langword="null" /> and <see cref="ColorStops" /> is empty, the beam takes the primary
    /// color of the active design language as start color and its hover variant as end color.
    /// </summary>
    public Color? Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    private AvaloniaList<BorderBeamColorStop> _colorStops = new();
    private IDisposable _colorStopsSubscription;

    /// <summary>
    /// Gradient stops of the beam, each with a color and a position in percent. A list with at least one stop
    /// replaces <see cref="Color" />; an empty list falls back to it. Setting <see langword="null" /> stores an
    /// empty list. Adding, removing or replacing stops, or changing a stop that is in the list, repaints the beam.
    /// </summary>
    public AvaloniaList<BorderBeamColorStop> ColorStops
    {
        get => _colorStops;
        set
        {
            value ??= new AvaloniaList<BorderBeamColorStop>();
            if (ReferenceEquals(_colorStops, value))
            {
                return;
            }

            _colorStopsSubscription.Dispose();
            _colorStopsSubscription = value.WeakSubscribe(HandleColorStopsChanged);
            SetAndRaise(ColorStopsProperty, ref _colorStops, value);
        }
    }

    /// <summary>
    /// Moves the painted ring outward by this amount per side, in device-independent units. Only the paint is
    /// moved; the layout size of the control does not change. <see langword="null" /> or zero keeps the ring on the
    /// bounds of the control.
    /// </summary>
    public Thickness? Outset
    {
        get => GetValue(OutsetProperty);
        set => SetValue(OutsetProperty, value);
    }

    /// <summary>
    /// Width of the ring the beam is painted on, per side. The beam is clipped to the area between the outer and
    /// the inner border edge, so a side with zero width shows no beam. The value is used only while the content
    /// does not implement <see cref="IBorderBeamAwareControl" />, because such a content supplies the geometry.
    /// </summary>
    public new Thickness BorderThickness
    {
        get => GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    /// <summary>
    /// Radius of the ring the beam is painted on, per corner. The value is used only while the content does not
    /// implement <see cref="IBorderBeamAwareControl" />, because such a content supplies the geometry.
    /// </summary>
    public new CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <summary>
    /// Controls whether the beam is shown. When <see langword="true" />, the beam is painted and, with a positive
    /// <see cref="Duration" />, travels around the border in an endless loop. When <see langword="false" />, nothing
    /// is painted, not even a motionless beam.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// Time of one loop of the beam around the border; the loop repeats without a pause. A value of zero or less
    /// keeps the beam from travelling, and it is painted at a fixed position. The theme sets this property from
    /// <see cref="BorderBeamToken.MotionDuration" />.
    /// </summary>
    public TimeSpan Duration
    {
        get => GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    /// <summary>
    /// Length of one beam along the border path, in device-independent units. A value of zero or less draws no
    /// beam. The theme sets this property from <see cref="BorderBeamToken.BeamSize" />.
    /// </summary>
    public double BeamSize
    {
        get => GetValue(BeamSizeProperty);
        set => SetValue(BeamSizeProperty, value);
    }

    /// <summary>
    /// Number of beams that travel around the border at the same time, spaced evenly along it. A value below 1
    /// is treated as 1.
    /// </summary>
    public int Count
    {
        get => GetValue(CountProperty);
        set => SetValue(CountProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<BorderBeam, BorderBeamGeometry> EffectiveBorderBeamGeometryProperty =
        AvaloniaProperty.RegisterDirect<BorderBeam, BorderBeamGeometry>(
            nameof(EffectiveBorderBeamGeometry),
            o => o.EffectiveBorderBeamGeometry,
            (o, v) => o.EffectiveBorderBeamGeometry = v);

    internal static readonly StyledProperty<bool> UseBeamPaintTokenResourcesProperty =
        AvaloniaProperty.Register<BorderBeam, bool>(nameof(UseBeamPaintTokenResources));

    internal static readonly DirectProperty<BorderBeam, BorderBeamPaint> EffectivePaintProperty =
        AvaloniaProperty.RegisterDirect<BorderBeam, BorderBeamPaint>(nameof(EffectivePaint), o => o.EffectivePaint);

    private BorderBeamGeometry _effectiveBorderBeamGeometry;

    internal BorderBeamGeometry EffectiveBorderBeamGeometry
    {
        get => _effectiveBorderBeamGeometry;
        set => SetAndRaise(EffectiveBorderBeamGeometryProperty, ref _effectiveBorderBeamGeometry, value);
    }

    internal bool UseBeamPaintTokenResources
    {
        get => GetValue(UseBeamPaintTokenResourcesProperty);
        set => SetValue(UseBeamPaintTokenResourcesProperty, value);
    }

    internal BorderBeamPaint EffectivePaint => _effectivePaint;

    #endregion

    private IBorderBeamAwareControl? _awareContent;
    private BorderBeamPaint _effectivePaint = BorderBeamPaint.Default;
    private OwnerTokenSet? _paintTokens;

    private static readonly object[] s_paintKeys =
    [
        ControlTokenResourceKey.Global(BorderBeamTokens.Identity, SharedTokenKind.ColorPrimary),
        ControlTokenResourceKey.Global(BorderBeamTokens.Identity, SharedTokenKind.ColorPrimaryHover),
        BorderBeamTokenKind.BeamOpacity,
        BorderBeamTokenKind.MaxVisibleStopPercent
    ];

    static BorderBeam()
    {
        AffectsRender<BorderBeam>(
            ColorProperty,
            ColorStopsProperty,
            OutsetProperty,
            BorderThicknessProperty,
            CornerRadiusProperty,
            IsMotionEnabledProperty,
            DurationProperty,
            BeamSizeProperty);
    }

    public BorderBeam()
    {
        _colorStopsSubscription = _colorStops.WeakSubscribe(HandleColorStopsChanged);
        ConfigureEffectiveBorderBeamGeometry();
    }

    /// <summary>
    /// Called when the control joins a visual tree. The base implementation is called first. After that the beam
    /// follows the border geometry of a content that implements <see cref="IBorderBeamAwareControl" />, and the
    /// fallback colors and the opacity of the beam are read from the theme tokens the style enabled. An override
    /// in a derived class must call this implementation.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ConfigureAwareContent(Content);
        ConfigureEffectiveBorderBeamGeometry();
        UpdatePaintTokenSubscription();
    }

    /// <summary>
    /// Called when the control leaves a visual tree. It stops following the border geometry of the content and
    /// disposes the theme token subscription, and then calls the base implementation. An override in a derived
    /// class must call this implementation.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ConfigureAwareContent(null);
        _paintTokens?.Dispose();
        _paintTokens = null;
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Called when a property of the control changes. The base implementation is called first. A new
    /// <see cref="ContentControl.Content" /> becomes the geometry source of the beam when it implements
    /// <see cref="IBorderBeamAwareControl" />. A change of <see cref="ContentControl.Content" />,
    /// <see cref="BorderThickness" /> or <see cref="CornerRadius" /> recomputes the border geometry the beam
    /// follows. While the control is attached, a change of <c>UseBeamPaintTokenResources</c> updates the theme
    /// token subscription. An override in a derived class must call this implementation.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ContentProperty)
        {
            ConfigureAwareContent(change.NewValue);
            ConfigureEffectiveBorderBeamGeometry();
        }
        else if (change.Property == BorderThicknessProperty ||
                 change.Property == CornerRadiusProperty)
        {
            ConfigureEffectiveBorderBeamGeometry();
        }
        else if (change.Property == UseBeamPaintTokenResourcesProperty && this.IsAttachedToVisualTree())
        {
            UpdatePaintTokenSubscription();
        }
    }

    private void UpdatePaintTokenSubscription()
    {
        if (UseBeamPaintTokenResources && _paintTokens is null)
        {
            _paintTokens = new OwnerTokenSet(this, s_paintKeys.Length, RefreshPaint);
            for (var index = 0; index < s_paintKeys.Length; ++index)
            {
                _paintTokens.SetResourceKey(index, s_paintKeys[index]);
            }
        }
        else if (!UseBeamPaintTokenResources && _paintTokens is not null)
        {
            _paintTokens.Dispose();
            _paintTokens = null;
        }
        RefreshPaint();
    }

    private void RefreshPaint()
    {
        var paint = new BorderBeamPaint(
            ReadBrush(_paintTokens?.GetValue(0), EffectivePaint.StartColor),
            ReadBrush(_paintTokens?.GetValue(1), EffectivePaint.EndColor),
            ReadNumber(_paintTokens?.GetValue(2), EffectivePaint.Opacity, BorderBeamPaint.Default.Opacity),
            ReadNumber(_paintTokens?.GetValue(3), EffectivePaint.MaxVisibleStopPercent, BorderBeamPaint.Default.MaxVisibleStopPercent));
        SetAndRaise(EffectivePaintProperty, ref _effectivePaint, paint);
    }

    private static IBrush? ReadBrush(object? value, IBrush? previous) => value switch
    {
        IBrush brush => brush,
        Color color => new ImmutableSolidColorBrush(color),
        string text when Avalonia.Media.Color.TryParse(text, out var color) => new ImmutableSolidColorBrush(color),
        _ when ReferenceEquals(value, BindingOperations.DoNothing) => previous,
        _ => null
    };

    private static double ReadNumber(object? value, double previous, double fallback)
    {
        if (value is double number)
        {
            return number;
        }
        if (ReferenceEquals(value, BindingOperations.DoNothing))
        {
            return previous;
        }
        try
        {
            return value is IConvertible convertible ? convertible.ToDouble(CultureInfo.CurrentCulture) : fallback;
        }
        catch (Exception error) when (error is FormatException or InvalidCastException or OverflowException)
        {
            return fallback;
        }
    }

    private void ConfigureAwareContent(object? content)
    {
        if (ReferenceEquals(_awareContent, content))
        {
            return;
        }

        if (_awareContent is not null)
        {
            _awareContent.BorderBeamGeometryChanged -= HandleAwareContentGeometryChanged;
        }

        _awareContent = content as IBorderBeamAwareControl;
        if (_awareContent is not null)
        {
            _awareContent.BorderBeamGeometryChanged += HandleAwareContentGeometryChanged;
        }
    }

    private void ConfigureEffectiveBorderBeamGeometry()
    {
        EffectiveBorderBeamGeometry = _awareContent?.GetBorderBeamGeometry()
                                      ?? new BorderBeamGeometry(BorderThickness, CornerRadius);
    }

    private void HandleAwareContentGeometryChanged(object? sender, EventArgs e)
    {
        ConfigureEffectiveBorderBeamGeometry();
    }

    private void HandleColorStopsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RaisePropertyChanged(ColorStopsProperty, ColorStops, ColorStops);
    }
}
