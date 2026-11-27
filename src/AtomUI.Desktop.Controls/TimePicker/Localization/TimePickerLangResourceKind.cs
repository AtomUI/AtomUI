using AtomUI.Localization;

namespace AtomUI.Desktop.Controls.Localization;

/// <summary>
/// Localized texts of the time picker: the AM and PM designators and the label of the Now button.
/// The date picker uses the designators as well.
/// </summary>
[LanguageCatalog]
public enum TimePickerLangResourceKind
{
    /// <summary>
    /// The AM designator of a 12-hour time. It applies when the time is formatted without an explicit designator.
    /// </summary>
    AMText,
    /// <summary>
    /// The PM designator of a 12-hour time. It applies when the time is formatted without an explicit designator.
    /// </summary>
    PMText,
    /// <summary>
    /// The label of the Now button in the time picker popup panel.
    /// </summary>
    Now
}
