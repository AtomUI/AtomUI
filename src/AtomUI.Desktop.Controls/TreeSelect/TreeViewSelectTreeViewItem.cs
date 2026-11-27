using AtomUI.Generated.AtomUIDesktopControls;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

internal class TreeViewSelectTreeViewItem : TreeViewItem
{
    public TreeViewSelectTreeViewItem()
    {
        // The containers are created at runtime, so the class marker is injected in the constructor. That covers every
        // nesting level and container recycling.
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