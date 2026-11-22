using AtomUI.Controls.Data;
using AtomUI.Desktop.Controls.Themes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using AtomUI.Theme;

namespace AtomUI.Desktop.Controls.Primitives;

// Token-owner transparent part: the candidate list of AutoComplete and Mentions has no Own Tokens; its ListBox base theme
// reads the owner's globals, so the hosting control's Controls[X] reaches the candidates (their ListBox Own Tokens are
// borrowed, see the mentions / auto-complete token.md).
[TokenOwnerTransparent]
public class CandidateList : ListBox, ICandidateList
{
    /// <summary>
    /// 候选项容器的语义标记类，对应 AutoComplete 系列 popup.listItem 语义部件；
    /// 容器运行时创建，标记与 ListBox 的 semantic-item 一样在容器创建时注入。
    /// </summary>
    internal const string PopupListItemClass = "semantic-popup-list-item";

    #region 公共属性定义

    public static readonly StyledProperty<bool> IsCandidateItemNavigationEnabledProperty =
        AvaloniaProperty.Register<CandidateList, bool>(nameof(IsCandidateItemNavigationEnabled), true);

    public static readonly DirectProperty<CandidateList, object?> CandidateSelectedItemProperty =
        AvaloniaProperty.RegisterDirect<CandidateList, object?>(
            nameof(CandidateSelectedItem),
            o => o.CandidateSelectedItem,
            (o, v) => o.CandidateSelectedItem = v,
            defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);
    
    public static readonly DirectProperty<CandidateList, int> CandidateSelectedIndexProperty =
        AvaloniaProperty.RegisterDirect<CandidateList, int>(
            nameof(CandidateSelectedIndex),
            o => o.CandidateSelectedIndex,
            (o, v) => o.CandidateSelectedIndex = v,
            defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);
    
    public static readonly StyledProperty<int> MaxCountProperty =
        AvaloniaProperty.Register<CandidateList, int>(nameof(MaxCount), int.MaxValue);
    
    public bool IsCandidateItemNavigationEnabled
    {
        get => GetValue(IsCandidateItemNavigationEnabledProperty);
        set => SetValue(IsCandidateItemNavigationEnabledProperty, value);
    }

    private object? _candidateSelectedItem;

    public object? CandidateSelectedItem
    {
        get => _candidateSelectedItem;
        set
        {
            if (value is null)
            {
                ClearActiveCandidate();
            }
            else if (!TrySetCandidateItemSelected(value))
            {
                ClearActiveCandidate();
            }
        }
    }

    private int _candidateSelectedIndex = -1;

    public int CandidateSelectedIndex
    {
        get => _candidateSelectedIndex;
        set
        {
            if (value < 0)
            {
                ClearActiveCandidate();
            }
            else if (!TrySetCandidateItemSelected(value))
            {
                ClearActiveCandidate();
            }
        }
    }
    
    public int MaxCount
    {
        get => GetValue(MaxCountProperty);
        set => SetValue(MaxCountProperty, value);
    }
    #endregion

    #region 内部属性定义

    // Candidate text colours from the host's Token, passed to every prepared item (as ItemSelectedBg).
    internal static readonly StyledProperty<IBrush?> ItemColorProperty =
        AvaloniaProperty.Register<CandidateList, IBrush?>(nameof(ItemColor));

    internal static readonly StyledProperty<IBrush?> ItemSelectedColorProperty =
        AvaloniaProperty.Register<CandidateList, IBrush?>(nameof(ItemSelectedColor));

    // Candidate row margin and corners from the host's Token (a design language insets rounded rows with a gap).
    internal static readonly StyledProperty<Thickness> ItemMarginProperty =
        AvaloniaProperty.Register<CandidateList, Thickness>(nameof(ItemMargin));

    internal static readonly StyledProperty<CornerRadius> ItemCornerRadiusProperty =
        AvaloniaProperty.Register<CandidateList, CornerRadius>(nameof(ItemCornerRadius));

    // The middle-size row height from the host's Token (its option height).
    internal static readonly StyledProperty<double> ItemMinHeightProperty =
        AvaloniaProperty.Register<CandidateList, double>(nameof(ItemMinHeight));

