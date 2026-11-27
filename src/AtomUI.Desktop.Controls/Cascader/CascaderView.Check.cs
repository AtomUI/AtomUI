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

        var oldSelectedOptionSet = BuildSelectedOptionSet(oldSelectedOptions);
        var newSelectedOptionSet = BuildSelectedOptionSet(newSelectedOptions);

        if (oldSelectedOptions != null)
        {
            foreach (var oldItem in oldSelectedOptions)
            {
                if (!newSelectedOptionSet.Contains(oldItem))
                {
                    DoUnCheckedSubTree(oldItem);
                }
            }
        }

        if (newSelectedOptions != null)
        {
            foreach (var newItem in newSelectedOptions)
            {
                if (!oldSelectedOptionSet.Contains(newItem))
                {
                    DoCheckedSubTree(newItem);
                }
            }
        }
    }
    
    /// <summary>
    /// Checks the option attached to <paramref name="item" /> with its subtree and adds the checked options to
    /// <see cref="SelectedOptions" />. Does nothing when the item is not checkable or has no attached option.
    /// </summary>
    public void CheckedSubTree(CascaderViewItem item)
    {
        if (!item.IsEffectiveCheckable() || item.AttachedOption == null)
        {
            return;
        }

        CheckedSubTree(item.AttachedOption);
    }
    
    /// <summary>
    /// Checks the option, its checkable descendants and every ancestor whose children are then all checked, and adds
    /// those options to <see cref="SelectedOptions" />.
    /// </summary>
    /// <remarks>
    /// Does nothing when the option is not checkable. The check state is written to the option data and to the item
    /// containers that are realized. <see cref="SelectedOptions" /> is replaced by a new list only when an option
    /// that was not selected before became checked; that change raises <see cref="SelectedOptionsChanged" /> without
    /// syncing the item states again.
    /// </remarks>
    public void CheckedSubTree(ICascaderOption viewOption)
    {
        var container = GetCascaderViewItem(viewOption);
        if (!container?.IsEffectiveCheckable() ?? viewOption.IsEnabled && viewOption.IsCheckBoxEnabled == false)
        {
            return;
        }

        ISet<ICascaderOption> checkedOptions = DoCheckedSubTree(viewOption);
        var originSelectedOptions = new HashSet<ICascaderOption>(SelectedOptions?.Count ?? 0);
        if (SelectedOptions != null)
        {
            foreach (var checkedItem in SelectedOptions)
            {
                originSelectedOptions.Add(checkedItem);
            }
        }
        var newSelectedOptions = new HashSet<ICascaderOption>(originSelectedOptions.Count + checkedOptions.Count);
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
            SelectedOptions            = BuildSelectedOptionsList(newSelectedOptions);
        }
    }
    
    private ISet<ICascaderOption> DoCheckedSubTree(ICascaderOption viewOption)
    {
        var checkedOptions = new HashSet<ICascaderOption>();
        DoCheckedSubTreeCore(viewOption, checkedOptions);
        return checkedOptions;
    }

    private void DoCheckedSubTreeCore(ICascaderOption viewOption, HashSet<ICascaderOption> checkedOptions)
    {
        MarkViewOptionChecked(viewOption, true);
        checkedOptions.Add(viewOption);
        foreach (var childItem in viewOption.Children)
        {
            var container = GetCascaderViewItem(childItem);
            if ((container != null && container.IsEffectiveCheckable()) || 
                (childItem.IsEnabled && childItem.IsCheckBoxEnabled))
            {
                DoCheckedSubTreeCore(childItem, checkedOptions);
            }
        }
        
        var (checkedParentItems, _) = SetupParentNodeCheckedStatus(viewOption);
        checkedOptions.UnionWith(checkedParentItems);
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

    /// <summary>
    /// Unchecks the option attached to <paramref name="item" /> with its subtree and removes the unchecked options
    /// from <see cref="SelectedOptions" />. Does nothing when the item is not checkable or has no attached option.
    /// </summary>
    public void UnCheckedSubTree(CascaderViewItem item)
    {
        if (!item.IsEffectiveCheckable() || item.AttachedOption == null)
        {
            return;
        }
        UnCheckedSubTree(item.AttachedOption);
    }
    
    /// <summary>
    /// Unchecks the option and its checkable descendants, updates the check state of their ancestors, and removes the
    /// options that are no longer checked from <see cref="SelectedOptions" />.
    /// </summary>
    /// <remarks>
    /// Does nothing when the option is not checkable. The check state is written to the option data and to the item
    /// containers that are realized. <see cref="SelectedOptions" /> is replaced by a new list only when a selected
    /// option became unchecked; that change raises <see cref="SelectedOptionsChanged" /> without syncing the item
    /// states again.
    /// </remarks>
    public void UnCheckedSubTree(ICascaderOption viewOption)
    {
        var container = GetCascaderViewItem(viewOption);
        if (!container?.IsEffectiveCheckable() ?? viewOption.IsEnabled && viewOption.IsCheckBoxEnabled == false)
        {
            return;
        }
        
        ISet<ICascaderOption> unSelectedOptions = DoUnCheckedSubTree(viewOption);
            
        var originSelectedOptions = new HashSet<ICascaderOption>(SelectedOptions?.Count ?? 0);
        if (SelectedOptions != null)
        {
            foreach (var checkedItem in SelectedOptions)
            {
                originSelectedOptions.Add(checkedItem);
            }
        }
        var newSelectedOptions = new HashSet<ICascaderOption>(originSelectedOptions.Count);
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
            SelectedOptions            = BuildSelectedOptionsList(newSelectedOptions);
        }
    }
    
    /// <summary>
    /// Unchecks the option and its checkable descendants, updates the check state of their ancestors on the option
    /// data and on the realized item containers, and returns those options without changing
    /// <see cref="SelectedOptions" />.
    /// </summary>
    public ISet<ICascaderOption> DoUnCheckedSubTree(ICascaderOption viewOption)
    {
        var unSelectedOptions = new HashSet<ICascaderOption>();
        DoUnCheckedSubTreeCore(viewOption, unSelectedOptions);
        return unSelectedOptions;
    }

    private void DoUnCheckedSubTreeCore(ICascaderOption viewOption, HashSet<ICascaderOption> unSelectedOptions)
    {
        MarkViewOptionChecked(viewOption, false);
        unSelectedOptions.Add(viewOption);
    
        foreach (var childItem in viewOption.Children)
        {
            var container = GetCascaderViewItem(childItem);
            if ((container != null && container.IsEffectiveCheckable()) || 
                (childItem.IsEnabled && childItem.IsCheckBoxEnabled))
            {
                DoUnCheckedSubTreeCore(childItem, unSelectedOptions);
            }
        }
        var (_, unCheckedParentItems) = SetupParentNodeCheckedStatus(viewOption);
        unSelectedOptions.UnionWith(unCheckedParentItems);
    }
    
    private (ISet<ICascaderOption>, ISet<ICascaderOption>) SetupParentNodeCheckedStatus(ICascaderOption viewOption)
    {
        var parent           = viewOption.ParentNode;
        var parentDepth      = Math.Max(0, CountPathDepth(viewOption) - 1);
        var checkedParents   =  new HashSet<ICascaderOption>(parentDepth);
        var unCheckedParents =  new HashSet<ICascaderOption>(parentDepth);
        while (parent is ICascaderOption parentViewOption)
        {
            var parentContainer = GetCascaderViewItem(parentViewOption);
            if ((parentContainer?.IsEnabled ?? parentViewOption.IsEnabled) == false)
            {
                break;
            }
            var (isAllChecked, isAnyChecked) = GetChildCheckedStatus(parentViewOption);
    
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

    private (bool IsAllChecked, bool IsAnyChecked) GetChildCheckedStatus(ICascaderOption parentViewOption)
    {
        var hasChildren  = false;
        var isAllChecked = true;
        var isAnyChecked = false;

        foreach (var childItem in parentViewOption.Children)
        {
            hasChildren = true;
            var childContainer       = GetCascaderViewItem(childItem);
            var childIsEnabled       = childContainer?.IsEnabled ?? childItem.IsEnabled;
            var childIsCheckable     = childContainer?.IsCheckBoxEnabled ?? childItem.IsCheckBoxEnabled;
            var childIsChecked       = childContainer?.IsChecked ?? childItem.IsChecked;
            var isEffectiveCheckable = childIsEnabled && childIsCheckable;
            if (!isEffectiveCheckable)
            {
                continue;
            }

            if (childIsChecked != true)
            {
                isAllChecked = false;
            }

            if (childIsChecked != false)
            {
                isAnyChecked = true;
            }
        }

        return hasChildren
            ? (isAllChecked, isAnyChecked)
            : (false, false);
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
                        var removedItemsClosure = new HashSet<ICascaderOption>(e.OldItems.Count);
                        foreach (var item in e.OldItems)
                        {
                            if (item is ICascaderOption viewOption)
                            {
                                CollectSubTreeOptions(viewOption, removedItemsClosure);
                            }
                        }
                        var tobeRemoved = new List<ICascaderOption>(SelectedOptions.Count);
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

    private static List<ICascaderOption> BuildSelectedOptionsList(ICollection<ICascaderOption> options)
    {
        var selectedOptions = new List<ICascaderOption>(options.Count);
        foreach (var option in options)
        {
            selectedOptions.Add(option);
        }
        return selectedOptions;
    }

    private static HashSet<ICascaderOption> BuildSelectedOptionSet(ICollection<ICascaderOption>? options)
    {
        var selectedOptions = options == null
            ? new HashSet<ICascaderOption>()
            : new HashSet<ICascaderOption>(options.Count);
        if (options != null)
        {
            foreach (var option in options)
            {
                selectedOptions.Add(option);
            }
        }
        return selectedOptions;
    }
}
