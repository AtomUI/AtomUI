using System.Diagnostics;
using AtomUI.Controls;
using AtomUI.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public partial class CascaderView
{
    internal const int ROOT_LEVEL = 1;
    
    private static void SetupExpandAndCollapse()
    {
        CascaderViewItem.ExpandedEvent.AddClassHandler<CascaderView>((view, args) => view.HandleCascaderItemExpanded(args));
        CascaderViewItem.CollapsedEvent.AddClassHandler<CascaderView>((view, args) => view.HandleCascaderItemCollapsed(args));
    }

    private void HandleCascaderItemExpanded(RoutedEventArgs args)
    {
        if (args.Source is CascaderViewItem item)
        {
            if (_ignoreExpandAndCollapseLevel == 0)
            {
                Dispatcher.UIThread.InvokeAsync(async () =>
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
    
    private async Task<CascaderViewItem?> ExpandItemAsync(ICascaderViewOption cascaderViewOption)
    {
        if (_itemsPanel == null || _ignoreExpandAndCollapseLevel > 0)
        {
            return null;
        }
        try
        {
            ++_ignoreExpandAndCollapseLevel;
            var pathNodes  = new List<object>();
            
            var current  = cascaderViewOption;
            while (current != null)
            {
                pathNodes.Add(current);
                current = current.ParentNode as ICascaderViewOption;
            }
            pathNodes.Reverse();
            
            Debug.Assert(pathNodes.Count > 0);
            // 检查是否是野数据
            var rootNode  = pathNodes.First();
            var foundRoot = false;
            foreach (var root in Items)
            {
                if (rootNode == root)
                {
                    foundRoot = true;
                    break;
                }
            }
            
            if (!foundRoot || cascaderViewOption != pathNodes[^1])
            {
                throw new ArgumentOutOfRangeException(nameof(cascaderViewOption), "Wild CascaderViewOption, Only part of the path was found");
            }
            
            CascaderViewLevelList? currentLevelList    = null;
            CascaderViewItem?      currentCascaderItem = null;
            for (var i = 0; i < pathNodes.Count; i++)
            {
                currentLevelList = _itemsPanel.Children[i] as CascaderViewLevelList;
                Debug.Assert(currentLevelList != null);
                if (currentLevelList.Presenter?.Panel == null && this.GetVisualRoot() is ILayoutRoot visualRoot)
                {
                    var layoutManager = visualRoot.GetLayoutManager();
                    layoutManager.ExecuteLayoutPass();
                }
                var currentNode         = pathNodes[i];
                currentCascaderItem = currentLevelList.ContainerFromItem(currentNode) as CascaderViewItem;
                Debug.Assert(currentCascaderItem != null);
                for (var j = 0; j < currentLevelList.ItemCount; j++)
                {
                    if (currentLevelList.ContainerFromIndex(j) is CascaderViewItem levelItem)
                    {
                        if (currentNode != levelItem.AttachedOption && levelItem.IsExpanded && levelItem.AttachedOption != null)
                        {
                            DoCollapseItem(levelItem.AttachedOption);
                        }
                    }
                }
            
                if (!IsReallyExpanded(currentCascaderItem))
                {
                    await DoExpandItemAsync(currentCascaderItem);
                }
            }
            Debug.Assert(currentCascaderItem != null);
            ItemExpanded?.Invoke(this, new CascaderItemExpandedEventArgs(currentCascaderItem));
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

        return levelList.ParentCascaderViewItem == cascaderViewItem;
    }
    
    // 这个方法默认判断 cascaderViewItem 的父亲都已经正常展开了，从而可以正常的展开自己
    private async Task DoExpandItemAsync(CascaderViewItem cascaderViewItem)
    {
        Debug.Assert(_itemsPanel != null);
        var attachedOption = cascaderViewItem.AttachedOption;
        Debug.Assert(attachedOption != null);
        if (attachedOption.Children.Count == 0 && !cascaderViewItem.AsyncLoaded)
        {
            if (DataLoader != null)
            {
                try
                {
                    --_ignoreExpandAndCollapseLevel;
                    await Dispatcher.UIThread.InvokeAsync(async () => { await LoadItemDataAsync(cascaderViewItem); });
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

        if (attachedOption.Children.Count != 0)
        {
            var childLevelList = new CascaderViewLevelList()
            {
                OwnerView              = this,
                ItemsSource            = attachedOption.Children,
                ParentCascaderViewItem = cascaderViewItem
            };
            BindUtils.RelayBind(this, ItemTemplateProperty, childLevelList, CascaderViewLevelList.ItemTemplateProperty);
            _itemsPanel.Children.Add(childLevelList);
        }
        cascaderViewItem.SetCurrentValue(CascaderViewItem.IsExpandedProperty, true);
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

    private void DoCollapseItem(ICascaderViewOption cascaderViewOption)
    {
        if (_itemsPanel == null)
        {
            return;
        }
        var level = GetViewOptionLevel(cascaderViewOption);

        if (!cascaderViewOption.IsLeaf)
        {
            ClearExpandedState(level + 1);
        
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

        var selfLevelList = _itemsPanel.Children[level - 1] as CascaderViewLevelList;
        Debug.Assert(selfLevelList != null);
        var cascaderViewItem = selfLevelList.ContainerFromItem(cascaderViewOption) as CascaderViewItem;
        Debug.Assert(cascaderViewItem != null);
        cascaderViewItem.SetCurrentValue(CascaderViewItem.IsExpandedProperty, false);
        ItemCollapsed?.Invoke(this, new CascaderItemCollapsedEventArgs(cascaderViewItem));
    }

    private int GetViewOptionLevel(ICascaderViewOption cascaderViewOption)
    {
        var level   = 0;
        var current = cascaderViewOption;
        while (current != null)
        {
            level++;
            current = current.ParentNode as ICascaderViewOption;
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
            ClearExpandedState();
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
    
    private void ClearExpandedState(int level = 0)
    {
        if (_itemsPanel == null)
        {
            return;
        }
        for (var i = _itemsPanel.Children.Count - 1; i >= level; i--)
        {
            if (_itemsPanel.Children[i] is CascaderViewLevelList levelList)
            {
                foreach (var item in levelList.Items)
                {
                    if (item != null)
                    {
                        if (levelList.ContainerFromItem(item) is CascaderViewItem cascaderViewItem)
                        {
                            cascaderViewItem.IsExpanded = false;
                        }
                    }
                }
            }
        }
    }
}