    internal double ItemMinHeight
    {
        get => GetValue(ItemMinHeightProperty);
        set => SetValue(ItemMinHeightProperty, value);
    }

    internal Thickness ItemMargin
    {
        get => GetValue(ItemMarginProperty);
        set => SetValue(ItemMarginProperty, value);
    }

    internal CornerRadius ItemCornerRadius
    {
        get => GetValue(ItemCornerRadiusProperty);
        set => SetValue(ItemCornerRadiusProperty, value);
    }

    internal IBrush? ItemColor
    {
        get => GetValue(ItemColorProperty);
        set => SetValue(ItemColorProperty, value);
    }

    internal IBrush? ItemSelectedColor
    {
        get => GetValue(ItemSelectedColorProperty);
        set => SetValue(ItemSelectedColorProperty, value);
    }

    /// <summary>
    /// Resolves whether a candidate item is enabled, for hosts whose item type is not <see cref="IListItemData"/>
    /// (Mentions uses <see cref="IMentionOption.IsEnabled"/>). When set, it decides both candidate availability
    /// and the container's <c>IsEnabled</c>; when null, only <see cref="IListItemData.IsEnabled"/> is honoured.
    /// </summary>
    internal Func<object?, bool>? ItemEnabledResolver
    {
        get => _itemEnabledResolver;
        set
        {
            if (ReferenceEquals(_itemEnabledResolver, value))
            {
                return;
            }

            _itemEnabledResolver = value;
            ConfigureOptionsForMaxCount();
            ClearActiveCandidateIfUnavailable();
        }
    }

    private Func<object?, bool>? _itemEnabledResolver;

    #endregion
    
    #region 公共事件定义
    public static readonly RoutedEvent<RoutedEventArgs> CommitEvent =
        RoutedEvent.Register<CandidateList, RoutedEventArgs>(
            nameof(Commit),
            RoutingStrategies.Bubble);
    
    public static readonly RoutedEvent<RoutedEventArgs> CancelEvent =
        RoutedEvent.Register<CandidateList, RoutedEventArgs>(
            nameof(Cancel),
            RoutingStrategies.Bubble);
    
    public event EventHandler<RoutedEventArgs>? Commit
    {
        add => AddHandler(CommitEvent, value);
        remove => RemoveHandler(CommitEvent, value);
    }
    
    public event EventHandler<RoutedEventArgs>? Cancel
    {
        add => AddHandler(CancelEvent, value);
        remove => RemoveHandler(CancelEvent, value);
    }
    #endregion
    
    private static readonly FuncTemplate<Panel?> DefaultPanel = new(() => new CandidateVirtualizingStackPanel());

    private ScrollViewer? _scrollViewer;

    static CandidateList()
    {
        SelectedItemProperty.Changed.AddClassHandler<CandidateList>((list, args) => list.HandleSelectItemChanged(args));
        SelectionChangedEvent.AddClassHandler<CandidateList>((list, args) => list.HandleSelectionChanged());
        
        ItemsPanelProperty.OverrideDefaultValue<CandidateList>(DefaultPanel);
    }
    
    private void HandleSelectionChanged()
    {
        if (!IsSingleMode())
        {
            ConfigureOptionsForMaxCount();
        }
    }
    
    private void ConfigureOptionsForMaxCount()
    {
        if (SelectedItems?.Count >= MaxCount)
        {
            for (var i = 0; i < ItemCount; i++)
            {
                var item      = Items[i];
                var container = ContainerFromIndex(i);
                if (!SelectedItems.Contains(item))
                {
                    SetContainerEnabledIfChanged(container, false);
                }
                else
                {
                    SetContainerEnabledIfChanged(container, true);
                }
            }
        }
        else
        {
            for (var i = 0; i < ItemCount; i++)
            {
                var item      = Items[i];
                var container = ContainerFromIndex(i);
                if (ItemEnabledResolver is not null || item is IListItemData)
                {
                    SetContainerEnabledIfChanged(container, IsItemDataEnabled(item));
                }
            }
        }
    }

