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
        var listItem = OwningList?.CreateContainerForItemOverride(item, index, recycleKey);
        Debug.Assert(listItem != null);
        return listItem;
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
        var item = GetContainerFromEventSource(e.Source);
        if (item is ListItem listItem && listItem.IsGroupItem)
        {
            return false;
        }
        return  base.UpdateSelectionFromPointerEvent(source, e);
    }
}