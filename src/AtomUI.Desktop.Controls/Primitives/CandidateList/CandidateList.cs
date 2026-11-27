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

// Token-owner transparent part: the candidate list of AutoComplete and Mentions has no Own Tokens; its ListBox
// base theme reads the owner's globals, so the hosting control's Controls[X] reaches the candidates (their
// ListBox Own Tokens are borrowed).
/// <summary>
/// The list of candidates that a host control such as AutoComplete or Mentions shows in its popup, based on
/// <see cref="ListBox" />. Next to the selection it keeps an active candidate
/// (<see cref="CandidateSelectedItem" />): the highlighted item that Enter commits or toggles and that the
/// arrow keys and the mouse move. The list declares no tokens of its own. It borrows the ListBox tokens, and
/// global tokens are read from the hosting control.
/// </summary>
[TokenOwnerTransparent]
public class CandidateList : ListBox, ICandidateList
{
    /// <summary>
    /// Semantic marker class of the candidate item container. It matches the <c>popup.listItem</c> semantic part of
    /// the AutoComplete family. The container is created at runtime, and the marker is injected when the container is
    /// created, as the <c>semantic-item</c> marker is for ListBox.
    /// </summary>
    internal const string PopupListItemClass = "semantic-popup-list-item";

    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="IsCandidateItemNavigationEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsCandidateItemNavigationEnabledProperty =
        AvaloniaProperty.Register<CandidateList, bool>(nameof(IsCandidateItemNavigationEnabled), true);

    /// <summary>
    /// Identifies the <see cref="CandidateSelectedItem" /> property.
    /// </summary>
    public static readonly DirectProperty<CandidateList, object?> CandidateSelectedItemProperty =
        AvaloniaProperty.RegisterDirect<CandidateList, object?>(
            nameof(CandidateSelectedItem),
            o => o.CandidateSelectedItem,
            (o, v) => o.CandidateSelectedItem = v,
            defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);
    
    /// <summary>
    /// Identifies the <see cref="CandidateSelectedIndex" /> property.
    /// </summary>
    public static readonly DirectProperty<CandidateList, int> CandidateSelectedIndexProperty =
        AvaloniaProperty.RegisterDirect<CandidateList, int>(
            nameof(CandidateSelectedIndex),
            o => o.CandidateSelectedIndex,
            (o, v) => o.CandidateSelectedIndex = v,
            defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);
    
    /// <summary>
    /// Identifies the <see cref="MaxCount" /> property.
    /// </summary>
    public static readonly StyledProperty<int> MaxCountProperty =
        AvaloniaProperty.Register<CandidateList, int>(nameof(MaxCount), int.MaxValue);
    
    /// <summary>
    /// Meant to switch moving the active candidate with the Up and Down keys on or off; the default is
    /// <see langword="true" />. The value is not read at present: this class ignores it, and
    /// <see cref="HandleKeyDown" /> moves the active candidate whatever it is set to.
    /// </summary>
    public bool IsCandidateItemNavigationEnabled
    {
        get => GetValue(IsCandidateItemNavigationEnabledProperty);
        set => SetValue(IsCandidateItemNavigationEnabledProperty, value);
    }

    private object? _candidateSelectedItem;

    /// <summary>
    /// Gets or sets the active candidate, the highlighted item that a commit acts on, or <see langword="null" />
    /// when no candidate is active. Setting an item that is not available as a candidate (not in the list,
    /// disabled, or blocked by <see cref="MaxCount" />) clears the active candidate instead.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the index of the active candidate (<see cref="CandidateSelectedItem" />) in <c>Items</c>, or -1
    /// when no candidate is active. Setting a negative index, or an index whose item is not available as a
    /// candidate, clears the active candidate.
    /// </summary>
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
    
    /// <summary>
    /// Gets or sets how many items may be selected at the same time. The default, <c>int.MaxValue</c>, sets no
    /// limit. Once this many items are selected, the other rows are disabled and cannot become the active
    /// candidate.
    /// </summary>
    public int MaxCount
    {
        get => GetValue(MaxCountProperty);
        set => SetValue(MaxCountProperty, value);
    }
    #endregion

    #region 内部属性定义

