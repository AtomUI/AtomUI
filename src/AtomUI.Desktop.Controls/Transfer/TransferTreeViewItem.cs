using AtomUI.Theme;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The container of one node in a <see cref="TransferTreeView" />, also for the child nodes of that node.
/// </summary>
[TokenOwnerTransparent]
public class TransferTreeViewItem : TreeViewItem
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="IsMasked" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMaskedProperty =
        AvaloniaProperty.Register<TransferTreeViewItem, bool>(nameof(IsMasked));

    /// <summary>
    /// Whether this item is masked. <see cref="TransferTreeView" /> sets it when it prepares the container. A masked
    /// item is drawn as disabled, and the view leaves it out when it checks or unchecks nodes recursively.
    /// </summary>
    public bool IsMasked
    {
        get => GetValue(IsMaskedProperty);
        set => SetValue(IsMaskedProperty, value);
    }
    #endregion
    
    /// <summary>
    /// Creates a <see cref="TransferTreeViewItem" /> as the container of a child item, so that the containers of a
    /// whole tree have the same type. The base implementation is not called.
    /// </summary>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new TransferTreeViewItem();
    }
    
    /// <summary>
    /// Reports whether a child item needs a new <see cref="TransferTreeViewItem" /> as its container.
    /// </summary>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<TransferTreeViewItem>(item, out recycleKey);
    }

    /// <summary>
    /// Called by the tree view when a container is prepared for an item. It calls the base implementation and then
    /// lets the owning <see cref="TransferTreeView" /> prepare the container, which sets <see cref="IsMasked" />
    /// from the item. An override must call this implementation to keep the masked state of the items.
    /// </summary>
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
