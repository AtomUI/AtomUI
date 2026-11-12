using AtomUI.Controls;
using AtomUI.Generated.AtomUIDesktopControls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace AtomUI.Desktop.Controls;

using AvaloniaListBoxItem = Avalonia.Controls.ListBoxItem;

[PseudoClasses(KeyboardActivePseudoClass)]
public class TimeViewCell : AvaloniaListBoxItem
{
    internal const string KeyboardActivePseudoClass = ":keyboard-active";

    #region 内部属性定义
    
    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<TimeViewCell>();

    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>The selected value of the column that navigation keys from the picker input are stepping.</summary>
    internal bool IsKeyboardActive
    {
        get => PseudoClasses.Contains(KeyboardActivePseudoClass);
        set => PseudoClasses.Set(KeyboardActivePseudoClass, value);
    }

    #endregion

    protected override void OnInitialized()
    {
        base.OnInitialized();

        Classes.Add(TimePickerSemanticParts.PopupItemClass);
    }

}