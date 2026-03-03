using System.Collections.Specialized;
using Avalonia;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

public partial class CascaderView
{
    private static void SetupChecked()
    {
        SelectedOptionsProperty.Changed.AddClassHandler<CascaderView>((view, e) => view.HandleSelectedOptionsChanged(e));
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

    private void HandleSelectedOptionsChanged(AvaloniaPropertyChangedEventArgs args)
    {
        var oldSelectedOptions = args.OldValue as IList<ICascaderOption>;
        var newSelectedOptions = args.NewValue as IList<ICascaderOption>;
        SelectedOptionsChanged?.Invoke(this, new CascaderOptionsSelectedChangedEventArgs(
            oldSelectedOptions,
            newSelectedOptions));
        
        if (_ignoreSyncSelectedOptions)
        {
            _ignoreSyncSelectedOptions = false;
            return;
        }

        if (oldSelectedOptions != null)
        {
            foreach (var oldItem in oldSelectedOptions)
            {
                UnCheckedSubTree(oldItem);
            }
        }

        if (newSelectedOptions != null)
        {
            foreach (var newItem in newSelectedOptions)
            {
                CheckedSubTree(newItem);
            }
        }
    }
    
    public void CheckedSubTree(CascaderViewItem item)
    {
        if (!item.IsEffectiveCheckable() || item.AttachedOption == null)
        {
            return;
        }

        CheckedSubTree(item.AttachedOption);
    }
    
    public void CheckedSubTree(ICascaderOption viewOption)
    {
        var container = GetCascaderViewItem(viewOption);
        if (!container?.IsEffectiveCheckable() ?? viewOption.IsEnabled && viewOption.IsCheckBoxEnabled == false)
        {
            return;
        }

        ISet<ICascaderOption> checkedOptions = DoCheckedSubTree(viewOption);
        var originSelectedOptions = new HashSet<ICascaderOption>();
        if (SelectedOptions != null)
        {
            foreach (var checkedItem in SelectedOptions)
            {
                originSelectedOptions.Add(checkedItem);
            }
        }
        var newSelectedOptions = new HashSet<ICascaderOption>();
        foreach (var checkedItem in originSelectedOptions)
        {
            newSelectedOptions.Add(checkedItem);
        }
        foreach (var checkedItem in checkedOptions)
        {
            newSelectedOptions.Add(checkedItem);
        }

        if (!newSelectedOptions.SetEquals(originSelectedOptions))
        {
            _ignoreSyncSelectedOptions = true;
            SelectedOptions            = newSelectedOptions.ToList();
        }
    }
    
    private ISet<ICascaderOption> DoCheckedSubTree(ICascaderOption viewOption)
    {
        var checkedOptions = new HashSet<ICascaderOption>();
        MarkViewOptionChecked(viewOption, true);
        checkedOptions.Add(viewOption);
        foreach (var childItem in viewOption.Children)
        {
            var container = GetCascaderViewItem(childItem);
            if ((container != null && container.IsEffectiveCheckable()) || 
                (childItem.IsEnabled && childItem.IsCheckBoxEnabled))
            {
                var childSelectedOptions = DoCheckedSubTree(childItem);
                checkedOptions.UnionWith(childSelectedOptions);
            }
        }
        
        var (checkedParentItems, _) = SetupParentNodeCheckedStatus(viewOption);
        checkedOptions.UnionWith(checkedParentItems);
        return checkedOptions;
    }

    private void MarkViewOptionChecked(ICascaderOption option, bool? value)
    {
        option.IsChecked = value;
        var container = GetCascaderViewItem(option);
        if (container != null)
        {
            container.SetCurrentValue(CascaderViewItem.IsCheckedProperty, value);
        }
    }

    private CascaderViewItem? GetCascaderViewItem(ICascaderOption option)
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
    
    public void UnCheckedSubTree(ICascaderOption viewOption)
    {
        var container = GetCascaderViewItem(viewOption);
        if (!container?.IsEffectiveCheckable() ?? viewOption.IsEnabled && viewOption.IsCheckBoxEnabled == false)
        {
            return;
        }
        
        ISet<ICascaderOption> unSelectedOptions = DoUnCheckedSubTree(viewOption);
            
        var originSelectedOptions = new HashSet<ICascaderOption>();
        if (SelectedOptions != null)
        {
            foreach (var checkedItem in SelectedOptions)
            {
                originSelectedOptions.Add(checkedItem);
            }
        }
        var newSelectedOptions = new HashSet<ICascaderOption>();
        foreach (var checkedItem in originSelectedOptions)
        {
            newSelectedOptions.Add(checkedItem);
        }
        
        foreach (var unCheckedItem in unSelectedOptions)
        {
            newSelectedOptions.Remove(unCheckedItem);
        }

        if (!newSelectedOptions.SetEquals(originSelectedOptions))
        {
            _ignoreSyncSelectedOptions = true;
            SelectedOptions            = newSelectedOptions.ToList();
        }
    }
    
    public ISet<ICascaderOption> DoUnCheckedSubTree(ICascaderOption viewOption)
    {
        var unSelectedOptions = new HashSet<ICascaderOption>();
        
        MarkViewOptionChecked(viewOption, false);
        unSelectedOptions.Add(viewOption);
    
        foreach (var childItem in viewOption.Children)
        {
            var container = GetCascaderViewItem(childItem);
            if ((container != null && container.IsEffectiveCheckable()) || 
                (childItem.IsEnabled && childItem.IsCheckBoxEnabled))
            {
                var childSelectedOptions = DoUnCheckedSubTree(childItem);
                unSelectedOptions.UnionWith(childSelectedOptions);
            }
        }
        var (_, unCheckedParentItems) = SetupParentNodeCheckedStatus(viewOption);
        unSelectedOptions.UnionWith(unCheckedParentItems);
        return unSelectedOptions;
    }
    
    private (ISet<ICascaderOption>, ISet<ICascaderOption>) SetupParentNodeCheckedStatus(ICascaderOption viewOption)
    {
        var parent           = viewOption.ParentNode;
        var checkedParents   =  new HashSet<ICascaderOption>();
        var unCheckedParents =  new HashSet<ICascaderOption>();
        while (parent is ICascaderOption parentViewOption)
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
                if (SelectedOptions != null)
                {
                    if (e.OldItems != null)
                    {
                        var removedItemsClosure = new HashSet<ICascaderOption>();
                        foreach (var item in e.OldItems)
                        {
                            if (item is ICascaderOption viewOption)
                            {
                                CollectSubTreeOptions(viewOption, removedItemsClosure);
                            }
                        }
                        var tobeRemoved = new List<ICascaderOption>();
                        foreach (var checkedItem in SelectedOptions)
                        {
                            if (removedItemsClosure.Contains(checkedItem))
                            {
                                tobeRemoved.Add(checkedItem);
                            }
                        }

                        foreach (var removedItem in tobeRemoved)
                        {
                            SelectedOptions.Remove(removedItem);
                        }
                    }
                }
                break;
            case NotifyCollectionChangedAction.Reset:
                SelectedOptions = null;
                break;
        }
        FilterItems();
    }

    private void CollectSubTreeOptions(ICascaderOption viewOption, ISet<ICascaderOption> options)
    {
        options.Add(viewOption);
        foreach (var childItem in viewOption.Children)
        {
            CollectSubTreeOptions(childItem, options);
        }
    }
}