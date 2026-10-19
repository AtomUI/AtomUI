using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

public partial class TreeView
{
    private void ApplyCheckedSubTree(TreeViewItem viewItem)
    {
        if (!viewItem.IsEffectiveCheckable())
        {
            return;
        }

        var originIsMotionEnabled = IsMotionEnabled;
        try
        {
            SetCurrentValue(IsMotionEnabledProperty, false);
            var checkedItems = CollectCheckedSubTreeItems(viewItem);
            using (BeginCheckedItemsSync())
            {
                foreach (var checkedItem in checkedItems)
                {
                    if (!CheckedItems.Contains(checkedItem))
                    {
                        CheckedItems.Add(checkedItem);
                    }
                }
            }
        }
        finally
        {
            SetCurrentValue(IsMotionEnabledProperty, originIsMotionEnabled);
        }
    }

    private ISet<object> CollectCheckedSubTreeItems(TreeViewItem treeViewItem)
    {
        var expandedStates = new Dictionary<TreeViewItem, bool>();

        ExpandSubTreeForCheck(treeViewItem, expandedStates);
        var checkedItems = new HashSet<object>(GetSubTreeCheckResultCapacity(treeViewItem, expandedStates.Count));

        try
        {
            CollectCheckedSubTreeItemsCore(treeViewItem, checkedItems);

            if (!IsCheckStrictly)
            {
                var (checkedParentItems, _) = SetupParentNodeCheckedStatus(treeViewItem);
                checkedItems.UnionWith(checkedParentItems);
            }
        }
        finally
        {
            RestoreExpandedStates(expandedStates);
        }

        return checkedItems;
    }

    private void CollectCheckedSubTreeItemsCore(TreeViewItem treeViewItem, HashSet<object> checkedItems)
    {
        if (RecursiveCheckNodePredicate(treeViewItem))
        {
            treeViewItem.SetCurrentValue(TreeViewItem.IsCheckedProperty, true);
            var treeItemData = TreeItemFromContainer(treeViewItem);
            Debug.Assert(treeItemData != null);
            checkedItems.Add(treeItemData);
        }

        foreach (var childItem in treeViewItem.Items)
        {
            if (childItem != null)
            {
                var container = TreeContainerFromItem(childItem);
                if (container is TreeViewItem childTreeViewItem && childTreeViewItem.IsEffectiveCheckable())
                {
                    CollectCheckedSubTreeItemsCore(childTreeViewItem, checkedItems);
                }
            }
        }
    }

    private void ApplyUnCheckedSubTree(TreeViewItem viewItem)
    {
        if (!viewItem.IsEffectiveCheckable())
        {
            return;
        }

        var originIsMotionEnabled = IsMotionEnabled;
        try
        {
            SetCurrentValue(IsMotionEnabledProperty, false);
            var unCheckedItems = CollectUnCheckedSubTreeItems(viewItem);
            using (BeginCheckedItemsSync())
            {
                foreach (var unCheckedItem in unCheckedItems)
                {
                    CheckedItems.Remove(unCheckedItem);
                }

                var treeItemData = TreeItemFromContainer(viewItem);
                Debug.Assert(treeItemData != null);
                CheckedItems.Remove(treeItemData);
            }
        }
        finally
        {
            SetCurrentValue(IsMotionEnabledProperty, originIsMotionEnabled);
        }
    }

    private ISet<object> CollectUnCheckedSubTreeItems(TreeViewItem treeViewItem)
    {
        var expandedStates = new Dictionary<TreeViewItem, bool>();

        ExpandSubTreeForCheck(treeViewItem, expandedStates);
        var unCheckedItems = new HashSet<object>(GetSubTreeCheckResultCapacity(treeViewItem, expandedStates.Count));

        try
        {
            CollectUnCheckedSubTreeItemsCore(treeViewItem, unCheckedItems);

            if (!IsCheckStrictly)
            {
                var (_, unCheckedParentItems) = SetupParentNodeCheckedStatus(treeViewItem);
                unCheckedItems.UnionWith(unCheckedParentItems);
            }
        }
        finally
        {
            RestoreExpandedStates(expandedStates);
        }

        return unCheckedItems;
    }

    private int GetSubTreeCheckResultCapacity(TreeViewItem treeViewItem, int realizedSubTreeCount)
    {
        if (IsCheckStrictly)
        {
            return realizedSubTreeCount;
        }

        return realizedSubTreeCount + Math.Max(0, CountTreeViewItemPathDepth(treeViewItem) - 1);
    }

