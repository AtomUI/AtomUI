using AtomUI.Localization;

namespace AtomUI.Controls.Localization;

[LanguageCatalog]
public enum CommonLangResourceKind
{
    Ok,
    Submit,
    Cancel,
    Reset,
    Edit,
    Delete,
    Save,
    NoData,
    Loading,
    Optional,
    // Appended to keep the values of the existing members stable for compiled callers.
    Clear
}