    private static void SetContainerEnabledIfChanged(Control? container, bool isEnabled)
    {
        if (container is not null && container.IsEnabled != isEnabled)
        {
            container.SetCurrentValue(IsEnabledProperty, isEnabled);
        }
    }
    
    public bool TrySetCandidateItemSelected(object item)
        => TrySetCandidateItemSelectedCore(item);

    private bool TrySetCandidateItemSelectedCore(object item)
    {
        var index = Items.IndexOf(item);
        if (index == -1)
        {
            return false;
        }

        return TrySetCandidateItemSelectedCore(index);
    }
    
    public bool TrySetCandidateItemSelected(int index)
        => TrySetCandidateItemSelectedCore(index);

    private bool TrySetCandidateItemSelectedCore(int index)
    {
        if (index < 0 || index > ItemCount - 1)
        {
            return false;
        }

        var candidateItem = Items[index];
        var candidateContainer = ContainerFromIndex(index) as CandidateListItem;
        if (!IsCandidateAvailable(candidateItem, candidateContainer))
        {
            return false;
        }

        var changed = SetActiveCandidate(index, candidateItem, candidateContainer);
        if (changed && ItemsPanelRoot is CandidateVirtualizingStackPanel virtualizingStackPanel)
        {
            if (virtualizingStackPanel.ScrollCandidateItemIntoView(index) is CandidateListItem realizedContainer)
            {
                candidateContainer = realizedContainer;
            }
        }

        if (candidateContainer is CandidateListItem childContainer)
        {
            SetCandidateItemSelectedIfChanged(childContainer, true);
        }
        return true;
    }

    private void ClearCandidateItemSelection(int index)
    {
        if (index >= 0 &&
            index < ItemCount &&
            ContainerFromIndex(index) is CandidateListItem listItem)
        {
            SetCandidateItemSelectedIfChanged(listItem, false);
        }
    }

    private static void SetCandidateItemSelectedIfChanged(CandidateListItem item, bool isSelected)
    {
        if (item.IsCandidateSelected != isSelected)
        {
            item.SetCurrentValue(CandidateListItem.IsCandidateSelectedProperty, isSelected);
        }
    }

    private bool TrySetCandidateFromContainer(CandidateListItem listItem)
    {
        var index = IndexFromContainer(listItem);
        if (index < 0 || index >= ItemCount || !IsCandidateAvailable(Items[index], listItem))
        {
            return false;
        }

        return SetActiveCandidate(index, Items[index], listItem);
    }

    private bool SetActiveCandidate(int index, object? candidateItem, CandidateListItem? candidateContainer)
    {
        if (CandidateSelectedIndex == index && ReferenceEquals(CandidateSelectedItem, candidateItem))
        {
            if (candidateContainer is not null)
            {
                SetCandidateItemSelectedIfChanged(candidateContainer, true);
            }

            return false;
        }

        ClearCandidateItemSelection(CandidateSelectedIndex);
        SetAndRaise(CandidateSelectedItemProperty, ref _candidateSelectedItem, candidateItem);
        SetAndRaise(CandidateSelectedIndexProperty, ref _candidateSelectedIndex, index);
        if (candidateContainer is not null)
        {
            SetCandidateItemSelectedIfChanged(candidateContainer, true);
        }

        return true;
    }

    private bool IsItemDataEnabled(object? item)
    {
        if (ItemEnabledResolver is { } resolver)
        {
            return resolver(item);
        }

        return item is not IListItemData { IsEnabled: false };
    }

    private bool IsCandidateAvailable(object? item, CandidateListItem? container)
    {
        if (!IsItemDataEnabled(item))
        {
            return false;
        }

        if (!IsSingleMode() && SelectedItems?.Count >= MaxCount && SelectedItems.Contains(item) == false)
        {
            return false;
        }

        return container is null || container.IsEnabled && container.IsVisible;
    }

    private void ClearActiveCandidateIfUnavailable()
    {
        if (CandidateSelectedIndex < 0 ||
            CandidateSelectedIndex >= ItemCount ||
            !IsCandidateAvailable(Items[CandidateSelectedIndex], ContainerFromIndex(CandidateSelectedIndex) as CandidateListItem))
        {
            ClearActiveCandidate();
        }
    }

