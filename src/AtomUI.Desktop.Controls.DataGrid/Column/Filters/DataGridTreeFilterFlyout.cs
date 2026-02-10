using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.MotionScene;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace AtomUI.Desktop.Controls;

internal class DataGridTreeFilterFlyout : TreeViewFlyout
{
    internal static readonly StyledProperty<ItemToggleType> ToggleTypeProperty =
        TreeView.ToggleTypeProperty.AddOwner<DataGridTreeFilterFlyout>();

    internal bool IsActiveShutdown = false;

    public ItemToggleType ToggleType
    {
        get => GetValue(ToggleTypeProperty);
        set => SetValue(ToggleTypeProperty, value);
    }

    public event EventHandler<DataGridFilterValuesSelectedEventArgs>? FilterValuesSelected;
    
    private CompositeDisposable? _presenterBindingDisposables;

    public DataGridTreeFilterFlyout()
    {
        OpenMotion  = new SlideUpInMotion();
        CloseMotion = new SlideUpOutMotion();
    }

    protected override Control CreatePresenter()
    {
        var presenter = new DataGridTreeFilterFlyoutPresenter
        {
            IsDefaultExpandAll = true,
            ItemsSource        = Items,
            TreeViewFlyout     = this
        };
        foreach (var item in Items)
        {
            if (item is Control control) 
            {
                control.SetLogicalParent(null);
                control.SetVisualParent(null);
            }
        }
        _presenterBindingDisposables?.Dispose();
        _presenterBindingDisposables = new CompositeDisposable(4);
        
        _presenterBindingDisposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, presenter, DataGridTreeFilterFlyoutPresenter.IsMotionEnabledProperty));
        _presenterBindingDisposables.Add(BindUtils.RelayBind(this, IsShowArrowEffectiveProperty, presenter, DataGridTreeFilterFlyoutPresenter.IsShowArrowProperty));
        _presenterBindingDisposables.Add(BindUtils.RelayBind(this, ArrowPositionProperty, presenter, DataGridTreeFilterFlyoutPresenter.ArrowPositionProperty));
        _presenterBindingDisposables.Add(BindUtils.RelayBind(this, ToggleTypeProperty, presenter, DataGridTreeFilterFlyoutPresenter.ToggleTypeProperty));
        
        ConfigureShowArrowEffective();
        ConfigureArrowPosition();
        
        return presenter;
    }

    protected override void NotifyPopupOpened(object? sender, EventArgs e)
    {
        base.NotifyPopupOpened(sender, e);
        IsActiveShutdown = false;
    }

    protected override void NotifyPopupClosed(object? sender, EventArgs e)
    {
        base.NotifyPopupClosed(sender, e);
        var selectedItems = new List<string>();
        if (Popup.Child is DataGridTreeFilterFlyoutPresenter presenter)
        {
            selectedItems = presenter.GetFilterValues();
        }
        
        NotifyFilterValuesSelected(new DataGridFilterValuesSelectedEventArgs(IsActiveShutdown, selectedItems));
    }

    internal void NotifyFilterValuesSelected(DataGridFilterValuesSelectedEventArgs e)
    {
        FilterValuesSelected?.Invoke(this, e);
    }
}

internal class DataGridFilterTreeItem : TreeItem
{
    public string? FilterValue { get; set; }

    protected override void OnHeaderDoubleTapped(TappedEventArgs e)
    {
        if (ItemCount > 0)
        {
            e.Handled = true;
        }
    }
}