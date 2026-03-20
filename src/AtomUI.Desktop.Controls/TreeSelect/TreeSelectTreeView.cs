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
        return new TreeViewSelectTreeViewItem();
    }

    protected override bool NeedsContainerOverride(
        object? item,
        int index,
        out object? recycleKey)
    {
        return NeedsContainer<TreeViewSelectTreeViewItem>(item, out recycleKey);
    }

    protected override void NotifyTreeItemClicked(TreeViewItem viewItem)
    {
        if (ToggleType == ItemToggleType.CheckBox)
        {
            if (viewItem.IsChecked == true)
            {
                viewItem.SetCurrentValue(TreeViewItem.IsCheckedProperty, false);
            }
            else
            {
                viewItem.SetCurrentValue(TreeViewItem.IsCheckedProperty, true);
            }
        }
    }
    
    protected override void PrepareTreeViewItem(TreeViewItem treeViewItem, object? item, int index, CompositeDisposable disposables)
    {
        base.PrepareTreeViewItem(treeViewItem, item, index, disposables);
        disposables.Add(BindUtils.RelayBind(this, IsMaxSelectReachedProperty, treeViewItem, TreeViewSelectTreeViewItem.IsMaxSelectReachedProperty));
    }
}