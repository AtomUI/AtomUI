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
    /// <summary>Colour of the two diagonal indicator strokes (TextArea.ResizeIndicatorLineColor).</summary>
    internal static readonly StyledProperty<IBrush?> IndicatorBrushProperty =
        AvaloniaProperty.Register<ResizeHandle, IBrush?>(nameof(IndicatorBrush));

    /// <summary>Stroke width of the indicator (the owner's LineWidth).</summary>
    internal static readonly StyledProperty<double> IndicatorLineWidthProperty =
        AvaloniaProperty.Register<ResizeHandle, double>(nameof(IndicatorLineWidth));

    internal IBrush? IndicatorBrush
    {
        get => GetValue(IndicatorBrushProperty);
        set => SetValue(IndicatorBrushProperty, value);
    }

    internal double IndicatorLineWidth
    {
        get => GetValue(IndicatorLineWidthProperty);
        set => SetValue(IndicatorLineWidthProperty, value);
    }

    internal TextArea? Owner { get; set; }

    private Point? _lastPoint;
    private bool _dragging;
    private IPen? _indicatorPen;

    static ResizeHandle()
    {
        AffectsRender<ResizeHandle>(IndicatorBrushProperty, IndicatorLineWidthProperty);
    }

    /// <summary>
    /// Draws the indicator above the input surface (the handle is stacked over the decorated box, whose
    /// background would cover anything the box drew itself). The strokes are proportional to the handle size,
    /// which is TextArea.ResizeHandleSize: inset 1/4, outer stroke 7/8 and inner stroke 1/2 of the edge length,
    /// anchored at the bottom-right corner.
    /// </summary>
    public override void Render(DrawingContext context)
    {
        base.Render(context);
        var size = Math.Min(Bounds.Width, Bounds.Height);
        if (size <= 0)
        {
            return;
        }

        PenUtils.TryModifyOrCreate(ref _indicatorPen, IndicatorBrush, IndicatorLineWidth);
        if (_indicatorPen is null)
        {
            return;
        }

        var right  = Bounds.Width - size / 4;
        var bottom = Bounds.Height - size / 4;
        foreach (var length in (ReadOnlySpan<double>)[size * 7 / 8, size / 2])
        {
            context.DrawLine(_indicatorPen, new Point(right, bottom - length), new Point(right - length, bottom));
        }
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