    private void ClearActiveCandidate()
    {
        if (CandidateSelectedIndex == -1 && CandidateSelectedItem is null)
        {
            return;
        }

        ClearCandidateItemSelection(CandidateSelectedIndex);
        if (CandidateSelectedItem is not null && ContainerFromItem(CandidateSelectedItem) is CandidateListItem itemContainer)
        {
            SetCandidateItemSelectedIfChanged(itemContainer, false);
        }

        SetAndRaise(CandidateSelectedIndexProperty, ref _candidateSelectedIndex, -1);
        SetAndRaise(CandidateSelectedItemProperty, ref _candidateSelectedItem, null);
    }
    
    private void ResetScrollViewer()
    {
        if (_scrollViewer != null)
        {
            _scrollViewer.Offset = new Vector(0, 0);
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _scrollViewer = e.NameScope.Find<ScrollViewer>(ListViewThemeConstants.ScrollViewerPart);
    }
    
    public void HandleKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                if (IsSingleMode())
                {
                    HandleSingleModeCommit();
                }
                else
                {
                    HandleMultiModeCommit();
                }
                
                e.Handled = true;
                break;

            case Key.Up:
                SelectPreviousCandidateItem();
                e.Handled = true;
                break;

            case Key.Down:
                SelectNextCandidateItem();
             
                e.Handled = true;
                break;

            case Key.Escape:
                HandleCancel();
                e.Handled = true;
                break;

            default:
                break;
        }
    }

    private void HandleSingleModeCommit()
    {
        NotifyCommit();
        ClearState();
        ClearActiveCandidate();
    }
    
    private void HandleMultiModeCommit()
    {
        if (CandidateSelectedItem != null)
        {
            if (ContainerFromItem(CandidateSelectedItem) is CandidateListItem listItem)
            {
                var index = IndexFromContainer(listItem);
                if (index != -1)
                {
                    if (listItem.IsSelected)
                    {
                        Selection.Deselect(index);
                    }
                    else
                    {
                        Selection.Select(index);
                    }
                }
            }
        }
    }
    
    protected virtual void SelectPreviousCandidateItem()
    {
        if (ItemCount == 0)
        {
            return;
        }

        if (_candidateSelectedIndex == -1)
        {
            CandidateSelectedIndex = FindNextEnabledIndex(
                SelectedIndex != -1 ? SelectedIndex : 0,
                -1);
        }
        else
        {
            CandidateSelectedIndex = FindNextEnabledIndex(CandidateSelectedIndex, -1);
        }
    }
    
    protected virtual void SelectNextCandidateItem()
    {
        if (ItemCount == 0)
        {
            return;
        }

        if (_candidateSelectedIndex == -1)
        {
            CandidateSelectedIndex = FindNextEnabledIndex(
                SelectedIndex != -1 ? SelectedIndex : -1,
                1);
        }
        else
        {
            CandidateSelectedIndex = FindNextEnabledIndex(CandidateSelectedIndex, 1);
        }
    }

    protected virtual void NotifyCommit()
    {
        if (CandidateSelectedItem != null)
        {
            if (!Equals(SelectedItem, CandidateSelectedItem))
            {
                SetCurrentValue(SelectedItemProperty, CandidateSelectedItem);
            }
        }
        RaiseEvent(new RoutedEventArgs(CommitEvent)
        {
            Source = this,
        });
       
    }

    private void HandleCancel()
    {
        NotifyCancel();
        ClearState();
        ClearActiveCandidate();
    }
    
    protected virtual void NotifyCancel()
    {
        RaiseEvent(new RoutedEventArgs(CancelEvent)
        {
            Source = this,
        });
    }

    private void HandleSelectItemChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue == null)
        {
            ResetScrollViewer();
        }
    }

    private void ClearState()
    {
        if (SelectedItems is not null)
        {
            SelectedItems = null;
        }
        if (SelectedItem is not null)
        {
            SelectedItem = null;
        }
        if (SelectedIndex != -1)
        {
            SelectedIndex = -1;
        }
    }
    
    protected override void NotifyListBoxItemClicked(ListBoxItem item)
    {
        if (IsSingleMode())
        {
            if (item is CandidateListItem candidateListItem)
            {
                TrySetCandidateFromContainer(candidateListItem);
            }
            NotifyCommit();
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (e.Pointer.Type == PointerType.Mouse &&
            GetContainerFromEventSource(e.Source) is CandidateListItem candidateListItem)
        {
            TrySetCandidateFromContainer(candidateListItem);
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

            if (IsCandidateAvailable(Items[index], ContainerFromIndex(index) as CandidateListItem))
            {
                return index;
            }
        }
    }
    
    private bool IsSingleMode()
    {
        return (SelectionMode & SelectionMode.Multiple) != SelectionMode.Multiple;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == MaxCountProperty ||
            change.Property == SelectedItemsProperty)
        {
            ConfigureOptionsForMaxCount();
            ClearActiveCandidateIfUnavailable();
        }

        if (change.Property == SelectedItemsProperty)
        {
            ConfigureEmptyIndicator();
        }
        if (change.Property == ItemsSourceProperty || change.Property == ItemCountProperty ||
            change.Property == FilterValueProperty ||
            change.Property == FilterProperty)
        {
            ClearActiveCandidate();
        }
        
    }
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var listItem = new CandidateListItem();
        listItem.Classes.Add(PopupListItemClass);
        NotifyContainerForItemCreated(listItem, item);
        return listItem;
    }
    
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<CandidateListItem>(item, out recycleKey);
    }
    
    protected override void ConfigureEmptyIndicator()
    {
        var isEffectiveEmptyVisible = IsShowEmptyIndicator &&
                                      (ItemCount == 0 || (IsFiltering && FilterResultCount == 0));
        if (IsEffectiveEmptyVisible != isEffectiveEmptyVisible)
        {
            SetCurrentValue(IsEffectiveEmptyVisibleProperty, isEffectiveEmptyVisible);
        }
        SetCurrentValue(IsDefaultEmptyIndicatorVisibleProperty,
            IsEffectiveEmptyVisible && EmptyIndicator is null && EmptyIndicatorTemplate is null);
    }
    
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        ClearActiveCandidate();
    }

    #region 虚拟化上下文管理

    // The active candidate is an index. A virtualizing panel prepares recycled containers for other indexes, so
    // every prepared container takes its highlight from the index it is prepared for, whatever the item type.
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (ItemEnabledResolver is not null)
        {
            // The base restores IsEnabled saved by index for IListItemData; resolver-backed items are not
            // IListItemData and the saved index may now hold another item (filtering), so the item data decides.
            SetContainerEnabledIfChanged(container, IsCandidateAvailable(item, null));
        }

        if (container is CandidateListItem candidateListItem)
        {
            candidateListItem[!CandidateListItem.ItemColorProperty]         = this[!ItemColorProperty];
            candidateListItem[!CandidateListItem.ItemSelectedColorProperty] = this[!ItemSelectedColorProperty];
            candidateListItem[!CandidateListItem.ItemMarginProperty]        = this[!ItemMarginProperty];
            candidateListItem[!CandidateListItem.ItemCornerRadiusProperty]  = this[!ItemCornerRadiusProperty];
            candidateListItem[!CandidateListItem.ItemMinHeightProperty]     = this[!ItemMinHeightProperty];
            SetCandidateItemSelectedIfChanged(candidateListItem,
                CandidateSelectedIndex >= 0 && index == CandidateSelectedIndex);
        }
    }

    protected override void NotifyClearContainerForVirtualizingContext(ListBoxItem item)
    {
        base.NotifyClearContainerForVirtualizingContext(item);
        if (item is CandidateListItem listItem)
        {
            if (listItem.IsSet(CandidateListItem.IsCandidateSelectedProperty))
            {
                listItem.ClearValue(CandidateListItem.IsCandidateSelectedProperty);
            }
        }
    }
    #endregion
}
