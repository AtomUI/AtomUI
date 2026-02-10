using System.Reactive.Disposables;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

internal class TreeSelectTreeItem : TreeItem
{
    internal static readonly DirectProperty<TreeSelectTreeItem, bool> IsMaxSelectReachedProperty =
        AvaloniaProperty.RegisterDirect<TreeSelectTreeItem, bool>(nameof(IsMaxSelectReached),
            o => o.IsMaxSelectReached,
            (o, v) => o.IsMaxSelectReached = v);
    
    private bool _isMaxSelectReached;

    internal bool IsMaxSelectReached
    {
        get => _isMaxSelectReached;
        set => SetAndRaise(IsMaxSelectReachedProperty, ref _isMaxSelectReached, value);
    }
    
    protected override void PrepareTreeViewItem(TreeItem treeItem, object? item, int index, CompositeDisposable disposables)
    {
        base.PrepareTreeViewItem(treeItem, item, index, disposables);
        disposables.Add(BindUtils.RelayBind(this, TreeSelectTreeItem.IsMaxSelectReachedProperty, treeItem, TreeSelectTreeItem.IsMaxSelectReachedProperty));
    }
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new TreeSelectTreeItem();
    }
    
    protected override bool NeedsContainerOverride(
        object? item,
        int index,
        out object? recycleKey)
    {
        return NeedsContainer<TreeSelectTreeItem>(item, out recycleKey);
    }
}