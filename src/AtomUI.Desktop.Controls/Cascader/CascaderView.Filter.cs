using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

public partial class CascaderView
{
    internal static readonly DirectProperty<CascaderViewItem, bool> IsFilterModeProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewItem, bool>(nameof(IsFilterMode),
            o => o.IsFilterMode,
            (o, v) => o.IsFilterMode = v);
    
    private bool _isFilterMode;

    internal bool IsFilterMode
    {
        get => _isFilterMode;
        set => SetAndRaise(IsFilterModeProperty, ref _isFilterMode, value);
    }

    internal ISet<CascaderViewItem> SelectedItemsClosure = new HashSet<CascaderViewItem>();

    private static void ConfigureFilter()
    {
        SelectingItemsControl.SelectionChangedEvent.AddClassHandler<CascaderView>((cascaderView, args) => cascaderView.HandleSelectionChanged());
    }

    private void HandleSelectionChanged()
    {
        if (IsFilterMode)
        {
            SelectedItemsClosure.Clear();
            var startupItems = new List<CascaderViewItem>();
            if (SelectionMode.HasFlag(SelectionMode.Single))
            {
                if (SelectedItem != null && TreeContainerFromItem(SelectedItem) is CascaderViewItem item)
                {
                    startupItems.Add(item);
                }
            }
            else if (SelectionMode.HasFlag(SelectionMode.Multiple))
            {
                foreach (var entry in SelectedItems)
                {
                    if (entry != null && TreeContainerFromItem(entry) is CascaderViewItem item)
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

    private ISet<CascaderViewItem> CalculateSelectItemClosure(CascaderViewItem cascaderViewItem)
    {
        var            closure = new HashSet<CascaderViewItem>(); 
        StyledElement? current = cascaderViewItem;
        while (current != null && current is CascaderViewItem currentCascaderViewItem)
        {
            closure.Add(currentCascaderViewItem);
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
            IsFilterMode      = true;
            FilterResultCount = 0;
            HashSet<CascaderViewItem>? originExpandedItems = null;
            if (!ItemFilterAction.HasFlag(TreeItemFilterAction.ExpandPath))
            {
                originExpandedItems = new HashSet<CascaderViewItem>();
                for (int i = 0; i < ItemCount; i++)
                {
                    if (ContainerFromIndex(i) is CascaderViewItem item)
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
                    if (ContainerFromIndex(i) is CascaderViewItem item)
                    {
                        BackupStateForFilterMode(item);
                    }
                }
            }
         
            for (var i = 0; i < ItemCount; i++)
            {
                if (ContainerFromIndex(i) is CascaderViewItem cascaderViewItem)
                {
                    FilterItem(cascaderViewItem);
                }
            }
        }
        else
        {
            ClearFilter();
        }
    }

    private void FilterItem(CascaderViewItem cascaderViewItem)
    {
        if (ItemFilter == null)
        {
            return;
        }
        for (var i = 0; i < cascaderViewItem.ItemCount; i++)
        {
            if (cascaderViewItem.ContainerFromIndex(i) is CascaderViewItem childViewItem)
            {
                FilterItem(childViewItem);
            }
        }
        cascaderViewItem.IsFilterMode = true;
        var filterResult = ItemFilter.Filter(this, cascaderViewItem, ItemFilterValue);
        cascaderViewItem.IsFilterMatch = filterResult;
        if (filterResult)
        {
            ++FilterResultCount;
            if (ItemFilterAction.HasFlag(TreeItemFilterAction.HighlightedMatch) ||
                ItemFilterAction.HasFlag(TreeItemFilterAction.HighlightedWhole))
            {
                cascaderViewItem.FilterHighlightWords = ItemFilterValue?.ToString();
            }
        }
        ConfigureEmptyIndicator();
        
        if (ItemFilterAction.HasFlag(TreeItemFilterAction.HideUnMatched))
        {
            if (cascaderViewItem.IsFilterMatch)
            {
                var current = cascaderViewItem.Parent;
                while (current != null)
                {
                    if (current is CascaderViewItem item)
                    {
                        item.SetCurrentValue(CascaderViewItem.IsVisibleProperty, true);
                    }
                    current = current.Parent;
                }
                cascaderViewItem.SetCurrentValue(CascaderViewItem.IsVisibleProperty, true);
            }
            else if (!HasChildOrDescendantsMatchFilter(cascaderViewItem))
            {
                cascaderViewItem.SetCurrentValue(CascaderViewItem.IsVisibleProperty, false);
            }
        }

        if (ItemFilterAction.HasFlag(TreeItemFilterAction.ExpandPath))
        {
            SetupExpandForFilter(cascaderViewItem);
        }
    }

    private void SetupExpandForFilter(CascaderViewItem cascaderViewItem)
    {
        var hasChildOrDescendantsMatchFilter = HasChildOrDescendantsMatchFilter(cascaderViewItem);
        if (hasChildOrDescendantsMatchFilter || cascaderViewItem.IsFilterMatch)
        {
            var current = cascaderViewItem.Parent;
            while (current != null && current is CascaderViewItem)
            {
                if (current is CascaderViewItem item)
                {
                    item.SetCurrentValue(CascaderViewItem.IsExpandedProperty, true);
                }
                current =  current.Parent;
            }
        }

        if (!hasChildOrDescendantsMatchFilter)
        {
            cascaderViewItem.SetCurrentValue(CascaderViewItem.IsExpandedProperty, false);
        }
    }

    private bool HasChildOrDescendantsMatchFilter(CascaderViewItem cascaderViewItem)
    {
        for (int i = 0; i < cascaderViewItem.ItemCount; i++)
        {
            if (cascaderViewItem.ContainerFromIndex(i) is CascaderViewItem childItem)
            {
                if (HasChildOrDescendantsMatchFilter(childItem))
                {
                    return true;
                }
            }
        }
        return cascaderViewItem.IsFilterMatch;
    }
    
    private void BackupStateForFilterMode(CascaderViewItem cascaderViewItem)
    {
        for (int i = 0; i < cascaderViewItem.ItemCount; i++)
        {
            if (cascaderViewItem.ContainerFromIndex(i) is CascaderViewItem childItem)
            {
                BackupStateForFilterMode(childItem);
            }
        }
        cascaderViewItem.CreateFilterContextBackup();
    }

    private void ClearFilter()
    {
        if (!IsFilterMode)
        {
            return;
        }

        for (var i = 0; i < ItemCount; i++)
        {
            if (ContainerFromIndex(i) is CascaderViewItem cascaderViewItem)
            {
                ClearItemFilterMode(cascaderViewItem);
            }
        }

        IsFilterMode      = false;
        FilterResultCount = 0;
        SelectedItemsClosure.Clear();
    }
    
    private void ClearItemFilterMode(CascaderViewItem cascaderViewItem)
    {
        for (var i = 0; i < cascaderViewItem.ItemCount; i++)
        {
            if (cascaderViewItem.ContainerFromIndex(i) is CascaderViewItem childCascaderItem)
            {
                ClearItemFilterMode(childCascaderItem);
            }
        }
        cascaderViewItem.ClearFilterMode();
    }

    private ISet<CascaderViewItem> CollectExpandedItems(CascaderViewItem cascaderViewItem)
    {
        var expandedItems = new HashSet<CascaderViewItem>();
        for (var i = 0; i < cascaderViewItem.ItemCount; i++)
        {
            if (cascaderViewItem.ContainerFromIndex(i) is CascaderViewItem childCascaderItem)
            {
                expandedItems.UnionWith(CollectExpandedItems(childCascaderItem));
            }
        }

        if (cascaderViewItem.IsExpanded)
        {
            expandedItems.Add(cascaderViewItem);
        }
        return expandedItems;
    }

    private void RestoreItemExpandedStates(ISet<CascaderViewItem> originExpandedItems)
    {
        
        var originMotionEnabled = IsMotionEnabled;
        try
        {
            IsExpandAllProcess = true;
            SetCurrentValue(IsMotionEnabledProperty, false);

            for (int i = 0; i < ItemCount; i++)
            {
                if (ContainerFromIndex(i) is CascaderViewItem item)
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

    private void RestoreItemExpandedState(CascaderViewItem cascaderViewItem, in ISet<CascaderViewItem> expandedItems)
    {
        for (var i = 0; i < cascaderViewItem.ItemCount; i++)
        {
            if (cascaderViewItem.ContainerFromIndex(i) is CascaderViewItem childCascaderItem)
            {
                RestoreItemExpandedState(childCascaderItem, expandedItems);
            }
        }

        if (expandedItems.Contains(cascaderViewItem))
        {
            cascaderViewItem.SetCurrentValue(CascaderViewItem.IsExpandedProperty, true);
        }
        else
        {
            cascaderViewItem.SetCurrentValue(CascaderViewItem.IsExpandedProperty, false);
        }
    }

    private IDisposable BeginTurnOffMotion()
    {
        var originMotionEnabled = IsMotionEnabled;
        var disposable          = new MotionScopeDisposable(this, originMotionEnabled);
        SetCurrentValue(IsMotionEnabledProperty, false);
        return disposable;
    }

    private class MotionScopeDisposable : IDisposable
    {
        private readonly CascaderView _cascaderView;
        private bool _originMotionEnabled;
        public MotionScopeDisposable(CascaderView cascaderView, bool originMotionEnabled)
        {
            _cascaderView            = cascaderView;
            _originMotionEnabled = originMotionEnabled;
        }
        public void Dispose()
        {
            _cascaderView.SetCurrentValue(CascaderView.IsMotionEnabledProperty, _originMotionEnabled);
        }
    }
}