    private void CollectUnCheckedSubTreeItemsCore(TreeViewItem treeViewItem, HashSet<object> unCheckedItems)
    {
        if (treeViewItem.IsChecked == true && RecursiveUnCheckNodePredicate(treeViewItem))
        {
            var treeItemData = TreeItemFromContainer(treeViewItem);
            Debug.Assert(treeItemData != null);
            unCheckedItems.Add(treeItemData);
            treeViewItem.SetCurrentValue(TreeViewItem.IsCheckedProperty, false);
        }

        foreach (var childItem in treeViewItem.Items)
        {
            if (childItem != null)
            {
                var control = TreeContainerFromItem(childItem);
                if (control is TreeViewItem childTreeViewItem && childTreeViewItem.IsEffectiveCheckable())
                {
                    CollectUnCheckedSubTreeItemsCore(childTreeViewItem, unCheckedItems);
                }
            }
        }
    }

    private void ExpandSubTreeForCheck(TreeViewItem treeViewItem, Dictionary<TreeViewItem, bool> expandedStates)
    {
        var wasExpanded = treeViewItem.IsExpanded;
        expandedStates[treeViewItem] = wasExpanded;

        if (treeViewItem.Presenter?.Panel == null && !wasExpanded)
        {
            treeViewItem.SetCurrentValue(TreeViewItem.IsExpandedProperty, true);
            var topLevel = TopLevel.GetTopLevel(this);
            topLevel?.UpdateLayout();
        }

        foreach (var childItem in treeViewItem.Items)
        {
            if (childItem != null)
            {
                var container = TreeContainerFromItem(childItem);
                if (container is TreeViewItem childTreeViewItem)
                {
                    ExpandSubTreeForCheck(childTreeViewItem, expandedStates);
                }
            }
        }
    }

    private static void RestoreExpandedStates(Dictionary<TreeViewItem, bool> expandedStates)
    {
        foreach (var (item, wasExpanded) in expandedStates)
        {
            item.SetCurrentValue(TreeViewItem.IsExpandedProperty, wasExpanded);
        }
    }

    private (ISet<object>, ISet<object>) SetupParentNodeCheckedStatus(TreeViewItem viewItem)
    {
        var parent           = viewItem.Parent;
        var parentDepth      = Math.Max(0, CountTreeViewItemPathDepth(viewItem) - 1);
        var checkedParents   = new HashSet<object>(parentDepth);
        var unCheckedParents = new HashSet<object>(parentDepth);
        while (parent is TreeViewItem parentTreeItem && parentTreeItem.IsEnabled)
        {
            GetChildCheckStatus(parentTreeItem, out var isAllChecked, out var isAnyChecked);

            if (parentTreeItem.IsChecked == true && !isAllChecked)
            {
                var parentTreeItemData = TreeItemFromContainer(parentTreeItem);
                Debug.Assert(parentTreeItemData != null);
                unCheckedParents.Add(parentTreeItemData);
            }

            var originMotionEnabled = parentTreeItem.IsMotionEnabled;
            try
            {
                parentTreeItem.SetCurrentValue(TreeViewItem.IsMotionEnabledProperty, false);
                if (isAllChecked)
                {
                    parentTreeItem.SetCurrentValue(TreeViewItem.IsCheckedProperty, true);
                }
                else if (isAnyChecked)
                {
                    parentTreeItem.SetCurrentValue(TreeViewItem.IsCheckedProperty, null);
                }
                else
                {
                    parentTreeItem.SetCurrentValue(TreeViewItem.IsCheckedProperty, false);
                }
            }
            finally
            {
                parentTreeItem.SetCurrentValue(TreeViewItem.IsMotionEnabledProperty, originMotionEnabled);
            }

            if (parentTreeItem.IsChecked == true)
            {
                var parentTreeItemData = TreeItemFromContainer(parentTreeItem);
                Debug.Assert(parentTreeItemData != null);
                checkedParents.Add(parentTreeItemData);
            }
            parent = parent.Parent;
        }

        return (checkedParents, unCheckedParents);
    }

