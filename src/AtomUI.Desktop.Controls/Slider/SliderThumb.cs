using AtomUI.Theme;
using AtomUI.Controls;
using AtomUI.Media;
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The draggable handle of a <see cref="Slider" />, created by the slider track.
/// <see cref="Render" /> paints the circle with its border and outline ring. A pointer drag on the thumb is
/// reported through the <see cref="DragStarted" />, <see cref="DragDelta" /> and <see cref="DragCompleted" />
/// events, which bubble up to the slider.
/// </summary>
[PseudoClasses(StdPseudoClass.Pressed)]
// A Token-owner transparent part: the thumb is created by the Slider track only, so its theme reads the Slider
// TokenResource and its Token-declared transitions resolve to the Slider through the inherited owner.
[TokenOwnerTransparent]
public class SliderThumb : TemplatedControl
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="OutlineBrush" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> OutlineBrushProperty =
        AvaloniaProperty.Register<SliderThumb, IBrush?>(nameof(OutlineBrush));

    /// <summary>
    /// Identifies the <see cref="OutlineThickness" /> property.
    /// </summary>
    public static readonly StyledProperty<Thickness> OutlineThicknessProperty =
        AvaloniaProperty.Register<SliderThumb, Thickness>(nameof(OutlineThickness));

    /// <summary>
    /// Identifies the <see cref="ThumbCircleSize" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ThumbCircleSizeProperty =
        AvaloniaProperty.Register<SliderThumb, double>(nameof(ThumbCircleSize));

    #endregion

    #region 公共事件定义

    /// <summary>
    /// Identifies the <see cref="DragStarted" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<VectorEventArgs> DragStartedEvent =
        RoutedEvent.Register<SliderThumb, VectorEventArgs>(nameof(DragStarted), RoutingStrategies.Bubble);

    /// <summary>
    /// Identifies the <see cref="DragDelta" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<VectorEventArgs> DragDeltaEvent =
        RoutedEvent.Register<SliderThumb, VectorEventArgs>(nameof(DragDelta), RoutingStrategies.Bubble);

    /// <summary>
    /// Identifies the <see cref="DragCompleted" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<VectorEventArgs> DragCompletedEvent =
        RoutedEvent.Register<SliderThumb, VectorEventArgs>(nameof(DragCompleted), RoutingStrategies.Bubble);

    /// <summary>
    /// Raised when the pointer is pressed on an enabled thumb and a drag starts, after the pointer is captured
    /// and the <c>pressed</c> pseudo-class is set. The event bubbles up to the owning slider.
    /// </summary>
    /// <remarks>
    /// <c>Vector</c> is the pointer position relative to the thumb at the press.
    /// </remarks>
    public event EventHandler<VectorEventArgs>? DragStarted
    {
        add => AddHandler(DragStartedEvent, value);
        remove => RemoveHandler(DragStartedEvent, value);
    }

    /// <summary>
    /// Raised on every pointer move while a drag started on this thumb is in progress and the thumb is enabled.
    /// The event bubbles up to the owning slider.
    /// </summary>
    /// <remarks>
    /// <c>Vector</c> is the current pointer position relative to the thumb minus the position recorded at the
    /// drag start; <see cref="AdjustDrag" /> moves the recorded position.
    /// </remarks>
    public event EventHandler<VectorEventArgs>? DragDelta
    {
        add => AddHandler(DragDeltaEvent, value);
        remove => RemoveHandler(DragDeltaEvent, value);
    }

    /// <summary>
    /// Raised when a drag ends, after the drag state of the thumb is cleared: on pointer release while the drag
    /// is in progress, or when the pointer capture is lost during the drag. The event bubbles up to the owning
    /// slider; the <c>pressed</c> pseudo-class is removed after it is raised.
    /// </summary>
    /// <remarks>
    /// <c>Vector</c> is the pointer position relative to the thumb at the release, or the last recorded position
    /// when the pointer capture is lost.
    /// </remarks>
    public event EventHandler<VectorEventArgs>? DragCompleted
    {
        add => AddHandler(DragCompletedEvent, value);
        remove => RemoveHandler(DragCompletedEvent, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<SliderThumb>();

    internal static readonly DirectProperty<SliderThumb, int> HandleIndexProperty =
        AvaloniaProperty.RegisterDirect<SliderThumb, int>(
            nameof(HandleIndex),
            o => o.HandleIndex,
            (o, v) => o.HandleIndex = v);

    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    internal int HandleIndex
    {
        get => _handleIndex;
        set => SetAndRaise(HandleIndexProperty, ref _handleIndex, value);
    }

    /// <summary>
    /// Brush of the ring that <see cref="Render(DrawingContext)" /> draws around the thumb circle, outside the border
    /// stroke; <see langword="null" />, the default, draws no ring. The handle style of the slider sets it.
    /// </summary>
    internal IBrush? OutlineBrush
    {
        get => GetValue(OutlineBrushProperty);
        set => SetValue(OutlineBrushProperty, value);
    }

    /// <summary>
    /// Width of the outline ring around the thumb circle, in device-independent pixels; only the left value is used.
    /// The handle style of the slider sets it.
    /// </summary>
    internal Thickness OutlineThickness
    {
        get => GetValue(OutlineThicknessProperty);
        set => SetValue(OutlineThicknessProperty, value);
    }

    internal double ThumbCircleSize
    {
        get => GetValue(ThumbCircleSizeProperty);
        set => SetValue(ThumbCircleSizeProperty, value);
    }

    #endregion

    private int _handleIndex;
    private Point? _lastPoint;
    private IPen? _circlePen;
    private IPen? _outlinePen;

    static SliderThumb()
    {
        DragStartedEvent.AddClassHandler<SliderThumb>((x, e) => x.OnDragStarted(e), RoutingStrategies.Bubble);
        DragDeltaEvent.AddClassHandler<SliderThumb>((x, e) => x.OnDragDelta(e), RoutingStrategies.Bubble);
        DragCompletedEvent.AddClassHandler<SliderThumb>((x, e) => x.OnDragCompleted(e), RoutingStrategies.Bubble);
        AffectsRender<SliderThumb>(BackgroundProperty,
            ThumbCircleSizeProperty,
            OutlineThicknessProperty,
            BorderThicknessProperty,
            OutlineBrushProperty,
            BorderBrushProperty);
    }

    internal void AdjustDrag(Vector v)
    {
        if (_lastPoint.HasValue)
        {
            _lastPoint = _lastPoint.Value + v;
        }
    }

    /// <summary>
    /// Returns a <see cref="SliderThumbAutomationPeer" /> that exposes this thumb to UI Automation.
    /// </summary>
    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new SliderThumbAutomationPeer(this);
    }

    /// <summary>
    /// Called when this thumb raises its own <see cref="DragStarted" /> event. The default implementation does
    /// nothing.
    /// </summary>
    protected virtual void OnDragStarted(VectorEventArgs e)
    {
    }

    /// <summary>
    /// Called when this thumb raises its own <see cref="DragDelta" /> event. The default implementation does
    /// nothing.
    /// </summary>
    protected virtual void OnDragDelta(VectorEventArgs e)
    {
    }

    /// <summary>
    /// Called when this thumb raises its own <see cref="DragCompleted" /> event. The default implementation does
    /// nothing.
    /// </summary>
    protected virtual void OnDragCompleted(VectorEventArgs e)
    {
    }

    /// <summary>
    /// Ends a drag that is still in progress when the pointer capture is lost: raises
    /// <see cref="DragCompleted" /> with the last recorded pointer position and removes the <c>pressed</c>
    /// pseudo-class. The base implementation is called afterwards.
    /// </summary>
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        if (_lastPoint.HasValue)
        {
            var ev = new VectorEventArgs
            {
                RoutedEvent = DragCompletedEvent,
                Vector      = _lastPoint.Value
            };

            _lastPoint = null;

            RaiseEvent(ev);
        }

        PseudoClasses.Remove(StdPseudoClass.Pressed);

        base.OnPointerCaptureLost(e);
    }

    /// <summary>
    /// Raises <see cref="DragDelta" /> while a drag is in progress and the thumb is enabled. The base
    /// implementation is not called.
    /// </summary>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (IsEnabled && _lastPoint.HasValue)
        {
            var ev = new VectorEventArgs
            {
                RoutedEvent = DragDeltaEvent,
                Vector      = e.GetPosition(this) - _lastPoint.Value
            };

            RaiseEvent(ev);
        }
    }

    /// <summary>
    /// Starts a drag when the thumb is enabled: marks the press handled, records the pointer position relative to
    /// the thumb as the drag start, captures the pointer, adds the <c>pressed</c> pseudo-class, suppresses gesture
    /// recognition and raises <see cref="DragStarted" />. The base implementation is not called.
    /// </summary>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (!IsEnabled)
        {
            return;
        }

        e.Handled  = true;
        _lastPoint = e.GetPosition(this);
        e.Pointer.Capture(this);

        var ev = new VectorEventArgs
        {
            RoutedEvent = DragStartedEvent,
            Vector      = (Vector)_lastPoint
        };

        PseudoClasses.Add(StdPseudoClass.Pressed);

        e.PreventGestureRecognition();

        RaiseEvent(ev);
    }

    /// <summary>
    /// Ends a drag that is in progress: marks the release handled, clears the drag state, releases the pointer
    /// capture and raises <see cref="DragCompleted" />. Removes the <c>pressed</c> pseudo-class in every case.
    /// The base implementation is not called.
    /// </summary>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (_lastPoint.HasValue)
        {
            e.Handled  = true;
            _lastPoint = null;

            if (e.Pointer.Captured == this)
            {
                e.Pointer.Capture(null);
            }

            var ev = new VectorEventArgs
            {
                RoutedEvent = DragCompletedEvent,
                Vector      = e.GetPosition(this)
            };

            RaiseEvent(ev);
        }

        PseudoClasses.Remove(StdPseudoClass.Pressed);
    }

    /// <summary>
    /// Paints a circle of diameter <see cref="ThumbCircleSize" /> centered in the control bounds: filled with
    /// <c>Background</c>, stroked with <c>BorderBrush</c> at the left border thickness, and surrounded by an
    /// outline ring of <see cref="OutlineBrush" /> at the left <see cref="OutlineThickness" />.
    /// </summary>
    public override void Render(DrawingContext context)
    {
        // The thumb circle.
        var centerPos         = new Point(Bounds.Width / 2, Bounds.Height / 2);
        var thumbCircleRadius = ThumbCircleSize / 2 + BorderThickness.Left / 2;
        PenUtils.TryModifyOrCreate(ref _circlePen, BorderBrush, BorderThickness.Left);
        context.DrawEllipse(Background, _circlePen, centerPos, thumbCircleRadius, thumbCircleRadius);
        // The outline ring.
        var outlinePenRadius = ThumbCircleSize / 2 + BorderThickness.Left / 2 + OutlineThickness.Left / 2;
        PenUtils.TryModifyOrCreate(ref _outlinePen, OutlineBrush, OutlineThickness.Left);
        context.DrawEllipse(null, _outlinePen, centerPos, outlinePenRadius, outlinePenRadius);
    }

}
