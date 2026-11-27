using AtomUI.Controls;
using System.Windows.Input;
using AtomUI.Desktop.Controls.Internal.DateViewer;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Binding context of a date viewer header: the caption text, the step sizes of the navigation buttons and the
/// commands they call. The default header and a custom header template both bind to it.
/// </summary>
/// <param name="DisplayDate">
/// The date the panel under the header is anchored to. <see cref="DisplayText" /> is formatted from it.
/// </param>
/// <param name="PanelKind">
/// The kind of the panel under the header: date, month, quarter or year.
/// </param>
/// <param name="SelectionUnit">
/// The selection unit of the viewer. It decides which panel kinds <see cref="ChangePanelCommand" /> accepts.
/// </param>
/// <param name="DisplayText">
/// The caption of the header: month and year for a date panel, a decade range for a year panel and a year for
/// any other panel.
/// </param>
/// <param name="ParentPanelKind">
/// The panel kind that the caption button switches to: month for a date panel and year for any other panel.
/// </param>
/// <param name="PreviousPage">
/// The step of the previous-page button, in periods of <see cref="PanelKind" />: <c>-12</c> for a date panel and
/// <c>-1</c> for any other panel.
/// </param>
/// <param name="PreviousPeriod">
/// The step of the previous-period button: <c>-1</c>. The button is visible only while
/// <see cref="ShowPeriodNavigation" /> is <see langword="true" />.
/// </param>
/// <param name="NextPeriod">
/// The step of the next-period button: <c>1</c>. The button is visible only while
/// <see cref="ShowPeriodNavigation" /> is <see langword="true" />.
/// </param>
/// <param name="NextPage">
/// The step of the next-page button, in periods of <see cref="PanelKind" />: <c>12</c> for a date panel and
/// <c>1</c> for any other panel.
/// </param>
/// <param name="ShowPeriodNavigation">
/// Whether the previous-period and next-period buttons are visible. This is <see langword="true" /> only for a
/// date panel.
/// </param>
/// <param name="ShowPreviousNavigation">
/// Whether the buttons that step back are visible. A viewer with several panels shows them on its first panel only.
/// </param>
/// <param name="ShowNextNavigation">
/// Whether the buttons that step forward are visible. A viewer with several panels shows them on its last panel only.
/// </param>
/// <param name="NavigateCommand">
/// Moves the viewer by the number of periods passed as the command parameter. The command can execute only when
/// the parameter is an integer.
/// </param>
/// <param name="ChangePanelCommand">
/// Switches the viewer to the panel kind passed as the command parameter. It accepts the year panel, the panel that
/// matches <see cref="SelectionUnit" />, and the month panel while the selection unit is date or week.
/// </param>
public sealed record DateViewerHeaderContext(
    DateTime DisplayDate,
    DateViewerPanelKind PanelKind,
    DateViewerSelectionUnit SelectionUnit,
    string DisplayText,
    DateViewerPanelKind ParentPanelKind,
    int PreviousPage,
    int PreviousPeriod,
    int NextPeriod,
    int NextPage,
    bool ShowPeriodNavigation,
    bool ShowPreviousNavigation,
    bool ShowNextNavigation,
    ICommand NavigateCommand,
    ICommand ChangePanelCommand)
{
    internal static DateViewerHeaderContext Create(DatePanelInput input, Action<DatePanelAction> apply, int panelIndex = 0)
    {
        var text = input.PanelKind switch
        {
            DateViewerPanelKind.Date => input.DisplayDate.ToString(
                input.Culture.TwoLetterISOLanguageName == "zh" ? "yyyy年M月" : "MMM yyyy", input.Culture),
            DateViewerPanelKind.Year => $"{Math.Max(1, input.DisplayDate.Year / 10 * 10)}-{Math.Min(9999, input.DisplayDate.Year / 10 * 10 + 9)}",
            _ => input.DisplayDate.Year.ToString(input.Culture)
        };
        var target = DateViewer.TargetPanel(input.SelectionUnit);
        bool CanChange(DateViewerPanelKind kind) => kind == DateViewerPanelKind.Year || kind == target ||
            target == DateViewerPanelKind.Date && kind == DateViewerPanelKind.Month;
        var page = input.PanelKind == DateViewerPanelKind.Date ? 12 : 1;
        var parent = input.PanelKind == DateViewerPanelKind.Date ? DateViewerPanelKind.Month : DateViewerPanelKind.Year;
        var multiple = input.PanelCount > 1;
        var showPrevious = !multiple || panelIndex == 0;
        var showNext = !multiple || panelIndex == input.PanelCount - 1;
        return new DateViewerHeaderContext(input.DisplayDate, input.PanelKind, input.SelectionUnit, text, parent,
            -page, -1, 1, page, input.PanelKind == DateViewerPanelKind.Date, showPrevious, showNext,
            new DateViewerCommand(value => apply(new DatePanelAction.Navigate((int)value!)), value => value is int),
            new DateViewerCommand(value =>
            {
                var kind = (DateViewerPanelKind)value!;
                var anchor = panelIndex == 0 ? input.DisplayDate : DatePanelAlgorithms.Navigate(input.DisplayDate, kind, -panelIndex);
                apply(new DatePanelAction.ChangePanel(kind, anchor));
            },
                value => value is DateViewerPanelKind kind && CanChange(kind)));
    }
}

internal sealed class DateViewerCommand(Action<object?> execute, Func<object?, bool> canExecute) : ICommand
{
    public bool CanExecute(object? parameter) => canExecute(parameter);
    public void Execute(object? parameter)
    {
        if (CanExecute(parameter))
        {
            execute(parameter);
        }
    }
    public event EventHandler? CanExecuteChanged;
    internal void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
