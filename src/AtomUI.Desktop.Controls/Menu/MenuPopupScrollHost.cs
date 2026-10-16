using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

internal class MenuPopupScrollHost : ContentControl, IMotionAwareControl, IScrollAwareControl
{
    #region 公共属性定义

    public static readonly StyledProperty<bool> IsScrollEnabledProperty =
        ScrollAwareControlProperty.IsScrollEnabledProperty.AddOwner<MenuPopupScrollHost>();

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<MenuPopupScrollHost>();

    public static readonly StyledProperty<bool> AllowAutoHideProperty =
        Avalonia.Controls.ScrollViewer.AllowAutoHideProperty.AddOwner<MenuPopupScrollHost>();

    public bool IsScrollEnabled
    {
        get => GetValue(IsScrollEnabledProperty);
        set => SetValue(IsScrollEnabledProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    public bool AllowAutoHide
    {
        get => GetValue(AllowAutoHideProperty);
        set => SetValue(AllowAutoHideProperty, value);
    }

    #endregion
}
