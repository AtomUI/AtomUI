using Avalonia;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

public partial class ListTransfer : AbstractTransfer
{
    #region 公共属性定义
    
    public static readonly StyledProperty<int> PageSizeProperty =
        AvaloniaProperty.Register<ListTransfer, int>(nameof(PageSize), 0);
    
    public static readonly StyledProperty<ITransferView?> SourceViewProperty =
        AvaloniaProperty.Register<ListTransfer, ITransferView?>(nameof(SourceView));
    
    public static readonly StyledProperty<ITransferView?> TargetViewProperty =
        AvaloniaProperty.Register<ListTransfer, ITransferView?>(nameof(TargetView));
    
    public int PageSize
    {
        get => GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }
    
    public ITransferView? SourceView
    {
        get => GetValue(SourceViewProperty);
        set => SetValue(SourceViewProperty, value);
    }
    
    public ITransferView? TargetView
    {
        get => GetValue(TargetViewProperty);
        set => SetValue(TargetViewProperty, value);
    }
    #endregion

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        if (SourceView == null)
        {
            SetCurrentValue(SourceViewProperty, new TransferListView());
        }

        if (TargetView == null)
        {
            SetCurrentValue(TargetViewProperty, new TransferListView());
        }
        SetCurrentValue(IsPaginationEnabledProperty, PageSize > 0);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == PageSizeProperty)
        {
            SetCurrentValue(IsPaginationEnabledProperty, PageSize > 0);
        }
    }
    
}
