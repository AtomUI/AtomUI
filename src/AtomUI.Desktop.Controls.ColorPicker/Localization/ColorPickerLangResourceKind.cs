using AtomUI.Localization;

namespace AtomUI.Desktop.Controls.Localization;

/// <summary>
/// Keys of the built-in language resources of the color picker. Each member names a translation unit that the
/// <c>ColorPickerLangResource</c> markup extension resolves for the active language.
/// </summary>
[LanguageCatalog]
public enum ColorPickerLangResourceKind
{
    /// <summary>
    /// Text that stands for a color picker without a value. The default theme assigns it to the
    /// <c>EmptyColorText</c> property of the picker; the English source text is "Transparent".
    /// </summary>
    EmptyColorText,
    /// <summary>
    /// Caption of the confirm button in the footer of the picker views; the button accepts the pending color.
    /// </summary>
    Confirm,
    /// <summary>
    /// Caption of the cancel button in the footer of the picker views; the button discards the pending color.
    /// </summary>
    Cancel
}
