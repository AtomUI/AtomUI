using System.Diagnostics;
using AtomUI.Generated.AtomUIDesktopControls;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

public partial class CascaderView
{
    internal const int ROOT_LEVEL = 1;
    
    private static void SetupExpandAndCollapse()
    {
        CascaderViewItem.ExpandedEvent.AddClassHandler<CascaderView>((view, args) => view.HandleCascaderItemExpanded(args));
        CascaderViewItem.CollapsedEvent.AddClassHandler<CascaderView>((view, args) => view.HandleCascaderItemCollapsed(args));
        CascaderViewItem.ClearDescendantExpandedEvent.AddClassHandler<CascaderView>((view, args) => view.HandleClearCascaderDescendantExpanded(args));
    }

    private void HandleCascaderItemExpanded(RoutedEventArgs args)
    {
        if (args.Source is CascaderViewItem item)
        {
            if (_ignoreExpandAndCollapseLevel == 0)
            {
                Dispatcher.InvokeAsync(async () =>
                {
                    await ExpandItemAsync(item);
                });
            }
        }
    }
    
    private void HandleCascaderItemCollapsed(RoutedEventArgs args)
    {
        if (args.Source is CascaderViewItem item)
        {
            if (_ignoreExpandAndCollapseLevel == 0)
            {
                CollapseItem(item);
            }
        }
    }
    
    private async Task ExpandItemAsync(CascaderViewItem cascaderViewItem)
    {
        if (_itemsPanel == null || _ignoreExpandAndCollapseLevel > 0 || cascaderViewItem.AttachedOption == null)
        {
            return;
        }

        await ExpandItemAsync(cascaderViewItem.AttachedOption);
    }
    
    private async Task<CascaderViewItem?> ExpandItemAsync(ICascaderOption cascaderViewOption)
    {
        if (_itemsPanel == null || _ignoreExpandAndCollapseLevel > 0)
        {
            return null;
        }
        try
        {
            ++_ignoreExpandAndCollapseLevel;
            var pathNodes = BuildOptionPath(cascaderViewOption);
            
            Debug.Assert(pathNodes.Count > 0);
            // 检查是否是野数据
            var rootNode  = pathNodes[0];
            var foundRoot = false;
            foreach (var root in _options)
            {
                if (rootNode == root)
                {
                    foundRoot = true;
                    break;
                }
            }
            
            if (!foundRoot || cascaderViewOption != pathNodes[^1])
            {
                throw new ArgumentOutOfRangeException(nameof(cascaderViewOption), "Wild CascaderOption, Only part of the path was found");
            }
            
            CascaderViewLevelList? currentLevelList    = null;
            CascaderViewItem?      currentCascaderItem = null;
            for (var i = 0; i < pathNodes.Count; i++)
            {
                currentLevelList = GetLevelList(i);
                if (currentLevelList == null)
                {
                    return currentCascaderItem;
                }
                
                var currentNode = pathNodes[i];
                currentCascaderItem = TryGetRealizedCascaderViewItem(currentLevelList, currentNode);
                CollapseSiblingOptionsAtLevel(currentLevelList, currentNode);

                if (currentCascaderItem != null)
                {
                    if (!IsReallyExpanded(currentCascaderItem))
                    {
                        await DoExpandItemAsync(currentCascaderItem);
                        if (!currentCascaderItem.IsExpanded || !IsOptionInSource(currentNode))
                        {
                            return null;
                        }
                    }
                    else
                    {
                        currentNode.IsExpanded = true;
                    }
                }
                else
                {
                    ExpandOptionWithoutContainer(currentNode);
                }
            }
            if (!cascaderViewOption.HasChildren())
            {
                RemoveLevelListsFrom(pathNodes.Count);
            }
            if (currentCascaderItem != null)
            {
                ItemExpanded?.Invoke(this, new CascaderItemExpandedEventArgs(currentCascaderItem));
            }
            return currentCascaderItem;
        }
        finally
        {
            --_ignoreExpandAndCollapseLevel;
        }
    }

    private bool IsReallyExpanded(CascaderViewItem cascaderViewItem)
    {
        if (_itemsPanel == null)
        {
            return false;
        }
        var level = cascaderViewItem.Level;
        if (level < ROOT_LEVEL || level > _itemsPanel.Children.Count - 1)
        {
            return false;
        }

        var levelList = _itemsPanel.Children[level] as CascaderViewLevelList;
        if (levelList == null)
        {
            return false;
        }

        if (levelList.ParentCascaderViewItem == cascaderViewItem)
        {
            return true;
        }

        if (levelList.ParentCascaderViewItem == null &&
            cascaderViewItem.AttachedOption is { } attachedOption &&
            ReferenceEquals(levelList.ItemsSource, attachedOption.Children))
        {
            levelList.ParentCascaderViewItem = cascaderViewItem;
            return true;
        }

        return false;
    }
    
