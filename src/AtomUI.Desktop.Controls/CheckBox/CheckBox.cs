using AtomUI.Controls;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

using AvaloniaCheckBox = Avalonia.Controls.CheckBox;

public class CheckBox : AvaloniaCheckBox, IWaveSpiritAwareControl, IControlSharedTokenResourcesHost
{
    #region 公共属性定义

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<CheckBox>();

    public static readonly StyledProperty<bool> IsWaveSpiritEnabledProperty =
        WaveSpiritAwareControlProperty.IsWaveSpiritEnabledProperty.AddOwner<CheckBox>();
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    public bool IsWaveSpiritEnabled
    {
        get => GetValue(IsWaveSpiritEnabledProperty);
        set => SetValue(IsWaveSpiritEnabledProperty, value);
    }

    #endregion
    
    #region 内部属性定义
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => CheckBoxToken.ID;

    #endregion

    public CheckBox()
    {
        this.RegisterResources();
    }
}