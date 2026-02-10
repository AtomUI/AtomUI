using Avalonia;
using Avalonia.Layout;

namespace AtomUI.Desktop.Controls;

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