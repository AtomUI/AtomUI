using AtomUI.Theme;
using Avalonia;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The container of one row in a <see cref="TransferListView" />. The theme shows the checkbox of a checkable row
/// and the remove button of a row that is not checkable.
/// </summary>
[TokenOwnerTransparent]
public partial class TransferListItem : ListViewItem
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