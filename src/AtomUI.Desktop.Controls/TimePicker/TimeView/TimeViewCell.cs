using AtomUI.Controls;
using AtomUI.Generated.AtomUIDesktopControls;
using AtomUI.Theme.Resources;
using Avalonia;

namespace AtomUI.Desktop.Controls;

using AvaloniaListBoxItem = Avalonia.Controls.ListBoxItem;

public class TimeViewCell : AvaloniaListBoxItem, IThemeInheritanceSource
{
    #region 内部属性定义
    
    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<TimeViewCell>();

    internal static readonly StyledProperty<bool> IsDisabledColorExplicitProperty =
        AvaloniaProperty.Register<TimeViewCell, bool>(nameof(IsDisabledColorExplicit));

    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    internal bool IsDisabledColorExplicit
    {
        get => GetValue(IsDisabledColorExplicitProperty);
        set => SetValue(IsDisabledColorExplicitProperty, value);
    }

    #endregion

    protected override void OnInitialized()
    {
        base.OnInitialized();

        Classes.Add(TimePickerSemanticParts.PopupItemClass);
    }

    #region 实现 IThemeInheritanceSource 接口

    // ControlTheme cannot read the protected inheritance parent. Its Setters own
    // typography; this bridge exposes the actual source without copying values.
    AvaloniaObject? IThemeInheritanceSource.ThemeInheritanceParent => InheritanceParent;

    #endregion

}
