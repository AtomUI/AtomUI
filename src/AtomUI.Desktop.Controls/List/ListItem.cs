using Avalonia;

namespace AtomUI.Desktop.Controls;

public class ListItem : ListBoxItem
{
    #region 内部属性定义
    
    internal static readonly StyledProperty<bool> IsGroupItemProperty =
        AvaloniaProperty.Register<ListItem, bool>(nameof(IsGroupItem));
    
    internal bool IsGroupItem
    {
        get => GetValue(IsGroupItemProperty);
        set => SetValue(IsGroupItemProperty, value);
    }
    
    #endregion
}