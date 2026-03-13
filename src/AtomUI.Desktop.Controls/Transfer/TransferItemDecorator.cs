using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

internal class TransferItemDecorator : TemplatedControl,
                                       IMotionAwareControl,
                                       IInputControlStatusAware,
                                       ISizeTypeAware
{
    #region 公共属性定义

    public static readonly StyledProperty<object?> TitleProperty =
        AvaloniaProperty.Register<TransferItemDecorator, object?>(nameof(Title));
    
    public static readonly StyledProperty<IDataTemplate?> TitleTemplateProperty =
        AvaloniaProperty.Register<TransferItemDecorator, IDataTemplate?>(nameof(TitleTemplate));
    
    public static readonly StyledProperty<object?> ContentProperty =
        ContentControl.ContentProperty.AddOwner<TransferItemDecorator>();

    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        ContentControl.ContentTemplateProperty.AddOwner<TransferItemDecorator>();

    public static readonly StyledProperty<IEnumerable<IItemKey>?> ItemsSourceProperty =
        Transfer.ItemsSourceProperty.AddOwner<TransferItemDecorator>();
    
    public static readonly StyledProperty<InputControlStatus> StatusProperty =
        InputControlStatusProperty.StatusProperty.AddOwner<TransferItemDecorator>();
    
    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<TransferItemDecorator>();
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<TransferItemDecorator>();
    
    public static readonly StyledProperty<bool> IsShowSelectAllCheckboxProperty =
        AvaloniaProperty.Register<TransferItemDecorator, bool>(nameof(IsShowSelectAllCheckbox), true);
    
    public static readonly StyledProperty<bool> IsShowSelectDropdownMenuProperty =
        AvaloniaProperty.Register<TransferItemDecorator, bool>(nameof(IsShowSelectDropdownMenu), true);
    
    public static readonly StyledProperty<bool> IsPaginationEnabledProperty =
        Transfer.IsPaginationEnabledProperty.AddOwner<TransferItemDecorator>();
    
    public static readonly StyledProperty<IIconTemplate?> SelectionsIconTemplateProperty =
        AvaloniaProperty.Register<TransferItemDecorator, IIconTemplate?>(nameof(SelectionsIconTemplate));
    
    public static readonly StyledProperty<bool> IsOneWayProperty =
        Transfer.IsOneWayProperty.AddOwner<TransferItemDecorator>();
    
    public static readonly StyledProperty<string?> FilterPlaceholderTextProperty =
        AvaloniaProperty.Register<TransferItemDecorator, string?>(nameof(FilterPlaceholderText));
    
    public static readonly StyledProperty<object?> FooterProperty =
        AvaloniaProperty.Register<ContentControl, object?>(nameof(Footer));

    public static readonly StyledProperty<IDataTemplate?> FooterTemplateProperty =
        AvaloniaProperty.Register<ContentControl, IDataTemplate?>(nameof(FooterTemplate));
    
    public static readonly StyledProperty<double> ListHeightProperty =
        Transfer.ListHeightProperty.AddOwner<TransferItemDecorator>();
    
    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        ItemsControl.ItemTemplateProperty.AddOwner<TransferItemDecorator>();
    
    [DependsOn(nameof(TitleTemplate))]
    public object? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    
    public IDataTemplate? TitleTemplate
    {
        get => GetValue(TitleTemplateProperty);
        set => SetValue(TitleTemplateProperty, value);
    }
    
    [DependsOn(nameof(ContentTemplate))]
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public IDataTemplate? ContentTemplate
    {
        get => GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }
    
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
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    public bool IsShowSelectAllCheckbox
    {
        get => GetValue(IsShowSelectAllCheckboxProperty);
        set => SetValue(IsShowSelectAllCheckboxProperty, value);
    }
    
    public bool IsShowSelectDropdownMenu
    {
        get => GetValue(IsShowSelectDropdownMenuProperty);
        set => SetValue(IsShowSelectDropdownMenuProperty, value);
    }
    
    public bool IsPaginationEnabled
    {
        get => GetValue(IsPaginationEnabledProperty);
        set => SetValue(IsPaginationEnabledProperty, value);
    }
    
    public IIconTemplate? SelectionsIconTemplate
    {
        get => GetValue(SelectionsIconTemplateProperty);
        set => SetValue(SelectionsIconTemplateProperty, value);
    }
    
    public bool IsOneWay
    {
        get => GetValue(IsOneWayProperty);
        set => SetValue(IsOneWayProperty, value);
    }
    
    public string? FilterPlaceholderText
    {
        get => GetValue(FilterPlaceholderTextProperty);
        set => SetValue(FilterPlaceholderTextProperty, value);
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
    
    public double ListHeight
    {
        get => GetValue(ListHeightProperty);
        set => SetValue(ListHeightProperty, value);
    }
    
    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }
    
    public IList<EntityKey>? SelectedKeys => (_transferView as ITransferView)?.SelectedKeys;
    
    #endregion

    #region 公共事件定义

    public event EventHandler<TransferViewCreatedEventArgs>? TransferViewCreated;

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<TransferItemDecorator, string?> SelectedMessageProperty =
        AvaloniaProperty.RegisterDirect<TransferItemDecorator, string?>(nameof(SelectedMessage),
            o => o.SelectedMessage,
            (o, v) => o.SelectedMessage = v);
    
    internal static readonly DirectProperty<TransferItemDecorator, int> SelectedCountProperty =
        AvaloniaProperty.RegisterDirect<TransferItemDecorator, int>(nameof(SelectedCount),
            o => o.SelectedCount,
            (o, v) => o.SelectedCount = v);
    
    internal static readonly DirectProperty<TransferItemDecorator, bool> HasSelectedProperty =
        AvaloniaProperty.RegisterDirect<TransferItemDecorator, bool>(nameof(HasSelected),
            o => o.HasSelected,
            (o, v) => o.HasSelected = v);
    
    internal static readonly DirectProperty<TransferItemDecorator, int> ItemCountProperty =
        AvaloniaProperty.RegisterDirect<TransferItemDecorator, int>(nameof(ItemCount),
            o => o.ItemCount,
            (o, v) => o.ItemCount = v);
    
    internal static readonly DirectProperty<TransferItemDecorator, bool> IsItemsSourceEmptyProperty =
        AvaloniaProperty.RegisterDirect<TransferItemDecorator, bool>(nameof(IsItemsSourceEmpty),
            o => o.IsItemsSourceEmpty,
            (o, v) => o.IsItemsSourceEmpty = v);
    
    internal static readonly DirectProperty<TransferItemDecorator, string?> ItemUintProperty =
        AvaloniaProperty.RegisterDirect<TransferItemDecorator, string?>(nameof(ItemUint),
            o => o.ItemUint,
            (o, v) => o.ItemUint = v);
    
    internal static readonly DirectProperty<TransferItemDecorator, string?> ItemUintPluralProperty =
        AvaloniaProperty.RegisterDirect<TransferItemDecorator, string?>(nameof(ItemUintPlural),
            o => o.ItemUintPlural,
            (o, v) => o.ItemUintPlural = v);
    
    internal static readonly DirectProperty<TransferItemDecorator, bool?> IsAllSelectedProperty =
        AvaloniaProperty.RegisterDirect<TransferItemDecorator, bool?>(nameof(IsAllSelected),
            o => o.IsAllSelected,
            (o, v) => o.IsAllSelected = v);
    
    internal static readonly DirectProperty<TransferItemDecorator, PathIcon?> SelectionsIconProperty =
        AvaloniaProperty.RegisterDirect<TransferItemDecorator, PathIcon?>(nameof(SelectionsIcon),
            o => o.SelectionsIcon,
            (o, v) => o.SelectionsIcon = v);
    
    internal static readonly DirectProperty<TransferItemDecorator, TransferViewType> ViewTypeProperty =
        AvaloniaProperty.RegisterDirect<TransferItemDecorator, TransferViewType>(nameof(ViewType),
            o => o.ViewType,
            (o, v) => o.ViewType = v);
    
    internal static readonly DirectProperty<TransferItemDecorator, bool> IsFilterEnabledProperty =
        AvaloniaProperty.RegisterDirect<TransferItemDecorator, bool>(nameof(IsFilterEnabled),
            o => o.IsFilterEnabled,
            (o, v) => o.IsFilterEnabled = v);
    
    internal static readonly DirectProperty<TransferItemDecorator, CornerRadius> BodyCornerRadiusProperty =
        AvaloniaProperty.RegisterDirect<TransferItemDecorator, CornerRadius>(nameof(BodyCornerRadius),
            o => o.BodyCornerRadius,
            (o, v) => o.BodyCornerRadius = v);

    private string? _selectedMessage;
    internal string? SelectedMessage
    {
        get => _selectedMessage;
        set => SetAndRaise(SelectedMessageProperty, ref _selectedMessage, value);
    }
    
    private int _selectedCount;
    internal int SelectedCount
    {
        get => _selectedCount;
        set => SetAndRaise(SelectedCountProperty, ref _selectedCount, value);
    }
    
    private bool _hasSelected;
    internal bool HasSelected
    {
        get => _hasSelected;
        set => SetAndRaise(HasSelectedProperty, ref _hasSelected, value);
    }
    
    private int _itemCount;
    internal int ItemCount
    {
        get => _itemCount;
        set => SetAndRaise(ItemCountProperty, ref _itemCount, value);
    }
       
    private bool _isItemsSourceEmpty = true;
    internal bool IsItemsSourceEmpty
    {
        get => _isItemsSourceEmpty;
        set => SetAndRaise(IsItemsSourceEmptyProperty, ref _isItemsSourceEmpty, value);
    }
    
    private string? _itemUint;
    internal string? ItemUint
    {
        get => _itemUint;
        set => SetAndRaise(ItemUintProperty, ref _itemUint, value);
    }
    
    private string? _itemUintPlural;
    internal string? ItemUintPlural
    {
        get => _itemUintPlural;
        set => SetAndRaise(ItemUintPluralProperty, ref _itemUintPlural, value);
    }
    
    private bool? _isAllSelected = false;
    internal bool? IsAllSelected
    {
        get => _isAllSelected;
        set => SetAndRaise(IsAllSelectedProperty, ref _isAllSelected, value);
    }
    
    private PathIcon? _selectionsIcon;
    internal PathIcon? SelectionsIcon
    {
        get => _selectionsIcon;
        set => SetAndRaise(SelectionsIconProperty, ref _selectionsIcon, value);
    }
    
    private TransferViewType _viewType;
    internal TransferViewType ViewType
    {
        get => _viewType;
        set => SetAndRaise(ViewTypeProperty, ref _viewType, value);
    }
    
    private bool _isFilterEnabled;
    internal bool IsFilterEnabled
    {
        get => _isFilterEnabled;
        set => SetAndRaise(IsFilterEnabledProperty, ref _isFilterEnabled, value);
    }
       
    private CornerRadius _bodyCornerRadius;
    internal CornerRadius BodyCornerRadius
    {
        get => _bodyCornerRadius;
        set => SetAndRaise(BodyCornerRadiusProperty, ref _bodyCornerRadius, value);
    }
    #endregion
    
    private ContentPresenter? _contentPresenter;
    private Control? _transferView;
    private CompositeDisposable? _disposables;

    static TransferItemDecorator()
    {
        AffectsMeasure<TransferItemDecorator>(ItemUintProperty, ItemUintPluralProperty);
        TransferSelectDropdown.SelectActionRequestEvent.AddClassHandler<TransferItemDecorator>((decorator, args) => decorator.HandleSelectActionRequest(args));
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        ConfigureSelectedMessage();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ItemsSourceProperty)
        {
            if (_transferView is ITransferView transferView)
            {
                transferView.ItemsSource = ItemsSource;
            }
        }

        if (change.Property == SelectedCountProperty ||
            change.Property == ItemCountProperty ||
            change.Property == ItemUintProperty ||
            change.Property == ItemUintPluralProperty)
        {
            ConfigureSelectedMessage();
        }
        else if (change.Property == SelectionsIconTemplateProperty)
        {
            SelectionsIcon = SelectionsIconTemplate?.Build();
        }
        else if (change.Property == IsAllSelectedProperty)
        {
            if (_transferView is ITransferView transferView)
            {
                if (IsAllSelected == true)
                {
                    transferView.SelectAll();
                }
                else if (IsAllSelected == false)
                {
                    transferView.DeselectAll();
                }
            }
        }

        if (change.Property == ViewTypeProperty ||
            change.Property == IsOneWayProperty)
        {
            ConfigureTransferViewSelectionMode();
        }

        if (change.Property == CornerRadiusProperty ||
            change.Property == FooterProperty ||
            change.Property == FooterTemplateProperty)
        {
            ConfigureBodyCornerRadius();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_contentPresenter != null)
        {
            _contentPresenter.PropertyChanged -= HandleContentPresenterPropertyChanged;
        }

        _contentPresenter = e.NameScope.Find<ContentPresenter>("ContentPresenter");
        if (_contentPresenter != null)
        {
            _contentPresenter.PropertyChanged += HandleContentPresenterPropertyChanged;
        }
    }

    private void HandleContentPresenterPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ContentPresenter.ChildProperty)
        {
            if (e.NewValue != null && e.NewValue is not ITransferView)
            {
                throw new Exception("Transfer panel must implement ITransferItem.");
            }

            {
                if (_transferView is ITransferView transferView)
                {
                    transferView.ItemsSource        =  ItemsSource;
                    transferView.ItemCountChanged   -= HandleItemsCountChanged;
                    transferView.SelectedKeyChanged -= HandleSelectedChanged;
                    _disposables?.Dispose();
                    _disposables = null;
                }
            }
            _transferView = e.NewValue as Control;
            {
                if (_transferView is ITransferView transferView)
                {
                    _disposables                    =  new CompositeDisposable(2);
                    transferView.ItemsSource        =  ItemsSource;
                    transferView.ItemCountChanged   += HandleItemsCountChanged;
                    transferView.SelectedKeyChanged += HandleSelectedChanged;
                    transferView.ViewType           =  ViewType;
                    TransferViewCreated?.Invoke(this, new TransferViewCreatedEventArgs(transferView));
                    ConfigureTransferViewSelectionMode();
                    if (transferView.IsSupportItemTemplate)
                    {
                        _disposables.Add(BindUtils.RelayBind(this, ItemTemplateProperty, _transferView, ItemTemplateProperty));
                    }
                }
            }
        }
    }

    private void HandleItemsCountChanged(object? sender, ItemCountChangedEventArgs args)
    {
        SetCurrentValue(ItemCountProperty, args.ItemCount);
        SetCurrentValue(IsItemsSourceEmptyProperty, args.ItemCount == 0);
    }

    private void HandleSelectedChanged(object? sender, EventArgs args)
    {
        if (_transferView is ITransferView transferView)
        {
            var selectedCount = transferView.SelectedKeys?.Count ?? 0;
            SetCurrentValue(SelectedCountProperty, selectedCount);
            SetCurrentValue(HasSelectedProperty, selectedCount > 0);
            if (ItemCount > 0 && selectedCount == ItemCount)
            {
                SetCurrentValue(IsAllSelectedProperty, true);
            }
            else if (selectedCount != 0)
            {
                SetCurrentValue(IsAllSelectedProperty, null);
            }
            else
            {
                SetCurrentValue(IsAllSelectedProperty, false);
            }
        }
    }

    private void ConfigureSelectedMessage()
    {
        if (ItemCount <= 1)
        {
            if (SelectedCount != 0)
            {
                SelectedMessage = $"{SelectedCount}/{ItemCount} {ItemUint}";
            }
            else
            {
                SelectedMessage = $"{ItemCount} {ItemUint}";
            }
        }
        else
        {
            if (SelectedCount != 0)
            {
                SelectedMessage = $"{SelectedCount}/{ItemCount} {ItemUintPlural}";
            }
            else
            {
                SelectedMessage = $"{ItemCount} {ItemUintPlural}";
            }
        }
    }
    
    private void HandleSelectActionRequest(TransferSelectActionEventArgs args)
    {
        if (args.Action == TransferSelectAction.SelectAll)
        {
            SetCurrentValue(IsAllSelectedProperty, true);
        }
        else if (args.Action == TransferSelectAction.DeselectAll)
        {
            SetCurrentValue(IsAllSelectedProperty, false);
        }
    }

    public void NotifyAboutToTransfer(TransferDirection transferDirection)
    {
        if (_transferView is ITransferView transferView)
        {
            transferView.NotifyAboutToTransfer(transferDirection);
        }
    }

    public void NotifyTransferCompleted(TransferDirection transferDirection)
    {
        if (_transferView is ITransferView transferView)
        {
            transferView.NotifyTransferCompleted(transferDirection);
        }
    }

    private void ConfigureTransferViewSelectionMode()
    {
        if (_transferView is ITransferView transferView)
        {
            if (ViewType == TransferViewType.Target)
            {
                transferView.SetSelectionEnabled(!IsOneWay);
            }
        }
    }

    private void ConfigureBodyCornerRadius()
    {
        BodyCornerRadius = (Footer != null || FooterTemplate != null)
            ? new CornerRadius(0)
            : new CornerRadius(0, 0, CornerRadius.BottomRight, CornerRadius.BottomLeft);
    }
}