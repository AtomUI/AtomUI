using AtomUI.Controls;
using AtomUI.Controls.Utils;
using AtomUI.Icons.AntDesign;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

public class Transfer : TemplatedControl,
                        IMotionAwareControl,
                        IInputControlStatusAware,
                        ISizeTypeAware,
                        IControlSharedTokenResourcesHost
{
    #region 公共属性定义
    
    public static readonly StyledProperty<IEnumerable<IItemKey>?> ItemsSourceProperty =
        AvaloniaProperty.Register<Transfer, IEnumerable<IItemKey>?>(nameof(ItemsSource));
    
    public static readonly StyledProperty<InputControlStatus> StatusProperty =
        InputControlStatusProperty.StatusProperty.AddOwner<Transfer>();
    
    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<Transfer>();
    
    public static readonly StyledProperty<double> ListWidthProperty =
        AvaloniaProperty.Register<Transfer, double>(nameof(ListWidth), double.NaN);
    
    public static readonly StyledProperty<double> ListHeightProperty =
        AvaloniaProperty.Register<Transfer, double>(nameof(ListHeight), double.NaN);
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Transfer>();
    
    public static readonly StyledProperty<object?> SourceViewFooterProperty =
        AvaloniaProperty.Register<Transfer, object?>(nameof(SourceViewFooter));
    
    public static readonly StyledProperty<IDataTemplate?> SourceViewFooterTemplateProperty =
        AvaloniaProperty.Register<Transfer, IDataTemplate?>(nameof(SourceViewFooterTemplate));
    
    public static readonly StyledProperty<object?> TargetViewFooterProperty =
        AvaloniaProperty.Register<Transfer, object?>(nameof(TargetViewFooter));
    
    public static readonly StyledProperty<IDataTemplate?> TargetViewFooterTemplateProperty =
        AvaloniaProperty.Register<Transfer, IDataTemplate?>(nameof(TargetViewFooterTemplate));
    
    public static readonly StyledProperty<IIconTemplate?> SelectionsIconProperty =
        AvaloniaProperty.Register<Transfer, IIconTemplate?>(nameof(SelectionsIcon));
    
    public static readonly StyledProperty<PathIcon?> ToSourceTransferIconProperty =
        AvaloniaProperty.Register<Transfer, PathIcon?>(nameof(ToSourceTransferIcon));
    
    public static readonly StyledProperty<PathIcon?> ToTargetTransferIconProperty =
        AvaloniaProperty.Register<Transfer, PathIcon?>(nameof(ToTargetTransferIcon));
    
    public static readonly StyledProperty<bool> IsPaginationEnabledProperty =
        AvaloniaProperty.Register<Transfer, bool>(nameof(IsPaginationEnabled));
    
    public static readonly StyledProperty<int> PageSizeProperty =
        AvaloniaProperty.Register<Transfer, int>(nameof(PageSize), 10);
    
    public static readonly StyledProperty<bool> IsShowSearchProperty =
        AvaloniaProperty.Register<Transfer, bool>(nameof(IsShowSearch));
    
    public static readonly StyledProperty<bool> IsShowSelectAllProperty =
        AvaloniaProperty.Register<Transfer, bool>(nameof(IsShowSelectAll));
    
    public static readonly StyledProperty<object?> SourceTitleProperty =
        AvaloniaProperty.Register<Transfer, object?>(nameof(SourceTitle));
    
    public static readonly StyledProperty<IDataTemplate?> SourceTitleTemplateProperty =
        AvaloniaProperty.Register<Transfer, IDataTemplate?>(nameof(SourceTitleTemplate));
    
    public static readonly StyledProperty<object?> TargetTitleProperty =
        AvaloniaProperty.Register<Transfer, object?>(nameof(TargetTitle));
    
    public static readonly StyledProperty<IDataTemplate?> TargetTitleTemplateProperty =
        AvaloniaProperty.Register<Transfer, IDataTemplate?>(nameof(TargetTitleTemplate));
    
    public static readonly StyledProperty<bool> IsOneWayProperty =
        AvaloniaProperty.Register<Transfer, bool>(nameof(IsOneWay));
    
    public static readonly StyledProperty<IList<EntityKey>?> SelectedKeysProperty =
        AvaloniaProperty.Register<Transfer, IList<EntityKey>?>(nameof(SelectedKeys));
    
    public static readonly StyledProperty<IList<EntityKey>?> TargetKeysProperty =
        AvaloniaProperty.Register<Transfer, IList<EntityKey>?>(nameof(TargetKeys));
    
    public static readonly StyledProperty<object?> SourcePanelProperty =
        AvaloniaProperty.Register<Transfer, object?>(nameof(SourcePanel));
    
    public static readonly StyledProperty<IDataTemplate?> SourcePanelTemplateProperty =
        AvaloniaProperty.Register<Transfer, IDataTemplate?>(nameof(SourcePanelTemplate));
    
    public static readonly StyledProperty<object?> TargetPanelProperty =
        AvaloniaProperty.Register<Transfer, object?>(nameof(TargetPanel));
    
    public static readonly StyledProperty<IDataTemplate?> TargetPanelTemplateProperty =
        AvaloniaProperty.Register<Transfer, IDataTemplate?>(nameof(TargetPanelTemplate));
    
    public static readonly StyledProperty<bool> IsFilterEnabledProperty =
        AvaloniaProperty.Register<Transfer, bool>(nameof(IsFilterEnabled), false);
    
    public static readonly StyledProperty<IValueFilter?> FilterProperty =
        AvaloniaProperty.Register<Transfer, IValueFilter?>(nameof(Filter));
    
    public static readonly StyledProperty<DefaultFilterValueSelector?> FilterValueSelectorProperty =
        AvaloniaProperty.Register<Transfer, DefaultFilterValueSelector?>(
            nameof(FilterValueSelector));
    
    public static readonly StyledProperty<string?> FilterPlaceholderTextProperty =
        AvaloniaProperty.Register<Transfer, string?>(nameof(FilterPlaceholderText));
    
    public static readonly StyledProperty<string?> ToSourceButtonTextProperty =
        AvaloniaProperty.Register<Transfer, string?>(nameof(ToSourceButtonText));
    
    public static readonly StyledProperty<string?> ToTargetButtonTextProperty =
        AvaloniaProperty.Register<Transfer, string?>(nameof(ToTargetButtonText));
    
    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        ItemsControl.ItemTemplateProperty.AddOwner<Transfer>();
    
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
    
    public SizeType SizeType
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
    
    public bool IsPaginationEnabled
    {
        get => GetValue(IsPaginationEnabledProperty);
        set => SetValue(IsPaginationEnabledProperty, value);
    }
    
    public int PageSize
    {
        get => GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
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
    
    [DependsOn(nameof(SourcePanelTemplate))]
    public object? SourcePanel
    {
        get => GetValue(SourcePanelProperty);
        set => SetValue(SourcePanelProperty, value);
    }
    
    public IDataTemplate? SourcePanelTemplate
    {
        get => GetValue(SourcePanelTemplateProperty);
        set => SetValue(SourcePanelTemplateProperty, value);
    }
    
    [DependsOn(nameof(TargetPanelTemplate))]
    public object? TargetPanel
    {
        get => GetValue(TargetPanelProperty);
        set => SetValue(TargetPanelProperty, value);
    }
    
    public IDataTemplate? TargetPanelTemplate
    {
        get => GetValue(TargetPanelTemplateProperty);
        set => SetValue(TargetPanelTemplateProperty, value);
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
    
    #endregion

    #region 公共事件定义
    public event EventHandler<TransferSelectionChangedEventArgs>? SelectionChanged;
    #endregion

    #region 内部属性定义
    
    internal static readonly StyledProperty<IEnumerable<IItemKey>?> SourcePanelSourceProperty =
        AvaloniaProperty.Register<Transfer, IEnumerable<IItemKey>?>(nameof(SourcePanelSource));
    
    internal static readonly StyledProperty<IEnumerable<IItemKey>?> TargetPanelSourceProperty =
        AvaloniaProperty.Register<Transfer, IEnumerable<IItemKey>?>(nameof(TargetPanelSource));
    
    internal static readonly DirectProperty<Transfer, bool> ToTargetButtonEnabledProperty =
        AvaloniaProperty.RegisterDirect<Transfer, bool>(nameof(ToTargetButtonEnabled),
            o => o.ToTargetButtonEnabled,
            (o, v) => o.ToTargetButtonEnabled = v);
    
    internal static readonly DirectProperty<Transfer, bool> ToSourceButtonEnabledProperty =
        AvaloniaProperty.RegisterDirect<Transfer, bool>(nameof(ToSourceButtonEnabled),
            o => o.ToSourceButtonEnabled,
            (o, v) => o.ToSourceButtonEnabled = v);
    
    internal static readonly DirectProperty<Transfer, string?> SourceFilterValueProperty =
        AvaloniaProperty.RegisterDirect<Transfer, string?>(nameof(SourceFilterValue),
            o => o.SourceFilterValue,
            (o, v) => o.SourceFilterValue = v);
    
    internal static readonly DirectProperty<Transfer, string?> TargetFilterValueProperty =
        AvaloniaProperty.RegisterDirect<Transfer, string?>(nameof(TargetFilterValue),
            o => o.TargetFilterValue,
            (o, v) => o.TargetFilterValue = v);
    
    internal IEnumerable<IItemKey>? SourcePanelSource
    {
        get => GetValue(SourcePanelSourceProperty);
        set => SetValue(SourcePanelSourceProperty, value);
    }
    
    internal IEnumerable<IItemKey>? TargetPanelSource
    {
        get => GetValue(TargetPanelSourceProperty);
        set => SetValue(TargetPanelSourceProperty, value);
    }
    
    private bool _toTargetButtonEnabled;
    internal bool ToTargetButtonEnabled
    {
        get => _toTargetButtonEnabled;
        set => SetAndRaise(ToTargetButtonEnabledProperty, ref _toTargetButtonEnabled, value);
    }
    
    private bool _toSourceButtonEnabled;
    internal bool ToSourceButtonEnabled
    {
        get => _toSourceButtonEnabled;
        set => SetAndRaise(ToSourceButtonEnabledProperty, ref _toSourceButtonEnabled, value);
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
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => TransferToken.ID;

    #endregion

    private TransferItemDecorator? _sourceViewDecorator;
    private TransferItemDecorator? _targetViewDecorator;
    private ITransferView? _sourceView;
    private ITransferView? _targetView;
    
    static Transfer()
    {
        IconButton.ClickEvent.AddClassHandler<Transfer>((transfer, args) => transfer.HandleTransferRequest(args));
        TransferSelectDropdown.SelectActionRequestEvent.AddClassHandler<Transfer>((transfer, args) => transfer.HandleSelectActionRequest(args));
        LineEdit.TextChangedEvent.AddClassHandler<Transfer>((transfer, args) => transfer.HandleTransferFilterChanged(args));
    }
    
    public Transfer()
    {
        this.RegisterResources();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (ToSourceTransferIcon == null)
        {
            SetCurrentValue(ToSourceTransferIconProperty, new LeftOutlined());
        }
        if (ToTargetTransferIcon == null)
        {
            SetCurrentValue(ToTargetTransferIconProperty, new RightOutlined());
        }
        if (SourcePanel == null)
        {
            SetCurrentValue(SourcePanelProperty, new TransferListView());
        }

        if (TargetPanel == null)
        {
            SetCurrentValue(TargetPanelProperty, new TransferListView());
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
            _targetView.ItemRemoved -= HandleItemRemoved;
        }
        if (_sourceView != null)
        {
            _sourceView.ItemRemoved -= HandleItemRemoved;
        }
        _sourceViewDecorator = e.NameScope.Find<TransferItemDecorator>("SourceDecoratorView");
        _targetViewDecorator = e.NameScope.Find<TransferItemDecorator>("TargetDecoratorView");
        
        if (_sourceViewDecorator != null)
        {
            _sourceViewDecorator.TransferViewCreated += HandleTransferViewCreated;
        }
        if (_targetViewDecorator != null)
        {
            _targetViewDecorator.TransferViewCreated += HandleTransferViewCreated;
        }
    }

    private void HandleTransferViewCreated(object? sender, TransferViewCreatedEventArgs args)
    {
        if (args.TransferView.ViewType == TransferViewType.Source)
        {
            _sourceView = args.TransferView;
        }
        else
        {
            _targetView                 =  args.TransferView;
            _targetView.ItemRemoved     += HandleItemRemoved;
        }
    }

    private void HandleItemRemoved(object? sender, TransferItemRemovedEventArgs args)
    {
        var currentSet = new HashSet<EntityKey>();
        if (TargetKeys != null)
        {
            foreach (var targetKey in TargetKeys)
            {
                currentSet.Add(targetKey);
            }
        }
        _sourceViewDecorator?.NotifyAboutToTransfer(TransferDirection.ToSource);
        currentSet.Remove(args.Item?.ItemKey ?? default);
        SetCurrentValue(TargetKeysProperty, currentSet.ToList());
        _sourceViewDecorator?.NotifyTransferCompleted(TransferDirection.ToSource);
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
                 change.Property == ItemsSourceProperty ||
                 change.Property == TargetKeysProperty)
        {
            ConfigurePanelItemsSourceForFilter(FilterChangeType.Both);
        }
    }

    private void ConfigurePanelItemsSourceForFilter(FilterChangeType changeType)
    {
        if (changeType.HasFlag(FilterChangeType.Source))
        {
            SourcePanelSource = ItemsSource?
                .Where(item => !(TargetKeys?.Contains(item.ItemKey ?? default) ?? false))
                .Where(item => !IsFilterEnabled || string.IsNullOrEmpty(SourceFilterValue) || 
                               (Filter?.Filter(FilterValueSelector != null ? FilterValueSelector(item) : item,
                                   SourceFilterValue) ?? false))
                .ToArray();
        }

        if (changeType.HasFlag(FilterChangeType.Target))
        {
            TargetPanelSource = ItemsSource?
                                .Where(item => TargetKeys?.Contains(item.ItemKey ?? default) ?? false)
                                .Where(item => !IsFilterEnabled || string.IsNullOrEmpty(TargetFilterValue) || 
                                               (Filter?.Filter(FilterValueSelector != null ? FilterValueSelector(item) : item, TargetFilterValue) ?? false))
                                .ToArray();
        }
    }
    
    private void HandleTransferRequest(RoutedEventArgs args)
    {
        if (args.Source is Button button && button.Tag is TransferDirection transferDirection)
        {
            TransferItems(transferDirection);
        }
        args.Handled = true;
    }

    private void TransferItems(TransferDirection transferDirection)
    {
        var currentSet = new HashSet<EntityKey>();
        if (TargetKeys != null)
        {
            foreach (var targetKey in TargetKeys)
            {
                currentSet.Add(targetKey);
            }
        }
        _sourceViewDecorator?.NotifyAboutToTransfer(transferDirection);
        _targetViewDecorator?.NotifyAboutToTransfer(transferDirection);
        if (transferDirection == TransferDirection.ToTarget)
        {
            var keys = _sourceViewDecorator?.SelectedKeys;
            if (keys != null)
            {
                foreach (var key in keys)
                {
                    currentSet.Add(key);
                }
                SetCurrentValue(TargetKeysProperty, currentSet.ToList());
            }
        }
        else
        {
            var keys = _targetViewDecorator?.SelectedKeys;
            if (keys != null)
            {
                foreach (var key in keys)
                {
                    currentSet.Remove(key);
                }
                SetCurrentValue(TargetKeysProperty, currentSet.ToList());
            }
        }
        _sourceViewDecorator?.NotifyTransferCompleted(transferDirection);
        _targetViewDecorator?.NotifyTransferCompleted(transferDirection);
    }
    
    private void HandleSelectActionRequest(TransferSelectActionEventArgs args)
    {
        if (args.Action == TransferSelectAction.RemoveAll)
        {
            _sourceViewDecorator?.NotifyAboutToTransfer(TransferDirection.ToSource);
            SetCurrentValue(TargetKeysProperty, null);
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
    
    [Flags]
    private enum FilterChangeType
    {
        Source = 0x01,
        Target = 0x02,
        Both = Source | Target
    }
}