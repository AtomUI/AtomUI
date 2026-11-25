using AtomUI.Theme;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

[TokenOwnerTransparent]
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

    protected override void PrepareTreeViewItem(TreeViewItem treeViewItem, object? item, int index)
    {
        base.PrepareTreeViewItem(treeViewItem, item, index);
        if (treeViewItem is TransferTreeViewItem transferTreeViewItem &&
            OwnerTreeView is TransferTreeView transferTreeView)
        {
            transferTreeView.PrepareTransferTreeViewItem(transferTreeViewItem, item);
        }
    }
}
