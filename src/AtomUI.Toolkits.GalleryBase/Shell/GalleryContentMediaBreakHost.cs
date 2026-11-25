using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Toolkits.GalleryBase.Shell;

internal sealed class GalleryContentMediaBreakHost : Border, IMediaBreakAwareControl, IDisposable
{
    internal static readonly StyledProperty<MediaBreakPoint> MediaBreakPointProperty =
        MediaBreakAwareControlProperty.MediaBreakPointProperty.AddOwner<GalleryContentMediaBreakHost>();

    private bool _isDisposed;
    private GalleryMediaBreakPointResolver? _breakpoints;

    public GalleryContentMediaBreakHost()
    {
        SizeChanged += HandleSizeChanged;
    }

    public MediaBreakPoint MediaBreakPoint => GetValue(MediaBreakPointProperty);

    public event EventHandler<MediaBreakPointChangedEventArgs>? MediaBreakPointChanged;

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        SizeChanged -= HandleSizeChanged;
        _breakpoints?.Dispose();
        _breakpoints = null;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (_isDisposed)
        {
            _isDisposed = false;
            SizeChanged += HandleSizeChanged;
        }
        _breakpoints ??= new GalleryMediaBreakPointResolver(this, () =>
            NotifyMediaBreakPointChanged(_breakpoints!.Resolve(Bounds.Width)));
        if (Bounds.Width > 0)
        {
            NotifyMediaBreakPointChanged(_breakpoints.Resolve(Bounds.Width));
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Dispose();
        base.OnDetachedFromVisualTree(e);
    }

    private void HandleSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        NotifyMediaBreakPointChanged(_breakpoints?.Resolve(e.NewSize.Width) ?? MediaBreakPoint);
    }

    private void NotifyMediaBreakPointChanged(MediaBreakPoint breakPoint)
    {
        if (MediaBreakPoint == breakPoint)
        {
            return;
        }

        SetCurrentValue(MediaBreakPointProperty, breakPoint);
        MediaBreakPointChanged?.Invoke(this, new MediaBreakPointChangedEventArgs(breakPoint));
    }
}
