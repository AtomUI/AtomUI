using System.Reactive.Disposables;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

internal class TransferSelectDropdown : IconButton
{
    #region 公共属性定义

    public static readonly DirectProperty<TransferSelectDropdown, bool> IsAllSelectedProperty =
        AvaloniaProperty.RegisterDirect<TransferSelectDropdown, bool>(nameof(IsAllSelected),
            o => o.IsAllSelected,
            (o, v) => o.IsAllSelected = v);
    
    public static readonly StyledProperty<bool> IsPaginationEnabledProperty =
        Transfer.IsPaginationEnabledProperty.AddOwner<TransferSelectDropdown>();
    
    private bool _isAllSelected;
    public bool IsAllSelected
    {
        get => _isAllSelected;
        set => SetAndRaise(IsAllSelectedProperty, ref _isAllSelected, value);
    }

    public bool IsPaginationEnabled
    {
        get => GetValue(IsPaginationEnabledProperty);
        set => SetValue(IsPaginationEnabledProperty, value);
    }
    #endregion

    #region 公共事件定义

    public static readonly RoutedEvent<TransferSelectActionEventArgs> SelectActionRequestEvent =
        RoutedEvent.Register<TransferSelectDropdown, TransferSelectActionEventArgs>(
            nameof(SelectActionRequest),
            RoutingStrategies.Bubble);

    public event EventHandler<TransferSelectActionEventArgs>? SelectActionRequest
    {
        add => AddHandler(SelectActionRequestEvent, value);
        remove => RemoveHandler(SelectActionRequestEvent, value);
    }
    #endregion

    #region 内部属性定义
    internal static readonly DirectProperty<TransferSelectDropdown, string?> SelectAllTextProperty =
        AvaloniaProperty.RegisterDirect<TransferSelectDropdown, string?>(nameof(SelectAllText),
            o => o.SelectAllText,
            (o, v) => o.SelectAllText = v);
    
    internal static readonly DirectProperty<TransferSelectDropdown, string?> DeSelectAllTextProperty =
        AvaloniaProperty.RegisterDirect<TransferSelectDropdown, string?>(nameof(DeSelectAllText),
            o => o.DeSelectAllText,
            (o, v) => o.DeSelectAllText = v);
    
    internal static readonly DirectProperty<TransferSelectDropdown, string?> InvertSelectCurrentPageTextProperty =
        AvaloniaProperty.RegisterDirect<TransferSelectDropdown, string?>(nameof(InvertSelectCurrentPageText),
            o => o.InvertSelectCurrentPageText,
            (o, v) => o.InvertSelectCurrentPageText = v);
    
    internal static readonly DirectProperty<TransferSelectDropdown, string?> SelectCurrentPageTextProperty =
        AvaloniaProperty.RegisterDirect<TransferSelectDropdown, string?>(nameof(SelectCurrentPageText),
            o => o.SelectCurrentPageText,
            (o, v) => o.SelectCurrentPageText = v);
    
    internal static readonly DirectProperty<TransferSelectDropdown, string?> RemoveAllTextProperty =
        AvaloniaProperty.RegisterDirect<TransferSelectDropdown, string?>(nameof(RemoveAllText),
            o => o.RemoveAllText,
            (o, v) => o.RemoveAllText = v);
    
    internal static readonly StyledProperty<bool> IsOneWayProperty =
        Transfer.IsOneWayProperty.AddOwner<TransferSelectDropdown>();
    
    internal static readonly DirectProperty<TransferSelectDropdown, TransferViewType> ViewTypeProperty =
        AvaloniaProperty.RegisterDirect<TransferSelectDropdown, TransferViewType>(nameof(ViewType),
            o => o.ViewType,
            (o, v) => o.ViewType = v);

    private string? _selectAllText;
    internal string? SelectAllText
    {
        get => _selectAllText;
        set => SetAndRaise(SelectAllTextProperty, ref _selectAllText, value);
    }
    
    private string? _deSelectAllText;
    internal string? DeSelectAllText
    {
        get => _deSelectAllText;
        set => SetAndRaise(DeSelectAllTextProperty, ref _deSelectAllText, value);
    }
    
    private string? _invertSelectCurrentPageText;
    internal string? InvertSelectCurrentPageText
    {
        get => _invertSelectCurrentPageText;
        set => SetAndRaise(InvertSelectCurrentPageTextProperty, ref _invertSelectCurrentPageText, value);
    }
    
    private string? _selectCurrentPageText;
    internal string? SelectCurrentPageText
    {
        get => _selectCurrentPageText;
        set => SetAndRaise(SelectCurrentPageTextProperty, ref _selectCurrentPageText, value);
    }
    
