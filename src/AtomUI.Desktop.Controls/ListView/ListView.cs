using System.Collections;
using System.Collections.Specialized;
using AtomUI.Controls;
using AtomUI.Controls.Data;
using AtomUI.Controls.Utils;
using AtomUI.Generated.AtomUIDesktopControls;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Displays a scrollable list of data items in <see cref="ListViewItem" /> containers, with selection,
/// grouping, filtering, sorting and paging.
/// </summary>
/// <remarks>
/// An <see cref="IListCollectionView" /> set as the item source is used as it is. Any other enumerable is
/// wrapped in a collection view that the list owns and disposes when it is replaced. Every item must
/// implement <c>IListItemData</c>.
/// </remarks>
public partial class ListView : ItemsControl, ICustomizableSizeTypeAware, IMotionAwareControl
{
    #region 公共属性定义
    
    /// <summary>
    /// Identifies the <see cref="IsSelectable" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsSelectableProperty =
        AvaloniaProperty.Register<ListView, bool>(nameof(IsSelectable), true);
    
    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<ListView>();
    
    /// <summary>
    /// Identifies the <see cref="IsBorderless" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsBorderlessProperty =
        AvaloniaProperty.Register<ListView, bool>(nameof(IsBorderless), false);
        
    /// <summary>
    /// Identifies the <see cref="ItemHoverBg" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> ItemHoverBgProperty =
        AvaloniaProperty.Register<ListView, IBrush?>(nameof(ItemHoverBg));
    
    /// <summary>
    /// Identifies the <see cref="ItemSelectedBg" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> ItemSelectedBgProperty =
        AvaloniaProperty.Register<ListView, IBrush?>(nameof(ItemSelectedBg));
    
    /// <summary>
    /// Identifies the <see cref="IsShowSelectedIndicator" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowSelectedIndicatorProperty =
        AvaloniaProperty.Register<ListView, bool>(nameof(IsShowSelectedIndicator), false);
    
    /// <summary>
    /// Identifies the <see cref="SelectedIndicator" /> property.
    /// </summary>
    public static readonly StyledProperty<IconTemplate?> SelectedIndicatorProperty =
        AvaloniaProperty.Register<ListView, IconTemplate?>(nameof(SelectedIndicator));
    
    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<ListView>();
    
    /// <summary>
    /// Identifies the <see cref="EmptyIndicatorPadding" /> property.
    /// </summary>
    public static readonly StyledProperty<Thickness> EmptyIndicatorPaddingProperty =
        AvaloniaProperty.Register<ListView, Thickness>(nameof(EmptyIndicatorPadding));
    
    /// <summary>
    /// Identifies the <see cref="EmptyIndicator" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> EmptyIndicatorProperty =
        AvaloniaProperty.Register<ListView, object?>(nameof(EmptyIndicator));
    
    /// <summary>
    /// Identifies the <see cref="EmptyIndicatorTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> EmptyIndicatorTemplateProperty =
        AvaloniaProperty.Register<ListView, IDataTemplate?>(nameof(EmptyIndicatorTemplate));
    
    /// <summary>
    /// Identifies the <see cref="IsShowEmptyIndicator" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowEmptyIndicatorProperty =
        AvaloniaProperty.Register<ListView, bool>(nameof(IsShowEmptyIndicator), true);
    
    /// <summary>
    /// Identifies the <see cref="IsGroupEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsGroupEnabledProperty =
        AvaloniaProperty.Register<ListView, bool>(nameof(IsGroupEnabled), false);
    
    /// <summary>
    /// Identifies the <see cref="GroupPropertySelector" /> property.
    /// </summary>
    public static readonly StyledProperty<DefaultFilterValueSelector?> GroupPropertySelectorProperty =
        AvaloniaProperty.Register<ListView, DefaultFilterValueSelector?>(
            nameof(GroupPropertySelector));
    
    /// <summary>
    /// Identifies the <see cref="SortDescriptions" /> property.
    /// </summary>
    public static readonly StyledProperty<IList<IListSortDescription>?> SortDescriptionsProperty =
        AvaloniaProperty.Register<ListView, IList<IListSortDescription>?>(nameof(SortDescriptions));
    
    /// <summary>
    /// Identifies the <see cref="Filter" /> property.
    /// </summary>
    public static readonly StyledProperty<IValueFilter?> FilterProperty =
        AvaloniaProperty.Register<ListView, IValueFilter?>(nameof(Filter));
    
    /// <summary>
    /// Identifies the <see cref="FilterValue" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> FilterValueProperty =
        AvaloniaProperty.Register<ListView, object?>(nameof(FilterValue));
    
    /// <summary>
    /// Identifies the <see cref="FilterValueSelector" /> property.
    /// </summary>
    public static readonly StyledProperty<DefaultFilterValueSelector?> FilterValueSelectorProperty =
        AvaloniaProperty.Register<ListView, DefaultFilterValueSelector?>(
            nameof(FilterValueSelector));
    
    /// <summary>
    /// Identifies the <see cref="IsFiltering" /> property.
    /// </summary>
    public static readonly DirectProperty<ListView, bool> IsFilteringProperty =
        AvaloniaProperty.RegisterDirect<ListView, bool>(nameof(IsFiltering),
            o => o.IsFiltering);
    
    /// <summary>
    /// Identifies the <see cref="TotalItemCount" /> property.
    /// </summary>
    public static readonly DirectProperty<ListView, int> TotalItemCountProperty =
        AvaloniaProperty.RegisterDirect<ListView, int>(nameof(TotalItemCount), o => o.TotalItemCount);
    
    /// <summary>
    /// Identifies the <see cref="GroupItemTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> GroupItemTemplateProperty =
        AvaloniaProperty.Register<ListView, IDataTemplate?>(nameof(GroupItemTemplate));
    
