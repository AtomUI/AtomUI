using AtomUI.Controls;
using AtomUI.Generated.AtomUIDesktopControls;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace AtomUI.Desktop.Controls;

using AvaloniaListBoxItem = Avalonia.Controls.ListBoxItem;

// Token-owner transparent cell: a TimeViewCell belongs to one picker's time view and has no Own Tokens; its theme reads the
// owner's globals, so Controls[TimePicker] (or the hosting date picker) reaches the cells.
[TokenOwnerTransparent]
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

    // Forwarded by the owning DateTimePickerPanel from its TimeView.
    internal static readonly StyledProperty<Thickness> ItemBackgroundInsetProperty =
        AvaloniaProperty.Register<TimeViewCell, Thickness>(nameof(ItemBackgroundInset));

    internal static readonly StyledProperty<bool> CenterItemContentProperty =
        AvaloniaProperty.Register<TimeViewCell, bool>(nameof(CenterItemContent));

    internal Thickness ItemBackgroundInset
    {
        get => GetValue(ItemBackgroundInsetProperty);
        set => SetValue(ItemBackgroundInsetProperty, value);
    }

    internal bool CenterItemContent
    {
        get => GetValue(CenterItemContentProperty);
        set => SetValue(CenterItemContentProperty, value);
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