    private void GetChildCheckStatus(TreeViewItem parentTreeItem, out bool isAllChecked, out bool isAnyChecked)
    {
        isAllChecked = false;
        isAnyChecked = false;

        if (parentTreeItem.Items.Count == 0)
        {
            return;
        }

        isAllChecked = true;
        foreach (var childItem in parentTreeItem.Items)
        {
            var childSatisfiesAllChecked = false;
            if (childItem != null)
            {
                var container = TreeContainerFromItem(childItem);
                if (container is TreeViewItem treeViewItem)
                {
                    var isCheckable = treeViewItem.IsEffectiveCheckable();
                    childSatisfiesAllChecked = !isCheckable || treeViewItem.IsChecked == true;
                    if (isCheckable && treeViewItem.IsChecked != false)
                    {
                        isAnyChecked = true;
                    }
                }
            }

            if (!childSatisfiesAllChecked)
            {
                isAllChecked = false;
            }

            if (!isAllChecked && isAnyChecked)
            {
                break;
            }
        }
    }

    private void SubscribeToCheckedItems()
    {
        if (_checkedItems is INotifyCollectionChanged incc)
        {
            incc.CollectionChanged += HandleCheckedItemsCollectionChanged;
        }

        HandleCheckedItemsCollectionChanged(
            _checkedItems,
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    private void UnsubscribeFromCheckedItems()
    {
        if (_checkedItems is INotifyCollectionChanged incc)
        {
            incc.CollectionChanged -= HandleCheckedItemsCollectionChanged;
        }
    }

    private void HandleCheckedItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        IList? added = null;
        IList? removed = null;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems != null)
                {
                    if (!SyncingCheckedItems)
                    {
                        CheckedItemsAdded(e.NewItems);
                    }
                    added = e.NewItems;
                }
                break;
            case NotifyCollectionChangedAction.Remove:
                if (!SyncingCheckedItems && e.OldItems != null)
                {
                    for (var i = 0; i < e.OldItems.Count; i++)
                    {
                        MarkItemChecked(e.OldItems[i]!, false);
                    }
                }

                removed = e.OldItems;
                break;
            case NotifyCollectionChangedAction.Reset:
                if (!SyncingCheckedItems)
                {
                    foreach (var container in GetRealizedTreeContainers())
                    {
                        MarkContainerChecked(container, false);
                    }
                    if (e.NewItems?.Count > 0)
                    {
                        CheckedItemsAdded(e.NewItems);
                    }
                }

                if (e.NewItems?.Count > 0)
                {
                    added = new List<object>(CheckedItems.Count);
                    foreach (var item in CheckedItems)
                    {
                        added.Add(item);
                    }
                }
                break;
            case NotifyCollectionChangedAction.Replace:
                if (!SyncingCheckedItems)
                {
                    if (e.OldItems != null)
                    {
                        for (var i = 0; i < e.OldItems.Count; i++)
                        {
                            MarkItemChecked(e.OldItems[i]!, false);
                        }
                    }

                    if (e.NewItems != null)
                    {
                        for (var i = 0; i < e.NewItems.Count; i++)
                        {
                            MarkItemChecked(e.NewItems[i]!, true);
                        }
                    }
                }

                added = e.NewItems;
                removed = e.OldItems;
                break;
        }
        RecordExplicitCheckedUpdateDuringReplay(false);
        if (!IsItemsSourceReplayPending && (added?.Count > 0 || removed?.Count > 0))
        {
            CheckedItemsChanged?.Invoke(this, new TreeViewCheckedItemsChangedEventArgs(
                removed ?? Empty,
                added ?? Empty));
        }
    }

    private void CheckedItemsAdded(IList items)
    {
        if (items.Count == 0)
        {
            return;
        }
        foreach (var item in items)
        {
            MarkItemChecked(item, true);
        }
    }

    private void MarkItemChecked(object item, bool isChecked)
    {
        var container = TreeContainerFromItem(item);
        if (container != null)
        {
            MarkContainerChecked(container, isChecked);
        }
    }

    private static void MarkContainerChecked(Control container, bool isChecked)
    {
        container.SetCurrentValue(TreeViewItem.IsCheckedProperty, isChecked);
    }

    internal IDisposable BeginCheckedItemsSync()
    {
        return new CheckedItemsSyncScope(this);
    }

    private sealed class CheckedItemsSyncScope : IDisposable
    {
        private readonly TreeView _treeView;

        public CheckedItemsSyncScope(TreeView treeView)
        {
            _treeView = treeView;
            treeView.SyncingCheckedItems = true;
        }

        public void Dispose()
        {
            _treeView.SyncingCheckedItems = false;
        }
    }
}