    // Candidate text colors from the host's Token, passed to every prepared item (as ItemSelectedBg).
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
    /// Resolves whether a candidate item is enabled, for hosts whose item type is not <see cref="IListItemData" />
    /// (Mentions uses <see cref="IMentionOption.IsEnabled" />). When set, it decides both candidate availability
    /// and the container's <c>IsEnabled</c>; when null, only <see cref="IListItemData.IsEnabled" /> is honoured.
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
    /// <summary>
    /// Identifies the <see cref="Commit" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> CommitEvent =
        RoutedEvent.Register<CandidateList, RoutedEventArgs>(
            nameof(Commit),
            RoutingStrategies.Bubble);
    
    /// <summary>
    /// Identifies the <see cref="Cancel" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> CancelEvent =
        RoutedEvent.Register<CandidateList, RoutedEventArgs>(
            nameof(Cancel),
            RoutingStrategies.Bubble);
    
    /// <summary>
    /// Occurs when the active candidate is committed, which happens only in single selection (<c>SelectionMode</c>
    /// does not include <c>Multiple</c>): when Enter is pressed, or when a candidate row is clicked. Before the
    /// event is raised, <c>SelectedItem</c> is set to the active candidate when there is one. After Enter the
    /// selection and the active candidate are cleared once the handlers have returned.
    /// </summary>
    public event EventHandler<RoutedEventArgs>? Commit
    {
        add => AddHandler(CommitEvent, value);
        remove => RemoveHandler(CommitEvent, value);
    }
    
    /// <summary>
    /// Occurs when <see cref="HandleKeyDown" /> receives Escape. It is raised before the selection and the active
    /// candidate are cleared; the clearing follows once the handlers have returned.
    /// </summary>
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
    
    /// <summary>
    /// Makes <paramref name="item" /> the active candidate when it is in the list and available as a candidate.
    /// </summary>
    /// <param name="item">
    /// The item to make the active candidate.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the item is the active candidate afterwards. <see langword="false" /> when the
    /// item is not in the list or is not available (disabled, or blocked by <see cref="MaxCount" />); the active
    /// candidate is then left as it was.
    /// </returns>
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
    
    /// <summary>
    /// Makes the item at <paramref name="index" /> the active candidate when the index is in range and the item is
    /// available as a candidate.
    /// </summary>
    /// <param name="index">
    /// The index of the item to make the active candidate.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the item is the active candidate afterwards. <see langword="false" /> when the
    /// index is out of range or the item is not available (disabled, or blocked by <see cref="MaxCount" />); the
    /// active candidate is then left as it was.
    /// </returns>
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

