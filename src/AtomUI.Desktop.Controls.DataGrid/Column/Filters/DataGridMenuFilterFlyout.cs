using AtomUI.Controls;
using AtomUI.Animations;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

internal class DataGridMenuFilterFlyout : MenuFlyout
{
    public event EventHandler<DataGridFilterValuesSelectedEventArgs>? FilterValuesSelected;
    internal Func<bool>? ShouldFilterOnPassiveClose { get; set; }
    internal bool IsActiveShutdown = false;
    
    public DataGridMenuFilterFlyout()
    {
        OpenMotion  = new SlideUpInMotion();
        CloseMotion = new SlideUpOutMotion();
    }
    
    protected override Control CreatePresenter()
    {
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
            MenuFlyout = this
        };
        foreach (var item in Items)
        {
            Presenter.Items.Add(item);
        }

        Presenter[!MenuFlyoutPresenter.ItemTemplateProperty]          = this[!ItemTemplateProperty];
        Presenter[!MenuFlyoutPresenter.ItemContainerThemeProperty]    = this[!ItemContainerThemeProperty];
        Presenter[!MenuFlyoutPresenter.IsArrowVisibleProperty]           = this[!IsArrowVisibleEffectiveProperty];
        Presenter[!MenuFlyoutPresenter.IsMotionEnabledProperty]       = this[!IsMotionEnabledProperty];
        Presenter[!MenuFlyoutPresenter.ArrowPositionProperty]         = this[!ArrowPositionProperty];

        ConfigureShowArrowEffective();
        ConfigureArrowPosition();
        return Presenter;
    }

    protected override void OnOpened()
    {
        base.OnOpened();
        IsActiveShutdown = false;
    }

    protected override void OnClosed()
    {
        base.OnClosed();
        if (!IsActiveShutdown && ShouldFilterOnPassiveClose?.Invoke() != true)
        {
            return;
        }

        var selectedItems = Popup.Child is DataGridMenuFilterFlyoutPresenter presenter
            ? presenter.GetFilterValues()
            : DataGridFilterValuesSelectedEventArgs.EmptyValues;
        var commitKind = IsActiveShutdown
            ? DataGridFilterValuesCommitKind.Confirmed
            : DataGridFilterValuesCommitKind.PassiveClose;
        NotifyFilterValuesSelected(new DataGridFilterValuesSelectedEventArgs(commitKind, selectedItems));
    }

    internal void NotifyFilterValuesSelected(DataGridFilterValuesSelectedEventArgs e)
    {
        FilterValuesSelected?.Invoke(this, e);
    }
}

internal class DataGridFilterMenuItem : MenuItem
{
    public object? FilterValue { get; set; }
    public DataGridMenuFilterFlyoutPresenter? OwningPresenter { get; set; }

    static DataGridFilterMenuItem()
    {
        StaysOpenOnClickProperty.OverrideDefaultValue<DataGridFilterMenuItem>(false);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property != IsCheckedProperty)
        {
            return;
        }

        if (ToggleType == MenuItemToggleType.Radio)
        {
            if (!IsChecked)
            {
                return;
            }

            if (OwningPresenter != null)
            {
                ClearCheckStateRecursive(OwningPresenter);
                OwningPresenter.NotifyFilterSelectionChanged();
            }
            return;
        }

        OwningPresenter?.NotifyFilterSelectionChanged();
    }
    
    private void ClearCheckStateRecursive(SelectingItemsControl itemsControl)
    {
        for (var i = 0; i < itemsControl.ItemCount; i++)
        {
            var item = DataGridMenuFilterFlyoutPresenter.GetFilterItem(itemsControl, i);
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
