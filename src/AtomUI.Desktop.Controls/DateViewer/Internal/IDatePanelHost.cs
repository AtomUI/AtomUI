namespace AtomUI.Desktop.Controls.Internal.DateViewer;

/// <summary>The selection owner supplies inputs and accepts navigation/selection intent.</summary>
internal interface IDatePanelHost
{
    DatePanelInput ReadInput();
    void NavigateTo(DateTime displayDate, DateViewerPanelKind panelKind);
    void Activate(DateCellSelection selection);
    void Preview(DateTime? hoveredValue);
}

internal readonly record struct DateCellSelection(DateTime Value, DateViewerCellType Kind, DateUnitRange Period);

internal enum DateFocusDirection { Left, Right, Up, Down }

internal abstract record DatePanelAction
{
    public sealed record Navigate(int Periods) : DatePanelAction;
    public sealed record ChangePanel(DateViewerPanelKind Kind, DateTime? Anchor = null) : DatePanelAction;
    public sealed record MoveFocus(DateFocusDirection Direction) : DatePanelAction;
    public sealed record Focus(DateTime Value) : DatePanelAction;
    public sealed record Hover(DateTime? Value) : DatePanelAction;
    public sealed record Activate(DateTime Value, DateViewerCellType Kind) : DatePanelAction;
    public sealed record ActivateFocused : DatePanelAction;
}
