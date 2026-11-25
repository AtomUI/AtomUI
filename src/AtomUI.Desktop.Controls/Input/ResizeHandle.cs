using System.Globalization;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Media;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

internal class ResizeHandle : TemplatedControl
{
    internal static readonly StyledProperty<bool> UseResizeIndicatorTokenResourcesProperty =
        AvaloniaProperty.Register<ResizeHandle, bool>(nameof(UseResizeIndicatorTokenResources));

    internal bool UseResizeIndicatorTokenResources
    {
        get => GetValue(UseResizeIndicatorTokenResourcesProperty);
        set => SetValue(UseResizeIndicatorTokenResourcesProperty, value);
    }

    internal TextArea? Owner { get; set; }

    private Point? _lastPoint;
    private bool _dragging;
    private IPen? _indicatorPen;
    private static readonly object s_indicatorBrushKey = TextAreaTokenKind.ResizeIndicatorLineColor;
    private OwnerTokenSet? _indicatorTokens;
    private IBrush? _indicatorBrush;
    private double _indicatorLineWidth;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateResizeIndicatorTokenSubscription();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _indicatorTokens?.Dispose();
        _indicatorTokens = null;
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == UseResizeIndicatorTokenResourcesProperty && this.IsAttachedToVisualTree())
        {
            UpdateResizeIndicatorTokenSubscription();
        }
    }

    private void UpdateResizeIndicatorTokenSubscription()
    {
        if (UseResizeIndicatorTokenResources && _indicatorTokens is null)
        {
            _indicatorTokens = new OwnerTokenSet(this, 2, RefreshIndicator);
            _indicatorTokens.SetResourceKey(0, s_indicatorBrushKey);
            _indicatorTokens.SetKind(1, SharedTokenKind.LineWidth);
        }
        else if (!UseResizeIndicatorTokenResources && _indicatorTokens is not null)
        {
            _indicatorTokens.Dispose();
            _indicatorTokens = null;
        }
        RefreshIndicator();
    }

    private void RefreshIndicator()
    {
        var brush = _indicatorTokens?.GetValue(0) switch
        {
            IBrush value => value,
            Color color => new ImmutableSolidColorBrush(color),
            string text when Color.TryParse(text, out var color) => new ImmutableSolidColorBrush(color),
            var value when ReferenceEquals(value, BindingOperations.DoNothing) => _indicatorBrush,
            _ => null
        };
        var lineWidth = ReadLineWidth(_indicatorTokens?.GetValue(1));
        if (!Equals(_indicatorBrush, brush) || !_indicatorLineWidth.Equals(lineWidth))
        {
            _indicatorBrush = brush;
            _indicatorLineWidth = lineWidth;
            InvalidateVisual();
        }
    }

    private double ReadLineWidth(object? value)
    {
        if (value is double width)
        {
            return width;
        }
        if (ReferenceEquals(value, BindingOperations.DoNothing))
        {
            return _indicatorLineWidth;
        }
        try
        {
            return value is IConvertible convertible ? convertible.ToDouble(CultureInfo.CurrentCulture) : 0;
        }
        catch (Exception error) when (error is FormatException or InvalidCastException or OverflowException)
        {
            return 0;
        }
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

        PenUtils.TryModifyOrCreate(ref _indicatorPen, _indicatorBrush, _indicatorLineWidth);
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
