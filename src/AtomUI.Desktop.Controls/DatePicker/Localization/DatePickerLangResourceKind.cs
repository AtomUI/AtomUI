using AtomUI.Localization;

namespace AtomUI.Desktop.Controls.Localization;

/// <summary>
/// Localized texts of the date pickers: the labels of the Today and Now buttons in the popup.
/// </summary>
[LanguageCatalog]
public enum DatePickerLangResourceKind
{
    /// <summary>
    /// Label of the Today button in the popup, which selects the current date.
    /// The button is shown only when IsShowNow is on and the mode is Date without the time selection.
    /// </summary>
    Today,
    /// <summary>
    /// Label of the Now button in the popup, which selects the current date and time.
    /// The button is shown only when IsShowNow is on and the mode is Date with the time selection.
    /// </summary>
    Now
}
