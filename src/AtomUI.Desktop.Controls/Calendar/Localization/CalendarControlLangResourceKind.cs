using AtomUI.Localization;

namespace AtomUI.Desktop.Controls.Localization;

/// <summary>
/// The language resources of the <see cref="AtomUI.Desktop.Controls.Calendar" /> control: the labels of the mode
/// switch and of the year Select of its default header, and the name of a week.
/// </summary>
[LanguageCatalog]
public enum CalendarControlLangResourceKind
{
    /// <summary>
    /// The label of the option that switches the default header to month mode.
    /// </summary>
    Month,
    /// <summary>
    /// The label of the option that switches the default header to year mode.
    /// </summary>
    Year,
    /// <summary>
    /// The text appended to the year number in the options of the year Select of the default header. It is empty in
    /// English.
    /// </summary>
    YearSuffix,
    /// <summary>
    /// The word that the default presentation puts before the display text of a week cell when it spells out a cell
    /// as text.
    /// </summary>
    Week
}
