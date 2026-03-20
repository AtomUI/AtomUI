using System.Reactive.Disposables;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

internal class TreeViewSelectTreeViewItem : TreeViewItem
{
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
    
    protected override void PrepareTreeViewItem(TreeViewItem treeViewItem, object? item, int index, CompositeDisposable disposables)
    {
        base.PrepareTreeViewItem(treeViewItem, item, index, disposables);
        disposables.Add(BindUtils.RelayBind(this, TreeViewSelectTreeViewItem.IsMaxSelectReachedProperty, treeViewItem, TreeViewSelectTreeViewItem.IsMaxSelectReachedProperty));
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