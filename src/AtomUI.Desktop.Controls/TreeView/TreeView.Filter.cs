using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

public partial class TreeView
{
    internal static readonly DirectProperty<TreeView, bool> IsFilterModeProperty =
        AvaloniaProperty.RegisterDirect<TreeView, bool>(nameof(IsFilterMode),
            o => o.IsFilterMode,
            (o, v) => o.IsFilterMode = v);
    
    private bool _isFilterMode;

    internal bool IsFilterMode
    {
        get => _isFilterMode;
        set => SetAndRaise(IsFilterModeProperty, ref _isFilterMode, value);
    }

    internal ISet<TreeViewItem> SelectedItemsClosure = new HashSet<TreeViewItem>();

    private HashSet<TreeViewItem>? _descendantsMatchCache;

    private static void ConfigureFilter()
    {
        SelectingItemsControl.SelectionChangedEvent.AddClassHandler<TreeView>((treeView, args) => treeView.HandleSelectionChanged());
    }

    private void HandleSelectionChanged()
    {
        if (IsFilterMode)
        {
            SelectedItemsClosure.Clear();
            var isMultiple = (SelectionMode & SelectionMode.Multiple) == SelectionMode.Multiple;
            if (!isMultiple)
            {
                if (SelectedItem != null && TreeContainerFromItem(SelectedItem) is TreeViewItem item)
                {
                    SelectedItemsClosure.UnionWith(CalculateSelectItemClosure(item));
                }
            }
            else
            {
                foreach (var entry in SelectedItems)
                {
                    if (entry != null && TreeContainerFromItem(entry) is TreeViewItem item)
                    {
                        SelectedItemsClosure.UnionWith(CalculateSelectItemClosure(item));
                    }
                }
            }
        }
    }

    private ISet<TreeViewItem> CalculateSelectItemClosure(TreeViewItem treeViewItem)
    {
        var closure = new HashSet<TreeViewItem>(CountTreeViewItemPathDepth(treeViewItem));
        StyledElement? current = treeViewItem;
        while (current != null && current is TreeViewItem currentTreeViewItem)
        {
            closure.Add(currentTreeViewItem);
            current = current.Parent;
        }

        return closure;
    }

