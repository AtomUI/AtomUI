using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

internal class TreeSelectTreeView : TreeView
{
    protected override Type StyleKeyOverride => typeof(TreeView);
    
    internal static readonly DirectProperty<TreeSelectTreeView, bool> IsMaxSelectReachedProperty =
        AvaloniaProperty.RegisterDirect<TreeSelectTreeView, bool>(nameof(IsMaxSelectReached),
            o => o.IsMaxSelectReached,
            (o, v) => o.IsMaxSelectReached = v);
    
    private bool _isMaxSelectReached;

    internal bool IsMaxSelectReached
    {
        get => _isMaxSelectReached;
        set => SetAndRaise(IsMaxSelectReachedProperty, ref _isMaxSelectReached, value);
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

    protected override void NotifyTreeItemClicked(TreeItem item)
    {
        if (ToggleType == ItemToggleType.CheckBox)
        {
            if (item.IsChecked == true)
            {
                item.SetCurrentValue(TreeItem.IsCheckedProperty, false);
            }
            else
            {
                item.SetCurrentValue(TreeItem.IsCheckedProperty, true);
            }
        }
    }
    
    protected override void PrepareTreeViewItem(TreeItem treeItem, object? item, int index, CompositeDisposable disposables)
    {
        base.PrepareTreeViewItem(treeItem, item, index, disposables);
        disposables.Add(BindUtils.RelayBind(this, IsMaxSelectReachedProperty, treeItem, TreeSelectTreeItem.IsMaxSelectReachedProperty));
    }
}