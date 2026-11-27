using System.Windows.Input;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The data context of a custom header template, which is set through <see cref="Calendar.HeaderTemplate" />.
/// It does not expose the <see cref="Calendar" /> instance: the template submits user intent only through
/// <see cref="ChangeValueCommand" /> and <see cref="ChangeModeCommand" />.
/// </summary>
public class CalendarHeaderContext
{
    /// <summary>
    /// Creates the context for the current state of the calendar and the two commands of its custom header.
    /// </summary>
    /// <param name="value">
    /// The calendar value that the header shows.
    /// </param>
    /// <param name="mode">
    /// The calendar mode that the header shows.
    /// </param>
    /// <param name="changeValueCommand">
    /// The command that becomes <see cref="ChangeValueCommand" />.
    /// </param>
    /// <param name="changeModeCommand">
    /// The command that becomes <see cref="ChangeModeCommand" />.
    /// </param>
    public CalendarHeaderContext(DateTime value, CalendarMode mode, ICommand changeValueCommand, ICommand changeModeCommand)
    {
        Value = value;
        Mode = mode;
        ChangeValueCommand = changeValueCommand;
        ChangeModeCommand = changeModeCommand;
    }

    /// <summary>
    /// The value of the calendar at the time the context was created.
    /// </summary>
    public DateTime Value { get; }

    /// <summary>
    /// The mode of the calendar at the time the context was created.
    /// </summary>
    public CalendarMode Mode { get; }

    /// <summary>
    /// Submits a date as the new value of the calendar.
    /// Executing it with a <see cref="DateTime" /> commits a user selection. <see cref="Calendar.Selected" />
    /// reports it with the source <see cref="CalendarSelectSource.Customize" />. The valid range and the disabled
    /// dates of the calendar are not applied, so the custom header must enforce them itself.
    /// <c>CanExecute</c> is <c>false</c> for any parameter that is not a <see cref="DateTime" />.
    /// </summary>
    public ICommand ChangeValueCommand { get; }

    /// <summary>
    /// Submits the new mode of the calendar. Executing it with <see cref="CalendarMode.Month" /> or
    /// <see cref="CalendarMode.Year" /> switches the calendar to that mode; a switch to the current mode does
    /// nothing. <c>CanExecute</c> is <c>false</c> for any other value.
    /// </summary>
    public ICommand ChangeModeCommand { get; }
}