    private static int CountTreeViewItemPathDepth(TreeViewItem treeViewItem)
    {
        var count = 0;
        StyledElement? current = treeViewItem;
        while (current != null && current is TreeViewItem)
        {
            count++;
            current = current.Parent;
        }

        return count;
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
            HashSet<TreeViewItem>? originExpandedItems = null;
            var filterStrategy = FilterStrategy;
            var shouldExpandPath =
                (filterStrategy & TreeFilterStrategy.ExpandPath) == TreeFilterStrategy.ExpandPath;
            if (!shouldExpandPath)
            {
                originExpandedItems = new HashSet<TreeViewItem>();
                for (int i = 0; i < ItemCount; i++)
                {
                    if (ContainerFromIndex(i) is TreeViewItem item)
                    {
                        CollectExpandedItems(item, originExpandedItems);
                    }
                }
            }

            ExpandAll(false);
            using var state = BeginTurnOffMotion();
            if (!shouldExpandPath && originExpandedItems != null)
            {
                RestoreItemExpandedStates(originExpandedItems);
            }

            var needBackup = !originIsFilterMode;
            _descendantsMatchCache = new HashSet<TreeViewItem>();

            for (var i = 0; i < ItemCount; i++)
            {
                if (GetFilterRootItem(i) is TreeViewItem treeViewItem)
                {
                    FilterItem(treeViewItem, needBackup);
                }
            }

            _descendantsMatchCache = null;
            ConfigureEmptyIndicator();
        }
        else
        {
            ClearFilter();
        }
    }

    private TreeViewItem? GetFilterRootItem(int index)
    {
        return ContainerFromIndex(index) as TreeViewItem ??
               Items[index] as TreeViewItem;
    }

    private static TreeViewItem? GetFilterChildItem(TreeViewItem treeViewItem, int index)
    {
        return treeViewItem.ContainerFromIndex(index) as TreeViewItem ??
               treeViewItem.Items[index] as TreeViewItem;
    }

    private void FilterItem(TreeViewItem treeViewItem, bool needBackup)
    {
        if (Filter == null)
        {
            return;
        }

        var anyDescendantMatched = false;
        for (var i = 0; i < treeViewItem.ItemCount; i++)
        {
            if (GetFilterChildItem(treeViewItem, i) is TreeViewItem childTreeViewItem)
            {
                FilterItem(childTreeViewItem, needBackup);
                if (_descendantsMatchCache!.Contains(childTreeViewItem))
                {
                    anyDescendantMatched = true;
                }
            }
        }

        if (needBackup)
        {
            treeViewItem.CreateFilterContextBackup();
        }

        treeViewItem.IsFilterMode = true;
        var selector     = FilterValueSelector ?? DefaultTreeFilterValueSelector;
        var filterResult = Filter.Filter(selector(treeViewItem), FilterValue);
        treeViewItem.IsFilterMatch = filterResult;
        var filterStrategy = FilterStrategy;
        if (filterResult)
        {
            ++FilterResultCount;
            if ((filterStrategy & TreeFilterStrategy.HighlightedMatch) == TreeFilterStrategy.HighlightedMatch ||
                (filterStrategy & TreeFilterStrategy.HighlightedWhole) == TreeFilterStrategy.HighlightedWhole)
            {
                treeViewItem.FilterHighlightWords = FilterValue?.ToString();
            }
        }

        if (filterResult || anyDescendantMatched)
        {
            _descendantsMatchCache!.Add(treeViewItem);
        }

        if ((filterStrategy & TreeFilterStrategy.HideUnMatched) == TreeFilterStrategy.HideUnMatched)
        {
            if (treeViewItem.IsFilterMatch)
            {
                var current = treeViewItem.Parent;
                while (current != null)
                {
                    if (current is TreeViewItem item)
                    {
                        item.SetCurrentValue(TreeViewItem.IsVisibleProperty, true);
                    }
                    current = current.Parent;
                }
                treeViewItem.SetCurrentValue(TreeViewItem.IsVisibleProperty, true);
            }
            else if (!anyDescendantMatched)
            {
                treeViewItem.SetCurrentValue(TreeViewItem.IsVisibleProperty, false);
            }
        }

        if ((filterStrategy & TreeFilterStrategy.ExpandPath) == TreeFilterStrategy.ExpandPath)
        {
            SetupExpandForFilter(treeViewItem, anyDescendantMatched);
        }
    }

    private void SetupExpandForFilter(TreeViewItem treeViewItem, bool hasDescendantMatch)
    {
        if (hasDescendantMatch || treeViewItem.IsFilterMatch)
        {
            var current = treeViewItem.Parent;
            while (current is TreeViewItem item)
            {
                item.SetCurrentValue(TreeViewItem.IsExpandedProperty, true);
                current = item.Parent;
            }
        }

        if (!hasDescendantMatch && !treeViewItem.IsFilterMatch)
        {
            treeViewItem.SetCurrentValue(TreeViewItem.IsExpandedProperty, false);
        }
    }

    private void ClearFilter()
    {
        if (!IsFilterMode)
        {
            return;
        }

        for (var i = 0; i < ItemCount; i++)
        {
            if (GetFilterRootItem(i) is TreeViewItem treeViewItem)
            {
                ClearItemFilterMode(treeViewItem);
            }
        }

        IsFilterMode      = false;
        FilterResultCount = 0;
        SelectedItemsClosure.Clear();
    }
    
    private void ClearItemFilterMode(TreeViewItem treeViewItem)
    {
        for (var i = 0; i < treeViewItem.ItemCount; i++)
        {
            if (GetFilterChildItem(treeViewItem, i) is TreeViewItem childTreeItem)
            {
                ClearItemFilterMode(childTreeItem);
            }
        }
        treeViewItem.ClearFilterMode();
    }

    private void CollectExpandedItems(TreeViewItem treeViewItem, HashSet<TreeViewItem> expandedItems)
    {
        for (var i = 0; i < treeViewItem.ItemCount; i++)
        {
            if (GetFilterChildItem(treeViewItem, i) is TreeViewItem childTreeItem)
            {
                CollectExpandedItems(childTreeItem, expandedItems);
            }
        }

        if (treeViewItem.IsExpanded)
        {
            expandedItems.Add(treeViewItem);
        }
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
                if (GetFilterRootItem(i) is TreeViewItem item)
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

    private void RestoreItemExpandedState(TreeViewItem treeViewItem, in ISet<TreeViewItem> expandedItems)
    {
        for (var i = 0; i < treeViewItem.ItemCount; i++)
        {
            if (GetFilterChildItem(treeViewItem, i) is TreeViewItem childTreeItem)
            {
                RestoreItemExpandedState(childTreeItem, expandedItems);
            }
        }

        if (expandedItems.Contains(treeViewItem))
        {
            treeViewItem.SetCurrentValue(TreeViewItem.IsExpandedProperty, true);
        }
        else
        {
            treeViewItem.SetCurrentValue(TreeViewItem.IsExpandedProperty, false);
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
