using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

internal class DataGridTransferSelectionColumnHeader : TemplatedControl
{
    #region 公共属性定义

    public static readonly DirectProperty<DataGridTransferSelectionColumnHeader, PathIcon?> SelectionsIconProperty =
        AvaloniaProperty.RegisterDirect<DataGridTransferSelectionColumnHeader, PathIcon?>(nameof(SelectionsIcon),
            o => o.SelectionsIcon,
            (o, v) => o.SelectionsIcon = v);
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<DataGridTransferSelectionColumnHeader>();
    
    public static readonly StyledProperty<bool> IsPaginationEnabledProperty =
        AbstractTransfer.IsPaginationEnabledProperty.AddOwner<DataGridTransferSelectionColumnHeader>();
    
    public static readonly StyledProperty<bool> IsOneWayProperty =
        AbstractTransfer.IsOneWayProperty.AddOwner<DataGridTransferSelectionColumnHeader>();
    
    public static readonly DirectProperty<DataGridTransferSelectionColumnHeader, bool?> IsAllSelectedProperty =
        AvaloniaProperty.RegisterDirect<DataGridTransferSelectionColumnHeader, bool?>(nameof(IsAllSelected),
            o => o.IsAllSelected,
            (o, v) => o.IsAllSelected = v);
    
    private PathIcon? _selectionsIcon;
    internal PathIcon? SelectionsIcon
    {
        get => _selectionsIcon;
        set => SetAndRaise(SelectionsIconProperty, ref _selectionsIcon, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    public bool IsPaginationEnabled
    {
        get => GetValue(IsPaginationEnabledProperty);
        set => SetValue(IsPaginationEnabledProperty, value);
    }
    
    public bool IsOneWay
    {
        get => GetValue(IsOneWayProperty);
        set => SetValue(IsOneWayProperty, value);
    }
    
    private bool? _isAllSelected = false;
    internal bool? IsAllSelected
    {
        get => _isAllSelected;
        set => SetAndRaise(IsAllSelectedProperty, ref _isAllSelected, value);
    }

    #endregion
    
    TransferDataGridView? _dataGridView;

    internal void NotifyDataGridAttached(TransferDataGridView dataGridView)
    {
        dataGridView.SelectionChanged += HandleSelectionChanged;
        _dataGridView                 =  dataGridView;
    }

    private void HandleSelectionChanged(object? sender, SelectionChangedEventArgs args)
    {
        if (sender is TransferDataGridView dataGridView)
        {
            SetCurrentValue(IsAllSelectedProperty, dataGridView.IsAllRowSelected());
        }
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsAllSelectedProperty)
        {
            if (IsAllSelected == true)
            {
                _dataGridView?.SelectAll();
            }
            else
            {
                _dataGridView?.DeselectAll();
            }
        }
    }
}