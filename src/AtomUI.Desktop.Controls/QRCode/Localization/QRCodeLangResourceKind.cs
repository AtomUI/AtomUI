using AtomUI.Localization;

namespace AtomUI.Desktop.Controls.Localization;

/// <summary>
/// Language resources of the QR code control: the texts shown on the cover over the QR image.
/// </summary>
[LanguageCatalog]
public enum QRCodeLangResourceKind
{
    /// <summary>
    /// Caption of the button that refreshes an expired QR code.
    /// </summary>
    Refresh,
    /// <summary>
    /// Text that states that the QR code has expired. Shown on the cover in the Expired status.
    /// </summary>
    Expired,
    /// <summary>
    /// Text that states that the QR code has been scanned. Shown on the cover in the Scanned status.
    /// </summary>
    Scanned
}
