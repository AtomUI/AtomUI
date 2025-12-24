using AtomUI.Controls;
using AtomUI.Desktop.Controls.Themes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace AtomUI.Desktop.Controls;

internal class OverlayDialogResizer : TemplatedControl
{
    public OverlayDialogHost? TargetDialog { get; set; }
    
    public event EventHandler<OverlayDialogResizeEventArgs>? AboutToResize;
    public event EventHandler<OverlayDialogResizeEventArgs>? ResizeRequest;
    public event EventHandler<OverlayDialogResizeEventArgs>? ResizeCompleted;
    
    private Panel? _rootLayout;
    private Point? _lastPoint;
    private bool _dragging;
    private ResizeHandleLocation? _dragLocation;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_rootLayout != null)
        {
            foreach (var child in _rootLayout.Children)
            {
                child.PointerPressed  -= HandleResizeHandlePressed;
                child.PointerReleased -= HandleResizeHandleReleased;
                child.PointerMoved    -= HandleResizeHandleMoved;
            }
        }
        _rootLayout = e.NameScope.Find<Panel>(WindowResizerThemeConstants.RootLayoutPart);
        if (_rootLayout != null)
        {
            foreach (var child in _rootLayout.Children)
            {
                child.PointerPressed  += HandleResizeHandlePressed;
                child.PointerReleased += HandleResizeHandleReleased;
                child.PointerMoved    += HandleResizeHandleMoved;
            }
        }
    }
    
    private void HandleResizeHandlePressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Properties.IsLeftButtonPressed && sender is Border border && border.Tag is ResizeHandleLocation location)
        {
            e.Handled     = true;
            _dragLocation = location;
            _lastPoint    = e.GetPosition(TopLevel.GetTopLevel(this));
            e.PreventGestureRecognition();
        }
    }

    private void HandleResizeHandleReleased(object? sender, PointerEventArgs e)
    {
        if (_lastPoint.HasValue && _dragLocation.HasValue)
        {
            var delta             = e.GetPosition(TopLevel.GetTopLevel(this)) - _lastPoint.Value;
            ResizeCompleted?.Invoke(this, new OverlayDialogResizeEventArgs(_dragLocation.Value, delta.X, delta.Y));
            e.Handled     = true;
            _lastPoint    = null;
            _dragLocation = null;
            _dragging     = false;
        }
    }

    private void HandleResizeHandleMoved(object? sender, PointerEventArgs e)
    {
        if (_lastPoint.HasValue && e.Properties.IsLeftButtonPressed)
        {
            var delta             = e.GetPosition(TopLevel.GetTopLevel(this)) - _lastPoint.Value;
            var manhattanDistance = Math.Abs(delta.X) + Math.Abs(delta.Y);
            if (manhattanDistance > Constants.DragThreshold)
            {
                if (sender is Border edge && edge.Tag is ResizeHandleLocation location)
                {
                    if (!_dragging)
                    {
                        AboutToResize?.Invoke(this, new OverlayDialogResizeEventArgs(location, 0, 0));
                    }
                    _dragging = true;
                    ResizeRequest?.Invoke(this, new OverlayDialogResizeEventArgs(location, delta.X, delta.Y));
                }
            }
        }
    }
}