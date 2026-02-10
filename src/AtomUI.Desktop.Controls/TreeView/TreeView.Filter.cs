using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

public partial class TreeView
{
    internal static readonly DirectProperty<TreeItem, bool> IsFilterModeProperty =
        AvaloniaProperty.RegisterDirect<TreeItem, bool>(nameof(IsFilterMode),
            o => o.IsFilterMode,
            (o, v) => o.IsFilterMode = v);
    
    private bool _isFilterMode;

    internal bool IsFilterMode
    {
        get => _isFilterMode;
        set => SetAndRaise(IsFilterModeProperty, ref _isFilterMode, value);
    }

    internal ISet<TreeItem> SelectedItemsClosure = new HashSet<TreeItem>();

    private static void ConfigureFilter()
    {
        SelectingItemsControl.SelectionChangedEvent.AddClassHandler<TreeView>((treeView, args) => treeView.HandleSelectionChanged());
    }

    private void HandleSelectionChanged()
    {
        if (IsFilterMode)
        {
            SelectedItemsClosure.Clear();
            var startupItems = new List<TreeItem>();
            if (SelectionMode.HasFlag(SelectionMode.Single))
            {
                if (SelectedItem != null && TreeContainerFromItem(SelectedItem) is TreeItem item)
                {
                    startupItems.Add(item);
                }
            }
            else if (SelectionMode.HasFlag(SelectionMode.Multiple))
            {
                foreach (var entry in SelectedItems)
                {
                    if (entry != null && TreeContainerFromItem(entry) is TreeItem item)
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

    private ISet<TreeItem> CalculateSelectItemClosure(TreeItem treeItem)
    {
        var closure = new HashSet<TreeItem>(); 
        StyledElement? current = treeItem;
        while (current != null && current is TreeItem currentTreeViewItem)
        {
            closure.Add(currentTreeViewItem);
            current = current.Parent;
        }

        return closure;
    }
    
    public void FilterTreeNode()
    {
        if (Filter != null && FilterValue != null && IsLoaded)
        {
            if (FilterValue is string strFilterValue && string.IsNullOrWhiteSpace(strFilterValue))
            {
                using (BeginTurnOffMotion())
                {
                    ClearFilter();
                    return;
                }
            }

            var originIsFilterMode = IsFilterMode;
            IsFilterMode      = true;
            FilterResultCount = 0;
            HashSet<TreeItem>? originExpandedItems = null;
            if (!FilterHighlightStrategy.HasFlag(TreeFilterHighlightStrategy.ExpandPath))
            {
                originExpandedItems = new HashSet<TreeItem>();
                for (int i = 0; i < ItemCount; i++)
                {
                    if (ContainerFromIndex(i) is TreeItem item)
                    {
                        originExpandedItems.UnionWith(CollectExpandedItems(item));
                    }
                }
            }
 
            ExpandAll(false); // TODO 这样合适吗？
            using var state = BeginTurnOffMotion();
            if (!FilterHighlightStrategy.HasFlag(TreeFilterHighlightStrategy.ExpandPath) && originExpandedItems != null)
            {
                RestoreItemExpandedStates(originExpandedItems);
            }

            if (!originIsFilterMode)
            {
                for (int i = 0; i < ItemCount; i++)
                {
                    if (ContainerFromIndex(i) is TreeItem item)
                    {
                        BackupStateForFilterMode(item);
                    }
                }
            }
         
            for (var i = 0; i < ItemCount; i++)
            {
                if (ContainerFromIndex(i) is TreeItem treeViewItem)
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

    private void FilterItem(TreeItem treeItem)
    {
        if (Filter == null)
        {
            return;
        }
        for (var i = 0; i < treeItem.ItemCount; i++)
        {
            if (treeItem.ContainerFromIndex(i) is TreeItem treeViewItem)
            {
                FilterItem(treeViewItem);
            }
        }
        treeItem.IsFilterMode = true;
        var filterResult = Filter.Filter(this, treeItem, FilterValue);
        treeItem.IsFilterMatch = filterResult;
        if (filterResult)
        {
            ++FilterResultCount;
            if (FilterHighlightStrategy.HasFlag(TreeFilterHighlightStrategy.HighlightedMatch) ||
                FilterHighlightStrategy.HasFlag(TreeFilterHighlightStrategy.HighlightedWhole))
            {
                treeItem.FilterHighlightWords = FilterValue?.ToString();
            }
        }
        ConfigureEmptyIndicator();
        
        if (FilterHighlightStrategy.HasFlag(TreeFilterHighlightStrategy.HideUnMatched))
        {
            if (treeItem.IsFilterMatch)
            {
                var current = treeItem.Parent;
                while (current != null)
                {
                    if (current is TreeItem item)
                    {
                        item.SetCurrentValue(TreeItem.IsVisibleProperty, true);
                    }
                    current = current.Parent;
                }
                treeItem.SetCurrentValue(TreeItem.IsVisibleProperty, true);
            }
            else if (!HasChildOrDescendantsMatchFilter(treeItem))
            {
                treeItem.SetCurrentValue(TreeItem.IsVisibleProperty, false);
            }
        }

        if (FilterHighlightStrategy.HasFlag(TreeFilterHighlightStrategy.ExpandPath))
        {
            SetupExpandForFilter(treeItem);
        }
    }

    private void SetupExpandForFilter(TreeItem treeItem)
    {
        var hasChildOrDescendantsMatchFilter = HasChildOrDescendantsMatchFilter(treeItem);
        if (hasChildOrDescendantsMatchFilter || treeItem.IsFilterMatch)
        {
            var current = treeItem.Parent;
            while (current != null && current is TreeItem)
            {
                if (current is TreeItem item)
                {
                    item.SetCurrentValue(TreeItem.IsExpandedProperty, true);
                }
                current =  current.Parent;
            }
        }

        if (!hasChildOrDescendantsMatchFilter)
        {
            treeItem.SetCurrentValue(TreeItem.IsExpandedProperty, false);
        }
    }

    private bool HasChildOrDescendantsMatchFilter(TreeItem treeItem)
    {
        for (int i = 0; i < treeItem.ItemCount; i++)
        {
            if (treeItem.ContainerFromIndex(i) is TreeItem childItem)
            {
                if (HasChildOrDescendantsMatchFilter(childItem))
                {
                    return true;
                }
            }
        }
        return treeItem.IsFilterMatch;
    }
    
    private void BackupStateForFilterMode(TreeItem treeItem)
    {
        for (int i = 0; i < treeItem.ItemCount; i++)
        {
            if (treeItem.ContainerFromIndex(i) is TreeItem childItem)
            {
                BackupStateForFilterMode(childItem);
            }
        }
        treeItem.CreateFilterContextBackup();
    }

    private void ClearFilter()
    {
        if (!IsFilterMode)
        {
            return;
        }

        for (var i = 0; i < ItemCount; i++)
        {
            if (ContainerFromIndex(i) is TreeItem treeViewItem)
            {
                ClearItemFilterMode(treeViewItem);
            }
        }

        IsFilterMode      = false;
        FilterResultCount = 0;
        SelectedItemsClosure.Clear();
    }
    
    private void ClearItemFilterMode(TreeItem treeItem)
    {
        for (var i = 0; i < treeItem.ItemCount; i++)
        {
            if (treeItem.ContainerFromIndex(i) is TreeItem childTreeItem)
            {
                ClearItemFilterMode(childTreeItem);
            }
        }
        treeItem.ClearFilterMode();
    }

    private ISet<TreeItem> CollectExpandedItems(TreeItem treeItem)
    {
        var expandedItems = new HashSet<TreeItem>();
        for (var i = 0; i < treeItem.ItemCount; i++)
        {
            if (treeItem.ContainerFromIndex(i) is TreeItem childTreeItem)
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

    private void RestoreItemExpandedStates(ISet<TreeItem> originExpandedItems)
    {
        
        var originMotionEnabled = IsMotionEnabled;
        try
        {
            IsExpandAllProcess = true;
            SetCurrentValue(IsMotionEnabledProperty, false);

            for (int i = 0; i < ItemCount; i++)
            {
                if (ContainerFromIndex(i) is TreeItem item)
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

    private void RestoreItemExpandedState(TreeItem treeItem, in ISet<TreeItem> expandedItems)
    {
        for (var i = 0; i < treeItem.ItemCount; i++)
        {
            if (treeItem.ContainerFromIndex(i) is TreeItem childTreeItem)
            {
                RestoreItemExpandedState(childTreeItem, expandedItems);
            }
        }

        if (expandedItems.Contains(treeItem))
        {
            treeItem.SetCurrentValue(TreeItem.IsExpandedProperty, true);
        }
        else
        {
            treeItem.SetCurrentValue(TreeItem.IsExpandedProperty, false);
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