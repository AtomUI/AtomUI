using AtomUI.Controls;
using AtomUI.Animations;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace AtomUI.Desktop.Controls;

internal class DataGridTreeFilterFlyout : TreeViewFlyout
{
    internal static readonly StyledProperty<ItemToggleType> ToggleTypeProperty =
        TreeView.ToggleTypeProperty.AddOwner<DataGridTreeFilterFlyout>();

    internal bool IsActiveShutdown = false;
    internal Func<bool>? ShouldFilterOnPassiveClose { get; set; }

    public ItemToggleType ToggleType
    {
        get => GetValue(ToggleTypeProperty);
        set => SetValue(ToggleTypeProperty, value);
    }

    public event EventHandler<DataGridFilterValuesSelectedEventArgs>? FilterValuesSelected;

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
            TreeViewFlyout     = this
        };
        foreach (var item in Items)
        {
            if (item is Control control) 
            {
                control.SetLogicalParent(null);
                control.SetVisualParent(null);
            }
            presenter.Items.Add(item);
        }

        presenter[!DataGridTreeFilterFlyoutPresenter.IsMotionEnabledProperty] = this[!IsMotionEnabledProperty];
        presenter[!DataGridTreeFilterFlyoutPresenter.IsArrowVisibleProperty]     = this[!IsArrowVisibleEffectiveProperty];
        presenter[!DataGridTreeFilterFlyoutPresenter.ArrowPositionProperty]   = this[!ArrowPositionProperty];
        presenter[!DataGridTreeFilterFlyoutPresenter.ToggleTypeProperty]      = this[!ToggleTypeProperty];
        
        ConfigureShowArrowEffective();
        ConfigureArrowPosition();
        
        return presenter;
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

        var selectedItems = Popup.Child is DataGridTreeFilterFlyoutPresenter presenter
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

internal class DataGridFilterTreeViewItem : TreeViewItem
{
    public object? FilterValue { get; set; }
    public DataGridTreeFilterFlyoutPresenter? OwningPresenter { get; set; }

    protected override void OnHeaderDoubleTapped(TappedEventArgs e)
    {
        if (ItemCount > 0)
        {
            e.Handled = true;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property != IsCheckedProperty)
        {
            return;
        }

        if (ToggleType == ItemToggleType.Radio && IsChecked != true)
        {
            return;
        }

        OwningPresenter?.NotifyFilterSelectionChanged();
    }
}
