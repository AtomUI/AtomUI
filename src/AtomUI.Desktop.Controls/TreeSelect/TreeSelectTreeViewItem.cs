using System.Reactive.Disposables;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

internal class TreeSelectTreeViewItem : TreeViewItem
{
    internal static readonly DirectProperty<TreeSelectTreeViewItem, bool> IsMaxSelectReachedProperty =
        AvaloniaProperty.RegisterDirect<TreeSelectTreeViewItem, bool>(nameof(IsMaxSelectReached),
            o => o.IsMaxSelectReached,
            (o, v) => o.IsMaxSelectReached = v);
    
    private bool _isMaxSelectReached;

    internal bool IsMaxSelectReached
    {
        get => _isMaxSelectReached;
        set => SetAndRaise(IsMaxSelectReachedProperty, ref _isMaxSelectReached, value);
    }
    
    protected override void PrepareTreeViewItem(TreeViewItem treeViewItem, object? item, int index, CompositeDisposable disposables)
    {
        base.PrepareTreeViewItem(treeViewItem, item, index, disposables);
        disposables.Add(BindUtils.RelayBind(this, TreeSelectTreeViewItem.IsMaxSelectReachedProperty, treeViewItem, TreeSelectTreeViewItem.IsMaxSelectReachedProperty));
    }
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new TreeSelectTreeViewItem();
    }
    
    protected override bool NeedsContainerOverride(
        object? item,
        int index,
        out object? recycleKey)
    {
        return NeedsContainer<TreeSelectTreeViewItem>(item, out recycleKey);
    }
}