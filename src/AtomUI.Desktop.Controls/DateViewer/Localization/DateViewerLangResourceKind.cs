using AtomUI.Localization;

namespace AtomUI.Desktop.Controls.Localization;

/// <summary>
/// The localizable texts of the date viewer and the range date viewer, such as the names of the header buttons and
/// the text of the Today link.
/// </summary>
[LanguageCatalog]
public enum DateViewerLangResourceKind
{
    /// <summary>
    /// Accessible name of the button that steps back by one period, "Previous period".
    /// </summary>
    PreviousPeriod,
    /// <summary>
    /// Accessible name of the button that steps back by one page, "Previous page".
    /// </summary>
    PreviousPage,
    /// <summary>
    /// Accessible name of the button that steps forward by one period, "Next period".
    /// </summary>
    NextPeriod,
    /// <summary>
    /// Accessible name of the button that steps forward by one page, "Next page".
    /// </summary>
    NextPage,
    /// <summary>
    /// Accessible name of the header button that switches to the month or year panel, "Choose month or year".
    /// </summary>
    ChoosePeriod,
    /// <summary>
    /// The word "Week", placed before the number of a week cell in its accessible name.
    /// </summary>
    Week,
    /// <summary>
    /// The text of the Today link in the default footer.
    /// </summary>
    Today
}
