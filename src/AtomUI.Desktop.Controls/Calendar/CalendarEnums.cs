namespace AtomUI.Desktop.Controls;

/// <summary>
/// The display mode of a Calendar, which decides the kind of panel it shows.
/// </summary>
public enum CalendarMode
{
    /// <summary>
    /// Shows a grid of the days of one month.
    /// </summary>
    Month,

    /// <summary>
    /// Shows a grid of the months of one year.
    /// </summary>
    Year
}

/// <summary>
/// The part of the calendar a user selection came from, as <see cref="Calendar.Selected" /> reports it.
/// </summary>
public enum CalendarSelectSource
{
    /// <summary>
    /// The user picked a year in the year Select of the default header.
    /// </summary>
    Year,
    /// <summary>
    /// The user picked a month cell of the grid in <see cref="CalendarMode.Year" /> mode, or a month in the month
    /// Select of the default header.
    /// </summary>
    Month,
    /// <summary>
    /// The user picked a date cell of the grid in <see cref="CalendarMode.Month" /> mode.
    /// </summary>
    Date,
    /// <summary>
    /// The value was submitted by the value command of a custom header template, or by a navigation of the date
    /// panel.
    /// </summary>
    Customize
}
