using AtomUI.Animations;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace AtomUI.Desktop.Controls;

internal class DataGridTreeFilterFlyout : TreeViewFlyout
{
    #region 公共属性定义

    public ItemToggleType ToggleType
    {
        get => GetValue(ToggleTypeProperty);
        set => SetValue(ToggleTypeProperty, value);
    }

    #endregion

    #region 公共事件定义

    public event EventHandler<DataGridFilterValuesSelectedEventArgs>? FilterValuesSelected;

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<ItemToggleType> ToggleTypeProperty =
        TreeView.ToggleTypeProperty.AddOwner<DataGridTreeFilterFlyout>();

    internal Func<bool>? ShouldFilterOnPassiveClose { get; set; }

    #endregion

    #region 运行时字段

    internal bool IsActiveShutdown = false;

    #endregion

    #region 内部协作 API

    // The filter has Dropdown semantics regardless of its tree content.
    internal override AbstractMotion GetDefaultOpenMotion() => PopupMotionDefaults.Create(PopupMotionDefaults.DropdownOpen);

    internal override AbstractMotion GetDefaultCloseMotion() => PopupMotionDefaults.Create(PopupMotionDefaults.DropdownClose);

    #endregion

    #region 控件 API

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

    #endregion

    #region 内部协作 API

    internal void NotifyFilterValuesSelected(DataGridFilterValuesSelectedEventArgs e)
    {
        FilterValuesSelected?.Invoke(this, e);
    }

    #endregion
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
