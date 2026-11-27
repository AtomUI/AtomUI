using Avalonia;
using Avalonia.Layout;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Implemented by every item of a <see cref="CompactSpace" />, which rejects an item that does not implement
/// it. Through it the control tells the item its position and the orientation of the group, so that the item can
/// drop the corners that touch a neighbour. AtomUI.Desktop.Controls.ColorPicker implements it on the color
/// picker.
/// </summary>
/// <remarks>
/// Position and orientation are reported when the template is applied, when the children of the control change
/// and when the orientation changes; the orientation only while the group has more than one item. An item
/// receives a <see langword="null" /> position when it is removed from the control or the template is rebuilt.
/// By default the control keeps the item that is hovered or focused above its neighbours. An item whose
/// <c>IsAlwaysActiveZIndex</c> returns <see langword="true" /> always stays above them. One whose
/// <c>IgnoreZIndexChange</c> returns <see langword="true" /> is not raised on hover or by its own
/// <c>GotFocus</c> event; the control still raises it when the item's classes come to contain
/// <c>:focus-within</c>.
/// </remarks>
internal interface ICompactSpaceAware
{
    void NotifyPositionChange(SpaceItemPosition? position);
    void NotifyOrientationChange(Orientation orientation);

    bool IsAlwaysActiveZIndex() => false;
    bool IgnoreZIndexChange() => false;

    double GetBorderThickness();
}

internal abstract class CompactSpaceAwareControlProperty
{
    public static readonly StyledProperty<SpaceItemPosition?> CompactSpaceItemPositionProperty = 
        AvaloniaProperty.Register<StyledElement, SpaceItemPosition?>("CompactSpaceItemPosition");
    
    public static readonly StyledProperty<Orientation> CompactSpaceOrientationProperty = 
        AvaloniaProperty.Register<StyledElement, Orientation>("CompactSpaceOrientation");
    
    public static readonly StyledProperty<bool> IsUsedInCompactSpaceProperty = 
        AvaloniaProperty.Register<StyledElement, bool>("IsUsedInCompactSpace", defaultValue: false);
}