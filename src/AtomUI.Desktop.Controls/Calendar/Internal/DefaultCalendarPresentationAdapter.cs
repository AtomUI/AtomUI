using System.Globalization;
using System.Windows.Input;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Resources;
using AtomUI.Desktop.Controls.Localization;
using Avalonia;

namespace AtomUI.Desktop.Controls.Internal.Calendar;

internal sealed class DefaultCalendarPresentationAdapter : ICalendarPresentationAdapter
{
    internal static DefaultCalendarPresentationAdapter Instance { get; } = new();

    public CalendarPresentationMetrics Metrics { get; } = new(
        CalendarTokenKind.MiniContentHeight,
        CalendarTokenKind.FullCellMinHeight,
        SharedTokenKind.ControlHeightSM);

    private DefaultCalendarPresentationAdapter()
    {
    }

    public CalendarEffectiveRange GetEffectiveRange(CalendarDateRange? validRange) =>
        CalendarEffectiveRange.FromValidRange(validRange);

    public DateViewerCellContext? CreateCellContext(Internal.DateViewer.DatePanelSession session, Internal.DateViewer.DateViewerCellModel model) =>
        model.Value is { } value && model.Kind != DateViewerCellType.Week
            ? new CalendarCellContext(value, session.Input.Today, model.Kind, model.DisplayText,
                model.IsToday, model.IsInView, model.IsSelected, model.IsDisabled, model.IsFocused) : null;

    public string GetAutomationName(Internal.DateViewer.DatePanelSession session, Internal.DateViewer.DateViewerCellModel model)
    {
        if (model.Value is not { } value)
        {
            return string.Empty;
        }

        var culture = session.Input.Culture;
        var localizer = Application.Current is { } application
            ? global::AtomUI.ApplicationExtensions.GetLocalizer(application)
            : null;
        return model.Kind switch
        {
            DateViewerCellType.Date => value.ToString("D", culture),
            DateViewerCellType.Month => value.ToString("Y", culture),
            _ => $"{localizer?.Get(CalendarControlLangResourceKind.Week) ?? CalendarControlLangResourceKind.Week.ToString()} {model.DisplayText}"
        };
    }

    public string FormatYearOption(int year, CultureInfo culture)
    {
        var localizer = Application.Current is { } application
            ? global::AtomUI.ApplicationExtensions.GetLocalizer(application)
            : null;
        var suffix = localizer?.Get(CalendarControlLangResourceKind.YearSuffix) ?? string.Empty;
        return year.ToString(CultureInfo.InvariantCulture) + suffix;
    }

    public string FormatMonthOption(int year, int month, CultureInfo culture) =>
        culture.DateTimeFormat.AbbreviatedMonthNames[month - 1];

    public CalendarHeaderContext CreateHeaderContext(
        AtomUI.Desktop.Controls.Calendar owner,
        ICommand changeValueCommand,
        ICommand changeModeCommand) =>
        new(owner.Value.Date, owner.Mode, changeValueCommand, changeModeCommand);
}
