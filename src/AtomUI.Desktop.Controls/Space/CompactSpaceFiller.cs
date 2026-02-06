using Avalonia.Controls;
using Avalonia.Layout;

namespace AtomUI.Desktop.Controls;

public class CompactSpaceFiller : Control, ICompactSpaceAware
{
    void ICompactSpaceAware.NotifyPositionChange(SpaceItemPosition? position)
    {
        // no op
    }

    void ICompactSpaceAware.NotifyOrientationChange(Orientation orientation)
    {
        // no op
    }
}