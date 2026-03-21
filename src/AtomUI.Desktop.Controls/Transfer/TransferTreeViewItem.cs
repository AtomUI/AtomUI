using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

public class TransferTreeViewItem : TreeViewItem
{
    #region 公共属性定义

    public static readonly StyledProperty<bool> IsMaskedProperty =
        AvaloniaProperty.Register<TransferTreeViewItem, bool>(nameof(IsMasked));

    public bool IsMasked
    {
        get => GetValue(IsMaskedProperty);
        set => SetValue(IsMaskedProperty, value);
    }
    #endregion
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new TransferTreeViewItem();
    }
    
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<TransferTreeViewItem>(item, out recycleKey);
    }
}