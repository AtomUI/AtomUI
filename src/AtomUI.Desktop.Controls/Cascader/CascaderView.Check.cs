using System.Collections.Specialized;
using Avalonia;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

public partial class CascaderView
{
    private static void SetupChecked()
    {
        CheckedItemsProperty.Changed.AddClassHandler<CascaderView>((view, e) => view.HandleCheckedItemsChanged(e));
        CascaderViewItem.CheckedEvent.AddClassHandler<CascaderView>((view, e) => view.HandleCascaderItemCheckedChanged(e));
    }

    private void HandleCascaderItemCheckedChanged(RoutedEventArgs e)
    {
        if (e.Source is CascaderViewItem cascaderViewItem)
        {
            if (cascaderViewItem.IsChecked == true)
            {
                CheckedSubTree(cascaderViewItem);
            }
            else if (cascaderViewItem.IsChecked == false)
            {
                UnCheckedSubTree(cascaderViewItem);
            }
        }
    }

    private void HandleCheckedItemsChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (_ignoreSyncCheckedItems)
        {
            _ignoreSyncCheckedItems = false;
            return;
        }

        var oldCheckedItems = args.OldValue as IList<ICascaderViewOption>;
        var newCheckedItems = args.NewValue as IList<ICascaderViewOption>;

        if (oldCheckedItems != null)
        {
            foreach (var oldItem in oldCheckedItems)
            {
                UnCheckedSubTree(oldItem);
            }
        }

        if (newCheckedItems != null)
        {
            foreach (var newItem in newCheckedItems)
            {
                CheckedSubTree(newItem);
            }
        }
        
