using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

internal enum PopupHostMarginPlacement
{
    None = 0,
    Left,
    Top,
    Right,
    Bottom
}

internal class PlacementAwareDecorator : ContentControl
{
    #region 公共属性定义
    public static readonly StyledProperty<PopupHostMarginPlacement> MarginPlacementProperty =
        AvaloniaProperty.Register<PlacementAwareDecorator, PopupHostMarginPlacement>(nameof (MarginPlacement));
    
    public static readonly StyledProperty<double> MarginToAnchorProperty =
        Popup.MarginToAnchorProperty.AddOwner<PlacementAwareDecorator>();
    
    public PopupHostMarginPlacement MarginPlacement
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