    /// <summary>
    /// Identifies the <see cref="IsOperating" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsOperatingProperty =
        AvaloniaProperty.Register<ListView, bool>(nameof(IsOperating));
    
    /// <summary>
    /// Identifies the <see cref="OperatingMsg" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> OperatingMsgProperty =
        AvaloniaProperty.Register<ListView, string?>(nameof(OperatingMsg));
    
    /// <summary>
    /// Identifies the <see cref="CustomOperatingIndicator" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> CustomOperatingIndicatorProperty =
        AvaloniaProperty.Register<ListView, object?>(nameof(CustomOperatingIndicator));

    /// <summary>
    /// Identifies the <see cref="CustomOperatingIndicatorTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> CustomOperatingIndicatorTemplateProperty =
        AvaloniaProperty.Register<ListView, IDataTemplate?>(nameof(CustomOperatingIndicatorTemplate));
    
    /// <summary>
    /// Identifies the <see cref="TopPagination" /> property.
    /// </summary>
    public static readonly StyledProperty<AbstractPagination?> TopPaginationProperty =
        AvaloniaProperty.Register<ListView, AbstractPagination?>(nameof(TopPagination));
    
    /// <summary>
    /// Identifies the <see cref="BottomPagination" /> property.
    /// </summary>
    public static readonly StyledProperty<AbstractPagination?> BottomPaginationProperty =
        AvaloniaProperty.Register<ListView, AbstractPagination?>(nameof(BottomPagination));
    
    /// <summary>
    /// Identifies the <see cref="IsHideOnSinglePage" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsHideOnSinglePageProperty =
        AbstractPagination.IsHideOnSinglePageProperty.AddOwner<ListView>();
    
    /// <summary>
    /// Identifies the <see cref="PaginationVisibility" /> property.
    /// </summary>
    public static readonly StyledProperty<ListPaginationVisibility> PaginationVisibilityProperty =
        AvaloniaProperty.Register<ListView, ListPaginationVisibility>(nameof(PaginationVisibility), ListPaginationVisibility.Bottom);
    
    /// <summary>
    /// Identifies the <see cref="TopPaginationAlign" /> property.
    /// </summary>
    public static readonly StyledProperty<PaginationAlign> TopPaginationAlignProperty =
        AvaloniaProperty.Register<ListView, PaginationAlign>(nameof(TopPaginationAlign), PaginationAlign.End);
    
    /// <summary>
    /// Identifies the <see cref="BottomPaginationAlign" /> property.
    /// </summary>
    public static readonly StyledProperty<PaginationAlign> BottomPaginationAlignProperty =
        AvaloniaProperty.Register<ListView, PaginationAlign>(nameof(BottomPaginationAlign), PaginationAlign.End);
    
    /// <summary>
    /// Identifies the <see cref="ItemClickMode" /> property.
    /// </summary>
    public static readonly StyledProperty<ClickMode> ItemClickModeProperty =
        AvaloniaProperty.Register<ListView, ClickMode>(nameof(ItemClickMode));
    
