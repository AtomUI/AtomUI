using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Repaints the window frame background inside the cached content root while a content cache is active.
/// </summary>
/// <remarks>
/// BitmapCache clears its layer to transparent. Subpixel text drawn over transparent layer pixels does not match
/// text drawn directly over the frame, so the cached subtree needs an opaque base beneath its content. The
/// backdrop uses the frame's own brush and rounded rectangle, inset by a few device pixels so the frame's
/// antialiased edge is never painted twice.
/// </remarks>
internal sealed class WindowContentBackdrop : Control
{
    private const double InsetDevicePixels = 2;

    private Border? _frame;

    public WindowContentBackdrop()
    {
        IsHitTestVisible = false;
    }

    internal void Activate(Border frame)
    {
        _frame = frame;
        SetCurrentValue(IsVisibleProperty, true);
        InvalidateVisual();
    }

    internal void Deactivate()
    {
        _frame = null;
        SetCurrentValue(IsVisibleProperty, false);
    }

    public override void Render(DrawingContext context)
    {
        if (_frame is not { Background: { } brush } frame || frame.TranslatePoint(default, this) is not { } origin)
        {
            return;
        }
        var radius = frame.CornerRadius;
        // Only rounded corners have an antialiased frame edge to keep clear of; square frames need no inset.
        var inset = radius == default ? 0 : InsetDevicePixels / (TopLevel.GetTopLevel(this)?.RenderScaling ?? 1);
        var rect = new Rect(origin, frame.Bounds.Size).Deflate(inset);
        if (rect.Width <= 0 || rect.Height <= 0)
        {
            return;
        }
        var geometry = new RoundedRect(rect,
            new CornerRadius(Math.Max(0, radius.TopLeft - inset), Math.Max(0, radius.TopRight - inset),
                Math.Max(0, radius.BottomRight - inset), Math.Max(0, radius.BottomLeft - inset)));
        context.DrawRectangle(brush, null, geometry);
    }
}
