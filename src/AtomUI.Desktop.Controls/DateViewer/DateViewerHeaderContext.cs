using System.Windows.Input;
using AtomUI.Desktop.Controls.Internal.DateViewer;

namespace AtomUI.Desktop.Controls;

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
