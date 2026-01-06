using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

internal class PlacementAwareDecorator : ContentControl
{
    #region 公共属性定义
    public static readonly StyledProperty<Direction> MarginPlacementProperty =
        AvaloniaProperty.Register<PlacementAwareDecorator, Direction>(nameof (MarginPlacement));
    
    public static readonly StyledProperty<double> MarginToAnchorProperty =
        Popup.MarginToAnchorProperty.AddOwner<PlacementAwareDecorator>();
    
    public Direction MarginPlacement
    {
        get => GetValue(MarginPlacementProperty);
        set => SetValue(MarginPlacementProperty, value);
    }
    
    public double MarginToAnchor
    {
        get => GetValue(MarginToAnchorProperty);
        set => SetValue(MarginToAnchorProperty, value);
    }
    #endregion
}