    // 这个方法默认判断 cascaderViewItem 的父亲都已经正常展开了，从而可以正常的展开自己
    private async Task DoExpandItemAsync(CascaderViewItem cascaderViewItem)
    {
        Debug.Assert(_itemsPanel != null);
        var attachedOption = cascaderViewItem.AttachedOption;
        Debug.Assert(attachedOption != null);
        // Mark the path open before awaiting data; another branch can collapse it while loading.
        cascaderViewItem.SetCurrentValue(CascaderViewItem.IsExpandedProperty, true);
        if (!attachedOption.HasChildren() && !cascaderViewItem.AsyncLoaded && !attachedOption.IsLeaf)
        {
            if (DataLoader != null)
            {
                try
                {
                    --_ignoreExpandAndCollapseLevel;
                    await Dispatcher.InvokeAsync(async () => { await LoadItemDataAsync(cascaderViewItem); });
                }
                finally
                {
                    ++_ignoreExpandAndCollapseLevel;
                }
                
                if (!cascaderViewItem.IsExpanded)
                {
                    return;
                }
            }
        }

        if (attachedOption.HasChildren())
        {
            EnsureChildLevelList(attachedOption, cascaderViewItem);
        }
        attachedOption.IsExpanded = true;
        cascaderViewItem.SetCurrentValue(CascaderViewItem.IsExpandedProperty, true);
    }

    private List<ICascaderOption> BuildOptionPath(ICascaderOption cascaderViewOption)
    {
        var pathCount = CountPathDepth(cascaderViewOption);
        var pathNodes = new List<ICascaderOption>(pathCount);
        for (var i = 0; i < pathCount; i++)
        {
            pathNodes.Add(null!);
        }

        var current = cascaderViewOption;
        for (var i = pathCount - 1; current != null; i--)
        {
            pathNodes[i] = current;
            current      = current.ParentNode as ICascaderOption;
        }

        return pathNodes;
    }

    private CascaderViewLevelList? GetLevelList(int index)
    {
        if (_itemsPanel == null || index < 0 || index >= _itemsPanel.Children.Count)
        {
            return null;
        }

        return _itemsPanel.Children[index] as CascaderViewLevelList;
    }

    private CascaderViewLevelList? GetLevelListForOption(ICascaderOption option)
    {
        return GetLevelList(GetViewOptionLevel(option) - 1);
    }

    private CascaderViewItem? TryGetRealizedCascaderViewItem(
        CascaderViewLevelList levelList,
        ICascaderOption option)
    {
        ExecuteLayoutPass();
        var cascaderViewItem = levelList.ContainerFromItem(option) as CascaderViewItem;
        if (cascaderViewItem != null)
        {
            return cascaderViewItem;
        }

        var indexOf = levelList.Items.IndexOf(option);
        if (indexOf >= 0 &&
            levelList.Presenter?.Panel is AtomUI.Controls.Primitives.VirtualizingStackPanel virtualizingStackPanel)
        {
            virtualizingStackPanel.ScrollItemIntoView(indexOf);
            ExecuteLayoutPass();
            cascaderViewItem = levelList.ContainerFromItem(option) as CascaderViewItem;
        }

        return cascaderViewItem;
    }

