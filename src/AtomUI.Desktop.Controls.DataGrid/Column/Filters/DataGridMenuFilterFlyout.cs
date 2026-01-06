using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.MotionScene;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

internal class DataGridMenuFilterFlyout : MenuFlyout
{
    public event EventHandler<DataGridFilterValuesSelectedEventArgs>? FilterValuesSelected;
    internal bool IsActiveShutdown = false;
    private CompositeDisposable? _presenterBindingDisposables;
    
    public DataGridMenuFilterFlyout()
    {
        OpenMotion  = new SlideUpInMotion();
        CloseMotion = new SlideUpOutMotion();
    }
    
    protected override Control CreatePresenter()
    {
        _presenterBindingDisposables?.Dispose();
        _presenterBindingDisposables = new CompositeDisposable(4);

        foreach (var item in Items)
        {
            if (item is Control control) 
            {
                control.SetLogicalParent(null);
                control.SetVisualParent(null);
            }
        }

        Presenter = new DataGridMenuFilterFlyoutPresenter
        {
            ItemsSource = Items,
            MenuFlyout  = this
        };
        _presenterBindingDisposables.Add(BindUtils.RelayBind(this, ItemTemplateProperty, Presenter, MenuFlyoutPresenter.ItemTemplateProperty));
        _presenterBindingDisposables.Add(BindUtils.RelayBind(this, ItemContainerThemeProperty, Presenter, MenuFlyoutPresenter.ItemContainerThemeProperty));
        _presenterBindingDisposables.Add(BindUtils.RelayBind(this, IsShowArrowEffectiveProperty, Presenter, MenuFlyoutPresenter.IsShowArrowProperty));
        _presenterBindingDisposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, Presenter, MenuFlyoutPresenter.IsMotionEnabledProperty));
        _presenterBindingDisposables.Add(BindUtils.RelayBind(this, ArrowPositionProperty, Presenter, MenuFlyoutPresenter.ArrowPositionProperty));
        _presenterBindingDisposables.Add(BindUtils.RelayBind(this, ShouldUseOverlayLayerProperty, Presenter, MenuFlyoutPresenter.ShouldUseOverlayLayerProperty));
        ConfigureShowArrowEffective();
        ConfigureArrowPosition();
        return Presenter;
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
        if (Popup.Child is DataGridMenuFilterFlyoutPresenter presenter)
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

internal class DataGridFilterMenuItem : MenuItem
{
    public string? FilterValue { get; set; }
    public DataGridMenuFilterFlyoutPresenter? OwningPresenter { get; set; }

    static DataGridFilterMenuItem()
    {
        StaysOpenOnClickProperty.OverrideDefaultValue<DataGridFilterMenuItem>(false);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (ToggleType == MenuItemToggleType.Radio)
        {
            if (change.Property == IsCheckedProperty && IsChecked)
            {
                if (OwningPresenter != null)
                {
                    ClearCheckStateRecursive(OwningPresenter);
                }
            }
        }
    }
    
    private void ClearCheckStateRecursive(SelectingItemsControl itemsControl)
    {
        for (var i = 0; i < itemsControl.ItemCount; i++)
        {
            var item = itemsControl.ContainerFromIndex(i);
            if (item is MenuItem filterMenuItem)
            {
                ClearCheckStateRecursive(filterMenuItem);
            }
        }

        if (itemsControl is MenuItem menuItem && menuItem != this)
        {
            menuItem.IsChecked = false;
        }
    }
    
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is DataGridFilterMenuItem menuItem)
        {
            menuItem.OwningPresenter = OwningPresenter;
        }
    }
}