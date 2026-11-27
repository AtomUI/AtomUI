using AtomUI.Localization;

namespace AtomUI.Desktop.Controls.Localization;

/// <summary>
/// The localizable texts of the upload control: the prompt of the default drop area and the status texts of
/// picture list items.
/// </summary>
[LanguageCatalog]
public enum UploadLangResourceKind
{
    /// <summary>
    /// Status text shown on a picture-card or picture-circle item while its file is uploading.
    /// </summary>
    Uploading,
    /// <summary>
    /// Status text shown on an item whose file is waiting to be uploaded, in the picture, picture-card and
    /// picture-circle lists.
    /// </summary>
    Pending,
    /// <summary>
    /// Heading of the default drop area, which tells the user to click or drag files into the area.
    /// </summary>
    DragUploadHead
}