    private void ExecuteLayoutPass()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        topLevel?.UpdateLayout();
    }

    private void CollapseSiblingOptionsAtLevel(CascaderViewLevelList levelList, ICascaderOption currentNode)
    {
        for (var i = 0; i < levelList.ItemCount; i++)
        {
            if (levelList.Items[i] is not ICascaderOption option ||
                ReferenceEquals(option, currentNode))
            {
                continue;
            }

            option.IsExpanded = false;
            if (levelList.ContainerFromItem(option) is CascaderViewItem item)
            {
                item.SetCurrentValue(CascaderViewItem.IsExpandedProperty, false);
            }
        }
    }

    private void ExpandOptionWithoutContainer(ICascaderOption option)
    {
        option.IsExpanded = true;
        if (option.HasChildren())
        {
            EnsureChildLevelList(option, null);
        }
    }

    private void EnsureChildLevelList(ICascaderOption option, CascaderViewItem? parentItem)
    {
        if (_itemsPanel == null || !option.HasChildren())
        {
            return;
        }

        var childLevelIndex = GetViewOptionLevel(option);
        if (_itemsPanel.Children.Count > childLevelIndex &&
            _itemsPanel.Children[childLevelIndex] is CascaderViewLevelList existingLevelList &&
            ReferenceEquals(existingLevelList.ItemsSource, option.Children))
        {
            existingLevelList.ParentCascaderViewItem = parentItem;
            return;
        }

        RemoveLevelListsFrom(childLevelIndex);
        var childLevelList = new CascaderViewLevelList
        {
            OwnerView              = this,
            ItemsSource            = option.Children,
            ParentCascaderViewItem = parentItem
        };
        childLevelList[!CascaderViewLevelList.ItemTemplateProperty]        = this[!OptionTemplateProperty];
        childLevelList[!CascaderViewLevelList.IsAllowSelectParentProperty] = this[!IsAllowSelectParentProperty];
        childLevelList[!CascaderViewLevelList.ExpandTriggerProperty]       = this[!ExpandTriggerProperty];
        childLevelList.Classes.Add(CascaderSemanticParts.PopupListClass);

        _itemsPanel.Children.Add(childLevelList);
    }

    private void RemoveLevelListsFrom(int startIndex)
    {
        if (_itemsPanel == null)
        {
            return;
        }

        ClearExpandedStateForLevel(startIndex + 1);
        var count = _itemsPanel.Children.Count;
        while (count > startIndex)
        {
            --count;
            if (_itemsPanel.Children[count] is CascaderViewLevelList levelList)
            {
                levelList.ItemsSource = null;
                levelList.Items.Clear();
            }

            _itemsPanel.Children.RemoveAt(count);
        }
    }
    
    private void CollapseItem(CascaderViewItem cascaderViewItem)
    {
        Debug.Assert(cascaderViewItem.Level >= ROOT_LEVEL);
        try
        {
            ++_ignoreExpandAndCollapseLevel;
            if (cascaderViewItem.AttachedOption == null || cascaderViewItem.IsLeaf)
            {
                return;
            }
            DoCollapseItem(cascaderViewItem.AttachedOption);
        }
        finally
        {
            --_ignoreExpandAndCollapseLevel;
        }
    }

    private void DoCollapseItem(ICascaderOption cascaderViewOption)
    {
        if (_itemsPanel == null)
        {
            return;
        }
        var level = GetViewOptionLevel(cascaderViewOption);

        if (!cascaderViewOption.IsLeaf)
        {
            ClearExpandedStateForLevel(level + 1);
        
            var targetIndex = level;
            var count       = _itemsPanel.Children.Count;
            while (count > targetIndex)
            {
                --count;
                if (_itemsPanel.Children[count] is CascaderViewLevelList levelList)
                {
                    levelList.ItemsSource = null;
                }
        
                _itemsPanel.Children.RemoveAt(count);
            }
        }

        cascaderViewOption.IsExpanded = false;
        var selfLevelList = _itemsPanel.Children[level - 1] as CascaderViewLevelList;
        Debug.Assert(selfLevelList != null);
        var cascaderViewItem = selfLevelList.ContainerFromItem(cascaderViewOption) as CascaderViewItem;
        Debug.Assert(cascaderViewItem != null);
        cascaderViewItem.SetCurrentValue(CascaderViewItem.IsExpandedProperty, false);
        ItemCollapsed?.Invoke(this, new CascaderItemCollapsedEventArgs(cascaderViewItem));
    }

    private int GetViewOptionLevel(ICascaderOption cascaderViewOption)
    {
        var level   = 0;
        var current = cascaderViewOption;
        while (current != null)
        {
            level++;
            current = current.ParentNode as ICascaderOption;
        }
        return level;
    }
    
    public void CollapseAll()
    {
        if (_itemsPanel == null || _ignoreExpandAndCollapseLevel > 0)
        {
            return;
        }
    
        try
        {
            ++_ignoreExpandAndCollapseLevel;
            ClearExpandedStateForLevel();
            var count       = _itemsPanel.Children.Count;
            while (count > 1)
            {
                --count;
                if (_itemsPanel.Children[count] is CascaderViewLevelList levelList)
                {
                    levelList.ItemsSource = null;
                    levelList.Items.Clear();
                }
    
                _itemsPanel.Children.RemoveAt(count);
            }
            InvalidateMeasure();
        }
        finally
        {
            --_ignoreExpandAndCollapseLevel;
        }
    }
    
    private void ClearExpandedStateForLevel(int level = 1)
    {
        if (_itemsPanel == null)
        {
            return;
        }
        var stop = Math.Max(0, level - 1);
        for (var i = _itemsPanel.Children.Count - 1; i >= stop; i--)
        {
            if (_itemsPanel.Children[i] is CascaderViewLevelList levelList)
            {
                foreach (var item in levelList.Items)
                {
                    if (item is ICascaderOption option)
                    {
                        option.IsExpanded = false;
                        if (levelList.ContainerFromItem(item) is CascaderViewItem cascaderViewItem)
                        {
                            cascaderViewItem.SetCurrentValue(CascaderViewItem.IsExpandedProperty, false);
                        }
                    }
                }
            }
        }
    }
    
    private void HandleClearCascaderDescendantExpanded(RoutedEventArgs args)
    {
        if (args.Source is CascaderViewItem item)
        {
            var option = item.AttachedOption;
            if (_itemsPanel == null || _ignoreExpandAndCollapseLevel > 0 || !item.IsExpanded || option == null)
            {
                return;
            }
    
            try
            {
                ++_ignoreExpandAndCollapseLevel;
                var level = GetViewOptionLevel(option);
                ClearExpandedStateForLevel(level + 1);
                item.IsSelected = false;
                var count = _itemsPanel.Children.Count;
                while (count > level + 1)
                {
                    --count;
                    if (_itemsPanel.Children[count] is CascaderViewLevelList levelList)
                    {
                        levelList.ItemsSource = null;
                        levelList.Items.Clear();
                    }
                
                    _itemsPanel.Children.RemoveAt(count);
                }

                args.Handled = true;
                InvalidateMeasure();
            }
            finally
            {
                --_ignoreExpandAndCollapseLevel;
            }
        }
    }

}
