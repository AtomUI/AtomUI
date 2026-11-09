using System.Globalization;
using System.Windows.Input;

namespace AtomUI.Desktop.Controls.Internal.Calendar;

internal readonly record struct CalendarEffectiveRange(DateTime? Start, DateTime? End, bool IsEmpty)
{
    internal static CalendarEffectiveRange FromValidRange(CalendarDateRange? validRange) =>
        validRange is null
            ? new CalendarEffectiveRange(null, null, false)
            : new CalendarEffectiveRange(validRange.Start, validRange.End, false);

    internal DateTime? MinDate => IsEmpty ? DateTime.MaxValue.Date : Start;
    internal DateTime? MaxDate => IsEmpty ? DateTime.MinValue.Date : End;
}

/// <summary>
/// The Token kinds of the presentation metrics, all resolved on <see cref="OwnerType"/>: an Own kind is its Own Token, a
/// <see cref="AtomUI.Theme.Resources.SharedTokenKind"/> its effective global value.
/// </summary>
internal readonly record struct CalendarPresentationMetrics(
    Type OwnerType,
    Enum MiniContentHeightResourceKey,
    Enum FullCellMinHeightResourceKey,
    Enum RangeBarTopOffsetResourceKey);

internal interface ICalendarPresentationAdapter
{
    DateViewerCellContext? CreateCellContext(Internal.DateViewer.DatePanelSession session, Internal.DateViewer.DateViewerCellModel model);
    CalendarPresentationMetrics Metrics { get; }

    CalendarEffectiveRange GetEffectiveRange(CalendarDateRange? validRange);

    string GetAutomationName(Internal.DateViewer.DatePanelSession session, Internal.DateViewer.DateViewerCellModel model);

    string FormatYearOption(int year, CultureInfo culture);

    string FormatMonthOption(int year, int month, CultureInfo culture);

    CalendarHeaderContext CreateHeaderContext(
        AtomUI.Desktop.Controls.Calendar owner,
        ICommand changeValueCommand,
        ICommand changeModeCommand);
}
