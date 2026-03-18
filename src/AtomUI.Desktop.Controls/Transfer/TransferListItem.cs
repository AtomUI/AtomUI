using Avalonia;

namespace AtomUI.Desktop.Controls;

public class TransferListItem : ListViewItem
{
    #region 内部属性定义
    internal static readonly DirectProperty<TransferListItem, bool> IsCheckableProperty =
        AvaloniaProperty.RegisterDirect<TransferListItem, bool>(nameof(IsCheckable), 
            o => o.IsCheckable,
            (o, v) => o.IsCheckable = v);
    
    private bool _isCheckable;

    internal bool IsCheckable
    {
        get => _isCheckable;
        set => SetAndRaise(IsCheckableProperty, ref _isCheckable, value);
    }
    #endregion
}