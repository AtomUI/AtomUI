using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Thumb = AtomUI.Controls.Primitives.Thumb;

namespace AtomUI.Desktop.Controls;

internal class GradientColorSlider : AbstractColorSlider
{
    #region 公共属性定义

    public static readonly StyledProperty<LinearGradientBrush?> GradientValueProperty =
        AvaloniaProperty.Register<GradientColorSlider, LinearGradientBrush?>(nameof(GradientValue));

    public LinearGradientBrush? GradientValue
    {
        get => GetValue(GradientValueProperty);
        set => SetValue(GradientValueProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<HsvColor> ActivatedHsvValueProperty =
        AvaloniaProperty.Register<GradientColorSlider, HsvColor>(
            nameof(ActivatedHsvValue),
            Colors.White.ToHsv(),
            defaultBindingMode: BindingMode.TwoWay);

    internal static readonly DirectProperty<GradientColorSlider, int?> ActivatedStopIndexProperty =
        AvaloniaProperty.RegisterDirect<GradientColorSlider, int?>(
            nameof(ActivatedStopIndex),
            o => o.ActivatedStopIndex,
            (o, v) => o.ActivatedStopIndex = v);

    internal HsvColor ActivatedHsvValue
    {
        get => GetValue(ActivatedHsvValueProperty);
        set => SetValue(ActivatedHsvValueProperty, value);
    }

    private int? _activatedStopIndex;

    internal int? ActivatedStopIndex
    {
        get => _activatedStopIndex;
        set => SetAndRaise(ActivatedStopIndexProperty, ref _activatedStopIndex, value);
    }

    #endregion

    #region 公共事件定义
    public event EventHandler<GradientColorChangedEventArgs>? GradientValueChanged;
    #endregion

    internal static readonly RoutedEvent<GradientActiveStopChangedEventArgs> GradientActiveStopChangedEvent =
        RoutedEvent.Register<GradientColorSlider, GradientActiveStopChangedEventArgs>(nameof(GradientActiveStopChanged), RoutingStrategies.Bubble);

    public event EventHandler<GradientActiveStopChangedEventArgs>? GradientActiveStopChanged
    {
        add => AddHandler(GradientActiveStopChangedEvent, value);
        remove => RemoveHandler(GradientActiveStopChangedEvent, value);
    }

    private IDisposable? _pressDispose;
    private IDisposable? _releaseDispose;
    private IDisposable? _activatedThumbDispose;
    private bool _ignoreSetActiveThumb;

    static GradientColorSlider()
    {
        Thumb.DragStartedEvent.AddClassHandler<GradientColorSlider>((x, e) => x.NotifyThumbDragStarted(e), RoutingStrategies.Bubble);
        Thumb.DragCompletedEvent.AddClassHandler<GradientColorSlider>((x, e) => x.NotifyThumbDragCompleted(e),
            RoutingStrategies.Bubble);
    }

    protected virtual void NotifyGradientValueChanged(GradientColorChangedEventArgs e)
    {
        GradientValueChanged?.Invoke(this, e);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _pressDispose?.Dispose();
        _releaseDispose?.Dispose();
        _activatedThumbDispose?.Dispose();
        Track = e.NameScope.Find<GradientColorPickerTrack>("PART_Track");

        if (Track is GradientColorPickerTrack track)
        {
            track.IsThumbDragIgnored = true;
            _pressDispose = this.AddDisposableHandler(PointerPressedEvent, TrackPressed, RoutingStrategies.Tunnel);
            _releaseDispose = this.AddDisposableHandler(PointerReleasedEvent, TrackReleased, RoutingStrategies.Tunnel);
            _activatedThumbDispose = track.GetObservable(GradientColorPickerTrack.ActivatedThumbProperty)
                .Subscribe(_ => HandleActivatedThumbChanged());
            if (ActivatedStopIndex != null)
            {
                track.SetActiveThumb(ActivatedStopIndex.Value);
            }
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _activatedThumbDispose?.Dispose();
        _activatedThumbDispose = null;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (Track is GradientColorPickerTrack track)
        {
            _activatedThumbDispose?.Dispose();
            _activatedThumbDispose = track.GetObservable(GradientColorPickerTrack.ActivatedThumbProperty)
                .Subscribe(_ => HandleActivatedThumbChanged());
        }
    }

    private void HandleActivatedThumbChanged()
    {
        if (Track is GradientColorPickerTrack track && track.ActivatedThumb != null)
        {
            SyncValueFromActivatedThumb(track.ActivatedThumb);
        }
    }

    private void SyncValueFromActivatedThumb(GradientColorSliderThumb thumb)
    {
        SetCurrentValue(ActivatedHsvValueProperty, thumb.Color.ToHsv());
    }

    protected override void TrackPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            ColorSliderThumb? thumb = null;
            if (Track is GradientColorPickerTrack track)
            {
                thumb = track.NotifyTrackPressed(e);
            }

            if (thumb != null)
            {
                IsDragging = true;
                _ignoreSetActiveThumb = true;
                SetCurrentValue(ActivatedStopIndexProperty, GetActivatedStopIndex());
            }
        }
    }

    private int? GetActivatedStopIndex()
    {
        if (Track is GradientColorPickerTrack track && track.ActivatedThumb != null)
        {
            var stops = GradientColorPickerTrack.CopyThumbsByValue(track.Thumbs);
            for (int i = 0; i < stops.Count; i++)
            {
                if (MathUtils.AreClose(stops[i].Value, track.ActivatedThumb.Value))
                {
                    return i;
                }
            }
        }

        return null;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == GradientValueProperty)
        {
            NotifyGradientValueChanged(new GradientColorChangedEventArgs(change.GetOldValue<LinearGradientBrush>(), change.GetNewValue<LinearGradientBrush>()));
        }
        else if (change.Property == ActivatedHsvValueProperty)
        {
            if (Track is GradientColorPickerTrack track)
            {
                track.NotifyActivateStopColorChanged(ActivatedHsvValue);
            }
        }
        else if (change.Property == ActivatedStopIndexProperty)
        {
            RaiseEvent(new GradientActiveStopChangedEventArgs(GradientActiveStopChangedEvent, this)
            {
                OldIndex = change.GetOldValue<int?>(),
                NewIndex = change.GetNewValue<int?>()
            });
            if (_ignoreSetActiveThumb)
            {
                _ignoreSetActiveThumb = false;
                return;
            }
            if (Track is GradientColorPickerTrack track)
            {
                if (ActivatedStopIndex != null)
                {
                    track.SetActiveThumb(ActivatedStopIndex.Value);
                }
            }
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var size = base.MeasureOverride(availableSize);
        if (Track is GradientColorPickerTrack track)
        {
            if (track.Thumbs.Count > 0)
            {
                ThumbSize = track.Thumbs[0].DesiredSize.Width;
            }
        }
        return size;
    }

    protected override void TrackMoved(object? sender, PointerEventArgs e)
    {
        base.TrackMoved(sender, e);
        if (IsDragging)
        {
            _ignoreSetActiveThumb = true;
            SetCurrentValue(ActivatedStopIndexProperty, GetActivatedStopIndex());
        }
    }
}
