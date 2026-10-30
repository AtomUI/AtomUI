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

public abstract class AbstractTransfer: TemplatedControl,
                                        IMotionAwareControl,
                                        IInputControlStatusAware,
                                        ICustomizableSizeTypeAware
{
    [Flags]
    protected enum FilterChangeType
    {
        Source = 0x01,
        Target = 0x02,
        Both = Source | Target
    }

    #region 公共属性定义
    
    public static readonly StyledProperty<IEnumerable<IItemKey>?> ItemsSourceProperty =
        AvaloniaProperty.Register<AbstractTransfer, IEnumerable<IItemKey>?>(nameof(ItemsSource));
    
    public static readonly StyledProperty<InputControlStatus> StatusProperty =
        InputControlStatusProperty.StatusProperty.AddOwner<AbstractTransfer>();
    
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<AbstractTransfer>();
    
    public static readonly StyledProperty<double> ListWidthProperty =
        AvaloniaProperty.Register<AbstractTransfer, double>(nameof(ListWidth), double.NaN);
    
    public static readonly StyledProperty<double> ListHeightProperty =
        AvaloniaProperty.Register<AbstractTransfer, double>(nameof(ListHeight), double.NaN);
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractTransfer>();
    
    public static readonly StyledProperty<object?> SourceViewFooterProperty =
        AvaloniaProperty.Register<AbstractTransfer, object?>(nameof(SourceViewFooter));
    
    public static readonly StyledProperty<IDataTemplate?> SourceViewFooterTemplateProperty =
        AvaloniaProperty.Register<AbstractTransfer, IDataTemplate?>(nameof(SourceViewFooterTemplate));
    
    public static readonly StyledProperty<object?> TargetViewFooterProperty =
        AvaloniaProperty.Register<AbstractTransfer, object?>(nameof(TargetViewFooter));
    
    public static readonly StyledProperty<IDataTemplate?> TargetViewFooterTemplateProperty =
        AvaloniaProperty.Register<AbstractTransfer, IDataTemplate?>(nameof(TargetViewFooterTemplate));
    
    public static readonly StyledProperty<IIconTemplate?> SelectionsIconProperty =
        AvaloniaProperty.Register<AbstractTransfer, IIconTemplate?>(nameof(SelectionsIcon));
    
    public static readonly StyledProperty<PathIcon?> ToSourceTransferIconProperty =
        AvaloniaProperty.Register<AbstractTransfer, PathIcon?>(nameof(ToSourceTransferIcon));
    
    public static readonly StyledProperty<PathIcon?> ToTargetTransferIconProperty =
        AvaloniaProperty.Register<AbstractTransfer, PathIcon?>(nameof(ToTargetTransferIcon));
    
    public static readonly StyledProperty<bool> IsShowSearchProperty =
        AvaloniaProperty.Register<AbstractTransfer, bool>(nameof(IsShowSearch));
    
    public static readonly StyledProperty<bool> IsShowSelectAllProperty =
        AvaloniaProperty.Register<AbstractTransfer, bool>(nameof(IsShowSelectAll));
    
    public static readonly StyledProperty<object?> SourceTitleProperty =
        AvaloniaProperty.Register<AbstractTransfer, object?>(nameof(SourceTitle));
    
    public static readonly StyledProperty<IDataTemplate?> SourceTitleTemplateProperty =
        AvaloniaProperty.Register<AbstractTransfer, IDataTemplate?>(nameof(SourceTitleTemplate));
    
    public static readonly StyledProperty<object?> TargetTitleProperty =
        AvaloniaProperty.Register<AbstractTransfer, object?>(nameof(TargetTitle));
    
    public static readonly StyledProperty<IDataTemplate?> TargetTitleTemplateProperty =
        AvaloniaProperty.Register<AbstractTransfer, IDataTemplate?>(nameof(TargetTitleTemplate));
    
    public static readonly StyledProperty<bool> IsOneWayProperty =
        AvaloniaProperty.Register<AbstractTransfer, bool>(nameof(IsOneWay));
    
    public static readonly StyledProperty<IList<EntityKey>?> SelectedKeysProperty =
        AvaloniaProperty.Register<AbstractTransfer, IList<EntityKey>?>(
            nameof(SelectedKeys),
            defaultBindingMode: BindingMode.TwoWay);
    
    public static readonly StyledProperty<IList<EntityKey>?> TargetKeysProperty =
        AvaloniaProperty.Register<AbstractTransfer, IList<EntityKey>?>(
            nameof(TargetKeys),
            defaultBindingMode: BindingMode.TwoWay);
    
    public static readonly StyledProperty<bool> IsFilterEnabledProperty =
        AvaloniaProperty.Register<AbstractTransfer, bool>(nameof(IsFilterEnabled), false);
    
    public static readonly StyledProperty<IValueFilter?> FilterProperty =
        AvaloniaProperty.Register<AbstractTransfer, IValueFilter?>(nameof(Filter));
    
    public static readonly StyledProperty<DefaultFilterValueSelector?> FilterValueSelectorProperty =
        AvaloniaProperty.Register<AbstractTransfer, DefaultFilterValueSelector?>(
            nameof(FilterValueSelector));
    
    public static readonly StyledProperty<string?> FilterPlaceholderTextProperty =
        AvaloniaProperty.Register<AbstractTransfer, string?>(nameof(FilterPlaceholderText));
    
    public static readonly StyledProperty<string?> ToSourceButtonTextProperty =
        AvaloniaProperty.Register<AbstractTransfer, string?>(nameof(ToSourceButtonText));
    
    public static readonly StyledProperty<string?> ToTargetButtonTextProperty =
        AvaloniaProperty.Register<AbstractTransfer, string?>(nameof(ToTargetButtonText));
    
    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        ItemsControl.ItemTemplateProperty.AddOwner<AbstractTransfer>();
    
    public static readonly StyledProperty<bool> IsStretchViewProperty =
        AvaloniaProperty.Register<AbstractTransfer, bool>(nameof(IsStretchView));
    
    
    public IEnumerable<IItemKey>? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }
    
    public InputControlStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }
    
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    public double ListWidth
    {
        get => GetValue(ListWidthProperty);
        set => SetValue(ListWidthProperty, value);
    }
    
    public double ListHeight
    {
        get => GetValue(ListHeightProperty);
        set => SetValue(ListHeightProperty, value);
    }
    
    [DependsOn(nameof(SourceViewFooterTemplate))]
    public object? SourceViewFooter
    {
        get => GetValue(SourceViewFooterProperty);
        set => SetValue(SourceViewFooterProperty, value);
    }
    
    public IDataTemplate? SourceViewFooterTemplate
    {
        get => GetValue(SourceViewFooterTemplateProperty);
        set => SetValue(SourceViewFooterTemplateProperty, value);
    }
    
    [DependsOn(nameof(TargetViewFooterTemplate))]
    public object? TargetViewFooter
    {
        get => GetValue(TargetViewFooterProperty);
        set => SetValue(TargetViewFooterProperty, value);
    }
    
    public IDataTemplate? TargetViewFooterTemplate
    {
        get => GetValue(TargetViewFooterTemplateProperty);
        set => SetValue(TargetViewFooterTemplateProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    public IIconTemplate? SelectionsIcon
    {
        get => GetValue(SelectionsIconProperty);
        set => SetValue(SelectionsIconProperty, value);
    }
    
    public PathIcon? ToSourceTransferIcon
    {
        get => GetValue(ToSourceTransferIconProperty);
        set => SetValue(ToSourceTransferIconProperty, value);
    }
    
    public PathIcon? ToTargetTransferIcon
    {
        get => GetValue(ToTargetTransferIconProperty);
        set => SetValue(ToTargetTransferIconProperty, value);
    }
    
    public bool IsShowSearch
    {
        get => GetValue(IsShowSearchProperty);
        set => SetValue(IsShowSearchProperty, value);
    }
    
    public bool IsShowSelectAll
    {
        get => GetValue(IsShowSelectAllProperty);
        set => SetValue(IsShowSelectAllProperty, value);
    }
    
    [DependsOn(nameof(SourceTitleTemplate))]
    public object? SourceTitle
    {
        get => GetValue(SourceTitleProperty);
        set => SetValue(SourceTitleProperty, value);
    }
    
    public IDataTemplate? SourceTitleTemplate
    {
        get => GetValue(SourceTitleTemplateProperty);
        set => SetValue(SourceTitleTemplateProperty, value);
    }

    [DependsOn(nameof(TargetTitleTemplate))]
    public object? TargetTitle
    {
        get => GetValue(TargetTitleProperty);
        set => SetValue(TargetTitleProperty, value);
    }
    
    public IDataTemplate? TargetTitleTemplate
    {
        get => GetValue(TargetTitleTemplateProperty);
        set => SetValue(TargetTitleTemplateProperty, value);
    }
    
    public bool IsOneWay
    {
        get => GetValue(IsOneWayProperty);
        set => SetValue(IsOneWayProperty, value);
    }
    
    public IList<EntityKey>? SelectedKeys
    {
        get => GetValue(SelectedKeysProperty);
        set => SetValue(SelectedKeysProperty, value);
    }
    
    public IList<EntityKey>? TargetKeys
    {
        get => GetValue(TargetKeysProperty);
        set => SetValue(TargetKeysProperty, value);
    }

    public bool IsFilterEnabled
    {
        get => GetValue(IsFilterEnabledProperty);
        set => SetValue(IsFilterEnabledProperty, value);
    }
    
    public IValueFilter? Filter
    {
        get => GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }
    
    public DefaultFilterValueSelector? FilterValueSelector
    {
        get => GetValue(FilterValueSelectorProperty);
        set => SetValue(FilterValueSelectorProperty, value);
    }
    
    public string? FilterPlaceholderText
    {
        get => GetValue(FilterPlaceholderTextProperty);
        set => SetValue(FilterPlaceholderTextProperty, value);
    }
    
    public string? ToSourceButtonText
    {
        get => GetValue(ToSourceButtonTextProperty);
        set => SetValue(ToSourceButtonTextProperty, value);
    }
    
    public string? ToTargetButtonText
    {
        get => GetValue(ToTargetButtonTextProperty);
        set => SetValue(ToTargetButtonTextProperty, value);
    }
    
    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }
    
    public bool IsStretchView
    {
        get => GetValue(IsStretchViewProperty);
        set => SetValue(IsStretchViewProperty, value);
    }
    
    #endregion

    #region 公共事件定义
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

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (IsFilterEnabled && Filter == null)
        {
            SetCurrentValue(FilterProperty, ValueFilterFactory.BuildFilter(ValueFilterMode.Contains));
        }
    }

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

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isVisualTreeAttached = false;
        ReleasePinnedOpenDropdown();
        ReleaseSelectedKeysCollectionChangedSource();
        ReleaseTargetKeysCollectionChangedSource();
        base.OnDetachedFromVisualTree(e);
    }

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

    protected void NotifySelectionChanged(IList<EntityKey>? sourceItemKeys, IList<EntityKey>? targetItemKeys)
    {
        SelectionChanged?.Invoke(this, new TransferSelectionChangedEventArgs(sourceItemKeys, targetItemKeys));
    }

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

    protected static List<EntityKey> BuildEntityKeyList(ICollection<EntityKey> keys)
    {
        var keyList = new List<EntityKey>(keys.Count);
        foreach (var key in keys)
        {
            keyList.Add(key);
        }
        return keyList;
    }

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
