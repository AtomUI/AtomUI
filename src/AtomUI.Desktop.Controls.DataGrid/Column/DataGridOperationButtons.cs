using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

internal class DataGridOperationButtons : TemplatedControl
{
    #region 公共接口

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<DataGridOperationButtons>();
    
    public static readonly StyledProperty<bool> IsEditEnabledProperty =
        AvaloniaProperty.Register<DataGridOperationButtons, bool>(nameof(IsEditEnabled));
    
    public static readonly StyledProperty<bool> IsDeleteEnabledProperty =
        AvaloniaProperty.Register<DataGridOperationButtons, bool>(nameof(IsDeleteEnabled));
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    public bool IsEditEnabled
    {
        get => GetValue(IsEditEnabledProperty);
        set => SetValue(IsEditEnabledProperty, value);
    }

    public bool IsDeleteEnabled
    {
        get => GetValue(IsDeleteEnabledProperty);
        set => SetValue(IsDeleteEnabledProperty, value);
    }
    #endregion
    
    internal static readonly DirectProperty<DataGridOperationButtons, bool> IsEditingProperty =
        AvaloniaProperty.RegisterDirect<DataGridOperationButtons, bool>(
            nameof(IsEditing),
            o => o.IsEditing,
            (o, v) => o.IsEditing = v);
    
    internal bool IsEditing
    {
        get => _isEditing;
        set => SetAndRaise(IsEditingProperty, ref _isEditing, value);
    }

    private bool _isEditing;

    private HyperLinkTextBlock? _editAction;
    private PopupConfirm? _deleteAction;
    private HyperLinkTextBlock? _saveAction;
    private PopupConfirm? _cancelAction;
    
    internal DataGrid? OwningGrid { get; set; }
    internal DataGridRow? OwningRow { get; set; }

    static DataGridOperationButtons()
    {
        HyperLinkTextBlock.ClickEvent.AddClassHandler<DataGridOperationButtons>(
            (buttons, args) => buttons.HandleActionClick(args));
        PopupConfirm.ConfirmedEvent.AddClassHandler<DataGridOperationButtons>(
            (buttons, args) => buttons.HandlePopupConfirmed(args));
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _editAction   = e.NameScope.Find<HyperLinkTextBlock>(DataGridOperationButtonsThemeConstants.EditActionPart);
        _deleteAction = e.NameScope.Find<PopupConfirm>(DataGridOperationButtonsThemeConstants.DeleteActionPart);
        _saveAction   = e.NameScope.Find<HyperLinkTextBlock>(DataGridOperationButtonsThemeConstants.SaveActionPart);
        _cancelAction = e.NameScope.Find<PopupConfirm>(DataGridOperationButtonsThemeConstants.CancelActionPart);
    }

    private void HandleActionClick(RoutedEventArgs e)
    {
        if (ReferenceEquals(e.Source, _editAction))
        {
            BeginEditingRow();
        }
        else if (ReferenceEquals(e.Source, _saveAction))
        {
            EndEditingRow();
        }
    }

    private void HandlePopupConfirmed(RoutedEventArgs e)
    {
        if (ReferenceEquals(e.Source, _deleteAction))
        {
            DeleteRow();
        }
        else if (ReferenceEquals(e.Source, _cancelAction))
        {
            EndEditingRow();
        }
    }

    private async void DeleteRow()
    {
        if (OwningGrid is { } grid && OwningRow is { RowKey.IsValid: true } row)
        {
            try
            {
                await grid.DeleteRowAsync(row.RowKey);
            }
            catch (OperationCanceledException)
            {
                // The row/query generation changed before the user action completed.
            }
        }
    }

    private void BeginEditingRow()
    {
        if (OwningRow != null && OwningGrid != null)
        {
            SetValue(IsEditingProperty, true, BindingPriority.Template);
        }
    }
    
    private void EndEditingRow()
    {
        if (OwningRow != null && OwningGrid != null)
        {
            SetValue(IsEditingProperty, false, BindingPriority.Template);
        }
    }

    internal void NotifyLoadingRow(DataGridRow row)
    {
        OwningRow                = row;
        this[!IsEditingProperty] = row[!DataGridRow.IsEditingModeProperty];
    }
    
    internal void NotifyUnLoadingRow(DataGridRow row)
    {
        OwningRow = null;
    }
}