    /// <summary>
    /// Gets or sets whether the user can select item rows with the pointer and the keyboard.
    /// It is <see langword="true" /> by default; setting it to <see langword="false" /> clears the selection.
    /// </summary>
    public bool IsSelectable
    {
        get => GetValue(IsSelectableProperty);
        set => SetValue(IsSelectableProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the size step of the list; the default is <see cref="CustomizableSizeType.Middle" />.
    /// The theme derives the item padding, the corner radius of the list frame and
    /// <see cref="EmptyIndicatorPadding" /> from it, and it is forwarded to every item container.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the background of a hovered item row. Group header rows do not use it.
    /// When it is cleared, the theme supplies the list's <c>ItemHoverBgColor</c>.
    /// <see langword="null" /> paints no background.
    /// </summary>
    public IBrush? ItemHoverBg
    {
        get => GetValue(ItemHoverBgProperty);
        set => SetValue(ItemHoverBgProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the background of a selected, enabled item row. Group header rows do not use it.
    /// When it is cleared, the theme supplies the list's <c>ItemSelectedBgColor</c>.
    /// <see langword="null" /> paints no background.
    /// </summary>
    public IBrush? ItemSelectedBg
    {
        get => GetValue(ItemSelectedBgProperty);
        set => SetValue(ItemSelectedBgProperty, value);
    }
    
    /// <summary>
    /// Gets or sets whether the list is drawn without the border set by <c>BorderThickness</c>.
    /// A borderless list keeps the split line under the last item row; a bordered list drops it there unless
    /// <see cref="BottomPagination" /> is set.
    /// </summary>
    public bool IsBorderless
    {
        get => GetValue(IsBorderlessProperty);
        set => SetValue(IsBorderlessProperty, value);
    }
    
    /// <summary>
    /// Gets or sets whether item rows animate their state changes. The theme follows the global
    /// <c>EnableMotion</c> token, and the value is forwarded to the item containers and the pagers.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    /// <summary>
    /// Gets or sets whether item rows show <see cref="SelectedIndicator" />. It is <see langword="false" /> by
    /// default, and the value is forwarded to every item container.
    /// </summary>
    public bool IsShowSelectedIndicator
    {
        get => GetValue(IsShowSelectedIndicatorProperty);
        set => SetValue(IsShowSelectedIndicatorProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the icon of the selected indicator of an item row, which is used while
    /// <see cref="IsShowSelectedIndicator" /> is <see langword="true" />. The theme supplies a check icon,
    /// and the value is forwarded to every item container.
    /// </summary>
    public IconTemplate? SelectedIndicator
    {
        get => GetValue(SelectedIndicatorProperty);
        set => SetValue(SelectedIndicatorProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the padding around the empty indicator. The theme derives it from
    /// <see cref="SizeType" />: the large size uses a wider padding than the other sizes.
    /// </summary>
    public Thickness EmptyIndicatorPadding
    {
        get => GetValue(EmptyIndicatorPaddingProperty);
        set => SetValue(EmptyIndicatorPaddingProperty, value);
    }
        
    /// <summary>
    /// Gets or sets the content shown when the list is empty and <see cref="IsShowEmptyIndicator" /> is
    /// <see langword="true" />. The theme's default empty indicator is replaced as soon as this property or
    /// <see cref="EmptyIndicatorTemplate" /> is set.
    /// </summary>
    [DependsOn(nameof(EmptyIndicatorTemplate))]
    public object? EmptyIndicator
    {
        get => GetValue(EmptyIndicatorProperty);
        set => SetValue(EmptyIndicatorProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the template that builds the custom empty indicator from <see cref="EmptyIndicator" />.
    /// Setting it replaces the theme's default empty indicator.
    /// </summary>
    public IDataTemplate? EmptyIndicatorTemplate
    {
        get => GetValue(EmptyIndicatorTemplateProperty);
        set => SetValue(EmptyIndicatorTemplateProperty, value);
    }
    
    /// <summary>
    /// Gets or sets whether an empty indicator replaces the item area while <see cref="TotalItemCount" /> is 0.
    /// It is <see langword="true" /> by default; with <see langword="false" /> the item area stays empty.
    /// </summary>
    public bool IsShowEmptyIndicator
    {
        get => GetValue(IsShowEmptyIndicatorProperty);
        set => SetValue(IsShowEmptyIndicatorProperty, value);
    }
    
    /// <summary>
    /// Gets or sets whether the items are grouped, with a header row in front of each group.
    /// The group key comes from <see cref="GroupPropertySelector" />.
    /// </summary>
    public bool IsGroupEnabled
    {
        get => GetValue(IsGroupEnabledProperty);
        set => SetValue(IsGroupEnabledProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the function that returns the group key of an item while <see cref="IsGroupEnabled" /> is
    /// <see langword="true" />; items with the same key form one group. It is applied when the item source
    /// changes, when grouping is switched on and when the selector itself is replaced. Unless one is set,
    /// the list uses the <c>Group</c> of an <c>IGroupHeader</c> item.
    /// </summary>
    public DefaultFilterValueSelector? GroupPropertySelector
    {
        get => GetValue(GroupPropertySelectorProperty);
        set => SetValue(GroupPropertySelectorProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the sort descriptions that order the items, applied in list order.
    /// <see langword="null" /> leaves the items unsorted. The entries are copied when this property or the
    /// item source changes; edits to the assigned list take effect only then.
    /// </summary>
    public IList<IListSortDescription>? SortDescriptions
    {
        get => GetValue(SortDescriptionsProperty);
        set => SetValue(SortDescriptionsProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the filter that decides which items stay visible while <see cref="FilterValue" /> is set.
    /// It receives the value that <see cref="FilterValueSelector" /> reads from an item and hides the item
    /// when it returns <see langword="false" />. Filtering needs both this property and <see cref="FilterValue" />.
    /// </summary>
    public IValueFilter? Filter
    {
        get => GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }

    /// <summary>
    /// Gets or sets the value the items are filtered by while <see cref="Filter" /> is set.
    /// <see langword="null" /> turns filtering off.
    /// </summary>
    public object? FilterValue
    {
        get => GetValue(FilterValueProperty);
        set => SetValue(FilterValueProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the function that reads the value of an item which <see cref="Filter" /> receives.
    /// Unless one is set, the <c>Content</c> of an <c>IListItemData</c> item is used.
    /// </summary>
    public DefaultFilterValueSelector? FilterValueSelector
    {
        get => GetValue(FilterValueSelectorProperty);
        set => SetValue(FilterValueSelectorProperty, value);
    }
    
    private bool _isFiltering;
    
    /// <summary>
    /// Gets whether the items are currently filtered, which requires both <see cref="Filter" /> and
    /// <see cref="FilterValue" /> to be set.
    /// </summary>
    public bool IsFiltering
    {
        get => _isFiltering;
        private set => SetAndRaise(IsFilteringProperty, ref _isFiltering, value);
    }
    
    private int _totalItemCount;
    /// <summary>
    /// Gets the number of items the collection view holds, counted over all pages.
    /// A change raises <see cref="ItemCountChanged" /> and updates the <c>:empty</c> and <c>:singleitem</c>
    /// pseudo-classes of the list.
    /// </summary>
    public int TotalItemCount
    {
        get => _totalItemCount;
        private set
        {
            if (SetAndRaise(TotalItemCountProperty, ref _totalItemCount, value))
            {
                UpdatePseudoClasses();
            }
        }
    }
    
    /// <summary>
    /// Gets or sets the template that builds the header row of a group. The theme supplies a default template
    /// that shows the <c>Content</c> of the group header item.
    /// </summary>
    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public IDataTemplate? GroupItemTemplate
    {
        get => GetValue(GroupItemTemplateProperty);
        set => SetValue(GroupItemTemplateProperty, value);
    }
    
    /// <summary>
    /// Gets or sets whether the operating indicator spins over the list content, which signals a running
    /// operation. It is <see langword="false" /> by default.
    /// </summary>
    public bool IsOperating
    {
        get => GetValue(IsOperatingProperty);
        set => SetValue(IsOperatingProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the tip text shown with the operating indicator while <see cref="IsOperating" /> is
    /// <see langword="true" />.
    /// </summary>
    public string? OperatingMsg
    {
        get => GetValue(OperatingMsgProperty);
        set => SetValue(OperatingMsgProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the content that replaces the default spinning indicator while <see cref="IsOperating" />
    /// is <see langword="true" />.
    /// </summary>
    [DependsOn(nameof(CustomOperatingIndicatorTemplate))]
    public object? CustomOperatingIndicator
    {
        get => GetValue(CustomOperatingIndicatorProperty);
        set => SetValue(CustomOperatingIndicatorProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the template that builds the custom operating indicator from
    /// <see cref="CustomOperatingIndicator" />.
    /// </summary>
    public IDataTemplate? CustomOperatingIndicatorTemplate
    {
        get => GetValue(CustomOperatingIndicatorTemplateProperty);
        set => SetValue(CustomOperatingIndicatorTemplateProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the pager shown above the items, or <see langword="null" /> to show none.
    /// The list keeps the pager in step with the paged collection view and pages the view when the pager
    /// requests another page.
    /// </summary>
    public AbstractPagination? TopPagination
    {
        get => GetValue(TopPaginationProperty);
        set => SetValue(TopPaginationProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the pager shown below the items, or <see langword="null" /> to show none.
    /// The list keeps the pager in step with the paged collection view and pages the view when the pager
    /// requests another page.
    /// </summary>
    public AbstractPagination? BottomPagination
    {
        get => GetValue(BottomPaginationProperty);
        set => SetValue(BottomPaginationProperty, value);
    }
    
    /// <summary>
    /// Gets or sets whether the pagers hide their content while the items fit on one page.
    /// The value is relayed to <see cref="TopPagination" /> and <see cref="BottomPagination" />;
    /// it is <see langword="false" /> by default.
    /// </summary>
    public bool IsHideOnSinglePage
    {
        get => GetValue(IsHideOnSinglePageProperty);
        set => SetValue(IsHideOnSinglePageProperty, value);
    }
    
    /// <summary>
    /// Gets or sets which pagers are visible; the default is <see cref="ListPaginationVisibility.Bottom" />.
    /// A pager stays hidden while the value excludes its side.
    /// </summary>
    public ListPaginationVisibility PaginationVisibility
    {
        get => GetValue(PaginationVisibilityProperty);
        set => SetValue(PaginationVisibilityProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the horizontal alignment of <see cref="TopPagination" />,
    /// <see cref="PaginationAlign.End" /> by default. The value is relayed to the pager.
    /// </summary>
    public PaginationAlign TopPaginationAlign
    {
        get => GetValue(TopPaginationAlignProperty);
        set => SetValue(TopPaginationAlignProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the horizontal alignment of <see cref="BottomPagination" />,
    /// <see cref="PaginationAlign.End" /> by default. The value is relayed to the pager.
    /// </summary>
    public PaginationAlign BottomPaginationAlign
    {
        get => GetValue(BottomPaginationAlignProperty);
        set => SetValue(BottomPaginationAlignProperty, value);
    }

    /// <summary>
    /// Gets or sets whether an item row is clicked on the press or on the release of the left button;
    /// the default is <see cref="ClickMode.Release" />. The value is forwarded to every item container.
    /// </summary>
    public ClickMode ItemClickMode
    {
        get => GetValue(ItemClickModeProperty);
        set => SetValue(ItemClickModeProperty, value);
    }
    #endregion
    
    #region 公共事件定义

    /// <summary>
    /// Raised when an item row is clicked, which happens on the press or on the release of the left button as
    /// <see cref="ItemClickMode" /> selects. It is raised after <see cref="NotifyItemClicked" /> has run.
    /// </summary>
    public event EventHandler<ListViewItemClickedEventArgs>? ItemClicked;
    /// <summary>
    /// Raised after <see cref="TotalItemCount" /> changed; the event data carries the new count.
    /// </summary>
    public event EventHandler<ItemCountChangedEventArgs>? ItemCountChanged;
    /// <summary>
    /// Raised after the filter description of the collection view was rebuilt, which happens when the item
    /// source, <see cref="Filter" />, <see cref="FilterValue" /> or <see cref="FilterValueSelector" /> changes.
    /// </summary>
    public event EventHandler? FilterContextChanged;

    #endregion
    
    #region 内部属性定义
    internal static readonly DirectProperty<ListView, bool> IsEmptyDataSourceProperty =
        AvaloniaProperty.RegisterDirect<ListView, bool>(
            nameof(IsEmptyDataSource),
            o => o.IsEmptyDataSource,
            (o, v) => o.IsEmptyDataSource = v);

    internal static readonly DirectProperty<ListView, Thickness> EffectiveBorderThicknessProperty =
        AvaloniaProperty.RegisterDirect<ListView, Thickness>(nameof(EffectiveBorderThickness),
            o => o.EffectiveBorderThickness,
            (o, v) => o.EffectiveBorderThickness = v);
    
    internal static readonly DirectProperty<ListView, bool> IsEffectiveEmptyVisibleProperty =
        AvaloniaProperty.RegisterDirect<ListView, bool>(
            nameof(IsEffectiveEmptyVisible),
            o => o.IsEffectiveEmptyVisible,
            (o, v) => o.IsEffectiveEmptyVisible = v);

    internal static readonly DirectProperty<ListView, bool> IsDefaultEmptyIndicatorVisibleProperty =
        AvaloniaProperty.RegisterDirect<ListView, bool>(
            nameof(IsDefaultEmptyIndicatorVisible),
            o => o.IsDefaultEmptyIndicatorVisible,
            (o, v) => o.IsDefaultEmptyIndicatorVisible = v);
    
    private bool _isEmptyDataSource = true;
    internal bool IsEmptyDataSource
    {
        get => _isEmptyDataSource;
        set => SetAndRaise(IsEmptyDataSourceProperty, ref _isEmptyDataSource, value);
    }
    
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

    #endregion
    
    private bool _areHandlersSuspended;
    private static readonly object GroupHeaderRecycleKey = new();
    private static readonly FuncTemplate<Panel?> DefaultPanel =
        new(() => new VirtualizingStackPanel());

    private IListCollectionView? _collectionView;
    /// <summary>
    /// Indicates whether _collectionView was created by ListView (and should be disposed by it).
    /// </summary>
    private bool _ownsCollectionView;
    private IListCollectionView? _defaultFilterOwner;
    private Func<object, bool>? _defaultFilterCallback;
    
    static ListView()
    {
        ItemsPanelProperty.OverrideDefaultValue<ListView>(DefaultPanel);
        // One Tab stop: Tab enters on the last focused (or selected) item, arrow keys move between items.
        KeyboardNavigation.TabNavigationProperty.OverrideDefaultValue<ListView>(KeyboardNavigationMode.Once);
        IsSelectedChangedEvent.AddClassHandler<ListView>((list, e) => list.ContainerSelectionChanged(e));
        IsSelectableProperty.Changed.AddClassHandler<ListView>((list, args) => list.HandleIsSelectableChanged(args));
        TotalItemCountProperty.Changed.AddClassHandler<ListView>((list, args) => list.HandleItemCountChanged());
        ItemsSourceProperty.Changed.AddClassHandler<ListView>((list, e) => list.HandleItemsSourcePropertyChanged(e));
    }

    internal static void RegisterItemClickedEvent(RoutedEvent<RoutedEventArgs> clickedEvent)
    {
        clickedEvent.AddClassHandler<ListView>((list, args) => list.HandleListViewItemClicked(args));
    }
    
    public ListView()
    {
        // Selecting setup. Subscribing to item source changes currently works only through reflection, which
        // AddSourceChangedEvent does internally; the handler re-attaches the selection view when the source of
        // the item collection is replaced.
        ((ItemCollection)ItemsView).AddSourceChangedEvent(OnItemsViewSourceChanged);
        _selection.Changed += (_, change) => HandleSelectionChange(change);
        var items = Items;
        items.CollectionChanged += HandleItemsViewCollectionChanged;
        Items.CollectionChanged += HandleItemCollectionChanged;
    }

    private void OnItemsViewSourceChanged(object? sender, EventArgs e)
    {
        AttachSelectionView();
    }
    
    private void HandleItemsSourcePropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (!_areHandlersSuspended)
        {
            var                  oldCollectionView = change.OldValue as ListCollectionView;
            var                  newItemsSource    = (IEnumerable?)change.NewValue;
            IListCollectionView? newCollectionView = null;
            bool                 ownsNew           = false;
            if (newItemsSource is IListCollectionView)
            {
                newCollectionView  = (IListCollectionView)newItemsSource;
            }
            else
            {
                if (newItemsSource != null)
                {
                    newCollectionView = new ListCollectionView(newItemsSource);
                    ownsNew           = true;
                }
            }
            if (oldCollectionView != null)
            {
                oldCollectionView.PropertyChanged   -= HandleCollectionPropertyChanged;
                oldCollectionView.CollectionChanged -= HandleCollectionViewChanged;
                oldCollectionView.PageChanging      -= HandlePageChanging;
                oldCollectionView.PageChanged       -= HandlePageChanged;
                ClearDefaultFilterCallback(oldCollectionView);
                // Dispose only if we created this view (not user-provided)
                if (_ownsCollectionView)
                {
                    oldCollectionView.Dispose();
                }
            }
            if (newCollectionView != null)
            {
                newCollectionView.PropertyChanged   += HandleCollectionPropertyChanged;
                newCollectionView.CollectionChanged += HandleCollectionViewChanged;
                newCollectionView.PageChanging      += HandlePageChanging;
                newCollectionView.PageChanged       += HandlePageChanged;
                IsEmptyDataSource                   =  newCollectionView.IsEmpty;
                TotalItemCount                      =  newCollectionView.TotalItemCount;
            }
            else
            {
                IsEmptyDataSource = true;
            }

            _collectionView     = newCollectionView;
            _ownsCollectionView = ownsNew;
            SetValueNoCallback(ItemsSourceProperty, newCollectionView);
            ConfigureFilterDescription();
            ConfigureSortDescriptions();
            ConfigureGroupInfo();
            ReConfigurePagination();
            AttachSelectionView();
            InvalidateMeasure();
            UpdatePseudoClasses();
        }
    }
    
    private void SetValueNoCallback<T>(AvaloniaProperty<T> property, T value,
                                       BindingPriority priority = BindingPriority.LocalValue)
    {
        _areHandlersSuspended = true;
        try
        {
            SetValue(property, value, priority);
        }
        finally
        {
            _areHandlersSuspended = false;
        }
    }

    private void HandleCollectionViewChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (sender is IListCollectionView view)
        {
            IsEmptyDataSource = view.IsEmpty;
            TotalItemCount    = view.TotalItemCount;
            SyncPaginationState();
        }
    }
    
    private void ConfigureGroupInfo()
    {
        if (ItemsSource is IListCollectionView collectionView)
        {
            if (IsGroupEnabled)
            {
                if (GroupPropertySelector != null)
                {
                    collectionView.GroupDescriptions.Add(new ListGroupDescription(GroupPropertySelector));
                }
            }
            else
            {
                collectionView.GroupDescriptions.Clear();
            }
        }
    }
    
    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(ListPseudoClass.Empty, TotalItemCount == 0);
        PseudoClasses.Set(ListPseudoClass.SingleItem, TotalItemCount == 1);
    }
    
    /// <summary>
    /// Supplies the default group selector when none is set, then refreshes the empty indicator, the
    /// filtering state and the selection view. The base member is called first.
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (GroupPropertySelector == null)
        {
            SetCurrentValue(GroupPropertySelectorProperty, GroupSelector);
        }
        ConfigureEmptyIndicator();
        ConfigureIsFiltering();
        AttachSelectionView();
    }
    
    private object? GroupSelector(object? item)
    {
        if (item is IGroupHeader groupHeader)
        {
            return groupHeader.Group;
        }

        return null;
    }

    private void HandleItemCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        _virtualRestoreContext.Clear();
        ConfigureEmptyIndicator();
        RefreshContainerSplitLines();
        foreach (var item in Items)
        {
            if (item is not IListItemData)
            {
                throw new Exception("Unexpected item type, must implement IListItemData.");
            }
        }
    }

    /// <summary>
    /// Reacts to property changes by rebuilding the affected state: the empty indicator, the effective border
    /// thickness and the split lines, the filter description, the sort descriptions and the grouping of the
    /// collection view, then the selection and the pagers.
    /// </summary>
    /// <remarks>
    /// A derived override must call this member, otherwise the list stops reacting to the properties it handles.
    /// </remarks>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ItemsSourceProperty ||
            change.Property == IsFilteringProperty ||
            change.Property == IsShowEmptyIndicatorProperty ||
            change.Property == IsEmptyDataSourceProperty ||
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
        else if (change.Property == BottomPaginationProperty)
        {
            RefreshContainerSplitLines();
        }
        else if (change.Property == FilterValueProperty ||
                 change.Property == FilterProperty ||
                 change.Property == FilterValueSelectorProperty)
        {
            ConfigureIsFiltering();
            ConfigureFilterDescription();
        }
        else if (change.Property == SortDescriptionsProperty)
        {
            ConfigureSortDescriptions();
        }
        else if (change.Property == IsGroupEnabledProperty)
        {
            if (_collectionView != null)
            {
                using (_collectionView.DeferRefresh())
                {
                    ConfigureGroupInfo();
                }
                AttachSelectionView();
            }
        }
        else if (change.Property == GroupPropertySelectorProperty)
        {
            ReConfigureGroupInfo();
        }

        HandlePropertyChangedForSelecting(change);
        HandlePropertyChangedForPagination(change);
    }

    private void HandleIsSelectableChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is bool && (bool)e.NewValue == false)
        {
            Selection.Clear();
        }
    }
    
    private void ConfigureIsFiltering()
    {
        IsFiltering = Filter != null && FilterValue != null;
    }
    
    /// <summary>
    /// Creates the container for an item: a <c>GroupHeaderItem</c> for a group header entry and a
    /// <see cref="ListViewItem" /> otherwise, each marked with its semantic part class, and announces it
    /// through <see cref="NotifyContainerForItemCreated" />.
    /// </summary>
    /// <param name="item">
    /// The item or group header entry to create a container for.
    /// </param>
    /// <param name="index">
    /// The index of the item in the item collection.
    /// </param>
    /// <param name="recycleKey">
    /// Not used; a new container is created on every call.
    /// </param>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        ListViewItem listItem;
        if (IsGroupDataItem(item))
        {
            listItem = new GroupHeaderItem();
            listItem.Classes.Add(ListViewSemanticParts.GroupHeaderClass);
        }
        else
        {
            listItem = new ListViewItem();
            listItem.Classes.Add(ListViewSemanticParts.ItemClass);
        }

        NotifyContainerForItemCreated(listItem, item);
        return listItem;
    }

    private static bool IsGroupDataItem(object? item)
    {
        return item is IGroupListItemData { IsGroupItem: true };
    }
    
    /// <summary>
    /// Called for each newly created container before it is returned. For a <see cref="ListViewItem" />
    /// whose item is data that implements <c>IListItemData</c>, it restores the default context of the
    /// container from the item data.
    /// </summary>
    /// <param name="container">
    /// The container that was created.
    /// </param>
    /// <param name="item">
    /// The item the container was created for.
    /// </param>
    protected virtual void NotifyContainerForItemCreated(Control container, object? item)
    {
        if (container is ListViewItem listItem && item != null && item is not Visual)
        {
            if (item is IListItemData itemData)
            {
                NotifyRestoreDefaultContext(listItem, itemData);
            }
        }
    }

    /// <summary>
    /// Decides whether the item needs a container that this list creates. A group header entry always does
    /// and gets a recycle key of its own, so a header container is never reused for an item row or the
    /// other way round.
    /// </summary>
    /// <param name="item">
    /// The item to check.
    /// </param>
    /// <param name="index">
    /// The index of the item in the item collection.
    /// </param>
    /// <param name="recycleKey">
    /// Receives a key of its own for a group header entry, or otherwise the key of the item's own container.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the list must create the container; <see langword="false" /> when the
    /// item is itself a <see cref="ListViewItem" />.
    /// </returns>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        if (IsGroupDataItem(item))
        {
            // Group headers own a dedicated recycle key so they can never be
            // recycled as regular item containers (and vice versa).
            recycleKey = GroupHeaderRecycleKey;
            return true;
        }

        return NeedsContainer<ListViewItem>(item, out recycleKey);
    }
    
    /// <summary>
    /// Prepares an item container for its item: it creates the selection model, lets the base member prepare
    /// the container, sets the split-line visibility, binds the content template, forwards the item properties
    /// and theme of the list to the container, and restores the item's default and virtualizing contexts.
    /// </summary>
    /// <param name="container">
    /// The container to prepare. It must be a <see cref="ListViewItem" />.
    /// </param>
    /// <param name="item">
    /// The item the container displays.
    /// </param>
    /// <param name="index">
    /// The index of the item in the item collection.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="container" /> is not a <see cref="ListViewItem" />.
    /// </exception>
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        // Ensure that the selection model is created at this point so that accessing it in 
        // ContainerForItemPreparedOverride doesn't cause it to be initialized (which can
        // make containers become deselected when they're synced with the empty selection
        // mode).
        GetOrCreateSelectionModel();
        
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is ListViewItem listItem)
        {
            listItem.IsSplitLineVisible = ShouldShowSplitLine(index == ItemCount - 1);
            if (item is IGroupListItemData groupListItemData)
            {
                listItem.IsGroupItem = groupListItemData.IsGroupItem;
                var isGroupEnabled = listItem.IsGroupItem;
                if (GroupItemTemplate != null && isGroupEnabled)
                {
                    listItem[!ListViewItem.ContentTemplateProperty] = this[!GroupItemTemplateProperty];
                }
                else if (ItemTemplate != null)
                {
                    listItem[!ListViewItem.ContentTemplateProperty] = this[!ItemTemplateProperty];
                }
                else
                {
                    listItem.ClearValue(ListViewItem.ContentTemplateProperty);
                }
            }
            else
            {
                if (ItemTemplate != null)
                {
                    listItem[!ListViewItem.ContentTemplateProperty] = this[!ItemTemplateProperty];
                }
                else
                {
                    listItem.ClearValue(ListViewItem.ContentTemplateProperty);
                }
            }
            
            listItem[!ListViewItem.SizeTypeProperty]                = this[!SizeTypeProperty];
            listItem[!ListViewItem.IsMotionEnabledProperty]         = this[!IsMotionEnabledProperty];
            listItem[!ListViewItem.SelectedIndicatorProperty]       = this[!SelectedIndicatorProperty];
            listItem[!ListViewItem.ItemHoverBgProperty]             = this[!ItemHoverBgProperty];
            listItem[!ListViewItem.ItemSelectedBgProperty]          = this[!ItemSelectedBgProperty];
            listItem[!ListViewItem.IsShowSelectedIndicatorProperty] = this[!IsShowSelectedIndicatorProperty];
            listItem[!ListViewItem.ItemClickModeProperty]           = this[!ItemClickModeProperty];
            ForwardItemTheme(this, listItem);
            
            PrepareListViewItem(listItem, item, index);

            var originMotionEnabled = false;
            try
            {
                originMotionEnabled = listItem.IsMotionEnabled;
                listItem.SetCurrentValue(IsMotionEnabledProperty, false);
                if (item is IListItemData itemData)
                {
                    NotifyRestoreDefaultContext(listItem, itemData);
                }

                if (this is IListVirtualizingContextAware listVirtualizingContextAwareControl &&
                    listItem is IListItemVirtualizingContextAware virtualListItem)
                {
                    virtualListItem.VirtualIndex = index;
                    if (_virtualRestoreContext.TryGetValue(index, out var context))
                    {
                        listVirtualizingContextAwareControl.RestoreVirtualizingContext(listItem, context);
                        _virtualRestoreContext.Remove(index);
                    }
                }
            }
            finally
            {
                listItem.SetCurrentValue(IsMotionEnabledProperty, originMotionEnabled);
            }
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type ListViewItem.");
        }
    }
    
    /// <summary>
    /// Called for an item container after the list forwarded its item properties and theme to it and before
    /// the item context is restored. The base implementation does nothing.
    /// </summary>
    /// <param name="listItem">
    /// The container being prepared.
    /// </param>
    /// <param name="item">
    /// The item the container displays.
    /// </param>
    /// <param name="index">
    /// The index of the item in the item collection.
    /// </param>
    protected virtual void PrepareListViewItem(ListViewItem listItem, object? item, int index)
    {
    }
    
    /// <summary>
    /// Recomputes whether the empty indicator area is visible and whether it shows the theme's default
    /// indicator rather than custom content. It runs on initialization, on template application and whenever
    /// one of its inputs changes. A derived override must call the base member; the template binds to the two
    /// flags it computes.
    /// </summary>
    protected virtual void ConfigureEmptyIndicator()
    {
        IsEffectiveEmptyVisible        = IsShowEmptyIndicator && TotalItemCount == 0;
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
        if (IsBorderless)
        {
            return true;
        }

        if (!isLastItem)
        {
            return true;
        }

        return BottomPagination is not null;
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
            if (child is not ListViewItem listItem)
            {
                continue;
            }

            listItem.IsSplitLineVisible = ShouldShowSplitLine(IsLastItem(listItem));
        }
    }
    
    /// <summary>
    /// Updates the selection from a pointer event on an item container: Shift selects the range from the
    /// anchor, the command modifier toggles, and a pressed right button is passed on to the selection update.
    /// The item containers call it on a pointer press or release.
    /// </summary>
    /// <param name="source">
    /// The item container that received the pointer event.
    /// </param>
    /// <param name="e">
    /// The pointer event.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the selection was updated and the caller should mark the event handled;
    /// <see langword="false" /> when the list is not selectable or the source is not an item row.
    /// </returns>
    protected internal virtual bool UpdateSelectionFromPointerEvent(Control source, PointerEventArgs e)
    {
        if (IsSelectable)
        {
            // TODO: read the hotkey configuration from TopLevel.PlatformSettings instead of the application-wide
            // settings once the tests that select items with the pointer host the ListView in a TopLevel.
            var hotkeys = Application.Current!.PlatformSettings?.HotkeyConfiguration;
            var toggle  = hotkeys is not null && e.KeyModifiers.HasAllFlags(hotkeys.CommandModifiers);

            return UpdateSelectionFromEventSource(
                source,
                true,
                e.KeyModifiers.HasAllFlags(KeyModifiers.Shift),
                toggle,
                e.GetCurrentPoint(source).Properties.IsRightButtonPressed);
        }
        return false;
    }
    
    private void HandleListViewItemClicked(RoutedEventArgs args)
    {
        if (args.Source is ListViewItem item)
        {
            NotifyItemClicked(item);
            ItemClicked?.Invoke(this, new ListViewItemClickedEventArgs(item));
        }
    }
    
    /// <summary>
    /// Called before <see cref="ItemClicked" /> is raised, so that a derived list can react to the click
    /// first. The base implementation does nothing.
    /// </summary>
    /// <param name="item">
    /// The item container that was clicked.
    /// </param>
    protected internal virtual void NotifyItemClicked(ListViewItem item)
    {
    }
    
    // Keyboard model: the item containers are the focus targets and the list is one Tab stop, the same model as
    // ListBox. Directional keys move focus to the next navigable item (enabled, visible, focusable; group headers
    // are not focusable); the selection follows only in single selection mode. Select's popup candidate list keeps
    // focus in the Select and uses its own forwarded keys.
    /// <summary>
    /// Handles the directional, select-all and commit keys of the keyboard model. A directional key, a
    /// matched select-all hotkey and a handled commit key do not reach the base member; every other key does.
    /// </summary>
    /// <param name="e">
    /// The key event.
    /// </param>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key.ToNavigationDirection() is { } direction && direction.IsDirectional())
        {
            // ItemsControl.OnKeyDown would only repeat the focus move, so it is not called for directional keys.
            e.Handled = HandleNavigationKey(e, direction);
            return;
        }

        var hotkeys = this.GetPlatformSettings()?.HotkeyConfiguration;
        if (hotkeys is not null && SelectionMode.HasAllFlags(SelectionMode.Multiple) && MatchesSelectAllHotkey(hotkeys.SelectAll, e))
        {
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
        var from    = GetContainerFromEventSource(focused);
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
        // PageUp / PageDown find no target (Avalonia's VirtualizingStackPanel has no page navigation): they bubble
        // to the ScrollViewer, which scrolls the viewport and leaves the focus where it is.
        if (GetNextControl(panel, direction, from, WrapSelection) is not Control next)
        {
            return false;
        }

        var hotkeys = this.GetPlatformSettings()?.HotkeyConfiguration;
        var command = hotkeys is not null && e.KeyModifiers.HasAllFlags(hotkeys.CommandModifiers);
        if (IsSelectable && !command && TryGetSourceIndexFromContainer(next, out var index))
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
            e.Source is not ListViewItem { IsGroupItem: false } source ||
            !ReferenceEquals(source.Parent, this) ||
            !TryGetSourceIndexFromContainer(source, out var index))
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

    private static bool MatchesSelectAllHotkey(IEnumerable<KeyGesture> hotkeys, KeyEventArgs args)
    {
        foreach (var hotkey in hotkeys)
        {
            if (hotkey.Matches(args))
            {
                return true;
            }
        }

        return false;
    }
    
    private void HandleItemCountChanged()
    {
        ItemCountChanged?.Invoke(this, new ItemCountChangedEventArgs(TotalItemCount));
    }
    
    private void ReConfigureGroupInfo()
    {
        if (_collectionView != null)
        {
            _collectionView.GroupDescriptions.Clear();
            if (IsGroupEnabled)
            {
                if (GroupPropertySelector != null)
                {
                    _collectionView.GroupDescriptions.Add(new ListGroupDescription(GroupPropertySelector));
                }
            }
        }
    }
    
    /// <summary>
    /// Lets the selection part react to the applied template, then updates the pseudo-classes, the empty
    /// indicator and the pager visibility. The base member is called first.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        NotifyApplyTemplateForSelecting();
        UpdatePseudoClasses();
        ConfigureEmptyIndicator();
        HandlePaginationVisibility();
    }

    private void ConfigureFilterDescription()
    {
        if (_collectionView != null)
        {
            _collectionView.FilterDescriptions.Clear();
            if (FilterValue != null && Filter != null)
            {
                _collectionView.FilterDescriptions.Add(new ListFilterDescription()
                {
                    FilterPropertySelector = FilterValueSelector ?? (record => (record as IListItemData)?.Content) ,
                    FilterConditions       = [FilterValue],
                    Filter                 = Filter.Filter
                });
                EnsureDefaultFilterCallback(_collectionView);
            }
            else
            {
                ClearDefaultFilterCallback(_collectionView);
            }
            IsFiltering = _collectionView.FilterDescriptions.Count > 0;
        }

        NotifyFilterContextChanged();
        FilterContextChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called after the filter description of the collection view was rebuilt and before
    /// <see cref="FilterContextChanged" /> is raised. The base implementation does nothing.
    /// </summary>
    protected virtual void NotifyFilterContextChanged()
    {
    }

    private void ConfigureSortDescriptions()
    {
        if (_collectionView != null)
        {
            _collectionView.SortDescriptions.Clear();
            if (SortDescriptions != null)
            {
                _collectionView.SortDescriptions.AddRange(SortDescriptions);
            }
        }
    }

    private void EnsureDefaultFilterCallback(IListCollectionView collectionView)
    {
        if (ReferenceEquals(_defaultFilterOwner, collectionView) &&
            ReferenceEquals(collectionView.Filter, _defaultFilterCallback))
        {
            return;
        }

        if (collectionView.Filter == null)
        {
            _defaultFilterOwner    = collectionView;
            _defaultFilterCallback = new ListDefaultFilter(collectionView);
            collectionView.Filter  = _defaultFilterCallback;
        }
    }

    private void ClearDefaultFilterCallback(IListCollectionView collectionView)
    {
        if (ReferenceEquals(_defaultFilterOwner, collectionView) &&
            ReferenceEquals(collectionView.Filter, _defaultFilterCallback))
        {
            collectionView.Filter = null;
        }

        if (ReferenceEquals(_defaultFilterOwner, collectionView))
        {
            _defaultFilterOwner    = null;
            _defaultFilterCallback = null;
        }
    }
}
