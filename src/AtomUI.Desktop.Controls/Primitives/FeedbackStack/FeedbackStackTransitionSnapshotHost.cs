using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Owns a bounded, one-shot bitmap of its child while the outer feedback card completes a stack transition.
/// </summary>
internal sealed class FeedbackStackTransitionSnapshotHost : Decorator
{
    internal const string SnapshotHostPart = "PART_StackTransitionSnapshotHost";

    private RenderTargetBitmap? _snapshotBitmap;
    private Control? _snapshotContent;
    private double _contentOpacity;
    private bool _contentIsHitTestVisible;

    internal bool IsSnapshotActive => _snapshotBitmap is not null;

    internal RenderTargetBitmap? SnapshotBitmap => _snapshotBitmap;

    internal bool TryBeginSnapshot()
    {
        ReleaseSnapshot();

        if (Child is not { } content ||
            !content.IsAttachedToVisualTree() ||
            content.Bounds.Width <= 0 ||
            content.Bounds.Height <= 0)
        {
            return false;
        }

        RenderTargetBitmap? bitmap = null;
        try
        {
            // The stretched layout can be wider than its intrinsic DesiredSize. Capture
            // the arranged pixels so replacing the live visual cannot rescale its text.
            var scaling = TopLevel.GetTopLevel(content)?.RenderScaling ?? 1;
            bitmap = new RenderTargetBitmap(
                PixelSize.FromSize(content.Bounds.Size, scaling),
                new Vector(96 * scaling, 96 * scaling));
            bitmap.Render(content);
            _snapshotContent = content;
            _contentOpacity = content.Opacity;
            _contentIsHitTestVisible = content.IsHitTestVisible;
            _snapshotBitmap = bitmap;
            bitmap = null;

            content.SetCurrentValue(OpacityProperty, 0d);
            content.SetCurrentValue(IsHitTestVisibleProperty, false);
            InvalidateVisual();
            return true;
        }
        catch
        {
            bitmap?.Dispose();
            ReleaseSnapshot();
            return false;
        }
    }

    internal void ReleaseSnapshot()
    {
        if (_snapshotBitmap is null && _snapshotContent is null)
        {
            return;
        }

        var content = _snapshotContent;
        _snapshotContent = null;
        if (content is not null)
        {
            content.SetCurrentValue(OpacityProperty, _contentOpacity);
            content.SetCurrentValue(IsHitTestVisibleProperty, _contentIsHitTestVisible);
        }

        var bitmap = _snapshotBitmap;
        _snapshotBitmap = null;
        bitmap?.Dispose();
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (_snapshotBitmap is not { } bitmap)
        {
            return;
        }

        var pixelSize = bitmap.PixelSize;
        context.DrawImage(
            bitmap,
            new Rect(0, 0, pixelSize.Width, pixelSize.Height),
            new Rect(Bounds.Size));
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == ChildProperty && change.OldValue is not null)
        {
            ReleaseSnapshot();
        }
        base.OnPropertyChanged(change);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ReleaseSnapshot();
        base.OnDetachedFromVisualTree(e);
    }
}
