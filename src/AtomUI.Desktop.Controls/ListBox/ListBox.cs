using AtomUI.Animations;
using System.Collections.Specialized;
using AtomUI.Controls;
using AtomUI.Controls.Data;
using AtomUI.Controls.Utils;
using AtomUI.Generated.AtomUIDesktopControls;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using AvaloniaListBox = Avalonia.Controls.ListBox;

/// <summary>
/// A list that presents its items as <see cref="ListBoxItem" /> rows with AtomUI sizing, split lines, an empty
/// indicator and text filtering. The row settings of the list are forwarded to every row container.
/// </summary>
public partial class ListBox : AvaloniaListBox,
                       ICustomizableSizeTypeAware,
                       IMotionAwareControl,
                       IListVirtualizingContextAware
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="IsSelectable" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsSelectableProperty =
        AvaloniaProperty.Register<ListBox, bool>(nameof(IsSelectable), true);
    
    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<ListBox>();
    
    /// <summary>
    /// Identifies the <see cref="IsBorderless" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsBorderlessProperty =
        AvaloniaProperty.Register<ListBox, bool>(nameof(IsBorderless), false);
        
    /// <summary>
    /// Identifies the <see cref="ItemHoverBg" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> ItemHoverBgProperty =
        AvaloniaProperty.Register<ListBox, IBrush?>(nameof(ItemHoverBg));
    
    /// <summary>
    /// Identifies the <see cref="ItemSelectedBg" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> ItemSelectedBgProperty =
        AvaloniaProperty.Register<ListBox, IBrush?>(nameof(ItemSelectedBg));
    
    /// <summary>
    /// Identifies the <see cref="IsShowSelectedIndicator" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowSelectedIndicatorProperty =
        AvaloniaProperty.Register<ListBox, bool>(nameof(IsShowSelectedIndicator), false);
    
    /// <summary>
    /// Identifies the <see cref="SelectedIndicator" /> property.
    /// </summary>
    public static readonly StyledProperty<IconTemplate?> SelectedIndicatorProperty =
        AvaloniaProperty.Register<ListBox, IconTemplate?>(nameof(SelectedIndicator));
    
    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<ListBox>();
    
    /// <summary>
    /// Identifies the <see cref="EmptyIndicatorPadding" /> property.
    /// </summary>
    public static readonly StyledProperty<Thickness> EmptyIndicatorPaddingProperty =
        AvaloniaProperty.Register<ListBox, Thickness>(nameof(EmptyIndicatorPadding));
    
    /// <summary>
    /// Identifies the <see cref="EmptyIndicator" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> EmptyIndicatorProperty =
        AvaloniaProperty.Register<ListBox, object?>(nameof(EmptyIndicator));
    
    /// <summary>
    /// Identifies the <see cref="EmptyIndicatorTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> EmptyIndicatorTemplateProperty =
        AvaloniaProperty.Register<ListBox, IDataTemplate?>(nameof(EmptyIndicatorTemplate));
    
    /// <summary>
    /// Identifies the <see cref="IsShowEmptyIndicator" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowEmptyIndicatorProperty =
        AvaloniaProperty.Register<ListBox, bool>(nameof(IsShowEmptyIndicator), true);
    
    /// <summary>
    /// Identifies the <see cref="Filter" /> property.
    /// </summary>
    public static readonly StyledProperty<IValueFilter?> FilterProperty =
        AvaloniaProperty.Register<ListBox, IValueFilter?>(nameof(Filter));

    /// <summary>
    /// Identifies the <see cref="FilterValue" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> FilterValueProperty =
        AvaloniaProperty.Register<ListBox, object?>(nameof(FilterValue));

    /// <summary>
    /// Identifies the <see cref="FilterValueSelector" /> property.
    /// </summary>
    public static readonly StyledProperty<DefaultFilterValueSelector?> FilterValueSelectorProperty =
        AvaloniaProperty.Register<ListBox, DefaultFilterValueSelector?>(nameof(FilterValueSelector));

    /// <summary>
    /// Identifies the <see cref="FilterHighlightStrategy" /> property.
    /// </summary>
    public static readonly StyledProperty<TextBlockHighlightStrategy> FilterHighlightStrategyProperty =
        AvaloniaProperty.Register<ListBox, TextBlockHighlightStrategy>(nameof(FilterHighlightStrategy), TextBlockHighlightStrategy.All);
    
    /// <summary>
    /// Identifies the <see cref="FilterResultCount" /> property.
    /// </summary>
    public static readonly DirectProperty<ListBox, int> FilterResultCountProperty =
        AvaloniaProperty.RegisterDirect<ListBox, int>(nameof(FilterResultCount),
            o => o.FilterResultCount);
    
    /// <summary>
    /// Identifies the <see cref="IsFiltering" /> property.
    /// </summary>
    public static readonly DirectProperty<ListBox, bool> IsFilteringProperty =
        AvaloniaProperty.RegisterDirect<ListBox, bool>(nameof(IsFiltering),
            o => o.IsFiltering);
    
    /// <summary>
    /// Identifies the <see cref="FilterHighlightForeground" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> FilterHighlightForegroundProperty =
        AvaloniaProperty.Register<ListBox, IBrush?>(nameof(FilterHighlightForeground));
    
    /// <summary>
    /// Gets or sets whether the user can select rows with the pointer or the keyboard. The default is
    /// <see langword="true" />. Setting the value to <see langword="false" /> clears the current selection.
    /// </summary>
    public bool IsSelectable
    {
        get => GetValue(IsSelectableProperty);
        set => SetValue(IsSelectableProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the size step of the list. It is forwarded to every row, whose theme takes the minimum height
    /// and padding of the row from it, and selects the corner radius of the list and the padding of the empty
    /// indicator. Clearing the value restores the default of
    /// <see cref="CustomizableSizeTypeControlProperty.SizeTypeProperty" />.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the background of an enabled row while the pointer is over it. A selected row keeps its
    /// <see cref="ItemSelectedBg" /> instead. The value is forwarded to every row; the theme sets it from the
    /// ListBox token <c>ItemHoverBgColor</c>, and <see langword="null" /> paints no hover background.
    /// </summary>
    public IBrush? ItemHoverBg
    {
        get => GetValue(ItemHoverBgProperty);
        set => SetValue(ItemHoverBgProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the background of a selected row that is enabled, whether or not the pointer is over it.
    /// The value is forwarded to every row; the theme sets it from the ListBox token
    /// <c>ItemSelectedBgColor</c>, and <see langword="null" /> paints none.
    /// </summary>
    public IBrush? ItemSelectedBg
    {
        get => GetValue(ItemSelectedBgProperty);
        set => SetValue(ItemSelectedBgProperty, value);
    }
    
    /// <summary>
    /// Gets or sets whether the list has no frame border. When <see langword="true" />, the frame is drawn
    /// without a border whatever its border thickness, and every row including the last one shows its split
    /// line. The default is <see langword="false" />, which leaves the last row without a split line.
    /// </summary>
    public bool IsBorderless
    {
        get => GetValue(IsBorderlessProperty);
        set => SetValue(IsBorderlessProperty, value);
    }
    
    /// <summary>
    /// Gets or sets whether changes of the background and foreground of a row animate. The value is forwarded
    /// to every row; the theme sets it from the EnableMotion token. The list switches it off on a row while
    /// the row is prepared or recycled, so that restoring the state of the row does not animate.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    /// <summary>
    /// Gets or sets whether a selected row shows the <see cref="SelectedIndicator" /> on its right. The
    /// value is forwarded to every row and has an effect only on rows that are selected. The default is
    /// <see langword="false" />.
    /// </summary>
    public bool IsShowSelectedIndicator
    {
        get => GetValue(IsShowSelectedIndicatorProperty);
        set => SetValue(IsShowSelectedIndicatorProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the icon that a selected row shows on its right when
    /// <see cref="IsShowSelectedIndicator" /> is <see langword="true" />. The value is forwarded to every
    /// row; the theme supplies a check mark, and <see langword="null" /> shows no icon.
    /// </summary>
    public IconTemplate? SelectedIndicator
    {
        get => GetValue(SelectedIndicatorProperty);
        set => SetValue(SelectedIndicatorProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the space around the empty indicator: the padding of a custom indicator and the margin
    /// of the default one. The theme derives it from <see cref="SizeType" />.
    /// </summary>
    public Thickness EmptyIndicatorPadding
    {
        get => GetValue(EmptyIndicatorPaddingProperty);
        set => SetValue(EmptyIndicatorPaddingProperty, value);
    }
        
    /// <summary>
    /// Gets or sets the content shown in place of the list when no row is visible, i.e. the list is empty
    /// or the filter matches nothing. While it or <see cref="EmptyIndicatorTemplate" /> is set, the default
    /// empty indicator is hidden.
    /// </summary>
    [DependsOn(nameof(EmptyIndicatorTemplate))]
    public object? EmptyIndicator
    {
        get => GetValue(EmptyIndicatorProperty);
        set => SetValue(EmptyIndicatorProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the template that presents <see cref="EmptyIndicator" />. Setting it hides the default
    /// empty indicator, also when <see cref="EmptyIndicator" /> is <see langword="null" />.
    /// </summary>
    public IDataTemplate? EmptyIndicatorTemplate
    {
        get => GetValue(EmptyIndicatorTemplateProperty);
        set => SetValue(EmptyIndicatorTemplateProperty, value);
    }
    
    /// <summary>
    /// Gets or sets whether an indicator replaces the list when no row is visible, i.e. the list is empty
    /// or the filter matches nothing. When <see langword="false" />, no indicator is shown. The default is
    /// <see langword="true" />.
    /// </summary>
    public bool IsShowEmptyIndicator
    {
        get => GetValue(IsShowEmptyIndicatorProperty);
        set => SetValue(IsShowEmptyIndicatorProperty, value);
    }

    /// <summary>
    /// Gets or sets the rule that decides whether an item matches <see cref="FilterValue" />. It receives
    /// the value picked by <see cref="FilterValueSelector" /> and the filter value, and returns whether
    /// the item matches. When no filter is set at initialization, a
    /// <see cref="ValueFilterMode.Contains" /> filter is set instead; clearing the property afterwards ends
    /// filtering.
    /// </summary>
    public IValueFilter? Filter
    {
        get => GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }

    /// <summary>
    /// Gets or sets the value that items are matched against by <see cref="Filter" />. Filtering is active
    /// only while both properties are set; <see langword="null" /> shows every row.
    /// </summary>
    public object? FilterValue
    {
        get => GetValue(FilterValueProperty);
        set => SetValue(FilterValueProperty, value);
    }

    /// <summary>
    /// Gets or sets a function that picks, from an item, the value passed to <see cref="Filter" />. When
    /// it is <see langword="null" />, the text of an <see cref="IListItemData" /> is used, or a
    /// <see langword="string" /> item itself, or otherwise the item.
    /// </summary>
    public DefaultFilterValueSelector? FilterValueSelector
    {
        get => GetValue(FilterValueSelectorProperty);
        set => SetValue(FilterValueSelectorProperty, value);
    }

    /// <summary>
    /// Gets or sets how a row presents the text of its item while filtering is active: whether the matched
    /// text is painted in <see cref="FilterHighlightForeground" />, whether it is bold, and whether rows
    /// that do not match are hidden. The value is forwarded to every row. The default,
    /// <see cref="TextBlockHighlightStrategy.All" />, does all three.
    /// </summary>
    public TextBlockHighlightStrategy FilterHighlightStrategy
    {
        get => GetValue(FilterHighlightStrategyProperty);
        set => SetValue(FilterHighlightStrategyProperty, value);
    }

    private int _filterResultCount;
    
    /// <summary>
    /// Gets the number of items that match the current filter, or <c>0</c> when filtering is not active.
    /// </summary>
    public int FilterResultCount
    {
        get => _filterResultCount;
        private set => SetAndRaise(FilterResultCountProperty, ref _filterResultCount, value);
    }
    
    private bool _isFiltering;
    
    /// <summary>
    /// Gets whether the items are currently filtered, which is the case while both <see cref="Filter" />
    /// and <see cref="FilterValue" /> are set. While <see langword="true" />, each row shows the emphasized
    /// text of its item instead of its content.
    /// </summary>
    public bool IsFiltering
    {
        get => _isFiltering;
        private set => SetAndRaise(IsFilteringProperty, ref _isFiltering, value);
    }

    /// <summary>
    /// Gets or sets the foreground of the matched text of an item while filtering is active. The value is
    /// forwarded to every row; the theme sets it from the ListBox token <c>FilterHighlightColor</c>.
    /// </summary>
    public IBrush? FilterHighlightForeground
    {
        get => GetValue(FilterHighlightForegroundProperty);
        set => SetValue(FilterHighlightForegroundProperty, value);
    }
    #endregion

    #region 公共事件定义

    /// <summary>
    /// Raised when a row container of this list is clicked, after <see cref="NotifyListBoxItemClicked" />
    /// was called for it. The argument carries the clicked row container. The event cannot be canceled.
    /// </summary>
    public event EventHandler<ListBoxItemClickedEventArgs>? ItemClicked;
    /// <summary>
    /// Raised after the number of items of the list changed. The argument carries the new number of items.
    /// </summary>
    public event EventHandler<ItemCountChangedEventArgs>? ItemCountChanged;

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<ListBox, Thickness> EffectiveBorderThicknessProperty =
        AvaloniaProperty.RegisterDirect<ListBox, Thickness>(nameof(EffectiveBorderThickness),
            o => o.EffectiveBorderThickness,
            (o, v) => o.EffectiveBorderThickness = v);
    
    internal static readonly DirectProperty<ListBox, bool> IsEffectiveEmptyVisibleProperty =
        AvaloniaProperty.RegisterDirect<ListBox, bool>(
            nameof(IsEffectiveEmptyVisible),
            o => o.IsEffectiveEmptyVisible,
            (o, v) => o.IsEffectiveEmptyVisible = v);

    internal static readonly DirectProperty<ListBox, bool> IsDefaultEmptyIndicatorVisibleProperty =
        AvaloniaProperty.RegisterDirect<ListBox, bool>(
            nameof(IsDefaultEmptyIndicatorVisible),
            o => o.IsDefaultEmptyIndicatorVisible,
            (o, v) => o.IsDefaultEmptyIndicatorVisible = v);

    // The check mark color of a selected item, from the theme (ListBox: its Own Token; candidate lists: the host's
    // primary color).
    internal static readonly StyledProperty<IBrush?> ItemSelectedIndicatorColorProperty =
        AvaloniaProperty.Register<ListBox, IBrush?>(nameof(ItemSelectedIndicatorColor));
    
    private Thickness _effectiveBorderThickness;

    internal Thickness EffectiveBorderThickness
    {
        get => _effectiveBorderThickness;
        set => SetAndRaise(EffectiveBorderThicknessProperty, ref _effectiveBorderThickness, value);
    }
    
    private bool _isEffectiveEmptyVisible = false;
    internal bool IsEffectiveEmptyVisible
    {
        get => _isEffectiveEmptyVisible;
        set => SetAndRaise(IsEffectiveEmptyVisibleProperty, ref _isEffectiveEmptyVisible, value);
    }

    private bool _isDefaultEmptyIndicatorVisible;
    internal bool IsDefaultEmptyIndicatorVisible
    {
        get => _isDefaultEmptyIndicatorVisible;
        set => SetAndRaise(IsDefaultEmptyIndicatorVisibleProperty, ref _isDefaultEmptyIndicatorVisible, value);
    }

    internal IBrush? ItemSelectedIndicatorColor
    {
        get => GetValue(ItemSelectedIndicatorColorProperty);
        set => SetValue(ItemSelectedIndicatorColorProperty, value);
    }

    #endregion
    
    private protected readonly Dictionary<object, bool> _filterContext = new();
    private protected readonly Dictionary<int, bool> _filterResults = new();
    private protected readonly Dictionary<object, IDictionary<object, object?>> _virtualRestoreContext = new();

    static ListBox()
    {
        ListBoxItem.ClickedEvent.AddClassHandler<ListBox>((list, args) => list.HandleListBoxItemClicked(args));
        IsSelectableProperty.Changed.AddClassHandler<ListBox>((list, args) => list.HandleIsSelectableChanged(args));
        ItemCountProperty.Changed.AddClassHandler<ListBox>((list, args) => list.HandleItemCountChanged());
    }
    
    public ListBox()
    {
        Items.CollectionChanged += HandleItemCollectionChanged;
    }

    /// <summary>
    /// Calls the base method, sets a <see cref="ValueFilterMode.Contains" /> filter on <see cref="Filter" />
    /// when no filter is set, and updates the empty indicator flags and <see cref="IsFiltering" />. An
    /// override calls the base method to get the default filter.
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (Filter == null)
        {
            SetCurrentValue(FilterProperty, ValueFilterFactory.BuildFilter(ValueFilterMode.Contains));
        }

        ConfigureEmptyIndicator();
        ConfigureIsFiltering();
    }

    /// <summary>
    /// Calls the base method and applies the filter to the items when both <see cref="Filter" /> and
    /// <see cref="FilterValue" /> are set; the filter is not applied before the list is loaded. An override
    /// calls the base method so that the filter runs.
    /// </summary>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (Filter != null && FilterValue != null)
        {
            FilterItems();
        }
    }
    
    private void HandleItemCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        _virtualRestoreContext.Clear();
        ConfigureEmptyIndicator();
        RefreshContainerSplitLines();
        FilterItems();
    }

    /// <summary>
    /// Calls the base method and keeps the list in step with the change. Changes of the item source, the
    /// filter state and result, and the empty indicator settings update the empty indicator flags;
    /// changes of <c>BorderThickness</c> and <see cref="IsBorderless" /> update the effective border
    /// thickness and the split lines; changes of <see cref="Filter" />, <see cref="FilterValue" />,
    /// <see cref="FilterValueSelector" /> and <see cref="FilterHighlightStrategy" /> update the filter
    /// state and run the filter again. An override calls the base method to keep these updates.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ItemsSourceProperty ||
            change.Property == IsFilteringProperty ||
            change.Property == FilterResultCountProperty ||
            change.Property == IsShowEmptyIndicatorProperty ||
            change.Property == EmptyIndicatorProperty ||
            change.Property == EmptyIndicatorTemplateProperty)
        {
            ConfigureEmptyIndicator();
        }
        else if (change.Property == BorderThicknessProperty ||
                 change.Property == IsBorderlessProperty)
        {
            ConfigureEffectiveBorderThickness();
            RefreshContainerSplitLines();
        }
        else if (change.Property == FilterValueProperty ||
                 change.Property == FilterProperty ||
                 change.Property == FilterValueSelectorProperty ||
                 change.Property == FilterHighlightStrategyProperty)
        {
            ConfigureIsFiltering();
            FilterItems();
        }
    }

    private void HandleIsSelectableChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is bool && (bool)e.NewValue == false)
        {
            SetCurrentValue(SelectedIndexProperty, -1);
            SetCurrentValue(SelectedItemProperty, null);
            SetCurrentValue(SelectedItemsProperty, null);
        }
    }
    
    private void ConfigureIsFiltering()
    {
        IsFiltering = Filter != null && FilterValue != null;
    }
    
    /// <summary>
    /// Creates a <see cref="ListBoxItem" /> for an item and reports it through
    /// <see cref="NotifyContainerForItemCreated" />. The semantic part marker class is added only when the
    /// runtime type is exactly <see cref="ListBox" />, because a derived list has its own marker. A derived
    /// list that creates another container type calls <see cref="NotifyContainerForItemCreated" /> itself.
    /// </summary>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var listBoxItem = new ListBoxItem();
        if (GetType() == typeof(ListBox))
        {
            listBoxItem.Classes.Add(ListBoxSemanticParts.ItemClass);
        }
        NotifyContainerForItemCreated(listBoxItem, item);
        return listBoxItem;
    }
    
    /// <summary>
    /// Called for each container the list creates, before the container is prepared for its item. When the
    /// item is an <see cref="IListItemData" /> and not a visual, the base method applies the default state
    /// of the item to the container through <see cref="NotifyRestoreDefaultContext" />; for other items it
    /// does nothing. An override calls the base method to keep that restore.
    /// </summary>
    protected virtual void NotifyContainerForItemCreated(Control container, object? item)
    {
        if (container is ListBoxItem listBoxItem && item != null && item is not Visual)
        {
            if (item is IListItemData itemData)
            {
                NotifyRestoreDefaultContext(listBoxItem, itemData);
            }
        }
    }

    /// <summary>
    /// Determines whether an item needs a container that the list creates for it.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> when the item is neither a <see cref="ListBoxItem" /> nor another visual;
    /// then <c>recycleKey</c> is <see langword="null" />. Otherwise the item is its own container and is
    /// returned as the <c>recycleKey</c>.
    /// </returns>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<ListBoxItem>(item, out recycleKey);
    }
    
    /// <summary>
    /// Calls the base method and prepares a <see cref="ListBoxItem" /> for an item: sets whether its split
    /// line shows, binds the row settings of the list to it, binds or clears its content template, calls
    /// <see cref="PrepareListBoxItem" />, then applies the default state of the item and a context saved
    /// for the index, and applies the current filter result. Motion is switched off on the row until this
    /// is done, so that the row does not animate into its restored state.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <c>container</c> is not a <see cref="ListBoxItem" />.
    /// </exception>
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is ListBoxItem listBoxItem)
        {
            if (GetType() == typeof(ListBox))
            {
                listBoxItem.Classes.Add(ListBoxSemanticParts.ItemClass);
            }

            listBoxItem.IsSplitLineVisible = ShouldShowSplitLine(index == ItemCount - 1);
            if (ItemTemplate != null)
            {
                listBoxItem[!ListBoxItem.ContentTemplateProperty] = this[!ItemTemplateProperty];
            }
            else
            {
                listBoxItem.ClearValue(ListBoxItem.ContentTemplateProperty);
            }
            listBoxItem[!ListBoxItem.SizeTypeProperty]                  = this[!SizeTypeProperty];
            listBoxItem[!ListBoxItem.IsMotionEnabledProperty]           = this[!IsMotionEnabledProperty];
            listBoxItem[!ListBoxItem.SelectedIndicatorProperty]         = this[!SelectedIndicatorProperty];
            listBoxItem[!ListBoxItem.ItemHoverBgProperty]               = this[!ItemHoverBgProperty];
            listBoxItem[!ListBoxItem.ItemSelectedBgProperty]            = this[!ItemSelectedBgProperty];
            listBoxItem[!ListBoxItem.ItemSelectedIndicatorColorProperty] = this[!ItemSelectedIndicatorColorProperty];
            listBoxItem[!ListBoxItem.IsShowSelectedIndicatorProperty]   = this[!IsShowSelectedIndicatorProperty];
            listBoxItem[!ListBoxItem.FilterHighlightStrategyProperty]   = this[!FilterHighlightStrategyProperty];
            listBoxItem[!ListBoxItem.FilterHighlightForegroundProperty] = this[!FilterHighlightForegroundProperty];
           
            PrepareListBoxItem(listBoxItem, item, index);

            var originMotionEnabled = false;
            try
            {
                originMotionEnabled = listBoxItem.IsMotionEnabled;
                listBoxItem.SetCurrentValue(IsMotionEnabledProperty, false);
                if (item is IListItemData itemData)
                {
                    NotifyRestoreDefaultContext(listBoxItem, itemData);
                }

                if (this is IListVirtualizingContextAware listVirtualizingContextAwareControl &&
                    listBoxItem is IListItemVirtualizingContextAware virtualListItem)
                {
                    virtualListItem.VirtualIndex = index;
                    if (_virtualRestoreContext.TryGetValue(index, out var context))
                    {
                        listVirtualizingContextAwareControl.RestoreVirtualizingContext(listBoxItem, context);
                        _virtualRestoreContext.Remove(index);
                    }
                }

                ApplyCurrentFilterState(listBoxItem, index);
            }
            finally
            {
                listBoxItem.SetCurrentValue(IsMotionEnabledProperty, originMotionEnabled);
            }
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type ListBoxItem.");
        }
    }
    
    /// <summary>
    /// Called from <see cref="PrepareContainerForItemOverride" /> after the row settings of the list are
    /// bound to the row and before the default state of the item, a saved context and the filter result
    /// are applied. The base method does nothing.
    /// </summary>
    protected virtual void PrepareListBoxItem(ListBoxItem listBoxItem, object? item, int index)
    {
    }
    
    /// <summary>
    /// Updates <see cref="IsEffectiveEmptyVisible" /> and <see cref="IsDefaultEmptyIndicatorVisible" />
    /// from the current number of items, the filter result and the empty indicator settings. The list
    /// calls it whenever one of them changes; a derived list calls it when its own inputs change. An
    /// override calls the base method first and then adjusts the two flags.
    /// </summary>
    protected virtual void ConfigureEmptyIndicator()
    {
        IsEffectiveEmptyVisible        = IsShowEmptyIndicator && (ItemCount == 0 || (IsFiltering && FilterResultCount == 0));
        IsDefaultEmptyIndicatorVisible = IsEffectiveEmptyVisible && EmptyIndicator is null && EmptyIndicatorTemplate is null;
    }
    
    private void ConfigureEffectiveBorderThickness()
    {
        if (IsBorderless)
        {
            EffectiveBorderThickness = new Thickness(0);
        }
        else
        {
            EffectiveBorderThickness = BorderThickness;
        }
    }

    private bool ShouldShowSplitLine(bool isLastItem)
    {
        return IsBorderless || !isLastItem;
    }

    private bool IsLastItem(Control container)
    {
        return IndexFromContainer(container) == ItemCount - 1;
    }

    private void RefreshContainerSplitLines()
    {
        var presenter = this.GetVisualDescendants().OfType<ItemsPresenter>().FirstOrDefault();
        if (presenter?.Panel is null)
        {
            return;
        }

        // Realized containers only; unrealized ones receive the correct value when
        // they are prepared. A container's item is its Content for generated
        // containers and the container itself for directly supplied containers.
        foreach (var child in presenter.Panel.Children)
        {
            if (child is not ListBoxItem listBoxItem)
            {
                continue;
            }

            listBoxItem.IsSplitLineVisible = ShouldShowSplitLine(IsLastItem(listBoxItem));
        }
    }
    
    /// <summary>
    /// Updates the selection from an input event on a row and returns whether the event is to be treated
    /// as handled. The selection is left as it is and <see langword="false" /> is returned when the list
    /// is not selectable or the event is a key event, because Space and Enter are committed through
    /// <see cref="OnKeyDown" /> with the policy of the list. For any other event the base method is
    /// called and its result is returned.
    /// </summary>
    public override bool UpdateSelectionFromEvent(Control container, RoutedEventArgs eventArgs)
    {
        // Avalonia's ListBoxItem commits Space / Enter itself (always replacing a multiple selection); keyboard
        // commits go through OnKeyDown instead, which applies the list's commit policy.
        if (!IsSelectable || eventArgs is KeyEventArgs)
        {
            return false;
        }
        return base.UpdateSelectionFromEvent(container, eventArgs);
    }

    /// <summary>
    /// Called by a row container for a pointer event that may select it. The base method forwards to
    /// <see cref="UpdateSelectionFromEvent" />. A derived list overrides it to decide pointer selection
    /// itself; calling the base method keeps the behavior of <see cref="UpdateSelectionFromEvent" />.
    /// </summary>
    protected internal virtual bool UpdateSelectionFromPointerEvent(Control source, PointerEventArgs e)
    {
        return UpdateSelectionFromEvent(source, e);
    }

    /// <summary>
    /// Returns whether the content of a row matches the current <see cref="Filter" /> and
    /// <see cref="FilterValue" />. The list itself evaluates its data items directly and does not call
    /// this method.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> when the content matches, <see langword="false" /> when it does not or no
    /// filter is set.
    /// </returns>
    protected bool FilterItem(ListBoxItem item)
    {
        return FilterDataItem(item.Content);
    }

    private bool FilterDataItem(object? item)
    {
        if (Filter == null)
        {
            return false;
        }
        var value = SelectFilterValue(item);
        return Filter.Filter(value, FilterValue);
    }

    private object? SelectFilterValue(object? item)
    {
        var content = item is ListBoxItem listBoxItem ? listBoxItem.Content : item;
        var selector = FilterValueSelector;
        if (selector != null)
        {
            return selector(content);
        }
        if (content is IListItemData listItemData)
        {
            return listItemData.Content?.ToString();
        }
        if (content is string header)
        {
            return header;
        }
        return content;
    }

    private void FilterItems()
    {
        if (Filter != null && FilterValue != null && IsLoaded)
        {
            _filterResults.Clear();
            var count = 0;
            for (var index = 0; index < Items.Count; ++index)
            {
                var item = Items[index];
                if (item != null)
                {
                    var filterResult = FilterDataItem(item);
                    _filterResults[index] = filterResult;
                    if (filterResult)
                    {
                        ++count;
                    }
                }
            }
            FilterResultCount = count;
            IsFiltering       = true;
            ApplyFilterResultsToRealizedContainers();
        }
        else
        {
            ClearFilter();
        }
    }

    private void ClearFilter()
    {
        for (var index = 0; index < Items.Count; ++index)
        {
            if (ContainerFromIndex(index) is ListBoxItem listBoxItem)
            {
                ClearFilterState(listBoxItem);
            }
        }
        FilterResultCount = 0;
        IsFiltering       = false;
        _filterContext.Clear();
        _filterResults.Clear();
    }

    private void ApplyFilterResultsToRealizedContainers()
    {
        for (var index = 0; index < Items.Count; ++index)
        {
            if (_filterResults.TryGetValue(index, out var filterResult) &&
                ContainerFromIndex(index) is ListBoxItem listBoxItem)
            {
                ApplyFilterState(listBoxItem, filterResult);
            }
        }
    }

    private void ApplyCurrentFilterState(ListBoxItem listBoxItem, int index)
    {
        if (Filter != null &&
            FilterValue != null &&
            IsLoaded &&
            _filterResults.TryGetValue(index, out var filterResult))
        {
            ApplyFilterState(listBoxItem, filterResult);
        }
        else
        {
            ClearFilterState(listBoxItem);
        }
    }

    private void ApplyFilterState(ListBoxItem listBoxItem, bool filterResult)
    {
        if (IsHideUnmatchedFilterEnabled())
        {
            if (!_filterContext.ContainsKey(listBoxItem))
            {
                _filterContext[listBoxItem] = listBoxItem.IsVisible;
            }
            listBoxItem.SetCurrentValue(ListBoxItem.IsVisibleProperty, filterResult);
        }
        else
        {
            RestoreFilterVisibility(listBoxItem);
        }

        listBoxItem.IsFiltering = true;
        listBoxItem.FilterValue = FilterValue;
    }

    private void ClearFilterState(ListBoxItem listBoxItem)
    {
        RestoreFilterVisibility(listBoxItem);
        listBoxItem.IsFiltering = false;
        listBoxItem.FilterValue = null;
    }

    private void RestoreFilterVisibility(ListBoxItem listBoxItem)
    {
        if (_filterContext.TryGetValue(listBoxItem, out var value))
        {
            listBoxItem.SetCurrentValue(ListBoxItem.IsVisibleProperty, value);
            _filterContext.Remove(listBoxItem);
        }
    }

    private bool IsHideUnmatchedFilterEnabled()
    {
        return (FilterHighlightStrategy & TextBlockHighlightStrategy.HideUnMatched) == TextBlockHighlightStrategy.HideUnMatched;
    }
    
    private void HandleListBoxItemClicked(RoutedEventArgs args)
    {
        if (args.Source is ListBoxItem item)
        {
            NotifyListBoxItemClicked(item);
            ItemClicked?.Invoke(this, new ListBoxItemClickedEventArgs(item));
        }
    }
    
    /// <summary>
    /// Called for the row container that was clicked, before <see cref="ItemClicked" /> is raised. The
    /// base method does nothing.
    /// </summary>
    protected virtual void NotifyListBoxItemClicked(ListBoxItem item)
    {
    }
    
    // Keyboard model: the item containers are the focus targets and the list is one Tab stop (Avalonia's own
    // model). Directional keys move focus to the next navigable item (enabled, visible, focusable); whether the
    // selection follows depends on the selection mode, the same commit policy as TreeView. Candidate lists in
    // popups (CandidateList, Select, Cascader) keep focus in their host input and use their own forwarded keys.
    /// <summary>
    /// Handles the keyboard interaction of the list: directional keys move the focus to the next
    /// navigable row and select according to the selection mode, the select-all hotkey selects all rows
    /// only when the list is selectable, and Space or Enter on a row commits its selection. An override
    /// calls the base method for every key it does not handle.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key.ToNavigationDirection() is { } direction && direction.IsDirectional())
        {
            // Avalonia's ListBox.OnKeyDown moves the selection for directional keys without checking Handled, and
            // ItemsControl.OnKeyDown moves focus again; both are replaced here, so the base must not run for them.
            e.Handled = HandleNavigationKey(e, direction);
            return;
        }

        var hotkeys = this.GetPlatformSettings()?.HotkeyConfiguration;
        if (hotkeys is not null && SelectionMode.HasAllFlags(SelectionMode.Multiple) && MatchesAnyGesture(hotkeys.SelectAll, e))
        {
            // Avalonia would select all even when the list is not selectable.
            if (IsSelectable)
            {
                Selection.SelectAll();
                e.Handled = true;
            }
            return;
        }

        if (HandleCommitKey(e, hotkeys))
        {
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }

    private bool HandleNavigationKey(KeyEventArgs e, NavigationDirection direction)
    {
        if (e.KeyModifiers.HasAllFlags(KeyModifiers.Alt) || ItemsPanelRoot is not INavigableContainer panel)
        {
            return false;
        }

        var focused = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement();
        var from    = GetContainerFromEventSource(focused) ?? ContainerFromIndex(Selection.AnchorIndex);
        // ContainerFromIndex also returns items that are their own container while they are not realized.
        if (from is not null && IndexFromContainer(from) < 0)
        {
            from = null;
        }

        if (from is null)
        {
            direction = direction switch
            {
                NavigationDirection.Down or NavigationDirection.Right => NavigationDirection.First,
                NavigationDirection.Up or NavigationDirection.Left => NavigationDirection.Last,
                _ => direction
            };
        }

        // The virtualizing panel realizes (scrolls into view) the target; non-navigable items are skipped.
        // PageUp / PageDown find no target: Avalonia's VirtualizingStackPanel has no page navigation, they bubble
        // to the ScrollViewer, which scrolls the viewport and leaves the focus where it is.
        if (GetNextControl(panel, direction, from, WrapSelection) is not Control next ||
            IndexFromContainer(next) is not (>= 0 and var index))
        {
            return false;
        }

        var hotkeys = this.GetPlatformSettings()?.HotkeyConfiguration;
        var command = hotkeys is not null && e.KeyModifiers.HasAllFlags(hotkeys.CommandModifiers);
        if (IsSelectable && !command)
        {
            if (!SelectionMode.HasAllFlags(SelectionMode.Multiple))
            {
                // Single selection follows the focused item.
                UpdateSelection(index);
            }
            else if (e.KeyModifiers.HasAllFlags(KeyModifiers.Shift))
            {
                // Multiple selection: plain arrows only browse; Shift selects the range from the anchor.
                UpdateSelection(index, rangeModifier: true);
            }
        }

        next.Focus(NavigationMethod.Directional, e.KeyModifiers);
        return true;
    }

    private bool HandleCommitKey(KeyEventArgs e, PlatformHotkeyConfiguration? hotkeys)
    {
        if (!IsSelectable ||
            e.Key is not (Key.Space or Key.Enter) ||
            e.KeyModifiers.HasAllFlags(KeyModifiers.Alt) ||
            // Only on the item itself: focusable content inside an item keeps its own Space / Enter.
            e.Source is not Control source ||
            IndexFromContainer(source) is not (>= 0 and var index))
        {
            return false;
        }

        var multiple = SelectionMode.HasAllFlags(SelectionMode.Multiple);
        var command  = hotkeys is not null && e.KeyModifiers.HasAllFlags(hotkeys.CommandModifiers);
        if (!multiple && !command && Selection.IsSelected(index))
        {
            // Nothing to commit: the key keeps bubbling for application handlers.
            return false;
        }

        // Multiple: toggle the focused item (Shift: select the range from the anchor). Single: select it,
        // Ctrl / Cmd toggles it off.
        UpdateSelection(index,
            rangeModifier: e.KeyModifiers.HasAllFlags(KeyModifiers.Shift),
            toggleModifier: multiple || command);
        return true;
    }

    private static bool MatchesAnyGesture(IEnumerable<KeyGesture> gestures, KeyEventArgs e)
    {
        foreach (var gesture in gestures)
        {
            if (gesture.Matches(e))
            {
                return true;
            }
        }
        return false;
    }
    
    private void HandleItemCountChanged()
    {
        ItemCountChanged?.Invoke(this, new ItemCountChangedEventArgs(ItemCount));
    }

    #region 虚拟化上下文管理
    /// <summary>
    /// Prepares a row container for recycling: resets its transition initialization, saves its
    /// virtualizing context under its virtual index for the container that is later prepared for that
    /// position, clears the values the list set on it, and calls the base method. Motion is switched off
    /// on the container meanwhile, so that these changes do not animate. The method is sealed.
    /// </summary>
    protected sealed override void ClearContainerForItemOverride(Control element)
    {
        if (element is ITransitionInitializationOwner owner)
        {
            owner.TransitionInitialization.Reset();
        }

        var originMotionEnabled = false; 
        try
        {
            if (element is IMotionAwareControl motionAwareControl)
            {
                originMotionEnabled = motionAwareControl.IsMotionEnabled;
                element.SetCurrentValue(IsMotionEnabledProperty, false);
            }
        
            if (this is IListVirtualizingContextAware list && element is IListItemVirtualizingContextAware listItem)
            {
                var context = new Dictionary<object, object?>(1);
                list.SaveVirtualizingContext(element, context);
                _virtualRestoreContext[listItem.VirtualIndex] = context;
                list.ClearContainerValues(element);
            }

            base.ClearContainerForItemOverride(element);
        }
        finally
        {
            if (element is IMotionAwareControl)
            {
                element.SetCurrentValue(IsMotionEnabledProperty, originMotionEnabled);
            }
        }
    }
    
    /// <summary>
    /// Applies the default state of a data item to a row container: the data object becomes the content
    /// of the container and its enabled state becomes the enabled state of the container. It is called
    /// when a container is created or prepared for an <see cref="IListItemData" />. An override calls the
    /// base method to keep both values.
    /// </summary>
    protected virtual void NotifyRestoreDefaultContext(ListBoxItem item, IListItemData itemData)
    {
        item.SetCurrentValue(ListBoxItem.ContentProperty, itemData);
        item.SetCurrentValue(ListBoxItem.IsEnabledProperty, itemData.IsEnabled);
    }

    /// <summary>
    /// Clears the state a recycled row container keeps from its previous item: the filter visibility
    /// that was saved for it, the content, content template, enabled state and visibility set on the
    /// container, and its filter state. It is called when the container is recycled. An override calls
    /// the base method to keep this reset.
    /// </summary>
    protected virtual void NotifyClearContainerForVirtualizingContext(ListBoxItem item)
    {
        _filterContext.Remove(item);
        item.ClearValue(ListBoxItem.ContentProperty);
        item.ClearValue(ListBoxItem.ContentTemplateProperty);
        item.ClearValue(ListBoxItem.IsEnabledProperty);
        item.ClearValue(ListBoxItem.IsVisibleProperty);
        item.IsFiltering          = false;
        item.FilterValue          = null;
        item.FilterHighlightWords = null;
    }
    
    /// <summary>
    /// Stores in <c>context</c> what a recycled row container must restore when it is prepared again for
    /// the same position; the base method stores the enabled state of the container. It is called when
    /// the container is recycled. An override calls the base method first and adds its own entries.
    /// </summary>
    protected virtual void NotifySaveVirtualizingContext(ListBoxItem item, IDictionary<object, object?> context)
    {
        context.Add(ListBoxItem.IsEnabledProperty, item.IsEnabled);
    }

    /// <summary>
    /// Applies a context saved by <see cref="NotifySaveVirtualizingContext" /> to the row container that
    /// is prepared again for the position the context was saved for; the base method restores the enabled
    /// state. It runs after the default state of the item was applied, so the saved values win. An
    /// override calls the base method.
    /// </summary>
    protected virtual void NotifyRestoreVirtualizingContext(ListBoxItem item, IDictionary<object, object?> context)
    {
        {
            if (context.TryGetValue(ListBoxItem.IsEnabledProperty, out var value))
            {
                if (value is bool isEnabled)
                {
                    item.SetCurrentValue(ListBoxItem.IsEnabledProperty, isEnabled);
                }
            }
        }
    }

    void IListVirtualizingContextAware.SaveVirtualizingContext(Control item, IDictionary<object, object?> context)
    {
        if (item is ListBoxItem listBoxItem)
        {
            ListVirtualizingContextAwareUtils.ExecuteWithinContextClosure(listBoxItem, listItem => NotifySaveVirtualizingContext(listItem, context));
        }
    }

    void IListVirtualizingContextAware.RestoreVirtualizingContext(Control item, IDictionary<object, object?> context)
    {
        if (item is ListBoxItem listBoxItem)
        {
            ListVirtualizingContextAwareUtils.ExecuteWithinContextClosure(listBoxItem, listItem => NotifyRestoreVirtualizingContext(listItem, context));
        }
    }

    void IListVirtualizingContextAware.RestoreDefaultContext(Control item, object defaultContext)
    {
        if (item is ListBoxItem listBoxItem && defaultContext is IListItemData listBoxItemData)
        {
            ListVirtualizingContextAwareUtils.ExecuteWithinContextClosure(listBoxItem, listItem => NotifyRestoreDefaultContext(listItem, listBoxItemData));
        }
    }

    void IListVirtualizingContextAware.ClearContainerValues(Control item)
    {
        if (item is ListBoxItem listBoxItem)
        {
            ListVirtualizingContextAwareUtils.ExecuteWithinContextClosure(listBoxItem, NotifyClearContainerForVirtualizingContext);
        }
    }
    #endregion
}
