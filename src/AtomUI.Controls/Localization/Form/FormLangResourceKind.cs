using AtomUI.Localization;

namespace AtomUI.Controls.Localization;

/// <summary>
/// Identifies the text resources that tell the user why the value of a form item failed validation.
/// Each member is the id of a unit in the XLF files next to this enum. When a validator sets no message of its own,
/// its type selects the member and the unit is formatted with the label of the item as <c>{0}</c>.
/// </summary>
[LanguageCatalog]
public enum FormLangResourceKind
{
    /// <summary>
    /// The message of the <c>FormStringNotEmptyValidator</c> and <c>FormNotNullValidator</c>.
    /// </summary>
    FieldRequired,
    /// <summary>
    /// The message of the <c>FormStringLengthValidator</c>. <c>{1}</c> and <c>{2}</c> are its minimum and maximum
    /// length in characters.
    /// </summary>
    InvalidLength,
    /// <summary>
    /// The message for a value outside an allowed range. The library assigns it to no validator.
    /// </summary>
    InvalidRange,
    /// <summary>
    /// The message of the <c>FormUrlValidator</c>.
    /// </summary>
    InvalidUrl,
    /// <summary>
    /// The message for a value that is not a valid email address. The library assigns it to no validator.
    /// </summary>
    InvalidEmail,
    /// <summary>
    /// The message of every validator that is assigned no member of its own.
    /// </summary>
    InvalidValue
}
