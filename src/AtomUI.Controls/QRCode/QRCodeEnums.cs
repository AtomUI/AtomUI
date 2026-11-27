namespace AtomUI.Controls;

/// <summary>
/// The error-correction level of a QR code: how much redundant data is added to its text. A higher level keeps the
/// code readable after more damage, at the cost of more modules for the same text.
/// </summary>
public enum QRCodeEccLevel
{
    /// <summary>
    /// The lowest level. The code has the fewest modules for a given text and tolerates the least damage.
    /// </summary>
    L = 0,
    /// <summary>
    /// The level between <see cref="QRCodeEccLevel.L" /> and <see cref="QRCodeEccLevel.Q" />. This is the default of
    /// <see cref="AtomUI.Controls.Commons.AbstractQRCode.EccLevel" />.
    /// </summary>
    M = 1,
    /// <summary>
    /// The level between <see cref="QRCodeEccLevel.M" /> and <see cref="QRCodeEccLevel.H" />.
    /// </summary>
    Q = 2,
    /// <summary>
    /// The highest level. The code tolerates the most damage and has the most modules for a given text.
    /// </summary>
    H = 3
}

/// <summary>
/// The status presented over a QR code. The application sets it; the control does not change it on its own.
/// </summary>
public enum QRCodeStatus
{
    /// <summary>
    /// The code is shown without a mask. This is the default.
    /// </summary>
    Active = 0,
    /// <summary>
    /// The theme covers the code with a mask and shows the localized "Expired" text above a refresh button that raises
    /// <see cref="AtomUI.Controls.Commons.AbstractQRCode.RefreshRequested" />, or
    /// <see cref="AtomUI.Controls.Commons.AbstractQRCode.ExpiredContent" /> when it is set.
    /// </summary>
    Expired,
    /// <summary>
    /// The theme covers the code with a mask and shows a spinning indicator, or
    /// <see cref="AtomUI.Controls.Commons.AbstractQRCode.LoadingContent" /> when it is set.
    /// </summary>
    Loading,
    /// <summary>
    /// The theme covers the code with a mask and shows the localized "Scanned" text, or
    /// <see cref="AtomUI.Controls.Commons.AbstractQRCode.ScannedContent" /> when it is set.
    /// </summary>
    Scanned,
}