using AtomUI.Desktop.Controls.Internal.DateViewer;

namespace AtomUI.Desktop.Controls;

public partial class Calendar : IDatePanelHost
{
    DatePanelInput IDatePanelHost.ReadInput()
    {
        var range = _presentationAdapter.GetEffectiveRange(ValidRange);
        return new DatePanelInput
        {
            DisplayDate = Value, SelectedDate = Value, Today = DateTime.Today,
            IsSelectionRequired = true,
            AllowWeekActivation = true,
            PanelKind = Mode == CalendarMode.Year ? DateViewerPanelKind.Month : DateViewerPanelKind.Date,
            SelectionUnit = Mode == CalendarMode.Year ? DateViewerSelectionUnit.Month : DateViewerSelectionUnit.Date,
            Culture = CurrentCulture, ShowWeek = ShowWeek, WeekNumbering = DateWeekNumbering.Culture,
            MinDate = range.MinDate, MaxDate = range.MaxDate, DisabledDate = DisabledDate,
            ConstraintMode = DatePanelConstraintMode.Calendar, AbbreviatedWeekdays = true, DateNumberFormat = "D2"
        };
    }

    void IDatePanelHost.NavigateTo(DateTime displayDate, DateViewerPanelKind panelKind)
    {
        // Calendar navigation is a business Value/Mode operation, including clamping the day.
        if (panelKind is not (DateViewerPanelKind.Date or DateViewerPanelKind.Month))
        {
            return;
        }

        var day = Math.Min(Value.Day, DateTime.DaysInMonth(displayDate.Year, displayDate.Month));
        CommitUserSelection(new DateTime(displayDate.Year, displayDate.Month, day), CalendarSelectSource.Customize);
    }

    void IDatePanelHost.Activate(DateCellSelection selection)
    {
        var target = selection.Kind == DateViewerCellType.Month
            ? new DateTime(selection.Value.Year, selection.Value.Month, Math.Min(Value.Day,
                DateTime.DaysInMonth(selection.Value.Year, selection.Value.Month)))
            : selection.Value;
        CommitUserSelection(target, selection.Kind == DateViewerCellType.Month ? CalendarSelectSource.Month : CalendarSelectSource.Date);
    }

    void IDatePanelHost.Preview(DateTime? hoveredValue)
    {
    }

    private string CreateDateAutomationName(DatePanelSession session, DateViewerCellModel model) =>
        _presentationAdapter.GetAutomationName(session, model);

    private DateViewerCellContext? CreateDateCellContext(DatePanelSession session, DateViewerCellModel model) =>
        _presentationAdapter.CreateCellContext(session, model);
}
