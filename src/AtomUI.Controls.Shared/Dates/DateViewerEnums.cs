namespace AtomUI.Desktop.Controls;

/// <summary>
/// Period that one selection covers in a date panel.
/// </summary>
public enum DateViewerSelectionUnit
{
    /// <summary>
    /// One calendar day.
    /// </summary>
    Date,
    /// <summary>
    /// The seven-day week that contains the chosen day, counted from the first day of the week that the panel uses.
    /// </summary>
    Week,
    /// <summary>
    /// The calendar month that contains the chosen day.
    /// </summary>
    Month,
    /// <summary>
    /// The calendar quarter that contains the chosen day, starting in January, April, July or October.
    /// </summary>
    Quarter,
    /// <summary>
    /// The calendar year that contains the chosen day.
    /// </summary>
    Year
}

/// <summary>
/// Level at which a date panel is shown.
/// </summary>
public enum DateViewerPanelKind
{
    /// <summary>
    /// A month of days in a calendar grid. A column of week numbers is added in front when week numbers are shown.
    /// </summary>
    Date,
    /// <summary>
    /// The twelve months of one year.
    /// </summary>
    Month,
    /// <summary>
    /// The four quarters of one year.
    /// </summary>
    Quarter,
    /// <summary>
    /// Twelve year cells around one decade: the year before it, its ten years and the year after it. The two
    /// outer years are outside the current view.
    /// </summary>
    Year
}

/// <summary>
/// What one cell of a date panel stands for. Cell templates receive it as the <c>CellType</c> of
/// <c>DateViewerCellContext</c>.
/// </summary>
public enum DateViewerCellType
{
    /// <summary>
    /// A day cell in the date panel.
    /// </summary>
    Date,
    /// <summary>
    /// The week-number cell at the start of a row in the date panel. It exists only while week numbers are shown,
    /// and it cannot take focus.
    /// </summary>
    Week,
    /// <summary>
    /// A month cell in the month panel.
    /// </summary>
    Month,
    /// <summary>
    /// A quarter cell in the quarter panel.
    /// </summary>
    Quarter,
    /// <summary>
    /// A year cell in the year panel.
    /// </summary>
    Year
}
