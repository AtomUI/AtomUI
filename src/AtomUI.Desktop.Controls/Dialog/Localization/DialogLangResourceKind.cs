using AtomUI.Localization;

namespace AtomUI.Desktop.Controls.Localization;

/// <summary>
/// Names the localizable strings of a dialog: the caption of each button that
/// <see cref="DialogButtonBox" /> can show.
/// Each member is the key of a translation unit in the XLF translation files.
/// The build turns this enum into a XAML markup extension that the
/// <see cref="DialogButtonBox" /> theme uses to set the text of each button, as in
/// <c>{atom:DialogLangResource Ok}</c>.
/// </summary>
[LanguageCatalog]
public enum DialogLangResourceKind
{
    /// <summary>
    /// Caption of the OK button.
    /// </summary>
    Ok,
    /// <summary>
    /// Caption of the Open button.
    /// </summary>
    Open,
    /// <summary>
    /// Caption of the Save button.
    /// </summary>
    Save,
    /// <summary>
    /// Caption of the Cancel button.
    /// </summary>
    Cancel,
    /// <summary>
    /// Caption of the Close button.
    /// </summary>
    Close,
    /// <summary>
    /// Caption of the Discard button.
    /// </summary>
    Discard,
    /// <summary>
    /// Caption of the Apply button.
    /// </summary>
    Apply,
    /// <summary>
    /// Caption of the Reset button.
    /// </summary>
    Reset,
    /// <summary>
    /// Caption of the Reload button.
    /// </summary>
    Reload,
    /// <summary>
    /// Caption of the Restore Defaults button.
    /// </summary>
    RestoreDefaults,
    /// <summary>
    /// Caption of the Help button.
    /// </summary>
    Help,
    /// <summary>
    /// Caption of the Save All button.
    /// </summary>
    SaveAll,
    /// <summary>
    /// Caption of the Yes button.
    /// </summary>
    Yes,
    /// <summary>
    /// Caption of the Yes to All button.
    /// </summary>
    YesToAll,
    /// <summary>
    /// Caption of the No button.
    /// </summary>
    No,
    /// <summary>
    /// Caption of the No to All button.
    /// </summary>
    NoToAll,
    /// <summary>
    /// Caption of the Abort button.
    /// </summary>
    Abort,
    /// <summary>
    /// Caption of the Retry button.
    /// </summary>
    Retry,
    /// <summary>
    /// Caption of the Ignore button.
    /// </summary>
    Ignore
}
