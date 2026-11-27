using System.Collections.ObjectModel;
using System.Collections.Specialized;
using AtomUI.Controls;
using AtomUI.Controls.Utils;
using AtomUI.Data;
using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Base class of the controls that move items between a source panel and a target panel.
/// The class splits <see cref="ItemsSource" /> into the item sources of the two panels from
/// <see cref="TargetKeys" /> and the filter text, keeps <see cref="SelectedKeys" /> and
/// <see cref="TargetKeys" /> synchronized with the panels, and moves items by adding keys to or
/// removing keys from <see cref="TargetKeys" />. <see cref="ListTransfer" /> and
/// <see cref="TreeTransfer" /> provide the panel views and the themes with the operation buttons.
/// </summary>
public abstract class AbstractTransfer: TemplatedControl,
                                        IMotionAwareControl,
                                        IInputControlStatusAware,
                                        ICustomizableSizeTypeAware
{
    /// <summary>
    /// Names the panels whose item source <see cref="ConfigurePanelItemsSourceForFilter" /> rebuilds.
    /// </summary>
    [Flags]
    protected enum FilterChangeType
    {
        /// <summary>
        /// Rebuilds the item source of the source panel only.
        /// </summary>
        Source = 0x01,
        /// <summary>
        /// Rebuilds the item source of the target panel only.
        /// </summary>
        Target = 0x02,
        /// <summary>
        /// Rebuilds the item sources of the source and the target panel.
        /// </summary>
        Both = Source | Target
    }

    #region 公共属性定义
    
    /// <summary>
    /// Identifies the <see cref="ItemsSource" /> property.
    /// </summary>
    public static readonly StyledProperty<IEnumerable<IItemKey>?> ItemsSourceProperty =
        AvaloniaProperty.Register<AbstractTransfer, IEnumerable<IItemKey>?>(nameof(ItemsSource));
    
    /// <summary>
    /// Identifies the <see cref="Status" /> property.
    /// </summary>
    public static readonly StyledProperty<InputControlStatus> StatusProperty =
        InputControlStatusProperty.StatusProperty.AddOwner<AbstractTransfer>();
    
    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<AbstractTransfer>();
    
    /// <summary>
    /// Identifies the <see cref="ListWidth" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ListWidthProperty =
        AvaloniaProperty.Register<AbstractTransfer, double>(nameof(ListWidth), double.NaN);
    
    /// <summary>
    /// Identifies the <see cref="ListHeight" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ListHeightProperty =
        AvaloniaProperty.Register<AbstractTransfer, double>(nameof(ListHeight), double.NaN);
    
    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractTransfer>();
    
    /// <summary>
    /// Identifies the <see cref="SourceViewFooter" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> SourceViewFooterProperty =
        AvaloniaProperty.Register<AbstractTransfer, object?>(nameof(SourceViewFooter));
    
    /// <summary>
    /// Identifies the <see cref="SourceViewFooterTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> SourceViewFooterTemplateProperty =
        AvaloniaProperty.Register<AbstractTransfer, IDataTemplate?>(nameof(SourceViewFooterTemplate));
    
    /// <summary>
    /// Identifies the <see cref="TargetViewFooter" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> TargetViewFooterProperty =
        AvaloniaProperty.Register<AbstractTransfer, object?>(nameof(TargetViewFooter));
    
    /// <summary>
    /// Identifies the <see cref="TargetViewFooterTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> TargetViewFooterTemplateProperty =
        AvaloniaProperty.Register<AbstractTransfer, IDataTemplate?>(nameof(TargetViewFooterTemplate));
    
    /// <summary>
    /// Identifies the <see cref="SelectionsIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<IIconTemplate?> SelectionsIconProperty =
        AvaloniaProperty.Register<AbstractTransfer, IIconTemplate?>(nameof(SelectionsIcon));
    
    /// <summary>
    /// Identifies the <see cref="ToSourceTransferIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> ToSourceTransferIconProperty =
        AvaloniaProperty.Register<AbstractTransfer, PathIcon?>(nameof(ToSourceTransferIcon));
    
    /// <summary>
    /// Identifies the <see cref="ToTargetTransferIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> ToTargetTransferIconProperty =
        AvaloniaProperty.Register<AbstractTransfer, PathIcon?>(nameof(ToTargetTransferIcon));
    
    /// <summary>
    /// Identifies the <see cref="IsShowSearch" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowSearchProperty =
        AvaloniaProperty.Register<AbstractTransfer, bool>(nameof(IsShowSearch));
    
    /// <summary>
    /// Identifies the <see cref="IsShowSelectAll" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowSelectAllProperty =
        AvaloniaProperty.Register<AbstractTransfer, bool>(nameof(IsShowSelectAll));
    
    /// <summary>
    /// Identifies the <see cref="SourceTitle" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> SourceTitleProperty =
        AvaloniaProperty.Register<AbstractTransfer, object?>(nameof(SourceTitle));
    
    /// <summary>
    /// Identifies the <see cref="SourceTitleTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> SourceTitleTemplateProperty =
        AvaloniaProperty.Register<AbstractTransfer, IDataTemplate?>(nameof(SourceTitleTemplate));
    
    /// <summary>
    /// Identifies the <see cref="TargetTitle" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> TargetTitleProperty =
        AvaloniaProperty.Register<AbstractTransfer, object?>(nameof(TargetTitle));
    
    /// <summary>
    /// Identifies the <see cref="TargetTitleTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> TargetTitleTemplateProperty =
        AvaloniaProperty.Register<AbstractTransfer, IDataTemplate?>(nameof(TargetTitleTemplate));
    
    /// <summary>
    /// Identifies the <see cref="IsOneWay" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsOneWayProperty =
        AvaloniaProperty.Register<AbstractTransfer, bool>(nameof(IsOneWay));
    
    /// <summary>
    /// Identifies the <see cref="SelectedKeys" /> property.
    /// </summary>
    public static readonly StyledProperty<IList<EntityKey>?> SelectedKeysProperty =
        AvaloniaProperty.Register<AbstractTransfer, IList<EntityKey>?>(
            nameof(SelectedKeys),
            defaultBindingMode: BindingMode.TwoWay);
    
    /// <summary>
    /// Identifies the <see cref="TargetKeys" /> property.
    /// </summary>
    public static readonly StyledProperty<IList<EntityKey>?> TargetKeysProperty =
        AvaloniaProperty.Register<AbstractTransfer, IList<EntityKey>?>(
            nameof(TargetKeys),
            defaultBindingMode: BindingMode.TwoWay);
    
    /// <summary>
    /// Identifies the <see cref="IsFilterEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsFilterEnabledProperty =
        AvaloniaProperty.Register<AbstractTransfer, bool>(nameof(IsFilterEnabled), false);
    
    /// <summary>
    /// Identifies the <see cref="Filter" /> property.
    /// </summary>
    public static readonly StyledProperty<IValueFilter?> FilterProperty =
        AvaloniaProperty.Register<AbstractTransfer, IValueFilter?>(nameof(Filter));
    
    /// <summary>
    /// Identifies the <see cref="FilterValueSelector" /> property.
    /// </summary>
    public static readonly StyledProperty<DefaultFilterValueSelector?> FilterValueSelectorProperty =
        AvaloniaProperty.Register<AbstractTransfer, DefaultFilterValueSelector?>(
            nameof(FilterValueSelector));
    
    /// <summary>
    /// Identifies the <see cref="FilterPlaceholderText" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> FilterPlaceholderTextProperty =
        AvaloniaProperty.Register<AbstractTransfer, string?>(nameof(FilterPlaceholderText));
    
    /// <summary>
    /// Identifies the <see cref="ToSourceButtonText" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> ToSourceButtonTextProperty =
        AvaloniaProperty.Register<AbstractTransfer, string?>(nameof(ToSourceButtonText));
    
    /// <summary>
    /// Identifies the <see cref="ToTargetButtonText" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> ToTargetButtonTextProperty =
        AvaloniaProperty.Register<AbstractTransfer, string?>(nameof(ToTargetButtonText));
    
    /// <summary>
    /// Identifies the <see cref="ItemTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        ItemsControl.ItemTemplateProperty.AddOwner<AbstractTransfer>();
    
    /// <summary>
    /// Identifies the <see cref="IsStretchView" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsStretchViewProperty =
        AvaloniaProperty.Register<AbstractTransfer, bool>(nameof(IsStretchView));
    
    
    /// <summary>
    /// All items the two panels are built from. The source panel lists the items whose key is not in
    /// <see cref="TargetKeys" />, the target panel the items whose key is, and each panel only the
    /// items that match its filter text. The panel item sources are snapshots of this collection, which
    /// is not observed: a change made to it shows the next time the panel items are rebuilt, for example
    /// when the filter text or <see cref="TargetKeys" /> changes. The default is <see langword="null" />,
    /// which leaves both panels empty.
    /// </summary>
    public IEnumerable<IItemKey>? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }
    
    /// <summary>
    /// Validation status of both panels. While the control is enabled, <c>Error</c> paints the border of
    /// both panels in the error color and <c>Warning</c> in the warning color.
    /// </summary>
    public InputControlStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }
    
    /// <summary>
    /// Size of both panels, passed to the panel views. The operation buttons between them stay small,
    /// and the width and the height of a panel follow <see cref="ListWidth" /> and
    /// <see cref="ListHeight" />.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    /// <summary>
    /// Width of each panel; <see cref="double.NaN" /> lets a panel take the width of its content. The
    /// theme sets the value from the control's <c>ListWidth</c> token while <see cref="IsStretchView" />
    /// is <see langword="false" /> and to <c>NaN</c> while it is <see langword="true" />, so that both
    /// panels share the available width.
    /// </summary>
    public double ListWidth
    {
        get => GetValue(ListWidthProperty);
        set => SetValue(ListWidthProperty, value);
    }
    
    /// <summary>
    /// Height of the list inside each panel; the header, the search box and the footer take additional
    /// space. The theme sets the value from the control's <c>ListHeight</c> token. An explicit
    /// <see cref="double.NaN" /> lets the list grow with its items.
    /// </summary>
    public double ListHeight
    {
        get => GetValue(ListHeightProperty);
        set => SetValue(ListHeightProperty, value);
    }
    
    /// <summary>
    /// Content of the footer area at the bottom of the source panel. The footer area is visible only
    /// while this value is not <see langword="null" />; the default is <see langword="null" />.
    /// </summary>
    [DependsOn(nameof(SourceViewFooterTemplate))]
    public object? SourceViewFooter
    {
        get => GetValue(SourceViewFooterProperty);
        set => SetValue(SourceViewFooterProperty, value);
    }
    
    /// <summary>
    /// Template used to display <see cref="SourceViewFooter" /> in the footer of the source panel. When
    /// it is <see langword="null" />, the footer is displayed without a template.
    /// </summary>
    public IDataTemplate? SourceViewFooterTemplate
    {
        get => GetValue(SourceViewFooterTemplateProperty);
        set => SetValue(SourceViewFooterTemplateProperty, value);
    }
    
    /// <summary>
    /// Content of the footer area at the bottom of the target panel. The footer area is visible only
    /// while this value is not <see langword="null" />; the default is <see langword="null" />.
    /// </summary>
    [DependsOn(nameof(TargetViewFooterTemplate))]
    public object? TargetViewFooter
    {
        get => GetValue(TargetViewFooterProperty);
        set => SetValue(TargetViewFooterProperty, value);
    }
    
    /// <summary>
    /// Template used to display <see cref="TargetViewFooter" /> in the footer of the target panel. When
    /// it is <see langword="null" />, the footer is displayed without a template.
    /// </summary>
    public IDataTemplate? TargetViewFooterTemplate
    {
        get => GetValue(TargetViewFooterTemplateProperty);
        set => SetValue(TargetViewFooterTemplateProperty, value);
    }
    
    /// <summary>
    /// Controls whether the selection dropdown and the panel views play their motion. The theme sets the
    /// value from the <c>EnableMotion</c> token.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    /// <summary>
    /// Icon of the selection dropdown button in the header of each panel; the dropdown selects the
    /// items of the panel in bulk. The theme supplies a down arrow, which both panels use unless a value
    /// is set here.
    /// </summary>
    public IIconTemplate? SelectionsIcon
    {
        get => GetValue(SelectionsIconProperty);
        set => SetValue(SelectionsIconProperty, value);
    }
    
    /// <summary>
    /// Icon of the button that moves the items checked in the target panel back to the source panel.
    /// When the value is <see langword="null" /> at the time the template is applied, the control sets a
    /// <see cref="LeftOutlined" /> icon instead.
    /// </summary>
    public PathIcon? ToSourceTransferIcon
    {
        get => GetValue(ToSourceTransferIconProperty);
        set => SetValue(ToSourceTransferIconProperty, value);
    }
    
    /// <summary>
    /// Icon of the button that moves the items checked in the source panel to the target panel. When
    /// the value is <see langword="null" /> at the time the template is applied, the control sets a
    /// <see cref="RightOutlined" /> icon instead.
    /// </summary>
    public PathIcon? ToTargetTransferIcon
    {
        get => GetValue(ToTargetTransferIconProperty);
        set => SetValue(ToTargetTransferIconProperty, value);
    }
    
    /// <summary>
    /// Controls nothing: the control and its themes do not read this value, so setting it has no
    /// effect. To show the search box of the panels, set <see cref="IsFilterEnabled" />.
    /// </summary>
    public bool IsShowSearch
    {
        get => GetValue(IsShowSearchProperty);
        set => SetValue(IsShowSearchProperty, value);
    }
    
    /// <summary>
    /// Controls nothing: the control and its themes do not read this value, so setting it has no
    /// effect.
    /// </summary>
    public bool IsShowSelectAll
    {
        get => GetValue(IsShowSelectAllProperty);
        set => SetValue(IsShowSelectAllProperty, value);
    }
    
    /// <summary>
    /// Title shown in the header of the source panel, aligned to its right edge. The default is
    /// <see langword="null" />.
    /// </summary>
    [DependsOn(nameof(SourceTitleTemplate))]
    public object? SourceTitle
    {
        get => GetValue(SourceTitleProperty);
        set => SetValue(SourceTitleProperty, value);
    }
    
    /// <summary>
    /// Template used to display <see cref="SourceTitle" /> in the header of the source panel.
    /// </summary>
    public IDataTemplate? SourceTitleTemplate
    {
        get => GetValue(SourceTitleTemplateProperty);
        set => SetValue(SourceTitleTemplateProperty, value);
    }

    /// <summary>
    /// Title shown in the header of the target panel, aligned to its right edge. The default is
    /// <see langword="null" />.
    /// </summary>
    [DependsOn(nameof(TargetTitleTemplate))]
    public object? TargetTitle
    {
        get => GetValue(TargetTitleProperty);
        set => SetValue(TargetTitleProperty, value);
    }
    
    /// <summary>
    /// Template used to display <see cref="TargetTitle" /> in the header of the target panel.
    /// </summary>
    public IDataTemplate? TargetTitleTemplate
    {
        get => GetValue(TargetTitleTemplateProperty);
        set => SetValue(TargetTitleTemplateProperty, value);
    }
    
    /// <summary>
    /// Makes the transfer one-way, from the source panel to the target panel. When it is
    /// <see langword="true" />, the button that moves items back to the source panel is hidden and the
    /// target panel shows no select-all checkbox.
    /// </summary>
    public bool IsOneWay
    {
        get => GetValue(IsOneWayProperty);
        set => SetValue(IsOneWayProperty, value);
    }
    
    /// <summary>
    /// Keys of the items checked in either panel: a key that is in <see cref="TargetKeys" /> refers to
    /// the item in the target panel, any other key to the item in the source panel. The control keeps
    /// the value in step with the panels: it updates the current list in place when the list is
    /// mutable, and otherwise sets a new list, which is <see langword="null" /> when nothing is
    /// checked. When the value implements <see cref="INotifyCollectionChanged" />, changes made to the
    /// collection are applied to the panels. The property binds two-way by default.
    /// </summary>
    public IList<EntityKey>? SelectedKeys
    {
        get => GetValue(SelectedKeysProperty);
        set => SetValue(SelectedKeysProperty, value);
    }
    
    /// <summary>
    /// Keys of the items shown in the target panel. The target panel lists these items in the order of
    /// <see cref="ItemsSource" /> and the source panel omits them; <see langword="null" /> or an empty
    /// list leaves the target panel empty. Moving items adds keys to or removes keys from the current
    /// list in place when the list is mutable, and otherwise sets a new list. When the value implements
    /// <see cref="INotifyCollectionChanged" />, changes made to the collection update both panels. The
    /// property binds two-way by default.
    /// </summary>
    public IList<EntityKey>? TargetKeys
    {
        get => GetValue(TargetKeysProperty);
        set => SetValue(TargetKeysProperty, value);
    }

    /// <summary>
    /// Shows a search box in each panel and limits the panel to the items that match the text typed
    /// into it. The default is <see langword="false" />, which hides the search boxes and lists all
    /// items.
    /// </summary>
    public bool IsFilterEnabled
    {
        get => GetValue(IsFilterEnabledProperty);
        set => SetValue(IsFilterEnabledProperty, value);
    }
    
    /// <summary>
    /// Predicate that decides whether an item matches the filter text of a panel. It receives the value
    /// selected by <see cref="FilterValueSelector" />, or the item itself when the selector is
    /// <see langword="null" />, and the filter text; the item is listed when it returns
    /// <see langword="true" />. The predicate is used only while <see cref="IsFilterEnabled" /> is
    /// <see langword="true" /> and the filter text is not empty. When filtering is enabled and no filter
    /// is set at initialization, a <see cref="ValueFilterMode.Contains" /> filter is installed.
    /// </summary>
    public IValueFilter? Filter
    {
        get => GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }
    
    /// <summary>
    /// Selects the value that <see cref="Filter" /> tests for an item. When it is
    /// <see langword="null" />, the item itself is passed to the filter.
    /// </summary>
    public DefaultFilterValueSelector? FilterValueSelector
    {
        get => GetValue(FilterValueSelectorProperty);
        set => SetValue(FilterValueSelectorProperty, value);
    }
    
    /// <summary>
    /// Placeholder text of the search box of both panels. It is used only while
    /// <see cref="IsFilterEnabled" /> is <see langword="true" />.
    /// </summary>
    public string? FilterPlaceholderText
    {
        get => GetValue(FilterPlaceholderTextProperty);
        set => SetValue(FilterPlaceholderTextProperty, value);
    }
    
    /// <summary>
    /// Text of the button that moves the items checked in the target panel back to the source panel.
    /// When it is <see langword="null" />, the button shows no text.
    /// </summary>
    public string? ToSourceButtonText
    {
        get => GetValue(ToSourceButtonTextProperty);
        set => SetValue(ToSourceButtonTextProperty, value);
    }
    
    /// <summary>
    /// Text of the button that moves the items checked in the source panel to the target panel. When
    /// it is <see langword="null" />, the button shows no text.
    /// </summary>
    public string? ToTargetButtonText
    {
        get => GetValue(ToTargetButtonTextProperty);
        set => SetValue(ToTargetButtonTextProperty, value);
    }
    
    /// <summary>
    /// Template that displays an item of <see cref="ItemsSource" /> in both panels. The theme's default
    /// template shows the <c>Content</c> of an <c>IListItemData</c> item as text; set a template for
    /// items of another type.
    /// </summary>
    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }
    
    /// <summary>
    /// Lets both panels share the width of the control instead of taking a fixed width. When it is
    /// <see langword="true" />, each panel gets an equal share of the width left next to the operation
    /// buttons and the theme sets <see cref="ListWidth" /> to <c>NaN</c>. The default is
    /// <see langword="false" />, which lets the panels take the width from the theme.
    /// </summary>
    public bool IsStretchView
    {
        get => GetValue(IsStretchViewProperty);
        set => SetValue(IsStretchViewProperty, value);
    }
    
    #endregion

    #region 公共事件定义
    /// <summary>
    /// Raised after a rebuild of the panel items changed the items listed in at least one panel, which
    /// happens when the filter text, <see cref="ItemsSource" />, a filter setting or
    /// <see cref="TargetKeys" /> changes. The event is not raised for a change of the checked items.
    /// <see cref="TransferSelectionChangedEventArgs.SourceSelectedKeys" /> and
    /// <see cref="TransferSelectionChangedEventArgs.TargetSelectedKeys" /> carry the keys of the items
    /// listed in the rebuilt panels; the list of a panel that was not rebuilt in that pass is
    /// <see langword="null" />.
    /// </summary>
    public event EventHandler<TransferSelectionChangedEventArgs>? SelectionChanged;
    #endregion

    #region 内部属性定义
    
    internal static readonly StyledProperty<IEnumerable<IItemKey>?> SourceViewSourceProperty =
        AvaloniaProperty.Register<AbstractTransfer, IEnumerable<IItemKey>?>(nameof(SourceViewSource));
    
    internal static readonly StyledProperty<IEnumerable<IItemKey>?> TargetViewSourceProperty =
        AvaloniaProperty.Register<AbstractTransfer, IEnumerable<IItemKey>?>(nameof(TargetViewSource));
    
    internal static readonly DirectProperty<AbstractTransfer, bool> IsToTargetButtonEnabledProperty =
        AvaloniaProperty.RegisterDirect<AbstractTransfer, bool>(nameof(IsToTargetButtonEnabled),
            o => o.IsToTargetButtonEnabled,
            (o, v) => o.IsToTargetButtonEnabled = v);
    
    internal static readonly DirectProperty<AbstractTransfer, bool> IsToSourceButtonEnabledProperty =
        AvaloniaProperty.RegisterDirect<AbstractTransfer, bool>(nameof(IsToSourceButtonEnabled),
            o => o.IsToSourceButtonEnabled,
            (o, v) => o.IsToSourceButtonEnabled = v);
    
    internal static readonly DirectProperty<AbstractTransfer, string?> SourceFilterValueProperty =
        AvaloniaProperty.RegisterDirect<AbstractTransfer, string?>(nameof(SourceFilterValue),
            o => o.SourceFilterValue,
            (o, v) => o.SourceFilterValue = v);
    
    internal static readonly DirectProperty<AbstractTransfer, string?> TargetFilterValueProperty =
        AvaloniaProperty.RegisterDirect<AbstractTransfer, string?>(nameof(TargetFilterValue),
            o => o.TargetFilterValue,
            (o, v) => o.TargetFilterValue = v);
    
    internal static readonly StyledProperty<bool> IsPaginationEnabledProperty =
        AvaloniaProperty.Register<AbstractTransfer, bool>(nameof(IsPaginationEnabled));

    internal static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        TransferSelectDropdown.IsPopupPinnedOpenProperty.AddOwner<AbstractTransfer>();
    
    internal IEnumerable<IItemKey>? SourceViewSource
    {
        get => GetValue(SourceViewSourceProperty);
        set => SetValue(SourceViewSourceProperty, value);
    }
    
    internal IEnumerable<IItemKey>? TargetViewSource
    {
        get => GetValue(TargetViewSourceProperty);
        set => SetValue(TargetViewSourceProperty, value);
    }
    
    private bool _toTargetButtonEnabled;
    internal bool IsToTargetButtonEnabled
    {
        get => _toTargetButtonEnabled;
        set => SetAndRaise(IsToTargetButtonEnabledProperty, ref _toTargetButtonEnabled, value);
    }
    
    private bool _toSourceButtonEnabled;
    internal bool IsToSourceButtonEnabled
    {
        get => _toSourceButtonEnabled;
        set => SetAndRaise(IsToSourceButtonEnabledProperty, ref _toSourceButtonEnabled, value);
    }

    private string? _sourceFilterValue;
    internal string? SourceFilterValue
    {
        get => _sourceFilterValue;
        set => SetAndRaise(SourceFilterValueProperty, ref _sourceFilterValue, value);
    }
    
    private string? _targetFilterValue;
    internal string? TargetFilterValue
    {
        get => _targetFilterValue;
        set => SetAndRaise(TargetFilterValueProperty, ref _targetFilterValue, value);
    }
    
    internal bool IsPaginationEnabled
    {
        get => GetValue(IsPaginationEnabledProperty);
        set => SetValue(IsPaginationEnabledProperty, value);
    }

    internal bool IsPopupPinnedOpen
    {
        get => GetValue(IsPopupPinnedOpenProperty);
        set => SetCurrentValue(IsPopupPinnedOpenProperty, value);
    }
    
    #endregion

    private TransferItemDecorator? _sourceViewDecorator;
    private TransferItemDecorator? _targetViewDecorator;
    private ITransferView? _sourceView;
    private ITransferView? _targetView;
    private Grid? _rootLayout;
    private IDisposable? _popupPinnedOpenBinding;
    private TransferSelectDropdown? _pinnedOpenDropdown;
    private int _pinnedOpenGeneration;
    private INotifyCollectionChanged? _selectedKeysCollectionChangedSource;
    private INotifyCollectionChanged? _targetKeysCollectionChangedSource;
    private bool _isVisualTreeAttached;
    
    static AbstractTransfer()
    {
        IconButton.ClickEvent.AddClassHandler<AbstractTransfer>((transfer, args) => transfer.HandleTransferRequest(args));
        TransferSelectDropdown.SelectActionRequestEvent.AddClassHandler<AbstractTransfer>((transfer, args) => transfer.HandleSelectActionRequest(args));
        LineEdit.TextChangedEvent.AddClassHandler<AbstractTransfer>((transfer, args) => transfer.HandleTransferFilterChanged(args));
    }
    
    public AbstractTransfer()
    {
    }

    /// <summary>
    /// Installs a <see cref="ValueFilterMode.Contains" /> filter when filtering is enabled and
    /// <see cref="Filter" /> is <see langword="null" />. Overrides must call the base method to keep
    /// this default.
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (IsFilterEnabled && Filter == null)
        {
            SetCurrentValue(FilterProperty, ValueFilterFactory.BuildFilter(ValueFilterMode.Contains));
        }
    }

    /// <summary>
    /// Connects this class to the template: it finds the source and target panel decorators and the
    /// root layout grid, detaches from the panels of the previously applied template, arranges the grid
    /// columns for <see cref="IsStretchView" />, and replaces unset transfer icons with built-in ones.
    /// Overrides must call the base method, otherwise the panels are not connected.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ReleasePinnedOpenDropdown();
        if (ToSourceTransferIcon == null)
        {
            SetCurrentValue(ToSourceTransferIconProperty, new LeftOutlined());
        }

        if (ToTargetTransferIcon == null)
        {
            SetCurrentValue(ToTargetTransferIconProperty, new RightOutlined());
        }

        if (_sourceViewDecorator != null)
        {
            _sourceViewDecorator.TransferViewCreated -= HandleTransferViewCreated;
        }
        if (_targetViewDecorator != null)
        {
            _targetViewDecorator.TransferViewCreated -= HandleTransferViewCreated;
        }

        if (_targetView != null)
        {
            _targetView.ItemsRemoved -= HandleItemRemoved;
            _targetView.SelectedKeyChanged -= HandleTransferViewSelectedKeysChanged;
        }
        if (_sourceView != null)
        {
            _sourceView.SelectedKeyChanged -= HandleTransferViewSelectedKeysChanged;
        }
        _sourceView = null;
        _targetView = null;
        _sourceViewDecorator = e.NameScope.Find<TransferItemDecorator>("SourceDecoratorView");
        _targetViewDecorator = e.NameScope.Find<TransferItemDecorator>("TargetDecoratorView");
        _rootLayout          = e.NameScope.Find<Grid>("RootLayout");
        ConfigureRootLayout();
        if (_sourceViewDecorator != null)
        {
            _sourceViewDecorator.TransferViewCreated += HandleTransferViewCreated;
        }
        if (_targetViewDecorator != null)
        {
            _targetViewDecorator.TransferViewCreated += HandleTransferViewCreated;
        }

        if (_isVisualTreeAttached && IsPopupPinnedOpen)
        {
            QueuePinnedOpenDropdownRegistration();
        }
    }

    /// <summary>
    /// Starts observing collection changes of <see cref="SelectedKeys" /> and <see cref="TargetKeys" />,
    /// builds the items of both panels and applies the selected keys to them. Overrides must call the
    /// base method, otherwise the panels are not synchronized with the properties.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isVisualTreeAttached = true;
        ConfigureSelectedKeysCollectionChangedSource(SelectedKeys);
        ConfigureTargetKeysCollectionChangedSource(TargetKeys);
        ConfigurePanelItemsSourceForFilter(FilterChangeType.Both);
        ApplySelectedKeysToTransferViews();
        if (IsPopupPinnedOpen)
        {
            QueuePinnedOpenDropdownRegistration();
        }
    }

    /// <summary>
    /// Releases the pinned-open selection dropdown and stops observing collection changes of
    /// <see cref="SelectedKeys" /> and <see cref="TargetKeys" />. Overrides must call the base method;
    /// this implementation calls it last.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isVisualTreeAttached = false;
        ReleasePinnedOpenDropdown();
        ReleaseSelectedKeysCollectionChangedSource();
        ReleaseTargetKeysCollectionChangedSource();
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Keeps the panels in step with property changes: a change of the filter text,
    /// <see cref="ItemsSource" />, a filter setting or <see cref="TargetKeys" /> rebuilds the items of
    /// the affected panels, a change of <see cref="SelectedKeys" /> or <see cref="TargetKeys" />
    /// switches the observed collection, and a change of <see cref="IsStretchView" /> rearranges the
    /// layout columns. Overrides must call the base method first.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
 
        if (change.Property == SourceFilterValueProperty)
        {
            ConfigurePanelItemsSourceForFilter(FilterChangeType.Source);
        }
        else if (change.Property == TargetFilterValueProperty)
        {
            ConfigurePanelItemsSourceForFilter(FilterChangeType.Target);
        }
        else if (change.Property == FilterValueSelectorProperty ||
                 change.Property == FilterProperty ||
                 change.Property == IsFilterEnabledProperty ||
                 change.Property == ItemsSourceProperty)
        {
            ConfigurePanelItemsSourceForFilter(FilterChangeType.Both);
            ApplySelectedKeysToTransferViews();
        }
        else if (change.Property == SelectedKeysProperty)
        {
            ConfigureSelectedKeysCollectionChangedSource(change.GetNewValue<IList<EntityKey>?>());
            ApplySelectedKeysToTransferViews();
        }
        else if (change.Property == TargetKeysProperty)
        {
            ConfigureTargetKeysCollectionChangedSource(change.GetNewValue<IList<EntityKey>?>());
            HandleTargetKeysChanged();
        }
        else if (change.Property == IsStretchViewProperty)
        {
            ConfigureRootLayout();
        }
        else if (change.Property == IsPopupPinnedOpenProperty)
        {
            if (change.GetNewValue<bool>() &&
                _isVisualTreeAttached &&
                _pinnedOpenDropdown is null)
            {
                QueuePinnedOpenDropdownRegistration();
            }
            else if (!change.GetNewValue<bool>())
            {
                UnpinPinnedOpenDropdown();
            }
        }
    }

    private void QueuePinnedOpenDropdownRegistration()
    {
        var generation = ++_pinnedOpenGeneration;
        Dispatcher.UIThread.Post(() =>
        {
            if (generation != _pinnedOpenGeneration ||
                !IsPopupPinnedOpen ||
                !_isVisualTreeAttached)
            {
                return;
            }

            var dropdowns = this.GetVisualDescendants()
                                .OfType<TransferSelectDropdown>()
                                .ToArray();
            var dropdown = dropdowns.FirstOrDefault(candidate => candidate.ViewType == TransferViewType.Source)
                           ?? dropdowns.FirstOrDefault();
            RegisterPinnedOpenDropdown(dropdown);
        }, DispatcherPriority.Loaded);
    }

    private void RegisterPinnedOpenDropdown(TransferSelectDropdown? dropdown)
    {
        if (dropdown is null || ReferenceEquals(dropdown, _pinnedOpenDropdown))
        {
            return;
        }

        ReleasePinnedOpenDropdown();
        _pinnedOpenDropdown      = dropdown;
        _popupPinnedOpenBinding = BindUtils.RelayBind(
            this,
            IsPopupPinnedOpenProperty,
            dropdown,
            TransferSelectDropdown.IsPopupPinnedOpenProperty);
    }

    private void ReleasePinnedOpenDropdown()
    {
        ++_pinnedOpenGeneration;
        _pinnedOpenDropdown?.CloseForLifecycle();
        ClearPinnedOpenDropdownRegistration();
    }

    private void UnpinPinnedOpenDropdown()
    {
        ++_pinnedOpenGeneration;
        ClearPinnedOpenDropdownRegistration();
    }

    private void ClearPinnedOpenDropdownRegistration()
    {
        _popupPinnedOpenBinding?.Dispose();
        _popupPinnedOpenBinding = null;
        _pinnedOpenDropdown?.SetCurrentValue(TransferSelectDropdown.IsPopupPinnedOpenProperty, false);
        _pinnedOpenDropdown = null;
    }

    /// <summary>
    /// Builds the items of the panels selected by <paramref name="changeType" /> and assigns them to
    /// the panels, updating the item collection of a panel in place when it already has one. It raises
    /// <see cref="SelectionChanged" /> when the items of at least one panel changed. The method is
    /// called when the control is attached and whenever the filter text, <see cref="ItemsSource" />, a
    /// filter setting or <see cref="TargetKeys" /> changes. An override that does not call the base
    /// method must update the panels itself.
    /// </summary>
    /// <param name="changeType">
    /// The panels whose items are built.
    /// </param>
    protected virtual void ConfigurePanelItemsSourceForFilter(FilterChangeType changeType)
    {
        var               sourcePanelSourceChanged = false;
        var               targetPanelSourceChanged = false;
        IList<EntityKey>? sourceItemKeys           = null;
        IList<EntityKey>? targetItemKeys           = null;
        var               targetKeySet             = BuildTargetKeySet(TargetKeys);
        var               sourceChanged            = (changeType & FilterChangeType.Source) == FilterChangeType.Source;
        var               targetChanged            = (changeType & FilterChangeType.Target) == FilterChangeType.Target;
        if (sourceChanged)
        {
            var sourcePanelSource = BuildSourcePanelSource(targetKeySet);
            sourcePanelSourceChanged = SetPanelItemsSource(SourceViewSourceProperty, SourceViewSource, sourcePanelSource);
            sourceItemKeys           = BuildItemKeyList(sourcePanelSource);
        }

        if (targetChanged)
        {
            var targetPanelSource = BuildTargetPanelSource(targetKeySet);
            targetPanelSourceChanged = SetPanelItemsSource(TargetViewSourceProperty, TargetViewSource, targetPanelSource);
            targetItemKeys           = BuildItemKeyList(targetPanelSource);
        }

        if (sourcePanelSourceChanged || targetPanelSourceChanged)
        {
            NotifySelectionChanged(sourceItemKeys, targetItemKeys);
        }
    }

    /// <summary>
    /// Builds the items of the source panel: the items of <see cref="ItemsSource" /> whose key is not
    /// in <paramref name="targetKeySet" /> and that match the filter text of the source panel.
    /// </summary>
    /// <param name="targetKeySet">
    /// The keys of the items in the target panel, or <see langword="null" /> when it has none.
    /// </param>
    /// <returns>
    /// A new list with the matching items in the order of <see cref="ItemsSource" />, or
    /// <see langword="null" /> when <see cref="ItemsSource" /> is <see langword="null" />.
    /// </returns>
    protected IReadOnlyList<IItemKey>? BuildSourcePanelSource(ISet<EntityKey>? targetKeySet)
    {
        if (ItemsSource == null)
        {
            return null;
        }

        var source = ItemsSource;
        var items = source switch
        {
            ICollection<IItemKey> collection => new List<IItemKey>(collection.Count),
            IReadOnlyCollection<IItemKey> collection => new List<IItemKey>(collection.Count),
            _ => new List<IItemKey>()
        };
        foreach (var item in source)
        {
            if ((targetKeySet?.Contains(item.ItemKey ?? default) ?? false) ||
                !IsFilterMatched(item, SourceFilterValue))
            {
                continue;
            }

            items.Add(item);
        }
        return items;
    }

    /// <summary>
    /// Builds the items of the target panel: the items of <see cref="ItemsSource" /> whose key is in
    /// <paramref name="targetKeySet" /> and that match the filter text of the target panel.
    /// </summary>
    /// <param name="targetKeySet">
    /// The keys of the items in the target panel, or <see langword="null" /> when it has none.
    /// </param>
    /// <returns>
    /// A new list with the matching items in the order of <see cref="ItemsSource" />, or
    /// <see langword="null" /> when <see cref="ItemsSource" /> is <see langword="null" />.
    /// </returns>
    protected IReadOnlyList<IItemKey>? BuildTargetPanelSource(ISet<EntityKey>? targetKeySet)
    {
        if (ItemsSource == null)
        {
            return null;
        }

        if (targetKeySet == null)
        {
            return Array.Empty<IItemKey>();
        }

        var items = new List<IItemKey>(targetKeySet.Count);
        foreach (var item in ItemsSource)
        {
            if (!targetKeySet.Contains(item.ItemKey ?? default) ||
                !IsFilterMatched(item, TargetFilterValue))
            {
                continue;
            }

            items.Add(item);
        }
        return items;
    }

    /// <summary>
    /// Raises <see cref="SelectionChanged" /> with the given key lists.
    /// </summary>
    /// <param name="sourceItemKeys">
    /// The keys of the items listed in the source panel, or <see langword="null" /> when it was not
    /// rebuilt.
    /// </param>
    /// <param name="targetItemKeys">
    /// The keys of the items listed in the target panel, or <see langword="null" /> when it was not
    /// rebuilt.
    /// </param>
    protected void NotifySelectionChanged(IList<EntityKey>? sourceItemKeys, IList<EntityKey>? targetItemKeys)
    {
        SelectionChanged?.Invoke(this, new TransferSelectionChangedEventArgs(sourceItemKeys, targetItemKeys));
    }

    /// <summary>
    /// Moves the items checked in a panel by changing <see cref="TargetKeys" />: for
    /// <see cref="TransferDirection.ToTarget" /> the keys checked in the source panel are added, for
    /// <see cref="TransferDirection.ToSource" /> the keys checked in the target panel are removed. Both
    /// panels are notified before and after the change. The method is called when a
    /// <see cref="Button" /> inside the control is clicked while its <c>Tag</c> is a
    /// <see cref="TransferDirection" />. An override must call the base method or change
    /// <see cref="TargetKeys" /> itself.
    /// </summary>
    /// <param name="transferDirection">
    /// The direction in which the checked items are moved.
    /// </param>
    protected virtual void TransferItems(TransferDirection transferDirection)
    {
        _sourceViewDecorator?.NotifyAboutToTransfer(transferDirection);
        _targetViewDecorator?.NotifyAboutToTransfer(transferDirection);
        if (transferDirection == TransferDirection.ToTarget)
        {
            var keys = _sourceViewDecorator?.SelectedKeys;
            AddTargetKeys(keys);
        }
        else
        {
            var keys = _targetViewDecorator?.SelectedKeys;
            RemoveTargetKeys(keys);
        }
        _sourceViewDecorator?.NotifyTransferCompleted(transferDirection);
        _targetViewDecorator?.NotifyTransferCompleted(transferDirection);
    }

    /// <summary>
    /// Builds a hash set from a list of keys.
    /// </summary>
    /// <param name="keys">
    /// The keys to copy.
    /// </param>
    /// <returns>
    /// A new set with the keys, or <see langword="null" /> when <paramref name="keys" /> is
    /// <see langword="null" /> or empty.
    /// </returns>
    protected static HashSet<EntityKey>? BuildTargetKeySet(ICollection<EntityKey>? keys)
    {
        if (keys == null || keys.Count == 0)
        {
            return null;
        }

        var keySet = new HashSet<EntityKey>(keys.Count);
        foreach (var key in keys)
        {
            keySet.Add(key);
        }
        return keySet;
    }

    /// <summary>
    /// Copies the keys of a collection into a new list.
    /// </summary>
    /// <param name="keys">
    /// The keys to copy.
    /// </param>
    /// <returns>
    /// A new list with the keys in the order of <paramref name="keys" />.
    /// </returns>
    protected static List<EntityKey> BuildEntityKeyList(ICollection<EntityKey> keys)
    {
        var keyList = new List<EntityKey>(keys.Count);
        foreach (var key in keys)
        {
            keyList.Add(key);
        }
        return keyList;
    }

    /// <summary>
    /// Collects the keys of a sequence of items.
    /// </summary>
    /// <param name="items">
    /// The items to read the keys from.
    /// </param>
    /// <returns>
    /// A new list with the key of each item in the order of <paramref name="items" />, or
    /// <see langword="null" /> when <paramref name="items" /> is <see langword="null" />.
    /// </returns>
    protected static List<EntityKey>? BuildItemKeyList(IEnumerable<IItemKey>? items)
    {
        if (items == null)
        {
            return null;
        }

        var keyList = items switch
        {
            ICollection<IItemKey> collection => new List<EntityKey>(collection.Count),
            IReadOnlyCollection<IItemKey> collection => new List<EntityKey>(collection.Count),
            _ => new List<EntityKey>()
        };
        foreach (var item in items)
        {
            keyList.Add(item.ItemKey ?? default);
        }
        return keyList;
    }

    private bool IsFilterMatched(IItemKey item, string? filterValue)
    {
        return !IsFilterEnabled ||
               string.IsNullOrEmpty(filterValue) ||
               (Filter?.Filter(FilterValueSelector != null ? FilterValueSelector(item) : item,
                   filterValue) ?? false);
    }

    private void HandleTransferViewCreated(object? sender, TransferViewCreatedEventArgs args)
    {
        if (args.TransferView.ViewType == TransferViewType.Source)
        {
            if (_sourceView != null)
            {
                _sourceView.SelectedKeyChanged -= HandleTransferViewSelectedKeysChanged;
            }
            _sourceView = args.TransferView;
            _sourceView.SelectedKeyChanged += HandleTransferViewSelectedKeysChanged;
        }
        else
        {
            if (_targetView != null)
            {
                _targetView.ItemsRemoved -= HandleItemRemoved;
                _targetView.SelectedKeyChanged -= HandleTransferViewSelectedKeysChanged;
            }
            _targetView              =  args.TransferView;
            _targetView.ItemsRemoved += HandleItemRemoved;
            _targetView.SelectedKeyChanged += HandleTransferViewSelectedKeysChanged;
        }

        ApplySelectedKeysToTransferViews();
    }

    private void HandleItemRemoved(object? sender, TransferItemsRemovedEventArgs args)
    {
        _sourceViewDecorator?.NotifyAboutToTransfer(TransferDirection.ToSource);
        if (args.Items != null)
        {
            RemoveTargetKeys(BuildItemKeyList(args.Items));
        }
        _sourceViewDecorator?.NotifyTransferCompleted(TransferDirection.ToSource);
    }

    private void HandleTransferRequest(RoutedEventArgs args)
    {
        if (args.Source is Button button && button.Tag is TransferDirection transferDirection)
        {
            TransferItems(transferDirection);
        }
        args.Handled = true;
    }

    private void HandleSelectActionRequest(TransferSelectActionEventArgs args)
    {
        if (args.Action == TransferSelectAction.RemoveAll)
        {
            _sourceViewDecorator?.NotifyAboutToTransfer(TransferDirection.ToSource);
            ClearTargetKeys();
            _sourceViewDecorator?.NotifyTransferCompleted(TransferDirection.ToSource);
        }
    }

    private void HandleTransferFilterChanged(TextChangedEventArgs args)
    {
        if (!IsFilterEnabled)
        {
            return;
        }
        if (args.Source is LineEdit filterInput && filterInput.Tag is TransferViewType viewType)
        {
            if (viewType == TransferViewType.Source)
            {
                SourceFilterValue = filterInput.Text;
            }

            if (viewType == TransferViewType.Target)
            {
                TargetFilterValue = filterInput.Text;
            }
        }
    }

    private void ConfigureRootLayout()
    {
        if (_rootLayout != null)
        {
            _rootLayout.ColumnDefinitions.Clear();
            if (IsStretchView)
            {
                _rootLayout.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                _rootLayout.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
                _rootLayout.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            }
            else
            {
                _rootLayout.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
                _rootLayout.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
                _rootLayout.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            }
        }
    }

    private void ConfigureSelectedKeysCollectionChangedSource(IList<EntityKey>? selectedKeys)
    {
        if (!_isVisualTreeAttached)
        {
            ReleaseSelectedKeysCollectionChangedSource();
            return;
        }

        if (ReferenceEquals(_selectedKeysCollectionChangedSource, selectedKeys))
        {
            return;
        }

        ReleaseSelectedKeysCollectionChangedSource();

        _selectedKeysCollectionChangedSource = selectedKeys as INotifyCollectionChanged;
        if (_selectedKeysCollectionChangedSource != null)
        {
            _selectedKeysCollectionChangedSource.CollectionChanged += HandleSelectedKeysCollectionChanged;
        }
    }

    private void ReleaseSelectedKeysCollectionChangedSource()
    {
        if (_selectedKeysCollectionChangedSource != null)
        {
            _selectedKeysCollectionChangedSource.CollectionChanged -= HandleSelectedKeysCollectionChanged;
            _selectedKeysCollectionChangedSource = null;
        }
    }

    private void HandleSelectedKeysCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (!ReferenceEquals(sender, _selectedKeysCollectionChangedSource))
        {
            return;
        }

        ApplySelectedKeysToTransferViews();
    }

    private void ConfigureTargetKeysCollectionChangedSource(IList<EntityKey>? targetKeys)
    {
        if (!_isVisualTreeAttached)
        {
            ReleaseTargetKeysCollectionChangedSource();
            return;
        }

        if (ReferenceEquals(_targetKeysCollectionChangedSource, targetKeys))
        {
            return;
        }

        ReleaseTargetKeysCollectionChangedSource();

        _targetKeysCollectionChangedSource = targetKeys as INotifyCollectionChanged;
        if (_targetKeysCollectionChangedSource != null)
        {
            _targetKeysCollectionChangedSource.CollectionChanged += HandleTargetKeysCollectionChanged;
        }
    }

    private void ReleaseTargetKeysCollectionChangedSource()
    {
        if (_targetKeysCollectionChangedSource != null)
        {
            _targetKeysCollectionChangedSource.CollectionChanged -= HandleTargetKeysCollectionChanged;
            _targetKeysCollectionChangedSource = null;
        }
    }

    private void HandleTargetKeysCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (!ReferenceEquals(sender, _targetKeysCollectionChangedSource))
        {
            return;
        }

        HandleTargetKeysChanged();
    }

    private void HandleTargetKeysChanged()
    {
        ConfigurePanelItemsSourceForFilter(FilterChangeType.Both);
        ApplySelectedKeysToTransferViews();
    }

    private void HandleTransferViewSelectedKeysChanged(object? sender, EventArgs args)
    {
        UpdateSelectedKeysFromTransferViews();
    }

    private void ApplySelectedKeysToTransferViews()
    {
        var sourceSelectedKeys = new List<EntityKey>();
        var targetSelectedKeys = new List<EntityKey>();
        var targetKeySet       = BuildTargetKeySet(TargetKeys);
        if (SelectedKeys != null)
        {
            foreach (var key in SelectedKeys)
            {
                if (targetKeySet?.Contains(key) == true)
                {
                    targetSelectedKeys.Add(key);
                }
                else
                {
                    sourceSelectedKeys.Add(key);
                }
            }
        }

        SetTransferViewSelectedKeys(_sourceView, sourceSelectedKeys);
        SetTransferViewSelectedKeys(_targetView, targetSelectedKeys);
    }

    private static void SetTransferViewSelectedKeys(ITransferView? transferView, IList<EntityKey> selectedKeys)
    {
        if (transferView == null)
        {
            return;
        }

        if (AreKeyCollectionsEquivalent(transferView.SelectedKeys, selectedKeys))
        {
            return;
        }

        transferView.SelectedKeys = selectedKeys.Count == 0 ? null : BuildEntityKeyList(selectedKeys);
    }

    private void UpdateSelectedKeysFromTransferViews()
    {
        var selectedKeys = new List<EntityKey>();
        AddDistinctKeys(selectedKeys, _sourceView?.SelectedKeys);
        AddDistinctKeys(selectedKeys, _targetView?.SelectedKeys);
        SetOrUpdateSelectedKeys(selectedKeys);
    }

    private void SetOrUpdateSelectedKeys(IList<EntityKey> selectedKeys)
    {
        if (AreKeyCollectionsEquivalent(SelectedKeys, selectedKeys))
        {
            return;
        }

        if (TryUpdateExistingKeyList(SelectedKeys, selectedKeys, out var changed))
        {
            if (changed && SelectedKeys is not INotifyCollectionChanged)
            {
                ApplySelectedKeysToTransferViews();
            }
            return;
        }

        SetCurrentValue(SelectedKeysProperty, selectedKeys.Count == 0 ? null : BuildEntityKeyList(selectedKeys));
    }

    private void AddTargetKeys(ICollection<EntityKey>? keys)
    {
        if (keys == null || keys.Count == 0)
        {
            return;
        }

        if (CanUpdateExistingKeyList(TargetKeys))
        {
            var changed = false;
            foreach (var key in keys)
            {
                if (!TargetKeys!.Contains(key))
                {
                    TargetKeys.Add(key);
                    changed = true;
                }
            }

            if (changed && TargetKeys is not INotifyCollectionChanged)
            {
                HandleTargetKeysChanged();
            }
            return;
        }

        var currentSet = BuildMutableTargetKeySet();
        foreach (var key in keys)
        {
            currentSet.Add(key);
        }
        SetCurrentValue(TargetKeysProperty, BuildEntityKeyList(currentSet));
    }

    private void RemoveTargetKeys(ICollection<EntityKey>? keys)
    {
        if (keys == null || keys.Count == 0)
        {
            return;
        }

        if (CanUpdateExistingKeyList(TargetKeys))
        {
            var changed = false;
            foreach (var key in keys)
            {
                changed |= TargetKeys!.Remove(key);
            }

            if (changed && TargetKeys is not INotifyCollectionChanged)
            {
                HandleTargetKeysChanged();
            }
            return;
        }

        var currentSet = BuildMutableTargetKeySet();
        foreach (var key in keys)
        {
            currentSet.Remove(key);
        }
        SetCurrentValue(TargetKeysProperty, BuildEntityKeyList(currentSet));
    }

    private void ClearTargetKeys()
    {
        if (TargetKeys == null || TargetKeys.Count == 0)
        {
            return;
        }

        if (CanUpdateExistingKeyList(TargetKeys))
        {
            TargetKeys.Clear();
            if (TargetKeys is not INotifyCollectionChanged)
            {
                HandleTargetKeysChanged();
            }
            return;
        }

        SetCurrentValue(TargetKeysProperty, null);
    }

    private HashSet<EntityKey> BuildMutableTargetKeySet()
    {
        var currentSet = new HashSet<EntityKey>(TargetKeys?.Count ?? 0);
        if (TargetKeys != null)
        {
            foreach (var targetKey in TargetKeys)
            {
                currentSet.Add(targetKey);
            }
        }

        return currentSet;
    }

    private static bool TryUpdateExistingKeyList(
        IList<EntityKey>? currentKeys,
        ICollection<EntityKey> nextKeys,
        out bool changed)
    {
        changed = false;
        if (currentKeys == null || !CanUpdateExistingKeyList(currentKeys))
        {
            return false;
        }

        var nextKeySet = new HashSet<EntityKey>(nextKeys);
        for (var i = currentKeys.Count - 1; i >= 0; i--)
        {
            if (!nextKeySet.Contains(currentKeys[i]))
            {
                currentKeys.RemoveAt(i);
                changed = true;
            }
        }

        var currentKeySet = new HashSet<EntityKey>(currentKeys);
        foreach (var key in nextKeys)
        {
            if (currentKeySet.Add(key))
            {
                currentKeys.Add(key);
                changed = true;
            }
        }
        return true;
    }

    private static bool CanUpdateExistingKeyList(IList<EntityKey>? keys)
    {
        return keys != null &&
               !keys.IsReadOnly &&
               keys is not System.Collections.IList { IsFixedSize: true };
    }

    private static bool AreKeyCollectionsEquivalent(ICollection<EntityKey>? currentKeys, ICollection<EntityKey>? nextKeys)
    {
        if (currentKeys == null || currentKeys.Count == 0)
        {
            return nextKeys == null || nextKeys.Count == 0;
        }

        if (nextKeys == null || currentKeys.Count != nextKeys.Count)
        {
            return false;
        }

        var currentKeySet = new HashSet<EntityKey>(currentKeys.Count);
        foreach (var key in currentKeys)
        {
            currentKeySet.Add(key);
        }

        foreach (var key in nextKeys)
        {
            if (!currentKeySet.Contains(key))
            {
                return false;
            }
        }

        return true;
    }

    private bool SetPanelItemsSource(
        StyledProperty<IEnumerable<IItemKey>?> property,
        IEnumerable<IItemKey>? currentSource,
        IEnumerable<IItemKey>? nextSource)
    {
        if (AreItemSequencesEquivalent(currentSource, nextSource))
        {
            return false;
        }

        if (nextSource == null)
        {
            SetValue(property, null);
            return true;
        }

        if (currentSource is ObservableCollection<IItemKey> currentItems)
        {
            SynchronizePanelItems(currentItems, nextSource);
            return true;
        }

        SetValue(property, new ObservableCollection<IItemKey>(nextSource));
        return true;
    }

    private static void SynchronizePanelItems(
        ObservableCollection<IItemKey> currentItems,
        IEnumerable<IItemKey> nextItems)
    {
        var targetIndex = 0;
        foreach (var nextItem in nextItems)
        {
            var currentIndex = IndexOfPanelItem(currentItems, nextItem, targetIndex);
            if (currentIndex == targetIndex)
            {
                targetIndex++;
                continue;
            }

            if (currentIndex > targetIndex)
            {
                currentItems.Move(currentIndex, targetIndex);
            }
            else
            {
                currentItems.Insert(targetIndex, nextItem);
            }

            targetIndex++;
        }

        while (currentItems.Count > targetIndex)
        {
            currentItems.RemoveAt(currentItems.Count - 1);
        }
    }

    private static int IndexOfPanelItem(IList<IItemKey> items, IItemKey item, int startIndex)
    {
        for (var i = startIndex; i < items.Count; i++)
        {
            if (EqualityComparer<IItemKey>.Default.Equals(items[i], item))
            {
                return i;
            }
        }

        return -1;
    }

    private static bool AreItemSequencesEquivalent(IEnumerable<IItemKey>? currentItems, IEnumerable<IItemKey>? nextItems)
    {
        if (ReferenceEquals(currentItems, nextItems))
        {
            return true;
        }

        if (currentItems == null || nextItems == null)
        {
            return currentItems == null && nextItems == null;
        }

        using var currentEnumerator = currentItems.GetEnumerator();
        using var nextEnumerator    = nextItems.GetEnumerator();
        while (true)
        {
            var currentHasValue = currentEnumerator.MoveNext();
            var nextHasValue    = nextEnumerator.MoveNext();
            if (!currentHasValue || !nextHasValue)
            {
                return currentHasValue == nextHasValue;
            }

            if (!EqualityComparer<IItemKey>.Default.Equals(currentEnumerator.Current, nextEnumerator.Current))
            {
                return false;
            }
        }
    }

    private static void AddDistinctKeys(ICollection<EntityKey> target, ICollection<EntityKey>? keys)
    {
        if (keys == null || keys.Count == 0)
        {
            return;
        }

        var targetKeySet = new HashSet<EntityKey>(target);
        foreach (var key in keys)
        {
            if (targetKeySet.Add(key))
            {
                target.Add(key);
            }
        }
    }
}
