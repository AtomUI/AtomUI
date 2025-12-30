using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls.Primitives;

[PseudoClasses(StdPseudoClass.Pressed)]
public class Thumb : TemplatedControl
{
    public static readonly RoutedEvent<VectorEventArgs> DragStartedEvent =
        RoutedEvent.Register<Thumb, VectorEventArgs>(nameof(DragStarted), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<VectorEventArgs> DragDeltaEvent =
        RoutedEvent.Register<Thumb, VectorEventArgs>(nameof(DragDelta), RoutingStrategies.Bubble);

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

    public event EventHandler<VectorEventArgs>? DragStarted
    {
        add => AddHandler(DragStartedEvent, value);
        remove => RemoveHandler(DragStartedEvent, value);
    }

    public event EventHandler<VectorEventArgs>? DragDelta
    {
        add => AddHandler(DragDeltaEvent, value);
        remove => RemoveHandler(DragDeltaEvent, value);
    }

    public event EventHandler<VectorEventArgs>? DragCompleted
    {
        add => AddHandler(DragCompletedEvent, value);
        remove => RemoveHandler(DragCompletedEvent, value);
    }

    internal void AdjustDrag(Vector v)
    {
        if (_dragStartPoint.HasValue)
        {
            _dragStartPoint = _dragStartPoint.Value + v;
        }
    }
    
    protected virtual void OnDragStarted(VectorEventArgs e)
    {
    }
    
    protected virtual void OnDragDelta(VectorEventArgs e)
    {
    }
    
    protected virtual void OnDragCompleted(VectorEventArgs e)
    {
    }

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

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        e.Handled  = true;
        var topLevel = TopLevel.GetTopLevel(this) as Visual;
        _dragRoot = this.GetVisualRoot() as Visual ?? topLevel ?? this;
        _dragStartPoint = e.GetPosition(_dragRoot);
        _currentPoint = _dragStartPoint;

        var ev = new VectorEventArgs
        {
            RoutedEvent = DragStartedEvent,
            Vector      = (Vector)_dragStartPoint.Value,
        };

        PseudoClasses.Add(StdPseudoClass.Pressed);

        e.PreventGestureRecognition();

        RaiseEvent(ev);
    }

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
