using AtomUI.Generated.AtomUIDesktopControls;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

internal class TreeViewSelectTreeViewItem : TreeViewItem
{
    public TreeViewSelectTreeViewItem()
    {
        // 容器运行时创建，标记在创建时注入，覆盖任意嵌套层级与容器回收。
        Classes.Add(TreeSelectSemanticParts.PopupListItemClass);
    }

    internal static readonly DirectProperty<TreeViewSelectTreeViewItem, bool> IsMaxSelectReachedProperty =
        AvaloniaProperty.RegisterDirect<TreeViewSelectTreeViewItem, bool>(nameof(IsMaxSelectReached),
            o => o.IsMaxSelectReached,
            (o, v) => o.IsMaxSelectReached = v);
    
    private bool _isMaxSelectReached;

    internal bool IsMaxSelectReached
    {
        get => _isMaxSelectReached;
        set => SetAndRaise(IsMaxSelectReachedProperty, ref _isMaxSelectReached, value);
    }
    
    protected override void PrepareTreeViewItem(TreeViewItem treeViewItem, object? item, int index)
    {
        base.PrepareTreeViewItem(treeViewItem, item, index);
        treeViewItem[!TreeViewSelectTreeViewItem.IsMaxSelectReachedProperty] =
            this[!TreeViewSelectTreeViewItem.IsMaxSelectReachedProperty];
    }
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new TreeViewSelectTreeViewItem();
    }
    
    protected override bool NeedsContainerOverride(
        object? item,
        int index,
        out object? recycleKey)
    {
        return NeedsContainer<TreeViewSelectTreeViewItem>(item, out recycleKey);
    }
}