    /// <summary>
    /// Calls the base implementation, then finds the scroll viewer of the template, which is scrolled back to
    /// the top when <c>SelectedItem</c> is cleared. Call the base method from an override to keep that reset.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _scrollViewer = e.NameScope.Find<ScrollViewer>(ListViewThemeConstants.ScrollViewerPart);
    }
    
    /// <summary>
    /// Handles the navigation keys of the list and sets <c>Handled</c> for each of them. Enter commits the active
    /// candidate in single selection and toggles its selection in multiple selection. Up and Down move the active
    /// candidate to the previous or the next available item, wrapping around at the ends. Escape raises
    /// <see cref="Cancel" />. Other keys are left untouched.
    /// </summary>
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
    
    /// <summary>
    /// Moves the active candidate to the previous available item, wrapping around to the last item, and clears the
    /// active candidate when no item is available. With no active candidate, the search starts before the selected
    /// index, or at the first item when nothing is selected. Does nothing on an empty list. Called by
    /// <see cref="HandleKeyDown" /> for the Up key. Call the base method to keep this movement.
    /// </summary>
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
    
    /// <summary>
    /// Moves the active candidate to the next available item, wrapping around to the first item, and clears the
    /// active candidate when no item is available. With no active candidate, the search starts after the selected
    /// index, or before the first item when nothing is selected. Does nothing on an empty list. Called by
    /// <see cref="HandleKeyDown" /> for the Down key. Call the base method to keep this movement.
    /// </summary>
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

    /// <summary>
    /// Sets <c>SelectedItem</c> to the active candidate when there is one and it differs from the current value,
    /// then raises <see cref="Commit" />. Called when Enter is pressed and when a candidate row is clicked, in
    /// single selection. Call the base method to apply the candidate and raise the event.
    /// </summary>
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
    
    /// <summary>
    /// Raises <see cref="Cancel" />. Called when Escape is pressed, before the selection and the active candidate
    /// are cleared. Call the base method to raise the event.
    /// </summary>
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
    
    /// <summary>
    /// Handles a click on a candidate row in single selection: the row becomes the active candidate when it is
    /// available as one, and <see cref="Commit" /> is raised. In multiple selection this does nothing. The base
    /// implementation is not called.
    /// </summary>
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

    /// <summary>
    /// Calls the base implementation, then makes the candidate row under a moving mouse pointer the active
    /// candidate when its item is available as one. Pointer moves of other pointer types, such as touch and pen,
    /// are ignored. Call the base method from an override.
    /// </summary>
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

    /// <summary>
    /// Calls the base implementation, then keeps the list in step with the changed property. A change of
    /// <see cref="MaxCount" /> or <c>SelectedItems</c> updates which rows are enabled for the selection limit and
    /// clears the active candidate when it is no longer available. A change of <c>SelectedItems</c> also
    /// updates the empty indicator. A change of <c>ItemsSource</c>, <c>ItemCount</c>, <c>FilterValue</c> or
    /// <c>Filter</c> clears the active candidate as well. Call the base method from an override.
    /// </summary>
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
    
    /// <summary>
    /// Creates a <see cref="CandidateListItem" /> for the item, adds the <c>semantic-popup-list-item</c> class to
    /// it and reports the new container to the base list. The base implementation is not called.
    /// </summary>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var listItem = new CandidateListItem();
        listItem.Classes.Add(PopupListItemClass);
        NotifyContainerForItemCreated(listItem, item);
        return listItem;
    }
    
    /// <summary>
    /// Reports whether an item needs a <see cref="CandidateListItem" /> container: it does unless the item already
    /// is one, in which case it serves as its own container. The base implementation is not called.
    /// </summary>
    /// <param name="item">
    /// The item to check.
    /// </param>
    /// <param name="index">
    /// The index of the item in the list.
    /// </param>
    /// <param name="recycleKey">
    /// The key the list recycles containers by when a container is needed, or <see langword="null" /> when the
    /// item is its own container.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when a container must be created for the item; otherwise <see langword="false" />.
    /// </returns>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<CandidateListItem>(item, out recycleKey);
    }
    
    /// <summary>
    /// Recomputes <c>IsEffectiveEmptyVisible</c> and <c>IsDefaultEmptyIndicatorVisible</c>. The empty indicator is
    /// effectively visible only when <c>IsShowEmptyIndicator</c> is set and the list has no items, or a filter is
    /// active and matches nothing. The default indicator is used only while neither <c>EmptyIndicator</c> nor
    /// <c>EmptyIndicatorTemplate</c> is set. The base implementation is not called.
    /// </summary>
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
    
    /// <summary>
    /// Calls the base implementation, then clears the active candidate, so that it does not survive the list being
    /// detached. Call the base method from an override.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        ClearActiveCandidate();
    }

    #region 虚拟化上下文管理

    // The active candidate is an index. A virtualizing panel prepares recycled containers for other indexes, so
    // every prepared container takes its highlight from the index it is prepared for, whatever the item type.
    /// <summary>
    /// Calls the base implementation, then prepares a container for the item at <c>index</c>. When
    /// <see cref="ItemEnabledResolver" /> is set, the item data decides the container's <c>IsEnabled</c>. A
    /// <see cref="CandidateListItem" /> also receives the list's item colors, margin, corner radius and minimum
    /// height through bindings, and is highlighted only when <c>index</c> is the index of the active candidate.
    /// Call the base method first from an override: the <c>IsEnabled</c> step corrects what the base method restored.
    /// </summary>
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

    /// <summary>
    /// Calls the base implementation, then clears the locally set <c>IsCandidateSelected</c> value of a
    /// <see cref="CandidateListItem" />: the highlight belongs to the index the container was prepared for, and a
    /// cleared container no longer stands for that index. Call the base method from an override.
    /// </summary>
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
