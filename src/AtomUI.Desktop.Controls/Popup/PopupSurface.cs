using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia;

namespace AtomUI.Desktop.Controls;

internal class PopupSurface : Panel
{
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        this.FindLogicalAncestorOfType<Popup>()?.NotifySurfaceReady(this);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        // A directly opened Popup has no rooted logical owner, so its host does
        // not raise logical attach. Visual attach still precedes the first draw.
        this.FindLogicalAncestorOfType<Popup>()?.NotifySurfaceReady(this);
    }
}
