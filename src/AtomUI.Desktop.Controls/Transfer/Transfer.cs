using AtomUI.Controls;
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
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Transfer>();
    
    public static readonly StyledProperty<object?> FooterProperty =
        AvaloniaProperty.Register<Transfer, object?>(nameof(Footer));
    
    public static readonly StyledProperty<IDataTemplate?> FooterTemplateProperty =
        AvaloniaProperty.Register<Transfer, IDataTemplate?>(nameof(FooterTemplate));
    
    public static readonly StyledProperty<IIconTemplate?> SelectionsIconProperty =
        AvaloniaProperty.Register<Transfer, IIconTemplate?>(nameof(SelectionsIcon));
    
    public static readonly StyledProperty<PathIcon?> ToSourceTransferIconProperty =
        AvaloniaProperty.Register<Transfer, PathIcon?>(nameof(ToSourceTransferIcon));
    
    public static readonly StyledProperty<PathIcon?> ToTargetTransferIconProperty =
        AvaloniaProperty.Register<Transfer, PathIcon?>(nameof(ToTargetTransferIcon));
    
    public static readonly StyledProperty<bool> IsPaginationEnabledProperty =
        AvaloniaProperty.Register<Transfer, bool>(nameof(IsPaginationEnabled));
    
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
    
    [DependsOn(nameof(FooterTemplate))]
    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }
    
    public IDataTemplate? FooterTemplate
    {
        get => GetValue(FooterTemplateProperty);
        set => SetValue(FooterTemplateProperty, value);
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

    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => TransferToken.ID;

    #endregion

    private TransferItemDecorator? _sourceView;
    private TransferItemDecorator? _targetView;
    
    static Transfer()
    {
        IconButton.ClickEvent.AddClassHandler<Transfer>((transfer, args) => transfer.HandleTransferRequest(args));
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
        
        _sourceView = e.NameScope.Find<TransferItemDecorator>("SourceDecoratorView");
        _targetView = e.NameScope.Find<TransferItemDecorator>("TargetDecoratorView");
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ItemsSourceProperty ||
            change.Property == TargetKeysProperty)
        {
            ConfigurePanelItemsSource();
        }
    }

    private void ConfigurePanelItemsSource()
    {
        SourcePanelSource = ItemsSource?.Where(item => !(TargetKeys?.Contains(item.ItemKey ?? default) ?? false));
        TargetPanelSource = ItemsSource?.Where(item => TargetKeys?.Contains(item.ItemKey ?? default) ?? false);
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
        
        if (transferDirection == TransferDirection.ToTarget)
        {
            var keys = _sourceView?.SelectedKeys;
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
            var keys = _targetView?.SelectedKeys;
            if (keys != null)
            {
                foreach (var key in keys)
                {
                    currentSet.Remove(key);
                }
                SetCurrentValue(TargetKeysProperty, currentSet.ToList());
            }
        }
    }
}