        CheckedItemsChanged?.Invoke(this, new CascaderViewCheckedItemsChangedEventArgs(
            oldCheckedItems,
            newCheckedItems));
    }
    
    public void CheckedSubTree(CascaderViewItem item)
    {
        if (!item.IsEffectiveCheckable() || item.AttachedOption == null)
        {
            return;
        }

        CheckedSubTree(item.AttachedOption);
    }
    
    public void CheckedSubTree(ICascaderViewOption viewOption)
    {
        var container = GetCascaderViewItem(viewOption);
        if (!container?.IsEffectiveCheckable() ?? viewOption.IsEnabled && viewOption.IsCheckBoxEnabled == false)
        {
            return;
        }

        ISet<ICascaderViewOption> checkedItems = DoCheckedSubTree(viewOption);
        var originCheckedItems = new HashSet<ICascaderViewOption>();
        if (CheckedItems != null)
        {
            foreach (var checkedItem in CheckedItems)
            {
                originCheckedItems.Add(checkedItem);
            }
        }
        var newCheckedItems = new HashSet<ICascaderViewOption>();
        foreach (var checkedItem in originCheckedItems)
        {
            newCheckedItems.Add(checkedItem);
        }
        foreach (var checkedItem in checkedItems)
        {
            newCheckedItems.Add(checkedItem);
        }

        if (!newCheckedItems.SetEquals(originCheckedItems))
        {
            _ignoreSyncCheckedItems = true;
            CheckedItems            = newCheckedItems.ToList();
        }
    }
    
    private ISet<ICascaderViewOption> DoCheckedSubTree(ICascaderViewOption viewOption)
    {
        var checkedItems = new HashSet<ICascaderViewOption>();
        MarkViewOptionChecked(viewOption, true);
        checkedItems.Add(viewOption);
        foreach (var childItem in viewOption.Children)
        {
            var container = GetCascaderViewItem(childItem);
            if ((container != null && container.IsEffectiveCheckable()) || 
                (childItem.IsEnabled && childItem.IsCheckBoxEnabled))
            {
                var childCheckedItems = DoCheckedSubTree(childItem);
                checkedItems.UnionWith(childCheckedItems);
            }
        }
        
        var (checkedParentItems, _) = SetupParentNodeCheckedStatus(viewOption);
        checkedItems.UnionWith(checkedParentItems);
        return checkedItems;
    }

    private void MarkViewOptionChecked(ICascaderViewOption option, bool? value)
    {
        option.IsChecked = value;
        var container = GetCascaderViewItem(option);
        if (container != null)
        {
            container.SetCurrentValue(CascaderViewItem.IsCheckedProperty, value);
        }
    }

    private CascaderViewItem? GetCascaderViewItem(ICascaderViewOption option)
    {
        if (_itemsPanel == null)
        {
            return null;
        }
        var level          = GetViewOptionLevel(option);
        var levelListIndex = level - 1;
        if (levelListIndex < 0 || levelListIndex > _itemsPanel.Children.Count - 1)
        {
            return null;
        }
        var selfLevelList  = _itemsPanel.Children[levelListIndex] as CascaderViewLevelList;
        return selfLevelList?.ContainerFromItem(option) as CascaderViewItem;
    }

    public void UnCheckedSubTree(CascaderViewItem item)
    {
        if (!item.IsEffectiveCheckable() || item.AttachedOption == null)
        {
            return;
        }
        UnCheckedSubTree(item.AttachedOption);
    }
    
    public void UnCheckedSubTree(ICascaderViewOption viewOption)
    {
        var container = GetCascaderViewItem(viewOption);
        if (!container?.IsEffectiveCheckable() ?? viewOption.IsEnabled && viewOption.IsCheckBoxEnabled == false)
        {
            return;
        }
        
        ISet<ICascaderViewOption> unCheckedItems = DoUnCheckedSubTree(viewOption);
            
        var originCheckedItems = new HashSet<ICascaderViewOption>();
        if (CheckedItems != null)
        {
            foreach (var checkedItem in CheckedItems)
            {
                originCheckedItems.Add(checkedItem);
            }
        }
        var newCheckedItems = new HashSet<ICascaderViewOption>();
        foreach (var checkedItem in originCheckedItems)
        {
            newCheckedItems.Add(checkedItem);
        }
        
        foreach (var unCheckedItem in unCheckedItems)
        {
            newCheckedItems.Remove(unCheckedItem);
        }

        if (!newCheckedItems.SetEquals(originCheckedItems))
        {
            _ignoreSyncCheckedItems = true;
            CheckedItems            = newCheckedItems.ToList();
        }
    }
    
    public ISet<ICascaderViewOption> DoUnCheckedSubTree(ICascaderViewOption viewOption)
    {
        var unCheckedItems = new HashSet<ICascaderViewOption>();
        
        MarkViewOptionChecked(viewOption, false);
        unCheckedItems.Add(viewOption);
    
        foreach (var childItem in viewOption.Children)
        {
            var container = GetCascaderViewItem(childItem);
            if ((container != null && container.IsEffectiveCheckable()) || 
                (childItem.IsEnabled && childItem.IsCheckBoxEnabled))
            {
                var childCheckedItems = DoUnCheckedSubTree(childItem);
                unCheckedItems.UnionWith(childCheckedItems);
            }
        }
        var (_, unCheckedParentItems) = SetupParentNodeCheckedStatus(viewOption);
        unCheckedItems.UnionWith(unCheckedParentItems);
        return unCheckedItems;
    }
    
    private (ISet<ICascaderViewOption>, ISet<ICascaderViewOption>) SetupParentNodeCheckedStatus(ICascaderViewOption viewOption)
    {
        var parent           = viewOption.ParentNode;
        var checkedParents   =  new HashSet<ICascaderViewOption>();
        var unCheckedParents =  new HashSet<ICascaderViewOption>();
        while (parent is ICascaderViewOption parentViewOption)
        {
            var parentContainer = GetCascaderViewItem(parentViewOption);
            if ((parentContainer?.IsEnabled ?? parentViewOption.IsEnabled) == false)
            {
                break;
            }
            var isAllChecked    = false;
            var isAnyChecked    = false;
    
            if (parentViewOption.Children.Count > 0)
            {
                isAllChecked = parentViewOption.Children.All(childItem =>
                {
                    var childContainer = GetCascaderViewItem(childItem);
                    if (childContainer != null)
                    {
                        return !childContainer.IsEnabled || !childContainer.IsCheckBoxEnabled || childContainer.IsChecked.HasValue && childContainer.IsChecked.Value;
                    }
                    return !childItem.IsEnabled || !childItem.IsCheckBoxEnabled || childItem.IsChecked.HasValue && childItem.IsChecked.Value;
                });
    
                isAnyChecked = parentViewOption.Children.Any(childItem =>
                {
                    var childContainer = GetCascaderViewItem(childItem);
                    if (childContainer != null)
                    {
                        return childContainer.IsEnabled && childContainer.IsCheckBoxEnabled &&
                               (!childContainer.IsChecked.HasValue || childContainer.IsChecked.HasValue && childContainer.IsChecked.Value);
                    }
                    return childItem.IsEnabled && childItem.IsCheckBoxEnabled && (!childItem.IsChecked.HasValue || childItem.IsChecked.HasValue && childItem.IsChecked.Value);
                });
            }
    
            if (parentContainer?.IsChecked ?? parentViewOption.IsChecked == true && !isAllChecked)
            {
                unCheckedParents.Add(parentViewOption);
            }
            
            if (isAllChecked)
            {
                parentViewOption.IsChecked = true;
                if (parentContainer != null)
                {
                    parentContainer.SetCurrentValue(CascaderViewItem.IsCheckedProperty, true);
                }
            }
            else if (isAnyChecked)
            {
                parentViewOption.IsChecked = null;
                if (parentContainer != null)
                {
                    parentContainer.SetCurrentValue(CascaderViewItem.IsCheckedProperty, null);
                }
            }
            else
            {
                parentViewOption.IsChecked = false;
                if (parentContainer != null)
                {
                    parentContainer.SetCurrentValue(CascaderViewItem.IsCheckedProperty, false);
                }
            }
       
            if (parentViewOption.IsChecked == true)
            {
                checkedParents.Add(parentViewOption);
            }
            parent = parent.ParentNode;
        }
    
        return (checkedParents, unCheckedParents);
    }
    
    private void HandleCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ConfigureEmptyIndicator();
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Remove:
            case NotifyCollectionChangedAction.Replace:
                if (CheckedItems != null)
                {
                    if (e.OldItems != null)
                    {
                        var removedItemsClosure = new HashSet<ICascaderViewOption>();
                        foreach (var item in e.OldItems)
                        {
                            if (item is ICascaderViewOption viewOption)
                            {
                                CollectSubTreeOptions(viewOption, removedItemsClosure);
                            }
                        }
                        var tobeRemoved = new List<ICascaderViewOption>();
                        foreach (var checkedItem in CheckedItems)
                        {
                            if (removedItemsClosure.Contains(checkedItem))
                            {
                                tobeRemoved.Add(checkedItem);
                            }
                        }

                        foreach (var removedItem in tobeRemoved)
                        {
                            CheckedItems.Remove(removedItem);
                        }
                    }
                }
                break;
            case NotifyCollectionChangedAction.Reset:
                CheckedItems = null;
                break;
        }
        FilterItems();
    }

    private void CollectSubTreeOptions(ICascaderViewOption viewOption, ISet<ICascaderViewOption> options)
    {
        options.Add(viewOption);
        foreach (var childItem in viewOption.Children)
        {
            CollectSubTreeOptions(childItem, options);
        }
    }
}