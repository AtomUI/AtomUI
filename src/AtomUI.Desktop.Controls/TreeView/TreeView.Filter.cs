using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

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

    internal ISet<TreeViewItem> SelectedItemsClosure = new HashSet<TreeViewItem>();

    private static void ConfigureFilter()
    {
        SelectingItemsControl.SelectionChangedEvent.AddClassHandler<TreeView>((treeView, args) => treeView.HandleSelectionChanged());
    }

    private void HandleSelectionChanged()
    {
        if (IsFilterMode)
        {
            SelectedItemsClosure.Clear();
            var startupItems = new List<TreeViewItem>();
            if (SelectionMode.HasFlag(SelectionMode.Single))
            {
                if (SelectedItem != null && TreeContainerFromItem(SelectedItem) is TreeViewItem item)
                {
                    startupItems.Add(item);
                }
            }
            else if (SelectionMode.HasFlag(SelectionMode.Multiple))
            {
                foreach (var entry in SelectedItems)
                {
                    if (entry != null && TreeContainerFromItem(entry) is TreeViewItem item)
                    {
                        startupItems.Add(item);
                    }
                }
            }

            foreach (var item in startupItems)
            {
                var closure = CalculateSelectItemClosure(item);
                SelectedItemsClosure.UnionWith(closure);
            }
        }
    }

    private ISet<TreeViewItem> CalculateSelectItemClosure(TreeViewItem treeItem)
    {
        var closure = new HashSet<TreeViewItem>(); 
        StyledElement? current = treeItem;
        while (current != null && current is TreeViewItem currentTreeViewItem)
        {
            closure.Add(currentTreeViewItem);
            current = current.Parent;
        }

        return closure;
    }
    
    public void Filter()
    {
        if (ItemFilter != null && ItemFilterValue != null && IsLoaded)
        {
            if (ItemFilterValue is string strFilterValue && string.IsNullOrWhiteSpace(strFilterValue))
            {
                using (BeginTurnOffMotion())
                {
                    ClearFilter();
                    return;
                }
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
            using var state = BeginTurnOffMotion();
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
            if (ItemFilterAction.HasFlag(TreeItemFilterAction.HighlightedMatch) ||
                ItemFilterAction.HasFlag(TreeItemFilterAction.HighlightedWhole))
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
        SelectedItemsClosure.Clear();
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

    private IDisposable BeginTurnOffMotion()
    {
        var originMotionEnabled = IsMotionEnabled;
        var disposable = new MotionScopeDisposable(this, originMotionEnabled);
        SetCurrentValue(IsMotionEnabledProperty, false);
        return disposable;
    }

    private class MotionScopeDisposable : IDisposable
    {
        private readonly TreeView _treeView;
        private bool _originMotionEnabled;
        public MotionScopeDisposable(TreeView treeView, bool originMotionEnabled)
        {
            _treeView            = treeView;
            _originMotionEnabled = originMotionEnabled;
        }
        public void Dispose()
        {
            _treeView.SetCurrentValue(TreeView.IsMotionEnabledProperty, _originMotionEnabled);
        }
    }
}