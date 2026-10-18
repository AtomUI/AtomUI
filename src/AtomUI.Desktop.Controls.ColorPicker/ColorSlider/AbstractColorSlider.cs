using AtomUI.Desktop.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using Thumb = AtomUI.Controls.Primitives.Thumb;

namespace AtomUI.Desktop.Controls;

internal abstract class AbstractColorSlider : RangeBase
{
    #region 公共属性定义

    internal static readonly StyledProperty<IBrush?> TransparentBgIntervalColorProperty =
        AvaloniaProperty.Register<AbstractColorSlider, IBrush?>(nameof(TransparentBgIntervalColor));

    internal static readonly StyledProperty<double> TransparentBgSizeProperty =
        AvaloniaProperty.Register<AbstractColorSlider, double>(nameof(TransparentBgSize), 4.0);

    internal IBrush? TransparentBgIntervalColor
    {
        get => GetValue(TransparentBgIntervalColorProperty);
        set => SetValue(TransparentBgIntervalColorProperty, value);
    }

    internal double TransparentBgSize
    {
        get => GetValue(TransparentBgSizeProperty);
        set => SetValue(TransparentBgSizeProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<AbstractColorSlider, IBrush?> TransparentBgBrushProperty =
        AvaloniaProperty.RegisterDirect<AbstractColorSlider, IBrush?>(
            nameof(TransparentBgBrush),
            o => o.TransparentBgBrush,
            (o, v) => o.TransparentBgBrush = v);

    internal static readonly DirectProperty<AbstractColorSlider, double> ThumbSizeProperty =
        AvaloniaProperty.RegisterDirect<AbstractColorSlider, double>(
            nameof(ThumbSize),
            o => o.ThumbSize,
            (o, v) => o.ThumbSize = v);

    private IBrush? _transparentBgBrush;

    internal IBrush? TransparentBgBrush
    {
        get => _transparentBgBrush;
        set => SetAndRaise(TransparentBgBrushProperty, ref _transparentBgBrush, value);
    }

    private double _thumbSize = 0.0d;

    internal double ThumbSize
    {
        get => _thumbSize;
        set => SetAndRaise(ThumbSizeProperty, ref _thumbSize, value);
    }

    #endregion

    protected internal bool IsDragging;
    protected bool IgnorePropertyChanged = false;
    internal const double Tolerance = 0.0001;
    private IDisposable? _pointerMovedDispose;

    // Slider required parts
    protected internal AbstractColorPickerSliderTrack? Track;

    static AbstractColorSlider()
    {
        PressedMixin.Attach<AbstractColorSlider>();
        FocusableProperty.OverrideDefaultValue<AbstractColorSlider>(true);
        Thumb.DragStartedEvent.AddClassHandler<ColorSlider>((x, e) => x.NotifyThumbDragStarted(e), RoutingStrategies.Bubble);
        Thumb.DragCompletedEvent.AddClassHandler<ColorSlider>((x, e) => x.NotifyThumbDragCompleted(e),
            RoutingStrategies.Bubble);
    }

    protected void ConfigureCornerRadius(Size size)
    {
        CornerRadius = new CornerRadius(size.Height / 2);
    }

    protected override void UpdateDataValidation(
        AvaloniaProperty property,
        BindingValueType state,
        Exception? error)
    {
        if (property == ValueProperty)
        {
            DataValidationErrors.SetError(this, error);
        }
    }

    protected virtual void NotifyThumbDragStarted(VectorEventArgs e)
    {
        IsDragging = true;
    }

    protected virtual void NotifyThumbDragCompleted(VectorEventArgs e)
    {
        IsDragging = false;
    }

    protected void TrackReleased(object? sender, PointerReleasedEventArgs e)
    {
        IsDragging = false;
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        ConfigureCornerRadius(e.NewSize);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == TransparentBgIntervalColorProperty ||
                change.Property == TransparentBgSizeProperty)
            {
                if (TransparentBgIntervalColor != null && TransparentBgIntervalColor is ISolidColorBrush solidColorBrush)
                {
                    TransparentBgBrush = TransparentBgBrushUtils.Build(TransparentBgSize, solidColorBrush.Color);
                }
            }
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _pointerMovedDispose?.Dispose();
        _pointerMovedDispose = this.AddDisposableHandler(PointerMovedEvent, TrackMoved, RoutingStrategies.Tunnel);
        if (TransparentBgIntervalColor != null && TransparentBgIntervalColor is ISolidColorBrush solidColorBrush)
        {
            TransparentBgBrush = TransparentBgBrushUtils.Build(TransparentBgSize, solidColorBrush.Color);
        }
    }

    protected virtual void TrackMoved(object? sender, PointerEventArgs e)
    {
        if (!IsEnabled)
        {
            IsDragging = false;
            return;
        }
        if (IsDragging)
        {
            MoveToPoint(e.GetCurrentPoint(Track));
        }
    }

    protected virtual void MoveToPoint(PointerPoint posOnTrack)
    {
        if (Track is null)
        {
            return;
        }

        var thumbLength = ThumbSize + double.Epsilon;
        var trackLength = Track.Bounds.Width - thumbLength;
        var trackPos = posOnTrack.Position.X;
        var logicalPos = Math.Clamp((trackPos - thumbLength * 0.5) / trackLength, 0.0d, 1.0d);
        var calcVal = Math.Abs(-logicalPos);
        var range = Maximum - Minimum;
        var finalValue = calcVal * range + Minimum;

        SetCurrentValue(ValueProperty, finalValue);
    }

    protected virtual void TrackPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            MoveToPoint(e.GetCurrentPoint(Track));
            IsDragging = true;
        }
    }
}