    private string? _removeAllText;
    internal string? RemoveAllText
    {
        get => _removeAllText;
        set => SetAndRaise(RemoveAllTextProperty, ref _removeAllText, value);
    }
    
    internal bool IsOneWay
    {
        get => GetValue(IsOneWayProperty);
        set => SetValue(IsOneWayProperty, value);
    }

    private TransferViewType _viewType;
    internal TransferViewType ViewType
    {
        get => _viewType;
        set => SetAndRaise(ViewTypeProperty, ref _viewType, value);
    }
    #endregion

    private CompositeDisposable? _disposables;

    protected override void OnClick()
    {
        NotifyCreateFlyout();
        base.OnClick();
    }

    private void NotifyCreateFlyout()
    {
        if (Flyout == null)
        {
            _disposables?.Dispose();
            _disposables = new CompositeDisposable();
            var menuFlyout = new MenuFlyout
            {
                Placement = PlacementMode.BottomEdgeAlignedLeft,
            };
            
            _disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, menuFlyout, MenuFlyout.IsMotionEnabledProperty));

            if (ViewType == TransferViewType.Source || !IsOneWay)
            {
                var selectAllMenuItem = new MenuItem()
                {
                    Tag = TransferSelectAction.SelectAll
                };
                _disposables.Add(BindUtils.RelayBind(this, SelectAllTextProperty, selectAllMenuItem, MenuItem.HeaderProperty));
                _disposables.Add(BindUtils.RelayBind(this, IsAllSelectedProperty, selectAllMenuItem, MenuItem.IsVisibleProperty,
                    converter: value => !value));
                var deSelectAllMenuItem = new MenuItem()
                {
                    Tag = TransferSelectAction.DeselectAll
                };
                _disposables.Add(BindUtils.RelayBind(this, DeSelectAllTextProperty, deSelectAllMenuItem, MenuItem.HeaderProperty));
                _disposables.Add(BindUtils.RelayBind(this, IsAllSelectedProperty, deSelectAllMenuItem, MenuItem.IsVisibleProperty));
               
                var invertSelectCurrentPageMenuItem = new MenuItem()
                {
                    Tag = TransferSelectAction.InvertSelectCurrentPage
                };
                _disposables.Add(BindUtils.RelayBind(this, InvertSelectCurrentPageTextProperty, invertSelectCurrentPageMenuItem, MenuItem.HeaderProperty));
                
                var selectCurrentPageMenuItem = new MenuItem()
                {
                    Tag = TransferSelectAction.SelectCurrentPage
                };
                _disposables.Add(BindUtils.RelayBind(this, SelectCurrentPageTextProperty, selectCurrentPageMenuItem, MenuItem.HeaderProperty));
                _disposables.Add(BindUtils.RelayBind(this, IsPaginationEnabledProperty, selectCurrentPageMenuItem, MenuItem.IsVisibleProperty));
                
                menuFlyout.Items.Add(selectAllMenuItem);
                menuFlyout.Items.Add(deSelectAllMenuItem);
                menuFlyout.Items.Add(selectCurrentPageMenuItem);
                menuFlyout.Items.Add(invertSelectCurrentPageMenuItem);
            }
            else if (ViewType == TransferViewType.Target && IsOneWay)
            {
                var removeAllData = new MenuItem
                {
                    Tag = TransferSelectAction.RemoveAll
                };
                _disposables.Add(BindUtils.RelayBind(this, RemoveAllTextProperty, removeAllData, MenuItem.HeaderProperty));
                menuFlyout.Items.Add(removeAllData);
            }
            
            menuFlyout.MenuItemClicked += HandleMenuItemClicked;
            Flyout = menuFlyout;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsOneWayProperty ||
            change.Property == ViewTypeProperty)
        {
            ResetFlyout();
        }
    }

    private void ResetFlyout()
    {
        if (Flyout is MenuFlyout menuFlyout)
        {
            menuFlyout.MenuItemClicked -= HandleMenuItemClicked;
        }
        _disposables?.Dispose();
        _disposables = null;
        Flyout       = null;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        ResetFlyout();
    }

    private void HandleMenuItemClicked(object? sender, FlyoutMenuItemClickedEventArgs args)
    {
        if (args.Item.Tag is TransferSelectAction action)
        {
            RaiseEvent(new TransferSelectActionEventArgs(action)
            {
                RoutedEvent = SelectActionRequestEvent,
                Source = this
            });
        }
    }
}