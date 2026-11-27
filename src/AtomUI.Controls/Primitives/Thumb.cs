using Avalonia;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Primitives;

/// <summary>
/// A control that reports a pointer drag started on it through the <see cref="DragStarted" />,
/// <see cref="DragDelta" /> and <see cref="DragCompleted" /> routed events.
/// Every pointer press on it starts a drag; the <see cref="StdPseudoClass.Pressed" /> pseudo-class is set while
/// the drag lasts.
/// </summary>
[PseudoClasses(StdPseudoClass.Pressed)]
public class Thumb : TemplatedControl
{
    /// <summary>
    /// Identifies the <see cref="DragStarted" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<VectorEventArgs> DragStartedEvent =
        RoutedEvent.Register<Thumb, VectorEventArgs>(nameof(DragStarted), RoutingStrategies.Bubble);

    /// <summary>
    /// Identifies the <see cref="DragDelta" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<VectorEventArgs> DragDeltaEvent =
        RoutedEvent.Register<Thumb, VectorEventArgs>(nameof(DragDelta), RoutingStrategies.Bubble);

    /// <summary>
    /// Identifies the <see cref="DragCompleted" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<VectorEventArgs> DragCompletedEvent =
        RoutedEvent.Register<Thumb, VectorEventArgs>(nameof(DragCompleted), RoutingStrategies.Bubble);

    private Point? _dragStartPoint;
    private Point? _currentPoint;
    private Visual? _dragRoot;

    static Thumb()
    {
        DragStartedEvent.AddClassHandler<Thumb>((x,e) => x.OnDragStarted(e), RoutingStrategies.Bubble);
        DragDeltaEvent.AddClassHandler<Thumb>((x, e) => x.OnDragDelta(e), RoutingStrategies.Bubble);
        DragCompletedEvent.AddClassHandler<Thumb>((x, e) => x.OnDragCompleted(e), RoutingStrategies.Bubble);
    }

    /// <summary>
    /// Raised when a press on the thumb starts a drag, after the pointer has been captured.
    /// <see cref="VectorEventArgs.Vector" /> is the pointer position relative to the root visual, not an offset.
    /// </summary>
    public event EventHandler<VectorEventArgs>? DragStarted
    {
        add => AddHandler(DragStartedEvent, value);
        remove => RemoveHandler(DragStartedEvent, value);
    }

    /// <summary>
    /// Raised for each pointer move while a drag is in progress.
    /// <see cref="VectorEventArgs.Vector" /> is the total displacement from the drag start, measured relative to the
    /// root visual.
    /// </summary>
    public event EventHandler<VectorEventArgs>? DragDelta
    {
        add => AddHandler(DragDeltaEvent, value);
        remove => RemoveHandler(DragDeltaEvent, value);
    }

    /// <summary>
    /// Raised when the pointer is released or the thumb loses the pointer capture during a drag, after the drag
    /// state has been cleared. <see cref="VectorEventArgs.Vector" /> is the total displacement from the drag start,
    /// or the displacement up to the last reported position when the capture was lost.
    /// </summary>
    public event EventHandler<VectorEventArgs>? DragCompleted
    {
        add => AddHandler(DragCompletedEvent, value);
        remove => RemoveHandler(DragCompletedEvent, value);
    }

    /// <summary>
    /// Shifts the origin of a drag in progress by the given vector, so that the displacement reported afterwards is
    /// measured from the shifted origin. Does nothing when no drag is in progress.
    /// </summary>
    internal void AdjustDrag(Vector v)
    {
        if (_dragStartPoint.HasValue)
        {
            _dragStartPoint = _dragStartPoint.Value + v;
        }
    }
    
    /// <summary>
    /// Called when the <see cref="DragStarted" /> routed event is raised for this thumb. The default implementation
    /// does nothing.
    /// </summary>
    protected virtual void OnDragStarted(VectorEventArgs e)
    {
    }
    
    /// <summary>
    /// Called when the <see cref="DragDelta" /> routed event is raised for this thumb. The default implementation
    /// does nothing.
    /// </summary>
    protected virtual void OnDragDelta(VectorEventArgs e)
    {
    }
    
    /// <summary>
    /// Called when the <see cref="DragCompleted" /> routed event is raised for this thumb. The default implementation
    /// does nothing.
    /// </summary>
    protected virtual void OnDragCompleted(VectorEventArgs e)
    {
    }

    /// <summary>
    /// Ends a drag in progress when the pointer capture is lost: raises <see cref="DragCompleted" /> with the
    /// displacement up to the last reported position, clears the drag state, removes the pressed pseudo-class and
    /// then calls the base method.
    /// </summary>
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        if (_dragStartPoint.HasValue)
        {
            var endPoint = _currentPoint ?? _dragStartPoint.Value;
            var ev = new VectorEventArgs
            {
                RoutedEvent = DragCompletedEvent,
                Vector      = endPoint - _dragStartPoint.Value,
            };

            _dragStartPoint = null;
            _currentPoint = null;
            _dragRoot = null;

            RaiseEvent(ev);
        }

        PseudoClasses.Remove(StdPseudoClass.Pressed);

        base.OnPointerCaptureLost(e);
    }

    /// <summary>
    /// While a drag is in progress, records the pointer position relative to the root visual and raises
    /// <see cref="DragDelta" /> with the displacement from the drag start. Ignores the move otherwise.
    /// </summary>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (_dragStartPoint.HasValue)
        {
            var position = e.GetPosition(_dragRoot ?? this);
            _currentPoint = position;
            var ev = new VectorEventArgs
            {
                RoutedEvent = DragDeltaEvent,
                Vector      = position - _dragStartPoint.Value,
            };

            RaiseEvent(ev);
        }
    }

    /// <summary>
    /// Starts a drag: marks the press handled, captures the pointer, records the start position relative to the root
    /// visual, sets the pressed pseudo-class, suppresses gesture recognition and raises <see cref="DragStarted" />.
    /// </summary>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        e.Handled  = true;
        _dragRoot = this.GetPresentationSource()?.RootVisual ?? this;
        _dragStartPoint = e.GetPosition(_dragRoot);
        _currentPoint = _dragStartPoint;
        e.Pointer.Capture(this);

        var ev = new VectorEventArgs
        {
            RoutedEvent = DragStartedEvent,
            Vector      = _dragStartPoint.Value,
        };

        PseudoClasses.Add(StdPseudoClass.Pressed);
        e.PreventGestureRecognition();
        RaiseEvent(ev);
    }

    /// <summary>
    /// Ends a drag in progress: marks the release handled, clears the drag state, releases the pointer capture and
    /// raises <see cref="DragCompleted" /> with the total displacement, then removes the pressed pseudo-class.
    /// </summary>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (_dragStartPoint.HasValue)
        {
            e.Handled  = true;
            var position = e.GetPosition(_dragRoot ?? this);
            _currentPoint = position;
            var delta = position - _dragStartPoint.Value;
            _dragStartPoint = null;
            _currentPoint = null;
            _dragRoot = null;
            if (e.Pointer.Captured == this)
            {
                e.Pointer.Capture(null);
            }

            var ev = new VectorEventArgs
            {
                RoutedEvent = DragCompletedEvent,
                Vector      = delta,
            };

            RaiseEvent(ev);
        }

        PseudoClasses.Remove(StdPseudoClass.Pressed);
    }
}
