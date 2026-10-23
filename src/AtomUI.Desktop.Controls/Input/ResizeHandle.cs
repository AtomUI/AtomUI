using AtomUI.Controls;
using AtomUI.Media;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

internal class ResizeHandle : TemplatedControl
{
    #region 内部属性定义

    internal static readonly StyledProperty<double> ResizeIndicatorLineThicknessProperty =
        AvaloniaProperty.Register<ResizeHandle, double>(nameof(ResizeIndicatorLineThickness), 1);

    // The owner theme supplies its effective LineWidth. Bounds supply glyph size.
    internal double ResizeIndicatorLineThickness
    {
        get => GetValue(ResizeIndicatorLineThicknessProperty);
        set => SetValue(ResizeIndicatorLineThicknessProperty, value);
    }

    #endregion

    #region 内部协作 API

    internal TextArea? Owner { get; set; }

    #endregion
    
    private Point? _lastPoint;
    private bool _dragging;
    private IPen? _resizeIndicatorPen;

    static ResizeHandle()
    {
        AffectsRender<ResizeHandle>(ForegroundProperty, ResizeIndicatorLineThicknessProperty);
    }

    public override void Render(DrawingContext context)
    {
        if (Bounds.Width <= 0 || Bounds.Height <= 0 ||
            !double.IsFinite(ResizeIndicatorLineThickness) || ResizeIndicatorLineThickness <= 0)
        {
            return;
        }

        // This handle is the last template layer. Painting in the decorated parent
        // put the glyph behind its opaque template background. The original 8-unit
        // geometry is retained, with Bounds determining both paint and hit size.
        var scale = Math.Min(Bounds.Width, Bounds.Height) / 8;
        var bounds = new Rect(Bounds.Size).Deflate(new Thickness(2 * scale));
        PenUtils.TryModifyOrCreate(ref _resizeIndicatorPen, Foreground, ResizeIndicatorLineThickness);
        if (_resizeIndicatorPen is null)
        {
            return;
        }
        context.DrawLine(_resizeIndicatorPen, new Point(bounds.Right, bounds.Bottom - 7 * scale),
            new Point(bounds.Right - 7 * scale, bounds.Bottom));
        context.DrawLine(_resizeIndicatorPen, new Point(bounds.Right, bounds.Bottom - 4 * scale),
            new Point(bounds.Right - 4 * scale, bounds.Bottom));
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        e.Handled  = true;
        _lastPoint = e.GetPosition(TopLevel.GetTopLevel(this));
        e.Pointer.Capture(this);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (_lastPoint.HasValue)
        {
            Owner?.NotifyResizeCompleted();
            e.Handled  = true;
            _lastPoint = null;
            _dragging  = false;
            e.Pointer.Capture(null);
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_lastPoint.HasValue && e.Properties.IsLeftButtonPressed)
        {
            var delta             = e.GetPosition(TopLevel.GetTopLevel(this)) - _lastPoint.Value;
            var manhattanDistance = Math.Abs(delta.X) + Math.Abs(delta.Y);
            if (manhattanDistance > Constants.DragThreshold)
            {
                if (!_dragging)
                {
                    Owner?.NotifyAboutToResize();
                    _dragging = true;
                }
                Owner?.NotifyResizing(delta);
            }
        }
    }
}