using System.Diagnostics;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;

namespace AtomUI.Desktop.Controls;

internal class GroupableListView : ListBox
{
    #region 公共属性定义

    public static readonly StyledProperty<IDataTemplate?> GroupItemTemplateProperty =
        List.GroupItemTemplateProperty.AddOwner<GroupableListView>();

    public IDataTemplate? GroupItemTemplate
    {
        get => GetValue(GroupItemTemplateProperty);
        set => SetValue(GroupItemTemplateProperty, value);
    }
    #endregion
    
    internal List? OwningList = null;
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var container = OwningList?.CreateContainerForItemOverride(item, index, recycleKey);
        Debug.Assert(container != null);
        if (container is ListItem listItem)
        {
            if (item != null && item is not Visual)
            {
                if (item is IListBoxItemData itemData)
                {
                    if (!listItem.IsSet(ListItem.ContentProperty))
                    {
                        listItem.SetCurrentValue(ListItem.ContentProperty, item);
                    }
                    listItem.SetCurrentValue(ListItem.IsSelectedProperty, itemData.IsSelected);
                    listItem.SetCurrentValue(ListItem.IsEnabledProperty, itemData.IsEnabled);
                }
            }
        }
        
        return container;
    }
    
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        object? recycleKeyValue = null;
        var     result          = OwningList?.NeedsContainer<ListItem>(item, out recycleKeyValue);
        Debug.Assert(result != null);
        recycleKey = recycleKeyValue;
        return result.Value;
    }

    protected override void PrepareListBoxItem(ListBoxItem listBoxItem, object? item, int index, CompositeDisposable disposables)
    {
        base.PrepareListBoxItem(listBoxItem, item, index, disposables);
        if (listBoxItem is ListItem listItem)
        {
            OwningList?.PrepareListBoxItem(listItem, item, index, disposables);
        }
    }

    protected internal override bool UpdateSelectionFromPointerEvent(Control source, PointerEventArgs e)
    {
        var listItem = GetContainerFromEventSource(e.Source) as ListItem;
        if (listItem == null || listItem.IsGroupItem)
        {
            return false;
        }

        if (OwningList != null)
        {
            var result = OwningList.UpdateSelectionFromPointerEvent(listItem, e);
            if (e.Handled)
            {
                return result;
            }
        }

        return base.UpdateSelectionFromPointerEvent(source, e);
    }
    
    protected override void NotifyListBoxItemClicked(ListBoxItem item)
    {
        if (item is ListItem listItem && !listItem.IsGroupItem)
        {
            OwningList?.NotifyListItemClicked(listItem);
        }
    }
    
    internal bool UpdateSelection(ListItem listItem,
                                  bool select = true,
                                  bool rangeModifier = false,
                                  bool toggleModifier = false,
                                  bool rightButton = false,
                                  bool fromFocus = false)
    {
        return UpdateSelectionFromEventSource(listItem, select, rangeModifier, toggleModifier, rightButton, fromFocus);
    }
    
    protected override void ClearContainerForItemOverride(Control element)
    {
        if (element is ListItem listItem)
        {
            OwningList?.ClearContainerForItem(listItem);
        }
        base.ClearContainerForItemOverride(element);
    }
    
    
}