using Avalonia.Controls;
using Avalonia.Layout;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// An invisible item for a <see cref="CompactSpace" /> that fills the length the other items leave over. Set
/// a star size on it, such as <c>5*</c> through <c>CompactSpace.ItemSize</c>, to push the other items to one
/// side. Use at most one filler and put it last; it is left out when the items are told their position.
/// </summary>
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

    double ICompactSpaceAware.GetBorderThickness() => 0.0;
}