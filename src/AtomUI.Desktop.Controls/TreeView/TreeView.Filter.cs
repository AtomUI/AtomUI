using Avalonia;

namespace AtomUI.Desktop.Controls;

public partial class TreeView
{
    internal static readonly DirectProperty<TreeViewItem, bool> IsFilterModeProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItem, bool>(nameof(IsFilterMode),
            o => o.IsFilterMode,
            (o, v) => o.IsFilterMode = v);
    
    private bool _isFilterMode;

    internal bool IsFilterMode
    {
        get => _isFilterMode;
        set => SetAndRaise(IsFilterModeProperty, ref _isFilterMode, value);
    }
    
    public void Filter()
    {
        if (ItemFilter != null && ItemFilterValue != null && IsLoaded)
        {
            if (ItemFilterValue is string strFilterValue && string.IsNullOrWhiteSpace(strFilterValue))
            {
                ClearFilter();
                return;
            }

            var originIsFilterMode = IsFilterMode;
            IsFilterMode = true;
            HashSet<TreeViewItem>? originExpandedItems = null;
            if (!ItemFilterAction.HasFlag(TreeItemFilterAction.ExpandPath))
            {
                originExpandedItems = new HashSet<TreeViewItem>();
                for (int i = 0; i < ItemCount; i++)
                {
                    if (ContainerFromIndex(i) is TreeViewItem item)
                    {
                        originExpandedItems.UnionWith(CollectExpandedItems(item));
                    }
                }
            }
 
            ExpandAll(false); // TODO 这样合适吗？
            if (!ItemFilterAction.HasFlag(TreeItemFilterAction.ExpandPath) && originExpandedItems != null)
            {
                RestoreItemExpandedStates(originExpandedItems);
            }

            if (!originIsFilterMode)
            {
                for (int i = 0; i < ItemCount; i++)
                {
                    if (ContainerFromIndex(i) is TreeViewItem item)
                    {
                        BackupStateForFilterMode(item);
                    }
                }
            }
         
            for (var i = 0; i < ItemCount; i++)
            {
                if (ContainerFromIndex(i) is TreeViewItem treeViewItem)
                {
                    FilterItem(treeViewItem);
                }
            }
        }
        else
        {
            ClearFilter();
        }
    }

    private void FilterItem(TreeViewItem treeItem)
    {
        if (ItemFilter == null)
        {
            return;
        }
        for (var i = 0; i < treeItem.ItemCount; i++)
        {
            if (treeItem.ContainerFromIndex(i) is TreeViewItem treeViewItem)
            {
                FilterItem(treeViewItem);
            }
        }
        treeItem.IsFilterMode = true;
        var filterResult = ItemFilter.Filter(this, treeItem, ItemFilterValue);
        treeItem.IsFilterMatch = filterResult;
        if (filterResult)
        {
            if (ItemFilterAction.HasFlag(TreeItemFilterAction.Highlighted))
            {
                treeItem.FilterHighlightWords = ItemFilterValue?.ToString();
            }
        }
        
        if (ItemFilterAction.HasFlag(TreeItemFilterAction.HideUnMatched))
        {
            if (treeItem.IsFilterMatch)
            {
                var current = treeItem.Parent;
                while (current != null)
                {
                    if (current is TreeViewItem item)
                    {
                        item.SetCurrentValue(TreeViewItem.IsVisibleProperty, true);
                    }
                    current = current.Parent;
                }
                treeItem.SetCurrentValue(TreeViewItem.IsVisibleProperty, true);
            }
            else if (!HasChildOrDescendantsMatchFilter(treeItem))
            {
                treeItem.SetCurrentValue(TreeViewItem.IsVisibleProperty, false);
            }
        }

        if (ItemFilterAction.HasFlag(TreeItemFilterAction.ExpandPath))
        {
            SetupExpandForFilter(treeItem);
        }
    }

