using Avalonia;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A transfer whose two panels show flat lists. Each panel presents the list view held by
/// <see cref="SourceView" /> or <see cref="TargetView" />, a <see cref="TransferListView" /> by default.
/// A <see cref="PageSize" /> greater than 0 pages both lists.
/// </summary>
public partial class ListTransfer : AbstractTransfer
{
    #region 公共属性定义
    
    /// <summary>
    /// Identifies the <see cref="PageSize" /> property.
    /// </summary>
    public static readonly StyledProperty<int> PageSizeProperty =
        AvaloniaProperty.Register<ListTransfer, int>(nameof(PageSize), 0);
    
    /// <summary>
    /// Identifies the <see cref="SourceView" /> property.
    /// </summary>
    public static readonly StyledProperty<ITransferView?> SourceViewProperty =
        AvaloniaProperty.Register<ListTransfer, ITransferView?>(nameof(SourceView));
    
    /// <summary>
    /// Identifies the <see cref="TargetView" /> property.
    /// </summary>
    public static readonly StyledProperty<ITransferView?> TargetViewProperty =
        AvaloniaProperty.Register<ListTransfer, ITransferView?>(nameof(TargetView));
    
    /// <summary>
    /// Number of rows per page in the source and the target list. A value greater than 0 turns pagination on
    /// for both panels, including the page entries of their selection menus; 0, the default, turns it off.
    /// </summary>
    public int PageSize
    {
        get => GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }
    
    /// <summary>
    /// The view shown in the source panel, on the left. When it is <see langword="null" /> at the time the
    /// control template is applied, the control creates a <see cref="TransferListView" /> for it.
    /// </summary>
    public ITransferView? SourceView
    {
        get => GetValue(SourceViewProperty);
        set => SetValue(SourceViewProperty, value);
    }
    
    /// <summary>
    /// The view shown in the target panel, on the right. When it is <see langword="null" /> at the time the
    /// control template is applied, the control creates a <see cref="TransferListView" /> for it.
    /// </summary>
    public ITransferView? TargetView
    {
        get => GetValue(TargetViewProperty);
        set => SetValue(TargetViewProperty, value);
    }
    #endregion

    /// <summary>
    /// Called when the control template is applied. After the base call it creates a
    /// <see cref="TransferListView" /> for <see cref="SourceView" /> or <see cref="TargetView" /> that is
    /// <see langword="null" />, and updates <see cref="AbstractTransfer.IsPaginationEnabled" /> from
    /// <see cref="PageSize" />. Overrides must call the base member first.
    /// </summary>
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

    /// <summary>
    /// Called when a styled property of the control changes. After the base call it updates
    /// <see cref="AbstractTransfer.IsPaginationEnabled" /> from <see cref="PageSize" /> whenever
    /// <see cref="PageSize" /> changes. Overrides must call the base member first.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == PageSizeProperty)
        {
            SetCurrentValue(IsPaginationEnabledProperty, PageSize > 0);
        }
    }
    
}
