using AtomUI.Generated.AtomUIDesktopControls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace AtomUI.Desktop.Controls;

internal class CascaderViewFilterList : ListBox
{
    #region 内部属性定义

    internal static readonly DirectProperty<CascaderViewFilterList, object?> CandidateSelectedItemProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewFilterList, object?>(
            nameof(CandidateSelectedItem),
            o => o.CandidateSelectedItem,
            (o, v) => o.CandidateSelectedItem = v);

    internal static readonly DirectProperty<CascaderViewFilterList, int> CandidateSelectedIndexProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewFilterList, int>(
            nameof(CandidateSelectedIndex),
            o => o.CandidateSelectedIndex,
            (o, v) => o.CandidateSelectedIndex = v);

    private object? _candidateSelectedItem;

    internal object? CandidateSelectedItem
    {
        get => _candidateSelectedItem;
        set => SetAndRaise(CandidateSelectedItemProperty, ref _candidateSelectedItem, value);
    }

    private int _candidateSelectedIndex = -1;

    internal int CandidateSelectedIndex
    {
        get => _candidateSelectedIndex;
        set => SetAndRaise(CandidateSelectedIndexProperty, ref _candidateSelectedIndex, value);
    }

    #endregion

    static CascaderViewFilterList()
    {
        CandidateSelectedItemProperty.Changed.AddClassHandler<CascaderViewFilterList>(
            (list, args) => list.HandleCandidateSelectedItemChanged(args));
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var listItem = new CascaderViewFilterListItem();
        listItem.Classes.Add(CascaderSemanticParts.PopupListItemClass);
        return listItem;
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<CascaderViewFilterListItem>(item, out recycleKey);
    }

    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index)
    {
        base.ContainerForItemPreparedOverride(container, item, index);
        if (container is CascaderViewFilterListItem listItem)
        {
            listItem.SetCurrentValue(
                CascaderViewFilterListItem.IsCandidateSelectedProperty,
                Equals(item, CandidateSelectedItem));
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (e.Pointer.Type != PointerType.Mouse ||
            GetContainerFromEventSource(e.Source) is not CascaderViewFilterListItem listItem)
        {
            return;
        }

        var index = IndexFromContainer(listItem);
        if (index >= 0 && index < ItemCount &&
            Items[index] is CascaderViewFilterListItemData itemData && itemData.IsEnabled)
        {
            SetCandidate(index);
        }
    }

    #region 内部协作 API

    internal bool TryMoveCandidate(int delta)
    {
        if (ItemCount == 0 || delta == 0)
        {
            return false;
        }

        var startIndex = CandidateSelectedIndex;
        if (startIndex == -1)
        {
            startIndex = delta > 0 ? 0 : ItemCount - 1;
            if (Items[startIndex] is not CascaderViewFilterListItemData itemData || itemData.IsEnabled)
            {
                SetCandidate(startIndex);
                ScrollIntoView(startIndex);
                return true;
            }
        }

        var nextIndex = FindNextEnabledIndex(startIndex, delta);
        if (nextIndex == -1)
        {
            return false;
        }

        SetCandidate(nextIndex);
        ScrollIntoView(nextIndex);
        return true;
    }

    internal void RestoreCandidate(CascaderViewFilterListItemData? previous)
    {
        if (previous?.ExpandItems is not { } previousPath)
        {
            return;
        }
        for (var i = 0; i < ItemCount; i++)
        {
            if (Items[i] is CascaderViewFilterListItemData { IsEnabled: true, ExpandItems: { } path } &&
                path.Count == previousPath.Count &&
                path.Zip(previousPath).All(pair => ReferenceEquals(pair.First, pair.Second)))
            {
                SetCandidate(i);
                return;
            }
        }
    }

    internal void ClearCandidate()
    {
        SetCurrentValue(CandidateSelectedIndexProperty, -1);
        SetCurrentValue(CandidateSelectedItemProperty, null);
    }

    #endregion

    private void HandleCandidateSelectedItemChanged(AvaloniaPropertyChangedEventArgs args)
    {
        UpdateCandidateContainer(args.OldValue, false);
        UpdateCandidateContainer(args.NewValue, true);
    }

    private void UpdateCandidateContainer(object? item, bool isCandidate)
    {
        if (item != null && ContainerFromItem(item) is CascaderViewFilterListItem listItem)
        {
            listItem.SetCurrentValue(CascaderViewFilterListItem.IsCandidateSelectedProperty, isCandidate);
        }
    }

    private int FindNextEnabledIndex(int startIndex, int delta)
    {
        var index     = startIndex;
        var findCycle = false;

        while (true)
        {
            index += delta;
            if (index >= ItemCount)
            {
                index = 0;
                if (!findCycle)
                {
                    findCycle = true;
                }
                else
                {
                    return -1;
                }
            }
            else if (index < 0)
            {
                index = ItemCount - 1;
                if (!findCycle)
                {
                    findCycle = true;
                }
                else
                {
                    return -1;
                }
            }

            if (Items[index] is not CascaderViewFilterListItemData itemData || itemData.IsEnabled)
            {
                return index;
            }
        }
    }

    private void SetCandidate(int index)
    {
        if (index < 0 || index >= ItemCount)
        {
            ClearCandidate();
            return;
        }

        SetCurrentValue(CandidateSelectedItemProperty, Items[index]);
        SetCurrentValue(CandidateSelectedIndexProperty, index);
    }
}
