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

public class BorderBeam : ContentControl,
                          IMotionAwareControl
{
    #region 公共属性定义

    public static readonly StyledProperty<Color?> ColorProperty =
        AvaloniaProperty.Register<BorderBeam, Color?>(nameof(Color));

    public static readonly DirectProperty<BorderBeam, AvaloniaList<BorderBeamColorStop>> ColorStopsProperty =
        AvaloniaProperty.RegisterDirect<BorderBeam, AvaloniaList<BorderBeamColorStop>>(
            nameof(ColorStops),
            o => o.ColorStops,
            (o, v) => o.ColorStops = v);

    public static readonly StyledProperty<Thickness?> OutsetProperty =
        AvaloniaProperty.Register<BorderBeam, Thickness?>(nameof(Outset));

    public new static readonly StyledProperty<Thickness> BorderThicknessProperty =
        Border.BorderThicknessProperty.AddOwner<BorderBeam>();

    public new static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        Border.CornerRadiusProperty.AddOwner<BorderBeam>();

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<BorderBeam>(
            new StyledPropertyMetadata<bool>(true, BindingMode.OneWay));

    public static readonly StyledProperty<TimeSpan> DurationProperty =
        AvaloniaProperty.Register<BorderBeam, TimeSpan>(nameof(Duration), TimeSpan.FromSeconds(6));

    public static readonly StyledProperty<double> BeamSizeProperty =
        AvaloniaProperty.Register<BorderBeam, double>(nameof(BeamSize), 100d);

    public static readonly StyledProperty<int> CountProperty =
        AvaloniaProperty.Register<BorderBeam, int>(nameof(Count), 1);

    public Color? Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    private AvaloniaList<BorderBeamColorStop> _colorStops = new();
    private IDisposable _colorStopsSubscription;

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

    public Thickness? Outset
    {
        get => GetValue(OutsetProperty);
        set => SetValue(OutsetProperty, value);
    }

    public new Thickness BorderThickness
    {
        get => GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    public new CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    public TimeSpan Duration
    {
        get => GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    public double BeamSize
    {
        get => GetValue(BeamSizeProperty);
        set => SetValue(BeamSizeProperty, value);
    }

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

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ConfigureAwareContent(Content);
        ConfigureEffectiveBorderBeamGeometry();
        UpdatePaintTokenSubscription();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ConfigureAwareContent(null);
        _paintTokens?.Dispose();
        _paintTokens = null;
        base.OnDetachedFromVisualTree(e);
    }

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