    private void SetupExpandForFilter(TreeViewItem treeViewItem)
    {
        var hasChildOrDescendantsMatchFilter = HasChildOrDescendantsMatchFilter(treeViewItem);
        if (hasChildOrDescendantsMatchFilter || treeViewItem.IsFilterMatch)
        {
            var current = treeViewItem.Parent;
            while (current != null && current is TreeViewItem)
            {
                if (current is TreeViewItem item)
                {
                    item.SetCurrentValue(TreeViewItem.IsExpandedProperty, true);
                }
                current =  current.Parent;
            }
        }

        if (!hasChildOrDescendantsMatchFilter)
        {
            treeViewItem.SetCurrentValue(TreeViewItem.IsExpandedProperty, false);
        }
    }

    private bool HasChildOrDescendantsMatchFilter(TreeViewItem treeViewItem)
    {
        for (int i = 0; i < treeViewItem.ItemCount; i++)
        {
            if (treeViewItem.ContainerFromIndex(i) is TreeViewItem childItem)
            {
                if (HasChildOrDescendantsMatchFilter(childItem))
                {
                    return true;
                }
            }
        }
        return treeViewItem.IsFilterMatch;
    }
    
    private void BackupStateForFilterMode(TreeViewItem treeViewItem)
    {
        for (int i = 0; i < treeViewItem.ItemCount; i++)
        {
            if (treeViewItem.ContainerFromIndex(i) is TreeViewItem childItem)
            {
                BackupStateForFilterMode(childItem);
            }
        }
        treeViewItem.CreateFilterContextBackup();
    }

    private void ClearFilter()
    {
        if (!IsFilterMode)
        {
            return;
        }

        for (var i = 0; i < ItemCount; i++)
        {
            if (ContainerFromIndex(i) is TreeViewItem treeViewItem)
            {
                ClearItemFilterMode(treeViewItem);
            }
        }

        IsFilterMode = false;
    }
    
    private void ClearItemFilterMode(TreeViewItem treeItem)
    {
        for (var i = 0; i < treeItem.ItemCount; i++)
        {
            if (treeItem.ContainerFromIndex(i) is TreeViewItem childTreeItem)
            {
                ClearItemFilterMode(childTreeItem);
            }
        }
        treeItem.ClearFilterMode();
    }

    private ISet<TreeViewItem> CollectExpandedItems(TreeViewItem treeItem)
    {
        var expandedItems = new HashSet<TreeViewItem>();
        for (var i = 0; i < treeItem.ItemCount; i++)
        {
            if (treeItem.ContainerFromIndex(i) is TreeViewItem childTreeItem)
            {
                expandedItems.UnionWith(CollectExpandedItems(childTreeItem));
            }
        }

        if (treeItem.IsExpanded)
        {
            expandedItems.Add(treeItem);
        }
        return expandedItems;
    }

    private void RestoreItemExpandedStates(ISet<TreeViewItem> originExpandedItems)
    {
        
        var originMotionEnabled = IsMotionEnabled;
        try
        {
            IsExpandAllProcess = true;
            SetCurrentValue(IsMotionEnabledProperty, false);

            for (int i = 0; i < ItemCount; i++)
            {
                if (ContainerFromIndex(i) is TreeViewItem item)
                {
                    RestoreItemExpandedState(item, originExpandedItems);
                }
            }
        }
        finally
        {
            IsExpandAllProcess = false;
            SetCurrentValue(IsMotionEnabledProperty, originMotionEnabled);
        }
        
        
    }

    private void RestoreItemExpandedState(TreeViewItem treeItem, in ISet<TreeViewItem> expandedItems)
    {
        for (var i = 0; i < treeItem.ItemCount; i++)
        {
            if (treeItem.ContainerFromIndex(i) is TreeViewItem childTreeItem)
            {
                RestoreItemExpandedState(childTreeItem, expandedItems);
            }
        }

        if (expandedItems.Contains(treeItem))
        {
            treeItem.SetCurrentValue(TreeViewItem.IsExpandedProperty, true);
        }
        else
        {
            treeItem.SetCurrentValue(TreeViewItem.IsExpandedProperty, false);
        }
    }
}