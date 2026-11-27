using Avalonia;
using Avalonia.Layout;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Base class of <see cref="AutoComplete" /> and <see cref="AutoCompleteSearchEdit" /> that lets the control
/// take part in a compact space. The space reports where the control lies among its siblings and in which
/// direction they are arranged. The control stores this in <c>CompactSpaceItemPosition</c>,
/// <c>CompactSpaceOrientation</c> and <c>IsUsedInCompactSpace</c>, which its ControlTheme binds into the
/// template.
/// </summary>
public class CompactSpaceAwareAutoComplete : AbstractAutoComplete, ICompactSpaceAware
{
    #region 内部属性定义

    internal static readonly StyledProperty<SpaceItemPosition?> CompactSpaceItemPositionProperty = 
        CompactSpaceAwareControlProperty.CompactSpaceItemPositionProperty.AddOwner<CompactSpaceAwareAutoComplete>();
    
    internal static readonly StyledProperty<Orientation> CompactSpaceOrientationProperty = 
        CompactSpaceAwareControlProperty.CompactSpaceOrientationProperty.AddOwner<CompactSpaceAwareAutoComplete>();
    
    internal static readonly StyledProperty<bool> IsUsedInCompactSpaceProperty = 
        CompactSpaceAwareControlProperty.IsUsedInCompactSpaceProperty.AddOwner<CompactSpaceAwareAutoComplete>();

    internal SpaceItemPosition? CompactSpaceItemPosition
    {
        get => GetValue(CompactSpaceItemPositionProperty);
        set => SetValue(CompactSpaceItemPositionProperty, value);
    }
    
    internal Orientation CompactSpaceOrientation
    {
        get => GetValue(CompactSpaceOrientationProperty);
        set => SetValue(CompactSpaceOrientationProperty, value);
    }
    
    internal bool IsUsedInCompactSpace
    {
        get => GetValue(IsUsedInCompactSpaceProperty);
        set => SetValue(IsUsedInCompactSpaceProperty, value);
    }
    #endregion
    
    void ICompactSpaceAware.NotifyPositionChange(SpaceItemPosition? position)
    {
        IsUsedInCompactSpace     = position != null;
        CompactSpaceItemPosition = position;
    }
    
    void ICompactSpaceAware.NotifyOrientationChange(Orientation orientation)
    {
        CompactSpaceOrientation = orientation;
    }

    double ICompactSpaceAware.GetBorderThickness()
    {
        return GetBorderThicknessForCompactSpace();
    }

    /// <summary>
    /// Returns the width of the border that the control shows when it lies in a compact space. The space moves
    /// its items closer together by this width per position, so that adjacent controls overlap their borders.
    /// </summary>
    /// <remarks>
    /// The compact space reads the value through <c>ICompactSpaceAware</c> while it measures its items. The base
    /// implementation returns 0; a derived control that draws a border overrides it and returns that width.
    /// </remarks>
    protected virtual double GetBorderThicknessForCompactSpace() => 0;
}