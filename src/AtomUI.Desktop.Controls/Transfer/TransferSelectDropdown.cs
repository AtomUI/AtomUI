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
    
    internal static readonly DirectProperty<TransferSelectDropdown, string?> ToggleSelectCurrentPageTextProperty =
        AvaloniaProperty.RegisterDirect<TransferSelectDropdown, string?>(nameof(ToggleSelectCurrentPageText),
            o => o.ToggleSelectCurrentPageText,
            (o, v) => o.ToggleSelectCurrentPageText = v);

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
    
    private string? _toggleSelectCurrentPageText;
    internal string? ToggleSelectCurrentPageText
    {
        get => _toggleSelectCurrentPageText;
        set => SetAndRaise(ToggleSelectCurrentPageTextProperty, ref _toggleSelectCurrentPageText, value);
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
            var toggleSelectCurrentPageMenuItem = new MenuItem()
            {
                Tag = TransferSelectAction.SetCurrentPage
            };
            _disposables.Add(BindUtils.RelayBind(this, ToggleSelectCurrentPageTextProperty, toggleSelectCurrentPageMenuItem, MenuItem.HeaderProperty));
            _disposables.Add(BindUtils.RelayBind(this, IsPaginationEnabledProperty, toggleSelectCurrentPageMenuItem, MenuItem.IsVisibleProperty));
            menuFlyout.Items.Add(selectAllMenuItem);
            menuFlyout.Items.Add(deSelectAllMenuItem);
            menuFlyout.Items.Add(toggleSelectCurrentPageMenuItem);
            
            menuFlyout.MenuItemClicked += HandleMenuItemClicked;
            Flyout = menuFlyout;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (Flyout is MenuFlyout menuFlyout)
        {
            menuFlyout.MenuItemClicked -= HandleMenuItemClicked;
        }
        _disposables?.Dispose();
        _disposables = null;
        Flyout       = null;
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