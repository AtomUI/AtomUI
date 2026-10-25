using AtomUI.Controls;
using AtomUI.Utils;
using Avalonia;

namespace AtomUI.Desktop.Controls;

public class ArrowDecoratedBox : AbstractArrowDecoratedBox
{
    public ArrowDecoratedBox()
    {
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        CompositionTreeRetirement.Retire(this);
        base.OnDetachedFromVisualTree(e);
    }
}